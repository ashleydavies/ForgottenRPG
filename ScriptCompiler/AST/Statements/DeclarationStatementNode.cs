using System.Collections.Generic;
using ScriptCompiler.AST.Statements.Expressions;

namespace ScriptCompiler.AST.Statements {
    public class DeclarationStatementNode : StatementNode {
        public readonly bool Static;
        public readonly ExplicitTypeNode TypeNode;
        public readonly string Identifier;
        public readonly ExpressionNode? InitialValue;

        public DeclarationStatementNode(ExplicitTypeNode typeNode, string identifier,
                                        bool isStatic, ExpressionNode? initialValue = null) {
            TypeNode     = typeNode;
            Identifier    = identifier;
            InitialValue = initialValue;
            Static       = isStatic;
        }

        public override List<ASTNode> Children() {
            if (InitialValue == null) return new List<ASTNode> {TypeNode};
            return new List<ASTNode> {TypeNode, InitialValue};
        }
    }
}
