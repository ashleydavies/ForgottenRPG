using ScriptCompiler.CodeGeneration.Assembly;

namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class StackToRegisterInstruction : Instruction {
        private readonly Register _register;

        public StackToRegisterInstruction(Register register) {
            _register = register;
        }

        protected override string AsString() {
            return $"POP {_register}";
        }
    }
}
