// <copyright file="IPartialErrorResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class IPartialErrorResponse
    {
        [JsonProperty("apiErrors", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<ApiError> ApiErrors { get; set; }

        [JsonProperty("hasPartialResult", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool HasPartialResult { get; set; }
    }
}