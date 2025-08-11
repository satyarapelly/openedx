// <copyright file="FileBasedOfflineChecker.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class FileBasedOfflineChecker : IOfflineStatusChecker
    {
        private readonly object syncObject = new object();

        private DateTime lastChecked = DateTime.MinValue;
        private bool offlineState;
        private TimeSpan checkPeriod = TimeSpan.FromSeconds(5);
        private Task backgroundCheck = null;
        private CancellationTokenSource cancellationTokenSource = null;
        private CancellationToken cancellationToken = CancellationToken.None;

        public event EventHandler<string> LogEvent = (sender, s) => { };

        public string LocalOfflineFile { get; set; }

        public string RemoteOfflineFile { get; set; }

        public bool GetOfflineState()
        {
            TimeSpan currentCheckPeriod = this.checkPeriod;
            if (currentCheckPeriod == Timeout.InfiniteTimeSpan)
            {
                return this.offlineState;
            }

            currentCheckPeriod += currentCheckPeriod;
            if (this.lastChecked.Add(currentCheckPeriod) < DateTime.UtcNow)
            {
                lock (this.syncObject)
                {
                    if (this.lastChecked.Add(currentCheckPeriod) < DateTime.UtcNow)
                    {
                        DateTime newLastChecked = DateTime.UtcNow;
                        this.offlineState = File.Exists(this.LocalOfflineFile);
                        this.lastChecked = newLastChecked;
                    }
                }
            }

            if (this.backgroundCheck == null || this.backgroundCheck.IsCompleted)
            {
                lock (this.syncObject)
                {
                    if (this.backgroundCheck == null || this.backgroundCheck.IsCompleted)
                    {
                        if (this.cancellationTokenSource != null)
                        {
                            this.cancellationToken = CancellationToken.None;
                            this.cancellationTokenSource.Dispose();
                            this.cancellationTokenSource = null;
                        }

                        this.cancellationTokenSource = new CancellationTokenSource();
                        this.cancellationToken = this.cancellationTokenSource.Token;
                        this.backgroundCheck = Task.Run(async () => await this.BackgroundCheckOffline(), this.cancellationToken);
                    }
                }
            }

            return this.offlineState;
        }

        private async Task BackgroundCheckOffline()
        {
            try
            {
                while (!this.cancellationToken.IsCancellationRequested)
                {
                    bool newOfflineState = false;

                    DateTime newLastChecked = DateTime.UtcNow;
                    string offlineFile = this.LocalOfflineFile;
                    string remoteOfflineFile = this.RemoteOfflineFile;

                    if (!string.IsNullOrWhiteSpace(offlineFile))
                    {
                        newOfflineState = File.Exists(offlineFile);

                        if (!string.IsNullOrWhiteSpace(remoteOfflineFile) &&
                            Directory.Exists(Path.GetDirectoryName(remoteOfflineFile)) &&
                            File.Exists(remoteOfflineFile) != newOfflineState)
                        {
                            if (newOfflineState)
                            {
                                File.Delete(offlineFile);
                            }
                            else
                            {
                                File.WriteAllText(offlineFile, DateTime.UtcNow.ToString());
                            }

                            newOfflineState = !newOfflineState;
                        }
                    }

                    this.LogEvent(null, string.Format("Offline helper. {0}. {1}. {2}", offlineFile ?? "NULL", remoteOfflineFile ?? "NULL", newOfflineState));

                    lock (this.syncObject)
                    {
                        this.offlineState = newOfflineState;
                        this.lastChecked = newLastChecked;
                    }

                    await Task.Delay(this.checkPeriod, this.cancellationToken);
                }

                this.LogEvent(null, "Offline helper cancelation requested.");
            }
            catch (Exception exception)
            {
                // top level exception
                this.LogEvent(null, string.Format("Offline helper. Exception: {0}", exception));
            }
        }
    }
}