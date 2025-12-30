using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCompiler.AST;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.Types;
using ScriptCompiler.Visitors;

namespace ScriptCompiler.CodeGeneration {
    public sealed class StackCodeGenerator {
        private readonly FunctionTypeRepository _functionTypeRepository = new FunctionTypeRepository();
        private readonly StaticVariableRepository _staticVariableRepository = new StaticVariableRepository();
        private readonly UserTypeRepository _userTypeRepository = new UserTypeRepository();
        private readonly Dictionary<string, StringLabel> _stringPoolAliases = new Dictionary<string, StringLabel>();
        private readonly ForgottenRPG.NativeFunctions.NativeFunctionRegistry _nativeRegistry;

        public StackCodeGenerator(ForgottenRPG.NativeFunctions.NativeFunctionRegistry nativeRegistry = null) {
            _nativeRegistry = nativeRegistry;
        }

        public IReadOnlyList<string> Generate(ProgramNode program) {
            var programNodes = HandleImports(program);

            InitialiseUserTypeRepo(programNodes);
            InitialiseFunctionTypeRepo(programNodes);
            InitialiseStringPool(programNodes);

            var emitter = new StackAssemblyEmitter();

            foreach (var stringLabel in _stringPoolAliases.Values) {
                emitter.EmitData($"string {stringLabel.ToString().TrimStart('!')} {stringLabel.Contents}");
            }

            emitter.EmitText("literal", "1000000");
            emitter.EmitText("pop", "r1");

            var mainEndLabel = "end";
            var mainStackFrame = new CompileUtil.StackFrame();
            var mainStatementEmitter = new StackStatementEmitter(emitter,
                                                                 _functionTypeRepository,
                                                                 _userTypeRepository,
                                                                 _staticVariableRepository,
                                                                 _stringPoolAliases,
                                                                 mainStackFrame,
                                                                 mainEndLabel,
                                                                 _nativeRegistry);
            mainStatementEmitter.EmitStatements(program.StatementNodes);
            emitter.EmitText("jmp", $"${mainEndLabel}");

            foreach (var functionNode in programNodes.SelectMany(n => n.FunctionNodes)) {
                if (functionNode.IsExtern) continue;

                var functionRef = functionNode.Reference(_userTypeRepository);
                var symbolHash = _functionTypeRepository.SymbolHash(functionRef);
                string functionLabel = functionNode.Label.ToString().TrimStart('$');
                emitter.EmitText("export", $"{functionLabel} {symbolHash}");

                var returnType = _functionTypeRepository.ReturnType(functionRef);
                var stackFrame = new CompileUtil.StackFrame();
                if (!ReferenceEquals(returnType, SType.SVoid)) {
                    stackFrame.AddIdentifier(returnType, CompileUtil.StackFrame.ReturnIdentifier);
                    stackFrame = new CompileUtil.StackFrame(stackFrame);
                }

                foreach (var (type, depth, name) in functionNode.ParameterDefinitions) {
                    stackFrame.AddIdentifier(SType.FromTypeString(type, _userTypeRepository, depth), name);
                }

                stackFrame.Pushed(SType.SInteger);
                stackFrame = new CompileUtil.StackFrame(stackFrame);

                var functionEndLabel = $"end_{functionLabel}";
                emitter.EmitLabel(functionLabel);

                var statementEmitter = new StackStatementEmitter(emitter,
                                                                 _functionTypeRepository,
                                                                 _userTypeRepository,
                                                                 _staticVariableRepository,
                                                                 _stringPoolAliases,
                                                                 stackFrame,
                                                                 functionEndLabel,
                                                                 _nativeRegistry);
                statementEmitter.EmitStatements(functionNode.CodeBlock.Statements);

                emitter.EmitLabel(functionEndLabel);

                var (_, len) = statementEmitter.CurrentStackFrame.Purge();
                EmitAdjustStackPointer(emitter, -len);

                EmitAdjustStackPointer(emitter, -1);
                emitter.EmitText("push", "r1");
                emitter.EmitText("memread_stack");
                emitter.EmitText("jmp");
            }

            emitter.EmitLabel(mainEndLabel);

            EmitStaticData(emitter);

            return emitter.Lines;
        }

