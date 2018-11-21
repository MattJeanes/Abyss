using Abyss.Web.Data;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Controllers
{
    [Route("api/auth")]
    public class AuthenticationController : BaseController
    {
        private readonly IUserManager _userManager;
        public AuthenticationController(IConfiguration config, IUserManager userManager, IUserHelper userHelper) : base(userHelper)
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
            return AuthSchemes.ExternalSchemes;
        }

        [Authorize(AuthenticationSchemes = AuthSchemes.ExternalLogin + "," + AuthSchemes.JsonWebToken)]
        [HttpPost]
        [Route("token/{schemeId}")]
        public async Task<AuthResult> GetToken(string schemeId)
        {
            var token = await _userManager.Login(schemeId);

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
}
