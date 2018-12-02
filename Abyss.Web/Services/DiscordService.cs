using Abyss.Web.Commands.Discord.Interfaces;
using Abyss.Web.Data.Options;
using DSharpPlus;
using DSharpPlus.EventArgs;
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
    public class DiscordService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly DiscordClient _client;
        private readonly DiscordOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IDiscordCommand> _commands;

        public DiscordService(ILogger<DiscordService> logger, DiscordClient client, IOptions<DiscordOptions> options, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _client = client;
            _options = options.Value;
            _serviceProvider = serviceProvider;
            _commands = _serviceProvider.GetServices<IDiscordCommand>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Discord service is starting");

            _client.MessageCreated += MessageCreated;

            await _client.ConnectAsync();
        }

        private async Task MessageCreated(MessageCreateEventArgs e)
        {
            if (!e.Message.Content.ToLower().StartsWith(_options.CommandPrefix)) { return; }
            var args = e.Message.Content.Replace(_options.CommandPrefix, "").Trim().Split(" ").ToList();
            var cmd = args.FirstOrDefault();
            if (string.IsNullOrEmpty(cmd))
            {
                cmd = "help";
            }
            args = args.Skip(1).ToList();
            var command = _commands.FirstOrDefault(x => x.Command == cmd.ToLower());
            if (command == null)
            {
                await e.Message.RespondAsync($"Unknown command, try `{_options.CommandPrefix} help`");
                return;
            }
            _logger.LogInformation($"Command {command} with args {string.Join(" ", args)} run by {e.Author.Username}");
            await command.ProcessMessage(e, args);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Discord service is stopping");

            await _client.DisconnectAsync();
        }
    }
}
