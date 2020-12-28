using ScriptCompiler.Types;

namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class SubInstruction : BinaryArithmeticInstruction {
        public SubInstruction(Location toLocation, Value amount, SType? type = null) : base(toLocation, amount, type) { }

        protected override string AsString() {
            return $"SUB{InstructionSuffix()} {ToLocation} {Value}";
        }

        public override bool IsNoop() {
            return Value is NumericConstant nc && nc == 0;
        }
    }
}
