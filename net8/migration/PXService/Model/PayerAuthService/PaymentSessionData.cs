// <copyright file="PaymentSessionData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Microsoft.Commerce.Payments.PXService.Model.SessionService;
    using Newtonsoft.Json;
    using ThreeDSExternalService;
    using PXModel = V7.PaymentChallenge.Model;

    /// <summary>
    /// This is a model per the PayerAuth.V3 API.
    /// This object is sent as payload to PayerAuth.V3's POST /paymentSession API.
    /// </summary>
    public class PaymentSessionData
    {
        public PaymentSessionData()
        {
        }

        public PaymentSessionData(PaymentSessionData rhs)
        {
            this.PaymentInstrumentAccountId = rhs.PaymentInstrumentAccountId;
            this.PaymentInstrumentId = rhs.PaymentInstrumentId;
            this.CommercialAccountId = rhs.CommercialAccountId;

            this.Partner = rhs.Partner;
            this.Amount = rhs.Amount;
            this.Currency = rhs.Currency;
            this.Country = rhs.Country;
            this.HasPreOrder = rhs.HasPreOrder;
            this.IsLegacy = rhs.IsLegacy;
            this.IsMOTO = rhs.IsMOTO;
            this.ChallengeScenario = rhs.ChallengeScenario;

            this.PaymentMethodFamily = rhs.PaymentMethodFamily;
            this.PaymentMethodType = rhs.PaymentMethodType;
            this.DeviceChannel = rhs.DeviceChannel;
            this.PiRequiresAuthentication = rhs.PiRequiresAuthentication;
            this.PurchaseOrderId = rhs.PurchaseOrderId;
            this.UserId = rhs.UserId;
        }

        public PaymentSessionData(
            string accountId,
            PXModel.PaymentSessionData data,
            PimsModel.V4.PaymentInstrument paymentInstrument,
            DeviceChannel deviceChannel,
            bool piRequiresAuthentication)
        {
            this.PaymentInstrumentAccountId = accountId;
            this.PaymentInstrumentId = data.PaymentInstrumentId;
            this.CommercialAccountId = data.CommercialAccountId;

            this.Partner = data.Partner;
            this.Amount = data.Amount;
            this.Currency = data.Currency;
            this.Country = data.Country;
            this.HasPreOrder = data.HasPreOrder;
            this.IsLegacy = data.IsLegacy;
            this.IsMOTO = data.IsMOTO;
            this.IsPoints = data.RedeemRewards;
            this.RewardsPoints = data.RewardsPoints;
            this.ChallengeScenario = PaymentSessionData.FromPXChallengeScenario(data.ChallengeScenario);
            this.PurchaseOrderId = data.PurchaseOrderId;
            this.PaymentMethodFamily = paymentInstrument.PaymentMethod.PaymentMethodFamily;
            this.PaymentMethodType = paymentInstrument.PaymentMethod.PaymentMethodType;
            this.DeviceChannel = deviceChannel;
            this.PiRequiresAuthentication = piRequiresAuthentication;
        }

        // TODO: Confrim that this is the id of the user making the request (Customer or a CSS agent)
        [JsonProperty(PropertyName = "account_id")]
        public string PaymentInstrumentAccountId { get; set; }

        // This is the ID created by OMS for OMS Legacy purchases.  Its also called OCPTenantId.
        [JsonProperty(PropertyName = "caid")]
        public string CommercialAccountId { get; set; }

        [JsonProperty(PropertyName = "payment_instrument_id")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty(PropertyName = "partner")]
        public string Partner { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "has_pre_order")]
        public bool HasPreOrder { get; set; }

        [JsonProperty(PropertyName = "is_legacy")]
        public bool IsLegacy { get; set; }

        [JsonProperty(PropertyName = "is_moto")]
        public bool IsMOTO { get; set; }

        [JsonProperty(PropertyName = "three_dsecure_scenario")]
        public ChallengeScenario ChallengeScenario { get; set; }

        [JsonProperty(PropertyName = "payment_method_family")]
        public string PaymentMethodFamily { get; set; }

        [JsonProperty(PropertyName = "payment_method_type")]
        public string PaymentMethodType { get; set; }

        [JsonProperty(PropertyName = "device_channel")]
        public DeviceChannel DeviceChannel { get; set; }

        [JsonProperty(PropertyName = "is_challenge_required")]
        public bool PiRequiresAuthentication { get; set; }

        [JsonProperty(PropertyName = "purchase_order_id")]
        public string PurchaseOrderId { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "cvv_token ")]
        public string CvvToken { get; set; }

        [JsonProperty(PropertyName = "is_points")]
        public bool IsPoints { get; set; }

        [JsonProperty(PropertyName = "rewards_points")]
        public int RewardsPoints { get; set; }

        [JsonProperty(PropertyName = "context")]
        public SecondScreenSessionData Context { get; set; }

        private static ChallengeScenario FromPXChallengeScenario(PXModel.ChallengeScenario challengeScenario)
        {
            switch (challengeScenario)
            {
                case PXModel.ChallengeScenario.AddCard:
                    return ChallengeScenario.AddCard;
                case PXModel.ChallengeScenario.PaymentTransaction:
                    return ChallengeScenario.PaymentTransaction;
                case PXModel.ChallengeScenario.RecurringTransaction:
                    return ChallengeScenario.RecurringTransaction;
                default:
                    throw new IntegrationException(
                        GlobalConstants.ServiceName,
                        "An Enum type in PXModel.ChallengeScenario does not have a mapping in PayerAuth.ChallengeScenario",
                        "MissingEnumDefinition");
            }
        }
    }
}