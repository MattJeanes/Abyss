﻿using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Abyss.Web.Services;

public class CleanupService(ILogger<CleanupService> logger, IOptions<CleanupOptions> options, IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    private readonly ILogger _logger = logger;
    private readonly CleanupOptions _options = options.Value;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private Timer _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cleanup service is starting");

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
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(_options.IntervalSeconds));

        return Task.CompletedTask;
    }

    private async Task DoWork()
    {
        _logger.LogInformation("Running cleanup");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRepository<RefreshToken>>();
            var refreshTokens = await refreshTokenRepository.GetAll().Where(x => DateTime.UtcNow >= x.Expiry).ToListAsync();
            foreach (var token in refreshTokens)
            {
                refreshTokenRepository.Remove(token);
            }
            await refreshTokenRepository.SaveChanges();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to run cleanup");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cleanup service is stopping");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
