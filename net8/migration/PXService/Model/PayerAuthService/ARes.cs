// <copyright file="ARes.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Common.Web;
    using Newtonsoft.Json;

    [JsonConverter(typeof(EnumJsonConverter))]
    public enum PaymentInstrumentEnrollmentStatus
    {
        Enrolled,
        NotEnrolled,
        Unavailable,
        Bypassed
    }

    [JsonConverter(typeof(EnumJsonConverter))]
    public enum PaymentInstrumentEnrollmentType
    {
        ThreeDs,
    }

    public class ARes
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

        [JsonProperty(PropertyName = "authenticate_value")]
        public string AuthenticateValue { get; set; }

        [JsonProperty(PropertyName = "eci")]
        public string Eci { get; set; }

        [JsonProperty(PropertyName = "acs_signed_content")]
        public string AcsSignedContent { get; set; }

        [JsonProperty(PropertyName = "ucaf")]
        public string Ucaf { get; set; }

        [JsonProperty(PropertyName = "cavv")]
        public string Cavv { get; set; }

        [JsonProperty(PropertyName = "xid")]
        public string Xid { get; set; }

        [JsonProperty(PropertyName = "cavv_algorithm")]
        public string CavvAlgorithm { get; set; }
    }
}