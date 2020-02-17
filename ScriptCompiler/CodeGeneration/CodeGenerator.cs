using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ForgottenRPG.VM;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.CodeGeneration.Assembly.Instructions;
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
        private readonly UserTypeRepository _userTypeRepository = new UserTypeRepository();
        private readonly Dictionary<string, StringLabel> StringPoolAliases = new Dictionary<string, StringLabel>();

        public List<Instruction> Generate(ProgramNode program) {
            List<Instruction> instructions = new List<Instruction>();
            var programNodes = HandleImports(program);

            InitialiseUserTypeRepo(programNodes);
            InitialiseFunctionTypeRepo(programNodes);
            InitialiseStringPool(programNodes);

            // Begin data section
            instructions.Add(Section.DataSection);
            
            // Add strings to data section
            foreach (var (_, stringLabel) in StringPoolAliases) {
                instructions.Add(new StringInstruction(stringLabel));
            }
            
            // Begin code section
            instructions.Add(Section.CodeSection);
            
            // Main statement
            // TODO
            
            // Jump to the end to avoid falling to function definitions
            instructions.Add(new JmpInstruction(Label.EndLabel).WithComment("Standard termination"));
            
            // Function definitions
            // TODO
            
            instructions.Add(new LabelInstruction(Label.EndLabel));

            return new List<Instruction>();
        }

        private void InitialiseStringPool(List<ProgramNode> allProgramNodes) {
            List<string> stringPool  = allProgramNodes.SelectMany(p => new StringCollectorVisitor().Visit(p)).ToList();
            var          stringIndex = 0;
            foreach (var entry in stringPool) {
                StringPoolAliases[entry] = new StringLabel(stringIndex++, entry);
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
                _functionTypeRepository.Register(f.FunctionName, f.TypeNode.GetSType(_userTypeRepository),
                                                 f.ParameterTypes(_userTypeRepository));
            }
        }

        private void InitialiseUserTypeRepo(List<ProgramNode> programNodes) {
            Dictionary<string, StructNode> userTypeNodeMapping = new Dictionary<string, StructNode>();
            // TODO: Validation of no repeat names etc.
            foreach (StructNode structNode in programNodes.SelectMany(f => f.StructNodes)) {
                userTypeNodeMapping.Add(structNode.StructName, structNode);
            }

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
