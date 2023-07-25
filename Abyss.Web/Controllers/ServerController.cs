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
public class ServerController
{
    private readonly IServerManager _serverManager;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public ServerController(IServerManager serverManager, IBackgroundTaskQueue backgroundTaskQueue)
    {
        _serverManager = serverManager;
        _backgroundTaskQueue = backgroundTaskQueue;
    }

    [HttpGet]
    public async Task<List<Server>> GetServers()
    {
        return await _serverManager.GetServers();
    }

    [Route("start/{serverId}")]
    [HttpPost]
    public IActionResult Start(int serverId, [Required][FromQuery] string connectionId)
    {
        _backgroundTaskQueue.Queue(async (serviceProvider, cancellationToken) =>
        {
            var serverManagerHub = serviceProvider.GetRequiredService<IHubContext<ServerManagerHub>>();
            var scopedServerManager = serviceProvider.GetRequiredService<IServerManager>();
            var client = serverManagerHub.Clients.Clients(connectionId);
            string err = null;
            try
            {
                await scopedServerManager.Start(serverId, async (logItem) =>
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
    public IActionResult Stop(int serverId, [Required][FromQuery] string connectionId)
    {
        _backgroundTaskQueue.Queue(async (serviceProvider, cancellationToken) =>
        {
            var serverManagerHub = serviceProvider.GetRequiredService<IHubContext<ServerManagerHub>>();
            var scopedServerManager = serviceProvider.GetRequiredService<IServerManager>();
            var client = serverManagerHub.Clients.Clients(connectionId);
            string err = null;
            try
            {
                await scopedServerManager.Stop(serverId, async (logItem) =>
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
}
