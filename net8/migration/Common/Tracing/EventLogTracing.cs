// <copyright file="EventLogTracing.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Versioning;

    /// <summary>
    /// A class for writing warning and error traces to a Windows event log.
    /// </summary>    
    public class EventLogTracing
    {
        private string logSource;
        private string logName;
        private bool diagnosticTrace;

        public EventLogTracing(string logName, string logSource) :
            this(logName, logSource, false)
        {
        }

        public EventLogTracing(string logName, string logSource, bool diagnosticTrace)
        {
            if (string.IsNullOrWhiteSpace(logName))
            {
                throw new ArgumentException("Argument can not be null or whitespace.", "logName");
            }

            if (string.IsNullOrWhiteSpace(logSource))
            {
                throw new ArgumentException("Argument can not be null or whitespace.", "logSource");
            }

            this.logSource = logSource;
            this.logName = logName;
            this.diagnosticTrace = diagnosticTrace;
        }

        public void Information(string message, params object[] args)
        {
#if WINDOWS
            this.Trace(message, EventLogEntryType.Information, args);
#else
            this.Trace(message, 0, args); // 0 is a placeholder, will not be used
#endif
        }

        public void Warning(string message, params object[] args)
        {
#if WINDOWS
            this.Trace(message, EventLogEntryType.Warning, args);
#else
            this.Trace(message, 0, args);
#endif
        }

        public void Error(string message, params object[] args)
        {
#if WINDOWS
            this.Trace(message, EventLogEntryType.Error, args);
#else
            this.Trace(message, 0, args);
#endif
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Ignoring as these are actually used in specific diagnostic flow.")]
        public void Verbose(string message, params object[] args)
        {
            if (this.diagnosticTrace)
            {
                System.Diagnostics.Trace.WriteLine(string.Format(message, args));
            }
        }

        [SupportedOSPlatform("windows")]
        private void TraceWindows(string message, EventLogEntryType eventType, params object[] args)
        {
            if (this.diagnosticTrace)
            {
                switch (eventType)
                {
                    case EventLogEntryType.Error:
                    case EventLogEntryType.FailureAudit:
                        System.Diagnostics.Trace.TraceError(string.Format(message, args));
                        break;
                    case EventLogEntryType.Warning:
                        System.Diagnostics.Trace.TraceWarning(string.Format(message, args));
                        break;
                    case EventLogEntryType.SuccessAudit:
                    case EventLogEntryType.Information:
                        System.Diagnostics.Trace.TraceInformation(string.Format(message, args));
                        break;
                    default:
                        System.Diagnostics.Trace.WriteLine(string.Format(message, args));
                        break;
                }
            }

            using (EventLog log = new EventLog(this.logName, Environment.MachineName, this.logSource))
            {
                log.WriteEntry(string.Format(message, args), eventType);
            }
        }

        private void Trace(string message, int eventType, params object[] args)
        {
#if WINDOWS
            this.TraceWindows(message, (EventLogEntryType)eventType, args);
#else
            // Only diagnostic trace on non-Windows
            if (this.diagnosticTrace)
            {
                System.Diagnostics.Trace.WriteLine(string.Format(message, args));
            }
#endif
        }
    }
}