using System;
using ScriptCompiler.Visitors;

namespace ScriptCompiler.CodeGeneration.Assembly {
    public class NumericConstant : Value {
        private readonly int _amount;

        public NumericConstant(int amount) {
            _amount = amount;
        }

        public override string ToString() {
            return $"{_amount}";
        }
    }
}
