using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Abyss.Web.Helpers;

public class UserHelper : IUserHelper
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRepository<Permission> _permissionRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly AuthenticationOptions _authenticationOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private const string UserClaimField = "User";
    private const string RefreshExpiryField = "RefreshExpiry";
    private const string RefreshTokenIdField = "uid";

    public UserHelper(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRepository<Permission> permissionRepository,
        IOptions<JwtOptions> jwtOptions,
        IOptions<AuthenticationOptions> authenticationOptions,
        IHttpContextAccessor httpContextAccessor,
        IRepository<RefreshToken> refreshTokenRepository
        )
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _jwtOptions = jwtOptions.Value;
        _authenticationOptions = authenticationOptions.Value;
        _httpContextAccessor = httpContextAccessor;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public ClientUser GetClientUser(User user)
    {
        return new ClientUser
        {
            Id = user.Id,
            Name = user.Name,
            Authentication = user.Authentications.ToDictionary(x => (int)x.SchemeType, x => x.Identifier),
            Permissions = user?.Role?.Permissions.Select(x => x.Identifier).ToList() ?? new List<string>(),
            RoleId = user.RoleId
        };
    }

    public async Task<User> GetUser(ClientUser clientUser)
    {
        if (clientUser.Id == default) { throw new Exception("Invalid id"); }
        var user = await _userRepository.GetById(clientUser.Id);
        return user;
    }

    public async Task<User> GetUser()
    {
        var clientUser = GetClientUser();
        if (clientUser != null)
        {
            var user = await GetUser(clientUser);
            return user;
        }
        return null;
    }

    public static ClientUser GetClientUser(ClaimsPrincipal user)
    {
        var encodedUser = user?.Claims.FirstOrDefault(x => x.Type == UserClaimField) ?? null;
        if (encodedUser != null)
        {
            return JsonSerializer.Deserialize<ClientUser>(encodedUser.Value);
        }
        return null;
    }

    public static ClientUser GetClientUser(HttpContext httpContext)
    {
        if (httpContext == null) { return null; }
        return GetClientUser(httpContext.User);
    }

    public ClientUser GetClientUser()
    {
        return GetClientUser(_httpContextAccessor.HttpContext);
    }

    public ClientUser GetClientUser(string token)
    {
        var jwt = VerifyToken(token, TokenType.Access);
        return GetClientUser(jwt);
    }

    public async Task<RefreshToken> AddRefreshToken(User user, RefreshToken currentToken)
    {
        var (refreshToken, entity) = await GetRefreshToken(user);
        if (currentToken != null)
        {
            currentToken.Revoked = true;
            await _refreshTokenRepository.SaveChanges();
        }
        if (_httpContextAccessor.HttpContext == null) { throw new Exception("HttpContext is invalid"); }
        _httpContextAccessor.HttpContext.Response.Cookies.Append(AuthSchemes.RefreshToken, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Strict,
            Secure = true,
            Expires = entity.Expiry
        });
        return entity;
    }

    public async Task<string> GetAccessToken(User user)
    {
        var refreshToken = await GetCurrentRefreshToken();
        if (NeedsNewRefreshToken(refreshToken) || refreshToken == null)
        {
            refreshToken = await AddRefreshToken(user, refreshToken);
        }
        var clientUser = GetClientUser(user);
        var clientUserSerialized = JsonSerializer.Serialize(clientUser);
        var claims = new List<Claim>
            {
                new Claim(UserClaimField, clientUserSerialized),
                new Claim(RefreshExpiryField, ((DateTimeOffset)refreshToken.Expiry).ToUnixTimeSeconds().ToString()),
                new Claim(RefreshTokenIdField, refreshToken.Id.ToString())
            };

        return GetToken(claims, DateTime.UtcNow.AddMinutes(_authenticationOptions.AccessToken.ValidMinutes), TokenType.Access);
    }

    private bool NeedsNewRefreshToken(RefreshToken token)
    {
        if (token == null) { return true; }
        var renewAfter = token.FromDate.AddMinutes(token.Expiry.Subtract(token.FromDate).TotalMinutes / 2);
        if (DateTime.UtcNow > renewAfter)
        {
            return true;
        }
        return false;
    }

    public async Task<(string token, RefreshToken entity)> GetRefreshToken(User user)
    {
        var expiry = DateTime.UtcNow.AddMinutes(_authenticationOptions.RefreshToken.ValidMinutes);
        var entity = new RefreshToken
        {
            FromDate = DateTime.UtcNow,
            Expiry = expiry,
            UserId = user.Id
        };
        _refreshTokenRepository.Add(entity);
        await _refreshTokenRepository.SaveChanges();

        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(RefreshTokenIdField, entity.Id.ToString())
            };

        var token = GetToken(claims, expiry, TokenType.Refresh);
        return (token, entity);
    }

    private string GetToken(List<Claim> claims, DateTime expires, TokenType type)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(_jwtOptions.Issuer,
          type.ToString(),
          claims,
          expires: expires,
          signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal VerifyToken(string token, TokenType type)
    {
        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = type.ToString(),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key)),
                ClockSkew = TimeSpan.FromHours(DateTime.Now.Hour - DateTimeOffset.UtcNow.Hour)
            };
            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }

    public async Task<User> VerifyRefreshToken(string token)
    {
        var claimsPrincipal = VerifyToken(token, TokenType.Refresh);
        if (claimsPrincipal != null)
        {
            var id = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Invalid refresh token");
            var dbId = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == RefreshTokenIdField)?.Value ?? throw new Exception("Invalid refresh token");
            var dbToken = await _refreshTokenRepository.GetById(int.Parse(dbId));
            if (dbToken?.Revoked ?? false) { throw new Exception("Token is revoked"); }
            var user = await _userRepository.GetById(int.Parse(id));
            if (user == null)
            {
                throw new Exception("Invalid user in refresh token");
            }
            return user;
        }
        else
        {
            throw new Exception("Invalid token");
        }
    }

    public async Task Logout(bool allSessions)
    {
        var currentToken = await GetCurrentRefreshToken();
        if (currentToken != null)
        {
            if (allSessions)
            {
                var userTokens = await _refreshTokenRepository.GetAll().Where(x => x.UserId == currentToken.UserId && !x.Revoked).ToListAsync();
                foreach (var token in userTokens)
                {
                    token.Revoked = true;
                    await _refreshTokenRepository.SaveChanges();
                }
            }
            else
            {
                currentToken.Revoked = true;
                await _refreshTokenRepository.SaveChanges();
            }
        }

        if (_httpContextAccessor.HttpContext == null) { throw new Exception("HttpContext is invalid"); }
        _httpContextAccessor.HttpContext.Response.Cookies.Delete(AuthSchemes.RefreshToken);
    }

    public async Task<RefreshToken> GetCurrentRefreshToken()
    {
        if (_httpContextAccessor.HttpContext == null) { throw new Exception("HttpContext is invalid"); }
        var refreshTokenId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == RefreshTokenIdField);
        if (refreshTokenId != null)
        {
            var refreshToken = await _refreshTokenRepository.GetById(int.Parse(refreshTokenId.Value));
            return refreshToken;
        }
        return null;
    }

    public bool HasPermission(ClientUser user, string permission)
    {
        return user?.Permissions?.Contains(permission) ?? false;
    }

    public bool HasPermission(string permission)
    {
        var user = GetClientUser();
        return user != null && HasPermission(user, permission);
    }

    public async Task<IList<Permission>> GetPermissions()
    {
        var user = GetClientUser();
        return await GetPermissions(user);
    }

    public async Task<IList<Permission>> GetPermissions(ClientUser user)
    {
        if (!user.RoleId.HasValue) { return new List<Permission>(); }
        var role = await _roleRepository.GetById(user.RoleId.Value);
        return role.Permissions;
    }
}
