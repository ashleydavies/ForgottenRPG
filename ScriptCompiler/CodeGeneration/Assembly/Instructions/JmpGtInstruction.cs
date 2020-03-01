namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpGtInstruction : Instruction {
        private readonly Location _to;
        
        public JmpGtInstruction(Location to) {
            _to = to;
        }

        protected override string AsString() {
            return $"JGT {_to}";
        }
    }
}
