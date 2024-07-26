using DSharpPlus.Commands;
using System.ComponentModel;

namespace Abyss.Web.Commands.Discord;

public class PingCommand(IServiceProvider serviceProvider) : BaseCommand(serviceProvider)
{
    [Command("ping"), Description("A simple ping test command")]
    public async Task Ping(CommandContext ctx)
    {
        await ctx.RespondAsync("Pong!");
    }
}
