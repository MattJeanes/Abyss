using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abyss.Web.Commands.Discord.Interfaces;
using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Abyss.Web.Commands.Discord
{
    public class BaseCommand : IDiscordCommand
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IUserHelper _userHelper;
        protected readonly IUserRepository _userRepository;
        protected readonly DiscordOptions _baseOptions;

        public virtual string Command => null;
        public virtual string Permission => null;

        public BaseCommand(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
            _userHelper = _serviceProvider.GetRequiredService<IUserHelper>();
            _userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
            _baseOptions = _serviceProvider.GetRequiredService<IOptions<DiscordOptions>>().Value;
        }

        public virtual Task ProcessMessage(MessageCreateEventArgs e, List<string> args)
        {
            return Task.CompletedTask;
        }

        public virtual Task MemberRemoved(GuildMemberRemoveEventArgs e)
        {
            return Task.CompletedTask;
        }

        public Task<ClientUser> GetClientUser(MessageCreateEventArgs e)
        {
            return GetClientUser(e.Author);
        }

        public Task<ClientUser> GetClientUser(GuildMemberRemoveEventArgs e)
        {
            return GetClientUser(e.Member);
        }

        public async Task<ClientUser> GetClientUser(DiscordUser discordUser)
        {
            var user = await _userRepository.GetByExternalIdentifier(AuthSchemes.Discord.Id, discordUser.Id.ToString());
            if(user == null)
            {
                return null;
            }
            var clientUser = await _userHelper.GetClientUser(user);
            return clientUser;
        }
    }
}