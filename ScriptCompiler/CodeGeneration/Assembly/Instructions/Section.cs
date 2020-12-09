namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class Section : Instruction {
        public static readonly Section DataSection = new Section("data");
        public static readonly Section StaticSection = new Section("static");
        public static readonly Section CodeSection = new Section("text");

        private readonly string _sectionType;

        private Section(string type) {
            _sectionType = type;
        }

        protected override string AsString() {
            return $".{_sectionType}";
        }
    }
}
