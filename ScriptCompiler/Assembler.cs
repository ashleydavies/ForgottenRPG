using System;
using System.Collections.Generic;
using System.Linq;
using ForgottenRPG.Service;
using ForgottenRPG.VM;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.CodeGeneration.Assembly.Instructions;

namespace ScriptCompiler {
    internal enum AssemblyContext {
        Null,
        Data,
        Static,
        Text
    }

    public class Assembler {
        // Hardcoded constant strings etc begin at index 2, as index 0 contains a pointer to the start of executable code
        // and index 1 contains the length of the space allocated to storing static variable values.
        private const int UserDataOffset = 2;

        private int _instructionId;
        private readonly List<string> _lines;
        private readonly List<Instruction> _instructionsInput;
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

        public Assembler(List<string> lines) {
            _lines           = lines;
            _instructionsInput = null;
            _instructionId   = 0;
            _instructions    = new List<string>();
            _userData        = new List<uint>();
            _userDataIndexes = new Dictionary<string, int>();
            _labels          = new Dictionary<string, string>();
        }

        public Assembler(List<Instruction> instructions) {
            _lines = null;
            _instructionsInput = instructions;
            _instructionId = 0;
            _instructions = new List<string>();
            _userData = new List<uint>();
            _userDataIndexes = new Dictionary<string, int>();
            _labels = new Dictionary<string, string>();
        }

        public List<string> Compile() {
            if (_instructionsInput != null) {
                ProcessInstructions();
            } else {
                Process();
            }
            Postprocess();

            return GetCompiled();
        }

        public List<string> GetCompiled() {
            return _instructions.ToList();
        }

        private void ProcessInstructions() {
            foreach (var instruction in _instructionsInput) {
                if (instruction is Section section) {
                    if (section == Section.StaticSection) SwitchContext("static");
                    else if (section == Section.DataSection) SwitchContext("data");
                    else if (section == Section.CodeSection) SwitchContext("text");
                    continue;
                }

                if (_context == AssemblyContext.Null) {
                     _context = AssemblyContext.Text;
                }

                if (_context == AssemblyContext.Static) {
                    if (instruction is StaticInstruction staticInstr) {
                        _userDataIndexes[staticInstr.Label.ToString().TrimStart('!')] = _userData.Count + UserDataOffset;
                        _userData.AddRange(staticInstr.Initial);
                    }
                } else if (_context == AssemblyContext.Data) {
                    if (instruction is StringInstruction stringInstr) {
                        string contents = stringInstr.Label.Contents.Replace("\\n", "\n");
                        _userDataIndexes[stringInstr.Label.ToString().TrimStart('!')] = _nextDataIndex;
                        _nextDataIndex += contents.Length + 1;
                        _userData.Add((uint)contents.Length);
                        _userData.AddRange(contents.ToCharArray().Select(Convert.ToUInt32));
                    }
                } else if (_context == AssemblyContext.Text) {
                    ProcessInstruction(instruction);
                }
            }
        }

