using Abyss.Web.Data.Options;
using Abyss.Web.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Abyss.Web.Services
{
    public class TeamSpeakService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly TeamSpeakOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public TeamSpeakService(ILogger<TeamSpeakService> logger, IOptions<TeamSpeakOptions> options, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _options = options.Value;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TeamSpeak service is starting");

            _timer = new Timer(async (state) => { await Task.Run(DoWork); }, null, TimeSpan.Zero, TimeSpan.FromSeconds(_options.UpdateIntervalSeconds));

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
        }
    }
}
