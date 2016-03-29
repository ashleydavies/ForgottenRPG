using ShaRPG.Service;

namespace ShaRPG.Command
{
    public class NullCommand : ICommand
    {
        public void Execute()
        {
            ServiceLocator.LogService.Log(LogType.NullObject, "Null command executed");
        }
    }
}