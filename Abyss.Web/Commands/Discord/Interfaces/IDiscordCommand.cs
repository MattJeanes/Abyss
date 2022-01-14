using Abyss.Web.Data;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Abyss.Web.Commands.Discord.Interfaces;

public interface IDiscordCommand
{
    Task MemberRemoved(GuildMemberRemoveEventArgs e);
    Task<ClientUser?> GetClientUser(MessageCreateEventArgs e);
    Task<ClientUser?> GetClientUser(GuildMemberRemoveEventArgs e);
    Task<ClientUser?> GetClientUser(DiscordUser discordUser);
    Task<bool> HasPermission(MessageCreateEventArgs e, string permission);
}
