using ScriptCompiler.AST.Statements.Expressions;

namespace ScriptCompiler.AST.Statements {
    public class DeclarationStatementNode : StatementNode {
        public readonly string TypeString;
        public readonly string Identifier;
        public readonly ExpressionNode InitialValue;

        public DeclarationStatementNode(string typeString, string identifier) : this(typeString, identifier, null) { }

        public DeclarationStatementNode(string typeString, string identifier, ExpressionNode initialValue) {
            TypeString = typeString;
            Identifier = identifier;
            InitialValue = initialValue;
        }
    }
}
