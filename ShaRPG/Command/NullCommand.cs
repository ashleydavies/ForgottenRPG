using ShaRPG.Service;

namespace ShaRPG.Command {
    public class NullCommand : ICommand {
        public void Execute(float delta) {
            ServiceLocator.LogService.Log(LogType.NullObject, "Null command executed");
        }
    }
}
