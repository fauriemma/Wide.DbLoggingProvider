using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Wide82.DbLoggingProvider
{
    public class DbLoggerEntry
    {
        private string message = string.Empty;
        private string title = string.Empty;
        private int priority = -1;
        private int eventId = 0;
        private TraceEventType severity = TraceEventType.Information;
        private string machineName = string.Empty;
        private DateTime timeStamp = DateTime.MaxValue;

        private StringBuilder errorMessages;
        private IDictionary<string, object> extendedProperties;

        private string appDomainName;
        private string processId;
        private string processName;
        private string threadName;
        private string win32ThreadId;

        public DbLoggerEntry(string applicationName)
        {
            MachineName = $"{applicationName}";

            CollectIntrinsicProperties();
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public int Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public int EventId
        {
            get { return eventId; }
            set { eventId = value; }
        }

        public TraceEventType Severity
        {
            get { return severity; }
            set { severity = value; }
        }

        public string LoggedSeverity
        {
            get { return severity.ToString(); }
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public DateTime TimeStamp
        {
            get { return timeStamp; }
            set { timeStamp = value; }
        }

        public string MachineName
        {
            get { return machineName; }
            set { machineName = value; }
        }

        public string AppDomainName
        {
            get { return appDomainName; }
            set { appDomainName = value; }
        }

        public string ProcessId
        {
            get { return processId; }
            set { processId = value; }
        }

        public string ProcessName
        {
            get { return processName; }
            set { processName = value; }
        }

        public string ManagedThreadName
        {
            get { return threadName; }
            set { threadName = value; }
        }

        public string Win32ThreadId
        {
            get { return win32ThreadId; }
            set { win32ThreadId = value; }
        }

        public IDictionary<string, object> ExtendedProperties
        {
            get
            {
                if (extendedProperties == null)
                {
                    extendedProperties = new Dictionary<string, object>();
                }
                return extendedProperties;
            }
            set { extendedProperties = value; }
        }

        public string TimeStampString
        {
            get { return TimeStamp.ToString(System.Globalization.CultureInfo.CurrentCulture); }
        }

        public virtual void AddErrorMessage(string message)
        {
            if (errorMessages == null)
            {
                errorMessages = new StringBuilder();
            }
            errorMessages.Insert(0, Environment.NewLine);
            errorMessages.Insert(0, Environment.NewLine);
            errorMessages.Insert(0, message);
        }

        public string ErrorMessages
        {
            get
            {
                if (errorMessages == null)
                    return null;
                else
                    return errorMessages.ToString();
            }
        }

        private void CollectIntrinsicProperties()
        {
            TimeStamp = DateTime.Now;

            try { MachineName += $" - {Environment.MachineName}"; } catch { }

            try
            {
                AppDomainName = AppDomain.CurrentDomain.FriendlyName;
            }
            catch (Exception)
            {
                AppDomainName = "";
            }

            try
            {
                processId = Process.GetCurrentProcess().Id.ToString();
            }
            catch (Exception)
            {
                processId = "";
            }

            try
            {
                processName = Process.GetCurrentProcess().ProcessName;
            }
            catch (Exception)
            {
                processName = "";
            }

            try
            {
                win32ThreadId = Environment.CurrentManagedThreadId.ToString();
            }
            catch (Exception)
            {
                win32ThreadId = "";
            }

            try
            {
                threadName = Thread.CurrentThread.Name;
            }
            catch (Exception)
            {
                threadName = "";
            }
        }
    }
}