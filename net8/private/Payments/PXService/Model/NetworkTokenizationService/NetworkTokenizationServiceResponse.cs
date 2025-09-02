// <copyright file="NetworkTokenizationServiceResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class NetworkTokenizationServiceResponse
    {
        [JsonProperty("tokens")]
        public IEnumerable<NetworkToken> Tokens { get; set; }
    }
}