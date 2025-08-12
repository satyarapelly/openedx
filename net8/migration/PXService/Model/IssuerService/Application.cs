// <copyright file="Application.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.Model.IssuerService
{
    using Newtonsoft.Json;

    public class Application
    {
        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "customerPuid")]
        public string CustomerPuid { get; set; }

        [JsonProperty(PropertyName = "jarvisAccountId")]
        public string JarvisAccountId { get; set; }

        [JsonProperty(PropertyName = "issuerCustomerId")]
        public string IssuerCustomerId { get; set; }

        [JsonProperty(PropertyName = "cardProduct")]
        public string CardProduct { get; set; }

        [JsonProperty(PropertyName = "channel")]
        public string Channel { get; set; }

        [JsonProperty(PropertyName = "referrerId")]
        public string ReferrerId { get; set; }

        [JsonProperty(PropertyName = "market")]
        public string Market { get; set; }

        [JsonProperty(PropertyName = "issuerAccountId")]
        public string IssuerAccountId { get; set; }

        [JsonProperty(PropertyName = "lastFourDigits")]
        public string LastFourDigits { get; set; }

        [JsonProperty(PropertyName = "paymentInstrumentId")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "errorDetails")]
        public ErrorDetails ErrorDetails { get; set; }

        [JsonProperty(PropertyName = "createDate")]
        public string CreateDate { get; set; }

        [JsonProperty(PropertyName = "modifiedDate")]
        public string ModifiedDate { get; set; }
    }
}