
using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Managers.Interfaces;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.ComponentModel;

namespace Abyss.Web.Commands.Discord;

public class RegisterCommand(
    IServiceProvider serviceProvider,
    IUserManager userManager,
    ILogger<RegisterCommand> logger
        ) : BaseCommand(serviceProvider)
{
    private readonly IUserManager _userManager = userManager;
    private readonly ILogger<RegisterCommand> _logger = logger;

    [Command("register"), Description("Register your account")]
    public async Task Register(CommandContext ctx)
    {
        await ctx.DeferResponseAsync();

        var response = await RegisterUser(ctx.User);

        await ctx.EditResponseAsync(response);
    }

    public override async Task MemberRemoved(GuildMemberRemovedEventArgs e)
    {
        try
        {
            var user = await _userRepository.GetByExternalIdentifier(AuthSchemes.Discord.Id, e.Member.Id.ToString());
            if (user != null)
            {
                await _userManager.DeleteAuthScheme(user, AuthSchemes.Discord.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle removed Discord member: {Id}", e.Member.Id);
        }
    }

    private async Task<string> RegisterUser(DiscordUser discordUser)
    {
        var user = await _userRepository.GetByExternalIdentifier(AuthSchemes.Discord.Id, discordUser.Id.ToString());
        if (user == null)
        {
            user = new User
            {
                Name = discordUser.Username
            };
            user.Authentications.Add(new UserAuthentication
            {
                SchemeType = AuthSchemeType.Discord,
                Identifier = discordUser.Id.ToString()
            });
            _userRepository.Add(user);
            await _userRepository.SaveChanges();
            return "User account created";
        }
        return $"You are already registered as {user.Name}";
    }
}
