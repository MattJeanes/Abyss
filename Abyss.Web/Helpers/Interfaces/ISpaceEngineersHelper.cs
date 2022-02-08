using Abyss.Web.Data.SpaceEngineers;
using Abyss.Web.Entities;

namespace Abyss.Web.Helpers.Interfaces;

public interface ISpaceEngineersHelper
{
    Task<List<SpaceEngineersCharacters.Character>> GetCharacters(Server server);
}