using Abyss.Web.Commands.Discord.ChoiceProviders;
using Abyss.Web.Data;
using Abyss.Web.Helpers;
using Abyss.Web.Managers.Interfaces;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Abyss.Web.Commands.Discord;

[SlashCommandGroup("server", "View or manage servers")]
public class ServerCommand : BaseCommand
{
    private readonly IServerManager _serverManager;
    private readonly ILogger<ServerCommand> _logger;

    public ServerCommand(IServiceProvider serviceProvider, IServerManager serverManager, ILogger<ServerCommand> logger) : base(serviceProvider)
    {
        _serverManager = serverManager;
        _logger = logger;
    }

    [SlashCommand("status", "Get server status")]
    public async Task GetStatus(InteractionContext ctx, [ChoiceProvider(typeof(ServerChoiceProvider))][Option("server", "Server name")] string alias)
    {
        await ctx.CreateDeferredResponseAsync();

        var server = await _serverManager.GetServerByAlias(alias);
        if (server != null)
        {
            var richStatus = await _serverManager.GetServerRichStatus(server);
            await ctx.EditResponseAsync(FormatServerStatus(server, richStatus));
        }
        else
        {
            await ctx.EditResponseAsync("Server not found");
        }
    }

    [SlashCommand("start", "Start a server")]
    public async Task StartServer(InteractionContext ctx, [ChoiceProvider(typeof(ServerChoiceProvider))][Option("server", "Server name")] string alias)
    {
        await ctx.CreateDeferredResponseAsync();

        if (!await CheckPermission(ctx, Data.Permissions.ServerManager))
        {
            return;
        }

        var server = await _serverManager.GetServerByAlias(alias);
        if (server == null)
        {
            await ctx.EditResponseAsync("Server not found");
            return;
        }
        if (server.StatusId != ServerStatus.Inactive)
        {
            await ctx.EditResponseAsync($"Cannot start server: {FormatServerStatus(server)}");
            return;
        }
        var thread = await TryCreateThread(ctx, $"Starting {server.Name}");
        var response = string.Empty;
        try
        {
            await _serverManager.Start(server.Id.ToString(), async (logItem) =>
            {
                if (thread == null)
                {
                    await ctx.CreateFollowupMessageAsync(logItem.ToString());
                }
                else
                {
                    await thread.SendMessageAsync(logItem.ToString());
                }
            });
            response = $"Started server {server.Name}";
        }
        catch (Exception ex)
        {
            response = $"Failed to start server {server.Name}: {ex.Message}";
        }
        finally
        {
            await ctx.EditResponseAsync(response);
            if (thread == null)
            {
                await ctx.CreateFollowupMessageAsync($"{response} {ctx.User.Mention}");
            }
            else
            {
                await thread.SendMessageAsync($"{response} {ctx.User.Mention}");
            }
        }
    }

    [SlashCommand("stop", "Stop a server")]
    public async Task StopServer(InteractionContext ctx, [ChoiceProvider(typeof(ServerChoiceProvider))][Option("server", "Server name")] string alias)
    {
        await ctx.CreateDeferredResponseAsync();

        if (!await CheckPermission(ctx, Data.Permissions.ServerManager))
        {
            return;
        }

        var server = await _serverManager.GetServerByAlias(alias);
        if (server == null)
        {
            await ctx.EditResponseAsync("Server not found");
            return;
        }
        if (server.StatusId != ServerStatus.Active)
        {
            await ctx.EditResponseAsync($"Cannot stop server: {FormatServerStatus(server)}");
            return;
        }
        var thread = await TryCreateThread(ctx, $"Stopping {server.Name}");
        var response = string.Empty;
        try
        {
            await _serverManager.Stop(server.Id.ToString(), async (logItem) =>
            {
                if (thread == null)
                {
                    await ctx.CreateFollowupMessageAsync(logItem.ToString());
                }
                else
                {
                    await thread.SendMessageAsync(logItem.ToString());
                }
            });
            response = $"Stopped server {server.Name}";
        }
        catch (Exception ex)
        {
            response = $"Failed to stop server {server.Name}: {ex.Message}";
        }
        finally
        {
            await ctx.EditResponseAsync(response);
            if (thread == null)
            {
                await ctx.CreateFollowupMessageAsync($"{response} {ctx.User.Mention}");
            }
            else
            {
                await thread.SendMessageAsync($"{response} {ctx.User.Mention}");
            }
        }
    }

    [SlashCommand("restart", "Restart a server, also updates it")]
    public async Task RestartServer(InteractionContext ctx, [ChoiceProvider(typeof(ServerChoiceProvider))][Option("server", "Server name")] string alias)
    {
        await ctx.CreateDeferredResponseAsync();

        if (!await CheckPermission(ctx, Data.Permissions.ServerManager))
        {
            return;
        }

        var server = await _serverManager.GetServerByAlias(alias);
        if (server == null)
        {
            await ctx.EditResponseAsync($"Server '{alias}' not found");
            return;
        }
        if (server.StatusId != ServerStatus.Active)
        {
            await ctx.EditResponseAsync($"Cannot restart server: {FormatServerStatus(server)}");
            return;
        }
        var thread = await TryCreateThread(ctx, $"Restarting {server.Name}");
        var response = string.Empty;
        try
        {
            await _serverManager.Restart(server.Id.ToString(), async (logItem) =>
            {
                if (thread == null)
                {
                    await ctx.CreateFollowupMessageAsync(logItem.ToString());
                }
                else
                {
                    await thread.SendMessageAsync(logItem.ToString());
                }
            });
            response = $"Restarted server {server.Name}";
        }
        catch (Exception ex)
        {
            response = $"Failed to restart server {server.Name}: {ex.Message}";
        }
        finally
        {
            await ctx.EditResponseAsync(response);
            if (thread == null)
            {
                await ctx.CreateFollowupMessageAsync($"{response} {ctx.User.Mention}");
            }
            else
            {
                await thread.SendMessageAsync($"{response} {ctx.User.Mention}");
            }
        }
    }

    private static string FormatServerStatus(Entities.Server server, ServerRichStatus richStatus = null)
    {
        var status = server.StatusId switch
        {
            ServerStatus.Activating => $"{server.Name} is starting up",
            ServerStatus.Active => $"{server.Name} is running",
            ServerStatus.Deactivating => $"{server.Name} is stopping",
            ServerStatus.Inactive => $"{server.Name} is stopped",
            _ => $"{server.Name} is in unknown state '{server.StatusId}'",
        };

        if (richStatus != null)
        {
            if (richStatus.Players != null)
            {
                if (richStatus.Players.Count > 0)
                {
                    status += $". Players online: {string.Join(", ", richStatus.Players)}";
                }
                else
                {
                    status += ". No players online";
                }
            }
        }

        return status;
    }

    private async Task<DiscordThreadChannel> TryCreateThread(InteractionContext ctx, string name)
    {
        DiscordThreadChannel thread = null;
        if (!ctx.Channel.IsPrivate)
        {
            try
            {
                var response = await ctx.GetOriginalResponseAsync();
                thread = await response.CreateThreadAsync(name, AutoArchiveDuration.Hour);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to create thread");
            }
        }
        return thread;
    }
}