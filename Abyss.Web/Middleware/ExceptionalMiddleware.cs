using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.Extensions.Options;
using StackExchange.Exceptional;

namespace Abyss.Web.Middleware;

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
            var errorMessage = "No token specified";
            if (context.Request.Query.TryGetValue("token", out var queryToken) && CheckToken(queryToken, out errorMessage))
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
            if (context.Request.Cookies.ContainsKey(Permissions.ErrorViewer) && context.Request.Cookies.TryGetValue(Permissions.ErrorViewer, out var cookie) && CheckToken(cookie, out errorMessage))
            {
                await ExceptionalMiddleware.HandleRequestAsync(context);
                return;
            }
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync(errorMessage);
        }
        else
        {
            await _next(context);
        }
    }

    private bool CheckToken(string token, out string errorMessage)
    {
        errorMessage = "";
        if (string.IsNullOrEmpty(token))
        {
            errorMessage = "No token specified";
            return false;
        }
        var user = _userHelper.GetClientUser(token);
        if (user == null)
        {
            errorMessage = "Invalid token specified";
            return false;
        }
        if (!_userHelper.HasPermission(user, Permissions.ErrorViewer))
        {
            errorMessage = $"User does not have {Permissions.ErrorViewer} permission";
            return false;
        }
        return true;
    }
}

public static class ErrorViewerMiddlewareExtension
{
    public static IApplicationBuilder UseErrorViewer(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorViewerMiddleware>();
    }
}
