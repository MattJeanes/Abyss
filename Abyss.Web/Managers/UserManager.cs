using Abyss.Web.Data;
using Abyss.Web.Data.GMod;
using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
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
        private readonly IUserHelper _userHelper;
        private readonly IUserRepository _userRepository;
        private readonly IGModHelper _gmodHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DiscordOptions _discordOptions;

        public UserManager(
            IUserHelper userHelper,
            IUserRepository userRepository,
            IGModHelper gmodHelper,
            IHttpContextAccessor httpContextAccessor,
            IOptions<DiscordOptions> discordOptions
            )
        {
            _userHelper = userHelper;
            _userRepository = userRepository;
            _gmodHelper = gmodHelper;
            _httpContextAccessor = httpContextAccessor;
            _discordOptions = discordOptions.Value;
        }

        public async Task<string> Login(string schemeId)
        {
            var user = await _userHelper.GetUser();
            var (username, identifier) = GetUsernameAndIdentifier(schemeId);
            var externalUser = await _userRepository.GetByExternalIdentifier(schemeId, identifier);
            if (externalUser != null)
            {
                if (user == null)
                {
                    user = externalUser;
                }
                else if (user.Id != externalUser.Id)
                {
                    user = await MergeUsers(user, externalUser);
                }
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
            return await _userHelper.GetAccessToken(user);
        }

        private (string username, string identifier) GetUsernameAndIdentifier(string schemeId)
        {
            var user = _httpContextAccessor.HttpContext.User;
            var username = user.Claims.First(x => x.Type == ClaimTypes.Name).Value;
            var identifier = user.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            if (schemeId == AuthSchemes.Steam.Id)
            {
                identifier = identifier.Split('/').Last();
            }

            return (username, identifier);
        }

        public async Task<string> ChangeUsername(User user, string username)
        {
            user.Name = username;
            await _userRepository.Update(user);
            return await _userHelper.GetAccessToken(user);
        }

        public async Task<string> DeleteAuthScheme(User user, string schemeId)
        {
            if (user.Authentication.Count <= 1)
            {
                throw new Exception("Cannot remove only auth provider");
            }
            else if (!user.Authentication.ContainsKey(schemeId))
            {
                throw new Exception($"User does not have {schemeId} auth provider");
            }

            var steamAndDiscord = new[] { AuthSchemes.Discord.Id, AuthSchemes.Steam.Id };
            if (_gmodHelper.IsActive() && steamAndDiscord.Contains(schemeId) && steamAndDiscord.All(x => user.Authentication.ContainsKey(x)))
            {
                await _gmodHelper.ChangeRank(new ChangeRankDTO
                {
                    Rank = _discordOptions.GuestRankId,
                    MaxRankForDemote = _discordOptions.MemberRankId,
                    CanDemote = true,
                    SteamId64 = user.Authentication[AuthSchemes.Steam.Id]
                });
            }

            user.Authentication.Remove(schemeId);
            await _userRepository.Update(user);
            return await _userHelper.GetAccessToken(user);
        }

        private async Task<User> MergeUsers(User userFrom, User userTo)
        {
            userFrom.Authentication.ToList().ForEach(x => userTo.Authentication[x.Key] = x.Value);
            await _userRepository.Update(userTo);
            await _userRepository.Remove(userFrom);
            return userTo;
        }

        public async Task<string> RefreshAccessToken()
        {
            var refreshTokenCookie = _httpContextAccessor.HttpContext.Request.Cookies.FirstOrDefault(x => x.Key == AuthSchemes.RefreshToken);
            var user = await _userHelper.VerifyRefreshToken(refreshTokenCookie.Value);
            return await _userHelper.GetAccessToken(user);
        }

        public async Task Logout(bool allSessions)
        {
            await _userHelper.Logout(allSessions);
        }
    }
}
