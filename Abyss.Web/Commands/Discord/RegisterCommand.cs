
using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Helpers;
using Abyss.Web.Managers.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Options;

namespace Abyss.Web.Commands.Discord;

public class RegisterCommand(
    IServiceProvider serviceProvider,
    IUserManager userManager,
    ILogger<RegisterCommand> logger,
    IOptions<DiscordOptions> discordOptions
        ) : BaseCommand(serviceProvider)
{
    private readonly IUserManager _userManager = userManager;
    private readonly ILogger<RegisterCommand> _logger = logger;
    private readonly DiscordOptions _discordOptions = discordOptions.Value;

    [SlashCommand("register", "Register your account")]
    public async Task Register(InteractionContext ctx)
    {
        await ctx.CreateDeferredResponseAsync();

        var response = await RegisterUser(ctx.User);

        await ctx.EditResponseAsync(response);
    }

    public override async Task MemberRemoved(GuildMemberRemoveEventArgs e)
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
            _logger.LogError(ex, $"Failed to handle removed Discord member: {e.Member.Id}");
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
