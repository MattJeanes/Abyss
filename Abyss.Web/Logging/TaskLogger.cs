using Abyss.Web.Logging.Interfaces;

namespace Abyss.Web.Logging;

public class TaskLogger(ILogger logger) : ITaskLogger
{
    private readonly ILogger _logger = logger;
    private readonly List<LogItem> _log = new List<LogItem>();
    private readonly Dictionary<string, Func<LogItem, Task>> _logHandlers = new Dictionary<string, Func<LogItem, Task>>();

    public void AddLogHandler(string name, Func<LogItem, Task> handler)
    {
        _logHandlers[name] = handler;
    }

    public void RemoveLogHandler(string name)
    {
        if (_logHandlers.ContainsKey(name))
        {
            _logHandlers.Remove(name);
        }
    }


    public List<LogItem> GetLog()
    {
        return _log;
    }

    public void ClearLog()
    {
        _log.Clear();
    }

    public void LogInformation(string message)
    {
        Log(LogItemLevel.Info, message);
    }

    public void LogWarning(string message)
    {
        Log(LogItemLevel.Warn, message);
    }

    public void LogError(string message)
    {
        Log(LogItemLevel.Error, message);
    }

    public void LogError(Exception ex, string message)
    {
        Log(LogItemLevel.Error, message, ex);
    }

    public void Log(LogItemLevel logLevel, string message, Exception ex = null)
    {
        switch (logLevel)
        {
            case LogItemLevel.Info:
                _logger.LogInformation(message);
                break;
            case LogItemLevel.Warn:
                _logger.LogWarning(message);
                break;
            case LogItemLevel.Error:
                if (ex != null)
                {
                    _logger.LogError(ex, message);
                }
                else
                {
                    _logger.LogError(message);
                }
                break;
        }
        var logItem = new LogItem(logLevel, message, ex);
        _log.Add(logItem);
        foreach (var item in _logHandlers.Values)
        {
            item(logItem);
        }
    }
}
