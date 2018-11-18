using Abyss.Web.Entities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Abyss.Web.Managers.Interfaces
{
    public interface IUserManager
    {
        Task<User> GetUser(HttpContext httpContext, string schemeId);
        Task ChangeUsername(User user, string username);
        Task DeleteAuthScheme(User user, string schemeId);
    }
}
