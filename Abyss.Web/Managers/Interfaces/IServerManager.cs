using Abyss.Web.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Managers.Interfaces
{
    public interface IServerManager
    {
        Task Start(string serverId);
        Task Stop(string serverId);
        Task<List<Server>> GetServers();
    }
}
