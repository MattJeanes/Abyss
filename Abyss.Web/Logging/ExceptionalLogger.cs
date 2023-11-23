using Abyss.Web.Data;
using StackExchange.Exceptional;

namespace Abyss.Web.Logging;

public class ExceptionalLoggerProvider(IHttpContextAccessor httpContextAccessor) : ILoggerProvider
{
    public readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public ILogger CreateLogger(string categoryName)
    {
        return new ExceptionalLogger(_httpContextAccessor, categoryName);
    }

    public void Dispose()
    {
        // nothing to dispose
    }
}


public class ExceptionalLogger(IHttpContextAccessor httpContextAccessor, string categoryName) : ILogger
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly string _categoryName = categoryName;

    public IDisposable BeginScope<TState>(TState state)
    {
#pragma warning disable CS8603 // Possible null reference return.
        return null;
#pragma warning restore CS8603 // Possible null reference return.
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
            if (exception != null)
            {
                Console.WriteLine("Original exception:");
                Console.WriteLine(exception.ToString());
            }
            else
            {
                Console.WriteLine("No original exception found");
            }
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
