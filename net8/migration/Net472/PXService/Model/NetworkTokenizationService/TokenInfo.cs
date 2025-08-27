// <copyright file="TokenInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;

    public class TokenInfo
    {
        [JsonProperty("tokenStatus")]
        public string TokenStatus { get; set; }

        [JsonProperty("lastFourDigits")]
        public string LastFourDigits { get; set; }

        [JsonProperty("expirationDate")]
        public ExpirationDate ExpirationDate { get; set; }
    }
}