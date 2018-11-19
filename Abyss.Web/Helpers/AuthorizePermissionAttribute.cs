using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Abyss.Web.Helpers
{
    public class AuthorizePermissionAttribute : TypeFilterAttribute
    {
        private List<string> _permissions { get; set; }
        public AuthorizePermissionAttribute(string permissions) : base(typeof(AuthorizePermissionFilter))
        {
            Arguments = new object[] { permissions.Split(',').Select(x => x.Trim()).ToList() };
        }
    }
}