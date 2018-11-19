using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers
{
    public class ErrorHandlingMiddleware
    {
        private static readonly JsonSerializerSettings errorSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        private readonly RequestDelegate _next;
        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private static Task HandleException(HttpContext context, Exception exception)
        {
            try
            {
                var logger = context.RequestServices.GetService<ILogger<ErrorHandlingMiddleware>>();
                if (logger != null)
                {
                    logger.LogError(exception, nameof(ErrorHandlingMiddleware));
                }
            }
            catch
            {
                // don't want to blow up if logging the error fails
            }
            var code = GetStatusCodeForException(exception);
            var result = GetErrorResponse(exception);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }

        public static HttpStatusCode GetStatusCodeForException(Exception e)
        {
            if (e is UnauthorizedAccessException)
                return HttpStatusCode.Unauthorized;
            else
                return HttpStatusCode.InternalServerError;
        }

        public static string GetErrorResponse(Exception e)
        {
            return JsonConvert.SerializeObject(new { Error = e.Message }, errorSerializerSettings);
        }
    }

    public static class ErrorHandlingMiddlewareExtension
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
