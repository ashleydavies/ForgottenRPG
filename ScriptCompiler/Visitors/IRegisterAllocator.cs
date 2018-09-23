namespace ScriptCompiler.Visitors {
    public interface IRegisterAllocator {
        string GetRegister();
        void FreeRegister();
    }
}
