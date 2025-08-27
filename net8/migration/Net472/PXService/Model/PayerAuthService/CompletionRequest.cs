// <copyright file="CompletionRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model per the PayerAuth.V3 API.  
    /// This object is sent as payload to PayerAuth.V3's POST /completeChallenge API
    /// </summary>
    public class CompletionRequest
    {
        public CompletionRequest()
        {
            this.AuthorizationParameters = new Dictionary<string, string>();
        }

        [JsonProperty(PropertyName = "payment_session")]
        public PaymentSession PaymentSession { get; set; }

        [JsonProperty(PropertyName = "authentication_context")]
        public AuthenticationResponse AuthenticationContext { get; set; }

        [JsonProperty(PropertyName = "authorization_parameters")]
        public IDictionary<string, string> AuthorizationParameters { get; }
    }
}