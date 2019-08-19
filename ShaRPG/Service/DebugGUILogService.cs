using ShaRPG.GUI;

namespace ShaRPG.Service {
    internal class DebugGuiLogService : ILogService {
        private readonly DebugGUI _debugWindow;

        public DebugGuiLogService(DebugGUI debugWindow) {
            _debugWindow = debugWindow;
        }
        public void Log(LogType logType, string content) {
            _debugWindow.AddLogText(logType + ": " + content);
        }
    }
}
