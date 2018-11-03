namespace ScriptCompiler.AST.Statements.Expressions {
    public class VariableAccessNode : ExpressionNode {
        public readonly string Identifier;
        
        public VariableAccessNode(string identifier) {
            Identifier = identifier;
        }
    }
}
