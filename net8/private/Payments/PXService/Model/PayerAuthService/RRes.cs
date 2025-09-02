// <copyright file="RRes.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Newtonsoft.Json;

    public class RRes
    {
        [JsonProperty(PropertyName = "authenticate_value")]
        public string AuthenticateValue { get; set; }

        [JsonProperty(PropertyName = "eci")]
        public string Eci { get; set; }

        [JsonProperty(PropertyName = "transaction_challenge_status")]
        public string TransactionStatus { get; set; }
    }
}