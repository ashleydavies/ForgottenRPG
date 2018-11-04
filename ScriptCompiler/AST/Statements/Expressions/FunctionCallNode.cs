namespace ScriptCompiler.AST.Statements.Expressions {
    public class FunctionCallNode : ExpressionNode {
        public readonly string FunctionName;
        public readonly List<ExpressionNode> Params;

        public FunctionCallNode(string functionName, List<ExpressionNode> @params) {
            FunctionName = functionName;
            Params = @params;
        }
    }
}
