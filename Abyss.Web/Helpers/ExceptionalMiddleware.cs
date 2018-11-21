using Abyss.Web.Helpers.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using StackExchange.Exceptional;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers
{
    public class ErrorViewerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUserHelper _userHelper;

        public ErrorViewerMiddleware(RequestDelegate next, IUserHelper userHelper)
        {
            _next = next;
            _userHelper = userHelper;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(new PathString("/errors")))
            {
                // todo no JWT in errors page so this doesn't work not sure what to do!!
                if (_userHelper.HasPermission("ErrorViewer"))
                {
                    await ExceptionalMiddleware.HandleRequestAsync(context);
                }
                else
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Unauthorized");
                }
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
