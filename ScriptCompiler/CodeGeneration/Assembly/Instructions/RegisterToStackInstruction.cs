using ScriptCompiler.CodeGeneration.Assembly;

namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class RegisterToStackInstruction : Instruction {
        private readonly Register _register;

        public RegisterToStackInstruction(Register register) {
            _register = register;
        }

        protected override string AsString() {
            return $"PUSH {_register}";
        }
    }
}
