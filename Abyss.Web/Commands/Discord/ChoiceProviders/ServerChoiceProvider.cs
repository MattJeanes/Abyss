using Abyss.Web.Managers.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Abyss.Web.Commands.Discord.ChoiceProviders;

public class ServerChoiceProvider : ChoiceProvider
{
    public override async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
    {
        using var scope = Services.CreateScope();
        var serverManager = scope.ServiceProvider.GetRequiredService<IServerManager>();
        var servers = await serverManager.GetServers();

        return servers.OrderBy(x => x.Name).Select(x => new DiscordApplicationCommandOptionChoice(x.Name, x.Alias));
    }
}
