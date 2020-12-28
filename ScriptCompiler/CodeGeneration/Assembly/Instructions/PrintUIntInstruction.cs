namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class PrintUIntInstruction : Instruction {
        private readonly Location _from;
        
        public PrintUIntInstruction(Location from) {
            _from = from;
        }

        protected override string AsString() {
            return $"PRINTUINT {_from}";
        }
    }
}
