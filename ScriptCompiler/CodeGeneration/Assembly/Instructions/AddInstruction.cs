namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class AddInstruction : Instruction {
        private readonly Location _toLocation;
        private readonly Value _value;

        public AddInstruction(Location toLocation, Value value) {
            _toLocation = toLocation;
            _value     = value;
        }

        protected override string AsString() {
            return $"ADD {_toLocation} {_value}";
        }
    }
}
