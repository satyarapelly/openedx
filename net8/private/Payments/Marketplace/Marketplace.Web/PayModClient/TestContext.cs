// <copyright file="TestContext.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Marketplace.Web.PayModClient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class TestContext
    {
        public const string RedirectToTestScenario = "redirectToTest";

        public const string RealScenario = "real";

        public const string AdyenTimeoutScenario = "AdyenTimeoutScenario";

        public const string AuthorizeUnknownScenario = "authorizeunknown";
        public const string AuthorizeDeclineScenario = "authorizedecline";
        public const string AuthorizePendingScenario = "authorizepending";
        public const string AuthorizeApproveScenario = "authorizeapprove";
        public const string AuthorizeFailScenario = "authorizefail";

        public const string AuthorizationReverseUnknownScenario = "authorizationreverseunknown";
        public const string AuthorizationReverseDeclineScenario = "authorizationreversedecline";
        public const string AuthorizationReversePendingScenario = "authorizationreversepending";
        public const string AuthorizationReverseApproveScenario = "authorizationreverseapprove";
        public const string AuthorizationReverseFailScenario = "authorizationreversefail";
        public const string PreAuthorizationScenario = "preauth";

        public const string RefundUnknownScenario = "refundunknown";
        public const string RefundDeclineScenario = "refunddecline";
        public const string RefundPendingScenario = "refundpending";
        public const string RefundApproveScenario = "refundapprove";
        public const string RefundFailScenario = "refundfail";

        public const string RefundReverseApproveScenario = "refundreverseapprove";
        public const string RefundApproveThenDeclineScenario = "refundapprovethendecline";

        public const string SettleUnknownScenario = "settleunknown";
        public const string SettleDeclineScenario = "settledecline";
        public const string SettlePendingScenario = "settlepending";
        public const string SettleApproveScenario = "settleapprove";
        public const string SettleFailScenario = "settlefail";

        public const string SettleReverseUnknownScenario = "settlereverseunknown";
        public const string SettleReverseDeclineScenario = "settlereversedecline";
        public const string SettleReversePendingScenario = "settlereversepending";
        public const string SettleReverseApproveScenario = "settlereverseapprove";
        public const string SettleReverseFailScenario = "settlereversefail";

        public const string ChargeUnknownScenario = "chargeunknown";
        public const string ChargeDeclineScenario = "chargedecline";
        public const string ChargePendingScenario = "chargepending";
        public const string ChargeApproveScenario = "chargeapprove";
        public const string ChargeFailScenario = "chargefail";
        public const string ChargeProcessorFailScenario = "chargeprocessorfail";
        public const string ThreeDInvalidChargeDecline = "threedinvalidchargedecline";

        public const string ChargeReverseUnknownScenario = "chargereverseunknown";
        public const string ChargeReverseDeclineScenario = "chargereversedecline";
        public const string ChargeReversePendingScenario = "chargereversepending";
        public const string ChargeReverseApproveScenario = "chargereverseapprove";
        public const string ChargeReverseFailScenario = "chargereversefail";
        public const string ChargeReverseApproveThenFailScenario = "chargereverseapprovedthenfail";

        public const string ChargeFileRejectScenario = "chargefilereject";
        public const string CreditFileRejectScenario = "creditfilereject";

        // TODO, try to remove it if we find all the usage.
        public const string DeclineChargeScenario = "declinecharge";

        public const string ChargebackScenario = "chargeback";
        public const string ChargebackReversalScenario = "chargebackreversal";
        public const string SkipBackendChargebackScenario = "skipbackendchargeback";

        public const string NotNotifyScenario = "notnotify";

        public const string CreditUnknownScenario = "creditunknown";
        public const string CreditDeclineScenario = "creditdecline";
        public const string CreditAsyncDeclineScenario = "creditasyncdecline";

        public const string CreditApprovedScenarioWLBAI = "creditapprovedWLBAI";
        public const string CreditApprovedScenarioWLPOC = "creditapprovedWLPOC";
        public const string CreditAsyncDeclineScenarioWLPOR = "creditasyncdeclineWLPOR";
        public const string CreditAsyncDeclineScenarioWLFXRPT = "creditasyncdeclineWLFXRPT";

        public const string CreditPendingScenario = "creditpending";
        public const string CreditApproveScenario = "creditapprove";
        public const string CreditFailScenario = "creditfail";

        public const string CreditReverseUnknownScenario = "creditreverseunknown";
        public const string CreditReverseDeclineScenario = "creditreversedecline";
        public const string CreditReversePendingScenario = "creditreversepending";
        public const string CreditReverseApproveScenario = "creditreverseapprove";
        public const string CreditReverseFailScenario = "creditreversefail";

        public const string AsyncScenario = "async";

        public const string OnBehalfofPartnerScenario = "onbehalfofpartner";

        public const string OnBehalfofPartnerPropKey = "onbehalfofpartner";

        public const string NotPaymentInstrumentNotifyScenario = "notpaymentinstrumentnotify";
        public const string EndToEndRedirectScenario = "endtoendredirect";

        public const string MockSmsProviderScenario = "mocksmsprovider";

        public const string PublicFileExchangeSampleData = "publicfileexchangesampledata";
        public const string PublicFileExchangeNotFound = "publicfileexchangenotfound";
        public const string PublicFileExchangeValidateUpload = "publicfileexchangevalidateupload";

        public const string CashBalanceServiceTestScenario = "cashbalanceservicetest";

        public const string TipScenario = "tip";

        public const string QueryServiceReturnPIReferenceTrue = "queryservicereturnpireferencetrue";

        public const string APMChargeSuccessScenario = "APMchargesuccess";
        public const string APMChargeFailureScenario = "APMchargefailure";
        public const string APMTestScenarioPaymodProdConnectToAdyenInt = "PaymodToAdyenInt";

        public const string TokenizedCardError = "tokenizedcarderror";

        public const string EmulatorScenario = "emulator";
        public const string SkipaadScenario = "skipaad";

        public const string MDollarPurchase = "mdollarpurchase";

        public const string TestPaymentInstrumentScenario = "testpaymentinstrument";

        public const string PartitionTipScenario = "partitiontip";

        public const string MultiProviderSelectionScenario = "multiproviderselection";

        public const string SkipScenarioOverride = "skipscenariooverride";

        // Non-SIM mobi scenarios
        public const string NonSimMobiE2ESuccessScenario = "E2ESuccess";
        public const string NonSimMobiBeginEnrollmentSuccessScenario = "BeginEnrollmentSuccess";
        public const string NonSimMobiBeginEnrollmentAccountNotFoundScenario = "BeginEnrollmentAccountNotFound";
        public const string NonSimMobiCompleteEnrollmentSuccessScenario = "CompleteEnrollmentSuccess";
        public const string NonSimMobiCompleteEnrollmentAccountNotFoundScenario = "CompleteEnrollmentAccountNotFound";
        public const string NonSimMobiChallengeSuccessScenario = "ChallengeSuccess";
        public const string NonSimMobiResendOtpSuccessScenario = "ResendOtpSuccess";
        public const string NonSimMobiResendOtpExceededChallengeAttemptsScenario = "ResendOtpExceededChallengeAttempts";
        public const string NonSimMobiAccountNotFoundScenario = "AccountNotFoundTest";
        public const string NonSimMobiValidateChallengeSuccessScenario = "ValidateChallengeSuccess";
        public const string NonSimMobiInvalidOtpScenario = "InvalidOtpTest";

        // Monetary Commitment scenarios
        public const string MCBalanceIdNotFoundScenario = "MCBalanceIdNotFound";
        public const string MCInvalidInputScenario = "MCInvalidInput";
        public const string MCInsufficientFundScenario = "MCInsufficientFund";
        public const string MCOperationInProgressScenario = "MCOperationInProgress";
        public const string MCBalanceLockedScenario = "MCBalanceLocked";
        public const string MCInternalServerErrorScenario = "MCInternalServerError";
        public const string MCAbandonedScenario = "MCAbandoned";
        public const string MCCouldNotStartOperationScenario = "MCCouldNotStartOperation";
        public const string MCNoSessionIdScenario = "MCNoSessionId";
        public const string MCNoSessionDataScenario = "MCNoSessionData";
        public const string MCNotSupported = "MCNotSupported";
        public const string MCNoActiveLot = "MCNoActiveLot";
        public const string MCNoUsagesFound = "MCNoUsagesFound";
        public const string MCEventOnInactiveLot = "MCEventOnInactiveLot";
        public const string MCEventIdNotFound = "MCEventIdNotFound";
        public const string MCEventAlreadyCanceled = "MCEventAlreadyCanceled";
        public const string MCEventAlreadyRefunded = "MCEventAlreadyRefunded";
        public const string MCQuantityTooLarge = "MCQuantityTooLarge";

        // Risk scenarios
        public const string RiskTokenValidationCVV = "RiskTokenValidationCVV";
        public const string RiskTokenValidation3DS = "RiskTokenValidation3DS";
        public const string RiskTokenValidationCVV3DS = "RiskTokenValidationCVV3DS";
        public const string RiskTokenValidationInvalid = "RiskTokenValidationInvalid";

        // Risk Service Scenarios
        public const string RiskServiceTokenValidationValid = "risk-token-validation-valid";
        public const string RiskServiceTokenValidationInvalid = "risk-token-validation-invalid";
        public const string RiskServiceChallengeCVV3DS = "risk-challenge-ThreeDSAndCVV";

        // GenericPayments scenarios
        public const string GenericPaymentsInvalidCallbackRequest = "GenericPaymentsInvalidCallbackRequest";
        public const string GenericPaymentsInternalSystemError = "GenericPaymentsInternalSystemError";
        public const string GenericPaymentsTestStore = "GenericPaymentsTestStore";
        public const string DisclosureRequired = "DisclosureRequired";
        public const string SendNotification = "SendNotification";
        public const string PspAccountClosed = "PspAccountClosed";
        public const string PspAccountBarred = "PspAccountBarred";
        public const string PspAccountNotFound = "PspAccountNotFound";
        public const string PspUnsupportedAction = "PspUnsupportedAction";
        public const string PspSystemError = "PspSystemError";
        public const string PspPaymentMethodSystemError = "PspPaymentMethodSystemError";

        // Klarna Scenarios
        public const string KlarnaSkipValidateOnAddPi = "TestKlarnaAddPi-IgnoreValidate";
        public const string KlarnaValidate = "KlarnaValidate";
        public const string KlarnaValidateFailWithKlarnaError = "KlarnaValidateFailWithKlarnaError";

        // Adyen Checkout API Scenarios
        public const string AdyenCheckoutApiRedirect = "AdyenCheckoutApiRedirect";
        public const string AdyenCheckoutChargeWithTokenApproved = "AdyenCheckoutChargeWithTokenApproved";

        // Dynamic Retry Scenarios
        public const string FdcRetryableChargeScenario = "FdcRetryableCharge";
        public const string AdyenRetryableChargeScenario = "AdyenRetryableCharge";
        public const string PayPalInlineRetryScenario = "paypalinlineretryscenario";
        public const string PayPalInternalErrorRetryScenario = "paypalinternalerrorretryscenario";

        // Use QR code path for AliPayBillingAgreement.
        public const string AliPayQRCodePathScenario = "AliPayBA-UseQRCode";
        public const string AliPayQRCodePathSkipPiResumeScenario = "AliPayBA-UseQRCode-SkipPiResumeScenario";

        public const string ProviderOverrideContextPropertyName = "ProviderOverride";

        // ThreeDS scenarios
        public const string PxServiceE2eEmulatorScenario = "px-service-psd2-e2e-emulator";

        // AccountUpdater scenarios
        public const string AccountUpdaterDownloadTimeout = "AccountUpdaterDownloadTimeout";
        public const string AccountUpdaterUploadTimeout = "AccountUpdaterUploadTimeout";

        // BillDesk scenarios
        public const string SyncToLegacy = "SyncToLegacy";
        public const string SkipConfirmPaymentInstrument = "SkipConfirmPaymentInstrument";
        public const string SkipEmulatorRedirect = "SkipEmulatorRedirect";
        public const string IgnorePaymentInstrumentFormat = "IgnorePaymentInstrumentFormat";

        // Context Props
        public const string PSD2ScenarioOverride = "PSD2ScenarioOverride";
        public const string ExpectedAmount = "ExpectedAmount";
        public const string ExpectedUri = "ExpectedUri";

        // cvv
        public const string CvvMatched = "cvvmatched";
        public const string CvvUnmatched = "cvvunmatched";

        // wirecard
        public const string WireCardForce = "WireCardForce";

        // MasterCard PSD2
        public const string FailedResultRequest = "FailedResultRequest";
        public const string PSD2NameScenario = "PSD2NameScenario";

        // WorldPayWPG scenarios
        public const string WorldPayWPGChargeApproveWithPrimeRoutingResponse = "WorldPayWPGChargeApproveWithPrimeRoutingResponse";
        public const string WorldPayWPGAuthorizeSettledRefundApprove = "WorldPayWPGAuthorizeSettledRefundApprove";

        public const string WorldPayWPGGatewayError = "WorldPayWPGGatewayError";
        public const string WorldPayWPGInvalidExpiryDate = "WorldPayWPGInvalidExpiryDate";
        public const string WorldPayWPGCardNumberNotRecognized = "WorldPayWPGCardNumberNotRecognized";
        public const string WorldPayWPGCouldNotFindPaymentForOrder = "WorldPayWPGCouldNotFindPaymentForOrder";
        public const string WorldPayWPGCouldNotFindPaymentForOrderWithoutMerchantReferenceNumber = "WorldPayWPGCouldNotFindPaymentForOrderWithoutMerchantReferenceNumber";
        public const string WorldPayWPGOrderNotReady = "WorldPayWPGOrderNotReady";

        public const string WorldPayWPGReturnCode51LimitExceeded = "WorldPayWPGReturnCode51LimitExceeded";
        public const string WorldPayWPGReturnCode62RestrictedCard = "WorldPayWPGReturnCode62RestrictedCard";
        public const string WorldPayWPGReturnCode5Refused = "WorldPayWPGReturnCode5Refused";
        public const string WorldPayWPGReturnCode106InvalidAccount = "WorldPayWPGReturnCode106InvalidAccount";
        public const string WorldPayWPGReturnCode12InvalidTransaction = "WorldPayWPGReturnCode12InvalidTransaction";
        public const string WorldPayWPGReturnCode43StolenCardPickUp = "WorldPayWPGReturnCode43StolenCardPickUp";
        public const string WorldPayWPGReturnCode54DeclinedExpiredCard = "WorldPayWPGReturnCode54DeclinedExpiredCard";
        public const string WorldPayWPGReturnCode91CreditCardIssuerTemporarilyNotReachable = "WorldPayWPGReturnCode91CreditCardIssuerTemporarilyNotReachable";
        public const string WorldPayWPGReturnCode13InvalidAmount = "WorldPayWPGReturnCode13InvalidAmount";
        public const string WorldPayWPGReturnCode102InvalidCardIssuer = "WorldPayWPGReturnCode102InvalidCardIssuer";
        public const string WorldPayWPGReturnCode70ContactCardIssuer = "WorldPayWPGReturnCode70ContactCardIssuer";
        public const string WorldPayWPGReturnCode975Refused = "WorldPayWPGReturnCode975Refused";
        public const string WorldPayWPGReturnCode55InvalidSecurityCode = "WorldPayWPGReturnCode55InvalidSecurityCode";
        public const string WorldPayWPGReturnCode3InvalidAcceptor = "WorldPayWPGReturnCode3InvalidAcceptor";
        public const string WorldPayWPGReturnCode74AllowableNumberOfPinRetriesExceeded = "WorldPayWPGReturnCode74AllowableNumberOfPinRetriesExceeded";
        public const string WorldPayWPGQueryStatusError = "WorldPayWPGQueryStatusError";

        // FDC scenarios
        public const string FDCAccountUpdater = "FDCAccountUpdater";
        public const string FDCEncryptedCvv = "FDCEncryptedCvv";

        // BillDesk eMandate
        public const string BillDeskEMandate = "BillDeskEMandate";

        private string scenarios;
        private List<string> scenariosList;

        public TestContext()
        {
            this.Scenarios = string.Empty;
            this.ContextProps = new Dictionary<string, object>();
        }

        public TestContext(string contact, DateTime retention)
            : this()
        {
            this.Contact = contact;
            this.Retention = retention;
        }

        public TestContext(string contact, DateTime retention, string scenarios)
            : this(contact, retention)
        {
            this.Scenarios = scenarios;
        }

        [JsonProperty(PropertyName = "Scenarios")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "This is required for deserializing the previous version of serialized TextContext values")]
        public List<string> ScenarioBackCompatData
        {
            get
            {
                return null;
            }

            set
            {
                if (value != null)
                {
                    this.Scenarios = string.Join(",", value);
                }
            }
        }

        [JsonProperty(PropertyName = "scenarios")]
        public string Scenarios
        {
            get
            {
                return this.scenarios;
            }

            set
            {
                this.scenarios = value;
                this.scenariosList = new List<string>();
                if (!string.IsNullOrEmpty(this.scenarios))
                {
                    string[] scenariosArray = this.scenarios.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < scenariosArray.Length; ++i)
                    {
                        this.scenariosList.Add(scenariosArray[i].Trim());
                    }
                }
            }
        }

        [JsonProperty(PropertyName = "contact")]
        public string Contact { get; set; }

        [JsonProperty(PropertyName = "retention")]
        public DateTime Retention { get; set; }

        [JsonProperty(PropertyName = "context_props")]
        public IDictionary<string, object> ContextProps { get; private set; }

        public object this[string propName]
        {
            get
            {
                if (this.ContextProps.ContainsKey(propName))
                {
                    return this.ContextProps[propName];
                }

                return null;
            }

            set
            {
                if (this.ContextProps.ContainsKey(propName))
                {
                    this.ContextProps[propName] = value;
                }
                else
                {
                    this.ContextProps.Add(propName, value);
                }
            }
        }

        public bool ScenariosContain(string scenario)
        {
            return this.scenariosList.Contains(scenario);
        }
    }
}