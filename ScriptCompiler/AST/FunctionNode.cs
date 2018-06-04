namespace ScriptCompiler.AST {
    public class FunctionNode : ASTNode {
        private ExplicitTypeNode _typeNode;
        private readonly CodeBlockNode _codeBlock;

        public FunctionNode(ExplicitTypeNode typeNode, CodeBlockNode codeBlock) {
            _typeNode = typeNode;
            _codeBlock = codeBlock;
        }
    }
}
