using Abyss.Web.Data;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Abyss.Web.Managers
{
    public class UserManager : IUserManager
    {
        public readonly IConfiguration _config;
        public UserManager(IConfiguration config)
        {
            _config = config;
        }

        public string GetToken(HttpContext httpContext, string schemeId)
        {
            var user = httpContext.User;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var name = user.Claims.First(x => x.Type == ClaimTypes.Name).Value;
            var identifier = user.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            if (schemeId == AuthSchemes.Steam.Id)
            {
                identifier = identifier.Split('/').Last();
            }

            var claims = new List<Claim>
            {
                new Claim("Name", name),
                new Claim("Identifier", identifier),
                new Claim("SchemeId", schemeId)
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
