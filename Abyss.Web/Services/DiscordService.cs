using Abyss.Web.Data.Options;
using DSharpPlus;
using Microsoft.Extensions.Options;

namespace Abyss.Web.Services;

public class DiscordService : IHostedService
{
    private readonly ILogger _logger;
    private readonly DiscordClient _client;

    public DiscordService(
        ILogger<DiscordService> logger,
        DiscordClient client,
        IOptions<DiscordOptions> options)
    {
        _logger = logger;
        _client = client;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Discord service is starting");

        await _client.ConnectAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Discord service is stopping");

        await _client.DisconnectAsync();
    }
}
