using System.Collections.Generic;
using ScriptCompiler.CodeGeneration.Assembly.Instructions;

namespace ScriptCompiler.CodeGeneration {
    public class Optimiser {
        public static List<Instruction> Optimise(List<Instruction> instructions) {
            int i = 0;
            while (i < instructions.Count) {
                // Combine arithmetic
                while (i + 1 < instructions.Count) {
                    if (instructions[i] is BinaryArithmeticInstruction left) {
                        if (left.TryCombine(instructions[i + 1], out var result)) {
                            instructions[i] = result!;
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
