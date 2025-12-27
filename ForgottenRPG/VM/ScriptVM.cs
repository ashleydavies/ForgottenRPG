using System;
using System.Collections.Generic;
using System.Globalization;
using ForgottenRPG.Service;
using ForgottenRPG.NativeFunctions;
using static System.BitConverter;

namespace ForgottenRPG.VM {
    // Bytecode VM for a small language. 32-bit integers are the smallest atomic unit.
    public class ScriptVm {
        private const uint InstructionRegister = 0;
        private const uint StackPointerRegister = 1;
        private readonly List<uint> _bytes;
        private readonly Dictionary<uint, uint> _registers;
        private readonly Flags _flagRegister;
        private readonly Stack<uint> _stack;
        private readonly Dictionary<uint, IMemoryPage> _memory = new Dictionary<uint, IMemoryPage>();
        private readonly List<MemoryPage> _instructionPages = new List<MemoryPage>();
        private bool _instructionPagesLocked;
        private readonly uint _stackPointerBase;
        private readonly uint _maxExecutionAddress;
        private readonly Dictionary<int, object> _entityRegistry = new Dictionary<int, object>();
        private readonly Stack<object> _pendingData = new Stack<object>();
        private NativeFunctionRegistry _nativeFunctionRegistry;
        private readonly uint _loadOffset;
        private readonly List<uint>? _stdlibBytes;
        private readonly uint _stdlibBase;
        public Action<string> PrintMethod { get; set; } = s => ServiceLocator.LogService.Log(LogType.Info, "VM : " + s);

        public ScriptVm(List<uint> bytes) {
            _bytes = bytes;
            _loadOffset = 0;
            _stdlibBytes = null;
            _stdlibBase = 0;
            uint maxLoadedPage = 0;
            _maxExecutionAddress = LoadModule(bytes, 0, _instructionPages, ref maxLoadedPage);

            _stackPointerBase = (maxLoadedPage + 1) * MemoryPage.PageSize;
            _registers = new Dictionary<uint, uint> {
                [InstructionRegister]  = bytes[0],
                [StackPointerRegister] = _stackPointerBase
            };

            _flagRegister = new Flags();
            _stack        = new Stack<uint>();

            RegisterExports(bytes, _loadOffset);
            ResolveBaseRelocations(bytes, _loadOffset);
            LockInstructionPagesIfReady(bytes);
        }

        public ScriptVm(List<uint> stdlibBytes, List<uint> scriptBytes) {
            _bytes = scriptBytes;
            _stdlibBytes = stdlibBytes;
            _stdlibBase = 0;
            _loadOffset = CalculateStdlibReservedPages() * MemoryPage.PageSize;

            uint maxLoadedPage = 0;
            uint maxExecutionAddress = 0;

            maxExecutionAddress = Math.Max(maxExecutionAddress, LoadModule(stdlibBytes, _stdlibBase, _instructionPages,
                                                                           ref maxLoadedPage));
            maxExecutionAddress = Math.Max(maxExecutionAddress, LoadModule(scriptBytes, _loadOffset, _instructionPages,
                                                                           ref maxLoadedPage));
            _maxExecutionAddress = maxExecutionAddress;

            _stackPointerBase = (maxLoadedPage + 1) * MemoryPage.PageSize;
            _registers = new Dictionary<uint, uint> {
                [InstructionRegister]  = scriptBytes[0] + _loadOffset,
                [StackPointerRegister] = _stackPointerBase
            };

            _flagRegister = new Flags();
            _stack        = new Stack<uint>();

            RegisterExports(stdlibBytes, _stdlibBase);
            RegisterExports(scriptBytes, _loadOffset);
            ResolveBaseRelocations(scriptBytes, _loadOffset);
            LockInstructionPagesIfReady(stdlibBytes, scriptBytes);
        }

        public void Execute() {
            while (_registers[InstructionRegister] >= 0 && _registers[InstructionRegister] < _maxExecutionAddress) {
                ExecuteInstruction();
            }
        }

        /// <summary>
        /// Register native functions that can be called from scripts
        /// </summary>
        public void RegisterNativeFunctions(NativeFunctionRegistry registry) {
            _nativeFunctionRegistry = registry;
            foreach (var func in registry.AllFunctions) {
                GlobalSymbolRegistry.Register(func.SymbolHash, (uint)func.Id);
            }

            if (_stdlibBytes != null) {
                ResolveRelocations(_stdlibBytes, _stdlibBase);
            }
            ResolveRelocations(_bytes, _loadOffset);
            LockInstructionPages();
        }

        public void ResolveRelocations() {
            ResolveRelocations(_bytes, _loadOffset);
            LockInstructionPages();
        }

        /// <summary>
        /// Register all symbols exported by this script into the global registry.
        /// Symbols are basically any label in the script.
        /// </summary>
        public void RegisterSymbols(Dictionary<string, int> labels, uint loadOffset) {
            // Wait, currently labels are resolved by the assembler.
            // If we want to export them, the assembler needs to include a symbol table.
            // For now, let's assume we want to export EVERYTHING that looks like a function.
        }

