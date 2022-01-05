using Abyss.Web.Data;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Commands.Discord.Interfaces
{
    public interface IDiscordCommand
    {
        string Command { get; }
        string Permission { get; }
        Task ProcessMessage(MessageCreateEventArgs e, List<string> args);
        Task MemberRemoved(GuildMemberRemoveEventArgs e);
        Task<ClientUser> GetClientUser(MessageCreateEventArgs e);
        Task<ClientUser> GetClientUser(GuildMemberRemoveEventArgs e);
        Task<ClientUser> GetClientUser(DiscordUser discordUser);
        Task<bool> HasPermission(MessageCreateEventArgs e, string permission);
    }
}
