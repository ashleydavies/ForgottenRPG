namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class DivInstruction : Instruction {
        private readonly Location _toLocation;
        private readonly Value _value;

        public DivInstruction(Location toLocation, Value value) {
            _toLocation = toLocation;
            _value     = value;
        }

        protected override string AsString() {
            return $"DIV {_toLocation} {_value}";
        }
    }
}
