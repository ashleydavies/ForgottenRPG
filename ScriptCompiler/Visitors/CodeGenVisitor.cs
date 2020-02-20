using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Parsing;
using ScriptCompiler.Types;

namespace ScriptCompiler.Visitors {
    public class CodeGenVisitor : Visitor<string>, IRegisterAllocator {
        public const string MagicReturnIdentifier = "!RETURN_VALUE";

        public StackFrame StackFrame { get; private set; } = new StackFrame();
        public readonly Dictionary<string, string> StringLiteralAliases = new Dictionary<string, string>();
        public readonly UserTypeRepository UTRepo = new UserTypeRepository();
        public readonly FunctionTypeRepository FTRepo = new FunctionTypeRepository();

        // The instruction and stack pointer registers are always 'occupied'
        private readonly List<bool> _occupiedRegisters = new List<bool> {true, true};


        /// <summary>
        /// Main entry method - sets up private parameters and generates code for a full program
        /// </summary>
        public string Visit(ProgramNode node) {
            // Set up the initial stack frame
            StackFrame = new StackFrame();
            List<ProgramNode> importedFiles =
                node.ImportNodes.Select(f => Parser.FromFile(f.FileName).Parse()).ToList();
            List<string> programStrings = new StringCollectorVisitor().Visit(node);
            importedFiles.ForEach(p => programStrings.AddRange(new StringCollectorVisitor().Visit(p)));

            InitialiseUTRepo(node, importedFiles);

            List<FunctionNode> allFunctions = new List<FunctionNode>();
            allFunctions.AddRange(node.FunctionNodes);
            allFunctions.AddRange(importedFiles.SelectMany(n => n.FunctionNodes));

            allFunctions.ForEach(f =>
                                     FTRepo.Register(f.Name, f.TypeNode.GetSType(UTRepo),
                                                     f.ParameterDefinitions
                                                      .Select(p => SType.FromTypeString(p.type, UTRepo)).ToList()));


            StringBuilder programBuilder = new StringBuilder();

            programBuilder.AppendLine(".data");
            for (int i = 0; i < programStrings.Count; i++) {
                StringLiteralAliases[programStrings[i]] = $"str_{i}";
                programBuilder.AppendLine($"STRING str_{i} {programStrings[i]}");
            }

            programBuilder.AppendLine(Comment(".text", "Begin code"));
            programBuilder.AppendLine(VisitStatementBlock(node.StatementNodes));
            // Exit, or we'll fall into a random function or terminate
            programBuilder.AppendLine(Comment("JMP end", "Standard termination"));

            allFunctions.ForEach(f => programBuilder.AppendLine(Visit(f)));

            programBuilder.AppendLine("LABEL end");

            return programBuilder.ToString();
        }

        private void InitialiseUTRepo(ProgramNode node, List<ProgramNode> importedFiles) {
            Dictionary<string, StructNode> userTypeNodeMapping = new Dictionary<string, StructNode>();
            // TODO: Validation of no repeat names etc.
            foreach (StructNode structNode in node.StructNodes) {
                userTypeNodeMapping.Add(structNode.StructName, structNode);
            }

            foreach (StructNode structNode in importedFiles.SelectMany(f => f.StructNodes)) {
                userTypeNodeMapping.Add(structNode.StructName, structNode);
            }

            List<string> userTypesToProcess = new List<string>(userTypeNodeMapping.Keys);
            while (userTypesToProcess.Count > 0) {
                string userType = userTypesToProcess[0];

                Queue<string> dependencies = new Queue<string>();
                dependencies.Enqueue(userType);
                while (dependencies.Count != 0) {
                    string next = dependencies.Dequeue();
                    bool   deps = false;

                    var structNode = userTypeNodeMapping[next];
                    foreach (DeclarationStatementNode declNode in structNode.DeclarationNodes) {
                        if (ReferenceEquals(declNode.TypeNode.GetSType(UTRepo), SType.SNoType)) {
                            dependencies.Enqueue(declNode.TypeNode.TypeString);
                            deps = true;
                        }
                    }

                    if (deps) {
                        dependencies.Enqueue(next);
                        continue;
                    }

                    userTypesToProcess.Remove(next);
                    UTRepo[next] = UserType.FromStruct(structNode, UTRepo);
                }
            }
        }

