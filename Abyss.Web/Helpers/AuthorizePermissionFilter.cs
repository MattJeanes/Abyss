using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Abyss.Web.Helpers;

public class AuthorizePermissionFilter(List<string> permissions) : IAuthorizationFilter
{
    private readonly List<string> _permissions = permissions;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = UserHelper.GetClientUser(context.HttpContext);
        if (user?.Permissions?.Any(x => _permissions.Any(y => x == y)) ?? false)
        {
            return;
        }
        context.Result = new ForbidResult();
    }
}
