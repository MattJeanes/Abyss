using DSharpPlus.SlashCommands;

namespace Abyss.Web.Commands.Discord;

public class PingCommand : BaseCommand
{
    public PingCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {

    }

    [SlashCommand("ping", "A simple ping test command")]
    public async Task Ping(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync("Pong!");
    }
}
