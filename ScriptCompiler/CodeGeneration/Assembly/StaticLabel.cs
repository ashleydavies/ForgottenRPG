using System;

namespace ScriptCompiler.CodeGeneration.Assembly {
    // There are some code sharing opportunities between this and StringLabel for sure
    public class StaticLabel : Location {
        private readonly Label _label;
        
        public StaticLabel(Guid guid) {
            _label = new Label($"stc_{guid}");
        }

        public override string ToString() {
            return $"!{_label}";
        }
    }
}
