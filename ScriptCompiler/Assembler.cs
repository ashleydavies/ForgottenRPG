﻿using System;
using System.Collections.Generic;
using System.Linq;
using ForgottenRPG.Service;
using ForgottenRPG.VM;

namespace ScriptCompiler {
    internal enum AssemblyContext {
        Null,
        Data,
        Text
    }
    
    public class Assembler {
        private int _instructionId;
        private List<string> _lines;
        private List<string> _instructions;
        private List<string> _userData;
        // Hardcoded constant strings etc begin at index 1, as index 0 contains a pointer to the start of executable code
        private int _userDataNextIndex = 1;
        private Dictionary<string, int> _userDataIndexes;
        private AssemblyContext _context = AssemblyContext.Null;
        private readonly Dictionary<string, string> _labels;

        public Assembler(List<string> lines) {
            _lines = lines;
            _instructionId = 0;
            _instructions = new List<string>();
            _userData = new List<string>();
            _userDataIndexes = new Dictionary<string, int>();
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

                    _userDataIndexes[components[1]] = _userDataNextIndex;
                    // Bump the next index to include this string and an initial length integer which precedes all arrays
                    _userDataNextIndex += contents.Length + 1;
                    _userData.Add(contents);
                    break;
            }
        }

        private void ProcessCodeLine(string line, string[] components) {
            switch (components[0]) {
                case "label":
                    ServiceLocator.LogService.Log(LogType.Info, "Assigning label " + components[1] + " to instruction ID " + _instructionId);
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
                case "sub":
                    ArithmeticOp(ScriptVm.Instruction.Sub, components[1], components[2]);
                    break;
                case "mul":
                    ArithmeticOp(ScriptVm.Instruction.Mul, components[1], components[2]);
                    break;
                case "div":
                    ArithmeticOp(ScriptVm.Instruction.Div, components[1], components[2]);
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
                case "text":
                    _context = AssemblyContext.Text;
                    break;
                case "data":
                    _context = AssemblyContext.Data;
                    break;
                default:
                    Console.WriteLine($"Unknown context type '{contextString}'");
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
            
            newInstructions.Insert(0, codeOffset.ToString());
            newInstructions.AddRange(_instructions);
            
            _instructions = newInstructions
                .Select(x => x.StartsWith("RESOLVELABEL") ? $"{int.Parse(_labels[x.Substring(12)]) + codeOffset}" : x)
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
                AddInstruction(source);
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
