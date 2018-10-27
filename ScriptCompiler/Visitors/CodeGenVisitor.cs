using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using ScriptCompiler.AST;
using ScriptCompiler.Types;

namespace ScriptCompiler.Visitors {
    public class CodeGenVisitor : Visitor<string>, IRegisterAllocator {
        public Dictionary<string, string> StringAliases = new Dictionary<string, string>();
        private int _returnLabelCount = 0;
        // The instruction register is always 'occupied'
        private List<bool> _occupiedRegisters = new List<bool>() { true };

        public override string Visit(ASTNode node) {
            throw new NotImplementedException(node.GetType().Name);
        }

        public string Visit(ProgramNode node) {
            List<string> programStrings = new StringLiteralCollectorVisitor().Visit(node);
            
            StringBuilder programBuilder = new StringBuilder();

            programBuilder.AppendLine(".data");
            for (int i = 0; i < programStrings.Count; i++) {
                StringAliases[programStrings[i]] = $"str_{i}";
                programBuilder.AppendLine($"STRING str_{i} {programStrings[i]}");
            }
            programBuilder.AppendLine(".text");
            programBuilder.AppendLine(VisitStatementBlock(node.StatementNodes));

            // Exit, don't fall into executing a function
            programBuilder.AppendLine("JMP end");
            
            foreach (dynamic functionNode in node.FunctionNodes) {
                programBuilder.AppendLine(this.Visit(functionNode));
            }

            programBuilder.AppendLine("LABEL end");

            return programBuilder.ToString();
        }

        public string Visit(FunctionNode node) {
            var functionBuilder = new StringBuilder();

            functionBuilder.AppendLine($"LABEL func_{node.FunctionName}");
            functionBuilder.AppendLine(VisitStatementBlock(node.CodeBlock.Statements));
            // AKA RET
            functionBuilder.AppendLine($"POP r0");

            return functionBuilder.ToString();
        }

        public string Visit(FunctionCallNode node) {
            var functionCallBuilder = new StringBuilder();

            // Save the address of where we should return to after the function ends
            var returnLabelNo = _returnLabelCount++;
            functionCallBuilder.AppendLine($"PUSH $return_{returnLabelNo}");
            functionCallBuilder.AppendLine($"JMP func_{node.FunctionName}");
            functionCallBuilder.AppendLine($"LABEL return_{returnLabelNo}");

            return functionCallBuilder.ToString();
        }

        public string Visit(PrintStatementNode node) {
            var (commands, register) = new ExpressionGenVisitor(this).VisitDynamic(node.Expression);

            StringBuilder builder = new StringBuilder();
            
            using (register) {
                foreach (var command in commands) {
                    builder.AppendLine(command);
                }

                // There are different print instructions depending on the type of the thing we are printing
                var type = new TypeDeterminationVisitor().VisitDynamic(node.Expression);

                if (type == SType.SString) {
                    builder.AppendLine($"PRINT {register}");
                } else if (type == SType.SInteger) {
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
    }
}
