// <copyright file="ChallengeContext.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pims
{
    using Newtonsoft.Json;

    public class ChallengeContext
    {
        [JsonProperty(PropertyName = "IpAddress")]
        public string IpAddress { get; set; }

        [JsonProperty(PropertyName = "paymentMethodType")]
        public string PaymentMethodType { get; set; }

        [JsonProperty(PropertyName = "piid")]
        public string Piid { get; set; }
    }
}