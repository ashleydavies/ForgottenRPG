namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class MemReadStackInstruction : Instruction {
        protected override string AsString() {
            return "MEMREAD_STACK";
        }
    }
}
