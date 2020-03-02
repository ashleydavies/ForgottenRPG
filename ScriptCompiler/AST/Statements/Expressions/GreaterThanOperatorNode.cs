namespace ScriptCompiler.AST.Statements.Expressions {
    public class GreaterThanOperatorNode : ComparatorNode {
        public GreaterThanOperatorNode(ExpressionNode left, ExpressionNode right) : base(left, right) { }
    }
}
