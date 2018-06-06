using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public class FunctionNode : ASTNode {
        public readonly ExplicitTypeNode TypeNode;
        public readonly CodeBlockNode CodeBlock;

        public FunctionNode(ExplicitTypeNode typeNode, CodeBlockNode codeBlock) {
            TypeNode = typeNode;
            CodeBlock = codeBlock;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode> { TypeNode, CodeBlock };
        }
    }
}
