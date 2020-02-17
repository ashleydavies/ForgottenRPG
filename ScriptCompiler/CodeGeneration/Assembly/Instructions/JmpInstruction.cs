namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpInstruction : Instruction {
        private readonly Location _to;
        
        public JmpInstruction(Location to) {
            _to = to;
        }

        protected override string AsString() {
            return $"JMP {_to}";
        }
    }
}
