using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ForgottenRPG.Service;
using ForgottenRPG.VM;

namespace ScriptCompiler {
    internal enum AssemblyContext {
        Null,
        Data,
        Static,
        Text
    }

    public class Assembler {
        // Hardcoded constant strings etc begin at index 5, as index 0 contains a pointer to the start of executable code,
        // index 1 contains the length of the space allocated to storing static variable values, index 2 is relocation
        // table offset, index 3 is export table offset, and index 4 is base relocation table offset.
        private const int UserDataOffset = 5;

        private int _instructionId;
        private readonly List<string> _lines;
        private List<string> _instructions;
        private readonly List<uint> _userData;
        // The start position of the next user data (used since we need to e.g. skip remainder of a page after statics)
        private int _nextDataIndex;
        // Required to calculate where the page break goes
        private int _staticLength;

        private AssemblyContext _context = AssemblyContext.Null;

        // Technically static variables are a subset of user data for the purposes of assembling so we use the same map
        private readonly Dictionary<string, int> _userDataIndexes;
        private readonly Dictionary<string, string> _labels;
        private readonly List<(int position, uint symbolHash)> _relocations = new();
        private readonly List<(string label, uint symbolHash)> _exports = new();
        private readonly List<int> _baseRelocations = new();

        public Assembler(List<string> lines) {
            _lines           = lines;
            _instructionId   = 0;
            _instructions    = new List<string>();
            _userData        = new List<uint>();
            _userDataIndexes = new Dictionary<string, int>();
            _labels          = new Dictionary<string, string>();
        }

        public List<string> Compile() {
            Process();
            Postprocess();

            return GetCompiled();
        }

        public List<string> GetCompiled() {
            return _instructions.ToList();
        }

        private void Process() {
            // Remove comments
            for (int i = 0; i < _lines.Count; i++) {
                if (_lines[i].Contains('#')) {
                    _lines[i] = _lines[i].Substring(0, _lines[i].IndexOf('#')).Trim();
                }
            }

            foreach (string line in _lines) {
                if (line.Length == 0) continue;
                string[] components = line.Split(' ').Select(x => x.ToLower()).ToArray();

                if (components.Length == 0) {
                    Console.WriteLine("Invalid line detected. Terminating processing.");
                    return;
                }

                if (components[0].StartsWith(".")) {
                    SwitchContext(components[0].Substring(1));
                    continue;
                }

                if (_context == AssemblyContext.Null) {
                    Console.WriteLine($"No context specified in assembly - assuming .text");
                    _context = AssemblyContext.Text;
                }

                if (_context == AssemblyContext.Text) ProcessCodeLine(line, components);
                else if (_context == AssemblyContext.Data) ProcessDataLine(line, components);
                else if (_context == AssemblyContext.Static) ProcessStaticLine(line, components);
            }
        }

        private void ProcessDataLine(string line, string[] components) {
            switch (components[0]) {
                case "string":
                    // Skip the STRING ____ to get the actual string
                    int spaceCount = 0;
                    string contents = new string(
                        line.ToCharArray().SkipWhile(x => {
                            if (spaceCount >= 2) return false;
                            if (x == ' ') spaceCount++;
                            return true;
                        }).ToArray()
                    );

                    contents = contents.Replace("\\n", "\n");

                    _userDataIndexes[components[1]] =  _nextDataIndex;
                    _nextDataIndex                  += contents.Length + 1;
                    _userData.Add((uint)contents.Length);
                    _userData.AddRange(contents.ToCharArray().Select(Convert.ToUInt32));
                    break;
            }
        }

        private void ProcessStaticLine(string line, string[] components) {
            switch (components[0]) {
                case "static":
                    // The value is stored as a comma-separated int array
                    // e.g. STATIC $stc_... 0,0,0,0 => $stc_... = 0,0,0,0
                    List<uint> initialValue = components[2].Split(",").Select(uint.Parse).ToList();
                    _userDataIndexes[components[1]] = _userData.Count + UserDataOffset;
                    _userData.AddRange(initialValue);
                    break;
            }
        }

