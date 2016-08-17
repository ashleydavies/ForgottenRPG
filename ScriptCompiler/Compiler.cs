using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShaRPG.VM;

namespace ScriptCompiler {
    class Compiler {
        private int _instructionID;
        private List<string> _lines;
        private List<string> _instructions;
        private Dictionary<string, string> _labels;

        public Compiler(List<string> lines) {
            this._lines = lines;
            this._instructionID = 0;
            this._instructions = new List<string>();
            this._labels = new Dictionary<string, string>();
        }

        public void Compile() {
            Preprocess();
            Process();
            Postprocess();
        }

        public List<string> GetCompiled() {
            return _instructions.ToList();
        }

        private void Preprocess() {
            _lines = _lines.Select(x => x.Trim().ToLower()).Where(x => x != "" && !x.StartsWith("#")).ToList();
        }
        
        private void Process() {
            foreach (string line in _lines) {
                string[] components = line.Split(' ');

                if (components.Length == 0) {
                    Console.WriteLine("Invalid line detected. Terminating processing.");
                    return;
                }

                switch (components[0]) {
                    case "label":
                        Console.WriteLine("Assigning label " + components[1] + " to instruction ID " + _instructionID);
                        _labels[components[1]] = _instructionID.ToString();
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
                    default:
                        Console.WriteLine("Malformed instruction: " + line);
                        break;
                }
            }
        }

        private void Postprocess() {
            _instructions = _instructions.Select(x => x.StartsWith("RESOLVELABEL") ? _labels[x.Substring(12)] + " [label " + x.Substring(12) + "]" : x).ToList();
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
            AddInstruction(ConvertInstruction(instruction) + " [" + instruction.ToString() + "]");
        }

        private void AddInstruction(string instruction) {
            _instructions.Add(instruction);
            _instructionID++;
        }

        private string ConvertInstruction(ScriptVM.Instruction instruction) {
            return ((int) instruction).ToString();
        }
        
        static void Main(string[] args) {
            Console.WriteLine("Enter the path to the file to compile.");

            List<string> lines = File.ReadLines(Console.ReadLine() + ".shascr").ToList();

            Compiler compiler = new Compiler(lines);
            compiler.Compile();
            File.WriteAllLines("compiled.shabyte", compiler.GetCompiled());
            Console.WriteLine("Completed output:");
            foreach (string line in compiler.GetCompiled()) {
                Console.WriteLine(line);
            }
            Console.ReadLine();
        }
    }
}
