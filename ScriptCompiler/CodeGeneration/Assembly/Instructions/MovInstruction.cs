namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class MovInstruction : Instruction {
        private readonly Location _to;
        private readonly Location _from;
        
        public MovInstruction(Location to, Location @from) {
            _to = to;
            _from = @from;
        }

        protected override string AsString() {
            return $"MOV {_to} {_from}";
        }
    }
}
