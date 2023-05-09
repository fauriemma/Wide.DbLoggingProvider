namespace Wide82.DbLoggingProvider
{
    public enum DbLoggerEntryTypes : int
    {
        AuditMessage = 100,
        TraceMessage = 101,
        DebugTraceMessage = 102,
        ErrorMessage = 103,
        WarningMessage = 104
    }
}