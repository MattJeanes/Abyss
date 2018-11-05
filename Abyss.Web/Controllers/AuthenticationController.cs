using Abyss.Web.Data;
using AspNet.Security.OpenId.Steam;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Web.Controllers
{
    [Route("auth")]
    public class AuthenticationController : Controller
    {
        private readonly IConfiguration _config;
        public AuthenticationController(IConfiguration config)
        {
            _config = config;
        }
        [Route("login")]
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = "/login"
            });
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [Route("token")]
        public AuthResult GetToken()
        {
            var result = new AuthResult();

            var token = BuildToken(HttpContext.User);
            result.Token = token;

            return result;
        }

        private string BuildToken(ClaimsPrincipal user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var name = user.Claims.First(x => x.Type == ClaimTypes.Name).Value;
            var steamId = user.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            steamId = steamId.Split('/').Last();

            var claims = new List<Claim>
            {
                new Claim("Name", name),
                new Claim("SteamId", steamId)
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(30),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
