using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public class ProgramNode : ASTNode {
        private readonly List<FunctionNode> _functionNodes;
        private readonly List<StatementNode> _statementNodes;
        
        public ProgramNode(List<FunctionNode> functionNodes, List<StatementNode> statementNodes) {
            _functionNodes = functionNodes;
            _statementNodes = statementNodes;
        }
    }
}
