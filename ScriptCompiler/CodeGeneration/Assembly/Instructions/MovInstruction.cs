namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class MovInstruction : Instruction {
        private readonly Location _from;
        private readonly Location _to;
        
        public MovInstruction(Location @from, Location to) {
            _from = @from;
            _to = to;
        }

        protected override string AsString() {
            return $"MOV {_to} {_from}";
        }
    }
}
