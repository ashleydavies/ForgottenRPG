using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public class CodeBlockNode {
        private List<StatementNode> _statements;

        public CodeBlockNode(List<StatementNode> statements) {
            _statements = statements;
        }
    }
}