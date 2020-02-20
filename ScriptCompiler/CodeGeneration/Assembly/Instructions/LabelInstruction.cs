namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class LabelInstruction : Instruction {
        private readonly Label _label;
        
        public LabelInstruction(Label label) {
            _label = label;
        }

        protected override string AsString() {
            // Trim the starting $ of the label
            return $"LABEL {_label.ToString().TrimStart('$')}";
        }
    }
}
