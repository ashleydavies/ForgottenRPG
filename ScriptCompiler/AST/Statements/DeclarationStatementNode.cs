using System.Collections.Generic;
using ScriptCompiler.AST.Statements.Expressions;

namespace ScriptCompiler.AST.Statements {
    public class DeclarationStatementNode : StatementNode {
        public readonly ExplicitTypeNode TypeNode;
        public readonly string Identifier;
        public readonly ExpressionNode? InitialValue;

        public DeclarationStatementNode(ExplicitTypeNode typeNode, string identifier, ExpressionNode? initialValue = null) {
            TypeNode = typeNode;
            Identifier = identifier;
            InitialValue = initialValue;
        }

        public override List<ASTNode> Children() {
            if (InitialValue == null) return new List<ASTNode> { TypeNode };
            return new List<ASTNode> { TypeNode, InitialValue };
        }
    }
}
