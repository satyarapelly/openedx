// <copyright file="SetupProviderSessionOutgoingPayload.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.WalletService
{
    using Newtonsoft.Json;

    public class SetupProviderSessionOutgoingPayload
    {
        [JsonProperty(PropertyName = "piFamily")]
        public string PiFamily { get; set; }

        [JsonProperty(PropertyName = "piType")]
        public string PiType { get; set; }

        [JsonProperty(PropertyName = "walletSessionData")]
        public string WalletSessionData { get; set; }
    }
}