using Abyss.Web.Helpers.Interfaces;
using MineStatLib;

namespace Abyss.Web.Helpers;

public class MinecraftHelper() : IMinecraftHelper
{
    public async Task<List<string>> GetPlayers(string host, ushort port = 25565)
    {
        var ms = await Task.Run(() => new MineStat(host, port));
        if (ms.ServerUp)
        {
            if (ms.PlayerList != null)
            {
                return new List<string>(ms.PlayerList);
            }
            else
            {
                return [];
            }
        }
        else
        {
            throw new Exception("Unable to query server, it might be down or still starting up");
        }
    }
}
