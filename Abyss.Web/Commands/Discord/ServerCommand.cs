using Abyss.Web.Data;
using Abyss.Web.Managers.Interfaces;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace Abyss.Web.Commands.Discord;

[SlashCommandGroup("server", "View or manage servers")]
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

    [SlashCommand("status", "Get server status")]
    public async Task RunGetStatus(InteractionContext ctx, [ChoiceProvider(typeof(ServerChoiceProvider))][Option("server", "Server name")] string alias)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var server = await _serverManager.GetServerByAlias(alias);
        if (server != null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(GetServerStatus(server)));
        }
        else
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Server not found"));
        }
    }

    [SlashCommand("start", "Start server")]
    public async Task RunStartServer(InteractionContext ctx, [ChoiceProvider(typeof(ServerChoiceProvider))][Option("server", "Server name")] string alias)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var clientUser = await GetClientUser(ctx.User);
        if (!_userHelper.HasPermission(clientUser, Data.Permissions.ServerManager))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not authorized"));
            return;
        }

        var server = await _serverManager.GetServerByAlias(alias);
        if (server != null)
        {
            try
            {
                await _serverManager.Start(server.Id.ToString(), async (logItem) =>
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(logItem.ToString()));
                });
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Failed to start server: {ex.Message}"));
            }
        }
        else
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Server not found"));
        }
    }

    [SlashCommand("stop", "Stop server")]
    public async Task RunStopServer(InteractionContext ctx, [ChoiceProvider(typeof(ServerChoiceProvider))][Option("server", "Server name")] string alias)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var clientUser = await GetClientUser(ctx.User);
        if (!_userHelper.HasPermission(clientUser, Data.Permissions.ServerManager))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not authorized"));
            return;
        }

        var server = await _serverManager.GetServerByAlias(alias);
        if (server != null)
        {
            try
            {
                await _serverManager.Stop(server.Id.ToString(), async (logItem) =>
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(logItem.ToString()));
                });
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Failed to stop server: {ex.Message}"));
            }
        }
        else
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Server not found"));
        }
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
        if (!await HasPermission(e, Data.Permissions.ServerManager))
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
        if (!await HasPermission(e, Data.Permissions.ServerManager))
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
        await e.Message.RespondAsync(GetServerStatus(server));
    }

    private static string GetServerStatus(Entities.Server server)
    {
        return server.StatusId switch
        {
            ServerStatus.Activating => $"{server.Name} is starting up",
            ServerStatus.Active => $"{server.Name} is running",
            ServerStatus.Deactivating => $"{server.Name} is stopping",
            ServerStatus.Inactive => $"{server.Name} is stopped",
            _ => $"{server.Name} is in unknown state '{server.StatusId}'",
        };
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

public class ServerChoiceProvider : ChoiceProvider
{
    public override async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
    {
        using var scope = Services.CreateScope();
        var serverManager = scope.ServiceProvider.GetRequiredService<IServerManager>();
        var servers = await serverManager.GetServers();

        return servers.OrderBy(x => x.Name).Select(x => new DiscordApplicationCommandOptionChoice(x.Name, x.Alias));
    }
}