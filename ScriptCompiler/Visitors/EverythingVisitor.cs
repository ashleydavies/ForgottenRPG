using ScriptCompiler.AST;

namespace ScriptCompiler.Visitors {
    public abstract class EverythingVisitor<T> : Visitor<T> {
        protected abstract T Base();
        protected abstract T Aggregate(T a, T b);

        public override T Visit(ASTNode node) {
            var result = Base();
            foreach (dynamic child in node.Children()) {
                result = Aggregate(result, (this as dynamic).Visit(child));
            }
            return result;
        }
    }
}