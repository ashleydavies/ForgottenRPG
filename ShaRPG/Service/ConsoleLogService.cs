using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaRPG.Service {
    class ConsoleLogService : ILogService {
        public void Log(LogType logType, string content)
        {
            Console.WriteLine(logType.ToString() + ": " + content);
        }
    }
}
