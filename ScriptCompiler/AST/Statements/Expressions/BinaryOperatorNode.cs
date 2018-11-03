namespace ScriptCompiler.AST.Statements.Expressions {
    public class BinaryOperatorNode : ExpressionNode {
        public readonly ExpressionNode Left;
        public readonly ExpressionNode Right;

        public BinaryOperatorNode(ExpressionNode left, ExpressionNode right) {
            Left = left;
            Right = right;
        }
    }
}