        private void EmitStaticData(StackAssemblyEmitter emitter) {
            foreach (var (staticId, initialValue) in _staticVariableRepository.Values) {
                var label = new StaticLabel(staticId).ToString().TrimStart('!');
                emitter.EmitStatic($"static {label} {string.Join(",", initialValue)}");
            }
        }

        private static void EmitAdjustStackPointer(StackAssemblyEmitter emitter, int delta) {
            if (delta == 0) return;
            emitter.EmitText("push", "r1");
            emitter.EmitText("literal", Math.Abs(delta).ToString());
            emitter.EmitText(delta > 0 ? "add" : "sub");
            emitter.EmitText("pop", "r1");
        }

        private static List<ProgramNode> HandleImports(ProgramNode mainProgram) {
            var allPrograms = new List<ProgramNode> { mainProgram };
            var imports = new HashSet<ImportNode>();
            var currentlyImporting = new Stack<ProgramNode>();
            currentlyImporting.Push(mainProgram);
            while (currentlyImporting.TryPop(out var result)) {
                foreach (var import in result.ImportNodes) {
                    if (imports.Contains(import)) {
                        continue;
                    }

                    imports.Add(import);
                    var parsedProgram = Parsing.Parser.FromFile(import.FileName).Parse();
                    allPrograms.Add(parsedProgram);
                    currentlyImporting.Push(parsedProgram);
                }
            }

            return allPrograms;
        }

        private void InitialiseStringPool(List<ProgramNode> allProgramNodes) {
            List<string> stringPool = allProgramNodes.SelectMany(p => new StringCollectorVisitor().Visit(p)).ToList();
            var stringIndex = 0;
            foreach (var entry in stringPool) {
                _stringPoolAliases[entry] = new StringLabel(stringIndex++, entry);
            }
        }

        private void InitialiseFunctionTypeRepo(List<ProgramNode> allProgramNodes) {
            var functionNodes = allProgramNodes.SelectMany(p => p.FunctionNodes);
            foreach (var f in functionNodes) {
                _functionTypeRepository.Register(f.Reference(_userTypeRepository),
                                                 f.TypeNode.GetSType(_userTypeRepository),
                                                 f.ParameterTypes(_userTypeRepository),
                                                 f.IsExtern);
            }
        }

        private void InitialiseUserTypeRepo(List<ProgramNode> programNodes) {
            if (_nativeRegistry != null) {
                foreach (var (scriptName, _) in _nativeRegistry.GetRegisteredTypes()) {
                    if (!_userTypeRepository.ExistsType(scriptName)) {
                        _userTypeRepository[scriptName] = new UserType(scriptName, new List<(string, SType)>());
                    }
                }
            }

            Dictionary<string, StructNode> userTypeNodeMapping
                = programNodes.SelectMany(f => f.StructNodes)
                              .ToDictionary(structNode => structNode.StructName);

            List<string> userTypesToProcess = new List<string>(userTypeNodeMapping.Keys);
            while (userTypesToProcess.Count > 0) {
                string userType = userTypesToProcess[0];

                Queue<string> dependencies = new Queue<string>();
                dependencies.Enqueue(userType);
                while (dependencies.Count != 0) {
                    string next = dependencies.Dequeue();
                    var structNode = userTypeNodeMapping[next];

                    var unknownDependencies = structNode.DeclarationNodes
                                                        .Where(n => n.TypeNode.IsOfUnknownType(_userTypeRepository))
                                                        .Select(n => n.TypeNode.TypeString)
                                                        .Where(t => t != userType)
                                                        .ToList();

                    if (unknownDependencies.Count > 0) {
                        userTypesToProcess.Remove(userType);
                        userTypesToProcess.Add(userType);
                        foreach (var dep in unknownDependencies) {
                            dependencies.Enqueue(dep);
                        }
                        break;
                    }

                    _userTypeRepository[userType] = UserType.FromStruct(structNode, _userTypeRepository);
                    userTypesToProcess.Remove(userType);
                }
            }
        }
    }
}
