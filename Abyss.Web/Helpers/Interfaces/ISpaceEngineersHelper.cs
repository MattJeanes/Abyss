using Abyss.Web.Data.SpaceEngineers;

namespace Abyss.Web.Helpers.Interfaces;

public interface ISpaceEngineersHelper
{
    Task<List<SpaceEngineersCharacters.Character>> GetCharacters();
}