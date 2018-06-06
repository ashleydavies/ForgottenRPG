using ScriptCompiler.AST;

namespace ScriptCompiler.Visitors {
    public abstract class Visitor<T> {
        public abstract T Visit(ASTNode node);
    }
}