using System.Collections.Generic;
using ScriptCompiler.CodeGeneration;
using ScriptCompiler.Types;

namespace ScriptCompiler.AST.Statements.Expressions {
    public class FunctionCallNode : ExpressionNode {
        public readonly ExpressionNode Function;
        public readonly List<ExpressionNode> Args;
        public FunctionReference FunctionRef(TypeIdentifier typeIdentifier) {
            if (Function is VariableAccessNode vn) {
                return new FunctionReference(null, vn.Identifier);
            }
            if (Function is StructAccessNode sn) {
                return new FunctionReference(typeIdentifier.Identify(sn.Left), sn.Field);
            }

            throw new CompileException("unable to identify function to call", 0, 0);
        }

        public string AsmLabel(TypeIdentifier typeIdentifier) {
            var @ref = FunctionRef(typeIdentifier);
            return $"func_{@ref.Type?.Name}!{@ref.Identifier}";
        }

        public FunctionCallNode(ExpressionNode function, List<ExpressionNode> args) {
            Function = function;
            Args = args;
        }

        public override List<ASTNode> Children() {
            return new(Args);
        }
    }
}
