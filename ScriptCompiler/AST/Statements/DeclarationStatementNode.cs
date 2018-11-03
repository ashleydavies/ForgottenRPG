using System.Collections.Generic;
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

        public override List<ASTNode> Children() {
            if (InitialValue == null) return new List<ASTNode>();
            return new List<ASTNode> { InitialValue };
        }
    }
}
