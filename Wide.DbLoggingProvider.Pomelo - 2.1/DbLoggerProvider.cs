using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Wide82.DbLoggingProvider
{
    [ProviderAlias("Database")]
    public class DbLoggerProvider : ILoggerProvider
    {
        public readonly DbLoggerOptions Options;

        public DbLoggerProvider(IOptions<DbLoggerOptions> _options)
        {
            Options = _options.Value; // Stores all the options.
            DbLoggerInizializer.Init(Options.ConnectionString, Options.AsyncWrite, Options.LogPath, Options.ApplicationName);
        }

        /// <summary>
        /// Creates a new instance of the db logger.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            return new DbLogger(this, categoryName);
        }

        public void Dispose()
        {
        }
    }
}
