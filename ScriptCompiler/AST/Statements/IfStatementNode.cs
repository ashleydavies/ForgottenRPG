using System.Collections.Generic;
using ScriptCompiler.AST.Statements.Expressions;

namespace ScriptCompiler.AST.Statements {
    public class IfStatementNode : StatementNode {
        public readonly ExpressionNode Expression;
        public readonly CodeBlockNode Block;

        public IfStatementNode(ExpressionNode expression, CodeBlockNode block) {
            Expression = expression;
            Block = block;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode> {Block, Expression};
        }
    }
}
