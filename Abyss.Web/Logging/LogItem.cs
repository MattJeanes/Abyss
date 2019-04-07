using System;

namespace Abyss.Web.Logging
{
    public class LogItem
    {
        public LogItem(LogItemLevel logLevel, string message, Exception exception = null)
        {
            LogLevel = logLevel;
            Message = message;
        }
        public LogItemLevel LogLevel { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public override string ToString()
        {
            return $"[{LogLevel}] {Message}{(Exception != null ? $" (Exception: {Exception.Message} - see error log for details))" : "")}";
        }
    }
}