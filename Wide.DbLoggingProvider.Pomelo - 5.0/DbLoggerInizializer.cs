namespace Wide82.DbLoggingProvider
{
    public class DbLoggerInizializer
    {
        public static void Init(string connString, bool setAsyncWrite, string logPath, string applicationName)
        {
            DbLoggerWriter.SetConnectionString(connString);

            if (!string.IsNullOrEmpty(logPath))
                DbLoggerWriter.SetLogPath(logPath);

            DbLoggerWriter.SetAsyncWrite(setAsyncWrite);
            DbLoggerWriter.SetApplicationName(applicationName);
        }
    }
}