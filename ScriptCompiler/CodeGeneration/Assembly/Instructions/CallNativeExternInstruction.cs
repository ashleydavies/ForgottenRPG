namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class CallNativeExternInstruction : Instruction {
        private readonly uint _symbolHash;

        public CallNativeExternInstruction(uint symbolHash) {
            _symbolHash = symbolHash;
        }

        protected override string AsString() {
            return $"CALL_NATIVE_EXTERN {_symbolHash}";
        }
    }
}
