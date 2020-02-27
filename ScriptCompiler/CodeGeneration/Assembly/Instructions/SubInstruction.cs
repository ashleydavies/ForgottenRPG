namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class SubInstruction : BinaryArithmeticInstruction {
        public SubInstruction(Location toLocation, Value amount) : base(toLocation, amount) { }

        protected override string AsString() {
            return $"SUB {ToLocation} {Value}";
        }

        public override bool IsNoop() {
            return Value is NumericConstant nc && nc == 0;
        }
    }
}
