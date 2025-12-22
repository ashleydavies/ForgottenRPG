namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class LabelInstruction : Instruction {
        public Label Label { get; }
        
        public LabelInstruction(Label label) {
            Label = label;
        }

        protected override string AsString() {
            // Trim the starting $ of the label
            return $"LABEL {Label.ToString().TrimStart('$')}";
        }
    }
}
