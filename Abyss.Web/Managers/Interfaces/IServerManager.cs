using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Logging;

namespace Abyss.Web.Managers.Interfaces;

public interface IServerManager
{
    Task Start(int serverId, Func<LogItem, Task> logHandler = null, Func<ServerStatus, Task> statusHandler = null);
    Task Stop(int serverId, Func<LogItem, Task> logHandler = null, Func<ServerStatus, Task> statusHandler = null);
    Task Restart(int serverId, Func<LogItem, Task> logHandler = null);
    Task<List<Server>> GetServers();
    Task<Server> GetServerByAlias(string alias);
    Task<ServerRichStatus> GetServerRichStatus(Server server);
    Task<string> ExecuteCommand(int serverId, string command);
}
