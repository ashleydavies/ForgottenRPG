using ForgottenRPG.GUI;

namespace ForgottenRPG.Service {
    internal class DebugGuiLogService : ILogService {
        private readonly DebugGui _debugWindow;

        public DebugGuiLogService(DebugGui debugWindow) {
            _debugWindow = debugWindow;
        }
        public void Log(LogType logType, string content) {
            _debugWindow.AddLogText(logType + ": " + content);
        }
    }
}
