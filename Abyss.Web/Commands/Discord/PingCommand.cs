using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace Abyss.Web.Commands.Discord;

public class PingCommand : BaseCommand
{
    public override string Command => "ping";

    public PingCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {

    }

    public override Task ProcessMessage(MessageCreateEventArgs e, List<string> args)
    {
        return e.Message.RespondAsync("Pong!");
    }

    [SlashCommand("ping", "A simple ping test command")]
    public async Task RunCommand(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Pong!"));
    }
}
