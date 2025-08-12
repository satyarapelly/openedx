// <copyright file="AuthenticationResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model per the PayerAuth.V3 API.  
    /// This object is returned by PayerAuth.V3's POST /authenticate API
    /// This model is equivalent to the V2's ARes model.
    /// </summary>
    public class AuthenticationResponse
    {
        [JsonProperty(PropertyName = "enrollment_status")]
        public PaymentInstrumentEnrollmentStatus EnrollmentStatus { get; set; }

        [JsonProperty(PropertyName = "enrollment_type")]
        public PaymentInstrumentEnrollmentType EnrollmentType { get; set; }

        [JsonProperty(PropertyName = "three_ds_server_transaction_id")]
        public string ThreeDSServerTransactionId { get; set; }

        [JsonProperty(PropertyName = "acs_url")]
        public string AcsUrl { get; set; }

        [JsonProperty(PropertyName = "acs_transaction_id")]
        public string AcsTransactionId { get; set; }

        [JsonProperty(PropertyName = "authenticate_update_url")]
        public string AuthenticateUpdateUrl { get; set; }

        [JsonProperty(PropertyName = "acs_signed_content")]
        public string AcsSignedContent { get; set; }

        [JsonProperty(PropertyName = "transaction_challenge_status")]
        public TransactionStatus TransactionStatus { get; set; }

        [JsonProperty(PropertyName = "transaction_challenge_status_reason")]
        public TransactionStatusReason TransactionStatusReason { get; set; }

        [JsonProperty(PropertyName = "card_holder_info")]
        public string CardHolderInfo { get; set; }

        [JsonProperty(PropertyName = "acs_rendering_type")]
        public AcsRenderingType AcsRenderingType { get; set; }

        [JsonProperty(PropertyName = "acs_challenge_mandated")]
        public string AcsChallengeMandated { get; set; }

        [JsonProperty(PropertyName = "acs_operator_id")]
        public string AcsOperatorID { get; set; }

        [JsonProperty(PropertyName = "acs_reference_number")]
        public string AcsReferenceNumber { get; set; }

        [JsonProperty(PropertyName = "authentication_type")]
        public string AuthenticationType { get; set; }

        [JsonProperty(PropertyName = "ds_reference_number")]
        public string DsReferenceNumber { get; set; }

        // Message version supported by the ACS
        [JsonProperty(PropertyName = "message_version")]
        public string MessageVersion { get; set; }

        [JsonProperty(PropertyName = "is_form_post_acs_url")]
        public bool? IsFormPostAcsUrl { get; set; }

        [JsonProperty(PropertyName = "is_full_page_redirect")]
        public bool? IsFullPageRedirect { get; set; }

        [JsonProperty(PropertyName = "transaction_session_id")]
        public string TransactionSessionId { get; set; }
    }
}