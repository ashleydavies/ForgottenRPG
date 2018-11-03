#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using OpenTK.Audio.OpenAL;

#endregion

namespace ShaRPG.VM {
    // Bytecode VM for a small language. 32-bit integers are the smallest atomic unit.
    public class ScriptVM {
        private const int InstructionRegister = 0;
        private const int StackPointerRegister = 1;
        private readonly List<int> _bytes;
        private readonly Dictionary<int, int> _registers;
        private readonly Flags _flagRegister;
        private readonly Stack<int> _stack;
        private readonly Dictionary<int, IMemoryPage> _memory = new Dictionary<int, IMemoryPage>();
        public Action<string> PrintMethod { get; set; } = Console.WriteLine;

        public ScriptVM(List<int> bytes) {
            _bytes = bytes;
            // Copy the bytes to memory
            List<MemoryPage> instructionPages = new List<MemoryPage>();
            for (int i = 0; i < bytes.Count; i++) {
                int page   = i / MemoryPage.PageSize;
                int offset = i % MemoryPage.PageSize;

                if (!_memory.ContainsKey(page)) {
                    var memoryPage = new MemoryPage();
                    _memory[page] = memoryPage;
                    instructionPages.Add(memoryPage);
                }
                
                _memory[page].WriteAddress(offset, bytes[i]);
            }
            
            // Lock the pages for writing so programs can't overwrite instruction data
            instructionPages.ForEach(x => x.Lock());
            
            _registers = new Dictionary<int, int> {
                [InstructionRegister] = bytes[0],
                [StackPointerRegister] = instructionPages.Count * MemoryPage.PageSize
            };
            
            _flagRegister = new Flags();
            _stack = new Stack<int>();
        }

        public void Execute() {
            while (_registers[InstructionRegister] >= 0 && _registers[InstructionRegister] < _bytes.Count) {
                ExecuteInstruction();
            }
        }

        // TODO: Migrate fully from stack to register machine for performance improvements.
        // The whole VM was originally intended to be a full stack machine, but the higher level
        //   form is compiled more trivially into a register machine, and has more capacity for optimisation.
        // Currently, the pseudo-assembly language is emulating a register machine by exclusively using registers and
        //   pushing and popping before each operation, which is very inefficient.
        // Not to mention horrendously confusing, since there is also the higher level stack stored in `_memory`.
        //
        // ¯\_(ツ)_/¯
        private void ExecuteInstruction() {
            int a, b;
            switch (ReadInstruction()) {
                case Instruction.NULL:
                    break;
                case Instruction.Literal:
                    PushStack(ReadInstructionByte());
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
                    _registers[ReadInstructionByte()] = PopStack();
                    break;
                case Instruction.RegisterToStack:
                    if (!_registers.ContainsKey(PeekInstructionByte())) _registers[PeekInstructionByte()] = 0;
                    PushStack(_registers[ReadInstructionByte()]);
                    break;
                case Instruction.Print:
                    PrintMethod(StringFromMemory(PopStack()));
                    break;
                case Instruction.PrintInt:
                    PrintMethod(PopStack().ToString());
                    break;
                case Instruction.MemWrite:
                    var val = PopStack();
                    var pos = PopStack();
                    Console.WriteLine($"Writing mem[{pos}]={val}");
                    WriteMemory(pos, val);
                    break;
                case Instruction.MemRead:
                    var loc = PopStack();
                    Console.WriteLine($"Reading mem[{loc}]");
                    _stack.Push(ReadMemory(loc));
                    break;
                default:
                    PrintMethod("Unexpected instruction");
                    break;
            }
        }

        private string StringFromMemory(int userDataId) {
            // Add one as the first byte is the initial instruction register
            int len = ReadMemory(userDataId);
            
            char[] userString = new char[len];
            for (int i = 0; i < len; i++) {
                userString[i] = Convert.ToChar(ReadMemory(userDataId + 1 + i));
            }
            
            return new string(userString);
        }

        private void Jump(bool doJump) {
            int instructionPointer = ReadInstructionByte();
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

        private void IncrementIr() => _registers[InstructionRegister] += 1;
        private Instruction ReadInstruction() => (Instruction) ReadInstructionByte();

        private int ReadInstructionByte() {
            var data = ReadMemory(_registers[InstructionRegister]);
            IncrementIr();
            return data;
        }

        private int PeekInstructionByte() => ReadMemory(_registers[InstructionRegister]);
        private void PushStack(int data) => _stack.Push(data);
        private int PeekStack() => _stack.Peek();
        private int PopStack() => _stack.Pop();

        private int ReadMemory(int index) {
            var (page, offset) = GetPageAndOffset(index);
            if (!_memory.ContainsKey(page)) _memory[page] = new MemoryPage();
            return _memory[page].ReadAddress(offset);
        }

        private void WriteMemory(int index, int value) {
            var (page, offset) = GetPageAndOffset(index);
            if (!_memory.ContainsKey(page)) _memory[page] = new MemoryPage();
            _memory[page].WriteAddress(offset, value);
        }

        private (int, int) GetPageAndOffset(int memIndex) {
            return (memIndex / MemoryPage.PageSize, memIndex % MemoryPage.PageSize);
        }

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
            MemWrite,
            MemRead,
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
