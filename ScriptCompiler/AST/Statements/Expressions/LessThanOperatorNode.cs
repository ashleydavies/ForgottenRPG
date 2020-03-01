namespace ScriptCompiler.AST.Statements.Expressions {
    public class LessThanOperatorNode : ComparatorNode {
        public LessThanOperatorNode(ExpressionNode left, ExpressionNode right) : base(left, right) { }
    }
}
