using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abyss.Web.Managers
{
    public class UserManager : IUserManager
    {
        public readonly IUserHelper _userHelper;
        private readonly IUserRepository _userRepository;

        public UserManager(IUserHelper userHelper, IUserRepository userRepository)
        {
            _userHelper = userHelper;
            _userRepository = userRepository;
        }

        public async Task<User> GetUser(HttpContext httpContext, string schemeId)
        {
            var user = await _userHelper.GetUser(httpContext);

            var (username, identifier) = GetUsernameAndIdentifier(httpContext, schemeId);

            if (user == null)
            {
                user = await _userRepository.GetByOAuthIdentifierAsync(schemeId, identifier);
            }
            if (user == null)
            {
                user = new User
                {
                    Name = username,
                    Authentication = new Dictionary<string, string>
                    {
                        [schemeId] = identifier
                    }
                };
                await _userRepository.Add(user);
            }
            else if (!user.Authentication.ContainsKey(schemeId) || user.Authentication[schemeId] != identifier)
            {
                user.Authentication[schemeId] = identifier;
                await _userRepository.Update(user);
            }

            return user;
        }

        private (string username, string identifier) GetUsernameAndIdentifier(HttpContext httpContext, string schemeId)
        {
            var user = httpContext.User;
            var username = user.Claims.First(x => x.Type == ClaimTypes.Name).Value;
            var identifier = user.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            if (schemeId == AuthSchemes.Steam.Id)
            {
                identifier = identifier.Split('/').Last();
            }

            return (username, identifier);
        }

        public async Task ChangeUsername(User user, string username)
        {
            user.Name = username;
            await _userRepository.Update(user);
        }

        public async Task DeleteAuthScheme(User user, string schemeId)
        {
            if (user.Authentication.Count <= 1)
            {
                throw new Exception("Cannot remove only auth provider");
            }
            else if (!user.Authentication.ContainsKey(schemeId))
            {
                throw new Exception($"User does not have {schemeId} auth provider");
            }
            user.Authentication.Remove(schemeId);
            await _userRepository.Update(user);
        }
    }
}
