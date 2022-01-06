using Abyss.Web.Data;
using Abyss.Web.Managers.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Abyss.Web.Commands.Discord;

public class ServerCommand : BaseCommand
{
    private readonly IServerManager _serverManager;
    private readonly ILogger<ServerCommand> _logger;

    public override string Command => "server";

    public ServerCommand(IServiceProvider serviceProvider, IServerManager serverManager, ILogger<ServerCommand> logger) : base(serviceProvider)
    {
        _serverManager = serverManager;
        _logger = logger;
    }

    public override async Task ProcessMessage(MessageCreateEventArgs e, List<string> args)
    {
        var subcommand = args.FirstOrDefault();
        var alias = args.Skip(1).FirstOrDefault();
        switch (subcommand)
        {
            case "list":
                await GetServers(e);
                break;
            case "start":
                await StartServer(e, alias);
                break;
            case "stop":
                await StopServer(e, alias);
                break;
            case "status":
                await GetServerStatus(e, alias);
                break;
            default:
                await e.Message.RespondAsync($"Unknown sub-command '{subcommand}', try: list, start, stop, status");
                break;
        }
    }

    private async Task GetServers(MessageCreateEventArgs e)
    {
        var servers = await _serverManager.GetServers();
        var serverList = string.Join("\n", servers.Select(x => $"{x.Name} ({x.Alias}): {x.StatusId}"));
        await e.Message.RespondAsync($"Server list:\n{serverList}");
    }

    private async Task StartServer(MessageCreateEventArgs e, string? alias)
    {
        if (string.IsNullOrEmpty(alias))
        {
            await e.Message.RespondAsync($"Server alias required");
            return;
        }
        if (!await HasPermission(e, Permissions.ServerManager))
        {
            await e.Message.RespondAsync($"You do not have permission to start servers");
            return;
        };
        var server = await _serverManager.GetServerByAlias(alias);
        if (server == null)
        {
            await e.Message.RespondAsync("Server not found");
            return;
        }
        var thread = await TryCreateThread(e, $"Starting up {server.Name}");
        await _serverManager.Start(server.Id.ToString(), async (logItem) =>
        {
            if (thread == null)
            {
                await e.Message.RespondAsync(logItem.ToString());
            }
            else
            {
                await thread.SendMessageAsync(logItem.ToString());
            }
        });
    }

    private async Task StopServer(MessageCreateEventArgs e, string? alias)
    {
        if (string.IsNullOrEmpty(alias))
        {
            await e.Message.RespondAsync($"Server alias required");
            return;
        }
        if (!await HasPermission(e, Permissions.ServerManager))
        {
            await e.Message.RespondAsync($"You do not have permission to stop servers");
            return;
        };
        var server = await _serverManager.GetServerByAlias(alias);
        if (server == null)
        {
            await e.Message.RespondAsync("Server not found");
            return;
        }
        var thread = await TryCreateThread(e, $"Stopping {server.Name}");
        await _serverManager.Stop(server.Id.ToString(), async (logItem) =>
        {
            if (thread == null)
            {
                await e.Message.RespondAsync(logItem.ToString());
            }
            else
            {
                await thread.SendMessageAsync(logItem.ToString());
            }
        });
    }

    private async Task GetServerStatus(MessageCreateEventArgs e, string? alias)
    {
        if (string.IsNullOrEmpty(alias))
        {
            await e.Message.RespondAsync($"Server alias required");
            return;
        }
        var server = await _serverManager.GetServerByAlias(alias);
        if (server == null)
        {
            await e.Message.RespondAsync("Server not found");
            return;
        }
        switch (server.StatusId)
        {
            case ServerStatus.Activating:
                await e.Message.RespondAsync($"{server.Name} is starting up");
                break;
            case ServerStatus.Active:
                await e.Message.RespondAsync($"{server.Name} is running");
                break;
            case ServerStatus.Deactivating:
                await e.Message.RespondAsync($"{server.Name} is stopping");
                break;
            case ServerStatus.Inactive:
                await e.Message.RespondAsync($"{server.Name} is stopped");
                break;
            default:
                await e.Message.RespondAsync($"{server.Name} is in unknown state '{server.StatusId}'");
                break;
        }
    }

    private async Task<DiscordThreadChannel?> TryCreateThread(MessageCreateEventArgs e, string name)
    {
        DiscordThreadChannel? thread = null;
        if (!e.Channel.IsPrivate)
        {
            try
            {
                thread = await e.Message.CreateThreadAsync(name, DSharpPlus.AutoArchiveDuration.Hour);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to create thread");
            }
        }
        return thread;
    }
}
