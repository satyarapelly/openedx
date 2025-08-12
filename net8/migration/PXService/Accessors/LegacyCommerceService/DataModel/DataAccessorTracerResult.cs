// <copyright file="DataAccessorTracerResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Xml;

    public class DataAccessorTracerResult
    {
        public const int ApiNone = 0;
        public const int ApiSuccess = 1;
        public const int ApiError = 2;
        public const int ApiException = 3;

        public string ApiName { get; set; }

        public int ApiResponseType { get; set; }

        public string ApiRequest { get; set; }

        public string ApiResponse { get; set; }

        public Guid TrackingGuid { get; set; }

        public string RawApiRequest { get; set; }

        public string RawApiResponse { get; set; }

        public DateTime ApiStart { get; set; }

        public DateTime ApiEnd { get; set; }

        public int ErrorCode { get; set; }

        public int TryCount { get; set; }

        public Dictionary<string, Stopwatch> StopWatches { get; }

        public string ElapsedTimes
        {
            get
            {
                if (this.StopWatches == null || this.StopWatches.Count == 0)
                {
                    return "No elapsed times were recorded.";
                }
                else
                {
                    StringBuilder result = new StringBuilder();
                    foreach (var watch in this.StopWatches)
                    {
                        if (watch.Value.IsRunning)
                        {
                            continue;
                        }

                        if (result.Length > 0)
                        {
                            result.Append(", ");
                        }

                        result.AppendFormat("{0} {1} ms", watch.Key, watch.Value.ElapsedMilliseconds);
                    }

                    return result.ToString();
                }
            }
        }


        public void SetException(Exception ex, int errorCode)
        {
            this.ApiResponseType = ApiException;
            this.ErrorCode = errorCode;

            StringWriter stringWriter = new StringWriter();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
            xmlTextWriter.WriteElementString("Exception", ex.ToString());

            this.ApiResponse = stringWriter.ToString();
        }

        public DataAccessorTracerResult()
        {
            this.StopWatches = new Dictionary<string, Stopwatch>();
        }

        public void StartWatch(string watchName)
        {
            if (!this.StopWatches.ContainsKey(watchName))
            {
                var sw = new Stopwatch();
                sw.Start();
                this.StopWatches.Add(watchName, sw);
            }
        }

        public void StopWatch(string watchName)
        {
            if (this.StopWatches.ContainsKey(watchName))
            {
                this.StopWatches[watchName].Stop();
            }
        }
    }
}

