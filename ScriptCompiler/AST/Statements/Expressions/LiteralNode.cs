using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements.Expressions {
    public class LiteralNode : ExpressionNode {
        public override List<ASTNode> Children() {
            return new List<ASTNode>();
        }
    }
}
