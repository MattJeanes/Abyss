using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Helpers;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Hubs;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;

namespace Abyss.Web.Controllers;

[AuthorizePermission(Permissions.ServerManager)]
[Route("api/server")]
public class ServerController(IServerManager serverManager, IBackgroundTaskQueue backgroundTaskQueue, IUserHelper userHelper) : BaseController(userHelper)
{
    private readonly IServerManager _serverManager = serverManager;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue = backgroundTaskQueue;

    [HttpGet]
    public async Task<List<Server>> GetServers()
    {
        return await _serverManager.GetServers();
    }

    [Route("start/{serverId}")]
    [HttpPost]
    public async Task<IActionResult> Start(int serverId, [Required][FromQuery] string connectionId)
    {
        var user = await GetUser();
        _backgroundTaskQueue.Queue(async (serviceProvider, cancellationToken) =>
        {
            var serverManagerHub = serviceProvider.GetRequiredService<IHubContext<ServerManagerHub>>();
            var scopedServerManager = serviceProvider.GetRequiredService<IServerManager>();
            var client = serverManagerHub.Clients.Clients(connectionId);
            string err = null;
            try
            {
                await scopedServerManager.Start(serverId, user, async (logItem) =>
                {
                    await client.SendAsync("log", logItem.Message);
                }, async (status) =>
                {
                    await client.SendAsync("status", status);
                });
            }
            catch (Exception e)
            {
                err = e.Message;
            }
            await client.SendAsync("complete", err);
        });

        return new AcceptedResult();
    }

    [Route("stop/{serverId}")]
    [HttpPost]
    public async Task<IActionResult> Stop(int serverId, [Required][FromQuery] string connectionId)
    {
        var user = await GetUser();
        _backgroundTaskQueue.Queue(async (serviceProvider, cancellationToken) =>
        {
            var serverManagerHub = serviceProvider.GetRequiredService<IHubContext<ServerManagerHub>>();
            var scopedServerManager = serviceProvider.GetRequiredService<IServerManager>();
            var client = serverManagerHub.Clients.Clients(connectionId);
            string err = null;
            try
            {
                await scopedServerManager.Stop(serverId, user, async (logItem) =>
                {
                    await client.SendAsync("log", logItem.Message);
                }, async (status) =>
                {
                    await client.SendAsync("status", status);
                });
            }
            catch (Exception e)
            {
                err = e.Message;
            }
            await client.SendAsync("complete", err);
        });

        return new AcceptedResult();
    }

    [Route("command/{serverId}")]
    [HttpPost]
    [AuthorizePermission(Permissions.ServerCommand)]
    public async Task<IActionResult> ExecuteCommand(int serverId, [FromBody] string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return new BadRequestObjectResult("Command cannot be empty");
        }

        try 
        {
            var user = await GetUser();
            var response = await _serverManager.ExecuteCommand(serverId, command, user);
            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}
