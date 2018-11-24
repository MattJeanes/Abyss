using Abyss.Web.Data;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using StackExchange.Exceptional;
using System;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers
{
    public class ErrorViewerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUserHelper _userHelper;
        private readonly AuthenticationOptions _options;

        public ErrorViewerMiddleware(RequestDelegate next, IUserHelper userHelper, IOptions<AuthenticationOptions> options)
        {
            _next = next;
            _userHelper = userHelper;
            _options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(new PathString("/errors")))
            {
                if (context.Request.Query.TryGetValue("token", out var queryToken))
                {
                    var user = _userHelper.GetClientUser(queryToken);
                    if (_userHelper.HasPermission(user, Permissions.ErrorViewer))
                    {
                        context.Response.Cookies.Append(Permissions.ErrorViewer, queryToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Expires = DateTime.UtcNow.AddMinutes(_options.AccessToken.ValidMinutes),
                            IsEssential = true,
                            Secure = true
                        });
                        context.Response.Redirect("/errors");
                        return;
                    }
                }
                if (context.Request.Cookies.ContainsKey(Permissions.ErrorViewer))
                {
                    if (context.Request.Cookies.TryGetValue(Permissions.ErrorViewer, out var cookie))
                    {
                        var user = _userHelper.GetClientUser(cookie);
                        if (_userHelper.HasPermission(user, Permissions.ErrorViewer))
                        {
                            await ExceptionalMiddleware.HandleRequestAsync(context);
                            return;
                        }
                    }
                }
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("No token specified");
            }
            else
            {
                await _next(context);
            }
        }
    }

    public static class ErrorViewerMiddlewareExtension
    {
        public static IApplicationBuilder UseErrorViewer(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorViewerMiddleware>();
        }
    }
}
