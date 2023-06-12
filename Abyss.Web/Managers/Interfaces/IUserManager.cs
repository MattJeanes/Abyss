using Abyss.Web.Data;
using Abyss.Web.Entities;

namespace Abyss.Web.Managers.Interfaces;

public interface IUserManager
{
    Task<string> Login(AuthSchemeType schemeType);
    Task<string> ChangeUsername(User user, string username);
    Task<string> DeleteAuthScheme(User user, AuthSchemeType schemeType);
    Task<string> RefreshAccessToken();
    Task Logout(bool allSessions);
}
