using Abyss.Web.Data;
using Abyss.Web.Data.GMod;
using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Abyss.Web.Managers;

public class UserManager(
    IUserHelper userHelper,
    IUserRepository userRepository,
    IGModHelper gmodHelper,
    IHttpContextAccessor httpContextAccessor,
    IOptions<DiscordOptions> discordOptions
        ) : IUserManager
{
    private readonly IUserHelper _userHelper = userHelper;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IGModHelper _gmodHelper = gmodHelper;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly DiscordOptions _discordOptions = discordOptions.Value;

    public async Task<string> Login(AuthSchemeType schemeType)
    {
        var user = await _userHelper.GetUser();
        var (username, identifier) = GetUsernameAndIdentifier(schemeType);
        var externalUser = await _userRepository.GetByExternalIdentifier(schemeType, identifier);
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
                Name = username
            };
            user.Authentications.Add(new UserAuthentication { SchemeType = schemeType, Identifier = identifier });
            _userRepository.Add(user);
            await _userRepository.SaveChanges();
        }
        else
        {
            var userAuth = user.Authentications.FirstOrDefault(x => x.SchemeType == schemeType);
            if (userAuth != null && userAuth.Identifier != identifier)
            {
                userAuth.Identifier = identifier;
                await _userRepository.SaveChanges();
            }
            else if (userAuth == null)
            {
                user.Authentications.Add(new UserAuthentication { SchemeType = schemeType, Identifier = identifier });
                await _userRepository.SaveChanges();
            }
        }
        return await _userHelper.GetAccessToken(user);
    }

    private (string username, string identifier) GetUsernameAndIdentifier(AuthSchemeType schemeType)
    {
        if (_httpContextAccessor.HttpContext == null) { throw new Exception("HttpContext is invalid"); }
        var user = _httpContextAccessor.HttpContext.User;
        var username = user.Claims.First(x => x.Type == ClaimTypes.Name).Value;
        var identifier = user.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        if (schemeType == AuthSchemeType.Steam)
        {
            identifier = identifier.Split('/').Last();
        }

        return (username, identifier);
    }

    public async Task<string> ChangeUsername(User user, string username)
    {
        user.Name = username;
        await _userRepository.SaveChanges();
        return await _userHelper.GetAccessToken(user);
    }

    public async Task<string> DeleteAuthScheme(User user, AuthSchemeType schemeType)
    {
        var userAuth = user.Authentications.SingleOrDefault(x => x.SchemeType == schemeType);
        if (user.Authentications.Count <= 1)
        {
            throw new Exception("Cannot remove only auth provider");
        }
        else if (userAuth == null)
        {
            throw new Exception($"User does not have {schemeType} auth provider");
        }

        var steamAndDiscord = new[] { AuthSchemeType.Steam, AuthSchemeType.Discord };
        if (_gmodHelper.IsActive() && steamAndDiscord.Contains(schemeType) && steamAndDiscord.All(x => user.Authentications.Any(y => y.SchemeType == x)))
        {
            await _gmodHelper.ChangeRank(new ChangeRankDTO
            {
                Rank = _discordOptions.GuestRankId,
                MaxRankForDemote = _discordOptions.MemberRankId,
                CanDemote = true,
                SteamId64 = user.Authentications.Single(x => x.SchemeType == AuthSchemeType.Steam).Identifier
            });
        }

        user.Authentications.Remove(userAuth);
        await _userRepository.SaveChanges();
        return await _userHelper.GetAccessToken(user);
    }

    private async Task<User> MergeUsers(User userFrom, User userTo)
    {
        foreach (var auth in userFrom.Authentications)
        {
            var userAuth = userTo.Authentications.SingleOrDefault(x => x.SchemeType == auth.SchemeType);
            if (userAuth != null)
            {
                userAuth.Identifier = auth.Identifier;
            }
            else
            {
                userTo.Authentications.Add(new UserAuthentication { SchemeType = auth.SchemeType, Identifier = auth.Identifier });
            }
        }
        _userRepository.Remove(userFrom);
        await _userRepository.SaveChanges();
        return userTo;
    }

    public async Task<string> RefreshAccessToken()
    {
        if (_httpContextAccessor.HttpContext == null) { throw new Exception("HttpContext is invalid"); }
        var refreshTokenCookie = _httpContextAccessor.HttpContext.Request.Cookies.FirstOrDefault(x => x.Key == AuthSchemes.RefreshToken);
        var user = await _userHelper.VerifyRefreshToken(refreshTokenCookie.Value);
        return await _userHelper.GetAccessToken(user);
    }

    public async Task Logout(bool allSessions)
    {
        await _userHelper.Logout(allSessions);
    }
}
