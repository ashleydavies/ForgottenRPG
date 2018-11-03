namespace ScriptCompiler.AST.Statements.Expressions {
    public class IntegerLiteralNode : NumericLiteralNode {
        public readonly int value;

        public IntegerLiteralNode(int value) {
            this.value = value;
        }
    }
}
