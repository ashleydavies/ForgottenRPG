using System.Collections.Generic;
using ScriptCompiler.AST.Statements.Expressions;

namespace ScriptCompiler.AST.Statements {
    public class ReturnStatementNode : StatementNode {
        public readonly ExpressionNode? Expression;

        public ReturnStatementNode(ExpressionNode? expression) {
            Expression = expression;
        }

        public override List<ASTNode> Children() {
            if (Expression == null) return new();
            return new() { Expression };
        }
    }
}
