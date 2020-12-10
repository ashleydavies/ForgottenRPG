using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ForgottenRPG.VM;
using ScriptCompiler.AST;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.CodeGeneration.Assembly.Instructions;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Parsing;
using ScriptCompiler.Types;
using ScriptCompiler.Visitors;

namespace ScriptCompiler.CodeGeneration {
    /// <summary>
    /// CodeGenerator is the second iteration of the CodeGenVisitor, made to allow for a clean-up of the code
    /// gen visitor.
    /// </summary>
    public class CodeGenerator {
        private readonly FunctionTypeRepository _functionTypeRepository = new FunctionTypeRepository();
        private readonly StaticVariableRepository _staticVariableRepository = new StaticVariableRepository();
        private readonly UserTypeRepository _userTypeRepository = new UserTypeRepository();
        private readonly Dictionary<string, StringLabel> _stringPoolAliases = new Dictionary<string, StringLabel>();

        public List<Instruction> Generate(ProgramNode program) {
            List<Instruction> instructions = new List<Instruction>();
            var               programNodes = HandleImports(program);

            InitialiseUserTypeRepo(programNodes);
            InitialiseFunctionTypeRepo(programNodes);
            InitialiseStringPool(programNodes);

            // Begin code section
            instructions.Add(Section.CodeSection);

            // Main statements
            instructions.AddRange(
                new StatementBlockGenerationVisitor(
                    _functionTypeRepository,
                    _userTypeRepository,
                    _staticVariableRepository,
                    _stringPoolAliases).VisitStatementBlock(program.StatementNodes)
            );

            // Jump to the end to avoid falling to function definitions
            instructions.Add(new JmpInstruction(Label.EndLabel).WithComment("Standard termination"));

            // Function definitions
            foreach (var functionNode in programNodes.SelectMany(n => n.FunctionNodes)) {
                instructions.AddRange(GenerateFunction(functionNode));
            }

            instructions.Add(new LabelInstruction(Label.EndLabel));

            /* Now that we have insights set up on the code by our pass above, set up and prepend the data sections */
            // Begin data section
            var dataSections = new List<Instruction> {Section.StaticSection};
            // Add static variable space
            foreach (var (staticId, initialValue) in _staticVariableRepository.Values) {
                dataSections.Add(new StaticInstruction(new StaticLabel(staticId), initialValue));
            }
            // Add strings to data section
            dataSections.Add(Section.DataSection);
            dataSections.AddRange(from StringLabel stringLabel in _stringPoolAliases.Values
                                 select new StringInstruction(stringLabel));

            // Add data section
            instructions.InsertRange(0, dataSections);

            return instructions;
        }

        private List<Instruction> GenerateFunction(FunctionNode functionNode) {
            var instructions = new List<Instruction>();
            // Set up the stack frame for the return location
            var stackFrame      = new StackFrame();
            var registerManager = new RegisterManager();

            var returnType = _functionTypeRepository.ReturnType(functionNode.Name);
            if (!ReferenceEquals(returnType, SType.SVoid)) {
                stackFrame.AddIdentifier(returnType, StackFrame.ReturnIdentifier);
                stackFrame = new StackFrame(stackFrame);
            }

            // Set up the stack frame for the function parameters
            foreach (var (type, name) in functionNode.ParameterDefinitions) {
                stackFrame.AddIdentifier(SType.FromTypeString(type, _userTypeRepository), name);
            }

            // The return pointer
            stackFrame.Pushed(SType.SInteger);

            stackFrame = new StackFrame(stackFrame);
            instructions.Add(new LabelInstruction(functionNode.Label)
                                 .WithComment($"Entry point of {functionNode.Name}"));
            instructions.AddRange(new StatementBlockGenerationVisitor(
                                      _functionTypeRepository,
                                      _userTypeRepository,
                                      _staticVariableRepository,
                                      _stringPoolAliases,
                                      stackFrame
                                  ).VisitStatementBlock(functionNode.CodeBlock.Statements));

            // Pop the stack
            var (_, length) = stackFrame.Purge();
            instructions.Add(new SubInstruction(registerManager.StackPointer, length)
                                 .WithComment("Pop function locals"));
            // Pop the instruction pointer and return to it
            instructions.Add(new SubInstruction(registerManager.StackPointer, 1));
            instructions.Add(new MemReadInstruction(registerManager.InstructionPointer, registerManager.StackPointer)
                                 .WithComment($"Return from {functionNode.Name}"));

            return instructions;
        }

        private void InitialiseStringPool(List<ProgramNode> allProgramNodes) {
            List<string> stringPool  = allProgramNodes.SelectMany(p => new StringCollectorVisitor().Visit(p)).ToList();
            var          stringIndex = 0;
            foreach (var entry in stringPool) {
                _stringPoolAliases[entry] = new StringLabel(stringIndex++, entry);
            }
        }

        private static List<ProgramNode> HandleImports(ProgramNode mainProgram) {
            // Recursively load every included program
            var allPrograms        = new List<ProgramNode> {mainProgram};
            var imports            = new HashSet<ImportNode>();
            var currentlyImporting = new Stack<ProgramNode>();
            currentlyImporting.Push(mainProgram);
            while (currentlyImporting.TryPop(out var result)) {
                foreach (var import in result.ImportNodes) {
                    if (imports.Contains(import)) {
                        continue;
                    }

                    imports.Add(import);
                    var parsedProgram = Parser.FromFile(import.FileName).Parse();
                    allPrograms.Add(parsedProgram);
                    currentlyImporting.Push(parsedProgram);
                }
            }

            return allPrograms;
        }

        private void InitialiseFunctionTypeRepo(List<ProgramNode> allProgramNodes) {
            var functionNodes = allProgramNodes.SelectMany(p => p.FunctionNodes);
            foreach (var f in functionNodes) {
                _functionTypeRepository.Register(f.Name, f.TypeNode.GetSType(_userTypeRepository),
                                                 f.ParameterTypes(_userTypeRepository));
            }
        }

        private void InitialiseUserTypeRepo(List<ProgramNode> programNodes) {
            // TODO: Validation of no repeat names etc.
            Dictionary<string, StructNode> userTypeNodeMapping
                = programNodes.SelectMany(f => f.StructNodes)
                              .ToDictionary(structNode => structNode.StructName);

            List<string> userTypesToProcess = new List<string>(userTypeNodeMapping.Keys);
            while (userTypesToProcess.Count > 0) {
                string userType = userTypesToProcess[0];

                Queue<string> dependencies = new Queue<string>();
                dependencies.Enqueue(userType);
                while (dependencies.Count != 0) {
                    string next       = dependencies.Dequeue();
                    var    structNode = userTypeNodeMapping[next];

                    // If this user type has any unknown dependencies, put it to the end and queue up its dependencies.
                    var unknownDependencies = structNode.DeclarationNodes
                                                        .Where(n => n.TypeNode.IsOfUnknownType(_userTypeRepository))
                                                        .ToList();
                    if (unknownDependencies.Count > 0) {
                        unknownDependencies.ForEach(dep => dependencies.Enqueue(dep.TypeNode.TypeString));
                        dependencies.Enqueue(next);
                        continue;
                    }

                    userTypesToProcess.Remove(next);
                    _userTypeRepository[next] = UserType.FromStruct(structNode, _userTypeRepository);
                }
            }
        }
    }
}
