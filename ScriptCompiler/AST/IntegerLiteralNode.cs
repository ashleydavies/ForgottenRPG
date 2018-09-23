namespace ScriptCompiler.AST {
    public class IntegerLiteralNode : NumericLiteralNode {
        public readonly int value;

        public IntegerLiteralNode(int value) {
            this.value = value;
        }
    }
}
