using Abyss.Web.Data.Options;
using Abyss.Web.Helpers.Interfaces;
using DSharpPlus;
using Microsoft.Extensions.Options;

namespace Abyss.Web.Services;

public class QuoteOfTheDayService : CronJobService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IQuoteHelper _quoteHelper;
    private readonly QuoteOfTheDayOptions _options;
    private readonly ILogger<QuoteOfTheDayService> _logger;

    public QuoteOfTheDayService(
        IServiceProvider serviceProvider,
        IQuoteHelper quoteHelper,
        IScheduleConfig<QuoteOfTheDayService> config,
        IOptions<QuoteOfTheDayOptions> options,
        ILogger<QuoteOfTheDayService> logger)
        : base(config.CronExpression, config.TimeZoneInfo)
    {
        _serviceProvider = serviceProvider;
        _quoteHelper = quoteHelper;
        _options = options.Value;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Quote of the day service started");
        return base.StartAsync(cancellationToken);
    }

    public override async Task DoWork(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var discordClient = scope.ServiceProvider.GetRequiredService<DiscordClient>();
            _logger.LogInformation("Sending quote of the day");
            string message;
            try
            {
                var quote = await _quoteHelper.GetQuote();
                message = $"**Quote of the day:** {_quoteHelper.FormatQuote(quote)}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve quote of the day");
                message = $"Failed to retrieve quote of the day: {ex.Message}";
            }
            var channel = await discordClient.GetChannelAsync(_options.DiscordChannelId);
            await discordClient.SendMessageAsync(channel, message.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send quote of the day");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Quote of the day service stopped");
        return base.StopAsync(cancellationToken);
    }
}
