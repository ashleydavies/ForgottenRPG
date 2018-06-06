using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public class PrintStatementNode : StatementNode {
        public readonly ExpressionNode Expression;

        public PrintStatementNode(ExpressionNode expression) {
            Expression = expression;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode> { Expression };
        }
    }
}