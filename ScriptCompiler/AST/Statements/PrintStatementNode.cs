using System.Collections.Generic;
using ScriptCompiler.AST.Statements;
using ScriptCompiler.AST.Statements.Expressions;

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
