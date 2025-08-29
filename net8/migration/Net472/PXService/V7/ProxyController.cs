// <copyright file="ProxyController.cs" company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using global::Azure.Core;
    using Common.Helper;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.Settings;
    using Microsoft.Commerce.Payments.PXService.V7.Contexts;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Microsoft.Commerce.Payments.PXService.V7.PXChallengeManagement;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;
    using PaymentChallenge;
    using Address = Accessors.LegacyCommerceService.DataModel.Address;
    using ClientAction = PXCommon.ClientAction;
    using ClientActionType = PXCommon.ClientActionType;
    using PayerAuth = PXService.Model.PayerAuthService;
    using PaymentInstrument = PimsModel.V4.PaymentInstrument;
    using PaymentMethod = PimsModel.V4.PaymentMethod;

    public class ProxyController : ApiController
    {
        private Dictionary<string, object> clientContext;

        private PaymentSessionsHandler psd2PaymentSessionHandler;

        private PaymentSessionsHandlerV2 psd2PaymentSessionHandlerV2;

        private SecondScreenSessionHandler secondScreenSessionHandler;

        private PXChallengeManagementHandler pXChallengeManagementHandler;

        private List<string> exposedFlightFeatures;

        private PartnerSettings partnerSettings;

        protected PXServiceSettings Settings
        {
            get
            {
                return this.Configuration.Properties[WebApiConfig.PXSettingsType] as PXServiceSettings;
            }
        }

        // TODO Bug 1682833:[PX AP] Separate userContext from clientContext
        protected Dictionary<string, object> ClientContext
        {
            get
            {
                if (this.clientContext == null)
                {
                    this.InitializeClientContext();
                }

                return this.clientContext;
            }
        }

        protected string PidlBaseUrl
        {
            get
            {
                if (this.ExposedFlightFeatures != null && this.exposedFlightFeatures.Contains(Flighting.Features.PXUsePifdBaseUrlInsteadOfForwardedHostHeader, StringComparer.OrdinalIgnoreCase))
                {
                    return this.Settings.PifdBaseUrl;
                }
                else
                {
                    return this.Request.Headers.Contains(GlobalConstants.HeaderValues.ForwardedHostHeader)
                    ? string.Format("https://{0}/V6.0", this.Request.Headers.GetValues(GlobalConstants.HeaderValues.ForwardedHostHeader).FirstOrDefault())
                    : this.Settings.PifdBaseUrl;
                }
            }
        }

        protected string OfferId
        {
            get
            {
                string offerId = null;
                IEnumerable<string> offerIds = null;
                if (this.Request.Headers.TryGetValues(GlobalConstants.HeaderValues.OfferId, out offerIds))
                {
                    string value = offerIds.FirstOrDefault();
                    if (value != null)
                    {
                        offerId = ProxyController.Base64Decode(value);
                    }
                }

                return offerId;
            }
        }

        protected Version PidlSdkVersion
        {
            get
            {
                IEnumerable<string> pidlsdkVersions;
                if (this.Request.Headers.TryGetValues(GlobalConstants.HeaderValues.PidlSdkVersion, out pidlsdkVersions))
                {
                    string value = pidlsdkVersions.FirstOrDefault();
                    if (value != null)
                    {
                        string[] pidlsdkVersionDetails = value.Split('_');
                        if (pidlsdkVersionDetails.Length > 0)
                        {
                            Version pidlsdkVersion;
                            if (Version.TryParse(pidlsdkVersionDetails[0], out pidlsdkVersion))
                            {
                                return pidlsdkVersion;
                            }
                        }
                    }
                }

                return null;
            }
        }

        protected PaymentSessionsHandler PaymentSessionsHandler
        {
            get
            {
                if (this.psd2PaymentSessionHandler == null)
                {
                    this.psd2PaymentSessionHandler = new PaymentSessionsHandler(
                    this.Settings.PayerAuthServiceAccessor,
                    this.Settings.PIMSAccessor,
                    this.Settings.SessionServiceAccessor,
                    this.Settings.AccountServiceAccessor,
                    this.Settings.PurchaseServiceAccessor,
                    this.Settings.TransactionServiceAccessor,
                    this.Settings.TransactionDataServiceAccessor,
                    this.Settings.PifdBaseUrl);
                }

                return this.psd2PaymentSessionHandler;
            }
        }

        protected PaymentSessionsHandlerV2 PaymentSessionsHandlerV2
        {
            get
            {
                if (this.psd2PaymentSessionHandlerV2 == null)
                {
                    this.psd2PaymentSessionHandlerV2 = new PaymentSessionsHandlerV2(
                    this.Settings.PayerAuthServiceAccessor,
                    this.Settings.PIMSAccessor,
                    this.Settings.SessionServiceAccessor,
                    this.Settings.AccountServiceAccessor,
                    this.Settings.PurchaseServiceAccessor,
                    this.Settings.TransactionServiceAccessor,
                    this.Settings.TransactionDataServiceAccessor,
                    this.Settings.PifdBaseUrl);
                }

                return this.psd2PaymentSessionHandlerV2;
            }
        }

        protected SecondScreenSessionHandler SecondScreenSessionHandler
        {
            get
            {
                if (this.secondScreenSessionHandler == null)
                {
                    this.secondScreenSessionHandler = new SecondScreenSessionHandler(
                    this.Settings.SessionServiceAccessor);
                }

                return this.secondScreenSessionHandler;
            }
        }

        protected PXChallengeManagementHandler PXChallengeManagementHandler
        {
            get
            {
                if (this.pXChallengeManagementHandler == null)
                {
                    this.pXChallengeManagementHandler = new PXChallengeManagementHandler(
                    this.Settings.ChallengeManagementServiceAccessor);
                }

                return this.pXChallengeManagementHandler;
            }
        }

        protected List<string> ExposedFlightFeatures
        {
            get
            {
                if (this.exposedFlightFeatures == null)
                {
                    object exposableFeaturesObject = null;
                    Request.Properties.TryGetValue(GlobalConstants.RequestPropertyKeys.ExposedFlightFeatures, out exposableFeaturesObject);
                    this.exposedFlightFeatures = exposableFeaturesObject as List<string> ?? new List<string>();
                }

                return this.exposedFlightFeatures;
            }
        }

        protected PartnerSettings PartnerSettings
        {
            get
            {
                if (this.partnerSettings == null)
                {
                    object partnerSettingsObject = null;
                    Request.Properties.TryGetValue(GlobalConstants.RequestPropertyKeys.PartnerSettings, out partnerSettingsObject);
                    this.partnerSettings = partnerSettingsObject as PartnerSettings;
                }
                
                return this.partnerSettings;
            }

            set
            {
                this.partnerSettings = value;
            }
        }

        public static void MapCreditCardCommonError(ref ServiceErrorResponseException ex, string language)
        {
            if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidCvv, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidCvv, language),
                    Target = Constants.CreditCardErrorTargets.Cvv,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidAccountHolder, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidAccountHolder, language),
                    Target = Constants.CreditCardErrorTargets.AccountHolderName,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.ExpiredCard, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.ExpiredCard, language),
                    Target = string.Format("{0},{1}", Constants.CreditCardErrorTargets.ExpiryMonth, Constants.CreditCardErrorTargets.ExpiryYear),
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidExpiryDate, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidExpiryDate, language),
                    Target = string.Format("{0},{1}", Constants.CreditCardErrorTargets.ExpiryMonth, Constants.CreditCardErrorTargets.ExpiryYear),
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidCity, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidCity, language),
                    Target = Constants.CreditCardErrorTargets.City,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidState, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidState, language),
                    Target = Constants.CreditCardErrorTargets.State,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidZipCode, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidZipCode, language),
                    Target = Constants.CreditCardErrorTargets.PostalCode,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidCountryCode, StringComparison.OrdinalIgnoreCase)
                || string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidCountry, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidCountry, language),
                    Target = Constants.CreditCardErrorTargets.Country,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidAddress, StringComparison.OrdinalIgnoreCase))
            {
                // For InvalidAddress, PxService does not know the exact payload of address group.
                // Set full-set of address info as target, PIDL SDK will highlight only existed fields.
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidAddress, language),
                    Target = string.Format(
                    "{0},{1},{2},{3},{4},{5},{6}",
                    Constants.CreditCardErrorTargets.AddressLine1,
                    Constants.CreditCardErrorTargets.AddressLine2,
                    Constants.CreditCardErrorTargets.AddressLine3,
                    Constants.CreditCardErrorTargets.City,
                    Constants.CreditCardErrorTargets.State,
                    Constants.CreditCardErrorTargets.Country,
                    Constants.CreditCardErrorTargets.PostalCode),
                });
            }
            else
            {
                // Catch all for any other error scenario
                ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.Generic, language);
            }
        }

        protected static void SetDetailsData(PIDLData pi, Dictionary<string, object> data, EventTraceActivity traceActivityId)
        {
            string paymentInstrumentDetailsPropertyName = "details";

            JsonSerializerSettings serializationSettings = new JsonSerializerSettings();
            serializationSettings.NullValueHandling = NullValueHandling.Ignore;
            serializationSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            if (pi.ContainsKey(paymentInstrumentDetailsPropertyName) && pi[paymentInstrumentDetailsPropertyName] != null)
            {
                try
                {
                    var paymentInstrumentDetailsDataPayload = JsonConvert.SerializeObject(pi[paymentInstrumentDetailsPropertyName], serializationSettings);
                    var paymentInstrumentDetails = JsonConvert.DeserializeObject<PaymentInstrumentDetails>(paymentInstrumentDetailsDataPayload, serializationSettings);

                    foreach (var item in data)
                    {
                        paymentInstrumentDetails.GetType().GetProperty(item.Key).SetValue(paymentInstrumentDetails, item.Value);
                    }

                    paymentInstrumentDetailsDataPayload = JsonConvert.SerializeObject(paymentInstrumentDetails, serializationSettings);
                    pi[paymentInstrumentDetailsPropertyName] = JsonConvert.DeserializeObject<PIDLData>(paymentInstrumentDetailsDataPayload, serializationSettings);
                }
                catch (Exception ex)
                {
                    SllWebLogger.TracePXServiceException("PaymentInstrumentsExController.SetDetailsData: " + ex.ToString(), traceActivityId);
                }
            }
        }

        protected static void SetPiData(PIDLData pi, Dictionary<string, object> data)
        {
            foreach (var item in data)
            {
                if (pi.ContainsKey(item.Key))
                {
                    pi[item.Key] = item.Value;
                }
                else
                {
                    pi.Add(item.Key, item.Value);
                }
            }
        }

        protected static Dictionary<string, string> GetMetaData(RequestContext requestContext)
        {
            var metaData = new Dictionary<string, string>();
            if (requestContext == null)
            {
                return metaData;
            }

            if (requestContext.TenantId != null)
            {
                metaData.Add("TenantId", requestContext.TenantId);
            }

            if (requestContext.TenantCustomerId != null)
            {
                metaData.Add("TenantCustomerId", requestContext.TenantCustomerId);
            }

            if (requestContext.PaymentAccountId != null)
            {
                metaData.Add("PaymentAccountId", requestContext.PaymentAccountId);
            }

            if (requestContext.RequestId != null)
            {
                metaData.Add("RequestId", requestContext.RequestId);
            }

            return metaData;
        }

        protected static void RemoveDefaultValueIfExpression(List<PIDLResource> inputPidl)
        {
            // Remove expression in default_value and set it to null.
            if (inputPidl == null || inputPidl.Count == 0)
            {
                return;
            }

            foreach (PIDLResource pidl in inputPidl)
            {
                if (pidl.DataSources != null)
                {
                    pidl.RemoveDataSource();
                }

                if (pidl.DataDescription != null)
                {
                    foreach (string propertyName in pidl.DataDescription.Keys)
                    {
                        PropertyDescription propertyDescription = pidl.DataDescription[propertyName] as PropertyDescription;
                        if (propertyDescription == null)
                        {
                            // This must be a sub-pidl (e.g Address witin a CC pidl)
                            List<PIDLResource> subPidl = pidl.DataDescription[propertyName] as List<PIDLResource>;
                            if (subPidl != null)
                            {
                                ProxyController.RemoveDefaultValueIfExpression(subPidl);
                            }
                        }
                        else if (propertyDescription.DefaultValue != null && IsDefaultValueExpression(propertyDescription.DefaultValue.ToString()))
                        {
                            // Only remove default value if it contains an expression
                            // For other hardcoded default value, keep it as it is. Ex, type of profile, culture.
                            propertyDescription.DefaultValue = null;
                        }
                    }
                }
            }
        }

        protected static void UpdateIsOptionalProperty(List<PIDLResource> retVal, string[] propertyNames, bool isOptional)
        {
            foreach (PIDLResource resource in retVal)
            {
                foreach (string propertyName in propertyNames)
                {
                    PropertyDescription propertyDescription = resource.GetPropertyDescriptionByPropertyName(propertyName);
                    if (propertyDescription != null)
                    {
                        propertyDescription.IsOptional = isOptional;
                    }
                }
            }
        }

        protected static void AddHiddenCheckBoxElement(
            PIDLResource pidl,
            string propertyName,
            string path,
            bool inlinkedPidl)
        {
            if (inlinkedPidl)
            {
                foreach (var linkedPidl in pidl?.LinkedPidls)
                {
                    // only add hidden checkbox if it doesn't exist
                    if (linkedPidl.GetDisplayHintById(BuildHiddenCheckboxHintId(propertyName)) == null)
                    {
                        AddHiddenCheckBoxElement(linkedPidl, propertyName, path);
                    }
                }
            }
            else
            {
                AddHiddenCheckBoxElement(pidl, propertyName, path);
            }
        }

        protected static void AddHiddenCheckBoxElement(PIDLResource pidl, string propertyName, string path)
        {
            Dictionary<string, object> targetDataDescription = pidl?.GetTargetDataDescription(path);
            if (targetDataDescription != null && !targetDataDescription.ContainsKey(propertyName))
            {
                targetDataDescription[propertyName] = new PropertyDescription()
                {
                    PropertyType = "userData",
                    DataType = "bool",
                    PropertyDescriptionType = "bool",
                    IsUpdatable = true,
                    IsOptional = true,
                    IsKey = false,
                };
            }

            var hiddenIsCustomerConsentedPropertyDisplayHint = new PropertyDisplayHint() { PropertyName = propertyName, IsHidden = true, HintId = "HiddenCheckbox_" + propertyName };
            pidl?.DisplayPages[0]?.Members?.Insert(0, hiddenIsCustomerConsentedPropertyDisplayHint);
        }

        protected static void RemoveDisplayDescription(List<PIDLResource> retVal, string[] propertyNames)
        {
            foreach (PIDLResource pidl in retVal)
            {
                foreach (PageDisplayHint displayPage in pidl.DisplayPages)
                {
                    foreach (string propertyName in propertyNames)
                    {
                        int index = displayPage.Members.FindIndex(hint => hint.HintId == propertyName);
                        if (index != -1)
                        {
                            displayPage.Members.RemoveAt(index);
                        }
                    }
                }
            }
        }

        protected static void UpdateIsOptionalPropertyWithFullPath(PIDLResource pidlResource, string[] fullPaths, bool isOptional)
        {
            foreach (string propertyName in fullPaths)
            {
                PropertyDescription propertyDescription = pidlResource.GetPropertyDescriptionByPropertyNameWithFullPath(propertyName);
                if (propertyDescription != null)
                {
                    propertyDescription.IsOptional = isOptional;
                }
            }
        }

        protected static void HideDisplayDescriptionById(List<PIDLResource> pidlResource, string displayDescriptionId)
        {
            foreach (PIDLResource profilePidl in pidlResource)
            {
                DisplayHint targetDisplayDescription = profilePidl.GetDisplayHintById(displayDescriptionId);
                if (targetDisplayDescription != null)
                {
                    targetDisplayDescription.IsHidden = true;
                }
            }
        }

        protected static void HideDisplayDescriptionById(PIDLResource pidlResource, List<string> displayDescriptionIds)
        {
            foreach (string displayDescriptionId in displayDescriptionIds)
            {
                DisplayHint targetDisplayDescription = pidlResource.GetDisplayHintById(displayDescriptionId);
                if (targetDisplayDescription != null)
                {
                    targetDisplayDescription.IsHidden = true;
                }
            }
        }

        protected static string GetSettingTemplate(string partner, PaymentExperienceSetting setting, string descriptionType = null, string type = null, string family = null)
        {
            string resourceId = type;

            if (!string.IsNullOrEmpty(family))
            {
                if (string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    resourceId = family;
                }
                else
                {
                    resourceId = $"{family}.{type}";
                }
            }
            
            return TemplateHelper.GetSettingTemplate(partner, setting, descriptionType, resourceId);
        }

        protected static PaymentInstrument CreateNewPaymentMethodOption(string accountId)
        {
            return new PaymentInstrument()
            {
                PaymentMethod = new PaymentMethod()
                {
                    PaymentMethodFamily = Constants.PaymentMethodFamily.add_new_payment_method.ToString(),
                    PaymentMethodType = Constants.PaymentMethodType.AddNewPM
                },
                Status = PaymentInstrumentStatus.Active,
                PaymentInstrumentId = Constants.PaymentMethodType.AddNewPM,
                PaymentInstrumentAccountId = accountId,
                CreationTime = DateTime.UtcNow
            };
        }

        protected static PIDLData CreatePiPayload(string country, string type)
        {
            var details = new Dictionary<string, object>
            {
                { V7.Constants.PropertyDescriptionIds.Address, new Dictionary<string, object> { { V7.Constants.PropertyDescriptionIds.Country, country } } }
            };

            PIDLData pi = new PIDLData
            {
                [V7.Constants.PropertyDescriptionIds.PaymentMethodFamily] = V7.Constants.PaymentMethodFamily.ewallet.ToString(),
                [V7.Constants.PropertyDescriptionIds.PaymentMethodType] = type,
                [V7.Constants.PropertyDescriptionIds.Details] = details,
                [V7.Constants.PropertyDescriptionIds.AttachmentType] = AttachmentType.Wallet
            };

            return pi;
        }

        protected List<string> GetBillingAccountContext()
        {
            // If the partner send x-ms-billing-account-id header, then we should not skip complete prerequisites
            string flightValueString = Request.GetRequestHeader(GlobalConstants.HeaderValues.XMsBillingAccountId);
            if (flightValueString != null)
            {
                return flightValueString.Split(':').ToList();
            }

            return null;
        }

        // PaymentSessionsHandlerV2 is refactored from PaymentSessionsHandler.  We want to use V2
        // if either the flight is enabled for user, or if a session has HandlerVersion of "V2".
        protected async Task<PaymentSessionsHandler> GetVersionBasedPaymentSessionsHandler(EventTraceActivity traceActivityId, string sessionId = null)
        {
            Model.PXInternal.PaymentSession session = null;
            if (sessionId != null)
            {
                session = await this.PaymentSessionsHandler.TryGetStoredSession(sessionId, traceActivityId);
            }

            if (session?.HandlerVersion == PaymentSessionsHandlerV2.HandlerVersion || this.ExposedFlightFeatures.Contains(Flighting.Features.PXUsePaymentSessionsHandlerV2))
            {
                return this.PaymentSessionsHandlerV2;
            }

            return this.PaymentSessionsHandler;
        }

        protected PaymentExperienceSetting GetPaymentExperienceSetting(string operation)
        {
            PaymentExperienceSetting setting = null;

            if (this.PartnerSettings != null && this.PartnerSettings.PaymentExperienceSettings != null)
            {
                if (operation != null)
                {
                    var paymentExperienceSetting = this.PartnerSettings.PaymentExperienceSettings.FirstOrDefault(paymentExperienceSettings => paymentExperienceSettings.Key.Equals(operation, StringComparison.OrdinalIgnoreCase));
                    setting = !paymentExperienceSetting.Equals(default(KeyValuePair<string, PaymentExperienceSetting>)) ? paymentExperienceSetting.Value : null;
                }

                // If the setting for the operation is not found, use the default operation for setting
                if (setting == null)
                {
                    this.PartnerSettings.PaymentExperienceSettings.TryGetValue(Constants.Operations.Default, out setting);
                }
            }

            return setting;
        }

        protected IList<KeyValuePair<string, string>> GetAdditionalHeadersFromRequest(bool enablePIMSAccessorFlight = false)
        {
            List<KeyValuePair<string, string>> additionalHeaders = new List<KeyValuePair<string, string>>();

            // flight
            if (this.Settings.PIMSAccessorFlightEnabled || enablePIMSAccessorFlight)
            {
                string flightKey = GlobalConstants.HeaderValues.ExtendedFlightName;
                string flightValue = this.Request.GetRequestHeader(flightKey);
                if (flightValue != null)
                {
                    additionalHeaders.Add(new KeyValuePair<string, string>(flightKey, flightValue));
                }
            }

            return additionalHeaders;
        }

        protected async Task<PIDLResource> Generate3DS2ChallengePIDLResource(
            PaymentSessionData paymentSessionData,
            RequestContext requestContext,
            EventTraceActivity traceActivityId,
            PaymentExperienceSetting setting)
        {
            // Step 1: create payment session
            PaymentSession paymentSession = await PaymentSessionsHandlerV2.CreatePaymentSession(
                accountId: requestContext.PaymentAccountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: PXService.Model.ThreeDSExternalService.DeviceChannel.Browser,
                exposedFlightFeatures: this.ExposedFlightFeatures,
                emailAddress: await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress),
                traceActivityId: traceActivityId,
                isMotoAuthorized: null,
                tid: null,
                testContext: HttpRequestHelper.GetTestHeader(this.Request),
                setting: setting,
                userId: null,
                isGuestUser: false,
                requestContext: requestContext);

            if (!paymentSession.IsChallengeRequired)
            {
                return null;
            }

            // Step 2 Get ThreeDSMethodURL
            PayerAuth.BrowserInfo browserInfo = await this.CollectBrowserInfo();
            browserInfo.BrowserTZ = "0";
            browserInfo.BrowserLanguage = paymentSession.Language ?? GlobalConstants.Defaults.Locale;
            browserInfo.ChallengeWindowSize = paymentSession.ChallengeWindowSize;

            BrowserFlowContext result;

            result = await PaymentSessionsHandlerV2.GetThreeDSMethodURL(
                accountId: requestContext.PaymentAccountId,
                browserInfo: browserInfo,
                paymentSession: paymentSession,
                traceActivityId: traceActivityId,
                exposedFlightFeatures: this.ExposedFlightFeatures);

            // Step 3 generate challenge PIDL
            ClientAction clientAction = await this.GenerateChallengeClientAction(
                result,
                paymentSession,
                requestContext,
                paymentSessionData.PaymentInstrumentId,
                traceActivityId);

            if (clientAction != null)
            {
                PIDLResource clientActionPidl = new PIDLResource();
                clientActionPidl.ClientAction = clientAction;
                return clientActionPidl;
            }
            else
            {
                return null;
            }
        }

        protected void EnableFlightingsInPartnerSetting(PaymentExperienceSetting setting, string country)
        {
            if (setting == null || setting.Features == null)
            {
                return;
            }

            foreach (KeyValuePair<string, FeatureConfig> feature in setting?.Features)
            {
                if (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(feature.Key, country, setting, PXCommon.Constants.DisplayCustomizationDetail.UsePSSForPXFeatureFlighting))
                {
                    this.ExposedFlightFeatures.Add(feature.Key);
                }
            }
        }

        protected void EnableAVS(List<PIDLResource> pidls, bool showAVS, bool enableAVSAddtionalItems, string avsAttachButtonId, string type, string partner, string language, string country)
        {
            if (showAVS)
            {
                foreach (PIDLResource pidl in pidls)
                {
                    DisplayHint validateButtonDisplayHint = pidl.GetDisplayHintById(avsAttachButtonId);
                    if (validateButtonDisplayHint != null)
                    {
                        this.AddModernValidationAction(validateButtonDisplayHint, string.Empty, type, partner, language, country);
                    }

                    if (enableAVSAddtionalItems)
                    {
                        // For azure billing address form, include "is_customer_consented" and "is_avs_full_validation_succeeded" in payload when posting to Jarvis
                        ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsUserConsented, null);
                        ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, null);
                    }
                }
            }
        }

        protected void AddModernValidationActionForPaymentMethodDescription(PIDLResource paymentMethodPidl, string propertyName, string addressType, string partner, string language, string country)
        {
            DisplayHint modernValidateButtonDisplayHint; 

            // In XboxNative flows, the address validation check should come after the 2nd of 3 pages.
            // NextModernValidateButton won't exist for flows without the summary page
            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) && paymentMethodPidl.GetDisplayHintById(Constants.DisplayHintIds.NextModernValidateButton) != null)
            {
                modernValidateButtonDisplayHint = paymentMethodPidl.GetDisplayHintById(Constants.DisplayHintIds.NextModernValidateButton);
            }
            else
            {
                modernValidateButtonDisplayHint = paymentMethodPidl.GetDisplayHintById(Constants.DisplayHintIds.SaveButton);
                if (modernValidateButtonDisplayHint == null)
                {
                    modernValidateButtonDisplayHint = paymentMethodPidl.GetDisplayHintById(Constants.DisplayHintIds.SaveNextButton);
                }
            }

            if (modernValidateButtonDisplayHint != null)
            {
                this.AddModernValidationAction(modernValidateButtonDisplayHint, propertyName, addressType, partner, language, country);
            }
        }

        protected void AddModernValidationAction(DisplayHint displayHintToAddModernValidationAction, string propertyName, string type, string partner, string language, string country)
        {
            DisplayHintAction originalPidlAction = displayHintToAddModernValidationAction.Action;
            var validateLink = new PXCommon.RestLink();
            validateLink.Method = "POST";
            validateLink.SetErrorCodeExpressions(new[] { "({contextData.innererror.code})", "({contextData.code})" });
            if (!string.IsNullOrEmpty(propertyName))
            {
                validateLink.PropertyName = propertyName;
            }

            string modernAVSForTradeScenario = Constants.ScenarioNames.SuggestAddressesTradeAVS;
            if (this.ExposedFlightFeatures != null && this.exposedFlightFeatures.Contains(Flighting.Features.TradeAVSUsePidlModalInsteadofPidlPage, StringComparer.OrdinalIgnoreCase))
            {
                modernAVSForTradeScenario = Constants.ScenarioNames.SuggestAddressesTradeAVSUsePidlModal;
            }
            
            if (this.ExposedFlightFeatures != null && this.exposedFlightFeatures.Contains(Flighting.Features.TradeAVSUsePidlPageV2, StringComparer.OrdinalIgnoreCase))
            {
                modernAVSForTradeScenario = Constants.ScenarioNames.SuggestAddressesTradeAVSUsePidlPageV2;
            }

            validateLink.Href = string.Format(Constants.UriTemplate.PifdAnonymousModernAVSForTrade, type, partner, language, modernAVSForTradeScenario, country);
            DisplayHintAction modernValidateAction = new DisplayHintAction("validate", true, validateLink, null);
            modernValidateAction.NextAction = originalPidlAction;
            displayHintToAddModernValidationAction.Action = modernValidateAction;
        }

        /// <summary>
        /// Returns the first context value found for a given array of context paths
        /// </summary>
        /// <param name="contextPaths">An array of context paths to try and get the value for</param>
        /// <param name="accountId">Commerce account id of the user making this request</param>
        /// <param name="country">Country of the current user request</param>
        /// <param name="traceActivityId">Trace activity id to log to</param>
        /// <param name="partner">Partner of the current user request</param>
        /// <param name="pidlType">Type of the prefill pidl</param>
        /// <param name="completePrerequisites">When prefilling the properties of a pidl's data description, we must check whether that pidl has completePrerequisites to determine when to override V2 to V3 profiles</param>
        /// <param name="billableAccountId">Legacy Billable account id of the user making this request</param>
        /// <returns>Context value of the first context path for which a value exists.  If none are found, null is returned.</returns>
        protected async Task<string> TryGetClientContext(string[] contextPaths, string accountId, string country, EventTraceActivity traceActivityId, string partner = Constants.ServiceDefaults.DefaultPartnerName, string pidlType = null, bool completePrerequisites = false, string billableAccountId = null)
        {
            string retVal = null;
            foreach (string contextPath in contextPaths)
            {
                retVal = await this.TryGetClientContext(contextPath, accountId, country, traceActivityId, partner, pidlType, completePrerequisites, billableAccountId);
                if (retVal != null)
                {
                    break;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Returns the context value of a specified contextKey
        /// </summary>
        /// <param name="contextPath">Context path is a dot-separate path to the value to be retreived. 
        /// Example: "MsaProfile.emailAddress"
        /// </param>
        /// <param name="accountId">Commerce account id of the user making this request</param>
        /// <param name="country">Country of the current user request</param>
        /// <param name="traceActivityId">Trace activity id to log to</param>
        /// <param name="partner">Partner of the current user request</param>
        /// <param name="pidlType">Type of the prefill pidl</param>
        /// <param name="completePrerequisites">When prefilling the properties of a pidl's data description, we must check whether that pidl has completePrerequisites to determine when to override V2 to V3 profiles</param>
        /// <param name="billableAccountId">Legacy Billable account id of the user making this request</param>
        /// <param name="setting">Payment experience setting version</param>
        /// <returns>Returns the context value or null if not found.</returns>
        protected async Task<string> TryGetClientContext(string contextPath, string accountId = null, string country = null, EventTraceActivity traceActivityId = null, string partner = Constants.ServiceDefaults.DefaultPartnerName, string pidlType = null, bool completePrerequisites = false, string billableAccountId = null, PaymentExperienceSetting setting = null)
        {
            string retVal = null;
            string[] contextKeys = contextPath.Split(new char[] { '.' });
            if (contextKeys.Length != 2)
            {
                throw new ArgumentException("Currently, only 2 levels are supported in client context");
            }

            bool isJarvisV3 = PIDLResourceFactory.IsJarvisProfileV3Partner(partner, country, setting) || string.Equals(pidlType, Constants.ScenarioNames.Standalone, StringComparison.OrdinalIgnoreCase);

            if (GuestAccountHelper.IsGuestAccount(this.Request) || (this.ExposedFlightFeatures.Contains(Flighting.Features.PXUseJarvisV3ForCompletePrerequisites, StringComparer.OrdinalIgnoreCase) && completePrerequisites))
            {
                isJarvisV3 = true;
            }

            // Azure users do not have (employee) profiles. Hence, do not look-up for profile for Azure partner.
            // Look-up profile in the case where billableAccountId is not passed by Azure partner.
            bool lookupProfile = !PartnerHelper.IsAzureBasedPartner(partner) || string.IsNullOrWhiteSpace(billableAccountId);

            Dictionary<string, string> contextGroup;
            if (!this.ClientContext.ContainsKey(contextKeys[0]) || string.Equals(contextKeys[0], GlobalConstants.ClientContextGroups.TaxData))
            {
                if (string.Equals(contextKeys[0], GlobalConstants.ClientContextGroups.CMProfile) && !isJarvisV3 && lookupProfile)
                {
                    await this.AddCMProfileToClientContext(accountId, partner, traceActivityId);
                }
                else if (string.Equals(contextKeys[0], GlobalConstants.ClientContextGroups.CMProfileV3) && isJarvisV3 && lookupProfile)
                {
                    await this.AddCMProfileV3ToClientContext(accountId, partner, traceActivityId);
                }
                else if (string.Equals(contextKeys[0], GlobalConstants.ClientContextGroups.CMProfileAddress) && !isJarvisV3 && lookupProfile)
                {
                    await this.AddCMProfileAddressToClientContext(accountId, country, partner, traceActivityId);
                }
                else if (string.Equals(contextKeys[0], GlobalConstants.ClientContextGroups.CMProfileAddressV3) && isJarvisV3 && lookupProfile)
                {
                    await this.AddCMProfileAddressV3ToClientContext(accountId, country, partner, traceActivityId);
                }
                else if (string.Equals(contextKeys[0], GlobalConstants.ClientContextGroups.LegacyBillableAccountAddress))
                {
                    await this.AddLegacyBillableAccountAddressToClientContext(billableAccountId, partner, traceActivityId);
                }
                else if (string.Equals(contextKeys[0], GlobalConstants.ClientContextGroups.TaxData))
                {
                    await this.AddTaxDataToClientContext(accountId, country, partner, traceActivityId, pidlType);
                }
            }

            if (this.ClientContext.ContainsKey(contextKeys[0]))
            {
                contextGroup = this.ClientContext[contextKeys[0]] as Dictionary<string, string>;
                if (contextGroup != null)
                {
                    contextGroup.TryGetValue(contextKeys[1], out retVal);
                }
            }

            return retVal;
        }

        /// <summary>
        /// This function prefills the inputPidl with user specific data (e.g. MSAProfile, CommerceProfile.Address)
        /// if possible/applicable.
        /// </summary>
        /// <param name="inputPidl">Input Pidl that will be prefilled</param>
        /// <param name="accountId">Commerce account id of the user making this request</param>
        /// <param name="country">Country of the current user request</param>
        /// <param name="partner">Partner of the current user request</param>
        /// <param name="traceActivityId">Trace activity id to log to</param>
        /// <param name="pidlType">Type of Input pidl</param>
        /// <param name="completePrerequisites">Indicates whether the calling pidl to be prefilled has completePrerequisites</param>
        /// <param name="billableAccountId">Legacy Billable account id of the user making this request</param>
        /// <returns>Returns a task which completes when the prefill operation is complete</returns>
        protected async Task PrefillUserData(List<PIDLResource> inputPidl, string accountId, string country, string partner, EventTraceActivity traceActivityId, string pidlType = null, bool completePrerequisites = false, string billableAccountId = null)
        {
            // As discussed, for now, there is no scenario requires both service side prefill and client side prefill.
            // The following code tries to do service side prefill. So expression should be removed here.
            ProxyController.RemoveDefaultValueIfExpression(inputPidl);

            try
            {
                foreach (PIDLResource pidl in inputPidl)
                {
                    foreach (string propertyName in pidl.DataDescription.Keys)
                    {
                        PropertyDescription propertyDescription = pidl.DataDescription[propertyName] as PropertyDescription;
                        if (propertyDescription == null)
                        {
                            // This must be a sub-pidl (e.g Address witin a CC pidl)
                            List<PIDLResource> subPidl = pidl.DataDescription[propertyName] as List<PIDLResource>;
                            if (subPidl != null)
                            {
                                await this.PrefillUserData(subPidl, accountId, country, partner, traceActivityId, pidlType, completePrerequisites, billableAccountId);
                            }
                        }
                        else if (propertyDescription.DefaultValue == null)
                        {
                            string destinationPropertyName = string.Format(GlobalConstants.ClientContextKeys.Format, pidl.Identity["description_type"], propertyName);
                            string[] contextKeys = null;
                            if (GlobalConstants.PrefillMapping.TryGetValue(destinationPropertyName, out contextKeys))
                            {
                                propertyDescription.DefaultValue = await this.TryGetClientContext(contextKeys, accountId, country, traceActivityId, partner, pidlType, completePrerequisites, billableAccountId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // We want prefilling to be a best-effort operation.  Any errors during prefilling should be logged
                // and the PIDL should be returned as it was just before the failure. (it could be partially prefilled)
                SllWebLogger.TracePXServiceException(ex.ToString(), traceActivityId);
            }
        }

        /// <summary>
        /// This function returns the default address in the specific country, if the matching country address isn't the default, then set it as default and return.
        /// </summary>
        /// <param name="accountId">Account id of profile</param>
        /// <param name="profile">Account profile</param>
        /// <param name="country">Country of the current user request</param>
        /// <param name="traceActivityId">Trace activity id to log to</param>
        /// <returns>Returns address if address is found or null</returns>
        protected async Task<AddressInfo> ReturnDefaultAddressByCountry(string accountId, AccountProfile profile, string country, EventTraceActivity traceActivityId)
        {
            AddressInfo defaultAddress = null;
            if (profile != null)
            {
                string addressId = profile.DefaultAddressId;
                if (!string.IsNullOrEmpty(addressId))
                {
                    AddressInfo address = await this.Settings.AccountServiceAccessor.GetAddress<AddressInfo>(accountId, addressId, GlobalConstants.AccountServiceApiVersion.V2, traceActivityId);
                    if (address != null && string.Equals(country, address.Country, StringComparison.OrdinalIgnoreCase))
                    {
                        defaultAddress = address;
                        return defaultAddress;
                    }
                }

                // We are here means that the default address is null or it does not match the current country.  Check to see if this 
                // account has any other address for the current country.
                CMResources<AddressInfo> addresses = await this.Settings.AccountServiceAccessor.GetAddressesByCountry<CMResources<AddressInfo>>(accountId, country, GlobalConstants.AccountServiceApiVersion.V2, traceActivityId);
                if (addresses != null && addresses.ItemCount > 0 && addresses.Items != null)
                {
                    defaultAddress = addresses.Items[0];
                    if (defaultAddress != null)
                    {
                        // Make this the default address
                        profile.DefaultAddressId = defaultAddress.Id;

                        // Remove the try catch block, once account team grants our prod cert update permission. 
                        try
                        {
                            await this.Settings.AccountServiceAccessor.UpdateProfile(accountId, profile, traceActivityId);
                        }
                        catch (Exception)
                        {
                            defaultAddress = null;
                        }
                    }
                }
            }

            return defaultAddress;
        }

        /// <summary>
        /// This function returns the default addressV3 in the specific country, if the matching country address isn't the default, then set it as default and return.
        /// </summary>
        /// <param name="accountId">Account id of profile</param>
        /// <param name="profile">Account profile V3</param>
        /// <param name="profileType">Type of profile V3</param>
        /// <param name="country">Country of the current user request</param>
        /// <param name="traceActivityId">Trace activity id to log to</param>
        /// <returns>Returns address if address is found or null</returns>
        protected async Task<AddressInfoV3> ReturnDefaultAddressV3ByCountry(string accountId, AccountProfileV3 profile, string profileType, string country, EventTraceActivity traceActivityId)
        {
            AddressInfoV3 defaultAddress = null;
            if (profile != null)
            {
                string addressId = profile.DefaultAddressId;
                if (!string.IsNullOrEmpty(addressId))
                {
                    AddressInfoV3 address = await this.Settings.AccountServiceAccessor.GetAddress<AddressInfoV3>(accountId, addressId, GlobalConstants.AccountServiceApiVersion.V3, traceActivityId);
                    if (address != null && string.Equals(country, address.Country, StringComparison.OrdinalIgnoreCase))
                    {
                        defaultAddress = address;
                        return defaultAddress;
                    }
                }

                // We are here means that the default address is null or it does not match the current country.  Check to see if this 
                // account has any other address for the current country.
                CMResources<AddressInfoV3> addresses = await this.Settings.AccountServiceAccessor.GetAddressesByCountry<CMResources<AddressInfoV3>>(accountId, country, GlobalConstants.AccountServiceApiVersion.V3, traceActivityId);
                if (addresses != null && addresses.ItemCount > 0 && addresses.Items != null)
                {
                    defaultAddress = addresses.Items[0];
                    if (defaultAddress != null)
                    {
                        // Make this the default address
                        profile.DefaultAddressId = defaultAddress.Id;

                        // Remove the try catch block, once account team grants our prod cert update permission. 
                        try
                        {
                            await this.Settings.AccountServiceAccessor.UpdateProfileV3(accountId, profile, profileType, traceActivityId, this.ExposedFlightFeatures);
                        }
                        catch (Exception)
                        {
                            defaultAddress = null;
                        }
                    }
                }
            }

            return defaultAddress;
        }

        /// <summary>
        /// This function returns the default addressV3 
        /// </summary>
        /// <param name="accountId">Account id of profile</param>
        /// <param name="profile">Account profile V3</param>
        /// <param name="traceActivityId">Trace activity id to log to</param>
        /// <returns>Returns address if address is found or null</returns>
        protected async Task<AddressInfoV3> TryGetDefaultAddressV3(string accountId, AccountProfileV3 profile, EventTraceActivity traceActivityId)
        {
            AddressInfoV3 defaultAddress = null;

            try
            {
                if (profile != null)
                {
                    string addressId = profile.DefaultAddressId;
                    if (!string.IsNullOrEmpty(addressId))
                    {
                        defaultAddress = await this.Settings.AccountServiceAccessor.GetAddress<AddressInfoV3>(accountId, addressId, GlobalConstants.AccountServiceApiVersion.V3, traceActivityId);
                    }
                }
            }
            catch (Exception ex)
            {
                // When Jarvis is down, catch the exception and continue with returning a PIDL assuming that the defaultAddress is null
                SllWebLogger.TracePXServiceException(ex.ToString(), traceActivityId);
            }

            return defaultAddress;
        }

        protected string GetProfileType()
        {
            string profileType = GlobalConstants.ProfileTypes.Consumer;
            if (this.ClientContext.ContainsKey(GlobalConstants.ClientContextGroups.AuthInfo))
            {
                Dictionary<string, string> authInfo = this.ClientContext[GlobalConstants.ClientContextGroups.AuthInfo] as Dictionary<string, string>;
                if (authInfo != null)
                {
                    string authenticationType = null;
                    authInfo.TryGetValue(GlobalConstants.AuthInfoNames.Type, out authenticationType);
                    string authenticationContext = null;
                    authInfo.TryGetValue(GlobalConstants.AuthInfoNames.Context, out authenticationContext);

                    if (string.Equals(authenticationType, Constants.AuthenticationTypes.Aad, StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(authenticationContext, Constants.UserTypes.UserMe, StringComparison.OrdinalIgnoreCase))
                        {
                            profileType = GlobalConstants.ProfileTypes.Employee;
                        }
                        else if (string.Equals(authenticationContext, Constants.UserTypes.UserMyOrg, StringComparison.OrdinalIgnoreCase))
                        {
                            profileType = GlobalConstants.ProfileTypes.Organization;
                        }
                    }
                }
            }

            return profileType;
        }

        protected List<string> GetPartnerXMSFlightExposed()
        {
            List<string> xmsFlightHeader = null;
            string flightValueString = this.Request.GetRequestHeader(GlobalConstants.HeaderValues.ExtendedFlightName);
            if (!string.IsNullOrEmpty(flightValueString))
            {
                xmsFlightHeader = new List<string>(flightValueString.Split(','));
            }

            return xmsFlightHeader;
        }

        protected bool IsPartnerFlightExposed(string flightName)
        {
            string flightValueString = this.Request.GetRequestHeader(GlobalConstants.HeaderValues.ExtendedFlightName);
            if (flightValueString != null)
            {
                List<string> flightValue = new List<string>(flightValueString.Split(','));
                return flightValue.Contains(flightName, StringComparer.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Determines whether to use PaymentRequest instead of CheckoutRequest based on flight settings
        /// </summary>
        /// <returns>True if PaymentRequest should be used, false otherwise</returns>
        protected bool UsePaymentRequestApiEnabled()
        {
            // Check if the UsePaymentRequestApi flight feature is enabled
            return this.ExposedFlightFeatures?.Contains(Flighting.Features.UsePaymentRequestApi, StringComparer.OrdinalIgnoreCase) ?? false;
        }

        protected async Task<string> GetTokenizationPublicKey(EventTraceActivity traceActivityId)
        {
            string encryptionKey = string.Empty;
            if (this.IsTokenizationEncryptionEnabled())
            {
                encryptionKey = await this.Settings.TokenizationServiceAccessor.GetEncryptionKey(traceActivityId, this.ExposedFlightFeatures);
            }

            return encryptionKey;
        }

        protected bool IsTokenizationEncryptionEnabled()
        {
            return this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncryptionAddUpdateCC, StringComparer.OrdinalIgnoreCase)
                    || this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncryptionOtherOperation, StringComparer.OrdinalIgnoreCase)
                    || this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncryptionFetchConfigAddUpdateCC, StringComparer.OrdinalIgnoreCase)
                    || this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncryptionFetchConfigOtherOperation, StringComparer.OrdinalIgnoreCase)
                    || this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncFetchConfigAddCCPiAuthKey, StringComparer.OrdinalIgnoreCase);
        }

        protected void RemovePartnerFlight(string flightName)
        {
            string flightValueString = this.Request.GetRequestHeader(GlobalConstants.HeaderValues.ExtendedFlightName);
            if (flightValueString != null)
            {
                List<string> flightValue = new List<string>(flightValueString.Split(','));
                int targetFlightIndex = flightValue.FindIndex(x => x.Equals(flightName, StringComparison.OrdinalIgnoreCase));
                if (targetFlightIndex != -1)
                {
                    flightValue.RemoveAt(targetFlightIndex);
                    string newflightValueString = string.Join(",", flightValue.ToArray());
                    this.Request.Headers.Remove(GlobalConstants.HeaderValues.ExtendedFlightName);
                    this.Request.Headers.Add(GlobalConstants.HeaderValues.ExtendedFlightName, newflightValueString);
                }
            }
        }

        protected void AddFlightHeader(string flightName)
        {
            string flightValueString = this.Request.GetRequestHeader(GlobalConstants.HeaderValues.ExtendedFlightName);
            if (flightValueString != null)
            {
                List<string> flightValue = new List<string>(flightValueString.Split(','));
                int targetFlightIndex = flightValue.FindIndex(x => x.Equals(flightName, StringComparison.OrdinalIgnoreCase));
                if (targetFlightIndex == -1)
                {
                    flightValue.Add(flightName);
                    string newflightValueString = string.Join(",", flightValue.ToArray().Where(s => !string.IsNullOrEmpty(s)));
                    this.Request.Headers.Remove(GlobalConstants.HeaderValues.ExtendedFlightName);
                    this.Request.Headers.Add(GlobalConstants.HeaderValues.ExtendedFlightName, newflightValueString);
                }
            }
        }

        protected async Task<PayerAuth.BrowserInfo> CollectBrowserInfo()
        {
            PayerAuth.BrowserInfo clientBrowserInfo = new PayerAuth.BrowserInfo();

            try
            {
                HttpContextWrapper requestContext = null;
                this.Request.Properties.TryGetValue("MS_HttpContext", out requestContext);

                string ipAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);
                clientBrowserInfo.BrowserIP = !string.IsNullOrWhiteSpace(ipAddress) ? ipAddress : requestContext?.Request.UserHostAddress;

                string userAgent = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.UserAgent);
                clientBrowserInfo.BrowserUserAgent = !string.IsNullOrWhiteSpace(userAgent) ? userAgent : requestContext?.Request.UserAgent;

                clientBrowserInfo.BrowserAcceptHeader = this.Request.Headers.Accept.ToString();
                clientBrowserInfo.BrowserJavaEnabled = requestContext?.Request.Browser.JavaApplets;
                clientBrowserInfo.BrowserColorDepth = requestContext?.Request.Browser.ScreenBitDepth.ToString();
                clientBrowserInfo.BrowserScreenHeight = requestContext?.Request.Browser.ScreenPixelsHeight.ToString();
                clientBrowserInfo.BrowserScreenWidth = requestContext?.Request.Browser.ScreenPixelsWidth.ToString();
            }
            catch
            {
                // Task 23439705: Log specific events to ease querying for SafetyNet events
            }

            return clientBrowserInfo;
        }

        protected RequestContext GetRequestContext(EventTraceActivity traceActivityId)
        {
            return HttpRequestHelper.GetRequestContext(this.Request, traceActivityId);
        }

        private static string BuildHiddenCheckboxHintId(string propertyName)
        {
            return $"HiddenCheckbox_{propertyName}";
        }

        private static string Base64Decode(string encodedString)
        {
            string retVal = string.Empty;
            try
            {
                var decodedData = Convert.FromBase64String(encodedString);
                retVal = System.Text.Encoding.UTF8.GetString(decodedData);
            }
            catch
            {
                // TODO: Log this
            }

            return retVal;
        }

        private static bool IsDefaultValueExpression(string defaultValue)
        {
            return !string.IsNullOrEmpty(defaultValue)
                && ((defaultValue.StartsWith("({") && defaultValue.EndsWith("})")) || (defaultValue.StartsWith("(<|") && defaultValue.EndsWith("|>)")));
        }

        private async Task<ClientAction> GenerateChallengeClientAction(BrowserFlowContext result, PaymentSession paymentSession, RequestContext requestContext, string piid, EventTraceActivity traceActivityId)
        {
            List<PIDLResource> pidls = null;
            bool cspFrameEnabled = this.ExposedFlightFeatures.Contains(Flighting.Features.PXPSD2EnableCSPProxyFrame, StringComparer.OrdinalIgnoreCase);
            bool cspSourceUrlEnabled = this.ExposedFlightFeatures.Contains(Flighting.Features.PXPSD2EnableCSPUrlProxyFrame, StringComparer.OrdinalIgnoreCase) || this.ExposedFlightFeatures.Contains(Flighting.Features.PXPSD2EnableCSPUrlProxyFrameWithSanitizedInput, StringComparer.OrdinalIgnoreCase);
            string pxAuthUrl = string.Format("{0}/paymentSessions/{1}/authenticate", this.Settings.PifdBaseUrl, paymentSession.Id);
            var testHeader = HttpRequestHelper.GetRequestHeader(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader);
            if (result.IsFingerPrintRequired)
            {
                // step 1.1 genearate fingerprint pidl
                string cspStep = V7.Constants.CSPStepNames.None;
                string formActionUrl = result.FormActionURL;

                // handle iframe auth timeout as failure client action
                var errorDetails = new ServiceErrorResponse { ErrorCode = PaymentChallengeStatus.Failed.ToString(), InnerError = new ServiceErrorResponse { ErrorCode = "authTimeout", Message = PaymentChallengeStatus.Failed.ToString() } };
                if (cspFrameEnabled || cspSourceUrlEnabled)
                {
                    cspStep = V7.Constants.CSPStepNames.Fingerprint;
                    formActionUrl = pxAuthUrl;
                }

                if (cspSourceUrlEnabled)
                {
                    pidls = PIDLResourceFactory.GetThreeDSFingerprintUrlIFrameDescription(formActionUrl, result.FormInputThreeDSMethodData, paymentSession.Id, pxAuthUrl, cspStep, testHeader, errorDetails, this.ExposedFlightFeatures);
                }
                else
                {
                    pidls = PIDLResourceFactory.GetThreeDSFingerprintIFrameDescription(formActionUrl, result.FormInputThreeDSMethodData, paymentSession.Id, pxAuthUrl, cspStep, testHeader, errorDetails, this.ExposedFlightFeatures);
                }
            }
            else if (result.IsAcsChallengeRequired)
            {
                // step 1.1 genearate acs challenge pidl
                string cspStep = V7.Constants.CSPStepNames.None;
                string formActionUrl = result.FormActionURL;

                if (cspFrameEnabled || cspSourceUrlEnabled)
                {
                    cspStep = V7.Constants.CSPStepNames.Challenge;
                    formActionUrl = pxAuthUrl;
                }

                if (cspSourceUrlEnabled)
                {
                    pidls = PIDLResourceFactory.GetThreeDSChallengeUrlIFrameDescription(
                        formActionUrl,
                        result.FormInputCReq,
                        result.FormInputThreeDSSessionData,
                        paymentSession.Id,
                        cspStep,
                        result.ChallengeWindowSize.Width,
                        result.ChallengeWindowSize.Height,
                        testHeader,
                        this.ExposedFlightFeatures);
                }
                else
                {
                    pidls = PIDLResourceFactory.GetThreeDSChallengeIFrameDescription(
                        formActionUrl,
                        result.FormInputCReq,
                        result.FormInputThreeDSSessionData,
                        paymentSession.Id,
                        cspStep,
                        result.ChallengeWindowSize.Width,
                        result.ChallengeWindowSize.Height,
                        testHeader);
                }
            }
            else if (result.PaymentSession.ChallengeStatus == PaymentChallengeStatus.Succeeded
                || result.PaymentSession.ChallengeStatus == PaymentChallengeStatus.ByPassed
                || result.PaymentSession.ChallengeStatus == PaymentChallengeStatus.NotApplicable)
            {
                // step 1.3 if challenge status is succeeded or by passed or applicable, inform PO service
                if (this.UsePaymentRequestApiEnabled())
                {
                    var clientActions = await this.Settings.PaymentOrchestratorServiceAccessor.PSD2AttachChallengeDataToPaymentRequest(
                        requestContext.RequestId,
                        piid,
                        PaymentInstrumentChallengeType.ThreeDs2,
                        result.PaymentSession.ChallengeStatus,
                        paymentSession.Id,
                        traceActivityId,
                        requestContext.TenantId);

                    return new ClientAction(ClientActionType.ReturnContext, clientActions);
                }
                else
                {
                    var clientActions = await this.Settings.PaymentOrchestratorServiceAccessor.PSD2AttachChallengeData(
                        requestContext.RequestId,
                        piid,
                        PaymentInstrumentChallengeType.ThreeDs2,
                        result.PaymentSession.ChallengeStatus,
                        paymentSession.Id,
                        traceActivityId,
                        requestContext.TenantId);

                    return new ClientAction(ClientActionType.ReturnContext, clientActions);
                }
            }

            if (pidls != null)
            {
                foreach (PIDLResource pidl in pidls)
                {
                    //// this is needed for payment client to recognize the pidl as a challenge pidl
                    if (pidl?.DisplayPages?.Count > 0)
                    {
                        pidl.DisplayPages[0].HintId = "challenge_" + pidl.DisplayPages[0].HintId;
                    }
                }

                return new ClientAction(ClientActionType.Pidl, pidls);
            }

            return null;
        }

        private async Task AddCMProfileToClientContext(string accountId, string partner, EventTraceActivity traceActivityId)
        {
            // TODO Bug 1683007:[PX AP] Optimize getting user profile and address across PX controllers 
            string profileType = this.GetProfileType();

            // We only want to attempt getting the CMProfile once per request.  Setting the ClientContext value to null acts a flag
            // to indicate that an attempt to add was made.
            this.ClientContext[GlobalConstants.ClientContextGroups.CMProfile] = null;
            AccountProfile profile = await this.Settings.AccountServiceAccessor.GetProfile(accountId, profileType, traceActivityId);
            if (profile != null)
            {
                Dictionary<string, string> contextProfile = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                this.ClientContext[GlobalConstants.ClientContextGroups.CMProfile] = contextProfile;

                contextProfile[GlobalConstants.CMProfileFields.FirstName] = profile.FirstName;
                contextProfile[GlobalConstants.CMProfileFields.LastName] = profile.LastName;
                contextProfile[GlobalConstants.CMProfileFields.EmailAddress] = profile.EmailAddress;
                contextProfile[GlobalConstants.CMProfileFields.ProfileId] = profile.Id;
                contextProfile[GlobalConstants.CMProfileFields.DefaultAddressId] = profile.DefaultAddressId;
                contextProfile[GlobalConstants.CMProfileFields.Culture] = profile.Culture;
                contextProfile[GlobalConstants.CMProfileFields.CompanyName] = profile.CompanyName;
                contextProfile[GlobalConstants.CMProfileFields.ProfileType] = profile.ProfileType;
                contextProfile[GlobalConstants.CMProfileFields.Nationality] = profile.Nationality;
                contextProfile[GlobalConstants.CMProfileFields.BirthDate] = profile.DateOfBirth;
            }
        }

        private async Task AddCMProfileV3ToClientContext(string accountId, string partner, EventTraceActivity traceActivityId)
        {
            // TODO Bug 1683007:[PX AP] Optimize getting user profile and address across PX controllers 
            string profileType = this.GetProfileType();

            // We only want to attempt getting the CMProfileV3 once per request.  Setting the ClientContext value to null acts a flag
            // to indicate that an attempt to add was made.
            this.ClientContext[GlobalConstants.ClientContextGroups.CMProfileV3] = null;
            AccountProfileV3 profile = await this.Settings.AccountServiceAccessor.GetProfileV3(accountId, profileType, traceActivityId);
            if (profile != null)
            {
                Dictionary<string, string> contextProfile = profile.GetPropertyDictionary();
                this.ClientContext[GlobalConstants.ClientContextGroups.CMProfileV3] = contextProfile;
            }
        }

        private async Task AddCMProfileAddressToClientContext(string accountId, string country, string partner, EventTraceActivity traceActivityId)
        {
            string profileType = this.GetProfileType();

            // We only want to attempt getting the CMProfileAddress once per request.  Setting the ClientContext value to null acts a flag
            // to indicate that an attempt to add was made.
            this.ClientContext[GlobalConstants.ClientContextGroups.CMProfileAddress] = null;
            AccountProfile profile = await this.Settings.AccountServiceAccessor.GetProfile(accountId, profileType, traceActivityId);
            AddressInfo address = await this.ReturnDefaultAddressByCountry(accountId, profile, country, traceActivityId);
            if (address != null)
            {
                Dictionary<string, string> contextAddress = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                this.ClientContext[GlobalConstants.ClientContextGroups.CMProfileAddress] = contextAddress;

                contextAddress[GlobalConstants.CMAddressFields.AddressLine1] = address.AddressLine1;
                contextAddress[GlobalConstants.CMAddressFields.AddressLine2] = address.AddressLine2;
                contextAddress[GlobalConstants.CMAddressFields.AddressLine3] = address.AddressLine3;
                contextAddress[GlobalConstants.CMAddressFields.City] = address.City;
                contextAddress[GlobalConstants.CMAddressFields.Region] = address.State;
                contextAddress[GlobalConstants.CMAddressFields.PostalCode] = address.Zip;
                contextAddress[GlobalConstants.CMAddressFields.Country] = address.Country;
            }
        }

        private async Task AddCMProfileAddressV3ToClientContext(string accountId, string country, string partner, EventTraceActivity traceActivityId)
        {
            string profileType = this.GetProfileType();

            // We only want to attempt getting the CMProfileAddressV3 once per request.  Setting the ClientContext value to null acts a flag
            // to indicate that an attempt to add was made.
            this.ClientContext[GlobalConstants.ClientContextGroups.CMProfileAddressV3] = null;
            AccountProfileV3 profile = await this.Settings.AccountServiceAccessor.GetProfileV3(accountId, profileType, traceActivityId);
            AddressInfoV3 address = await this.ReturnDefaultAddressV3ByCountry(accountId, profile, profileType, country, traceActivityId);
            if (address != null)
            {
                Dictionary<string, string> contextAddress = address.GetPropertyDictionary();
                this.ClientContext[GlobalConstants.ClientContextGroups.CMProfileAddressV3] = contextAddress;
            }
        }

        private async Task AddLegacyBillableAccountAddressToClientContext(string billableAccountId, string partner, EventTraceActivity traceActivityId)
        {
            // Enabling prefillig client context with address from legacy billable account only if the billableAccountId is sent
            if (string.IsNullOrEmpty(billableAccountId)
                || !(PartnerHelper.IsAzureBasedPartner(partner) || PartnerHelper.IsGGPDEDSPartner(partner)))
            {
                return;
            }

            // We only want to attempt getting the LegacyBillableAccountAddress once per request.  Setting the ClientContext value to null acts a flag
            // to indicate that an attempt to add was made.
            this.ClientContext[GlobalConstants.ClientContextGroups.LegacyBillableAccountAddress] = null;

            // PUID is received in AltSecId for Azure based partners
            string altSecId = null;
            if (PartnerHelper.IsAzureBasedPartner(partner))
            {
                altSecId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.AltSecId);
            }
            else
            {
                altSecId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
            }

            string orgPuid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.OrgPuid);

            PayinPayoutAccount account = LegacyAccountHelper.GetLegacyBillableAccountFromId(this.Settings, billableAccountId, traceActivityId, altSecId, orgPuid, GlobalConstants.Defaults.Language);

            // Update ClientContext only if address set on PayIn account is not empty
            if (account.PayinAccount == null || account.PayinAccount.AddressSet == null || account.PayinAccount.AddressSet.Count <= 0)
            {
                return;
            }

            Address address = account.PayinAccount.AddressSet[0];
            if (address != null)
            {
                Dictionary<string, string> contextAddress = address.GetPropertyDictionary();
                this.ClientContext[GlobalConstants.ClientContextGroups.LegacyBillableAccountAddress] = contextAddress;
            }
        }

        private async Task AddTaxDataToClientContext(string accountId, string country, string partner, EventTraceActivity traceActivityId, string pidlType)
        {
            // We only want to attempt getting the TaxData once per request.  Setting the ClientContext value to null acts a flag
            // to indicate that an attempt to add was made.
            this.ClientContext[GlobalConstants.ClientContextGroups.TaxData] = null;

            string profileType = this.GetProfileType();

            // Tax prefilling is only required by update profile scenario.
            // For update profile scenario, it always passes pidlType/taxPidlType.
            if (pidlType != null)
            {
                TaxData[] taxDatas = await this.Settings.TaxIdServiceAccessor.GetTaxIdsByProfileTypeAndCountryWithState(accountId, profileType, country, traceActivityId);
                if (taxDatas != null)
                {
                    TaxData taxDataEntry = null;

                    // For tax id GST in India, search the value based on state and status
                    // For tax id others, search the value based on type
                    if (string.Equals(pidlType, Constants.CommercialTaxIdTypes.IndiaGst, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Dictionary<string, string> contextGroup = this.ClientContext[GlobalConstants.ClientContextGroups.CMProfileAddressV3] as Dictionary<string, string>;
                        if (contextGroup != null)
                        {
                            string state = null;
                            contextGroup.TryGetValue(GlobalConstants.CMAddressV3Fields.Region, out state);
                            if (state != null)
                            {
                                string stateInitials = PidlFactory.V7.PIDLResourceFactory.GetMappingStateIndia(state);
                                if (stateInitials != null)
                                {
                                    foreach (TaxData taxData in taxDatas)
                                    {
                                        if (string.Equals(taxData.TaxIdType, Constants.CommercialTaxIdTypes.IndiaGst, StringComparison.InvariantCultureIgnoreCase)
                                            && taxData.Scope != null && string.Equals(taxData.Scope.State, stateInitials, StringComparison.InvariantCultureIgnoreCase)
                                            && string.Equals(taxData.Status, Constants.CommercialTaxIdStatus.Valid, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            taxDataEntry = taxData;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (TaxData taxData in taxDatas)
                        {
                            if (string.Equals(taxData.TaxIdType, pidlType, StringComparison.InvariantCultureIgnoreCase)
                                && string.Equals(taxData.Status, Constants.CommercialTaxIdStatus.Valid, StringComparison.InvariantCultureIgnoreCase))
                            {
                                taxDataEntry = taxData;
                                break;
                            }
                        }
                    }

                    Dictionary<string, string> contextTaxData = taxDataEntry.GetPropertyDictionary();
                    this.ClientContext[GlobalConstants.ClientContextGroups.TaxData] = contextTaxData;
                }
            }
        }

        /// <summary>
        /// This function parses client context headers is into the clientContext dictionary.  If the clientContext is null, 
        /// it will be instantiated.  
        /// <para> 
        /// Client context headers are expected to be as shown in the below example: 
        /// Headers["x-ms-deviceinfo"] = "ipAddress=131.107.174.30,xboxLiveDeviceId=18230582934452242973"
        /// If GlobalConstants.ClientContextTypes["x-ms-deviceinfo"] = "DeviceInfo", the above example will lead 
        /// to 2 entries being written to the clientContext dictionary as below:
        ///     clientContext["DeviceInfo.ipAddress"] = "131.107.174.30"
        ///     clientContext["DeviceInfo.xboxLiveDeviceId"] = "18230582934452242973"
        /// </para>
        /// <para>
        /// If a client context header does not exist in the incoming request headers, clientContext is not modified.
        /// If this function finds a client context that already exists in the clientContext, the value will be overwritten.
        /// </para>
        /// </summary>
        private void InitializeClientContext()
        {
            if (this.clientContext == null)
            {
                this.clientContext = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            bool isEncoded = false;
            string clientContextEncoding = null;
            if (this.Request?.Headers != null)
            {
                IEnumerable<string> values = new List<string>();
                this.Request?.Headers.TryGetValues(GlobalConstants.HeaderValues.ClientContextEncoding, out values);
                clientContextEncoding = values?.FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(clientContextEncoding))
            {
                isEncoded = true;
            }

            foreach (string headerName in GlobalConstants.ClientContextGroups.FromHeader.Keys)
            {
                string headerValue = null;
                if (this.Request?.Headers != null)
                {
                    IEnumerable<string> values = new List<string>();
                    this.Request?.Headers.TryGetValues(headerName, out values);
                    headerValue = values?.FirstOrDefault();
                }

                if (headerValue == null)
                {
                    continue;
                }

                string groupName = GlobalConstants.ClientContextGroups.FromHeader[headerName];
                Dictionary<string, string> contextGroup = null;
                object result = null;
                this.clientContext.TryGetValue(groupName, out result);
                contextGroup = result as Dictionary<string, string>;
                if (contextGroup == null)
                {
                    contextGroup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    this.clientContext[groupName] = contextGroup;
                }

                headerValue = headerValue.Trim('"');
                string[] keyValuePairs = headerValue.Split(',');
                foreach (string keyValuePair in keyValuePairs)
                {
                    int indexOfDelim = keyValuePair.IndexOf('=');
                    if (indexOfDelim > -1)
                    {
                        string key = keyValuePair.Substring(0, indexOfDelim).Trim();
                        string value = keyValuePair.Substring(indexOfDelim + 1);

                        if (isEncoded)
                        {
                            value = ProxyController.Base64Decode(value);
                        }

                        contextGroup[key] = value;
                    }
                }
            }
        }
    }
}
