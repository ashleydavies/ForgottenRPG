namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class DivInstruction : BinaryArithmeticInstruction {
        public DivInstruction(Location toLocation, Value value) : base(toLocation, value) { }

        protected override string AsString() {
            return $"DIV {ToLocation} {Value}";
        }

        public override bool IsNoop() {
            return Value is NumericConstant nc && nc == 1;
        }
    }
}
