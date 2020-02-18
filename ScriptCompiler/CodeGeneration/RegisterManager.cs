using System.Collections.Generic;
using ScriptCompiler.CodeGeneration.Assembly;

namespace ScriptCompiler.CodeGeneration {
    public class RegisterManager {
        // The first two registers are always reserved
        public readonly Register InstructionPointer;
        public readonly Register StackPointer;

        private readonly List<bool> _registersInUse = new List<bool>();

        public RegisterManager() {
            InstructionPointer = NewRegister();
            StackPointer = NewRegister();
        }

        public Register NewRegister() {
            int reg = 0;
            while (true) {
                if (_registersInUse.Count <= reg) {
                    _registersInUse.Add(true);
                    return new Register(reg, this);
                }

                if (_registersInUse[reg] == false) {
                    _registersInUse[reg] = true;
                    return new Register(reg, this);
                }

                reg++;
            }
        }
        
        public void ClearRegister(Register register) {
            _registersInUse[register.Number] = false;
        }
    }
}
