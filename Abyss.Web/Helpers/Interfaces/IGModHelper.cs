using Abyss.Web.Data.GMod;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers.Interfaces
{
    public interface IGModHelper
    {
        Task<string> ChangeRank(ChangeRankDTO request);
        bool IsActive();
    }
}
