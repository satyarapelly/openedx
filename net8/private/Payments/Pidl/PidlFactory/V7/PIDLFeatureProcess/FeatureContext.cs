// <copyright file="FeatureContext.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PimsModel.V4;

    /// <summary>
    /// Class maintaining the paramters used for feature enablement process
    /// </summary>
    public class FeatureContext
    {
        public FeatureContext(
            string country,
            string partner,
            string resourceType,
            string operationType,
            string scenario,
            string language,
            HashSet<PaymentMethod> paymentMethods,
            List<string> exposedFlightFeatures,
            Dictionary<string, FeatureConfig> featureConfigs = null,
            string paymentMethodfamily = null,
            string paymentMethodType = null,
            string typeName = null,
            List<string> smdMarkets = null,
            string originalPartner = null,
            string pmGroupPageId = null,
            bool isGuestAccount = false,
            bool avsSuggest = false,
            string originalTypeName = null,
            string shortUrl = null,
            string contextDescriptionType = null,
            string tokenizationPublicKey = null,
            Dictionary<string, string> tokenizationServiceUrls = null,
            EventTraceActivity traceActivityId = null,
            string sessionId = null,
            string accountId = null,
            string defaultPaymentMethod = null,
            List<string> xmsFlightHeader = null,
            List<PaymentInstrument> paymentInstruments = null,
            List<PaymentInstrument> disabledPaymentInstruments = null,
            string filters = null)
        {
            this.Country = country;
            this.Partner = partner;
            this.ResourceType = resourceType;
            this.OperationType = operationType;
            this.Scenario = scenario;
            this.Language = language;
            this.PaymentMethods = paymentMethods;
            this.ExposedFlightFeatures = exposedFlightFeatures;
            this.FeatureConfigs = featureConfigs;
            this.PaymentMethodfamily = paymentMethodfamily;
            this.PaymentMethodType = paymentMethodType;
            this.TypeName = typeName;
            this.SmdMarkets = smdMarkets;
            this.OriginalPartner = originalPartner;
            this.PMGroupPageId = pmGroupPageId;
            this.IsGuestAccount = isGuestAccount;
            this.TraceActivityId = traceActivityId;
            this.SessionId = sessionId;
            this.AccountId = accountId;
            this.AvsSuggest = avsSuggest;
            this.OriginalTypeName = originalTypeName;
            this.ShortUrl = shortUrl;
            this.ContextDescriptionType = contextDescriptionType;
            this.TokenizationPublicKey = tokenizationPublicKey;
            this.TokenizationServiceUrls = tokenizationServiceUrls;
            this.DefaultPaymentMethod = defaultPaymentMethod;
            this.XMSFlightlHeader = xmsFlightHeader;
            this.PaymentInstruments = paymentInstruments;
            this.DisabledPaymentInstruments = disabledPaymentInstruments;
            this.Filters = filters;
        }

        /// <summary>
        /// Gets the value of the Country
        /// </summary>
        public string Country { get; private set; }

        /// <summary>
        /// Gets or sets the value of the Partner
        /// </summary>
        public string Partner { get; set; }

        /// <summary>
        /// Gets or sets the value of the OriginalPartner while <see cref="Partner"/> can be used to pass template or partner
        /// </summary>
        public string OriginalPartner { get; set; }

        /// <summary>
        /// Gets or sets the value of the ResourceType
        /// </summary>
        public string ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the value of the OperationType
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// Gets or sets the value of the Scenario
        /// </summary>
        public string Scenario { get; set; }

        /// <summary>
        /// Gets the value of the Language
        /// </summary>
        public string Language { get; private set; }

        /// <summary>
        /// Gets the value of the PaymentMethods
        /// </summary>
        public HashSet<PaymentMethod> PaymentMethods { get; }

        /// <summary>
        /// Gets the value of the ExposedFlightFeatures
        /// </summary>
        public List<string> ExposedFlightFeatures { get; }

        /// <summary>
        /// Gets or sets - Get the feature provided by Partner Settings Service, Sets the feature provided in FeatureConfiguration for the partners not onboarded to Partner Settings Service
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set for partners not onboarded to partner setting service")]
        public Dictionary<string, FeatureConfig> FeatureConfigs { get; set; }

        /// <summary>
        /// Gets the value of the payment method family
        /// </summary>
        public string PaymentMethodfamily { get; private set; }

        /// <summary>
        /// Gets the value of the payment method type
        /// </summary>
        public string PaymentMethodType { get; private set; }

        /// <summary>
        /// Gets or sets the value of the type of a PIDL form (e.g. type of address/profile)
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the value of the OriginalTypeName of a PIDL form
        /// </summary>
        public string OriginalTypeName { get; set; }

        /// <summary>
        /// Gets the value of the smdMarkets
        /// </summary>
        public List<string> SmdMarkets { get; }

        /// <summary>
        /// Gets or sets the value of the pageId
        /// </summary>
        public string PMGroupPageId { get; set; }

        /// <summary>
        /// Gets a value indicating whether the request if for a guest account
        /// </summary>
        public bool IsGuestAccount { get; }

        /// <summary>
        /// Gets or sets a value indicating whether avs suggestion should be shown or not
        /// </summary>
        public bool AvsSuggest { get; set; }

        /// <summary>
        /// Gets or sets the value of the shortUrl
        /// </summary>
        public string ShortUrl { get; set; }

        /// <summary>
        /// Gets or sets the value of the context description type
        /// </summary>
        public string ContextDescriptionType { get; set; }

        /// <summary>
        /// Gets or sets the encryption key to add to the PAN/CVV data protection.
        /// </summary>
        public string TokenizationPublicKey { get; set; }

        /// <summary>
        /// Gets the tokenization URL based on the environment for PAN/CVV data protection.
        /// </summary>
        public Dictionary<string, string> TokenizationServiceUrls { get; private set; }

        /// <summary>
        /// Gets the trace activity id
        /// </summary>
        public EventTraceActivity TraceActivityId { get; }

        /// <summary>
        /// Gets the sessionId for the session
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// Gets the users account id
        /// </summary>
        public string AccountId { get; }

        /// <summary>
        /// Gets or sets the default payment method value
        /// </summary>
        public string DefaultPaymentMethod { get; set; }

        /// <summary>
        /// Gets x-ms-flight from additional headers passed in thr URL.
        /// </summary>
        public List<string> XMSFlightlHeader { get; }

        /// <summary>
        /// Gets or sets PaymentInstrumentList.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to set if only required flow hits")]
        public List<PaymentInstrument> PaymentInstruments { get; set; }

        /// <summary>
        /// Gets or sets DisabledPaymentInstrumentList.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to set if only required flow hits")]
        public List<PaymentInstrument> DisabledPaymentInstruments { get; set; }

        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        public string Filters { get; set; }
    }
}