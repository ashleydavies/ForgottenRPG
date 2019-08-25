using System;

namespace ForgottenRPG.Service.Null {
    public class NullLogService : ILogService {
        public void Log(LogType logType, string content) {
            LogConsole(logType, content);
            LogConsole(LogType.Error, "Logging to NullLogService");
        }

        private void LogConsole(LogType logType, string content) {
            Console.WriteLine(logType + ": " + content);
        }
    }
}
