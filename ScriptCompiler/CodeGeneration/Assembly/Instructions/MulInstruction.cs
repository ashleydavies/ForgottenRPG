namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class MulInstruction : Instruction {
        private readonly Location _toLocation;
        private readonly Value _value;

        public MulInstruction(Location toLocation, Value value) {
            _toLocation = toLocation;
            _value     = value;
        }

        protected override string AsString() {
            return $"MUL {_toLocation} {_value}";
        }
    }
}
