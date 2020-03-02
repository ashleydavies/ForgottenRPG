namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class CmpInstruction : Instruction {
        private readonly Value _left;
        private readonly Value _right;

        public CmpInstruction(Value left, Value right) {
            _left = left;
            _right = right;
        } 

        protected override string AsString() {
            return $"CMP {_left} {_right}";
        }
    }
}
