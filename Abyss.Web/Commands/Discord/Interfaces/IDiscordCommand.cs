using DSharpPlus.EventArgs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Commands.Discord.Interfaces
{
    public interface IDiscordCommand
    {
        string Command { get; }
        Task ProcessMessage(MessageCreateEventArgs e, List<string> args);
        Task MemberRemoved(GuildMemberRemoveEventArgs e);
    }
}
