using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace Abyss.Web.Commands.Discord
{
    public class PingCommand : BaseCommand
    {
        public override string Command => "ping";

        public override Task ProcessMessage(MessageCreateEventArgs e, List<string> args)
        {
            return e.Message.RespondAsync("Pong!");
        }
    }
}
