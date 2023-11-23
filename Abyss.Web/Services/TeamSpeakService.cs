using Abyss.Web.Data.Options;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.Extensions.Options;

namespace Abyss.Web.Services;

public class TeamSpeakService(ILogger<TeamSpeakService> logger, IOptions<TeamSpeakOptions> options, IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    private readonly ILogger _logger = logger;
    private readonly TeamSpeakOptions _options = options.Value;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private Timer _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TeamSpeak service is starting");

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
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(_options.UpdateIntervalSeconds));

        return Task.CompletedTask;
    }

    private async Task DoWork()
    {
        _logger.LogDebug("Updating TeamSpeak");

        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var teamSpeakHelper = scope.ServiceProvider.GetRequiredService<ITeamSpeakHelper>();
                await teamSpeakHelper.Update();
            }
            _logger.LogDebug("TeamSpeak update complete");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update TeamSpeak");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TeamSpeak service is stopping");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
