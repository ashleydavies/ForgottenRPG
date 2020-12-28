using System;
using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements.Expressions {
    public class FloatLiteralNode : NumericLiteralNode, IConstExpr {
        public readonly float Value;

        public FloatLiteralNode(float value) {
            Value = value;
        }

        public override uint[] Calculate(CalcContext _) {
            return new[]{(uint)BitConverter.SingleToInt32Bits(Value)};
        }
    }
}
