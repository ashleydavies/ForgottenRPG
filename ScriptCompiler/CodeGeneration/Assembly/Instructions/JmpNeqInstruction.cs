namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpNeqInstruction : Instruction {
        private readonly Location _to;
        
        public JmpNeqInstruction(Location to) {
            _to = to;
        }

        protected override string AsString() {
            return $"JNEQ {_to}";
        }
    }
}
