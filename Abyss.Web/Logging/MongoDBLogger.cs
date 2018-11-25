using Abyss.Web.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;

namespace Abyss.Web.Logging
{
    public static class MongoDBLogger
    {
        private static Logger _logger;

        public static void AddMongoDB(this ILoggingBuilder loggingBuilder, string connectionString, string databaseName, string collectionName, int maxEntries, IConfiguration config)
        {
            loggingBuilder.SetMinimumLevel(LogLevel.Trace); // serilog handles levels
            _logger = GetLogger(connectionString, databaseName, collectionName, maxEntries, config);
            loggingBuilder.AddSerilog(_logger, true);
        }

        /// <summary>
        /// Flushes logs out, should be called once at end of program
        /// </summary>
        public static void FlushLogger()
        {
            if (_logger != null)
            {
                _logger.Dispose();
            }
        }

        private static Logger GetLogger(string connectionString, string databaseName, string collectionName, int maxEntries, IConfiguration config)
        {
            var logger = new LoggerConfiguration();

            // converts .NET Core standard logging config to Serilog config
            var logLevel = config.GetSection("Logging:LogLevel");
            foreach (var levelConfiguration in logLevel.GetChildren())
            {
                var level = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), levelConfiguration.Value);
                if (levelConfiguration.Key == "Default")
                {
                    logger = logger.MinimumLevel.Is(level);
                    continue;
                }
                logger = logger.MinimumLevel.Override(levelConfiguration.Key, level);
            }

            logger.Filter.ByExcluding(logEvent => logEvent.Exception != null && logEvent.Exception.IsLogged(LoggerType.MongoDB));

            var database = new MongoClient(connectionString).GetDatabase(databaseName);
            logger.WriteTo.MongoDBCapped(database, collectionName: collectionName, cappedMaxDocuments: maxEntries);

            return logger.CreateLogger();
        }
    }
}
