namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class MovInstruction : Instruction {
        private readonly Location _to;
        private readonly Value _from;
        
        public MovInstruction(Location to, Value @from) {
            _to = to;
            _from = @from;
        }

        protected override string AsString() {
            return $"MOV {_to} {_from}";
        }
    }
}
