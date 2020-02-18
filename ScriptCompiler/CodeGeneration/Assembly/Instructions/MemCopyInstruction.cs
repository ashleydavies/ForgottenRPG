namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class MemCopyInstruction : Instruction {
        private readonly Value _from;
        private readonly Value _to;
        
        public MemCopyInstruction(Value to, Value from) {
            _to = to;
            _from = from;
        }
        
        protected override string AsString() {
            return $"MEMCOPY {_to} {_from}";
        }
    }
}
