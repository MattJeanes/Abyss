using Abyss.Web.Data;

namespace Abyss.Web.Helpers.Interfaces;

public interface IWhoSaidHelper
{
    Task<WhoSaid> WhoSaid(string message);
}
