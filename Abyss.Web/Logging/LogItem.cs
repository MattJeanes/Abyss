namespace Abyss.Web.Logging;

public class LogItem(LogItemLevel logLevel, string message, Exception exception = null)
{
    public LogItemLevel LogLevel { get; set; } = logLevel;
    public string Message { get; set; } = message;
    public Exception Exception { get; set; } = exception;
    public override string ToString()
    {
        return $"[{LogLevel}] {Message}{(Exception != null ? $" (Exception: {Exception.Message} - see error log for details))" : "")}";
    }
}
