using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public class StringLiteralNode : LiteralNode {
        public readonly string String;

        public StringLiteralNode(string s) {
            String = s;
        }
    }
}
