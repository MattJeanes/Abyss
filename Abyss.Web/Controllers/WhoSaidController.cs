using Abyss.Web.Data;
using Abyss.Web.Helpers;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Abyss.Web.Controllers;

[AuthorizePermission(Permissions.WhoSaid)]
[Route("api/whosaid")]
public class WhoSaidController(IWhoSaidManager whoSaidManager)
{
    private readonly IWhoSaidManager _whoSaidManager = whoSaidManager;

    [HttpPost]
    public async Task<WhoSaid> WhoSaid([FromBody] string message)
    {
        return await _whoSaidManager.WhoSaid(message);
    }
}