        private uint LoadModule(List<uint> bytes, uint baseOffset, List<MemoryPage> instructionPages,
                                ref uint maxLoadedPage) {
            uint addr = baseOffset;
            uint? codeEndAddress = null;
            uint relocationTableIndex = bytes.Count >= 3 ? bytes[2] : uint.MaxValue;
            for (uint i = 0; i < bytes.Count; i++, addr++) {
                // Special case: if we are the last byte of the static section, we do a page jump
                if (i == bytes[1]) {
                    addr += (MemoryPage.PageSize - addr % MemoryPage.PageSize) % MemoryPage.PageSize;
                }

                if (i == relocationTableIndex) {
                    codeEndAddress = addr;
                }

                uint page   = addr / MemoryPage.PageSize;
                uint offset = addr % MemoryPage.PageSize;

                if (!_memory.ContainsKey(page)) {
                    var memoryPage = new MemoryPage();
                    _memory[page] = memoryPage;

                    if (addr > baseOffset + bytes[1]) {
                        instructionPages.Add(memoryPage);
                    }
                }

                _memory[page].WriteAddress(offset, bytes[(int)i]);
                if (page > maxLoadedPage) {
                    maxLoadedPage = page;
                }
            }

            return codeEndAddress ?? addr;
        }

        private void RegisterExports(List<uint> bytes, uint baseOffset) {
            if (bytes.Count < 5) return;
            uint exportTableOffset = bytes[3];
            if (exportTableOffset >= bytes.Count) return;

            int i = (int)exportTableOffset;
            uint count = bytes[i++];
            for (uint e = 0; e < count; e++) {
                uint symbolHash = bytes[i++];
                uint address = bytes[i++];
                GlobalSymbolRegistry.Register(symbolHash, address + baseOffset);
            }
        }

        private void ResolveBaseRelocations(List<uint> bytes, uint baseOffset) {
            if (baseOffset == 0) return;
            if (bytes.Count < 5) return;
            uint baseRelocationOffset = bytes[4];
            if (baseRelocationOffset >= bytes.Count) return;

            int i = (int)baseRelocationOffset;
            uint count = bytes[i++];
            for (uint r = 0; r < count; r++) {
                uint addressToPatch = bytes[i++] + baseOffset;
                uint current = ReadMemory(addressToPatch);
                WriteMemory(addressToPatch, current + baseOffset);
            }
        }

        private void ResolveRelocations(List<uint> bytes, uint baseOffset) {
            if (bytes.Count < 3) return;
            uint relocationTableOffset = bytes[2];
            if (relocationTableOffset >= bytes.Count) return;

            int i = (int)relocationTableOffset;
            uint count = bytes[i++];
            for (uint r = 0; r < count; r++) {
                uint addressToPatch = bytes[i++] + baseOffset;
                uint symbolHash = bytes[i++];

                if (GlobalSymbolRegistry.TryGet(symbolHash, out uint resolvedAddress)) {
                    WriteMemory(addressToPatch, resolvedAddress);
                } else {
                    PrintMethod($"Warning: Could not resolve symbol {symbolHash}");
                }
            }
        }

        private void LockInstructionPagesIfReady(params List<uint>[] modules) {
            foreach (var module in modules) {
                if (HasRelocations(module)) {
                    return;
                }
            }

            LockInstructionPages();
        }

        private void LockInstructionPages() {
            if (_instructionPagesLocked) return;
            _instructionPages.ForEach(x => x.Lock());
            _instructionPagesLocked = true;
        }

        private static bool HasRelocations(List<uint> bytes) {
            if (bytes.Count < 3) return false;
            uint relocationTableOffset = bytes[2];
            if (relocationTableOffset >= bytes.Count) return false;
            return bytes[(int)relocationTableOffset] > 0;
        }

        private static uint CalculateStdlibReservedPages() {
            uint totalPages = uint.MaxValue / MemoryPage.PageSize;
            return totalPages / 5;
        }

        /// <summary>
        /// Register an entity in the VM's entity registry and return its handle ID
        /// </summary>
        public int RegisterEntity(object entity) {
            int id = _entityRegistry.Count + 1; // Start from 1, 0 is null
            while (_entityRegistry.ContainsKey(id)) {
                id++;
            }
            _entityRegistry[id] = entity;
            return id;
        }

