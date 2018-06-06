using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public abstract class ASTNode {
        public abstract List<ASTNode> Children();
    }
}
