using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public class LiteralNode : ExpressionNode {
        public override List<ASTNode> Children() {
            return new List<ASTNode>();
        }
    }
}
