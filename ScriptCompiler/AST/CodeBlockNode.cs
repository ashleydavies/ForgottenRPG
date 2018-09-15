using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public class CodeBlockNode : ASTNode {
        public readonly List<StatementNode> Statements;

        public CodeBlockNode(List<StatementNode> statements) {
            Statements = statements;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode>(Statements);
        }
    }
}