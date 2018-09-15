using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public class StringLiteralNode : ExpressionNode {
        public readonly string String;

        public StringLiteralNode(string s) {
            String = s;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode>();
        }
    }
}