        public object GetEntity(int entityId) {
            if (_entityRegistry.TryGetValue(entityId, out var entity)) {
                return entity;
            }
            throw new InvalidOperationException($"Entity with ID {entityId} not found in VM registry");
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
            uint  a,  b;
            float f1, f2;
            switch (ReadInstruction()) {
                case Instruction.Null:
                    break;
                case Instruction.Literal:
                    PushStack(ReadInstructionByte());
                    break;
                case Instruction.Add:
                    PushStack(PopStack() + PopStack());
                    SetFlags((int) PeekStack());
                    break;
                case Instruction.Addf:
                    PushStackf(PopStackf() + PopStackf());
                    Console.WriteLine($"Added to {PeekStackf()}");
                    SetFlags((int) PeekStackf());
                    break;
                case Instruction.Sub:
                    b = PopStack();
                    a = PopStack();
                    PushStack(a - b);
                    SetFlags((int) PeekStack());
                    break;
                case Instruction.Subf:
                    f1 = PopStackf();
                    f2 = PopStackf();
                    PushStackf(f2 - f1);
                    SetFlags(PeekStackf());
                    break;
                case Instruction.Mul:
                    PushStack(PopStack() * PopStack());
                    SetFlags((int) PeekStack());
                    break;
                case Instruction.Mulf:
                    PushStackf(PopStackf() * PopStackf());
                    SetFlags((int) PeekStackf());
                    break;
                case Instruction.Div:
                    b = PopStack();
                    a = PopStack();
                    PushStack(a / b);
                    SetFlags((int) PeekStack());
                    break;
                case Instruction.Divf:
                    f1 = PopStackf();
                    f2 = PopStackf();
                    PushStackf(f2 / f1);
                    Console.WriteLine($"Dividing {f2} / {f1} to {PeekStackf()}; {f2 / f1}");
                    SetFlags(PeekStackf());
                    break;
                case Instruction.Inc:
                    PushStack(PopStack() + 1);
                    SetFlags((int) PeekStack());
                    break;
                case Instruction.Dec:
                    PushStack(PopStack() - 1);
                    SetFlags((int) PeekStack());
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
                    SetFlags((int) (a - b));
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
                    PrintMethod(((int) PopStack()).ToString());
                    break;
                case Instruction.PrintUInt:
                    PrintMethod(PopStack().ToString());
                    break;
                case Instruction.PrintFloat:
                    PrintMethod(PopStackf().ToString(CultureInfo.CurrentCulture));
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
                case Instruction.CallNative:
                    // Read function ID from instruction stream
                    int functionId = (int)ReadInstructionByte();
                    
                    if (_nativeFunctionRegistry == null) {
                        PrintMethod($"Error: No native function registry registered");
                        break;
                    }
                    
                    var funcInfo = _nativeFunctionRegistry.GetById(functionId);
                    if (funcInfo == null) {
                        PrintMethod($"Error: Native function with ID {functionId} not found");
                        break;
                    }
                    
                    // Create context and invoke the marshalled delegate
                    var context = new ScriptNativeContext(_stack, _entityRegistry, _pendingData, WriteMemory, ReadMemory);
                    funcInfo.MarshalledDelegate(context);
                    break;
                default:
                    PrintMethod("Unexpected instruction");
                    break;
            }
        }

        private string StringFromMemory(uint userDataId) {
            // Add one as the first byte is the initial instruction register
            var    len        = ReadMemory(userDataId);
            char[] userString = new char[len];
            for (uint i = 0; i < len; i++) {
                userString[i] = Convert.ToChar(ReadMemory(userDataId + 1 + i));
            }

            return new string(userString);
        }

        private void Jump(bool doJump) {
            uint instructionPointer = PopStack(); //ReadInstructionByte();
            if (doJump) {
                _registers[InstructionRegister] = instructionPointer;
            }
        }

        private void SetFlags(float basedOn) {
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
        private Instruction ReadInstruction() {
            var ir = _registers[InstructionRegister];
            var instr = (Instruction) ReadInstructionByte();
            return instr;
        }

        private uint ReadInstructionByte() {
            var data = ReadMemory(_registers[InstructionRegister]);
            IncrementIr();
            return data;
        }

        private uint  PeekInstructionByte()  => ReadMemory(_registers[InstructionRegister]);
        private void  PushStack(uint data)   => _stack.Push(data);
        private void  PushStackf(float data) => PushStack((uint) SingleToInt32Bits(data));
        private uint  PeekStack()            => _stack.Peek();
        private float PeekStackf()           => Int32BitsToSingle((int) _stack.Peek());
        private uint  PopStack()             => _stack.Pop();
        private float PopStackf()            => Int32BitsToSingle((int) PopStack());

        private uint ReadMemory(uint sindex) {
            // Treat memory addresses as unsigned integers
            var index = sindex;
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

        public uint DebugReadMemory(uint address) {
            return ReadMemory(address);
        }

        private uint _maxMem = 0;

        private void WriteMemory(uint sindex, uint value) {
            var index = sindex;
            var (page, offset) = GetPageAndOffset(index);
            if (!_memory.ContainsKey(page)) _memory[page] = new MemoryPage();

            try {
                _memory[page].WriteAddress(offset, value);
            } catch (IllegalMemoryAccessException e) {
                throw new IllegalMemoryAccessException($"{e.Message} (Page {page}, Address {index}, r1={_registers[StackPointerRegister]})");
            }

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
            Addf,
            Sub,
            Subf,
            Mul,
            Mulf,
            Div,
            Divf,
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
            PrintUInt,
            PrintFloat,
            MemWrite,
            MemRead,
            CallNative,
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
