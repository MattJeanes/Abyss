using Abyss.Web.Managers.Interfaces;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

namespace Abyss.Web.Commands.Discord.ChoiceProviders;

public class ServerChoiceProvider : IChoiceProvider
{
    private readonly IServerManager _serverManager;

    public ServerChoiceProvider(IServerManager serverManager)
    {
        _serverManager = serverManager;
    }

    public async ValueTask<IEnumerable<DiscordApplicationCommandOptionChoice>> ProvideAsync(CommandParameter parameter)
    {
        var servers = await _serverManager.GetServers();

        return servers.OrderBy(x => x.Name).Select(x => new DiscordApplicationCommandOptionChoice(x.Name, x.Alias));
    }
}
