// <copyright file="PerfMonitor.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;

    public class PerfMonitor : IDisposable
    {
        private const int TimerPeriod = 60 * 1000; // 1 minute
        private const string PerfLogsBasePath = "perf";

        private static readonly PerfMonitor NoOpPerfService = new PerfMonitor();

        private static bool enabled = false;
        private static Timer timer = null;
        private static ConcurrentQueue<PerfSample> perfSampleProcessingQueue = new ConcurrentQueue<PerfSample>();

        protected PerfMonitor()
        {
        }

        public static bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                enabled = value;

                if (timer != null)
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                    timer = null;
                }

                if (enabled)
                {
                    int millisecondsToNextMinute = (60 - DateTime.Now.Second) * 1000;
                    timer = new Timer(TimerCallback, null, millisecondsToNextMinute, TimerPeriod);
                }
            }
        }

        public static PerfMonitor CreateAndStart(string key, string action)
        {
            if (!Enabled)
            {
                return NoOpPerfService;
            }

            return new EnabledPerfService(key, action);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Done(false);
        }

        public void Done()
        {
            this.Done(true);
        }

        protected virtual void Done(bool successful)
        {
            // do nothing in base
        }

        private static void TimerCallback(object state)
        {
            DateTime currentTime = DateTime.Now;
            Dictionary<string, PerfLogEntry> logEntries = GetCurrentLogEntries(currentTime);
            WriteLogEntries(currentTime, logEntries);
        }

        private static Dictionary<string, PerfLogEntry> GetCurrentLogEntries(DateTime currentTime)
        {
            Dictionary<string, PerfLogEntry> logEntries = new Dictionary<string, PerfLogEntry>();
            PerfSample perfSample = GetNextPerfSampleFromQueue(currentTime);

            while (perfSample != null)
            {
                string logEntryKey = Path.Combine(perfSample.Key, perfSample.Action);
                PerfLogEntry logEntry;
                if (!logEntries.TryGetValue(logEntryKey, out logEntry))
                {
                    logEntry = new PerfLogEntry();
                    logEntries.Add(logEntryKey, logEntry);
                }

                if (perfSample.Successful)
                {
                    logEntry.TotalLatency += perfSample.Duration;
                    logEntry.Transactions++;
                }
                else
                {
                    logEntry.ErrorTotalLatency += perfSample.Duration;
                    logEntry.ErrorTransactions++;
                }

                perfSample = GetNextPerfSampleFromQueue(currentTime);
            }

            return logEntries;
        }

        private static PerfSample GetNextPerfSampleFromQueue(DateTime currentTime)
        {
            // We want to peek at the perf sample. We will stop dequeuing samples
            // when either (1) there are no more samples or (2) the samples completed
            // after the current time for which we are aggregatting.
            PerfSample perfSample = perfSampleProcessingQueue.FirstOrDefault();

            if (perfSample != null)
            {
                if (perfSample.FinishedAt > currentTime)
                {
                    perfSample = null;
                }
                else
                {
                    perfSampleProcessingQueue.TryDequeue(out perfSample);
                }
            }

            return perfSample;
        }

        private static void WriteLogEntries(DateTime currentTime, Dictionary<string, PerfLogEntry> logEntries)
        {
            foreach (KeyValuePair<string, PerfLogEntry> pair in logEntries)
            {
                string filePath = GetLogFileName(currentTime, pair.Key);

                PerfLogEntry logEntry = pair.Value;
                string entryLine = string.Format(
                    "{0}:{1}, {2}, {3}, {4}, {5}{6}",
                    currentTime.Hour,
                    currentTime.Minute,
                    logEntry.Transactions,
                    GetAverageLatency(logEntry.TotalLatency, logEntry.Transactions),
                    logEntry.ErrorTransactions,
                    GetAverageLatency(logEntry.ErrorTotalLatency, logEntry.ErrorTransactions),
                    Environment.NewLine);

                EnsureLogFileExists(currentTime, filePath);
                File.AppendAllText(filePath, entryLine);
            }
        }

        private static string GetLogFileName(DateTime currentTime, string logDirectoryPath)
        {
            if (!string.IsNullOrWhiteSpace(PerfLogsBasePath))
            {
                logDirectoryPath = Path.Combine(PerfLogsBasePath, logDirectoryPath);
            }

            string fileName = GetLogFileName(currentTime);
            string filePath = Path.Combine(logDirectoryPath, fileName);

            return filePath;
        }

        private static void EnsureLogFileExists(DateTime currentTime, string filePath)
        {
            if (!File.Exists(filePath))
            {
                // Ensure the directory exists
                string directoryPath = Path.GetDirectoryName(filePath);
                Directory.CreateDirectory(directoryPath);

                File.WriteAllText(filePath, "Time (h:m), Number of Successful Actions, Average Latency of Successful Actions (ms),  Number of Failed Actions, Average Latency of Failed Actions (ms)" + Environment.NewLine);

                string fileToDelete = GetLogFileName(currentTime.AddDays(-8));
                if (File.Exists(fileToDelete))
                {
                    File.Delete(fileToDelete);
                }
            }
        }

        private static string GetAverageLatency(TimeSpan totalLatency, int numberOfTransactions)
        {
            return ((double)totalLatency.Milliseconds / numberOfTransactions).ToString("#.####");
        }

        private static string GetLogFileName(DateTime currentTime)
        {
            string fileName = string.Format("{0}-{1}-{2}.csv", currentTime.Year, currentTime.Month, currentTime.Day);

            return fileName;
        }

        private class EnabledPerfService : PerfMonitor
        {
            private string key;
            private string action;
            private Stopwatch stopwatch;

            public EnabledPerfService(string key, string action)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentException("Argument can not be null or whitespace.", "key");
                }

                if (string.IsNullOrWhiteSpace(action))
                {
                    throw new ArgumentException("Argument can not be null or whitespace.", "action");
                }

                this.key = key;
                this.action = action;
                this.stopwatch = new Stopwatch();
                this.stopwatch.Start();
            }

            protected override void Done(bool successful)
            {
                if (this.stopwatch.IsRunning)
                {
                    this.stopwatch.Stop();

                    PerfSample perfSample = new PerfSample();
                    perfSample.Key = this.key;
                    perfSample.Action = this.action;
                    perfSample.Duration = this.stopwatch.Elapsed;
                    perfSample.Successful = successful;
                    perfSample.FinishedAt = DateTime.Now;

                    perfSampleProcessingQueue.Enqueue(perfSample);
                }
            }
        }

        private class PerfSample
        {
            public string Key { get; set; }

            public string Action { get; set; }

            public DateTime FinishedAt { get; set; }

            public TimeSpan Duration { get; set; }

            public bool Successful { get; set; }
        }

        private class PerfLogEntry
        {
            public int Transactions { get; set; }

            public int ErrorTransactions { get; set; }

            public TimeSpan TotalLatency { get; set; }

            public TimeSpan ErrorTotalLatency { get; set; }
        }
    }
}
