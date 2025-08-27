// <copyright file="WalletSessionData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.WalletService
{
    using Newtonsoft.Json;

    public class WalletSessionData
    {
        [JsonProperty(PropertyName = "merchantIdentifier")]
        public string MerchantIdentifier { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "initiative")]
        public string Initiative { get; set; }

        [JsonProperty(PropertyName = "initiativeContext")]
        public string InitiativeContext { get; set; }
    }
}