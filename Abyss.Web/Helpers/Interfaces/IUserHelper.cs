using Abyss.Web.Data;
using Abyss.Web.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers.Interfaces
{
    public interface IUserHelper
    {
        Task<User> GetUser();
        Task<User> GetUser(ClientUser clientUser);
        Task<ClientUser> GetClientUser(User user);
        Task<string> GetAccessToken(User user);
        Task<(string token, RefreshToken entity)> GetRefreshToken(User user);
        Task<RefreshToken> AddRefreshToken(User user, RefreshToken currentToken);
        ClaimsPrincipal VerifyToken(string token, TokenType type);
        Task<User> VerifyRefreshToken(string token);
        Task Logout(bool allSessions);
        Task<RefreshToken> GetCurrentRefreshToken();
    }
}
