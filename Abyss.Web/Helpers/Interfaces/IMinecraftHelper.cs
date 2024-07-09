
namespace Abyss.Web.Helpers.Interfaces;

public interface IMinecraftHelper
{
    Task<List<string>> GetPlayers(string host, ushort port = 25565);
}