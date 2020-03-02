namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpGteInstruction : Instruction {
        private readonly Location _to;
        
        public JmpGteInstruction(Location to) {
            _to = to;
        }

        protected override string AsString() {
            return $"JGTE {_to}";
        }
    }
}
