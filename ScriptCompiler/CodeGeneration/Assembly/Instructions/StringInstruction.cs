namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class StringInstruction : Instruction {
        public StringLabel Label { get; }
        
        public StringInstruction(StringLabel stringLabel) {
            Label = stringLabel;
        }

        protected override string AsString() {
            // Trim off the starting ! from the label as that is not used in the string instruction right now
            return $"STRING {Label.ToString().TrimStart('!')} {Label.Contents}";
        }
    }
}
