namespace ScriptCompiler.AST {
    public class FunctionCallNode : ExpressionNode {
        public readonly string FunctionName;

        public FunctionCallNode(string functionName) {
            FunctionName = functionName;
        }
    }
}
