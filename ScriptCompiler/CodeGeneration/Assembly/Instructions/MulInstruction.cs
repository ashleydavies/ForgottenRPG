using ScriptCompiler.Types;

namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class MulInstruction : BinaryArithmeticInstruction {
        public MulInstruction(Location toLocation, Value value, SType type) : base(toLocation, value, type) { }

        protected override string AsString() {
            return $"MUL{InstructionSuffix()} {ToLocation} {Value}";
        }

        public override bool IsNoop() {
            return Value is NumericConstant nc && nc == 1;
        }
    }
}
