namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpNeqInstruction : Instruction {
        public Location Location { get; }
        
        public JmpNeqInstruction(Location location) {
            Location = location;
        }

        protected override string AsString() {
            return $"JNEQ {Location}";
        }
    }
}
