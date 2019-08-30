using System.Collections.Generic;
using ScriptCompiler.Types;

namespace ScriptCompiler.AST {
    public class ExplicitTypeNode : ASTNode {
        public readonly string TypeString;
        public readonly int PointerDepth;
        
        public ExplicitTypeNode(string typeString, int pointerDepth = 0) {
            TypeString = typeString;
            PointerDepth = pointerDepth;
        }

        public SType GetSType(UserTypeRepository utr) {
            return SType.FromTypeString(TypeString, utr, PointerDepth);
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode>();
        }
    }
}
