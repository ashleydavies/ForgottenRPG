namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpLtInstruction : Instruction {
        private readonly Location _to;
        
        public JmpLtInstruction(Location to) {
            _to = to;
        }

        protected override string AsString() {
            return $"JLT {_to}";
        }
    }
}
