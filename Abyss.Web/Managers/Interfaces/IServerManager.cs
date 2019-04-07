using Abyss.Web.Entities;
using Abyss.Web.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Managers.Interfaces
{
    public interface IServerManager
    {
        Task Start(string serverId, Func<LogItem, Task> logHandler = null);
        Task Stop(string serverId, Func<LogItem, Task> logHandler = null);
        Task<List<Server>> GetServers();
    }
}
