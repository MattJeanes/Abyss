using Abyss.Web.Entities;
using Abyss.Web.Logging;

namespace Abyss.Web.Managers.Interfaces;

public interface IServerManager
{
    Task Start(string serverId, Func<LogItem, Task>? logHandler = null);
    Task Stop(string serverId, Func<LogItem, Task>? logHandler = null);
    Task Restart(string serverId, Func<LogItem, Task>? logHandler = null);
    Task<List<Server>> GetServers();
    Task<Server> GetServerByAlias(string alias);
}
