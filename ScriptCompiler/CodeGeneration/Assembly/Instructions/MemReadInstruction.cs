namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class MemReadInstruction : Instruction {
        private readonly Location _to;
        private readonly Value _from;
        
        public MemReadInstruction(Location to, Value from) {
            _to = to;
            _from = from;
        }

        protected override string AsString() {
            return $"MEMREAD {_to} {_from}";
        }
    }
}
