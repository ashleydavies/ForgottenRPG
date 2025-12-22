namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpLtInstruction : Instruction {
        public Location Location { get; }
        
        public JmpLtInstruction(Location location) {
            Location = location;
        }

        protected override string AsString() {
            return $"JLT {Location}";
        }
    }
}
