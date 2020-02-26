using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements.Expressions {
    public class AssignmentNode : ExpressionNode {
        public readonly ExpressionNode Destination;
        public readonly ExpressionNode Value;
        
        public AssignmentNode(ExpressionNode destination, ExpressionNode value) {
            Destination = destination;
            Value = value;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode>() { Destination, Value };
        }
    }
}
