
using Abyss.Web.Data;
using Abyss.Web.Data.GMod;
using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Commands.Discord
{
    public class RegisterCommand : BaseCommand
    {
        public override string Command => "register";

        private readonly IUserManager _userManager;
        private readonly ILogger<RegisterCommand> _logger;
        private readonly DiscordOptions _discordOptions;

        public RegisterCommand(
            IServiceProvider serviceProvider,
            IUserManager userManager,
            ILogger<RegisterCommand> logger,
            IOptions<DiscordOptions> discordOptions
            ) : base(serviceProvider)
        {
            _userManager = userManager;
            _logger = logger;
            _discordOptions = discordOptions.Value;
        }

        public override async Task ProcessMessage(MessageCreateEventArgs e, List<string> args)
        {
            var user = await _userRepository.GetByExternalIdentifier(AuthSchemes.Discord.Id, e.Author.Id.ToString());
            if (user == null)
            {
                user = new User
                {
                    Name = e.Author.Username,
                    Authentication = new Dictionary<string, string>
                    {
                        [AuthSchemes.Discord.Id] = e.Author.Id.ToString()
                    }
                };
                await _userRepository.Add(user);
                await e.Message.RespondAsync("User account created");
                return;
            }
            await e.Message.RespondAsync($"You are already registered as {user.Name}");
        }

        public override async Task MemberRemoved(GuildMemberRemoveEventArgs e)
        {
            try
            {
                var user = await _userRepository.GetByExternalIdentifier(AuthSchemes.Discord.Id, e.Member.Id.ToString());
                if (user != null)
                {
                    await _userManager.DeleteAuthScheme(user, AuthSchemes.Discord.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to handle removed Discord member: {e.Member.Id}");
            }
        }
    }
}
