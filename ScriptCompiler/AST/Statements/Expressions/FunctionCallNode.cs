using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements.Expressions {
    public class FunctionCallNode : ExpressionNode {
        public readonly string FunctionName;
        public readonly List<ExpressionNode> Args;

        public FunctionCallNode(string functionName, List<ExpressionNode> args) {
            FunctionName = functionName;
            Args = args;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode>(Args);
        }
    }
}
