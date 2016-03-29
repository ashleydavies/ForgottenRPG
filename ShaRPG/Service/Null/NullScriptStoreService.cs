using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using ShaRPG.VM;

namespace ShaRPG.Service.Null
{
    public class NullScriptStoreService : IScriptStoreService
    {
        public ScriptVM CreateScriptVm(int id)
        {
            ServiceLocator.LogService.Log(LogType.NullObject, $"Attempt to get script {id} from null store service");
            return null;
        }
    }
}