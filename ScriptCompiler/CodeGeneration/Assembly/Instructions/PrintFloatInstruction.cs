namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class PrintFloatInstruction : Instruction {
        private readonly Location _from;
        
        public PrintFloatInstruction(Location from) {
            _from = from;
        }

        protected override string AsString() {
            return $"PRINTFLOAT {_from}";
        }
    }
}
