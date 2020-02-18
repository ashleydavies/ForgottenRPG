namespace ScriptCompiler.AST.Statements.Expressions {
    public class IntegerLiteralNode : NumericLiteralNode {
        public readonly int Value;

        public IntegerLiteralNode(int value) {
            this.Value = value;
        }
    }
}
