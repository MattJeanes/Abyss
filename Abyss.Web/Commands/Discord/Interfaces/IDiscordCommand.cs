using Abyss.Web.Data;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Abyss.Web.Commands.Discord.Interfaces;

public interface IDiscordCommand
{
    Task MemberRemoved(GuildMemberRemovedEventArgs e);
    Task<ClientUser> GetClientUser(MessageCreatedEventArgs e);
    Task<ClientUser> GetClientUser(GuildMemberRemovedEventArgs e);
    Task<ClientUser> GetClientUser(DiscordUser discordUser);
    Task<bool> HasPermission(MessageCreatedEventArgs e, string permission);
}
