using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements.Expressions {
    public class AddressOfNode : ExpressionNode {
        public readonly ExpressionNode Expression;
        
        public AddressOfNode(ExpressionNode expression) {
            Expression = expression;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode> { Expression };
        }
    }
}
