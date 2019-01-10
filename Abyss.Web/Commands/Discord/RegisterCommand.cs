/*
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

        private readonly IGModHelper _gmodHelper;
        private readonly IUserManager _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<RegisterCommand> _logger;
        private readonly DiscordOptions _discordOptions;

        public RegisterCommand(
            IGModHelper gmodHelper,
            IUserManager userManager,
            IUserRepository userRepository,
            ILogger<RegisterCommand> logger,
            IOptions<DiscordOptions> discordOptions
            )
        {
            _gmodHelper = gmodHelper;
            _userManager = userManager;
            _userRepository = userRepository;
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
            }

            if (!user.Authentication.ContainsKey(AuthSchemes.Steam.Id))
            {
                await e.Message.RespondAsync("Your account is not linked with Steam, please visit https://abyss.mattjeanes.com and login with both Discord and Steam to link then try again");
                return;
            }

            var resp = await _gmodHelper.ChangeRank(new ChangeRankDTO
            {
                SteamId64 = user.Authentication[AuthSchemes.Steam.Id],
                Rank = _discordOptions.MemberRankId
            });

            if (string.IsNullOrEmpty(resp))
            {
                await e.Message.RespondAsync($"Your associated steam account ({user.Authentication[AuthSchemes.Steam.Id]}) has been updated to {_discordOptions.MemberRankName} on the GMod server");
            }
            else
            {
                await e.Message.RespondAsync($"No change was made: {resp}");
            }
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
*/