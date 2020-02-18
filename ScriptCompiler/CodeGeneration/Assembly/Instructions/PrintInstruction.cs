namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class PrintInstruction : Instruction {
        private readonly Location _from;
        
        public PrintInstruction(Location from) {
            _from = from;
        }

        protected override string AsString() {
            return $"PRINT {_from}";
        }
    }
}
