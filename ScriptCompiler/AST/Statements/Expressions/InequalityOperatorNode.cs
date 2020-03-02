namespace ScriptCompiler.AST.Statements.Expressions {
    public class InequalityOperatorNode : ComparatorNode {
        public InequalityOperatorNode(ExpressionNode left, ExpressionNode right) : base(left, right) { }
    }
}
