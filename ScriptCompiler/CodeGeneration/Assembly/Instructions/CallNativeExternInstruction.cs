namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class CallNativeExternInstruction : Instruction {
        private readonly string _functionName;

        public CallNativeExternInstruction(string functionName) {
            _functionName = functionName;
        }

        protected override string AsString() {
            return $"CALL_NATIVE_EXTERN {_functionName}";
        }
    }
}
