// <copyright file="PaymentInstrumentDetails.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Tests.Common.Model.Pims
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

        [JsonProperty(PropertyName = "defaultDisplayName")]
        public string DefaultDisplayName { get; set; }

        [JsonProperty(PropertyName = "hashIdentity")]
        public string HashIdentity { get; set; }

        [JsonProperty(PropertyName = "pendingOn")]
        public string PendingOn { get; set; }

        [JsonProperty(PropertyName = "pendingDetails")]
        public object PendingDetails { get; set; }

        [JsonProperty(PropertyName = "exportable")]
        public bool Exportable { get; set; }

        [JsonProperty(PropertyName = "accountHolderName")]
        public string CardHolderName { get; set; }

        [JsonProperty(PropertyName = "accountToken")]
        public string TokenizedAccountNumber { get; set; }

        [JsonProperty(PropertyName = "cvvToken")]
        public string TokenizedCvv { get; set; }

        [JsonProperty(PropertyName = "address")]
        public AddressInfo Address { get; set; }

        [JsonProperty(PropertyName = "bankIdentificationNumber")]
        public string BankIdentificationNumber { get; set; }

        [JsonProperty(PropertyName = "cardType")]
        public string CardType { get; set; }

        [JsonProperty(PropertyName = "lastFourDigits")]
        public string LastFourDigits { get; set; }

        [JsonProperty(PropertyName = "expiryYear")]
        public string ExpiryYear { get; set; }

        [JsonProperty(PropertyName = "expiryMonth")]
        public string ExpiryMonth { get; set; }

        /// <summary>
        /// Gets or sets token returned from external provider, such as BillDesk and/or Stripe
        /// </summary>
        [JsonProperty("providerToken")]
        public ProviderToken ProviderToken { get; set; }

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

        [JsonProperty(PropertyName = "phone"), JsonConverter(typeof(PhoneConverter))]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "msisdn")]
        public string Msisdn { get; set; }

        [JsonProperty(PropertyName = "paymentAccount")]
        public string PaymentAccount { get; set; }

        [JsonProperty(PropertyName = "picvRequired")]
        public bool PicvRequired { get; set; }

        [JsonProperty(PropertyName = "bankName")]
        public string BankName { get; set; }

        [JsonProperty(PropertyName = "bankCode")]
        public string BankCode { get; set; }

        [JsonProperty(PropertyName = "bankAccountType")]
        public string BankAccountType { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        [JsonProperty(PropertyName = "lots")]
        public List<StoredValueLotDetails> Lots { get; set; }

        [JsonProperty(PropertyName = "appSignUrl")]
        public string AppSignUrl { get; set; }

        [JsonProperty(PropertyName = "picvDetails")]
        public PicvDetailsInfo PicvDetails { get; set; }

        [JsonProperty(PropertyName = "vpa")]
        public string Vpa { get; set; }

        [JsonProperty(PropertyName = "transactionLink")]
        public TransactionLink TransactionLink { get; set; }

        [JsonProperty("usageType", NullValueHandling = NullValueHandling.Ignore)]
        public UsageType? UsageType { get; set; }

        public class PicvDetailsInfo
        {
            [JsonProperty(PropertyName = "status")]
            public string Status { get; set; }

            [JsonProperty(PropertyName = "remainingAttempts")]
            public string RemainingAttempts { get; set; }
        }

        [JsonProperty(PropertyName = "sessionQueryUrl")]
        public string SessionQueryUrl { get; set; }

        [JsonProperty(PropertyName = "originMarket")]
        public string OriginMarket { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "userLoginId")]
        public string UserLoginId { get; set; }

        [JsonProperty("networkTokens")]
        public List<NetworkToken> NetworkTokens { get; set; }

        [JsonProperty("taxAmount", NullValueHandling = NullValueHandling.Ignore)]
        public decimal TaxAmount { get; set; }
    }
}