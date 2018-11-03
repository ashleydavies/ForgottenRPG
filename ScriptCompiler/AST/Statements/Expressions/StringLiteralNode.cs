namespace ScriptCompiler.AST.Statements.Expressions {
    public class StringLiteralNode : LiteralNode {
        public readonly string String;

        public StringLiteralNode(string s) {
            String = s;
        }
    }
}
