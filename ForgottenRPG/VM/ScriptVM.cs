using System;
using System.Collections.Generic;
using ForgottenRPG.Service;

namespace ForgottenRPG.VM {
    // Bytecode VM for a small language. 32-bit integers are the smallest atomic unit.
    public class ScriptVm {
        private const int InstructionRegister = 0;
        private const int StackPointerRegister = 1;
        private readonly List<int> _bytes;
        private readonly Dictionary<int, int> _registers;
        private readonly Flags _flagRegister;
        private readonly Stack<int> _stack;
        private readonly Dictionary<uint, IMemoryPage> _memory = new Dictionary<uint, IMemoryPage>();
        private readonly uint _stackPointerBase;
        private readonly uint _maxExecutionAddress;
        public Action<string> PrintMethod { get; set; } = s => ServiceLocator.LogService.Log(LogType.Info, "VM : " + s);

        public ScriptVm(List<int> bytes) {
            _bytes = bytes;
            // Copy the bytes to memory
            var  allPreloadedPages = new List<MemoryPage>();
            // Subset of allPreloadedPages; just instruction-related ones
            var  instructionPages  = new List<MemoryPage>();
            uint addr              = 0;
            for (uint i = 0; i < bytes.Count; i++, addr++) {
                // Special case: if we are the last byte of the static section, we do a page jump
                if (i == bytes[1]) {
                    addr += (MemoryPage.PageSize - addr % MemoryPage.PageSize) % MemoryPage.PageSize;
                }

                uint page   = addr / MemoryPage.PageSize;
                uint offset = addr % MemoryPage.PageSize;

                if (!_memory.ContainsKey(page)) {
                    var memoryPage = new MemoryPage();
                    _memory[page] = memoryPage;
                    allPreloadedPages.Add(memoryPage);

                    if (addr > bytes[1]) {
                        instructionPages.Add(memoryPage);
                    }
                }

                _memory[page].WriteAddress(offset, bytes[(int) i]);
            }

            _maxExecutionAddress = addr;

            // Lock the pages for writing so programs can't overwrite instruction data
            instructionPages.ForEach(x => x.Lock());

            _stackPointerBase = (uint) allPreloadedPages.Count * MemoryPage.PageSize;
            _registers = new Dictionary<int, int> {
                [InstructionRegister]  = bytes[0],
                [StackPointerRegister] = (int) _stackPointerBase
            };

            _flagRegister = new Flags();
            _stack        = new Stack<int>();
        }

        public void Execute() {
            while (_registers[InstructionRegister] >= 0 && _registers[InstructionRegister] < _maxExecutionAddress) {
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
                case Instruction.Null:
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
                case Instruction.JmpNeq:
                    Jump(!_flagRegister.Eq);
                    break;
                case Instruction.JmpEq:
                    Jump(_flagRegister.Eq);
                    break;
                case Instruction.JmpGte:
                    Jump(_flagRegister.Gte);
                    break;
                case Instruction.JmpLte:
                    Jump(_flagRegister.Lte);
                    break;
                case Instruction.JmpGt:
                    Jump(_flagRegister.Gt);
                    break;
                case Instruction.JmpLt:
                    Jump(_flagRegister.Lt);
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
                    WriteMemory(pos, val);
                    break;
                case Instruction.MemRead:
                    var loc = PopStack();
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
            int instructionPointer = PopStack(); //ReadInstructionByte();
            if (doJump) {
                _registers[InstructionRegister] = instructionPointer;
            }
        }

        private void SetFlags(int basedOn) {
            if (basedOn == 0) {
                _flagRegister.Eq  = true;
                _flagRegister.Gte = true;
                _flagRegister.Lte = true;
                _flagRegister.Gt  = false;
                _flagRegister.Lt  = false;
            } else if (basedOn > 0) {
                _flagRegister.Eq  = false;
                _flagRegister.Gte = true;
                _flagRegister.Lte = false;
                _flagRegister.Gt  = true;
                _flagRegister.Lt  = false;
            } else {
                _flagRegister.Eq  = false;
                _flagRegister.Gte = false;
                _flagRegister.Lte = true;
                _flagRegister.Gt  = false;
                _flagRegister.Lt  = true;
            }
        }

        private void        IncrementIr()     => _registers[InstructionRegister] += 1;
        private Instruction ReadInstruction() => (Instruction) ReadInstructionByte();

        private int ReadInstructionByte() {
            var data = ReadMemory(_registers[InstructionRegister]);
            IncrementIr();
            return data;
        }

        private int  PeekInstructionByte() => ReadMemory(_registers[InstructionRegister]);
        private void PushStack(int data)   => _stack.Push(data);
        private int  PeekStack()           => _stack.Peek();
        private int  PopStack()            => _stack.Pop();

        private int ReadMemory(int sindex) {
            // Treat memory addresses as unsigned integers
            var index = (uint) sindex;
            var (page, offset) = GetPageAndOffset(index);
            if (!_memory.ContainsKey(page)) _memory[page] = new MemoryPage();
#if DEBUG_MEM
            if (index >= _stackPointerBase) {
                Console.WriteLine($"Reading {_memory[page].ReadAddress(offset)} from {index}");
                _maxMem = Math.Max(_maxMem, (uint) _registers[StackPointerRegister]);
                var output = "";
                for (uint i = _stackPointerBase; i <= _maxMem; i++) {
                    var (pN, oN) = GetPageAndOffset(i);
                    var sep = ":";
                    if (_registers[StackPointerRegister] == i) sep = "=";
                    output += $"{i}{sep} {_memory[pN]?.ReadAddress(oN) ?? 0}; ";
                }

                Console.WriteLine(output);
            }
#endif

            return _memory[page].ReadAddress(offset);
        }

        private uint _maxMem = 0;

        private void WriteMemory(int sindex, int value) {
            var index = (uint) sindex;
            var (page, offset) = GetPageAndOffset(index);
            if (!_memory.ContainsKey(page)) _memory[page] = new MemoryPage();
            _memory[page].WriteAddress(offset, value);

#if DEBUG_MEM
            Console.WriteLine($"Writing {value} to {index}");
            _maxMem = Math.Max(_maxMem, (uint) _registers[StackPointerRegister]);
            var output = "";
            for (uint i = _stackPointerBase; i <= _maxMem; i++) {
                var (pN, oN) = GetPageAndOffset(i);
                var sep = ":";
                if (_registers[StackPointerRegister] == i) sep = "=";
                output += $"{i}{sep} {_memory[pN]?.ReadAddress(oN) ?? 0}; ";
            }

            Console.WriteLine(output);
#endif
        }

        private (uint, uint) GetPageAndOffset(uint memIndex) {
            return (memIndex / MemoryPage.PageSize, memIndex % MemoryPage.PageSize);
        }

        public enum Instruction {
            Null,
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
            JmpNeq,
            JmpEq,
            JmpGt,
            JmpLt,
            JmpGte,
            JmpLte,
            Print,
            PrintInt,
            MemWrite,
            MemRead,
        }

        private class Flags {
            public bool Eq;
            public bool Gt;
            public bool Lt;
            public bool Gte;
            public bool Lte;
        }
    }
}
