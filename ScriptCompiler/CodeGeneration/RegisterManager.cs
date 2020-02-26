using System.Collections.Generic;
using ScriptCompiler.CodeGeneration.Assembly;

namespace ScriptCompiler.CodeGeneration {
    public class RegisterManager {
        // The first two registers are always reserved
        public readonly Register InstructionPointer;
        public readonly Register StackPointer;
        private const int ReservedRegCount = 2;

        private readonly List<bool> _registersInUse = new List<bool>();
        private readonly HashSet<Register> _locals = new HashSet<Register>();

        public RegisterManager() {
            InstructionPointer = NewRegister();
            StackPointer = NewRegister();
        }

        public Register NewRegister() {
            int reg = 0;
            while (true) {
                if (_registersInUse.Count <= reg) {
                    _registersInUse.Add(true);
                    break;
                }

                if (_registersInUse[reg] == false) {
                    _registersInUse[reg] = true;
                    break;
                }

                reg++;
            }

            var register = new Register(reg, this);
            if (reg >= ReservedRegCount) {
                _locals.Add(register);
            }
            return register;
        }
        
        public void ClearRegister(Register register) {
            _locals.Remove(register);
            _registersInUse[register.Number] = false;
        }

        public HashSet<Register> GetLocals() {
            return new HashSet<Register>(_locals);
        }
    }
}
