namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpExternInstruction : Instruction {
        private readonly uint _symbolHash;

        public JmpExternInstruction(uint symbolHash) {
            _symbolHash = symbolHash;
        }

        protected override string AsString() {
            return $"JMP_EXTERN {_symbolHash}";
        }
    }
}
