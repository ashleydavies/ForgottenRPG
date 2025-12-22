namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpLteInstruction : Instruction {
        public Location Location { get; }
        
        public JmpLteInstruction(Location location) {
            Location = location;
        }

        protected override string AsString() {
            return $"JLTE {Location}";
        }
    }
}
