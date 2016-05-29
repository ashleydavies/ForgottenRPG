#region

using ShaRPG.VM;

#endregion

namespace ShaRPG.Service {
    public interface IScriptStoreService {
        ScriptVM CreateScriptVm(int id);
    }
}
