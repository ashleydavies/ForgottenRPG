using ScriptCompiler.Types;

namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class DivInstruction : BinaryArithmeticInstruction {
        public DivInstruction(Location toLocation, Value value, SType type) : base(toLocation, value, type) { }

        protected override string AsString() {
            return $"DIV{InstructionSuffix()} {ToLocation} {Value}";
        }

        public override bool IsNoop() {
            return Value is NumericConstant nc && nc == 1;
        }
    }
}
