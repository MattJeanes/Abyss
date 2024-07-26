using Abyss.Web.Managers.Interfaces;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;

namespace Abyss.Web.Commands.Discord.ChoiceProviders;

public class ServerChoiceProvider : IChoiceProvider
{
    private readonly IServerManager _serverManager;

    public ServerChoiceProvider(IServerManager serverManager)
    {
        _serverManager = serverManager;
    }

    public async ValueTask<IReadOnlyDictionary<string, object>> ProvideAsync(CommandParameter parameter)
    {
        var servers = await _serverManager.GetServers();

        return servers.OrderBy(x => x.Name).ToDictionary(x => x.Name, x => (object)x.Alias);
    }
}
