using Abyss.Web.Data;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Abyss.Web.Controllers;

[Route("api/auth")]
public class AuthenticationController(IUserManager userManager, IUserHelper userHelper) : BaseController(userHelper)
{
    private readonly IUserManager _userManager = userManager;

    [Route("login/{schemeId}")]
    public IActionResult Login(string schemeId)
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = $"/login/{schemeId}"
        }, schemeId);
    }

    [Route("schemes")]
    public List<AuthScheme> GetSchemes()
    {
        return AuthSchemes.ExternalSchemes;
    }

    [Authorize(AuthenticationSchemes = AuthSchemes.ExternalLogin + "," + AuthSchemes.JsonWebToken)]
    [HttpPost]
    [Route("token/{schemeType}")]
    public async Task<AuthResult> GetToken(AuthSchemeType schemeType)
    {
        var token = await _userManager.Login(schemeType);

        return new AuthResult
        {
            Token = token
        };
    }

    [HttpPost]
    [Route("token")]
    public async Task<AuthResult> RefreshToken()
    {
        var token = await _userManager.RefreshAccessToken();

        return new AuthResult
        {
            Token = token
        };
    }

    [HttpDelete]
    [Route("{allSessions:bool?}")]
    public async Task<bool> Logout(bool? allSessions)
    {
        await _userManager.Logout(allSessions ?? false);
        return true;
    }

    [HttpGet]
    [Route("error")]
    public void Error()
    {
        throw new System.Exception("oh no");
    }
}
