using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Networking
{
    public class MyConsole
    {
        public enum LogType
        {
            Error,
            Debug
        };

        /// <summary>
        /// 格式化的日志输出，特别是日期
        /// </summary>
        /// <param name="message"></param>
        /// <param name="LoggerName"></param>
        /// <param name="logType"></param>
        public static void Log(string message,/*string LoggerName,*/ LogType logType)
        {
            string log = "";
            log += DateTime.Now.ToString() + " : ";
            log += Thread.CurrentThread.Name + " : ";
            switch (logType)
            {
                case LogType.Debug:
                    log += "DEBUG : ";
                    break;
                case LogType.Error:
                    log += "ERROR : ";
                    break;
                default:
                    break;
            }
            log += message;
            Console.WriteLine(log);
        }

    }
}
