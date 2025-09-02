// <copyright file="PaymentSession.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PXService.Model.PayerAuthService;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Newtonsoft.Json;
    using PayerAuth = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService;

    /// <summary>
    /// This is a model used by PXService internally to store and retrieve data from SessionService
    /// </summary>
    public class PaymentSession : PayerAuth.PaymentSession
    {
        public PaymentSession() : base()
        {
        }

        public PaymentSession(
            List<string> exposedFlightFeatures,
            string payerAuthApiVersion,
            PayerAuth.PaymentSession payerAuthPaymentSession,
            string accountId,
            string billableAccountId,
            string classicProduct,
            string emailAddress,
            string language,
            bool isGuestCheckout = false,
            string requestId = null,
            string tenantId = null) 
            : base(rhs: payerAuthPaymentSession)
        {
            this.ExposedFlightFeatures = exposedFlightFeatures == null ? null : new List<string>(exposedFlightFeatures);
            this.PayerAuthApiVersion = payerAuthApiVersion;
            this.AccountId = accountId;
            this.BillableAccountId = billableAccountId;
            this.ClassicProduct = classicProduct;
            this.EmailAddress = emailAddress;
            this.Language = language;
            this.HandlerVersion = PaymentSessionsHandler.HandlerVersion;
            this.IsGuestCheckout = isGuestCheckout;
            this.RequestId = requestId;
            this.TenantId = tenantId;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set for serialization")]
        public List<string> ExposedFlightFeatures { get; set; }

        // We need this to maintain the same version throughout the lifespan of a session
        public string PayerAuthApiVersion { get; set; }

        // This is the id of the caller (actor) making the service request.  This may be different
        // from the PaymentInstrumentAccountId (customer).  For example, this may be the CSS agent
        // We need to store this because we wont get this from the browser during authentication and
        // challenge completion
        [JsonProperty(PropertyName = "AccountId")]
        public string AccountId { get; set; }

        // BrowserInfo extracted from the GET /challengeDescriptions call
        [JsonProperty(PropertyName = "BrowserInfo")]
        public PayerAuth.BrowserInfo BrowserInfo { get; set; }

        // MethodData from the POST /PayerAuth.V3.GetThreeDSMethodURL call
        [JsonProperty(PropertyName = "MethodData")]
        public PayerAuth.ThreeDSMethodData MethodData { get; set; }

        // Response from the POST /PayerAuth.V3.Authenticate call
        [JsonProperty(PropertyName = "AuthenticationResponse")]
        public PayerAuth.AuthenticationResponse AuthenticationResponse { get; set; }

        [JsonProperty(PropertyName = GlobalConstants.QueryParamNames.BillableAccountId)]
        public string BillableAccountId { get; set; }

        [JsonProperty(PropertyName = GlobalConstants.QueryParamNames.ClassicProduct)]
        public string ClassicProduct { get; set; }

        [JsonProperty(PropertyName = "EmailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = "Language")]
        public string Language { get; set; }

        // ru provided by storefronts
        [JsonProperty(PropertyName = "SuccessUrl")]
        public string SuccessUrl { get; set; }

        // rx provided by storefronts
        [JsonProperty(PropertyName = "FailureUrl")]
        public string FailureUrl { get; set; }

        [JsonProperty(PropertyName = "ChallengeStatus")]
        public PaymentChallengeStatus ChallengeStatus { get; set; }

        [JsonProperty(PropertyName = "challengeType")]
        public string ChallengeType { get; set; }

        [JsonProperty(PropertyName = "transaction_challenge_status")]
        public TransactionStatus? TransactionStatus { get; set; }

        [JsonProperty(PropertyName = "transaction_challenge_status_reason")]
        public TransactionStatusReason? TransactionStatusReason { get; set; }

        [JsonProperty(PropertyName = "transaction_challenge_cancel_indicator")]
        public string ChallengeCancelIndicator { get; set; }

        [JsonProperty(PropertyName = "is_system_error")]
        public bool IsSystemError { get; set; }

        // Convert this into a string for version
        [JsonProperty(PropertyName = "is_payment_sessions_handler_v2")]
        public string HandlerVersion { get; set; }

        [JsonProperty(PropertyName = "isGuestCheckout")]
        public bool IsGuestCheckout { get; set; }

        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }
    }
}