namespace ScriptCompiler.AST {
    public class FunctionNode : ASTNode {
        private ExplicitTypeNode _typeNode;
        
        public FunctionNode(ExplicitTypeNode typeNode) {
            _typeNode = typeNode;
        }
    }
}
