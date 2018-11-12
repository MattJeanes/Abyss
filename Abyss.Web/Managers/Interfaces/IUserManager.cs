using Microsoft.AspNetCore.Http;

namespace Abyss.Web.Managers.Interfaces
{
    public interface IUserManager
    {
        string GetToken(HttpContext httpContext, string schemeId);
    }
}
