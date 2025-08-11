// <copyright file="TestContext.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class TestContext
    {
        public const string RealScenario = "real";

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

        public const string RefundUnknownScenario = "refundunknown";
        public const string RefundDeclineScenario = "refunddecline";
        public const string RefundPendingScenario = "refundpending";
        public const string RefundApproveScenario = "refundapprove";
        public const string RefundFailScenario = "refundfail";

        public const string RefundReverseApproveScenario = "refundreverseapprove";

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

        public const string ChargeReverseUnknownScenario = "chargereverseunknown";
        public const string ChargeReverseDeclineScenario = "chargereversedecline";
        public const string ChargeReversePendingScenario = "chargereversepending";
        public const string ChargeReverseApproveScenario = "chargereverseapprove";
        public const string ChargeReverseFailScenario = "chargereversefail";

        public const string ChargeFileRejectScenario = "chargefilereject";
        public const string CreditFileRejectScenario = "creditfilereject";

        // TODO, try to remove it if we find all the usage.
        public const string DeclineChargeScenario = "declinecharge";

        public const string ChargebackScenario = "chargeback";
        public const string ChargebackReversalScenario = "chargebackreversal";

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

        public const string NotPaymentInsturmentNotifyScenario = "notpaymentinstrumentnotify";

        public const string MockSmsProviderScenario = "mocksmsprovider";

        public const string PublicFileExchangeSampleData = "publicfileexchangesampledata";
        public const string PublicFileExchangeNotFound = "publicfileexchangenotfound";

        public const string CashBalanceServiceTestScenario = "cashbalanceservicetest";

        public const string TipScenario = "tip";

        public const string QueryServiceReturnPIReferenceTrue = "queryservicereturnpireferencetrue";

        public const string APMChargeSuccessScenario = "APMchargesuccess";
        public const string APMChargeFailureScenario = "APMchargefailure";
        public const string APMTestScenarioPaymodProdConnectToAdyenInt = "PaymodToAdyenInt";

        public const string TokenizedCardError = "tokenizedcarderror";

        public const string EmulatorScenario = "emulator";

        public const string MDollarPurchase = "mdollarpurchase";

        public const string TestPaymentInstrumentScenario = "testpaymentinstrument";

        public const string PartitionTipScenario = "partitiontip";

        public const string MultiProviderSelectionScenario = "multiproviderselection";

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

        private string scenarios;
        private List<string> scenariosList;

        public TestContext()
        {
            this.Scenarios = string.Empty;
            this.ContextProps = new Dictionary<string, object>();
            this.ConfigurationContexts = new List<ConfigurationContext>();
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

        [JsonProperty(PropertyName = "configuration_contexts")]
        public List<ConfigurationContext> ConfigurationContexts { get; private set; }

        [JsonIgnore]
        public List<string> ScenarioList
        {
            get
            {
                return this.scenariosList;
            }
        }

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

        public bool ReplaceTestHeader(string origin, string target)
        {
            bool result = false;
            for (int i  = 0; i < this.ScenarioList.Count; i++)
            {
                if (this.ScenarioList[i] == origin)
                {
                    this.ScenarioList[i] = target;
                    result = true;
                }
            }

            this.scenarios = string.Join(",", this.ScenarioList.ToArray());
            return result;
        }
    }
}