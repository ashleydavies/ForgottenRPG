namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class SubInstruction : Instruction {
        private readonly Location _fromLocation;
        private readonly Value _amount;

        public SubInstruction(Location toLocation, Value amount) {
            _fromLocation = toLocation;
            _amount       = amount;
        }

        protected override string AsString() {
            return $"SUB {_fromLocation} {_amount}";
        }
    }
}
