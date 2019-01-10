/*
using Abyss.Web.Data.Options;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Commands.Discord
{
    public class AddonsCommand : BaseCommand
    {
        public override string Command => "addons";

        private readonly DiscordOptions _discordOptions;

        public AddonsCommand(IOptions<DiscordOptions> discordOptions)
        {
            _discordOptions = discordOptions.Value;
        }

        public override async Task ProcessMessage(MessageCreateEventArgs e, List<string> args)
        {
            await e.Message.RespondAsync(_discordOptions.AddonsMessage);
        }
    }
}
*/