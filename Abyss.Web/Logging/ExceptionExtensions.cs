using Abyss.Web.Data;
using System;

namespace Abyss.Web.Logging
{
    public static class ExceptionExtensions
    {
        public static bool IsLogged(this Exception exception, LoggerType loggerType)
        {
            var key = $"Exceptional.Logged.{loggerType}";
            var logged = exception.Data.Contains(key);
            if (!logged)
            {
                exception.Data[key] = true;
            }
            return logged;
        }
    }
}
