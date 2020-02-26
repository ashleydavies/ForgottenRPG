namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class MulInstruction : BinaryArithmeticInstruction {
        public MulInstruction(Location toLocation, Value value) : base(toLocation, value) { }

        protected override string AsString() {
            return $"MUL {ToLocation} {Value}";
        }

        public override bool IsNoop() {
            return Value is NumericConstant nc && nc == 1;
        }
    }
}
