namespace ScriptCompiler.CodeGeneration.Assembly {
    public class StringLabel : Location {
        private readonly Label _label;
        public readonly string Contents;
        
        public StringLabel(int index, string s) {
            _label = new Label($"str_{index}");
            Contents = s;
        }

        public override string ToString() {
            return $"!{_label}";
        }
    }
}
