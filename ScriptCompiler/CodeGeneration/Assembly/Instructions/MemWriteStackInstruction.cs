namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public class MemWriteStackInstruction : Instruction {
        protected override string AsString() {
            return "MEMWRITE_STACK";
        }
    }
}
