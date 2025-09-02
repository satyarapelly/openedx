// <copyright file="PivotTree.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class PivotTree
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<string, PivotTree>> children;

        public PivotTree(string key, string value, IEnumerable<string> dimensions, int maxHeight)
        {
            if (dimensions == null)
            {
                throw new ArgumentNullException(nameof(dimensions));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (dimensions.Count() != dimensions.Distinct().Count())
            {
                throw new ArgumentException("dimensions has duplicates", nameof(dimensions));
            }

            this.Series = new DataSeries();

            this.Key = key;
            this.Value = value;
            this.MaxHeight = maxHeight;

            if (dimensions.Count() > 0 && this.MaxHeight > 0)
            {
                this.children = new ConcurrentDictionary<string, ConcurrentDictionary<string, PivotTree>>();

                // Create all dimensions
                foreach (var dimension in dimensions)
                {
                    this.children[dimension] = new ConcurrentDictionary<string, PivotTree>();
                }
            }
        }

        public string Key { get; }

        public string Value { get; }

        public DataSeries Series { get; set; }

        // If 0, this tree cannot have children. If 1, this tree can only have immediate children and so on.  
        private int MaxHeight { get; set; }

        public void AddData(Dictionary<string, string> data, bool isBadRequest, DateTime timeStamp)
        {
            this.Series.AddData(isBadRequest, timeStamp);

            if (this.children != null)
            {
                // Go through each dimension.  e.g. [IPAddress, AccountId]
                foreach (string key in this.children.Keys)
                {
                    // Its possible that data for all dimensions are not available all the time.  E.g. IPAddress will not
                    // be available when requests are sent from PIFD running on AKS.  Also, during CITs.
                    if (data.ContainsKey(key))
                    {
                        // E.g. key = IPAddress and value = 10.10.1.1
                        var value = data[key];

                        // E.g. if the child tree for IPAddress does not contain 10.10.1.1
                        if (!this.children[key].ContainsKey(value))
                        {
                            // if current dimensions are [IPAddress, AccountId] and we are creating
                            // a child tree for IPAddress = 10.10.1.1, its dimensions should be [AccountId]
                            var childDimensions = new List<string>(this.children.Keys);
                            childDimensions.Remove(key);
                            this.children[key].TryAdd(value, new PivotTree(key, value, childDimensions, this.MaxHeight - 1));
                        }

                        PivotTree child = null;
                        if (this.children[key].TryGetValue(value, out child))
                        {
                            child.AddData(data, isBadRequest, timeStamp);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the incoming data exceeds expected rate on one or more dimension/s.
        /// The dimension/s that exceed the expected rate are returned to the caller.
        /// </summary>
        /// <param name="data">Dictionary of key value pairs</param>
        /// <param name="flights">List of enabled flights</param>
        /// <param name="timeStamp">Timestamp of this event</param>
        /// <returns>
        /// Dimensions that excceded the rate  limit
        /// </returns>
        public HashSet<string> IsCardTesting(Dictionary<string, string> data, List<string> flights, DateTime timeStamp)
        {
            HashSet<string> ret = null;
            if (string.Equals(this.Key, "Operation"))
            {
                int allTotal = this.Series.GetAllRequestsTotal(timeStamp);
                if (allTotal < 100)
                {
                    // Not enough traffic to establish baseline percentage of bad requests
                    return null;
                }

                if (!(flights != null && flights.Contains(Flighting.Features.PXRateLimitDisableBaselineCheck, StringComparer.OrdinalIgnoreCase)))
                {
                    int badTotal = this.Series.GetBadRequestsTotal(timeStamp);
                    if (((badTotal * 100) / allTotal) > 70.0)
                    {
                        // Baseline percentage of bad requests is high which is most likely indicative
                        // of a functionality/system issue.  Do no apply rate limiting.
                        return null;
                    }
                }

                foreach (string key in this.children.Keys)
                {
                    PivotTree child = null;
                    string value = null;
                    if (data.TryGetValue(key, out value))
                    {
                        if (this.children[key].TryGetValue(value, out child))
                        {
                            var exceedingDimensions = child.IsCardTesting(data, flights, timeStamp);

                            if (exceedingDimensions != null)
                            {
                                if (ret == null)
                                {
                                    ret = exceedingDimensions;
                                }
                                else
                                {
                                    ret.UnionWith(exceedingDimensions);
                                }
                            }
                        }
                    }
                }
            }
            else if (string.Equals(this.Key, Constants.Dimensions.IPAddress) || string.Equals(this.Key, Constants.Dimensions.AccountId))
            {
                const int CountLimit = 6;
                const int FailPercentLimit = 85;
                int allRequestsTotal = this.Series.GetAllRequestsTotal(timeStamp);
                if (allRequestsTotal < CountLimit || allRequestsTotal <= 0)
                {
                    return null;
                }

                int badRequestsTotal = this.Series.GetBadRequestsTotal(timeStamp);
                int badRequestsPercent = (int)(badRequestsTotal * 100.0) / allRequestsTotal;
                if (badRequestsPercent >= FailPercentLimit)
                {
                    return new HashSet<string>() { this.Key };
                }
            }

            return ret;
        }

        public void PruneTree(DateTime timeStamp)
        {
            PivotTree removed = null;
            foreach (string key in this.children.Keys)
            {
                foreach (var tree in this.children[key].Values)
                {
                    int total = tree.Series.GetAllRequestsTotal(timeStamp);
                    if (total == 0)
                    {
                        this.children[tree.Key].TryRemove(tree.Value, out removed);
                    }
                }
            }
        }
    }
}
