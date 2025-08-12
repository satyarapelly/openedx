// <copyright file="CreateAndAuthenticateResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
{
    using Newtonsoft.Json;

    public class CreateAndAuthenticateResponse
    {
        [JsonProperty(PropertyName = "PaymentSession", Required = Required.Always)]
        public PaymentSession PaymentSession { get; set; }

        [JsonProperty(PropertyName = "AuthenticateResponse")]
        public AuthenticationResponse AuthenticateResponse { get; set; }

        [JsonProperty(PropertyName = "sessionToken")]
        public string SessionToken { get; set; }
    }
}