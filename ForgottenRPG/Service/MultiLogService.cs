using System.Collections.Generic;

namespace ForgottenRPG.Service {
    public class MultiLogService : ILogService {
        private readonly List<ILogService> _logServices;

        public MultiLogService(List<ILogService> logServices) {
            _logServices = logServices;
        }
        
        public void Log(LogType logType, string content) {
            _logServices.ForEach(ls => ls.Log(logType, content));
        }
    }
}
