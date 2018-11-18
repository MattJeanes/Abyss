using Abyss.Web.Data;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Abyss.Web.Controllers
{
    [Route("auth")]
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
            return AuthSchemes.All;
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme)]
        [Route("token/{schemeId}")]
        public async Task<AuthResult> GetToken(string schemeId)
        {
            var result = new AuthResult();

            var user = await _userManager.GetUser(HttpContext, schemeId);

            var token = _userHelper.GetToken(user);
            result.Token = token;

            return result;
        }

        [Authorize]
        [Route("username")]
        [HttpPost]
        public async Task<AuthResult> ChangeUsername()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var username = await reader.ReadToEndAsync();
                var user = await GetUser();
                await _userManager.ChangeUsername(user, username);
                var newToken = _userHelper.GetToken(user);
                return new AuthResult
                {
                    Token = newToken
                };
            }
        }
    }
}
