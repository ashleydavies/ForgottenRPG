namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class PrintIntInstruction : Instruction {
        private readonly Location _from;
        
        public PrintIntInstruction(Location from) {
            _from = from;
        }

        protected override string AsString() {
            return $"PRINTINT {_from}";
        }
    }
}
