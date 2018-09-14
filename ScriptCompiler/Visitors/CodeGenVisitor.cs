using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using ScriptCompiler.AST;

namespace ScriptCompiler.Visitors {
    public class CodeGenVisitor : Visitor<string> {
        public Dictionary<string, string> StringAliases = new Dictionary<string, string>();
        
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

            functionBuilder.AppendLine(VisitStatementBlock(node.CodeBlock.Statements));

            return functionBuilder.ToString();
        }

        public string Visit(PrintStatementNode node) {
            var (commands, result) = new ExpressionGenVisitor(this).VisitDynamic(node.Expression);
            
            StringBuilder builder = new StringBuilder();
            foreach (var command in commands) {
                builder.AppendLine(command);
            }

            builder.AppendLine($"print {result}");
            return builder.ToString();
        }

        public string VisitStatementBlock(List<StatementNode> statements) {
            var blockBuilder = new StringBuilder();
            
            foreach (dynamic statementNode in statements) {
                blockBuilder.AppendLine(Visit(statementNode));
            }

            return blockBuilder.ToString();
        }
    }
}
