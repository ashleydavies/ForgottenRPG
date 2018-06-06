using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public class ProgramNode : ASTNode {
        public readonly List<FunctionNode> FunctionNodes;
        public readonly List<StatementNode> StatementNodes;
        
        public ProgramNode(List<FunctionNode> functionNodes, List<StatementNode> statementNodes) {
            FunctionNodes = functionNodes;
            StatementNodes = statementNodes;
        }

        public override List<ASTNode> Children() {
            var children = new List<ASTNode>();
            children.AddRange(FunctionNodes);
            children.AddRange(StatementNodes);
            return children;
        }
    }
}
