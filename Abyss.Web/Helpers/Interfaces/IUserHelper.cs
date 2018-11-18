using Abyss.Web.Data;
using Abyss.Web.Entities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers.Interfaces
{
    public interface IUserHelper
    {
        Task<User> GetUser(ClientUser clientUser);
        Task<User> GetUser(HttpContext httpContext);
        ClientUser GetClientUser(User user);
        string GetToken(User user);
    }
}
