using System.Collections.Generic;
using ScriptCompiler.AST.Statements.Expressions;

namespace ScriptCompiler.AST.Statements {
    public class ForStatementNode : StatementNode {
        private readonly StatementNode? _declaration;
        private readonly ExpressionNode? _condition;
        private readonly ExpressionNode? _update;
        private readonly CodeBlockNode _block;

        public ForStatementNode(StatementNode? declaration, ExpressionNode? condition, ExpressionNode? update, CodeBlockNode block) {
            _declaration = declaration;
            _condition = condition;
            _update = update;
            _block = block;
        }

        public override List<ASTNode> Children() {
            var list = new List<ASTNode> {_block};
            if (_declaration != null) list.Add(_declaration);
            if (_condition != null) list.Add(_condition);
            if (_update != null) list.Add(_update);
            return list;
        }
    }
}
