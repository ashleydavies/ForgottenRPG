namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class JmpExternInstruction : Instruction {
        private readonly string _functionName;

        public JmpExternInstruction(string functionName) {
            _functionName = functionName;
        }

        protected override string AsString() {
            return $"JMP_EXTERN {_functionName}";
        }
    }
}
