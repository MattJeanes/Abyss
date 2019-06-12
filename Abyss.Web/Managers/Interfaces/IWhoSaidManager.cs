using Abyss.Web.Data;
using System.Threading.Tasks;

namespace Abyss.Web.Managers.Interfaces
{
    public interface IWhoSaidManager
    {
        Task<WhoSaid> WhoSaid(string message);
    }
}
