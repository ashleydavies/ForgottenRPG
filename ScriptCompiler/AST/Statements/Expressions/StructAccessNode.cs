using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements.Expressions {
    public class StructAccessNode : ExpressionNode {
        public readonly ExpressionNode Left;
        public readonly string Field;

        public StructAccessNode(ExpressionNode left, string field) {
            Left  = left;
            Field = field;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode> {Left};
        }
    }
}
