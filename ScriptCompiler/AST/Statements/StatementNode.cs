using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements {
    public abstract class StatementNode : ASTNode {
        public override List<ASTNode> Children() {
            return new List<ASTNode>();
        }
    }
}
