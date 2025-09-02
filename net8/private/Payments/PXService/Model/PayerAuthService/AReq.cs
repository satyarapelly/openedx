// <copyright file="AReq.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using System.Collections.Generic;
    using Common.Web;
    using Newtonsoft.Json;
    using ThreeDSExternalService;

    [JsonConverter(typeof(EnumJsonConverter))]
    public enum ChallengeType
    {
        Cvv,
        Sms
    }

    [JsonConverter(typeof(EnumJsonConverter))]
    public enum RiskChallengeIndicator
    {
        NoPreference, NoChallengeRequested, ChallengeRequestedPreference, ChallengeRequestedMandate
    }

    [JsonConverter(typeof(EnumJsonConverter))]
    public enum ThreeDSecureScenario
    {
        PaymentTransaction, RecurringTransaction, AddCard
    }

    public class AReq
    {
        [JsonProperty(PropertyName = "session_id")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "payment_instrument_id")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty(PropertyName = "account_id")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "is_moto")]
        public bool IsMoto { get; set; }

        [JsonProperty(PropertyName = "three_dsecure_scenario")]
        public ThreeDSecureScenario ThreeDSecureScenario { get; set; }

        [JsonProperty(PropertyName = "risk_challenge_indicator")]
        public RiskChallengeIndicator RiskChallengeIndicator { get; set; }

        [JsonProperty(PropertyName = "three_ds_server_transaction_id")]
        public string ThreeDSServerTransactionId { get; set; }

        [JsonProperty(PropertyName = "additional_challenge_data")]
        public Dictionary<ChallengeType, string> AdditionalChallengeData { get; }

        [JsonProperty(PropertyName = "device_channel")]
        public DeviceChannel DeviceChannel { get; set; }

        [JsonProperty(PropertyName = "browser_info")]
        public BrowserInfo BrowserInfo { get; set; }

        [JsonProperty(PropertyName = "sdk_info")]
        public SdkInfo SdkInfo { get; set; }

        [JsonProperty(PropertyName = "three_dsecure_method_completion_indicator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ThreeDSecureMethodCompletionIndicator { get; set; }

        [JsonProperty(PropertyName = "acs_challenge_notification_url")]
        public string AcsChallengeNotificationUrl { get; set; }
    }
}