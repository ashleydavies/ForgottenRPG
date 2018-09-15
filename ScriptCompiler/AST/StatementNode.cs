using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ScriptCompiler.AST {
    public abstract class StatementNode : ASTNode {
        public override List<ASTNode> Children() {
            return new List<ASTNode>();
        }
    }
}
