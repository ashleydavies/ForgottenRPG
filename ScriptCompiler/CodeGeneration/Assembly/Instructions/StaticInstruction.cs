using System;
using System.Linq;
using System.Text;

namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class StaticInstruction : Instruction {
        private readonly StaticLabel _label;
        private readonly uint[] _initial;
        
        public StaticInstruction(StaticLabel label, uint[] initial) {
            _label = label;
            _initial = initial;
        }

        protected override string AsString() {
            // Trim off the starting ! from the label as that is not used in the static instruction right now
            return $"STATIC {_label.ToString().TrimStart('!')} {string.Join("," , _initial)}";
        }
    }
}