        private void ProcessCodeLine(string line, string[] components) {
            if (TryProcessDirectInstruction(components)) {
                return;
            }

            switch (components[0]) {
                case "label":
                    ServiceLocator.LogService.Log(LogType.Info,
                                                  "Assigning label " + components[1] + " to instruction ID " +
                                                  _instructionId);
                    _labels[components[1]] = _instructionId.ToString();
                    break;
                case "jmp":
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.Jmp);
                    break;
                case "jmp_extern":
                    // A jmp_extern is basically PUSH 0 (placeholder), JMP
                    // We need to record a relocation for the 0
                    AddInstruction(ScriptVm.Instruction.Literal);
                    _relocations.Add((_instructionId, uint.Parse(components[1])));
                    AddInstruction("0");
                    AddInstruction(ScriptVm.Instruction.Jmp);
                    break;
                case "jeq":
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.JmpEq);
                    break;
                case "jneq":
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.JmpNeq);
                    break;
                case "jgt":
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.JmpGt);
                    break;
                case "jgte":
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.JmpGte);
                    break;
                case "jlt":
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.JmpLt);
                    break;
                case "jlte":
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.JmpLte);
                    break;
                case "mov":
                    HandleLoadToStack(components[2]);
                    HandleSaveFromStack(components[1]);
                    break;
                case "push":
                    HandleLoadToStack(components[1]);
                    break;
                case "pop":
                    HandleSaveFromStack(components[1]);
                    break;
                case "add":
                    ArithmeticOp(ScriptVm.Instruction.Add, components[1], components[2]);
                    break;
                case "addf":
                    ArithmeticOp(ScriptVm.Instruction.Addf, components[1], components[2]);
                    break;
                case "sub":
                    ArithmeticOp(ScriptVm.Instruction.Sub, components[1], components[2]);
                    break;
                case "subf":
                    ArithmeticOp(ScriptVm.Instruction.Subf, components[1], components[2]);
                    break;
                case "mul":
                    ArithmeticOp(ScriptVm.Instruction.Mul, components[1], components[2]);
                    break;
                case "mulf":
                    ArithmeticOp(ScriptVm.Instruction.Mulf, components[1], components[2]);
                    break;
                case "div":
                    ArithmeticOp(ScriptVm.Instruction.Div, components[1], components[2]);
                    break;
                case "divf":
                    ArithmeticOp(ScriptVm.Instruction.Divf, components[1], components[2]);
                    break;
                case "inc":
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.Inc);
                    HandleSaveFromStack(components[1]);
                    break;
                case "dec":
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.Dec);
                    HandleSaveFromStack(components[1]);
                    break;
                case "cmp":
                    // flip operands so that cmp(a, b) for, e.g., GT, is processed as a > b not b > a
                    HandleLoadToStack(components[2]);
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.Cmp);
                    break;
                case "print":
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.Print);
                    break;
                case "printint":
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.PrintInt);
                    break;
                case "printuint":
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.PrintUInt);
                    break;
                case "printfloat":
                    HandleLoadToStack(components[1]);
                    AddInstruction(ScriptVm.Instruction.PrintFloat);
                    break;
                case "memwrite":
                    HandleLoadToStack(components[1]);
                    HandleLoadToStack(components[2]);
                    AddInstruction(ScriptVm.Instruction.MemWrite);
                    break;
                case "memread":
                    HandleLoadToStack(components[2]);
                    AddInstruction(ScriptVm.Instruction.MemRead);
                    HandleSaveFromStack(components[1]);
                    break;
                case "memcopy":
                    HandleLoadToStack(components[1]);
                    HandleLoadToStack(components[2]);
                    AddInstruction(ScriptVm.Instruction.MemRead);
                    AddInstruction(ScriptVm.Instruction.MemWrite);
                    break;
                case "callnative":
                    AddInstruction(ScriptVm.Instruction.CallNative);
                    AddInstruction(components[1]);
                    break;
                case "call_native_extern":
                    AddInstruction(ScriptVm.Instruction.CallNative);
                    _relocations.Add((_instructionId, uint.Parse(components[1])));
                    AddInstruction("0");
                    break;
                case "export":
                    _exports.Add((components[1], uint.Parse(components[2])));
                    break;
                case "memread_stack":
                    // Expects address on stack. Pushes value to stack.
                    AddInstruction(ScriptVm.Instruction.MemRead);
                    break;
                case "memwrite_stack":
                    // Expects [pos, val] on stack.
                    AddInstruction(ScriptVm.Instruction.MemWrite);
                    break;
            }
        }

        private bool TryProcessDirectInstruction(string[] components) {
            string op = components[0];
            if (components.Length == 1) {
                if (TryGetDirectInstruction(op, out var instruction)) {
                    AddInstruction(instruction);
                    return true;
                }
                return false;
            }

            if (components.Length == 2 && op == "literal") {
                AddInstruction(ScriptVm.Instruction.Literal);
                AddInstruction(components[1]);
                return true;
            }

            return false;
        }

        private static bool TryGetDirectInstruction(string op, out ScriptVm.Instruction instruction) {
            switch (op) {
                case "null":
                    instruction = ScriptVm.Instruction.Null;
                    return true;
                case "add":
                    instruction = ScriptVm.Instruction.Add;
                    return true;
                case "addf":
                    instruction = ScriptVm.Instruction.Addf;
                    return true;
                case "sub":
                    instruction = ScriptVm.Instruction.Sub;
                    return true;
                case "subf":
                    instruction = ScriptVm.Instruction.Subf;
                    return true;
                case "mul":
                    instruction = ScriptVm.Instruction.Mul;
                    return true;
                case "mulf":
                    instruction = ScriptVm.Instruction.Mulf;
                    return true;
                case "div":
                    instruction = ScriptVm.Instruction.Div;
                    return true;
                case "divf":
                    instruction = ScriptVm.Instruction.Divf;
                    return true;
                case "inc":
                    instruction = ScriptVm.Instruction.Inc;
                    return true;
                case "dec":
                    instruction = ScriptVm.Instruction.Dec;
                    return true;
                case "cmp":
                    instruction = ScriptVm.Instruction.Cmp;
                    return true;
                case "jmp":
                    instruction = ScriptVm.Instruction.Jmp;
                    return true;
                case "print":
                    instruction = ScriptVm.Instruction.Print;
                    return true;
                case "printint":
                    instruction = ScriptVm.Instruction.PrintInt;
                    return true;
                case "printuint":
                    instruction = ScriptVm.Instruction.PrintUInt;
                    return true;
                case "printfloat":
                    instruction = ScriptVm.Instruction.PrintFloat;
                    return true;
                case "memwrite_stack":
                    instruction = ScriptVm.Instruction.MemWrite;
                    return true;
                case "memread_stack":
                    instruction = ScriptVm.Instruction.MemRead;
                    return true;
                case "callnative":
                    instruction = ScriptVm.Instruction.CallNative;
                    return true;
                default:
                    instruction = ScriptVm.Instruction.Null;
                    return false;
            }
        }

        private void SwitchContext(string contextString) {
            switch (contextString) {
                case "static":
                    _context = AssemblyContext.Static;
                    break;
                case "data":
                    // We should have just finished the static section
                    _staticLength = _userData.Count;
                    int staticPages  = (int)Math.Ceiling((_staticLength + UserDataOffset) / (double)MemoryPage.PageSize);
                    // Start the data at the beginning of the next page (since we want it to be immutable)
                    _nextDataIndex = staticPages * (int)MemoryPage.PageSize;
                    _context      = AssemblyContext.Data;
                    break;
                case "text":
                    _context = AssemblyContext.Text;
                    break;
                default:
                    Console.WriteLine($"Unknown context type '{contextString}'");
                    break;
            }
        }

        private void Postprocess() {
            // As entry 0 will be the initial instruction register value
            int codeOffset = _nextDataIndex;

            List<string> newInstructions = new List<string>();
            newInstructions.AddRange(_userData.Select(x => x.ToString()));

            /*
             * The thing left to fix is that anything referring to memory addresses after the 'break' after static
             * variables are left referring to a position that is not taking into account that the break is inserted.
             * We need to do some "virtual virtual" address translation prior to output so that we can account for
             * issues like this.
             */
            
            newInstructions.Insert(0, codeOffset.ToString());
            // This points to the end of the static section, at which point a "page break" needs inserting by the VM
            newInstructions.Insert(1, (_staticLength + UserDataOffset).ToString());

            int relocationTableOffset = UserDataOffset + _userData.Count + _instructions.Count;
            int relocationTableLength = 1 + _relocations.Count * 2;
            int exportTableOffset = relocationTableOffset + relocationTableLength;
            int exportTableLength = 1 + _exports.Count * 2;
            int baseRelocationTableOffset = exportTableOffset + exportTableLength;
            newInstructions.Insert(2, relocationTableOffset.ToString());
            newInstructions.Insert(3, exportTableOffset.ToString());
            newInstructions.Insert(4, baseRelocationTableOffset.ToString());

            newInstructions.AddRange(_instructions);

            // Append relocation table
            newInstructions.Add(_relocations.Count.ToString());
            foreach (var (pos, symbol) in _relocations) {
                newInstructions.Add((pos + codeOffset).ToString());
                newInstructions.Add(symbol.ToString());
            }

            // Append export table
            newInstructions.Add(_exports.Count.ToString());
            foreach (var (label, symbolHash) in _exports) {
                newInstructions.Add(symbolHash.ToString());
                newInstructions.Add((int.Parse(_labels[label]) + codeOffset).ToString());
            }

            // Append base relocation table
            newInstructions.Add(_baseRelocations.Count.ToString());
            foreach (var pos in _baseRelocations) {
                newInstructions.Add((pos + codeOffset).ToString());
            }

            _instructions = newInstructions
                            .Select(x => x.StartsWith("RESOLVELABEL")
                                        ? $"{int.Parse(_labels[x.Substring(12)]) + codeOffset}"
                                        : x)
                            .ToList();
        }

        private void ArithmeticOp(ScriptVm.Instruction instruction, string comp1, string comp2) {
            HandleLoadToStack(comp1);
            HandleLoadToStack(comp2);
            AddInstruction(instruction);
            HandleSaveFromStack(comp1);
        }

        private void HandleSaveFromStack(string source) {
            if (!source.StartsWith("r")) {
                Console.WriteLine("Cannot push to a non-register; register reference should begin with r");
                return;
            }

            AddInstruction(ScriptVm.Instruction.StackToRegister);
            AddInstruction(GetRegister(source));
        }

        private void HandleLoadToStack(string source) {
            if (source.StartsWith("r")) {
                AddInstruction(ScriptVm.Instruction.RegisterToStack);
                AddInstruction(GetRegister(source));
            } else if (source.StartsWith("$")) {
                AddInstruction(ScriptVm.Instruction.Literal);
                _baseRelocations.Add(_instructionId);
                AddInstruction("RESOLVELABEL" + source.Substring(1));
            } else if (source.StartsWith("!")) {
                AddInstruction(ScriptVm.Instruction.Literal);
                _baseRelocations.Add(_instructionId);
                AddInstruction(_userDataIndexes[source.Substring(1)].ToString());
            } else {
                AddInstruction(ScriptVm.Instruction.Literal);
                AddInstruction(source.StartsWith('-')
                                   ? ((uint) int.Parse(source)).ToString()
                                   : uint.Parse(source).ToString());
            }
        }

        private string GetRegister(string registerString) {
            if (!registerString.StartsWith("r")) {
                Console.WriteLine("Register reference should begin with r");
                return "RFAIL";
            }

            // TODO lint to make sure it's a valid number
            return registerString.Substring(1);
        }

        private void AddInstruction(ScriptVm.Instruction instruction) {
            AddInstruction(ConvertInstruction(instruction));
        }

        private void AddInstruction(string instruction) {
            if (instruction == "") throw new CompileException("Assembler failed critically", 0, 0);
            _instructions.Add(instruction);
            _instructionId++;
        }

        private string ConvertInstruction(ScriptVm.Instruction instruction) {
            return ((int) instruction).ToString();
        }
    }
}
