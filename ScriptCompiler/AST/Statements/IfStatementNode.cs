using System.Collections.Generic;
using ScriptCompiler.AST.Statements.Expressions;

namespace ScriptCompiler.AST.Statements {
    public class IfStatementNode : StatementNode {
        public readonly ExpressionNode Expression;
        public readonly CodeBlockNode IfBlock;
        public readonly CodeBlockNode? ElseBlock;

        public IfStatementNode(ExpressionNode expression, CodeBlockNode ifBlock, CodeBlockNode? elseBlock) {
            Expression = expression;
            IfBlock    = ifBlock;
            ElseBlock  = elseBlock;
        }

        public override List<ASTNode> Children() {
            if (ElseBlock != null) {
                return new List<ASTNode> {IfBlock, ElseBlock, Expression};
            }
            return new List<ASTNode> {IfBlock, Expression};
        }
    }
}
