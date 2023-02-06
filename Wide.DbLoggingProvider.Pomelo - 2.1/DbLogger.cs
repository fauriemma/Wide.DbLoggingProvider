using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Wide82.DbLoggingProvider
{
    public class DbLogger : ILogger
    {
        private readonly DbLoggerProvider _dbLoggerProvider;
        private readonly string _categoryName;

        public DbLogger(DbLoggerProvider dbLoggerProvider, string categoryName)
        {
            _dbLoggerProvider = dbLoggerProvider;

            _categoryName = categoryName;
        }


        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                // Don't log the entry if it's not enabled.
                return;
            }

            DbLoggerEntryTypes messageType;
            TraceEventType severity;

            switch (logLevel)
            {
                case LogLevel.Critical:
                    messageType = DbLoggerEntryTypes.ErrorMessage;
                    severity = TraceEventType.Critical;
                    break;

                case LogLevel.Debug:
                    messageType = DbLoggerEntryTypes.DebugTraceMessage;
                    severity = TraceEventType.Verbose;
                    break;

                case LogLevel.Error:
                    messageType = DbLoggerEntryTypes.ErrorMessage;
                    severity = TraceEventType.Error;
                    break;

                case LogLevel.Information:
                    messageType = DbLoggerEntryTypes.AuditMessage;
                    severity = TraceEventType.Information;
                    break;

                case LogLevel.Trace:
                    messageType = DbLoggerEntryTypes.TraceMessage;
                    severity = TraceEventType.Verbose;
                    break;

                case LogLevel.Warning:
                    messageType = DbLoggerEntryTypes.WarningMessage;
                    severity = TraceEventType.Warning;
                    break;

                default:
                    messageType = DbLoggerEntryTypes.TraceMessage;
                    severity = TraceEventType.Verbose;
                    break;
            }

            DbLoggerWriter.WriteLogEntry(!string.IsNullOrWhiteSpace(formatter(state, null)) ? formatter(state, null) : "", exception, _categoryName, messageType, severity);
        }
    }
}