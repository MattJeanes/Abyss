﻿using Abyss.Web.Data;
using Abyss.Web.Entities;
using System.Security.Claims;

namespace Abyss.Web.Helpers.Interfaces;

public interface IUserHelper
{
    Task<User> GetUser();
    ClientUser GetClientUser(string token);
    Task<User> GetUser(ClientUser clientUser);
    ClientUser GetClientUser(User user);
    ClientUser GetClientUser();
    Task<string> GetAccessToken(User user);
    Task<(string token, RefreshToken entity)> GetRefreshToken(User user);
    Task<RefreshToken> AddRefreshToken(User user, RefreshToken currentToken);
    ClaimsPrincipal VerifyToken(string token, TokenType type);
    Task<User> VerifyRefreshToken(string token);
    Task Logout(bool allSessions);
    Task<RefreshToken> GetCurrentRefreshToken();
    bool HasPermission(ClientUser user, string permission);
    bool HasPermission(string permission);
    Task<IList<Permission>> GetPermissions();
    Task<IList<Permission>> GetPermissions(ClientUser user);
}
