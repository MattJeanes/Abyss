using Abyss.Web.Data.GMod;

namespace Abyss.Web.Helpers.Interfaces;

public interface IGModHelper
{
    Task<string> ChangeRank(ChangeRankDTO request);
    bool IsActive();
}
