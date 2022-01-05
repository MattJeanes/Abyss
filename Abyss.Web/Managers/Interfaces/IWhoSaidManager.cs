using Abyss.Web.Data;

namespace Abyss.Web.Managers.Interfaces;

public interface IWhoSaidManager
{
    Task<WhoSaid> WhoSaid(string message);
}
