using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements.Expressions {
    public class ReferenceNode : ExpressionNode {
        public readonly ExpressionNode Expression;
        
        public ReferenceNode(ExpressionNode expression) {
            Expression = expression;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode> { Expression };
        }
    }
}
