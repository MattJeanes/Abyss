using Abyss.Web.Commands.Discord.Interfaces;
using Abyss.Web.Data.Options;
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

    public DiscordService(
        ILogger<DiscordService> logger,
        DiscordClient client,
        IOptions<DiscordOptions> options,
        IServiceProvider serviceProvider)
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
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Discord service is starting");
        ;
        _client.GuildMemberRemoved += GuildMemberRemovedAsync;

        await _client.ConnectAsync();
    }

    private async Task GuildMemberRemovedAsync(DiscordClient client, GuildMemberRemoveEventArgs e)
    {
        using var scope = _serviceProvider.CreateScope();
        var commands = scope.ServiceProvider.GetServices<IDiscordCommand>();
        await Task.WhenAll(commands.Select(x => x.MemberRemoved(e)).ToArray());
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Discord service is stopping");

        await _client.DisconnectAsync();
    }
}
