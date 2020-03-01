namespace ScriptCompiler.AST.Statements.Expressions {
    public class GreaterThanEqualOperatorNode : ComparatorNode {
        public GreaterThanEqualOperatorNode(ExpressionNode left, ExpressionNode right) : base(left, right) { }
    }
}
