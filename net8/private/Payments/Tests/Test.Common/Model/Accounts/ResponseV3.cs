// <copyright file="ResponseV3.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Accounts
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ResponseV3<T>
    {
        [JsonProperty(PropertyName = "total_count")]
        public int TotalCount { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Needed for serialization purpose.")]
        [JsonProperty(PropertyName = "items")]
        public List<T> Items { get; set; }
    }
}