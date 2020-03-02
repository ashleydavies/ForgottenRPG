namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpLteInstruction : Instruction {
        private readonly Location _to;
        
        public JmpLteInstruction(Location to) {
            _to = to;
        }

        protected override string AsString() {
            return $"JLTE {_to}";
        }
    }
}
