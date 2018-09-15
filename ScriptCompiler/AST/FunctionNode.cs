using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public class FunctionNode : ASTNode {
        public readonly string FunctionName;
        public readonly ExplicitTypeNode TypeNode;
        public readonly CodeBlockNode CodeBlock;

        public FunctionNode(string functionName, ExplicitTypeNode typeNode, CodeBlockNode codeBlock) {
            FunctionName = functionName;
            TypeNode = typeNode;
            CodeBlock = codeBlock;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode> { TypeNode, CodeBlock };
        }
    }
}
