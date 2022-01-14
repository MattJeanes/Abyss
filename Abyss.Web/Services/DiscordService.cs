using Abyss.Web.Commands.Discord.Interfaces;
using Abyss.Web.Data.Options;
using Abyss.Web.Helpers.Interfaces;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Abyss.Web.Services;

public class DiscordService : IHostedService
{
    private readonly ILogger _logger;
    private readonly DiscordClient _client;
    private readonly SlashCommandsExtension _slash;
    private readonly DiscordOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IDiscordCommand> _commands;
    private readonly IUserHelper _userHelper;

    public DiscordService(
        ILogger<DiscordService> logger,
        DiscordClient client,
        IOptions<DiscordOptions> options,
        IServiceProvider serviceProvider,
        IUserHelper userHelper)
    {
        _logger = logger;
        _client = client;
        _slash = _client.UseSlashCommands(new SlashCommandsConfiguration
        {
            Services = serviceProvider
        });
        _options = options.Value;
        _slash.RegisterCommands(Assembly.GetExecutingAssembly(), _options.GuildId);
        _serviceProvider = serviceProvider;
        _commands = _serviceProvider.GetServices<IDiscordCommand>();
        _userHelper = userHelper;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Discord service is starting");

        _client.MessageCreated += async (c, e) =>
        {
            try
            {
                await MessageCreated(e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to run {nameof(MessageCreated)}");
            }
        }
;
        _client.GuildMemberRemoved += GuildMemberRemovedAsync;

        await _client.ConnectAsync();
    }

    private async Task MessageCreated(MessageCreateEventArgs e)
    {
        if (!e.Message.Content.ToLower().StartsWith(_options.CommandPrefix)) { return; }
        await e.Message.RespondAsync($"AbyssBot commands have been migrated to Discord Slash Commands, type / to view commands");
    }

    private async Task GuildMemberRemovedAsync(DiscordClient client, GuildMemberRemoveEventArgs e)
    {
        await Task.WhenAll(_commands.Select(x => x.MemberRemoved(e)).ToArray());
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Discord service is stopping");

        await _client.DisconnectAsync();
    }
}
