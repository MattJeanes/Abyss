using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Helpers;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using DSharpPlus;
using Microsoft.Extensions.Options;
using System.Text;

namespace Abyss.Web.Services;

public class ReminderService(
    ILogger<ReminderService> logger,
    IOptions<ReminderOptions> options,
    IServiceProvider serviceProvider,
    DiscordClient discordClient) : IHostedService, IDisposable
{
    private readonly ILogger<ReminderService> _logger = logger;
    private readonly ReminderOptions _options = options.Value;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly DiscordClient _discordClient = discordClient;
    private Timer _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reminder service is starting");

        _timer = new Timer(async (state) =>
        {
            try
            {
                await DoWork();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to run {nameof(DoWork)}");
            }
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(_options.CheckIntervalSeconds));

        return Task.CompletedTask;
    }

    private async Task DoWork()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var serverManager = scope.ServiceProvider.GetRequiredService<IServerManager>();
            var serverRepository = scope.ServiceProvider.GetRequiredService<IRepository<Server>>();
            var servers = await serverManager.GetServers();
            foreach (var server in servers)
            {
                if (server.NextReminder.HasValue && DateTime.UtcNow > server.NextReminder.Value)
                {
                    try
                    {
                        var message = new StringBuilder($"Psst! As a reminder the {server.Name} server is still running. ");
                        var richStatus = await serverManager.GetServerRichStatus(server);
                        AppendRichStatusData(message, richStatus);
                        if (server.ReminderIntervalMinutes.HasValue)
                        {
                            message.Append($"I'll remind you again in {TimeSpan.FromMinutes(server.ReminderIntervalMinutes.Value).ToReadableString()} if it's not been stopped by then.");
                        }
                        else
                        {
                            message.Append("I won't remind you again.");
                        }

                        _logger.LogInformation($"Sending reminder: {message.ToString()}");
                        var channel = await _discordClient.GetChannelAsync(_options.DiscordChannelId);
                        await _discordClient.SendMessageAsync(channel, message.ToString());

                        if (server.ReminderIntervalMinutes.HasValue)
                        {
                            server.NextReminder = DateTime.UtcNow.AddMinutes(server.ReminderIntervalMinutes.Value);
                        }
                        else
                        {
                            server.NextReminder = null;
                        }
                        await serverRepository.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Failed to send reminder for server id {server.Id}");
                    }
                }
            }
        }
    }

    private static void AppendRichStatusData(StringBuilder message, Data.ServerRichStatus richStatus)
    {
        if (richStatus == null)
        {
            return;
        }
        if (!string.IsNullOrEmpty(richStatus.Error))
        {
            message.Append($"Failed to retrieve server rich status: {richStatus.Error}. ");
            return;
        }
        if (richStatus.Players != null)
        {
            if (richStatus.Players.Count > 0)
            {
                message.Append($"Players online: {string.Join(", ", richStatus.Players)}. ");
            }
            else
            {
                message.Append("No players online. ");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reminder service is stopping");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
