using ScriptCompiler.CodeGeneration.Assembly;

namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class ExportInstruction : Instruction {
        private readonly Label _label;
        private readonly string _exportName;

        public ExportInstruction(Label label, string exportName) {
            _label      = label;
            _exportName = exportName;
        }

        protected override string AsString() {
            return $"EXPORT {_label} {_exportName}";
        }
    }
}
