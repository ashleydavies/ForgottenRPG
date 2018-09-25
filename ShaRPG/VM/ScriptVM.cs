#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace ShaRPG.VM {
    public class ScriptVM {
        private const int InstructionRegister = 0;
        private readonly List<int> _bytes;
        private readonly Dictionary<int, int> _registers;
        private readonly Flags _flagRegister;
        private readonly Stack<int> _stack;
        public Action<string> PrintMethod { get; set; } = Console.WriteLine;

        public ScriptVM(List<int> bytes) {
            _bytes = bytes;
            _registers = new Dictionary<int, int> {
                [InstructionRegister] = bytes[0]
            };
            _flagRegister = new Flags();
            _stack = new Stack<int>();
        }

        public void Execute() {
            while (_registers[InstructionRegister] >= 0 && _registers[InstructionRegister] < _bytes.Count) {
                ExecuteInstruction();
            }
        }

        private void ExecuteInstruction() {
            int a, b;
            switch (PopInstruction()) {
                case Instruction.NULL:
                    break;
                case Instruction.Literal:
                    PushStack(PopByte());
                    break;
                case Instruction.Add:
                    PushStack(PopStack() + PopStack());
                    SetFlags(PeekStack());
                    break;
                case Instruction.Sub:
                    b = PopStack();
                    a = PopStack();
                    PushStack(a - b);
                    SetFlags(PeekStack());
                    break;
                case Instruction.Mul:
                    PushStack(PopStack() * PopStack());
                    SetFlags(PeekStack());
                    break;
                case Instruction.Div:
                    b = PopStack();
                    a = PopStack();
                    PushStack(a / b);
                    SetFlags(PeekStack());
                    break;
                case Instruction.Inc:
                    PushStack(PopStack() + 1);
                    SetFlags(PeekStack());
                    break;
                case Instruction.Dec:
                    PushStack(PopStack() - 1);
                    SetFlags(PeekStack());
                    break;
                case Instruction.FetchIntData:
                    // TODO
                    break;
                case Instruction.SetIntData:
                    // TODO
                    break;
                case Instruction.Cmp:
                    a = PopStack();
                    b = PopStack();
                    SetFlags(a - b);
                    break;
                case Instruction.Jmp:
                    Jump(true);
                    break;
                case Instruction.JmpNEQ:
                    Jump(!_flagRegister.EQ);
                    break;
                case Instruction.JmpEQ:
                    Jump(_flagRegister.EQ);
                    break;
                case Instruction.JmpGTE:
                    Jump(_flagRegister.GTE);
                    break;
                case Instruction.JmpLTE:
                    Jump(_flagRegister.LTE);
                    break;
                case Instruction.JmpGT:
                    Jump(_flagRegister.GT);
                    break;
                case Instruction.JmpLT:
                    Jump(_flagRegister.LT);
                    break;
                case Instruction.StackToRegister:
                    _registers[PopByte()] = PopStack();
                    break;
                case Instruction.RegisterToStack:
                    if (!_registers.ContainsKey(PeekByte())) _registers[PeekByte()] = 0;
                    PushStack(_registers[PopByte()]);
                    break;
                case Instruction.Print:
                    PrintMethod(GetUserDataString(PopStack()));
                    break;
                case Instruction.PrintInt:
                    PrintMethod(PopStack().ToString());
                    break;
                default:
                    PrintMethod("Unexpected instruction");
                    break;
            }
        }

        private string GetUserDataString(int userDataId) {
            // Add one as the first byte is the initial instruction register
            int pos = _bytes[userDataId + 1];
            int len = _bytes[pos];
            return new string(_bytes.GetRange(pos + 1, len).Select(Convert.ToChar).ToArray());
        }

        private void Jump(bool doJump) {
            int instructionPointer = PopByte();
            if (doJump) {
                _registers[InstructionRegister] = instructionPointer;
            }
        }

        private void SetFlags(int basedOn) {
            if (basedOn == 0) {
                _flagRegister.EQ = true;
                _flagRegister.GTE = true;
                _flagRegister.LTE = true;
                _flagRegister.GT = false;
                _flagRegister.LT = false;
            } else if (basedOn > 0) {
                _flagRegister.EQ = false;
                _flagRegister.GTE = true;
                _flagRegister.LTE = false;
                _flagRegister.GT = true;
                _flagRegister.LT = false;
            } else {
                _flagRegister.EQ = false;
                _flagRegister.GTE = false;
                _flagRegister.LTE = true;
                _flagRegister.GT = false;
                _flagRegister.LT = true;
            }
        }

        private Instruction PopInstruction() => (Instruction) PopByte();
        private void IncrementIr() => _registers[InstructionRegister] += 1;

        private int PopByte() {
            var data = _bytes[_registers[InstructionRegister]];
            IncrementIr();
            return data;
        }

        private int PeekByte() => _bytes[_registers[InstructionRegister]];
        private void PushStack(int data) => _stack.Push(data);
        private int PeekStack() => _stack.Peek();
        private int PopStack() => _stack.Pop();

        public enum Instruction {
            NULL,
            Literal,
            Add,
            Sub,
            Mul,
            Div,
            Inc,
            Dec,
            StackToRegister,
            RegisterToStack,
            FetchIntData,
            SetIntData,
            Cmp,
            Jmp,
            JmpNEQ,
            JmpEQ,
            JmpGT,
            JmpLT,
            JmpGTE,
            JmpLTE,
            Print,
            PrintInt,
        }

        private class Flags {
            public bool EQ;
            public bool GT;
            public bool LT;
            public bool GTE;
            public bool LTE;
        }
    }
}
