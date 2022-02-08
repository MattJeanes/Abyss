using Abyss.Web.Data.TeamSpeak;

namespace Abyss.Web.Helpers.Interfaces;

public interface ITeamSpeakHelper
{
    Task Update();
    Task<List<Client>> GetClients();
    Task<List<Channel>> GetChannels();
}
