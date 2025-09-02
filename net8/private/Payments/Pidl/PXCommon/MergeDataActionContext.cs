// <copyright file="MergeDataActionContext.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using Newtonsoft.Json;

    public class MergeDataActionContext
    {
        [JsonProperty(PropertyName = "payload")]
        public object Payload { get; set; }

        [JsonProperty(PropertyName = "explicit")]
        public bool Explicit { get; set; }
    }
}