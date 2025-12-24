namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class CallNativeInstruction : Instruction {
        private readonly int _functionId;
        
        public CallNativeInstruction(int functionId) {
            _functionId = functionId;
        }

        protected override string AsString() {
            return $"CALLNATIVE {_functionId}";
        }
    }
}
