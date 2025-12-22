namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpGtInstruction : Instruction {
        public Location Location { get; }
        
        public JmpGtInstruction(Location location) {
            Location = location;
        }

        protected override string AsString() {
            return $"JGT {Location}";
        }
    }
}
