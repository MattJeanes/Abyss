using Abyss.Web.Entities;

namespace Abyss.Web.Managers.Interfaces;

public interface IUserManager
{
    Task<string> Login(string schemeId);
    Task<string> ChangeUsername(User user, string username);
    Task<string> DeleteAuthScheme(User user, string schemeId);
    Task<string> RefreshAccessToken();
    Task Logout(bool allSessions);
}
