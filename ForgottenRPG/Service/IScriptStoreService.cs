using ForgottenRPG.VM;

namespace ForgottenRPG.Service {
    public interface IScriptStoreService {
        ScriptVm CreateScriptVm(int id);
    }
}
