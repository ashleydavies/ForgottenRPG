using System.Collections.Generic;

namespace ScriptCompiler.CodeGeneration {
    public sealed class StackAssemblyEmitter {
        private readonly List<string> _staticLines = new List<string>();
        private readonly List<string> _dataLines = new List<string>();
        private readonly List<string> _textLines = new List<string>();

        public void EmitStatic(string line) {
            _staticLines.Add(line);
        }

        public void EmitData(string line) {
            _dataLines.Add(line);
        }

        public void EmitText(string opcode) {
            _textLines.Add(opcode);
        }

        public void EmitText(string opcode, string operand) {
            _textLines.Add($"{opcode} {operand}");
        }

        public void EmitLabel(string label) {
            _textLines.Add($"label {label}");
        }

        public IReadOnlyList<string> Lines {
            get {
                var lines = new List<string>(_staticLines.Count + _dataLines.Count + _textLines.Count + 3) {
                    ".static"
                };
                lines.AddRange(_staticLines);
                lines.Add(".data");
                lines.AddRange(_dataLines);
                lines.Add(".text");
                lines.AddRange(_textLines);
                return lines;
            }
        }
    }
}
