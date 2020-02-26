using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements.Expressions {
    public class DereferenceNode : ExpressionNode {
        public readonly ExpressionNode Expression;
        
        public DereferenceNode(ExpressionNode expression) {
            Expression = expression;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode> { Expression };
        }
    }
}
