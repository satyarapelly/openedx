// <copyright file="ComponentDescription.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using Microsoft.Commerce.Payments.PXService.Model.WalletService;
    using Microsoft.Commerce.Payments.PXService.Settings;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.Commerce.Payments.PXService.V7.Contexts;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using PaymentMethod = PimsModel.V4.PaymentMethod;
    using RestLink = Common.Web.RestLink;

    public abstract class ComponentDescription
    {
        private IList<PaymentMethod> paymentMethods = new List<PaymentMethod>();
        private IList<PimsModel.V4.PaymentInstrument> activePaymentInstruments = new List<PimsModel.V4.PaymentInstrument>();

        public static Dictionary<string, string> AddressEmissionFields
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { V7.Constants.PropertyDescriptionIds.AddressLine1, "({address_line1})" },
                    { V7.Constants.PropertyDescriptionIds.AddressLine2, "({address_line2})" },
                    { V7.Constants.PropertyDescriptionIds.AddressLine3, "({address_line3})" },
                    { V7.Constants.PropertyDescriptionIds.City, "({city})" },
                    { V7.Constants.PropertyDescriptionIds.Region, "({region})" },
                    { V7.Constants.PropertyDescriptionIds.PostalCode, "({postal_code})" },
                    { V7.Constants.PropertyDescriptionIds.Country, "({country})" }
                };
            }
        }

        public abstract string DescriptionType { get; }

        public string Country { get; set; }

        public string Currency { get; set; }

        public string Operation { get; set; }

        public string Language { get; set; }

        public string Family { get; set; }

        public string PaymentMethodType { get; set; }

        public string Partner { get; set; }

        public string Scenario { get; set; }

        public string PiId { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public IList<string> ExposedFlightFeatures { get; set; } = new List<string>();

        public PaymentExperienceSetting PSSSetting { get; set; }

        public PXServiceSettings PXSettings { get; set; }

        public string RequestId { get; set; }

        public CheckoutRequestClientActions CheckoutRequestClientActions { get; set; }

        public PaymentRequestClientActions PaymentRequestClientActions { get; set; }

        public EventTraceActivity TraceActivityId { get; set; }

        public HttpRequestMessage? Request { get; set; }

        public IList<PimsModel.V4.PaymentInstrument> ActivePaymentInstruments
        {
            get
            {
                return this.activePaymentInstruments;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public IList<PaymentMethod> PaymentMethods
        {
            get
            {
                return this.paymentMethods;
            }

            set
            {
                this.paymentMethods = value;
            }
        }

        public static bool ShouldEnableEmitEventOnPropertyUpdate(string id)
        {
            return id.Equals(V7.Constants.PropertyDescriptionIds.CartTax)
                || id.Equals(V7.Constants.PropertyDescriptionIds.CartSubtotal)
                || id.Equals(V7.Constants.PropertyDescriptionIds.CartTotal);
        }

        /// <summary>
        /// Creates a DisplayHintAction for partner action with PIDL payload.
        /// </summary>
        /// <param name="resourceType">The resource type for the context.</param>
        /// <returns>A configured DisplayHintAction for partner action with PIDL payload.</returns>
        public static DisplayHintAction CreatePartnerActionWithPidlPayload(string resourceType)
        {
            return new DisplayHintAction(DisplayHintActionType.partnerActionWithPidlPayload.ToString())
            {
                Context = new EventContext()
                {
                    ResourceType = resourceType,
                    Payload = AddressEmissionFields
                }
            };
        }

        public static void UpdateSubmitURL(PIDLResource pidl, string hintId, string method, string submitUrl)
        {
            var buttonproperty = pidl.GetDisplayHintById(hintId) as ButtonDisplayHint;
            UpdateSubmitURL(buttonproperty, method, submitUrl);
        }

        public static void UpdateSubmitURL(ButtonDisplayHint buttonproperty, string method, string submitUrl)
        {
            if (buttonproperty?.Action?.ActionType == DisplayHintActionType.submit.ToString())
            {
                RestLink context = new RestLink()
                {
                    Method = method,
                    Href = submitUrl
                };
                buttonproperty.Action.Context = context;
            }
        }

        public static void SetDefaultValues(List<PIDLResource> retVal, IDictionary<string, object> defaultValues, bool usePreExistingValue = true)
        {
            foreach (var pidl in retVal)
            {
                foreach (var dataDes in defaultValues)
                {
                    var taxProperty = pidl.DataDescription[dataDes.Key] as PropertyDescription;
                    if (taxProperty != null)
                    {
                        taxProperty.UsePreExistingValue = usePreExistingValue;
                        taxProperty.DefaultValue = dataDes.Value;
                    }
                }
            }
        }

        public static void ValidatePaymentMethods(IList<PaymentMethod> paymentMethods, string component, EventTraceActivity traceActivityId)
        {
            if (paymentMethods?.Count == 0)
            {
                throw TraceCore.TraceException(traceActivityId, new InvalidOperationException($"Payment methods not available, failed to get {component} component PIDL description"));
            }
        }

        public static void UpdatePayLabel(DataSource dataSource, string language = GlobalConstants.Defaults.Locale)
        {
            foreach (WalletConfig config in dataSource.Members)
            {
                foreach (PaymentInstrumentHandler handler in config.PaymentInstrumentHandlers)
                {
                    handler.PayLabel = PidlModelHelper.GetLocalizedString(V7.Constants.WalletConfigConstants.TaxIncludedPayLabel, language);
                }
            }
        }

        /// <summary>
        /// Get Data sourece - Wallet Config from provider service
        /// This method will be moved to quick payment description while merging with express checkout description
        /// </summary>
        /// <param name="filteredPMs">List of quick payments methods</param>
        /// <param name="exposedFlightFeatures">List of exposedFlightFeatures</param>
        /// <returns>Data source object</returns>
        public async Task<DataSource> GetWalletConfigDataSource(IList<PaymentMethod> filteredPMs, List<string> exposedFlightFeatures)
        {
            DataSource dataSource = null;

            // Get Apple/Google pay provider data
            ProviderDataResponse response = await this.PXSettings.WalletServiceAccessor.GetProviderData(this.TraceActivityId, exposedFlightFeatures);

            ClientInfo clientInfo = new ClientInfo { IsCrossOrigin = true };
            string client = JsonConvert.SerializeObject(clientInfo);

            var singleMarkets = await this.PXSettings.CatalogServiceAccessor.GetSingleMarkets(this.TraceActivityId);
           
            if (singleMarkets == null)
            {
                // FallBack to default markets if singleMarkets is null
                singleMarkets = new List<string>(PIDLResourceFactory.GetCopiedDictionaryFromDomainDictionaries("MarketsEUSMD").Keys);
            }

            if (singleMarkets != null && singleMarkets.Count > 0)
            {
                singleMarkets = singleMarkets.ConvertAll(market => market.ToUpper());
            }

            WalletConfig walletConfig = WalletsHandler.AdaptWalletResponseToPIDLConfig(response, client, this.Partner, this.Request, this.ExposedFlightFeatures?.ToList(), this.TraceActivityId, filteredPMs, this.Language, true, singleMarkets);

            dataSource = new DataSource
            {
                DataSourceConfig = new DataSourceConfig { UseLocalDataSource = true },
                Members = new List<object> { walletConfig }
            };

            if (dataSource == null)
            {
                throw TraceCore.TraceException(this.TraceActivityId, new InvalidOperationException($"Data source for quick payment should not be null"));
            }

            return dataSource;
        }

        public abstract Task<List<PIDLResource>> GetDescription();

        public virtual async Task LoadComponentDescription(
            string requestId,
            PXServiceSettings pxSettings,
            EventTraceActivity traceActivityId,
            PaymentExperienceSetting setting,
            List<string> exposedFlightFeatures,
            string operation = null,
            string partner = null,
            string family = null,
            string type = null,
            string scenario = null,
            string country = null,
            string language = null,
            string currency = null,
            HttpRequestMessage? request = null,
            string piid = null)
        {
            this.Operation = operation;
            this.Partner = partner;
            this.Scenario = scenario;

            this.Country = country;
            this.Language = language;
            this.Currency = currency;

            this.ExposedFlightFeatures = exposedFlightFeatures;
            this.PSSSetting = setting;
            this.PXSettings = pxSettings;
            this.RequestId = requestId;
            this.Family = family;
            this.PaymentMethodType = type;
            this.TraceActivityId = traceActivityId;
            this.Request = request;
            this.PiId = piid;

            if (this.UsePaymentRequestApiEnabled()
                && PidlFactory.V7.PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PidlFactory.V7.PartnerSettingsHelper.Features.PaymentClientHandlePaymentCollection, country, setting))
            {
                this.PaymentRequestClientActions = await this.PXSettings.PaymentOrchestratorServiceAccessor.GetClientActionForPaymentRequest(traceActivityId, requestId);
                if (this.PaymentRequestClientActions != null)
                {
                    this.Country = this.PaymentRequestClientActions.Country;
                    this.Language = this.PaymentRequestClientActions.Language;
                    this.Currency = this.PaymentRequestClientActions.Currency;

                    this.activePaymentInstruments = this.PaymentRequestClientActions?.PaymentMethodResults?.PaymentInstruments?.Where(pi => pi.Status == PaymentInstrumentStatus.Active).ToList();
                    this.paymentMethods = this.PaymentRequestClientActions?.PaymentMethodResults?.PaymentMethods;
                }
            }
            else if (requestId != null && RequestContext.GetRequestType(requestId) == V7.Constants.RequestContextType.Checkout)
            {
                {
                    this.CheckoutRequestClientActions = await this.PXSettings.PaymentOrchestratorServiceAccessor.GetClientAction(traceActivityId, requestId);

                    if (this.CheckoutRequestClientActions != null)
                    {
                        this.Country = this.CheckoutRequestClientActions.Country;
                        this.Language = this.CheckoutRequestClientActions.Language;
                        this.Currency = this.CheckoutRequestClientActions.Currency;

                        this.activePaymentInstruments = this.CheckoutRequestClientActions?.PaymentMethodResults?.PaymentInstruments?.Where(pi => pi.Status == PaymentInstrumentStatus.Active).ToList();
                        this.paymentMethods = this.CheckoutRequestClientActions?.PaymentMethodResults?.PaymentMethods;
                    }
                }
            }
        }

        // LoadComponentsData - This method is used to load the component data for the given parameters.
        public void LoadComponentsData(
            string requestId,
            PXServiceSettings pxSettings,
            EventTraceActivity traceActivityId,
            PaymentExperienceSetting setting,
            List<string> exposedFlightFeatures,
            string operation = null,
            string partner = null,
            string family = null,
            string type = null,
            string scenario = null,
            string country = null,
            string language = null,
            string currency = null,
            HttpRequestMessage? request = null,
            CheckoutRequestClientActions? checkoutRequestClientActions = null,
            PaymentRequestClientActions? paymentRequestClientActions = null)
        {
            this.Operation = operation;
            this.Partner = partner;
            this.Scenario = scenario;

            this.Country = country;
            this.Language = language;
            this.Currency = currency;

            this.ExposedFlightFeatures = exposedFlightFeatures;
            this.PSSSetting = setting;
            this.PXSettings = pxSettings;
            this.RequestId = requestId;
            this.Family = family;
            this.PaymentMethodType = type;
            this.TraceActivityId = traceActivityId;
            this.Request = request;

            if (this.UsePaymentRequestApiEnabled())
            {
                this.PaymentRequestClientActions = paymentRequestClientActions;
                this.Country = paymentRequestClientActions.Country;
                this.Language = paymentRequestClientActions.Language;
                this.Currency = paymentRequestClientActions.Currency;

                this.activePaymentInstruments = paymentRequestClientActions?.PaymentMethodResults?.PaymentInstruments?.Where(pi => pi.Status == PaymentInstrumentStatus.Active).ToList();
                this.paymentMethods = paymentRequestClientActions?.PaymentMethodResults?.PaymentMethods;
            }
            else if (requestId != null && RequestContext.GetRequestType(requestId) == V7.Constants.RequestContextType.Checkout)
            {
                this.CheckoutRequestClientActions = checkoutRequestClientActions;
                if (this.CheckoutRequestClientActions != null)
                {
                    this.Country = this.CheckoutRequestClientActions.Country;
                    this.Language = this.CheckoutRequestClientActions.Language;
                    this.Currency = this.CheckoutRequestClientActions.Currency;

                    this.activePaymentInstruments = this.CheckoutRequestClientActions?.PaymentMethodResults?.PaymentInstruments?.Where(pi => pi.Status == PaymentInstrumentStatus.Active).ToList();
                    this.paymentMethods = this.CheckoutRequestClientActions?.PaymentMethodResults?.PaymentMethods;
                }
            }
            else if (requestId != null && RequestContext.GetRequestType(requestId) == V7.Constants.RequestContextType.Payment)
            {
                this.Country = paymentRequestClientActions?.Country;
                this.Language = paymentRequestClientActions?.Language;
                this.PaymentRequestClientActions = paymentRequestClientActions;
            }
        }

        public bool ShouldComputeTax()
        {
            return this.ExposedFlightFeatures != null && this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableUsePOCapabilities, StringComparer.OrdinalIgnoreCase)
                && (this.UsePaymentRequestApiEnabled() ? (this.PaymentRequestClientActions?.Capabilities?.ComputeTax).GetValueOrDefault(false) : (this.CheckoutRequestClientActions?.Capabilities?.ComputeTax).GetValueOrDefault(false));
        }

        protected void UpdateExpressCheckoutButtonSourceUrl(string displayHintId, PIDLResource pidl)
        {
            var expressCheckoutButton = pidl.GetDisplayHintById(displayHintId) as ExpressCheckoutButtonDisplayHint;

            if (this.ExposedFlightFeatures?.Contains(PXCommon.Flighting.Features.PXUseInlineExpressCheckoutHtml) ?? false)
            {
                if (displayHintId.Equals(V7.Constants.DisplayHintIds.GooglepayExpressCheckoutFrame))
                {
                    expressCheckoutButton.SourceUrl = expressCheckoutButton.SourceUrl.Replace("googlepay.html", "inline/googlepay.html");
                }
                else if (displayHintId.Equals(V7.Constants.DisplayHintIds.ApplepayExpressCheckoutFrame))
                {
                    expressCheckoutButton.SourceUrl = expressCheckoutButton.SourceUrl.Replace("applepay.html", "inline/applepay.html");
                }
            }

            if (this.ExposedFlightFeatures?.Contains(PXCommon.Flighting.Features.PXExpressCheckoutUseIntStaticResources) ?? false)
            {
                if (expressCheckoutButton.SourceUrl.Contains(V7.Constants.EnvironmentEndpoint.PROD))
                {
                    expressCheckoutButton.SourceUrl = expressCheckoutButton.SourceUrl.Replace(V7.Constants.EnvironmentEndpoint.PROD, V7.Constants.EnvironmentEndpoint.INT);
                }
            }
            else if (this.ExposedFlightFeatures?.Contains(PXCommon.Flighting.Features.PXExpressCheckoutUseProdStaticResources) ?? false)
            {
                if (expressCheckoutButton.SourceUrl.Contains(V7.Constants.EnvironmentEndpoint.INT))
                {
                    expressCheckoutButton.SourceUrl = expressCheckoutButton.SourceUrl.Replace(V7.Constants.EnvironmentEndpoint.INT, V7.Constants.EnvironmentEndpoint.PROD);
                }
            }
        }

        /// <summary>
        /// Checks if the UsePaymentRequestApi flight feature is enabled.
        /// </summary>
        /// <returns>True if the UsePaymentRequestApi feature is enabled, false otherwise.</returns>
        protected bool UsePaymentRequestApiEnabled()
        {
            return this.ExposedFlightFeatures?.Contains(Flighting.Features.UsePaymentRequestApi, StringComparer.OrdinalIgnoreCase) ?? false;
        }
    }
}