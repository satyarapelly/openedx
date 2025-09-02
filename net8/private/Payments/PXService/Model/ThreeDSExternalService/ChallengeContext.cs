// <copyright file="ChallengeContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService
{
    using Newtonsoft.Json;

    public class ChallengeContext
    {
        [JsonProperty(PropertyName = "ipAddress")]
        public string IpAddress { get; set; }

        [JsonProperty(PropertyName = "paymentMethodType")]
        public string PaymentMethodType { get; set; }

        [JsonProperty(PropertyName = "piid")]
        public string Piid { get; set; }
    }
}