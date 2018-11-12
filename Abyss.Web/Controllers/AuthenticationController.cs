using Abyss.Web.Data;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Abyss.Web.Controllers
{
    [Route("auth")]
    public class AuthenticationController : Controller
    {
        private readonly IUserManager _userManager;
        public AuthenticationController(IConfiguration config, IUserManager userManager)
        {
            _userManager = userManager;
        }

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
            return AuthSchemes.All;
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [Route("token/{schemeId}")]
        public AuthResult GetToken(string schemeId)
        {
            var result = new AuthResult();

            var token = _userManager.GetToken(HttpContext, schemeId);
            result.Token = token;

            return result;
        }
    }
}
