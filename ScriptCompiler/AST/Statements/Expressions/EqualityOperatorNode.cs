namespace ScriptCompiler.AST.Statements.Expressions {
    public class EqualityOperatorNode : ComparatorNode {
        public EqualityOperatorNode(ExpressionNode left, ExpressionNode right) : base(left, right) { }
    }
}
