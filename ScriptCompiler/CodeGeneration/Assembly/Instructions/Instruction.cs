using ForgottenRPG.VM;

namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public abstract class Instruction {
        private string? _comment;

        public Instruction WithComment(string comment) {
            _comment = comment;
            return this;
        }
        
        public sealed override string ToString() {
            if (_comment == null) return AsString();
            return AsString() + " # " + _comment;
        }

        protected abstract string AsString();
    }
}
