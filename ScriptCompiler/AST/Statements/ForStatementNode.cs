using System.Collections.Generic;
using ScriptCompiler.AST.Statements.Expressions;

namespace ScriptCompiler.AST.Statements {
    public class ForStatementNode : StatementNode {
        public readonly StatementNode? Declaration;
        public readonly ExpressionNode? Condition;
        public readonly ExpressionNode? Update;
        public readonly CodeBlockNode Block;

        public ForStatementNode(StatementNode? declaration, ExpressionNode? condition, ExpressionNode? update, CodeBlockNode block) {
            Declaration = declaration;
            Condition = condition;
            Update = update;
            Block = block;
        }

        public override List<ASTNode> Children() {
            var list = new List<ASTNode> {Block};
            if (Declaration != null) list.Add(Declaration);
            if (Condition != null) list.Add(Condition);
            if (Update != null) list.Add(Update);
            return list;
        }
    }
}
