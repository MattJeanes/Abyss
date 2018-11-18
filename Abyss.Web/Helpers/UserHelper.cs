using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Abyss.Web.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtOptions _jwtOptions;
        private const string UserClaimField = "user";

        public UserHelper(IUserRepository userRepository, IOptions<JwtOptions> options)
        {
            _userRepository = userRepository;
            _jwtOptions = options.Value;
        }

        public ClientUser GetClientUser(User user)
        {
            return new ClientUser
            {
                Id = user.Id.ToString(),
                Name = user.Name
            };
        }

        public async Task<User> GetUser(ClientUser clientUser)
        {
            var user = await _userRepository.GetById(clientUser.Id);
            return user;
        }

        public async Task<User> GetUser(HttpContext httpContext)
        {
            var encodedUser = httpContext.User.Claims.FirstOrDefault(x => x.Type == UserClaimField);
            if (encodedUser != null)
            {
                var clientUser = JsonConvert.DeserializeObject<ClientUser>(encodedUser.Value);
                var user = await GetUser(clientUser);
                return user;
            }
            return null;
        }

        public string GetToken(User user)
        {
            var clientUser = GetClientUser(user);
            var clientUserSerialized = JsonConvert.SerializeObject(clientUser);
            var claims = new List<Claim>
            {
                new Claim(UserClaimField, clientUserSerialized),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_jwtOptions.Issuer,
              _jwtOptions.Issuer,
              claims,
              expires: DateTime.Now.AddMinutes(_jwtOptions.ValidMinutes),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
