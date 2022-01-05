using Abyss.Web.Data.TeamSpeak;

namespace Abyss.Web.Managers.Interfaces;

public interface IOnlineManager
{
    Task<List<Client>> GetClients();
    Task<List<Channel>> GetChannels();
}
