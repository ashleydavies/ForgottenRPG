using System;
using System.Linq;
using System.Text;

namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class StaticInstruction : Instruction {
        public StaticLabel Label { get; }
        public uint[] Initial { get; }
        
        public StaticInstruction(StaticLabel label, uint[] initial) {
            Label = label;
            Initial = initial;
        }

        protected override string AsString() {
            // Trim off the starting ! from the label as that is not used in the static instruction right now
            return $"STATIC {Label.ToString().TrimStart('!')} {string.Join("," , Initial)}";
        }
    }
}
