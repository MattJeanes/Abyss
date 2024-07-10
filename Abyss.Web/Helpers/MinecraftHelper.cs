using Abyss.Web.Helpers.Interfaces;
using MineStatLib;

namespace Abyss.Web.Helpers;

public class MinecraftHelper() : IMinecraftHelper
{
    public async Task<List<string>> GetPlayers(string host, ushort port = 25565)
    {
        var ms = await Task.Run(() => new MineStat(host, port));
        if (ms.ServerUp && ms.PlayerList != null)
        {
            return new List<string>(ms.PlayerList);
        }
        else
        {
            return [];
        }
    }
}
