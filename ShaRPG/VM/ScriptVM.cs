using System;
using System.Collections.Generic;

namespace ShaRPG.VM
{
    public class ScriptVM
    {
        private const int InstructionRegister = 0;

        private readonly List<int> _bytes;
        private readonly Stack<int> _stack;
        private readonly Dictionary<int, int> _registers;

        private enum Instruction
        {
            Literal,
            Add,
            Sub,
            Mul,
            Div,
        }

        public ScriptVM(List<int> bytes)
        {
            _bytes = bytes;
            _stack = new Stack<int>();
            _registers = new Dictionary<int, int>
            {
                [InstructionRegister] = 0
            };
        }

        private void ExecuteInstruction()
        {
            switch (PopInstruction())
            {
                case Instruction.Literal:
                    PushStack(PopByte());
                    break;
                case Instruction.Add:
                    PushStack(PopStack() + PopStack());
                    break;
                case Instruction.Sub:
                    PushStack(PopStack() - PopStack());
                    break;
                case Instruction.Mul:
                    PushStack(PopStack()*PopStack());
                    break;
                case Instruction.Div:
                    PushStack(PopStack()/PopStack());
                    break;
            }
        }

        private Instruction PopInstruction() => (Instruction) PopByte();
        private void IncrementIr() => _bytes[_registers[InstructionRegister]] += 1;

        private int PopByte()
        {
            var data = _bytes[_registers[InstructionRegister]];
            IncrementIr();
            return data;
        }

        private int GetIr() => _bytes[_registers[InstructionRegister]];

        private void PushStack(int data) => _stack.Push(data);

        private int PopStack() => _stack.Pop();
    }
}