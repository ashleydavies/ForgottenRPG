namespace ScriptCompiler.AST.Statements.Expressions {
    public class LessThanEqualOperatorNode : ComparatorNode {
        public LessThanEqualOperatorNode(ExpressionNode left, ExpressionNode right) : base(left, right) { }
    }
}
