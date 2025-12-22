namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpGteInstruction : Instruction {
        public Location Location { get; }
        
        public JmpGteInstruction(Location location) {
            Location = location;
        }

        protected override string AsString() {
            return $"JGTE {Location}";
        }
    }
}
