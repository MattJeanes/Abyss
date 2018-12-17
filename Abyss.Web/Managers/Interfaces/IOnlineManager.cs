using Abyss.Web.Data.TeamSpeak;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Managers.Interfaces
{
    public interface IOnlineManager
    {
        Task<List<Client>> GetClients();
        Task<List<Channel>> GetChannels();
    }
}
