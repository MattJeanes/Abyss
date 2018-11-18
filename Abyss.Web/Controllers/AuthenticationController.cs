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

        [HttpDelete]
        [Route("{schemeId}")]
        public async Task<AuthResult> DeleteAuthScheme(string schemeId)
        {
            var user = await GetUser();
            await _userManager.DeleteAuthScheme(user, schemeId);
            return new AuthResult
            {
                Token = _userHelper.GetToken(user)
            };
        }
    }
}
