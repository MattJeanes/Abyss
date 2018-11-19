using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;

namespace Abyss.Web.Helpers
{
    public class AuthorizePermissionFilter : IAuthorizationFilter
    {
        private readonly List<string> _permissions;

        public AuthorizePermissionFilter(List<string> permissions)
        {
            _permissions = permissions;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = UserHelper.GetClientUser(context.HttpContext);
            if (user?.Permissions.Any(x => _permissions.Any(y => x == y)) ?? false)
            {
                return;
            }
            context.Result = new ForbidResult();
        }
    }
}
