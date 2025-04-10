using Abyss.Web.Commands.Discord.Interfaces;
using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Options;

namespace Abyss.Web.Commands.Discord;

public class BaseCommand : IDiscordCommand, IDisposable
{
    protected readonly IServiceScope _serviceScope;
    protected readonly IUserHelper _userHelper;
    protected readonly IUserRepository _userRepository;
    protected readonly DiscordOptions _baseOptions;

    public BaseCommand(IServiceProvider serviceProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _userHelper = _serviceScope.ServiceProvider.GetRequiredService<IUserHelper>();
        _userRepository = _serviceScope.ServiceProvider.GetRequiredService<IUserRepository>();
        _baseOptions = _serviceScope.ServiceProvider.GetRequiredService<IOptions<DiscordOptions>>().Value;
    }

    public virtual Task MemberRemoved(GuildMemberRemovedEventArgs e)
    {
        return Task.CompletedTask;
    }

    public async Task<ClientUser> GetClientUser(MessageCreatedEventArgs e)
    {
        return await GetClientUser(e.Author);
    }

    public async Task<ClientUser> GetClientUser(GuildMemberRemovedEventArgs e)
    {
        return await GetClientUser(e.Member);
    }

    public async Task<ClientUser> GetClientUser(DiscordUser discordUser)
    {
        var user = await _userRepository.GetByExternalIdentifier(AuthSchemes.Discord.Id, discordUser.Id.ToString());
        if (user == null)
        {
            return null;
        }
        var clientUser = _userHelper.GetClientUser(user);
        return clientUser;
    }

    public async Task<User> GetUser(DiscordUser discordUser)
    {
        var clientUser = await GetClientUser(discordUser);
        var user = await _userHelper.GetUser(clientUser);
        return user;
    }

    public async Task<bool> HasPermission(MessageCreatedEventArgs e, string permission)
    {
        var user = await GetClientUser(e.Author);
        return _userHelper.HasPermission(user, permission);
    }

    public async Task<bool> CheckPermission(CommandContext ctx, string permission)
    {
        var clientUser = await GetClientUser(ctx.User);
        if (!_userHelper.HasPermission(clientUser, permission))
        {
            await ctx.EditResponseAsync("You are not authorized");
            return false;
        }
        return true;
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
        GC.SuppressFinalize(this);
    }
}
