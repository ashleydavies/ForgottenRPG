using System.Data;
using ScriptCompiler.Types;

namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class AddInstruction : BinaryArithmeticInstruction {
        public AddInstruction(Location toLocation, Value value, SType? type = null) : base(toLocation, value, type) { } 

        protected override string AsString() {
            return $"ADD{InstructionSuffix()} {ToLocation} {Value}";
        }

        public override bool IsNoop() {
            return Value is NumericConstant nc && nc == 0;
        }
    }
}
