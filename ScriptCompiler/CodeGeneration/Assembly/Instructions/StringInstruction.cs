namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class StringInstruction : Instruction {
        private readonly StringLabel _stringLabel;
        
        public StringInstruction(StringLabel stringLabel) {
            _stringLabel = stringLabel;
        }

        protected override string AsString() {
            // Trim off the starting ! from the label as that is not used in the string instruction right now
            return $"STRING {_stringLabel.ToString().TrimStart('!')} {_stringLabel.Contents}";
        }
    }
}
