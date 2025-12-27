using ScriptCompiler.CodeGeneration.Assembly;

namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class ExportInstruction : Instruction {
        private readonly Label _label;
        private readonly uint _symbolHash;

        public ExportInstruction(Label label, uint symbolHash) {
            _label      = label;
            _symbolHash = symbolHash;
        }

        protected override string AsString() {
            return $"EXPORT {_label.ToString().TrimStart('$')} {_symbolHash}";
        }
    }
}
