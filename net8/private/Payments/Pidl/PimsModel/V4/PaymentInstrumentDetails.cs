// <copyright file="PaymentInstrumentDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class PaymentInstrumentDetails
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        [JsonProperty(PropertyName = "requiredChallenge")]
        public List<string> RequiredChallenge { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        [JsonProperty(PropertyName = "supportedChallenge")]
        public List<string> SurpportedChallenge { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        [JsonProperty(PropertyName = "metadata")]
        public Dictionary<string, string> MetaData { get; set; }

        [JsonProperty(PropertyName = "hashIdentity")]
        public string HashIdentity { get; set; }

        [JsonProperty(PropertyName = "pendingOn")]
        public string PendingOn { get; set; }

        [JsonProperty(PropertyName = "sessionQueryUrl")]
        public string SessionQueryUrl { get; set; }

        [JsonProperty(PropertyName = "pendingDetails")]
        public object PendingDetails { get; set; }

        [JsonProperty(PropertyName = "exportable")]
        public bool Exportable { get; set; }

        [JsonProperty(PropertyName = "daysUntilExpired")]
        public int? DaysUntilExpired { get; set; }

        [JsonProperty(PropertyName = "walletType")]
        public string WalletType { get; set; }

        #region Credit Card

        [JsonProperty(PropertyName = "accountHolderName")]
        public string CardHolderName { get; set; }

        [JsonProperty(PropertyName = "accountToken")]
        public string TokenizedAccountNumber { get; set; }

        [JsonProperty(PropertyName = "cvvToken")]
        public string TokenizedCvv { get; set; }

        /// <summary>
        /// Gets or sets token returned from external provider, such as BillDesk and/or Stripe
        /// </summary>
        [JsonProperty("providerToken")]
        public ProviderToken ProviderToken { get; set; }

        [JsonProperty(PropertyName = "address")]
        public AddressInfo Address { get; set; }

        [JsonProperty(PropertyName = "bankIdentificationNumber")]
        public string BankIdentificationNumber { get; set; }

        [JsonProperty(PropertyName = "secureDataId")]
        public string SecureDataId { get; set; }

        [JsonProperty(PropertyName = "cardType")]
        public string CardType { get; set; }

        // Delete once flight is no longer in use.
        [JsonProperty(PropertyName = "isIndiaExpiryGroupDeleteFlighted")]
        public bool IsIndiaExpiryGroupDeleteFlighted { get; set; }

        [JsonProperty(PropertyName = "lastFourDigits")]
        public string LastFourDigits { get; set; }

        [JsonProperty(PropertyName = "expiryYear")]
        public string ExpiryYear { get; set; }

        [JsonProperty(PropertyName = "expiryMonth")]
        public string ExpiryMonth { get; set; }

        [JsonProperty(PropertyName = "transactionLink")]
        public TransactionLink TransactionLink { get; set; }

        [JsonProperty("usageType", NullValueHandling = NullValueHandling.Ignore)]
        public UsageType? UsageType { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be set when deserializing the server response.")]
        [JsonProperty("networkTokens", NullValueHandling = NullValueHandling.Ignore)]
        public List<NetworkToken> NetworkTokens { get; set; }

        [JsonProperty("taxAmount", NullValueHandling = NullValueHandling.Ignore)]
        public decimal TaxAmount { get; set; }
        #endregion

        #region Paypal

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "redirectUrl")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "billingAgreementId")]
        public string BillingAgreementId { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "middleName")]
        public string MiddleName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "payerId")]
        public string PayerId { get; set; }

        [JsonProperty(PropertyName = "billingAgreementType")]
        public string BillingAgreementType { get; set; }
        #endregion

        #region Venmo
        [JsonProperty(PropertyName = "originMarket")]
        public string OriginMarket { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }
        #endregion

        #region CUP

        [JsonProperty(PropertyName = "phone"), JsonConverter(typeof(PhoneConverter))]
        public string Phone { get; set; }

        #endregion

        #region Non Sim Mobi

        [JsonProperty(PropertyName = "msisdn")]
        public string Msisdn { get; set; }

        [JsonProperty(PropertyName = "paymentAccount")]
        public string PaymentAccount { get; set; }

        #endregion

        #region Sepa

        [JsonProperty(PropertyName = "picvRequired")]
        public bool PicvRequired { get; set; }

        [JsonProperty(PropertyName = "bankName")]
        public string BankName { get; set; }

        [JsonProperty(PropertyName = "picvDetails")]
        public PicvDetailsInfo PicvDetails { get; set; }

        #endregion

        #region ACH

        [JsonProperty(PropertyName = "bankCode")]
        public string BankCode { get; set; }

        [JsonProperty(PropertyName = "bankAccountType")]
        public string BankAccountType { get; set; }

        #endregion

        #region StoredValue

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        [JsonProperty(PropertyName = "lots")]
        public List<StoredValueLotDetails> Lots { get; set; }

        #endregion

        #region AliPay

        [JsonProperty(PropertyName = "appSignUrl")]
        public string AppSignUrl { get; set; }

        #endregion

        #region Legacy Invoice

        [JsonProperty(PropertyName = "companyPONumber")]
        public string CompanyPONumber { get; set; }

        #endregion

        #region PX Extension
        [JsonProperty(PropertyName = "defaultDisplayName")]
        public string DefaultDisplayName { get; set; }

        [JsonProperty(PropertyName = "isFullPageRedirect")]
        public bool? IsFullPageRedirect { get; set; }
        #endregion

        #region Direct Debit
        [JsonProperty(PropertyName = "bankAccountLastFourDigits")]
        public string BankAccountLastFourDigits { get; set; }

        [JsonProperty(PropertyName = "issuer")]
        public string Issuer { get; set; }
        #endregion

        #region XboxCobrandedCard
        [JsonProperty(PropertyName = "isXboxCoBrandedCard")]
        public bool? IsXboxCoBrandedCard { get; set; }

        [JsonProperty(PropertyName = "pointsBalanceDetails")]
        public PointsBalanceDetails PointsBalanceDetails { get; set; }
        #endregion

        #region UPI         

        [JsonProperty(PropertyName = "vpa")]
        public string Vpa { get; set; }

        #endregion

        #region ANT PI's(PayPay, AlipayHK, GCash, TrueMoney and TouchNGo)

        [SuppressMessage("Microsoft.Naming", "CA1726", Justification = "We needed to use userLoginId")]
        [JsonProperty(PropertyName = "userLoginId")]
        public string UserLoginId { get; set; }

        #endregion

        public class PicvDetailsInfo
        {
            [JsonProperty(PropertyName = "status")]
            public string Status { get; set; }

            [JsonProperty(PropertyName = "remainingAttempts")]
            public string RemainingAttempts { get; set; }
        }
    }
}