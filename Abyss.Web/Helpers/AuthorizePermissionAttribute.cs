using Microsoft.AspNetCore.Mvc;

namespace Abyss.Web.Helpers;

public class AuthorizePermissionAttribute : TypeFilterAttribute
{
    public AuthorizePermissionAttribute(string permissions) : base(typeof(AuthorizePermissionFilter)) => Arguments = new object[] { permissions.Split(',').Select(x => x.Trim()).ToList() };
}
