using ForgottenRPG.VM;

namespace ForgottenRPG.Service {
    public interface IScriptStoreService {
        ScriptVM CreateScriptVm(int id);
    }
}
