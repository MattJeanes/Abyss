using Abyss.Web.Commands.Discord.ChoiceProviders;
using Abyss.Web.Data;
using Abyss.Web.Managers.Interfaces;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using System.ComponentModel;

namespace Abyss.Web.Commands.Discord;

[Command("server"), Description("View or manage servers")]
public class ServerCommand(IServiceProvider serviceProvider, IServerManager serverManager, ILogger<ServerCommand> logger) : BaseCommand(serviceProvider)
{
    private readonly IServerManager _serverManager = serverManager;
    private readonly ILogger<ServerCommand> _logger = logger;

    [Command("status"), Description("Get server status")]
    public async Task GetStatus(CommandContext ctx, [SlashChoiceProvider<ServerChoiceProvider>, Description("Server name")] string name)
    {
        await ctx.DeferResponseAsync();

        var server = await _serverManager.GetServerByAlias(name);
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

    [Command("start"), Description("Start a server")]
    public async Task StartServer(CommandContext ctx, [SlashChoiceProvider<ServerChoiceProvider>, Description("Server name")] string name)
    {
        await ctx.DeferResponseAsync();

        if (!await CheckPermission(ctx, Permissions.ServerManager))
        {
            return;
        }

        var server = await _serverManager.GetServerByAlias(name);
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
            await _serverManager.Start(server.Id, async (logItem) =>
            {
                if (thread == null)
                {
                    await ctx.FollowupAsync(logItem.ToString());
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
                await ctx.FollowupAsync($"{response} {ctx.User.Mention}");
            }
            else
            {
                await thread.SendMessageAsync($"{response} {ctx.User.Mention}");
            }
        }
    }

    [Command("stop"), Description("Stop a server")]
    public async Task StopServer(CommandContext ctx, [SlashChoiceProvider<ServerChoiceProvider>, Description("Server name")] string name)
    {
        await ctx.DeferResponseAsync();

        if (!await CheckPermission(ctx, Permissions.ServerManager))
        {
            return;
        }

        var server = await _serverManager.GetServerByAlias(name);
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
            await _serverManager.Stop(server.Id, async (logItem) =>
            {
                if (thread == null)
                {
                    await ctx.FollowupAsync(logItem.ToString());
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
                await ctx.FollowupAsync($"{response} {ctx.User.Mention}");
            }
            else
            {
                await thread.SendMessageAsync($"{response} {ctx.User.Mention}");
            }
        }
    }

    [Command("restart"), Description("Restart a server, also updates it")]
    public async Task RestartServer(CommandContext ctx, [SlashChoiceProvider<ServerChoiceProvider>, Description("Server name")] string name)
    {
        await ctx.DeferResponseAsync();

        if (!await CheckPermission(ctx, Permissions.ServerManager))
        {
            return;
        }

        var server = await _serverManager.GetServerByAlias(name);
        if (server == null)
        {
            await ctx.EditResponseAsync("Server not found");
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
            await _serverManager.Restart(server.Id, async (logItem) =>
            {
                if (thread == null)
                {
                    await ctx.FollowupAsync(logItem.ToString());
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
                await ctx.FollowupAsync($"{response} {ctx.User.Mention}");
            }
            else
            {
                await thread.SendMessageAsync($"{response} {ctx.User.Mention}");
            }
        }
    }

    [Command("command"), Description("Run a command on a server")]
    public async Task ExecuteCommand(CommandContext ctx, 
        [SlashChoiceProvider<ServerChoiceProvider>, Description("Server name")] string name, 
        [Description("Command to execute")] string command)
    {
        await ctx.DeferResponseAsync();

        if (!await CheckPermission(ctx, Permissions.ServerCommand))
        {
            return;
        }

        var server = await _serverManager.GetServerByAlias(name);
        if (server == null)
        {
            await ctx.EditResponseAsync("Server not found");
            return;
        }
        
        if (server.StatusId != ServerStatus.Active)
        {
            await ctx.EditResponseAsync($"Cannot execute command on server: {FormatServerStatus(server)}");
            return;
        }

        try
        {
            var response = await _serverManager.ExecuteCommand(server.Id, command);
            await ctx.EditResponseAsync($"```\n> {command}\n{response}\n```");
        }
        catch (Exception ex)
        {
            await ctx.EditResponseAsync($"Failed to execute command: {ex.Message}");
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

        if (richStatus == null)
        {
            return status;
        }
        if (!string.IsNullOrEmpty(richStatus.Error))
        {
            status += $". Failed to retrieve server rich status: {richStatus.Error}";
            return status;
        }
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

        return status;
    }

    private async Task<DiscordThreadChannel> TryCreateThread(CommandContext ctx, string name)
    {
        DiscordThreadChannel thread = null;
        if (!ctx.Channel.IsPrivate)
        {
            try
            {
                var response = await ctx.GetResponseAsync();
                var channel = await ctx.Client.GetChannelAsync(response.ChannelId);
                thread = await channel.CreateThreadAsync(response, name, DiscordAutoArchiveDuration.Hour);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to create thread");
            }
        }
        return thread;
    }
}