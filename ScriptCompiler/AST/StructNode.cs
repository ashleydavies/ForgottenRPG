using System;
using System.Collections.Generic;
using ScriptCompiler.AST.Statements;

namespace ScriptCompiler.AST {
    public class StructNode : ASTNode {
        public readonly string StructName;
        public readonly List<DeclarationStatementNode> DeclarationNodes;
        
        public StructNode(string structName, List<DeclarationStatementNode> declarationNodes) {
            StructName = structName;
            DeclarationNodes = declarationNodes;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode>(DeclarationNodes);
        }
    }
}