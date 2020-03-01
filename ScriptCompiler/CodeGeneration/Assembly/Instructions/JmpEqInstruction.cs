namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpEqInstruction : Instruction {
        private readonly Location _to;
        
        public JmpEqInstruction(Location to) {
            _to = to;
        }

        protected override string AsString() {
            return $"JEQ {_to}";
        }
    }
}
