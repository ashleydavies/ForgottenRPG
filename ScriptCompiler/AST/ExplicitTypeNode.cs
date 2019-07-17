using System.Collections.Generic;
using ScriptCompiler.Types;

namespace ScriptCompiler.AST {
    public class ExplicitTypeNode : ASTNode {
        public readonly string TypeString;
        
        public ExplicitTypeNode(string typeString) {
            TypeString = typeString;
        }

        public SType GetSType(UserTypeRepository utr) {
            return SType.FromTypeString(TypeString, utr);
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode>();
        }
    }
}
