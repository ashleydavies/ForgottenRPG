namespace ShaRPG.Service {
    public enum LogType {
        NullObject,
        Warning,
        Error,
        Info
    }

    public interface ILogService {
        void Log(LogType logType, string content);
    }
}
