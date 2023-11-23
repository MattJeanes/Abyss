using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.Extensions.Options;
using StackExchange.Exceptional;

namespace Abyss.Web.Middleware;

public class ErrorViewerMiddleware(RequestDelegate next, IServiceProvider serviceProvider, IOptions<AuthenticationOptions> options)
{
    private readonly RequestDelegate _next = next;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly AuthenticationOptions _options = options.Value;

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
        using var scope = _serviceProvider.CreateScope();
        var userHelper = scope.ServiceProvider.GetRequiredService<IUserHelper>();
        var user = userHelper.GetClientUser(token);
        if (user == null)
        {
            errorMessage = "Invalid token specified";
            return false;
        }
        if (!userHelper.HasPermission(user, Permissions.ErrorViewer))
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
