namespace ScriptCompiler.AST.Statements.Expressions {
    public abstract class ExpressionNode : StatementNode, IConstExpr {
        public virtual uint[]? Calculate(CalcContext _) {
            return null;
        }
    }
}
