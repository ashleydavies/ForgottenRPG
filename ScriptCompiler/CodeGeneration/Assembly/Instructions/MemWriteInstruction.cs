namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class MemWriteInstruction : Instruction {
        private readonly Value _to;
        private readonly Value _value;
        
        public MemWriteInstruction(Value to, Value value) {
            _to = to;
            _value = value;
        }

        protected override string AsString() {
            return $"MEMWRITE {_to} {_value}";
        }
    }
}
