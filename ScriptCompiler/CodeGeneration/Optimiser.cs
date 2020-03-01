using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.CodeGeneration.Assembly.Instructions;

namespace ScriptCompiler.CodeGeneration {
    public class Optimiser {
        public static List<Instruction> Optimise(List<Instruction> instructions) {
            return instructions;
            // Combine arithmetic
            int i = 0;
            while (i < instructions.Count) {
                while (i + 1 < instructions.Count) {
                    if (instructions[i] is BinaryArithmeticInstruction left) {
                        if (left.TryCombine(instructions[i + 1], out var result)) {
                            Console.WriteLine($"Combining {instructions[i]} with {instructions[i + 1]}");
                            instructions[i] = result;
                            Console.WriteLine($"Combined to {instructions[i]}");
                            instructions.RemoveAt(i + 1);
                            continue;
                        }
                    }

                    break;
                }

                if (instructions[i] is BinaryArithmeticInstruction num && num.IsNoop()) {
                    instructions.RemoveAt(i);
                    continue;
                }
                i++;
            }

            return instructions;
        }
    }
}
