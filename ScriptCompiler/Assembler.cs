using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShaRPG.VM;

namespace ScriptCompiler {
    internal enum AssemblyContext {
        Null,
        Data,
        Text
    }
    
    internal class Assembler {
        private int _instructionId;
        private List<string> _lines;
        private List<string> _instructions;
        private List<string> _userData;
        private Dictionary<string, int> _userDataLookup;
        private AssemblyContext _context = AssemblyContext.Null;
        private readonly Dictionary<string, string> _labels;

        public Assembler(List<string> lines) {
            _lines = lines;
            _instructionId = 0;
            _instructions = new List<string>();
            _userData = new List<string>();
            _userDataLookup = new Dictionary<string, int>();
            _labels = new Dictionary<string, string>();
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
            foreach (string line in _lines) {
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
            }
        }

        private void ProcessDataLine(string line, string[] components) {
            switch (components[0]) {
                case "string":
                    int spaceCount = 0;
                    string contents = new string(
                        line.ToCharArray().SkipWhile(x => x != ' ' || ++spaceCount < 2).ToArray()
                    );

                    contents = contents.Replace("\\n", "\n");

                    _userDataLookup[components[1]] = _userData.Count;
                    _userData.Add(contents);
                    break;
            }
        }

        private void ProcessCodeLine(string line, string[] components) {
            switch (components[0]) {
                case "label":
                    Console.WriteLine("Assigning label " + components[1] + " to instruction ID " + _instructionId);
                    _labels[components[1]] = _instructionId.ToString();
                    break;
                case "jmp":
                    AddInstruction(ScriptVM.Instruction.Jmp);
                    AddInstruction("RESOLVELABEL" + components[1]);
                    break;
                case "jeq":
                    AddInstruction(ScriptVM.Instruction.JmpEQ);
                    AddInstruction("RESOLVELABEL" + components[1]);
                    break;
                case "jneq":
                    AddInstruction(ScriptVM.Instruction.JmpNEQ);
                    AddInstruction("RESOLVELABEL" + components[1]);
                    break;
                case "jgt":
                    AddInstruction(ScriptVM.Instruction.JmpGT);
                    AddInstruction("RESOLVELABEL" + components[1]);
                    break;
                case "jgte":
                    AddInstruction(ScriptVM.Instruction.JmpGTE);
                    AddInstruction("RESOLVELABEL" + components[1]);
                    break;
                case "jlt":
                    AddInstruction(ScriptVM.Instruction.JmpLT);
                    AddInstruction("RESOLVELABEL" + components[1]);
                    break;
                case "jlte":
                    AddInstruction(ScriptVM.Instruction.JmpLTE);
                    AddInstruction("RESOLVELABEL" + components[1]);
                    break;
                case "mov":
                    HandleStackLoad(components[2]);
                    HandleStackSave(components[1]);
                    break;
                case "add":
                    ArithmeticOp(ScriptVM.Instruction.Add, components[1], components[2]);
                    break;
                case "sub":
                    ArithmeticOp(ScriptVM.Instruction.Sub, components[1], components[2]);
                    break;
                case "mul":
                    ArithmeticOp(ScriptVM.Instruction.Mul, components[1], components[2]);
                    break;
                case "div":
                    ArithmeticOp(ScriptVM.Instruction.Div, components[1], components[2]);
                    break;
                case "inc":
                    HandleStackLoad(components[1]);
                    AddInstruction(ScriptVM.Instruction.Inc);
                    HandleStackSave(components[1]);
                    break;
                case "dec":
                    HandleStackLoad(components[1]);
                    AddInstruction(ScriptVM.Instruction.Dec);
                    HandleStackSave(components[1]);
                    break;
                case "cmp":
                    HandleStackLoad(components[1]);
                    HandleStackLoad(components[2]);
                    AddInstruction(ScriptVM.Instruction.Cmp);
                    break;
                case "print":
                    HandleStackLoad(_userDataLookup[components[1]].ToString());
                    AddInstruction(ScriptVM.Instruction.Print);
                    break;
                default:
                    Console.WriteLine("Malformed instruction: " + line);
                    break;
            }
        }

        private void SwitchContext(string contextString) {
            switch (contextString) {
                case "text":
                    _context = AssemblyContext.Text;
                    break;
                case "data":
                    _context = AssemblyContext.Data;
                    break;
                default:
                    Console.WriteLine($"Unknown context type {contextString}");
                    break;
            }
        }

        private void Postprocess() {
            // As entry 0 will be the initial instruction register value
            int codeOffset = 1;
            
            List<int> userDataPointers = new List<int>();
            
            List<string> newInstructions = new List<string>();
            
            foreach (string userData in _userData) {
                userDataPointers.Add(codeOffset);
                codeOffset += 1 + userData.Length;
                newInstructions.Add(userData.Length.ToString());
                newInstructions.Add(string.Join(",", userData.ToCharArray().Select(Convert.ToInt32)));
            }
            
            // Now add lookup table to beginning. We need to skip #pointers as index will be offset by the table
            userDataPointers = userDataPointers.Select(x => x + userDataPointers.Count).ToList();
            for (int i = 0; i < userDataPointers.Count; i++) {
                codeOffset++;
                newInstructions.Insert(i, userDataPointers[i].ToString());                    
            }
            
            newInstructions.Insert(0, codeOffset.ToString());
            newInstructions.AddRange(_instructions);
            
            _instructions = newInstructions
                .Select(x => x.StartsWith("RESOLVELABEL") ? $"{int.Parse(_labels[x.Substring(12)]) + codeOffset}" : x)
                .ToList();
        }

        private void ArithmeticOp(ScriptVM.Instruction instruction, string comp1, string comp2) {
            HandleStackLoad(comp1);
            HandleStackLoad(comp2);
            AddInstruction(instruction);
            HandleStackSave(comp1);
        }

        private void HandleStackSave(string source) {
            if (!source.StartsWith("r")) {
                Console.WriteLine("Cannot push to a register; register reference should begin with r");
                return;
            }

            AddInstruction(ScriptVM.Instruction.StackToRegister);
            AddInstruction(GetRegister(source));
        }

        private void HandleStackLoad(string source) {
            if (source.StartsWith("r")) {
                AddInstruction(ScriptVM.Instruction.RegisterToStack);
                AddInstruction(GetRegister(source));
            } else {
                AddInstruction(ScriptVM.Instruction.Literal);
                AddInstruction(source);
            }
        }

        private string GetRegister(string registerString) {
            if (!registerString.StartsWith("r")) {
                Console.WriteLine("Register reference should begin with r");
                return "RFAIL";
            }

            return registerString.Substring(1);
        }

        private void AddInstruction(ScriptVM.Instruction instruction) {
            AddInstruction(ConvertInstruction(instruction));
        }

        private void AddInstruction(string instruction) {
            _instructions.Add(instruction);
            _instructionId++;
        }

        private string ConvertInstruction(ScriptVM.Instruction instruction) {
            return ((int) instruction).ToString();
        }
        
    }
}