        public string Visit(FunctionNode node) {
            // Set up the stack frame for the return location
            StackFrame = new StackFrame();

            var returnType = FTRepo.ReturnType(node.Name);
            if (!ReferenceEquals(returnType, SType.SVoid)) {
                StackFrame.AddIdentifier(returnType, MagicReturnIdentifier);
                StackFrame = new StackFrame(StackFrame);
            }

            // Set up the stack frame for the function parameters
            node.ParameterDefinitions.ForEach(p =>
                                                  StackFrame.AddIdentifier(
                                                      SType.FromTypeString(p.type, UTRepo), p.name));

            // TODO: Uncomment when the instruction pointer is stored on the memory stack and not the stack machine
            // 'Push' the instruction pointer, which is the same length as an integer
            //StackFrame.Pushed(SType.SInteger);

            var functionBuilder = new StringBuilder();

            StackFrame = new StackFrame(StackFrame);

            functionBuilder.AppendLine(Comment($"LABEL func_{node.Name}",
                                               $"Entry point of {node.Name}"));
            functionBuilder.AppendLine(VisitStatementBlock(node.CodeBlock.Statements));

            var (stackFrame, length) = StackFrame.Purge();
            StackFrame               = stackFrame!;
            functionBuilder.AppendLine($"SUB r1 {length}");

            node.ParameterDefinitions.ForEach(p => StackFrame.Popped(SType.FromTypeString(p.type, UTRepo)));

            // AKA RET
            functionBuilder.AppendLine(Comment($"POP r0", $"Return from {node.Name}"));

            return functionBuilder.ToString();
        }

        public string Visit(ReturnStatementNode node) {
            return Visit(new AssignmentNode(new VariableAccessNode(MagicReturnIdentifier),
                                            node.Expression));
        }

        // General expressions e.g. naked function calls
        public string Visit(ExpressionNode node) {
            var (commands, resultLoc) = new ExpressionGenVisitor(this).GenerateCode(node);
            using (resultLoc) { }

            return string.Join(Environment.NewLine, commands);
        }

        public string Visit(DeclarationStatementNode node) {
            var declarationBuilder = new StringBuilder();

            if (StackFrame.ExistsLocalScope(node.Identifier)) {
                // TODO: Add line and col numbers (as well as other debug info) to all nodes, and report correctly here
                throw new CompileException($"Attempt to redefine identifier {node.Identifier}", 0, 0);
            }

            SType type = node.TypeNode.GetSType(UTRepo);
            if (ReferenceEquals(type, SType.SNoType)) {
                throw new CompileException($"Unable to discern type from {node.TypeNode}", 0, 0);
            }

            StackFrame.AddIdentifier(type, node.Identifier);
            // Adjust stack pointer
            declarationBuilder.AppendLine(Comment($"ADD r1 {type.Length}",
                                                  $"Declaration of {node.Identifier}"));

            // Set up with default value, if any
            if (node.InitialValue != null) {
                var (commands, resultReg) = new ExpressionGenVisitor(this).GenerateCode(node.InitialValue);
                commands.ForEach(s => declarationBuilder.AppendLine(s));
                // TODO: Remove duplication with the ExpressionGenVisitor VariableAccessNode handler
                using (resultReg) {
                    // Put the memory location of our variable into a free register
                    using (var locationReg = GetRegister()) {
                        // reg = Stack
                        declarationBuilder.AppendLine($"MOV {locationReg} r1");

                        // reg = Stack - offset to variable
                        var offset = StackFrame.Lookup(node.Identifier).position;
                        declarationBuilder.AppendLine($"ADD {locationReg} {offset}");

                        // Write to memory
                        declarationBuilder.AppendLine($"MEMWRITE {locationReg} {resultReg}");
                    }
                }
            } else {
                declarationBuilder.AppendLine($"SUB r1 {type.Length}");
                for (int i = 0; i < type.Length; i++) {
                    declarationBuilder.AppendLine($"MEMWRITE r1 0");
                    declarationBuilder.AppendLine($"ADD r1 1");
                }
            }

            return declarationBuilder.ToString();
        }

        public string Visit(PrintStatementNode node) {
            var (commands, register) = new ExpressionGenVisitor(this).GenerateCode(node.Expression);

            StringBuilder builder = new StringBuilder();

            using (register) {
                foreach (var command in commands) {
                    builder.AppendLine(command);
                }

                // There are different print instructions depending on the type of the thing we are printing
                var type = new TypeDeterminationVisitor(this).VisitDynamic(node.Expression);

                if (ReferenceEquals(type, SType.SString)) {
                    builder.AppendLine($"PRINT {register}");
                } else if (ReferenceEquals(type, SType.SInteger)) {
                    builder.AppendLine($"PRINTINT {register}");
                }
            }

            return builder.ToString();
        }

        public string VisitStatementBlock(List<StatementNode> statements) {
            var blockBuilder = new StringBuilder();

            foreach (dynamic statementNode in statements) {
                blockBuilder.AppendLine(Visit(statementNode));
            }

            return blockBuilder.ToString();
        }

        public Register GetRegister() {
            for (int i = 0; i < _occupiedRegisters.Count; i++) {
                if (_occupiedRegisters[i] == false) {
                    _occupiedRegisters[i] = true;
                    return new Register(i, () => _occupiedRegisters[i] = false);
                }
            }

            int idx = _occupiedRegisters.Count;
            _occupiedRegisters.Add(true);
            return new Register(idx, () => _occupiedRegisters[idx] = false);
        }

        public override string Visit(ASTNode node) {
            throw new NotImplementedException(node.GetType().Name);
        }

        public static string Comment(string test, string comment) {
            return test.PadRight(40) + "# " + comment;
        }
    }
}
