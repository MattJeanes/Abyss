using Abyss.Web.Commands.Discord.Interfaces;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Commands.Discord
{
    public class HelpCommand : IDiscordCommand
    {
        public string Command => "help";

        public async Task ProcessMessage(MessageCreateEventArgs e, List<string> args)
        {
            await e.Message.RespondAsync("Mate you want some help");
        }
    }
}
