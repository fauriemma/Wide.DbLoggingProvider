using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Wide82.DbLoggingProvider
{
    internal class DbLoggerWriter
    {
        private static bool unmanagedExceptionToFile = false;
        private static bool executeAsyncFlag = false;

        private static string logPath = null;
        private static string connectionString;
        private static string applicationName;

        private static Queue<DbLoggerEntry> queue = new Queue<DbLoggerEntry>();

        private static readonly ReaderWriterLock queueLock = new ReaderWriterLock();
        private static readonly ReaderWriterLock filesLock = new ReaderWriterLock();

        static readonly WaitCallback writeCallBack = new WaitCallback(WriteFromQueue);
        static readonly WaitCallback addCallBack = new WaitCallback(AddToQueue);

        private static void AddToQueue(object log)
        {
            queueLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                queue.Enqueue((DbLoggerEntry)log);

                ThreadPool.QueueUserWorkItem(writeCallBack);
            }
            finally
            {
                queueLock.ReleaseWriterLock();
            }
        }
         
        private static void WriteFromQueue(object o)
        {
            try
            {
                queueLock.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    while (queue.Count != 0)
                    {
                        ExecWriteLogToDb(queue.Peek());

                        queue.Dequeue();
                    }
                }
                catch (Exception ex)
                {
                    ExecWriteLogToFile("Timestamp " + DateTime.Now.ToString(System.Globalization.CultureInfo.CurrentCulture) + " - Error while try to write log entry: " + ex, "DbLogger_Exception");
                }
                finally
                {
                    queueLock.ReleaseWriterLock();
                }
            }
            catch { }
        }

        internal static void SetLogPath(string _logPath)
        {
            if (!Directory.Exists(_logPath))
                Directory.CreateDirectory(_logPath);

            unmanagedExceptionToFile = true;
            logPath = _logPath;
        }

        internal static void SetConnectionString(string _connectionString)
        {
            connectionString = _connectionString;
        }
        internal static void SetApplicationName(string _applicationName)
        {
            applicationName = _applicationName;
        }

        internal static void SetAsyncWrite(bool asyncWrite)
        {
            executeAsyncFlag = asyncWrite;
        }

        /// <summary>
        /// Internal method used by the logger provider to submit a new log entry.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="component">Component that have generated log</param>
        /// <param name="messageType">Message Type of log (AuditMessage, TraceMessage, DebugTraceMessage, ErrorMessage, WarningMessage)</param>
        /// <param name="severity">Severity of log entry (Critical, Error, Warning, Information, Verbose, Start, Stop, Suspend, Resume, Transfer</param>
        internal static void WriteLogEntry(string message, Exception exception, string component, DbLoggerEntryTypes messageType, TraceEventType severity)
        {
            string newMessage = message?.Replace("\n", " - ");

            if (newMessage?.EndsWith(" - ") ?? false)
                newMessage = newMessage.Remove(newMessage.Length - 3, 3);

            Exception innerEx = exception;
            while (innerEx != null)
            {
                newMessage += $"\n\n--- {(innerEx == exception ? "Original" : "Inner")} Exception Details ---\n\n";

                string innerMsg = innerEx.Message?.Replace("\n", " - ");

                if (innerMsg?.EndsWith(" - ") ?? false)
                    innerMsg = innerMsg.Remove(innerMsg.Length - 3, 3);

                newMessage += "    Message: " + innerMsg + "\n    Type: " + (innerEx?.GetType()?.ToString() ?? "NotDefined") + "\n    Stack Trace:\n    " + (innerEx?.StackTrace?.Replace("\r\n", "\n").Replace("\n", "\n    ") ?? "NoStackTrace");

                innerEx = innerEx.InnerException;
            }

            try
            {
                ExecWriteLog(new DbLoggerEntry(applicationName)
                {
                    Message = newMessage,
                    Title = component,
                    EventId = (int)messageType,
                    Severity = severity
                });
            }
            catch { }
        }


        /// <summary>
        /// If asynchronous write is set, delegate a thread to queue the log entry to the queue for writing, otherwise try to write it immediately to the database.
        /// </summary>
        /// <param name="logEntry"></param>
        private static void ExecWriteLog(DbLoggerEntry logEntry)
        {
            if (executeAsyncFlag)
            {
                ThreadPool.QueueUserWorkItem(addCallBack, logEntry);
            }
            else
            {
                ExecWriteLogToDb(logEntry);
            }
        }

        /// <summary>
        /// Try to write log entry into database, but if an error occurs, try to write it into file.
        /// </summary>
        /// <param name="logEntry"></param>
        private static void ExecWriteLogToDb(DbLoggerEntry logEntry)
        {
            try
            {
                try
                {
                    using (IDbConnection objConn = new MySqlConnection(connectionString))
                    {
                        objConn.Open();

                        using (IDbCommand objCmd = new MySqlCommand())
                        {
                            objCmd.Connection = objConn;
                            objCmd.CommandText = "WriteLog";
                            objCmd.CommandType = CommandType.StoredProcedure;

                            objCmd.Parameters.Add(new MySqlParameter("LogID", Guid.NewGuid().ToString()));
                            objCmd.Parameters.Add(new MySqlParameter("EventID", logEntry.EventId));
                            objCmd.Parameters.Add(new MySqlParameter("Priority", logEntry.Priority));
                            objCmd.Parameters.Add(new MySqlParameter("Severity", logEntry.Severity));
                            objCmd.Parameters.Add(new MySqlParameter("Title", logEntry.Title));
                            objCmd.Parameters.Add(new MySqlParameter("Timestamp", logEntry.TimeStamp));
                            objCmd.Parameters.Add(new MySqlParameter("MachineName", logEntry.MachineName));
                            objCmd.Parameters.Add(new MySqlParameter("AppDomainName", logEntry.AppDomainName));
                            objCmd.Parameters.Add(new MySqlParameter("ProcessID", logEntry.ProcessId));
                            objCmd.Parameters.Add(new MySqlParameter("ProcessName", logEntry.ProcessName));
                            objCmd.Parameters.Add(new MySqlParameter("ThreadName", logEntry.ManagedThreadName ?? ""));
                            objCmd.Parameters.Add(new MySqlParameter("Win32ThreadId", logEntry.Win32ThreadId));
                            objCmd.Parameters.Add(new MySqlParameter("Message", logEntry.Message));


                            objCmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (unmanagedExceptionToFile)
                    {
                        // Write the Original Log in the file
                        try
                        {
                            ExecWriteLogToFile(logEntry);
                        }
                        catch { }

                        // Write the Unmanaged Exception in the file
                        try
                        {
                            ExecWriteLogToFile("Timestamp " + DateTime.Now.ToString(System.Globalization.CultureInfo.CurrentCulture) + " - Error while try to write log entry: " + ex, "DbLogger_Exception");
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Try to write log entry into file.
        /// </summary>
        /// <param name="logEntry"></param>
        private static void ExecWriteLogToFile(DbLoggerEntry logEntry)
        {
            ExecWriteLogToFile("TimeStamp: " + logEntry.TimeStampString + " eventId: " + logEntry.EventId + " Title: " + logEntry.Title + " Severity: " + logEntry.Severity + " Message: " + logEntry.Message, applicationName);
        }

        /// <summary>
        /// Try to write message into file for specified type.
        /// Access to file is guarantied with ReaderWriterLock use.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        private static void ExecWriteLogToFile(string message, string messageType)
        {
            filesLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                string path = Path.Combine(logPath, messageType + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log");

                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine(message);
                }
            }
            finally
            {
                filesLock.ReleaseWriterLock();
            }
        }
    }
}