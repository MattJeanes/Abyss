namespace Abyss.Web.Logging.Interfaces;

public interface ITaskLogger
{
    void AddLogHandler(string name, Func<LogItem, Task> handler);
    void ClearLog();
    List<LogItem> GetLog();
    void Log(LogItemLevel logLevel, string message, Exception? ex = null);
    void LogError(string message);
    void LogError(Exception ex, string message);
    void LogInformation(string message);
    void LogWarning(string message);
    void RemoveLogHandler(string name);
}
