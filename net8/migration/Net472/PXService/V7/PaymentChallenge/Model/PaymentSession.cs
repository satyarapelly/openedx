// <copyright file="PaymentSession.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
{
    using Newtonsoft.Json;
    using PayerAuth = PXService.Model.PayerAuthService;

    public class PaymentSession : PaymentSessionData
    {
        public PaymentSession()
        {
        }

        public PaymentSession(
            string id, 
            bool isChallengeRequired, 
            string signture, 
            PaymentChallengeStatus challengeStatus)
        {
            this.Id = id;
            this.IsChallengeRequired = isChallengeRequired;
            this.Signature = signture;
            this.ChallengeStatus = challengeStatus;
        }

        public PaymentSession(PaymentSessionData data)
        {
            this.Id = null;
            this.IsChallengeRequired = false;
            this.Signature = null;
            this.ChallengeStatus = PaymentChallengeStatus.Unknown;
            this.PaymentInstrumentId = data.PaymentInstrumentId;
            this.Language = data.Language;
            this.Partner = data.Partner;
            this.PaymentInstrumentAccountId = data.PaymentInstrumentAccountId;
            this.Amount = data.Amount;
            this.Currency = data.Currency;
            this.Country = data.Country;
            this.HasPreOrder = data.HasPreOrder;
            this.CommercialAccountId = data.CommercialAccountId;
            this.IsLegacy = data.IsLegacy;
            this.IsMOTO = data.IsMOTO;
            this.ChallengeScenario = data.ChallengeScenario;
            this.ChallengeWindowSize = data.ChallengeWindowSize;
            this.BillableAccountId = data.BillableAccountId;
            this.ClassicProduct = data.ClassicProduct;
            this.PurchaseOrderId = data.PurchaseOrderId;
            this.PaymentMethodType = data.PaymentMethodType;
            this.RedeemRewards = data.RedeemRewards;
            this.RewardsPoints = data.RewardsPoints;
        }

        public PaymentSession(PXService.Model.PXInternal.PaymentSession internalPaymentSession)
        {
            this.IsChallengeRequired = internalPaymentSession.PiRequiresAuthentication;
            this.Signature = null;
            this.ChallengeStatus = PaymentChallengeStatus.Unknown;

            this.Language = internalPaymentSession.Language;

            this.Id = internalPaymentSession.Id;

            this.CommercialAccountId = internalPaymentSession.CommercialAccountId;
            this.PaymentInstrumentAccountId = internalPaymentSession.PaymentInstrumentAccountId;
            this.PaymentInstrumentId = internalPaymentSession.PaymentInstrumentId;

            this.Partner = internalPaymentSession.Partner;
            this.Amount = internalPaymentSession.Amount;
            this.Currency = internalPaymentSession.Currency;
            this.Country = internalPaymentSession.Country;
            this.HasPreOrder = internalPaymentSession.HasPreOrder;
            this.IsLegacy = internalPaymentSession.IsLegacy;
            this.IsMOTO = internalPaymentSession.IsMOTO;
            this.ChallengeScenario = FromPayerAuthChallengeScenario(internalPaymentSession.ChallengeScenario);
            this.PurchaseOrderId = internalPaymentSession.PurchaseOrderId;
            this.PaymentMethodType = internalPaymentSession.PaymentMethodType;

            this.SuccessUrl = internalPaymentSession.SuccessUrl;
            this.FailureUrl = internalPaymentSession.FailureUrl;

            this.RedeemRewards = internalPaymentSession.IsPoints;
            this.RewardsPoints = internalPaymentSession.RewardsPoints;
            this.RequestId = internalPaymentSession.RequestId;
            this.TenantId = internalPaymentSession.TenantId;
        }

        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "isChallengeRequired", Required = Required.Always)]
        public bool IsChallengeRequired { get; set; }

        [JsonProperty(PropertyName = "signature", Required = Required.Always)]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "challengeStatus", Required = Required.Always)]
        public PaymentChallengeStatus ChallengeStatus { get; set; }

        [JsonProperty(PropertyName = "sessionToken")]
        public string SessionToken { get; set; }

        [JsonProperty(PropertyName = "challengeType")]
        public string ChallengeType { get; set; }

        [JsonProperty(PropertyName = "userDisplayMessage")]
        public string UserDisplayMessage { get; set; }

        // ru provided by storefronts
        [JsonProperty(PropertyName = "SuccessUrl")]
        public string SuccessUrl { get; set; }

        // rx provided by storefronts
        [JsonProperty(PropertyName = "FailureUrl")]
        public string FailureUrl { get; set; }

        // request id for example checkout/payment request id
        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }

        // tenant id used for checkout/payment reques
        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }

        // Whether the gpay/apay token is collected or not
        [JsonProperty(PropertyName = "isTokenCollected")]
        public bool IsTokenCollected { get; set; }

        public string GenerateSignature()
        {
            // TODO: add implemention later
            return string.Format("placeholder_for_paymentsession_signature_{0}", this.Id);
        }

        public bool HasValidSignature()
        {
            // TODO: add implemention later
            return !string.IsNullOrEmpty(this.Signature);
        }

        private static ChallengeScenario FromPayerAuthChallengeScenario(PayerAuth.ChallengeScenario challengeScenario)
        {
            switch (challengeScenario)
            {
                case PayerAuth.ChallengeScenario.AddCard:
                    return ChallengeScenario.AddCard;
                case PayerAuth.ChallengeScenario.PaymentTransaction:
                    return ChallengeScenario.PaymentTransaction;
                case PayerAuth.ChallengeScenario.RecurringTransaction:
                    return ChallengeScenario.RecurringTransaction;
                default:
                    throw new IntegrationException(
                        GlobalConstants.ServiceName,
                        "An Enum type in PayerAuth.ChallengeScenario does not have a mapping in PXMode.ChallengeScenario",
                        "MissingEnumDefinition");
            }
        }
    }
}