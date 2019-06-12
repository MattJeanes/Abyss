using Abyss.Web.Data;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers.Interfaces
{
    public interface IWhoSaidHelper
    {
        Task<WhoSaid> WhoSaid(string message);
    }
}