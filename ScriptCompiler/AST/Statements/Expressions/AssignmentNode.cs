using System.Collections.Generic;
using ScriptCompiler.AST.Statements.Expressions;

namespace ScriptCompiler.AST.Statements {
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
