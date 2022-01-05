﻿using DSharpPlus.EventArgs;

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
}
