
using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Managers.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Options;

namespace Abyss.Web.Commands.Discord;

public class RegisterCommand : BaseCommand
{
    public override string Command => "register";

    private readonly IUserManager _userManager;
    private readonly ILogger<RegisterCommand> _logger;
    private readonly DiscordOptions _discordOptions;

    public RegisterCommand(
        IServiceProvider serviceProvider,
        IUserManager userManager,
        ILogger<RegisterCommand> logger,
        IOptions<DiscordOptions> discordOptions
        ) : base(serviceProvider)
    {
        _userManager = userManager;
        _logger = logger;
        _discordOptions = discordOptions.Value;
    }

    public override async Task ProcessMessage(MessageCreateEventArgs e, List<string> args)
    {
        var response = await RegisterUser(e.Author);
        await e.Message.RespondAsync(response);
    }

    [SlashCommand("register", "Register your account")]
    public async Task RunCommand(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);

        var response = await RegisterUser(ctx.User);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(response));
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
                Name = discordUser.Username,
                Authentication = new Dictionary<string, string>
                {
                    [AuthSchemes.Discord.Id] = discordUser.Id.ToString()
                }
            };
            await _userRepository.Add(user);
            return "User account created";
        }
        return $"You are already registered as {user.Name}";
    }
}
