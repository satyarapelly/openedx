// <copyright file="AnomalyDetectionAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Azure.Core;
    using global::Azure.Storage.Blobs;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;

    public class AnomalyDetectionAccessor : IAnomalyDetectionAccessor
    {
        private const string AccountsBlobName = "blockedaccounts.csv";
        private const string ClientIPBlobName = "blockedips.csv";
        private const int BlobRefreshTimeInMinutes = 5;
        private bool enableTestHook = false;
        private int checkBlobReadCounter = 0;
        private BlobClient accountIdBlobClient = null;
        private BlobClient clientIPBlobClient = null;
        private Dictionary<string, DateTime> maliciousAccountIds;
        private Dictionary<string, DateTime> maliciousClientIPs;
        private DateTime lastBlobReadTime = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnomalyDetectionAccessor"/> class. 
        /// Note: This is only for used for selfhost scenarios. In webhost scenarios, please use the other constructor
        /// </summary>
        /// <param name="enableTestHook">set to true only in test environment to support test hooks</param>
        public AnomalyDetectionAccessor(bool enableTestHook)
        {
            this.enableTestHook = enableTestHook;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnomalyDetectionAccessor"/> class
        /// </summary>
        /// <param name="adResultsContainerPath">azure container path that holds anamaly detection results</param>
        /// <param name="tokenCredential">managed identity clientId</param>
        public AnomalyDetectionAccessor(
        string adResultsContainerPath,
        TokenCredential tokenCredential)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(adResultsContainerPath))
                {
                    this.accountIdBlobClient = new BlobClient(new Uri($"{adResultsContainerPath}/{AccountsBlobName}"), tokenCredential);
                    this.clientIPBlobClient = new BlobClient(new Uri($"{adResultsContainerPath}/{ClientIPBlobName}"), tokenCredential);
                }
                else
                {
                    SllWebLogger.TracePXServiceException("Anomaly detection results container path is not specified", EventTraceActivity.Empty);
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException($"AnomalyDetectionAccessor Initialization: {ex}", EventTraceActivity.Empty);
            }
        }

        public bool IsMaliciousAccountId(string accountId, EventTraceActivity traceActivityId)
        {
            // Queue background task if required
            this.CheckAndQueueBackgroundTaskToReadLatestBlobData(traceActivityId);

            // Ignore null or empty values
            if (string.IsNullOrEmpty(accountId))
            {
                return false;
            }

            DateTime accountIdExpDate = DateTime.MinValue;
            return (this.maliciousAccountIds?.TryGetValue(accountId, out accountIdExpDate) ?? false) && accountIdExpDate > DateTime.UtcNow;
        }

        public bool IsMaliciousClientIP(string clientIP, EventTraceActivity traceActivityId)
        {
            // Queue background task if required
            this.CheckAndQueueBackgroundTaskToReadLatestBlobData(traceActivityId);

            // Ignore null or empty values
            if (string.IsNullOrEmpty(clientIP))
            {
                return false;
            }

            DateTime clientIPExpDate = DateTime.MinValue;
            return (this.maliciousClientIPs?.TryGetValue(clientIP, out clientIPExpDate) ?? false) && clientIPExpDate > DateTime.UtcNow;
        }

        public bool InitializeAnomalyDetectionResults(byte[] accountIdBlobContent, byte[] clientIPBlobContent)
        {
            if (this.enableTestHook)
            {
                this.SetMaliciousAccountIds(new BinaryData(accountIdBlobContent));
                this.SetMaliciousClientIPs(new BinaryData(clientIPBlobContent));
            }

            return true;
        }

        private static Dictionary<string, DateTime> GetMaliciousIdDetails(BinaryData maliciousIdContent)
        {
            Dictionary<string, DateTime> maliciousIds = new Dictionary<string, DateTime>();
            if (maliciousIdContent != null)
            {
                using (var streamReader = new System.IO.StreamReader(maliciousIdContent.ToStream()))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var blockedInfo = streamReader.ReadLine();
                        string[] blockedInfoParts = blockedInfo.Split(',');
                        if (blockedInfoParts.Length == 2)
                        {
                            DateTime expiryDate;
                            if (DateTime.TryParse(blockedInfoParts[1], out expiryDate))
                            {
                                maliciousIds[blockedInfoParts[0]] = expiryDate.ToUniversalTime();
                            }
                        }
                    }
                }
            }

            return maliciousIds;
        }

        private void CheckAndQueueBackgroundTaskToReadLatestBlobData(EventTraceActivity traceActivityId)
        {
            traceActivityId = traceActivityId ?? EventTraceActivity.Empty;
            try
            {
                if (this.accountIdBlobClient != null && this.clientIPBlobClient != null)
                {
                    if (this.lastBlobReadTime < DateTime.UtcNow.AddMinutes(-BlobRefreshTimeInMinutes))
                    {
                        if (Interlocked.Exchange(ref this.checkBlobReadCounter, 1) == 0)
                        {
                            SllWebLogger.TraceServerMessage("AnomalyDetectionAccessor", traceActivityId.ToString(), null, "Queue background workitem to read latest blob content", EventLevel.Warning);
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    SllWebLogger.TraceServerMessage("AnomalyDetectionAccessor", traceActivityId.ToString(), null, "Try to get latest anomaly detection results", EventLevel.Informational);

                                    // refresh anomaly detection results
                                    await this.RefreshAnomalyDetectionResults();

                                    SllWebLogger.TraceServerMessage("AnomalyDetectionAccessor", traceActivityId.ToString(), null, $"Got anomaly detection results. MaliciousAccountCount : {this.maliciousAccountIds?.Count}, MaliciousClientIPCount: {this.maliciousClientIPs?.Count}", EventLevel.Informational);
                                }
                                catch (Exception ex)
                                {
                                    SllWebLogger.TracePXServiceException($"AnomalyDetectionAccessor.BlobReadTask: {ex}", traceActivityId);
                                }
                                finally
                                {
                                    SllWebLogger.TraceServerMessage("AnomalyDetectionAccessor", traceActivityId.ToString(), null, "Setting blob read counter to 0", EventLevel.Warning);
                                    Interlocked.Exchange(ref this.checkBlobReadCounter, 0);
                                }
                            });
                        }
                    }
                }
                else
                {
                    SllWebLogger.TraceServerMessage("AnomalyDetectionAccessor", traceActivityId.ToString(), null, "Blob clients didn't exist", EventLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException($"AnomalyDetectionAccessor.QueueBackgroundTaskToReadLatestBlobData: {ex}", traceActivityId);
            }
        }

        private void SetMaliciousAccountIds(BinaryData accountIdContent)
        {
            var accountIds = GetMaliciousIdDetails(accountIdContent);
            if ((accountIds?.Count ?? 0) > 0)
            {
                this.maliciousAccountIds = accountIds;
            }
        }

        private void SetMaliciousClientIPs(BinaryData clientIPContent)
        {
            var clientIPs = GetMaliciousIdDetails(clientIPContent);
            if ((clientIPs?.Count ?? 0) > 0)
            {
                this.maliciousClientIPs = clientIPs;
            }
        }

        private async Task RefreshAnomalyDetectionResults()
        {
            // read blob content and set malicious accountIds
            var accountIdResponse = await this.accountIdBlobClient.DownloadContentAsync();
            var accountIdContent = accountIdResponse?.Value?.Content;
            this.SetMaliciousAccountIds(accountIdContent);

            // read blob content and set malicious clientIPs
            var clientIPResponse = await this.clientIPBlobClient.DownloadContentAsync();
            var clientIPContent = clientIPResponse?.Value?.Content;
            this.SetMaliciousClientIPs(clientIPContent);

            // set lastblob read time
            this.lastBlobReadTime = DateTime.UtcNow;
        }
    }
}