namespace ScriptCompiler.AST.Statements.Expressions {
    public class FunctionCallNode : ExpressionNode {
        public readonly string FunctionName;

        public FunctionCallNode(string functionName) {
            FunctionName = functionName;
        }
    }
}
