namespace ShaRPG.Service {
    public enum LogType {
        NullObject,
        Warning,
        Error,
        Information
    }

    public interface ILogService {
        void Log(LogType logType, string content);
    }
}
