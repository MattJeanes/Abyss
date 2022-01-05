using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Helpers;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Abyss.Web.Controllers;

[AuthorizePermission(Permissions.ServerManager)]
[Route("api/server")]
public class ServerController
{
    private readonly IServerManager _serverManager;

    public ServerController(IServerManager serverManager)
    {
        _serverManager = serverManager;
    }

    [HttpGet]
    public async Task<List<Server>> GetServers()
    {
        return await _serverManager.GetServers();
    }

    [Route("start/{serverId}")]
    [HttpPost]
    public async Task Start(string serverId)
    {
        await _serverManager.Start(serverId);
    }

    [Route("stop/{serverId}")]
    [HttpPost]
    public async Task Stop(string serverId)
    {
        await _serverManager.Stop(serverId);
    }
}
