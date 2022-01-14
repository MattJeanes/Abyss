using Abyss.Web.Commands.Discord.Interfaces;
using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Helpers;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Options;

namespace Abyss.Web.Commands.Discord;

public class BaseCommand : ApplicationCommandModule, IDiscordCommand
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IUserHelper _userHelper;
    protected readonly IUserRepository _userRepository;
    protected readonly DiscordOptions _baseOptions;

    public BaseCommand(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _userHelper = _serviceProvider.GetRequiredService<IUserHelper>();
        _userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
        _baseOptions = _serviceProvider.GetRequiredService<IOptions<DiscordOptions>>().Value;
    }

    public virtual Task MemberRemoved(GuildMemberRemoveEventArgs e)
    {
        return Task.CompletedTask;
    }

    public async Task<ClientUser?> GetClientUser(MessageCreateEventArgs e)
    {
        return await GetClientUser(e.Author);
    }

    public async Task<ClientUser?> GetClientUser(GuildMemberRemoveEventArgs e)
    {
        return await GetClientUser(e.Member);
    }

    public async Task<ClientUser?> GetClientUser(DiscordUser discordUser)
    {
        var user = await _userRepository.GetByExternalIdentifier(AuthSchemes.Discord.Id, discordUser.Id.ToString());
        if (user == null)
        {
            return null;
        }
        var clientUser = await _userHelper.GetClientUser(user);
        return clientUser;
    }

    public async Task<bool> HasPermission(MessageCreateEventArgs e, string permission)
    {
        var user = await GetClientUser(e.Author);
        return _userHelper.HasPermission(user, permission);
    }

    public async Task<bool> CheckPermission(BaseContext ctx, string permission)
    {
        var clientUser = await GetClientUser(ctx.User);
        if (!_userHelper.HasPermission(clientUser, permission))
        {
            await ctx.EditResponseAsync("You are not authorized");
            return false;
        }
        return true;
    }
}
