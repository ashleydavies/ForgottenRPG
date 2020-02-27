namespace ScriptCompiler.AST.Statements.Expressions {
    public class IntegerLiteralNode : NumericLiteralNode {
        public readonly uint Value;

        public IntegerLiteralNode(uint value) {
            Value = value;
        }
    }
}
