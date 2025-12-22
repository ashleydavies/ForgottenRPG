namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpInstruction : Instruction {
        public Location Location { get; }
        
        public JmpInstruction(Location location) {
            Location = location;
        }

        protected override string AsString() {
            return $"JMP {Location}";
        }
    }
}
