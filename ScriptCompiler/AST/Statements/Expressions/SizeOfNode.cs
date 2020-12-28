using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements.Expressions {
    public class SizeOfNode : ExpressionNode {
        public readonly ExpressionNode Arg;

        public SizeOfNode(ExpressionNode arg) {
            Arg = arg;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode> {Arg};
        }
    }
}
