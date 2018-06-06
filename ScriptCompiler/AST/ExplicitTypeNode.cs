using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public class ExplicitTypeNode : ASTNode {
        private string _type;
        
        public ExplicitTypeNode(string type) {
            _type = type;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode>();
        }
    }
}
