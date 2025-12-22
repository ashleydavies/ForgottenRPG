namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpEqInstruction : Instruction {
        public Location Location { get; }
        
        public JmpEqInstruction(Location location) {
            Location = location;
        }

        protected override string AsString() {
            return $"JEQ {Location}";
        }
    }
}
