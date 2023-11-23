using DSharpPlus.SlashCommands;

namespace Abyss.Web.Commands.Discord;

public class PingCommand(IServiceProvider serviceProvider) : BaseCommand(serviceProvider)
{
    [SlashCommand("ping", "A simple ping test command")]
    public async Task Ping(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync("Pong!");
    }
}
