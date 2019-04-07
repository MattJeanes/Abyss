using Abyss.Web.Data;
using Abyss.Web.Managers.Interfaces;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Web.Commands.Discord
{
    public class ServerCommand : BaseCommand
    {
        private readonly IServerManager _serverManager;

        public override string Command => "server";
        public override string Permission => Permissions.ServerManager;

        public ServerCommand(IServiceProvider serviceProvider, IServerManager serverManager) : base(serviceProvider)
        {
            _serverManager = serverManager;
        }

        public override async Task ProcessMessage(MessageCreateEventArgs e, List<string> args)
        {
            switch (args.FirstOrDefault())
            {
                case null:
                    await GetServers(e);
                    break;
                case "start":
                    await StartServer(e, args.Skip(1).FirstOrDefault());
                    break;
                case "stop":
                    await StopServer(e, args.Skip(1).FirstOrDefault());
                    break;
                default:
                    await e.Message.RespondAsync("Unknown sub-command, try: (none), add");
                    break;
            }
        }

        private async Task GetServers(MessageCreateEventArgs e)
        {
            var servers = await _serverManager.GetServers();
            var serverList = string.Join("\n", servers.Select(x => $"{x.Name}"));
            await e.Message.RespondAsync($"Server list:\n{serverList}");
        }

        private async Task StartServer(MessageCreateEventArgs e, string name)
        {
            var servers = await _serverManager.GetServers();
            var server = servers.FirstOrDefault(x => x.Name == name);
            if (server == null)
            {
                await e.Message.RespondAsync("Server not found");
                return;
            }
            await e.Message.RespondAsync($"Starting up {server.Name}");
            await _serverManager.Start(server.Id.ToString(), async (logItem) =>
            {
                await e.Message.RespondAsync(logItem.ToString());
            });
        }

        private async Task StopServer(MessageCreateEventArgs e, string name)
        {
            var servers = await _serverManager.GetServers();
            var server = servers.FirstOrDefault(x => x.Name == name);
            if (server == null)
            {
                await e.Message.RespondAsync("Server not found");
                return;
            }
            await e.Message.RespondAsync($"Stopping {server.Name}");
            await _serverManager.Stop(server.Id.ToString(), async (logItem) =>
            {
                await e.Message.RespondAsync(logItem.ToString());
            });
        }
    }
}