        private void ProcessInstruction(Instruction instruction) {
            switch (instruction) {
                case LabelInstruction labelInstr:
                    ServiceLocator.LogService.Log(LogType.Info,
                                                  "Assigning label " + labelInstr.Label + " to instruction ID " +
                                                  _instructionId);
                    _labels[labelInstr.Label.ToString()] = _instructionId.ToString();
                    break;
                case LiteralInstruction literalInstr:
                    AddInstruction(ScriptVm.Instruction.Literal);
                    AddInstruction(literalInstr.Value.ToString());
                    break;
                case LabelLiteralInstruction labelLiteralInstr:
                    AddInstruction(ScriptVm.Instruction.Literal);
                    if (labelLiteralInstr.Label is StaticLabel || labelLiteralInstr.Label is StringLabel) {
                        AddInstruction(_userDataIndexes[labelLiteralInstr.Label.ToString().TrimStart('!')].ToString());
                    } else {
                        AddInstruction("RESOLVELABEL" + labelLiteralInstr.Label.ToString());
                    }
                    break;
                case RegisterToStackInstruction rtsInstr:
                    AddInstruction(ScriptVm.Instruction.RegisterToStack);
                    AddInstruction(rtsInstr.RegisterIndex.ToString());
                    break;
                case StackToRegisterInstruction strInstr:
                    AddInstruction(ScriptVm.Instruction.StackToRegister);
                    AddInstruction(strInstr.RegisterIndex.ToString());
                    break;
                case AddInstruction _:
                    AddInstruction(ScriptVm.Instruction.Add);
                    break;
                case AddFloatInstruction _:
                    AddInstruction(ScriptVm.Instruction.Addf);
                    break;
                case SubInstruction _:
                    AddInstruction(ScriptVm.Instruction.Sub);
                    break;
                case SubFloatInstruction _:
                    AddInstruction(ScriptVm.Instruction.Subf);
                    break;
                case MulInstruction _:
                    AddInstruction(ScriptVm.Instruction.Mul);
                    break;
                case MulFloatInstruction _:
                    AddInstruction(ScriptVm.Instruction.Mulf);
                    break;
                case DivInstruction _:
                    AddInstruction(ScriptVm.Instruction.Div);
                    break;
                case DivFloatInstruction _:
                    AddInstruction(ScriptVm.Instruction.Divf);
                    break;
                case IncInstruction _:
                    AddInstruction(ScriptVm.Instruction.Inc);
                    break;
                case DecInstruction _:
                    AddInstruction(ScriptVm.Instruction.Dec);
                    break;
                case CmpInstruction _:
                    AddInstruction(ScriptVm.Instruction.Cmp);
                    break;
                case JmpInstruction jmp:
                    HandleLoadToStack(jmp.Location);
                    AddInstruction(ScriptVm.Instruction.Jmp);
                    break;
                case JmpEqInstruction jmpEq:
                    HandleLoadToStack(jmpEq.Location);
                    AddInstruction(ScriptVm.Instruction.JmpEq);
                    break;
                case JmpNeqInstruction jmpNeq:
                    HandleLoadToStack(jmpNeq.Location);
                    AddInstruction(ScriptVm.Instruction.JmpNeq);
                    break;
                case JmpGtInstruction jmpGt:
                    HandleLoadToStack(jmpGt.Location);
                    AddInstruction(ScriptVm.Instruction.JmpGt);
                    break;
                case JmpGteInstruction jmpGte:
                    HandleLoadToStack(jmpGte.Location);
                    AddInstruction(ScriptVm.Instruction.JmpGte);
                    break;
                case JmpLtInstruction jmpLt:
                    HandleLoadToStack(jmpLt.Location);
                    AddInstruction(ScriptVm.Instruction.JmpLt);
                    break;
                case JmpLteInstruction jmpLte:
                    HandleLoadToStack(jmpLte.Location);
                    AddInstruction(ScriptVm.Instruction.JmpLte);
                    break;
                case PrintInstruction _:
                    AddInstruction(ScriptVm.Instruction.Print);
                    break;
                case PrintIntInstruction _:
                    AddInstruction(ScriptVm.Instruction.PrintInt);
                    break;
                case PrintUIntInstruction _:
                    AddInstruction(ScriptVm.Instruction.PrintUInt);
                    break;
                case PrintFloatInstruction _:
                    AddInstruction(ScriptVm.Instruction.PrintFloat);
                    break;
                case MemReadInstruction _:
                    AddInstruction(ScriptVm.Instruction.MemRead);
                    break;
                case MemWriteInstruction _:
                    AddInstruction(ScriptVm.Instruction.MemWrite);
                    break;
                case MemCopyInstruction _:
                    AddInstruction(ScriptVm.Instruction.MemRead);
                    AddInstruction(ScriptVm.Instruction.MemWrite);
                    break;
                case MovInstruction mov:
                    // Mov dest, src -> RegisterToStack src, StackToRegister dest
                    HandleLoadToStack(mov.From);
                    HandleSaveFromStack(mov.To);
                    break;
            }
        }

        private void HandleLoadToStack(Value location) {
            if (location is Register reg) {
                AddInstruction(ScriptVm.Instruction.RegisterToStack);
                AddInstruction(reg.Number.ToString());
            } else if (location is Label label) {
                AddInstruction(ScriptVm.Instruction.Literal);
                AddInstruction("RESOLVELABEL" + label.ToString());
            } else if (location is StaticLabel staticLabel) {
                AddInstruction(ScriptVm.Instruction.Literal);
                AddInstruction(_userDataIndexes[staticLabel.ToString().TrimStart('!')].ToString());
            } else if (location is StringLabel stringLabel) {
                AddInstruction(ScriptVm.Instruction.Literal);
                AddInstruction(_userDataIndexes[stringLabel.ToString().TrimStart('!')].ToString());
            } else if (location is NumericConstant literal) {
                AddInstruction(ScriptVm.Instruction.Literal);
                AddInstruction(((uint)literal.Amount).ToString());
            }
        }

        private void HandleSaveFromStack(Location location) {
            if (location is Register reg) {
                AddInstruction(ScriptVm.Instruction.StackToRegister);
                AddInstruction(reg.Number.ToString());
            } else {
                throw new CompileException("Cannot save from stack to non-register location: " + location, 0, 0);
            }
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
            newInstructions.AddRange(_instructions);

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
                AddInstruction("RESOLVELABEL" + source.Substring(1));
            } else if (source.StartsWith("!")) {
                AddInstruction(ScriptVm.Instruction.Literal);
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
