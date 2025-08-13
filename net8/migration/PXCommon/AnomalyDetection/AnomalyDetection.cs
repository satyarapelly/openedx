// <copyright file="AnomalyDetection.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class AnomalyDetection
    {
        private static PivotTree tree = new PivotTree(
            "Operation", 
            "AddCC", 
            new HashSet<string>()
            { 
                Constants.Dimensions.IPAddress,
                Constants.Dimensions.AccountId
            }, 
            1);

        private static object pruneTimeLock = new object();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211", Justification = "Need this to be accessible globally.")]
        public static DateTime StartTime { get; set; } = DateTime.UtcNow;

        private static DateTime PruneTime { get; set; } = AnomalyDetection.StartTime.AddMinutes(70);

        public static HashSet<string> IsCardTesting(Dictionary<string, string> data, List<string> flights)
        {
            return tree.IsCardTesting(data, flights, DateTime.UtcNow);
        }

        public static void AddData(Dictionary<string, string> data, bool isBadRequest)
        {
            AddData(data, isBadRequest, DateTime.UtcNow);
        }

        public static void AddData(Dictionary<string, string> data, bool isBadRequest, DateTime timeStamp)
        {
            tree.AddData(data, isBadRequest, timeStamp);

            // Prune tree at 1:10, 2:10, 3:10 from start etc.
            if (timeStamp > PruneTime)
            {
                lock (pruneTimeLock)
                {
                    if (timeStamp > PruneTime)
                    {
                        PruneTime = PruneTime.AddHours(1);
                        tree.PruneTree(timeStamp);
                    }
                }
            }
        }
    }
}
