// <copyright file="DirectoryServerData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class DirectoryServerData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("caRootCertificate")]
        public string CaRootCertificate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        [JsonProperty("caRootCertificates")]
        public List<string> CaRootCertificates { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("keyType")]
        public string KeyType { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }
    }
}