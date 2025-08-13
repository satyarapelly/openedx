// <copyright file="DataSeries.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// X-Axis is divided into equal time spans based on the "Resolution".  Y-Axis is the count of events.
    /// </summary>
    internal class DataSeries
    {
        private const int MaxLength = 12;
        private static readonly TimeSpan Resolution = new TimeSpan(0, 10, 0);

        private List<DataBucket> allRequests;
        private List<DataBucket> badRequests;

        private DataBucket allRequestsRecentPoint;
        private DataBucket badRequestsRecentPoint;

        private object allRequestsLock;
        private object badRequestsLock;

        public DataSeries()
        {
            this.allRequests = new List<DataBucket>(2);
            this.badRequests = new List<DataBucket>(2);
            this.allRequestsLock = new object();
            this.badRequestsLock = new object();
        }

        public int GetAllRequestsTotal(DateTime timeStamp)
        {
            return GetTotal(this.allRequests, this.allRequestsLock, DataSeries.Resolution, DataSeries.MaxLength, timeStamp);
        }

        public int GetBadRequestsTotal(DateTime timeStamp)
        {
            return GetTotal(this.badRequests, this.badRequestsLock, DataSeries.Resolution, DataSeries.MaxLength, timeStamp);
        }

        // To reduce memory usage, if the Y-Axis value is 0, dont add it to the list.
        public void AddData(bool isBadRequest, DateTime timeStamp)
        {
            AddData(
                this.allRequests, 
                ref this.allRequestsRecentPoint, 
                this.allRequestsLock, 
                DataSeries.Resolution, 
                MaxLength, 
                timeStamp);

            if (isBadRequest)
            {
                AddData(
                    this.badRequests, 
                    ref this.badRequestsRecentPoint, 
                    this.badRequestsLock, 
                    DataSeries.Resolution, 
                    MaxLength, 
                    timeStamp);
            }
        }

        private static int GetTotal(List<DataBucket> points, object lockObj, TimeSpan resolution, int maxLen, DateTime timeStamp)
        {
            var retEndIndex = Convert.ToInt32(Math.Floor((timeStamp - AnomalyDetection.StartTime).TotalMinutes / resolution.TotalMinutes));
            var retStartIndex = retEndIndex < maxLen ? 0 : retEndIndex - maxLen + 1;

            int total = 0;
            lock (lockObj)
            {
                points.RemoveAll(p => p.Index < retStartIndex || p.Index > retEndIndex);
                points.ForEach(dp => total += dp.Value);
            }

            return total;
        }

        private static void AddData(List<DataBucket> points, ref DataBucket recent, object lockObj, TimeSpan resolution, int maxLen, DateTime timeStamp)
        {
            var currentIndex = Convert.ToInt32(Math.Floor((timeStamp - AnomalyDetection.StartTime).TotalMinutes / resolution.TotalMinutes));

            // This is the case when the very first data point is being added to this series.
            var localRecent = recent;
            if (localRecent == null)
            {
                lock (lockObj)
                {
                    if (recent == null)
                    {
                        localRecent = recent = new DataBucket(currentIndex, 1);
                        points.Add(recent);
                        return;
                    }
                }
            }

            localRecent = recent;
            if (currentIndex == localRecent.Index)
            {
                // At the Operation=AddCC level, there are typically hunderds of requests in every bucket (10 minutes).  
                // In such cases, this code block is executed very frequently (hunderds of Increments on the same
                // recent bucket.  It is important to be able to do this without acquiring an object lock.
                Interlocked.Increment(ref localRecent.Value);
            }
            else
            {
                // We need to find if the currentIndex exists in the List.  We cant assume that indexes are added
                // in sequence in the List because of multiple threads that could get added slightly out of order
                // especially during boundaries of buckets. 
                lock (lockObj)
                {
                    localRecent = recent;
                    bool found = false;
                    for (int i = points.Count - 1; i >= 0; i--)
                    {
                        if (currentIndex == points[i].Index)
                        {
                            found = true;
                            Interlocked.Increment(ref points[i].Value);
                            if (currentIndex > localRecent.Index)
                            {
                                // recent was lagging behind.  Pull it forward to the bucket we found just now which
                                // is more recent.
                                localRecent = recent = points[i];
                            }

                            break;
                        }
                    }

                    if (!found)
                    {
                        localRecent = new DataBucket(currentIndex, 1);
                        points.Add(localRecent);
                        recent = localRecent;
                    }

                    // E.g. If currentIndex is 12 and maxLenght is 12, we dont need indexes smaller
                    // than 1.
                    points.RemoveAll(p => p.Index < currentIndex - DataSeries.MaxLength + 1);
                }
            }
        }
    }
}
