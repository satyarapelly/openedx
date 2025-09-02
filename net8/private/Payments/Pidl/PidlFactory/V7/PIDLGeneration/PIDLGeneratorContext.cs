// <copyright file="PIDLGeneratorContext.cs" company="Microsoft">Copyright (c) Microsoft 2023. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PimsModel.V4;

    /// <summary>
    /// Class maintaining the paramters used for PIDL generation process
    /// </summary>
    public class PIDLGeneratorContext
    {
        public PIDLGeneratorContext(
            PaymentInstrument paymentInstrument,
            string accountId,
            string country,
            string originalPartner,
            string partner,
            string resourceType,
            string operationType,
            string paymentMethodType,
            string scenario,
            string language,
            string classicProduct,
            string sessionQueryUrl,
            bool completePrerequisites,
            string requestType,
            string billableAccountId,
            string emailAddress,
            List<string> exposedFlightFeatures,
            string sessionId,
            string pidlBaseUrl,
            string descriptionType,
            string shortUrl,
            PaymentExperienceSetting setting = null,
            EventTraceActivity traceActivityId = null)
        {
            this.AccountId = accountId;
            this.Country = country;
            this.OriginalPartner = originalPartner;
            this.Partner = partner;
            this.ResourceType = resourceType;
            this.OperationType = operationType;
            this.PaymentMethodType = paymentMethodType;
            this.Scenario = scenario;
            this.Language = language;
            this.ClassicProduct = classicProduct;
            this.SessionQueryUrl = sessionQueryUrl;
            this.CompletePrerequisites = completePrerequisites;
            this.RequestType = requestType;
            this.BillableAccountId = billableAccountId;
            this.EmailAddress = emailAddress;
            this.SessionId = sessionId;
            this.PidlBaseUrl = pidlBaseUrl;
            this.PaymentInstrument = paymentInstrument;
            this.ExposedFlightFeatures = exposedFlightFeatures;
            this.PartnerSetting = setting;
            this.DescriptionType = descriptionType;
            this.ShortUrl = shortUrl;
            this.TraceActivityId = traceActivityId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PIDLGeneratorContext" /> class. - Set required paramer for Handle payment challenge for add and purchase flow.
        /// </summary>
        /// <param name="country">Current country</param>
        /// <param name="originalPartner">Orignal Partner Name</param>
        /// <param name="partner">Partner name</param>
        /// <param name="language">Current locale</param>
        /// <param name="operationType">Operation type like add/update/handlepaymentchallenge</param>
        /// <param name="resourceType">Resource type like challengeDescription</param>
        /// <param name="paymentMethodType">Payment method type like credit_card.mc</param>
        /// <param name="sessionId">Paymetn sesion Id</param>
        /// <param name="rdsSessionId">Redirect service session id</param>
        /// <param name="redirectUrl">Redirect Url</param>
        /// <param name="descriptionType">PIDL description type</param>
        /// <param name="useTransactionServiceForPaymentAuth">Use payment auth</param>
        /// <param name="setting">Partner settings form PSS</param>
        public PIDLGeneratorContext(
            string country,
            string originalPartner,
            string partner,
            string language,
            string operationType,
            string resourceType,
            string paymentMethodType,
            string sessionId,
            string rdsSessionId,
            string redirectUrl,
            string descriptionType,
            bool useTransactionServiceForPaymentAuth,
            PaymentExperienceSetting setting = null)
        {
            this.Country = country;
            this.OriginalPartner = originalPartner;
            this.Partner = partner;
            this.Language = language;
            this.OperationType = operationType;
            this.ResourceType = resourceType;
            this.PaymentMethodType = paymentMethodType;
            this.RedirectUrl = redirectUrl;
            this.SessionId = sessionId;
            this.RedirectSessionId = rdsSessionId;
            this.DescriptionType = descriptionType;
            this.PartnerSetting = setting;
            this.UseTransactionServiceForPaymentAuth = useTransactionServiceForPaymentAuth;
        }

        /// <summary>
        /// Gets the value of the Country
        /// </summary>
        public string Country { get; private set; }

        /// <summary>
        /// Gets the value of the original partner name
        /// </summary>
        public string OriginalPartner { get; private set; }

        /// <summary>
        /// Gets the value of the Partner
        /// </summary>
        public string Partner { get; private set; }

        /// <summary>
        /// Gets the value of the ResourceType
        /// </summary>
        public string ResourceType { get; private set; }

        /// <summary>
        /// Gets the value of the PaymentMethodType
        /// </summary>
        public string PaymentMethodType { get; private set; }

        /// <summary>
        /// Gets the value of the OperationType
        /// </summary>
        public string OperationType { get; private set; }

        /// <summary>
        /// Gets the value of the Scenario
        /// </summary>
        public string Scenario { get; private set; }

        /// <summary>
        /// Gets the value of the Language
        /// </summary>
        public string Language { get; private set; }

        /// <summary>
        /// Gets the value of the ClassicProduct
        /// </summary>
        public string ClassicProduct { get; private set; }

        /// <summary>
        /// Gets the value of the SessionQueryUrl
        /// </summary>
        public string SessionQueryUrl { get; private set; }

        /// <summary>
        /// Gets the value of the RequestType
        /// </summary>
        public string RequestType { get; private set; }

        /// <summary>
        /// Gets the value of the BillableAccountId
        /// </summary>
        public string BillableAccountId { get; private set; }

        /// <summary>
        /// Gets the value of the EmailAddress
        /// </summary>
        public string EmailAddress { get; private set; }

        /// <summary>
        /// Gets the value of the PidlBaseUrl
        /// </summary>
        public string PidlBaseUrl { get; private set; }

        /// <summary>
        /// Gets the value of the RedirectUrl
        /// </summary>
        public string RedirectUrl { get; private set; }

        /// <summary>
        /// Gets the value of the SessionId
        /// </summary>
        public string SessionId { get; private set; }

        /// <summary>
        /// Gets the value of the RedirectSessionId
        /// </summary>
        public string RedirectSessionId { get; private set; }

        /// <summary>
        /// Gets the value of the DescriptionType
        /// </summary>
        public string DescriptionType { get; private set; }

        /// <summary>
        /// Gets the value of the AccountId
        /// </summary>
        public string AccountId { get; private set; }

        /// <summary>
        /// Gets the value of the ShortUrl
        /// </summary>
        public string ShortUrl { get; private set; }

        /// <summary>
        /// Gets or sets the event trace activity identifier, also known as correlation identifier, for tracing
        /// </summary>
        public EventTraceActivity TraceActivityId { get; set; }

        /// <summary>
        /// Gets a value indicating whether CompletePrerequisites is enabled
        /// </summary>
        public bool CompletePrerequisites { get; private set; }

        /// <summary>
        /// Gets a value indicating whether UseTransactionServiceForPaymentAuth is enabled
        /// </summary>
        public bool UseTransactionServiceForPaymentAuth { get; private set; }

        /// <summary>
        /// Gets the value of the PaymentMethods
        /// </summary>
        public PaymentInstrument PaymentInstrument { get; }

        /// <summary>
        /// Gets the value of the ExposedFlightFeatures
        /// </summary>
        public List<string> ExposedFlightFeatures { get; }

        /// <summary>
        /// Gets or sets - Get the feature provided by Partner Settings Service, Sets the feature provided in FeatureConfiguration for the partners not onboarded to Partner Settings Service
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set for partners not onboarded to partner setting service")]
        public PaymentExperienceSetting PartnerSetting { get; set; }
    }
}