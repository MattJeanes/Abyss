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
        _backgroundTaskQueue.Queue(async (serviceScopeFactory, cancellationToken) =>
        {
            using var scope = serviceScopeFactory.CreateScope();
            var serverManagerHub = scope.ServiceProvider.GetRequiredService<IHubContext<ServerManagerHub>>();
            var serverManager = scope.ServiceProvider.GetRequiredService<IServerManager>();
            await _serverManager.Start(serverId, async (logItem) =>
            {
                await serverManagerHub.Clients.Clients(connectionId).SendAsync("update", logItem);
            });
        });

        return new AcceptedResult();
    }

    [Route("stop/{serverId}")]
    [HttpPost]
    public IActionResult Stop(int serverId, [Required][FromQuery] string connectionId)
    {
        _backgroundTaskQueue.Queue(async (serviceScopeFactory, cancellationToken) =>
        {
            using var scope = serviceScopeFactory.CreateScope();
            var serverManagerHub = scope.ServiceProvider.GetRequiredService<IHubContext<ServerManagerHub>>();
            var serverManager = scope.ServiceProvider.GetRequiredService<IServerManager>();
            await _serverManager.Stop(serverId, async (logItem) =>
            {
                await serverManagerHub.Clients.Clients(connectionId).SendAsync("update", logItem);
            });
        });

        return new AcceptedResult();
    }
}
