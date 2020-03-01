using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements.Expressions {
    public class BinaryOperatorNode : ExpressionNode {
        public readonly ExpressionNode Left;
        public readonly ExpressionNode Right;

        public BinaryOperatorNode(ExpressionNode left, ExpressionNode right) {
            Left  = left;
            Right = right;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode> {Left, Right};
        }
    }
}
