namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class LabelLiteralInstruction : Instruction {
        public Location Label { get; }
        
        public LabelLiteralInstruction(Location label) {
            Label = label;
        }

        protected override string AsString() {
            var labelStr = Label.ToString();
            if (labelStr.StartsWith("!")) {
                return $"PUSH {labelStr}";
            }
            // Trim the starting $ of the label
            return $"PUSH ${labelStr.TrimStart('$')}";
        }
    }
}
