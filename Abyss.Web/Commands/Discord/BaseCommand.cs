using System.Collections.Generic;
using System.Threading.Tasks;
using Abyss.Web.Commands.Discord.Interfaces;
using DSharpPlus.EventArgs;

namespace Abyss.Web.Commands.Discord
{
    public class BaseCommand : IDiscordCommand
    {
        public virtual string Command => null;

        public virtual Task MemberRemoved(GuildMemberRemoveEventArgs e)
        {
            return Task.CompletedTask;
        }

        public virtual Task ProcessMessage(MessageCreateEventArgs e, List<string> args)
        {
            return Task.CompletedTask;
        }
    }
}