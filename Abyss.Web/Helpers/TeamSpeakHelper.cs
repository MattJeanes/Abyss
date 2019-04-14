using Abyss.Web.Data.Options;
using Abyss.Web.Data.TeamSpeak;
using Abyss.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamSpeak3QueryApi.Net.Specialized;
using TeamSpeak3QueryApi.Net.Specialized.Responses;

namespace Abyss.Web.Helpers
{
    public class TeamSpeakHelper : ITeamSpeakHelper
    {
        private readonly TeamSpeakOptions _options;
        private readonly IHubContext<OnlineHub> _onlineHub;
        private List<Client> _clients;
        private List<Channel> _channels;
        private Task _updateTask;

        public TeamSpeakHelper(IOptions<TeamSpeakOptions> options, IHubContext<OnlineHub> onlineHub)
        {
            _options = options.Value;
            _onlineHub = onlineHub;
        }

        public async Task<List<Client>> GetClients()
        {
            if (_clients == null)
            {
                await Update();
            }
            return _clients;
        }

        public async Task<List<Channel>> GetChannels()
        {
            if (_channels == null)
            {
                await Update();
            }
            return _channels;
        }


        public Task Update()
        {
            if (_updateTask != null)
            {
                return _updateTask;
            }
            return _updateTask = Task.Run(async () =>
            {
                try
                {
                    using (var teamspeak = new TeamSpeakClient(_options.Host))
                    {
                        await teamspeak.Connect();
                        await teamspeak.UseServer(_options.ServerId);
                        var rawChannels = await teamspeak.GetChannels();
                        var channels = new List<Channel>();
                        foreach (var rawChannel in rawChannels)
                        {
                            channels.Add(ConvertChannel(rawChannel, rawChannels, channels));
                        }
                        var rawClients = await teamspeak.GetClients();
                        var rawDetailedClients = new List<GetClientDetailedInfo>();
                        foreach (var rawClient in rawClients.Where(x => x.Type == ClientType.FullClient))
                        {
                            rawDetailedClients.Add(await teamspeak.GetClientInfo(rawClient));
                        }
                        var clients = rawDetailedClients.Select(x => ConvertClient(x, channels)).ToList();

                        _channels = channels;
                        _clients = clients;
                        await _onlineHub.Clients.All.SendAsync("update", _clients, _channels);
                    }
                }
                finally
                {
                    _updateTask = null;
                }
            });
        }

        private Channel ConvertChannel(GetChannelListInfo rawChannel, IReadOnlyList<GetChannelListInfo> rawChannels, List<Channel> channels)
        {
            return new Channel
            {
                Id = rawChannel.Id,
                Name = rawChannel.Name,
                ParentId = rawChannel.ParentChannelId > 0 ? rawChannel.ParentChannelId : (int?)null
            };
        }

        private Client ConvertClient(GetClientDetailedInfo rawClient, List<Channel> channels)
        {
            return new Client
            {
                Name = rawClient.NickName,
                ChannelId = rawClient.ChannelId,
                ConnectedSeconds = (int)rawClient.ConnectionTime.TotalSeconds,
                IdleSeconds = (int)rawClient.IdleTime.TotalSeconds
            };
        }
    }
}
