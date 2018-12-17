using Abyss.Web.Data.TeamSpeak;
using Abyss.Web.Helpers;
using Abyss.Web.Managers.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Managers
{
    public class OnlineManager : IOnlineManager
    {
        private readonly ITeamSpeakHelper _teamSpeakHelper;

        public OnlineManager(ITeamSpeakHelper teamSpeakHelper)
        {
            _teamSpeakHelper = teamSpeakHelper;
        }
        public async Task<List<Client>> GetClients()
        {
            return await _teamSpeakHelper.GetClients();
        }

        public async Task<List<Channel>> GetChannels()
        {
            return await _teamSpeakHelper.GetChannels();
        }
    }
}
