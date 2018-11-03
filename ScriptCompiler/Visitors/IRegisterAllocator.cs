using ScriptCompiler.CompileUtil;

namespace ScriptCompiler.Visitors {
    public interface IRegisterAllocator {
        Register GetRegister();
    }
}
