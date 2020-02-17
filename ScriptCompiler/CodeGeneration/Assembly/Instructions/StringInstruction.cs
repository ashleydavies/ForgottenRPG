namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class StringInstruction : Instruction {
        private readonly StringLabel _stringLabel;
        
        public StringInstruction(StringLabel stringLabel) {
            _stringLabel = stringLabel;
        }

        protected override string AsString() {
            return $"STRING {_stringLabel} {_stringLabel.Contents}";
        }
    }
}
