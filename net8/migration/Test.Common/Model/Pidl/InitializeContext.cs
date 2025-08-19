// <copyright file="InitializeContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class InitializeContext
    {
        public InitializeContext()
        {
        }

        [JsonProperty(PropertyName = "components")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, object> Components { get; set; }

        [JsonProperty(PropertyName = "componentProps")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, PidlDocInfo> ComponentProps { get; set; }

        [JsonProperty(PropertyName = "pidlDocOverrides")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, PidlDocOverrides> PidlDocOverrides { get; set; }

        [JsonProperty(PropertyName = "submissionOrder")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<SubmissionOrder> SubmissionOrder { get; set; }

        [JsonProperty(PropertyName = "cachedPrefetcherData")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, object> CachedPrefetcherData { get; set; }
    }
}