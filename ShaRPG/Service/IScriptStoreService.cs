using ShaRPG.VM;

namespace ShaRPG.Service {
    public interface IScriptStoreService {
        ScriptVM CreateScriptVm(int id);
    }
}
