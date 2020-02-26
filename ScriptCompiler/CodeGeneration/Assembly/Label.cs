namespace ScriptCompiler.CodeGeneration.Assembly {
    public class Label : Location {
        public static readonly Label EndLabel = new Label("END");
        private readonly string _label;

        public Label(string label) {
            _label = label;
        }

        public override string ToString() {
            return $"${_label}";
        }
    }
}
