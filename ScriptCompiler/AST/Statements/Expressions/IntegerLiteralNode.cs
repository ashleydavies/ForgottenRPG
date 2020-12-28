using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements.Expressions {
    public class IntegerLiteralNode : NumericLiteralNode, IConstExpr {
        public readonly uint Value;

        public IntegerLiteralNode(uint value) {
            Value = value;
        }

        public override uint[] Calculate(CalcContext _) {
            return new[]{Value};
        }
    }
}
