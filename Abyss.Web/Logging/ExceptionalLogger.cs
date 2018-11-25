using Abyss.Web.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;

namespace Abyss.Web.Logging
{
    public class ExceptionalLoggerProvider : ILoggerProvider
    {
        public readonly IHttpContextAccessor _httpContextAccessor;
        public ExceptionalLoggerProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ExceptionalLogger(_httpContextAccessor, categoryName);
        }

        public void Dispose()
        {
            // nothing to dispose
        }
    }


    public class ExceptionalLogger : ILogger
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _categoryName;
        public ExceptionalLogger(IHttpContextAccessor httpContextAccessor, string categoryName)
        {
            _httpContextAccessor = httpContextAccessor;
            _categoryName = categoryName;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                if (exception != null && !exception.IsLogged(LoggerType.Exceptional))
                {
                    var context = _httpContextAccessor.HttpContext;
                    var customData = new Dictionary<string, string>
                    {
                        ["Message"] = formatter(state, exception)
                    };
                    if (context != null)
                    {
                        exception.Log(context, _categoryName, customData: customData);
                    }
                    else
                    {
                        exception.LogNoContext(_categoryName, customData: customData);
                    }
                }
            }
            catch (Exception e)
            {
                // Failed to log exception, we don't want to blow up here too!
                Console.WriteLine("Failed to log exception to Exceptional:");
                Console.WriteLine(e.ToString());
                Console.WriteLine("Original exception:");
                Console.WriteLine(exception.ToString());
            }
        }
    }

    public static class ExceptionalLoggerExtensions
    {
        public static ILoggingBuilder AddExceptional(this ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.Services.AddSingleton<ILoggerProvider, ExceptionalLoggerProvider>();
            loggingBuilder.AddFilter<ExceptionalLoggerProvider>(level => true);
            return loggingBuilder;
        }
    }
}
