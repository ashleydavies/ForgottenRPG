namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class MovInstruction : Instruction {
        public Location To { get; }
        public Value From { get; }
        
        public MovInstruction(Location to, Value @from) {
            To = to;
            From = @from;
        }

        protected override string AsString() {
            return $"MOV {To} {From}";
        }
    }
}
