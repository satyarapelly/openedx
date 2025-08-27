// <copyright file="CreateAndAuthenticateRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
{
    using Newtonsoft.Json;

    public class CreateAndAuthenticateRequest
    {
        [JsonProperty(PropertyName = "PaymentSessionData", Required = Required.Always)]
        public PaymentSessionData PaymentSessionData { get; set; }

        [JsonProperty(PropertyName = "AuthenticateRequest", Required = Required.Always)]
        public AuthenticationRequest AuthenticateRequest { get; set; }
    }
}