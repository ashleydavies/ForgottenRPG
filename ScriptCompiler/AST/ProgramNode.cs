using System.Collections.Generic;
using ScriptCompiler.AST.Statements;

namespace ScriptCompiler.AST {
    public class ProgramNode : ASTNode {
        public readonly List<ImportNode> ImportNodes;
        public readonly List<FunctionNode> FunctionNodes;
        public readonly List<StatementNode> StatementNodes;
        
        public ProgramNode(List<ImportNode> importNodes, List<FunctionNode> functionNodes,
            List<StatementNode> statementNodes) {
            ImportNodes = importNodes;
            FunctionNodes = functionNodes;
            StatementNodes = statementNodes;
        }

        public override List<ASTNode> Children() {
            var children = new List<ASTNode>();
            children.AddRange(ImportNodes);
            children.AddRange(FunctionNodes);
            children.AddRange(StatementNodes);
            return children;
        }
    }
}
