using Abyss.Web.Data.Options;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using Microsoft.Extensions.Options;

namespace Abyss.Web.Services;

public class DiscordService : IHostedService
{
    private readonly ILogger _logger;
    private readonly DiscordClient _client;
    private readonly CommandsExtension _commandsExtension;

    public DiscordService(
        ILogger<DiscordService> logger,
        DiscordClient client,
        IOptions<DiscordOptions> options)
    {
        _logger = logger;
        _client = client;
        _commandsExtension = _client.UseCommands(new CommandsConfiguration
        {
            DebugGuildId = options.Value.GuildId ?? default,
            RegisterDefaultCommandProcessors = false
        });
        _commandsExtension.AddCommands(typeof(Program).Assembly);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Discord service is starting");

        await _commandsExtension.AddProcessorAsync(new SlashCommandProcessor());
        await _client.ConnectAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Discord service is stopping");

        await _client.DisconnectAsync();
    }
}
