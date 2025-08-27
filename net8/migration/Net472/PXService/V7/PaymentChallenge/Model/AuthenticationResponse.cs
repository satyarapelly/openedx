// <copyright file="AuthenticationResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
{
    using Newtonsoft.Json;
    using PXService.Model.PayerAuthService;

    public class AuthenticationResponse
    {
        [JsonProperty(PropertyName = "threeDSServerTransactionID")]
        public string ThreeDSServerTransactionID { get; set; }

        [JsonProperty(PropertyName = "acsTransactionID")]
        public string AcsTransactionID { get; set; }

        [JsonProperty(PropertyName = "acsSignedContent")]
        public string AcsSignedContent { get; set; }

        [JsonProperty(PropertyName = "enrollmentStatus")]
        public PaymentInstrumentEnrollmentStatus EnrollmentStatus { get; set; }

        [JsonProperty(PropertyName = "sessionToken")]
        public string SessionToken { get; set; }

        [JsonProperty(PropertyName = "challengeStatus")]
        public PaymentChallengeStatus ChallengeStatus { get; set; }

        [JsonProperty(PropertyName = "cardholderInfo")]
        public string CardHolderInfo { get; set; }

        [JsonProperty(PropertyName = "acsRenderingType")]
        public AcsRenderingType AcsRenderingType { get; set; }

        [JsonProperty(PropertyName = "acsChallengeMandated")]
        public string AcsChallengeMandated { get; set; }

        [JsonProperty(PropertyName = "acsOperatorID")]
        public string AcsOperatorID { get; set; }

        [JsonProperty(PropertyName = "acsReferenceNumber")]
        public string AcsReferenceNumber { get; set; }

        [JsonProperty(PropertyName = "authenticationType")]
        public string AuthenticationType { get; set; }

        [JsonProperty(PropertyName = "dsReferenceNumber")]
        public string DsReferenceNumber { get; set; }
        
        // Latest messaging version supported by ACS returned to PSD2 SDK,
        // default to 2.1.0 if we do not receive this in response
        [JsonProperty(PropertyName = "messageVersion")]
        public string MessageVersion { get; set; } = "2.1.0";

        // Localized strings for text elements in PSD2 native challenges
        [JsonProperty(PropertyName = "displayStrings")]
        public PSD2NativeChallengeLocalizations DisplayStrings { get; set; }
    }
}