using Abyss.Web.Commands.Discord.Interfaces;
using Abyss.Web.Data.Options;
using Abyss.Web.Helpers.Interfaces;
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
            _options = options.Value;
            _serviceProvider = serviceProvider;
            _commands = _serviceProvider.GetServices<IDiscordCommand>();
            _userHelper = userHelper;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Discord service is starting");

            _client.MessageCreated += async (e) =>
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
            var argString = e.Message.Content.Trim().Substring(_options.CommandPrefix.Length).Trim();
            var args = argString.Split('"')
                     .Select((element, index) => index % 2 == 0  // If even index
                                           ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)  // Split the item
                                           : new string[] { element })  // Keep the entire item
                     .SelectMany(element => element).ToList();
            var cmd = args.FirstOrDefault();
            if (string.IsNullOrEmpty(cmd) || cmd == "help")
            {
                await e.Message.RespondAsync($"Commands: {string.Join(", ", _commands.Where(x => !string.IsNullOrEmpty(x.Command)).Select(x => x.Command))}");
                return;
            }
            args = args.Skip(1).ToList();
            var command = _commands.FirstOrDefault(x => x.Command == cmd.ToLower());
            if (command == null)
            {
                await e.Message.RespondAsync($"Unknown command, type `{_options.CommandPrefix} help` for all commands");
                return;
            }

            if (!string.IsNullOrEmpty(command.Permission))
            {
                var user = await command.GetClientUser(e);
                if (!_userHelper.HasPermission(user, command.Permission))
                {
                    await e.Message.RespondAsync($"You do not have permission to use this command");
                    return;
                };
            }
            _logger.LogInformation($"Command {command} with args {string.Join(" ", args)} run by {e.Author.Username}");
            try
            {
                await command.ProcessMessage(e, args);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"Failed to run {command} with args {string.Join(" ", args)} run by {e.Author.Username}");
                await e.Message.RespondAsync($"Failed to run command: {ex.Message}");
            }
        }

        private async Task GuildMemberRemovedAsync(GuildMemberRemoveEventArgs e)
        {
            await Task.WhenAll(_commands.Select(x => x.MemberRemoved(e)).ToArray());
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Discord service is stopping");

            await _client.DisconnectAsync();
        }
    }
}
