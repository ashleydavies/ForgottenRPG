namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class AddInstruction : BinaryArithmeticInstruction {
        public AddInstruction(Location toLocation, Value value) : base(toLocation, value) { } 

        protected override string AsString() {
            return $"ADD {ToLocation} {Value}";
        }

        public override bool IsNoop() {
            return Value is NumericConstant nc && nc == 0;
        }
    }
}
