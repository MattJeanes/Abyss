using Abyss.Web.Data.TeamSpeak;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers
{
    public interface ITeamSpeakHelper
    {
        Task Update();
        Task<List<Client>> GetClients();
        Task<List<Channel>> GetChannels();
    }
}
