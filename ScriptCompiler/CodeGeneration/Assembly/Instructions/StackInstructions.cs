using ForgottenRPG.VM;

namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class LiteralInstruction : Instruction {
        public readonly uint Value;

        public LiteralInstruction(uint value) {
            Value = value;
        }

        protected override string AsString() {
            return $"LITERAL {Value}";
        }
    }

    public class RegisterToStackInstruction : Instruction {
        public readonly int RegisterIndex;

        public RegisterToStackInstruction(int registerIndex) {
            RegisterIndex = registerIndex;
        }

        protected override string AsString() {
            return $"REG_TO_STACK r{RegisterIndex}";
        }
    }

    public class StackToRegisterInstruction : Instruction {
        public readonly int RegisterIndex;

        public StackToRegisterInstruction(int registerIndex) {
            RegisterIndex = registerIndex;
        }

        protected override string AsString() {
            return $"STACK_TO_REG r{RegisterIndex}";
        }
    }
    
    public class MemReadInstruction : Instruction {
        protected override string AsString() => "MEMREAD";
    }

    public class MemWriteInstruction : Instruction {
        protected override string AsString() => "MEMWRITE";
    }
    
    public class MemCopyInstruction : Instruction {
        protected override string AsString() => "MEMCOPY";
    }
    
    public class IncInstruction : Instruction {
        protected override string AsString() => "INC";
    }
    
    public class DecInstruction : Instruction {
        protected override string AsString() => "DEC";
    }
}
