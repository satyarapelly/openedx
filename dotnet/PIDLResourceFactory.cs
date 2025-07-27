// <copyright file="PIDLResourceFactory.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7.FeatureContextProcess;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration.Generators;
    using Microsoft.Commerce.Payments.PidlModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;
    using QRCoder;

    /// <summary>
    /// This class acts as the data source for PaymentMethodDescriptions by encapsulating logic to
    /// read and parse data files into a dictionary of PaymentMethodDescriptions and a dictionary
    /// of PaymentMethodPropertyDescriptions that make up PaymentMethodDescriptions.
    /// </summary>
    public sealed class PIDLResourceFactory
    {
        private static readonly IFeatureFactory FeatureFactoryInstance = new FeatureFactory();
        private static readonly IFeatureContextFactory FeatureContextFactoryInstance = new FeatureContextFactory();
        private static readonly IPIDLGenerationFactory<ClientAction> ClientActionGenerationFactoryInstance = new ClientActionGenerationFactory();
        private static readonly PIDLResourceFactory InstanceField = new PIDLResourceFactory();
        private static Dictionary<string, Dictionary<string, string>> domainDictionaries;
        private static List<string> partnerNames;

        private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, DataSourcesConfig>>>>> dataSources;
        private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, SubmitLink>>>>> submitLinks;
        private Dictionary<string, Dictionary<string, HashSet<string>>> taxIdsInCountries;
        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> validationChallengeTypes;
        private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, PIDLResourceConfig>>>>> pidlResourceConfigs;
        private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, PropertyDescription>>>> propertyDescriptions;
        private Dictionary<string, Dictionary<string, Dictionary<string, List<PropertyValidation>>>> propertyValidationLists;
        private Dictionary<string, Dictionary<string, Dictionary<string, PropertyTransformationInfo>>> propertyTransformations;
        private Dictionary<string, Dictionary<string, PropertyDataProtection>> propertyDataProtections;

        /// <summary>
        /// Prevents a default instance of the <see cref="PIDLResourceFactory"/> class from being created.
        /// It reads data from csv files and parses it into PaymentMethodDescriptions and
        /// PaymentMethodPropertyDescritions object in memory so we can serve them out on request.
        /// </summary>
        private PIDLResourceFactory()
        {
            this.Initialize();
        }

        public static PIDLResourceFactory Instance
        {
            get
            {
                return InstanceField;
            }
        }

        public static IFeatureFactory FeatureFactory
        {
            get
            {
                return FeatureFactoryInstance;
            }
        }

        public static IFeatureContextFactory FeatureContextFactory
        {
            get
            {
                return FeatureContextFactoryInstance;
            }
        }

        public static IPIDLGenerationFactory<ClientAction> ClientActionGenerationFactory
        {
            get
            {
                return ClientActionGenerationFactoryInstance;
            }
        }

        public static Dictionary<string, Dictionary<string, string>> GetDomainDictionaries()
        {
            return domainDictionaries;
        }

        public static bool IsTemplateInList(string partner, PaymentExperienceSetting setting, string descriptionType, string resourceId)
        {
            string templateOrPartnerName = TemplateHelper.GetSettingTemplate(partner, setting, descriptionType, resourceId);
            return TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(templateOrPartnerName);
        }

        public static bool IsQRcodePayPalFlowInTemplate(PaymentExperienceSetting setting)
        {
            return string.Equals(setting?.RedirectionPattern, Constants.RedirectionPatterns.QRCode, StringComparison.OrdinalIgnoreCase);
        }

        public static List<PIDLResource> GetCheckoutPaymentSelectDescriptions(
            HashSet<PaymentMethod> paymentMethods,
            string country,
            string operation,
            string language,
            string partnerName,
            string paymentProviderId,
            string checkoutId,
            string redirectUrl,
            string defaultPaymentMethod = null,
            List<string> exposedFlightFeatures = null,
            string scenario = null,
            PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);
            operation = Helper.TryToLower(operation);

            // Validate input parameters
            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            List<PIDLResource> retList = new List<PIDLResource>();

            PIDLResource retVal = GetCheckoutPaymentSelectPidl(partnerName, country, Constants.PidlOperationTypes.Select, Constants.PidlResourceIdentities.ThirdPartyPaymentSelectPM, paymentMethods, defaultPaymentMethod, language, paymentProviderId, redirectUrl, checkoutId, null, Constants.DescriptionTypes.PaymentMethodDescription, scenario: scenario, pageIndex: 0, setting: setting);
            retList.Add(retVal);

            // for each PM option in possibleOptions, add a new pidl which contains the button to load the checkout form for the PM option
            var propertyDisplayHint = retVal.GetDisplayHintById("paymentMethod") as PropertyDisplayHint;

            if (propertyDisplayHint == null)
            {
                propertyDisplayHint = retVal.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodTppSelect) as PropertyDisplayHint;
            }

            Dictionary<string, SelectOptionDescription> possibleOptions = propertyDisplayHint.PossibleOptions;
            for (int index = 1; index < possibleOptions.Count; index++)
            {
                PIDLResource pidl = GetCheckoutPaymentSelectPidl(partnerName, country, Constants.PidlOperationTypes.Select, Constants.PidlResourceIdentities.ThirdPartyPaymentSelectPM, paymentMethods, defaultPaymentMethod, language, paymentProviderId, redirectUrl, checkoutId, null, Constants.DescriptionTypes.PaymentMethodDescription, scenario: scenario, pageIndex: index, setting: setting);
                retList.Add(pidl);
            }

            return retList;
        }

        // TODO: Tax ID collection
        // Let profile description controller also call the method below to set IsOptional to be false
        public static void AdjustTaxPropertiesInPIDL(
           List<PIDLResource> pidlResource,
           string country,
           string profileType)
        {
            if (pidlResource == null || pidlResource.Count == 0)
            {
                return;
            }

            // TODO: Task 18605985: [PxService] Move "IsOptional" Property from DataDescription to DisplayDescription
            if (Constants.AllCountriesEnabledTaxIdCheckbox.Contains(country)
                && !string.Equals(profileType, Constants.ProfileTypes.Consumer, StringComparison.OrdinalIgnoreCase))
            {
                PropertyDescription taxIdDescription = pidlResource[0].GetPropertyDescriptionByPropertyName("taxId");
                if (taxIdDescription != null)
                {
                    taxIdDescription.IsOptional = false;
                }
            }
        }

        public static bool IsJarvisProfileV3Partner(string partner, string country = null, PaymentExperienceSetting setting = null)
        {
            string[] jarvisV3Partner = new string[] { Constants.PartnerNames.Commercialstores, Constants.PartnerNames.SmbOobe, Constants.PartnerNames.Azure, Constants.PartnerNames.AzureSignup, Constants.PartnerNames.AzureIbiza, Constants.PartnerNames.WebPay, Constants.TemplateName.OnePage, Constants.TemplateName.TwoPage, Constants.TemplateName.DefaultTemplate };
            bool skipJarvisV3ForProfile = !string.IsNullOrEmpty(country) && PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(Constants.FeatureFlight.SkipJarvisV3ForProfile, country, setting);
            if (skipJarvisV3ForProfile)
            {
                return false;
            }

            if (jarvisV3Partner.Contains(partner, StringComparer.InvariantCultureIgnoreCase))
            {
                return true;
            }

            bool hasUseJarvisV3ProfileFeature = !string.IsNullOrEmpty(country) && PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseJarvisV3ForProfile, country, setting);
            if (hasUseJarvisV3ProfileFeature)
            {
                return true;
            }

            return false;
        }

        public static bool IsJarvisAddressV3Partner(string partner, string country = null, PaymentExperienceSetting setting = null)
        {
            string[] jarvisV3Partner = new string[] { Constants.PartnerNames.Commercialstores, Constants.PartnerNames.SmbOobe, Constants.PartnerNames.Azure, Constants.PartnerNames.AzureSignup, Constants.PartnerNames.AzureIbiza, Constants.PartnerNames.WebPay, Constants.PartnerNames.Cart, Constants.TemplateName.OnePage, Constants.TemplateName.TwoPage, Constants.TemplateName.DefaultTemplate };
            if (jarvisV3Partner.Contains(partner, StringComparer.InvariantCultureIgnoreCase))
            {
                return true;
            }

            bool hasUseJarvisV3ForAddressFeature = !string.IsNullOrEmpty(country) && PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseJarvisV3ForAddress, country, setting);
            if (hasUseJarvisV3ForAddressFeature)
            {
                return true;
            }

            return false;
        }

        public static string GetUrlQrCodeImage(string url)
        {
            try
            {
                QRCodeGenerator generator = new QRCodeGenerator();
                QRCodeData dataQR = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                Base64QRCode codeQR = new Base64QRCode(dataQR);
                string base64Id = codeQR.GetGraphic(Constants.ImageResolution.QrCodeImageResolution);
                return "data:image/png;base64," + base64Id;
            }
            catch (Exception ex)
            {
                throw new PIDLException("Could not generate Alipay QRCode image.  Internal Exception: " + ex.ToString(), Constants.ErrorCodes.PIDLCouldNotGenerateQrCode);
            }
        }

        public static string GetMappingStateIndia(string state)
        {
            return Constants.IndiaStateMapping.GetMappingState(state);
        }

        public static Dictionary<string, string> GetDictionaryFromConfigString(string configText)
        {
            if (string.IsNullOrWhiteSpace(configText))
            {
                throw new PIDLArgumentException(
                    "DomainDictionary Name is null or blank",
                    Constants.ErrorCodes.PIDLArgumentDomainDictionaryNameIsNullOrBlank);
            }

            Dictionary<string, string> retVal = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            if (configText.StartsWith(Constants.ConfigSpecialStrings.CollectionNamePrefix))
            {
                string dictionaryName = configText.Substring(Constants.ConfigSpecialStrings.CollectionNamePrefix.Length);
                if (domainDictionaries.ContainsKey(dictionaryName))
                {
                    retVal = domainDictionaries[dictionaryName];
                }
                else
                {
                    throw new PIDLConfigException(
                        string.Format("Config file references an unknown dictionary name \"{0}\"", dictionaryName),
                        Constants.ErrorCodes.PIDLConfigUnknownDictionaryName);
                }
            }
            else
            {
                string[] collectionElements = configText.Split(new string[] { Constants.ConfigSpecialStrings.CollectionDelimiter }, StringSplitOptions.None);
                foreach (string element in collectionElements)
                {
                    string[] nameValuePair = element.Split(new string[] { Constants.ConfigSpecialStrings.NameValueDelimiter }, StringSplitOptions.None);
                    if (nameValuePair.Length == 1)
                    {
                        retVal[nameValuePair[0]] = null;
                    }
                    else if (nameValuePair.Length > 2)
                    {
                        throw new PIDLConfigException(
                            string.Format(
                                "Config file contains a string \"{0}\".  This has a substring \"{1}\" which has {2} \"{3}\" delimiters.  Only 0 or 1 occurence of this delimiter is allowed.",
                                configText,
                                element,
                                nameValuePair.Length - 1,
                                Constants.ConfigSpecialStrings.NameValueDelimiter),
                            Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                    }

                    // Bug 19723747: GetDictionaryFromConfigString does not handle the case when configText has 2 parts
                    // Found this bug while trying to improve test coverage.  It doesn't seem to affect production.  I think that the intent here was to
                    // have the below block of code which somehow was missing.  I am keeping the below code as comments for now as we dont want to change
                    // production behavior without fully understanding this (and reviewing all of our CSVs to see how this bug was not affecting production).
                    // else if (nameValuePair.Length == 2)
                    // {
                    //     retVal[nameValuePair[0]] = nameValuePair[1];
                    // }
                }
            }

            return retVal;
        }

        public static bool IsCountryValid(string country)
        {
            try
            {
                ValidateCountry(country);
            }
            catch (PIDLArgumentException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adds the needed linkedPidls to given list of resources
        /// </summary>
        /// <param name="pidlResource">The resource list to add the linked pidls to</param>
        /// <param name="linkedPidl">Pidl to be linked to given resource</param>
        /// <param name="partner">The partner of the request</param>
        /// <param name="submitOrder">When the linked pidl will be submitted relative to the base pidl</param>
        /// <param name="pidlIndex">Index of linked pidl. For India tax id, there are more than one linked pidl</param>
        public static void AddLinkedPidlToResourceList(
            List<PIDLResource> pidlResource,
            PIDLResource linkedPidl,
            string partner,
            PidlContainerDisplayHint.SubmissionOrder submitOrder = PidlContainerDisplayHint.DefaultSubmitOrder,
            int pidlIndex = 0)
        {
            foreach (PIDLResource pidl in pidlResource)
            {
                AddLinkedPidlToResource(pidl, linkedPidl, partner, submitOrder, pidlIndex);
            }
        }

        /// <summary>
        /// Adds the needed linkedPidls to given resource
        /// </summary>
        /// <param name="pidl">The pidl to add the linked pidls to</param>
        /// <param name="linkedPidl">Pidl to be linked to given resource</param>
        /// <param name="partner">The partner of the request</param>
        /// <param name="submitOrder">When the linked pidl will be submitted relative to the base pidl</param>
        /// <param name="pidlIndex">Index of linked pidl</param>
        public static void AddLinkedPidlToResource(
            PIDLResource pidl,
            PIDLResource linkedPidl,
            string partner,
            PidlContainerDisplayHint.SubmissionOrder submitOrder = PidlContainerDisplayHint.DefaultSubmitOrder,
            int pidlIndex = 0)
        {
            DisplayHint displayHint = pidl.GetFirstEmptyPidlContainer();
            PidlContainerDisplayHint pidlContainerHint = displayHint as PidlContainerDisplayHint;

            if (pidlContainerHint != null && linkedPidl != null)
            {
                pidl.AddLinkedPidl(linkedPidl);

                pidlContainerHint.SetLinkedPidlIdentity(linkedPidl.Identity);
                pidlContainerHint.HintId += pidlIndex;
                pidlContainerHint.IsMultiPage = false;
                pidlContainerHint.SubmitOrder = submitOrder;

                if (linkedPidl.Identity.ContainsKey(Constants.DescriptionIdentityFields.Type) && IsSplitPageTaxIdCollectionEnabled(partner, linkedPidl))
                {
                    ContainerDisplayHint cpfPage = pidl.GetDisplayHintOrPageById("profilePrerequisitesPage4") as ContainerDisplayHint;
                    ContainerDisplayHint emailPage = pidl.GetDisplayHintOrPageById("profilePrerequisitesPage3") as ContainerDisplayHint;
                    DisplayHint microsoftPrivacyTextGroup = pidl.GetDisplayHintOrPageById("microsoftPrivacyTextGroup");
                    DisplayHint previousSaveNextPrivacyStatementGroup = pidl.GetDisplayHintOrPageById("previousSaveNextPrivacyStatementGroup");
                    DisplayHint previousNextGroup = pidl.GetDisplayHintOrPageById("previousNextGroup");
                    if (cpfPage != null
                        && emailPage != null
                        && microsoftPrivacyTextGroup != null
                        && previousSaveNextPrivacyStatementGroup != null
                        && previousNextGroup != null)
                    {
                        // In the email page: remove microsoftPrivacyTextGroup
                        // and replace previousSaveNextPrivacyStatementGroup with previousNextGroup
                        // so that when the user clicks the next button, the CPF page can show up.
                        emailPage.Members.RemoveAt(emailPage.Members.Count - 2);
                        emailPage.Members[emailPage.Members.Count - 1] = previousNextGroup;

                        // In the cpf page: add microsoftPrivacyTextGroup and previousSaveNextPrivacyStatementGroup
                        // both of them need be shown up in the last page of prerequisite flow.
                        cpfPage.Members.Add(microsoftPrivacyTextGroup);
                        cpfPage.Members.Add(previousSaveNextPrivacyStatementGroup);
                    }
                }
            }
        }

        /// <summary>
        /// Changes noProfileAddress' isHidden property to false to given resource
        /// </summary>
        /// <param name="pidlResource">The resource to change the property to</param>
        public static void ShowNoProfileAddressToResource(List<PIDLResource> pidlResource)
        {
            foreach (PIDLResource pidl in pidlResource)
            {
                var noProfileAddressText = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<TextDisplayHint>(Constants.DisplayHintIds.NoProfileAddressText, pidl.DisplayPages);
                if (noProfileAddressText != null)
                {
                    noProfileAddressText.IsHidden = false;
                }
            }
        }

        /// <summary>
        /// Removes empty pidl containers (where linkedPidlId attribute is null) from given pidlResource
        /// </summary>
        /// <param name="pidlResource">The resource to remove empty pidl containers from</param>
        public static void RemoveEmptyPidlContainerHints(List<PIDLResource> pidlResource)
        {
            if (pidlResource != null)
            {
                foreach (PIDLResource pidl in pidlResource)
                {
                    pidl.RemoveEmptyPidlContainerHints();
                }
            }
        }

        /// <summary>
        /// Update userid in the submit url with real customer id for given profile pidlResource
        /// </summary>
        /// <param name="pidlResource">The profile pidlResource</param>
        /// <param name="customerId">The profile customer id</param>
        public static void UpdateProfilePidlSubmitUrl(PIDLResource pidlResource, string customerId)
        {
            ButtonDisplayHint submitButton = pidlResource.GetDisplayHintById(Constants.ButtonDisplayHintIds.SaveButtonHidden) as ButtonDisplayHint;
            if (submitButton?.Action?.Context != null)
            {
                RestLink actionContext = submitButton.Action.Context as RestLink;
                if (actionContext != null)
                {
                    actionContext.Href = actionContext.Href.Replace("{userId}", customerId);
                }
            }
        }

        /// <summary>
        /// Adds the secondary submit context for updating profile after address submit succeeds
        /// </summary>
        /// <param name="pidlResource">Address resource</param>
        /// <param name="profile">Profile payload to add to address pidl</param>
        /// <param name="partnerName">The name of partner is used to decide which version to use</param>
        /// <param name="country">Country of the user</param>
        /// <param name="setting">Payment experience setting</param>
        public static void AddSecondarySubmitAddressContext(List<PIDLResource> pidlResource, AccountProfile profile, string partnerName, string country = null, PaymentExperienceSetting setting = null)
        {
            if (profile != null)
            {
                profile.DefaultAddressId = "{id}";
            }

            Dictionary<string, RestLink> links = GetProfileSubmitLink(partnerName, profile == null ? null : profile.ProfileType, profile == null ? null : profile.Id, country: country, setting: setting);
            foreach (RestLink context in links.Values)
            {
                context.Payload = profile;
            }

            AddSubmitLinks(links, pidlResource, true);
        }

        /// <summary>
        /// Adds the secondary submit context for updating profile after address submit succeeds
        /// </summary>
        /// <param name="pidlResource">Address resource</param>
        /// <param name="profile">Profile payload to add to address pidl</param>
        /// <param name="partnerName">The name of partner is used to decide which version to use</param>
        /// <param name="profileV3Headers">Headers for v3 profile update</param>
        /// <param name="country">Country of the user</param>
        /// <param name="setting">Payment experience setting</param>
        public static void AddSecondarySubmitAddressV3Context(List<PIDLResource> pidlResource, AccountProfileV3 profile, string partnerName, Dictionary<string, string> profileV3Headers, string country = null, PaymentExperienceSetting setting = null)
        {
            if (profile != null)
            {
                profile.DefaultAddressId = "{id}";
            }

            Dictionary<string, RestLink> links = GetProfileSubmitLink(partnerName, profile == null ? null : profile.ProfileType, profile == null ? null : profile.Id, profileV3Headers, true, country: country, setting: setting);
            foreach (RestLink context in links.Values)
            {
                context.Payload = profile;
            }

            AddSubmitLinks(links, pidlResource, true);
        }

        /// <summary>
        /// Update CancelButton Context in Address Description when PI is successfully added
        /// </summary>
        /// <param name="pidlResources">The pidl Resources need to be modified</param>
        /// <param name="paymentInstrumentId">The id of payment instrument which is added successfully</param>
        /// <param name="partner">Calling partner</param>
        public static void UpdateCancelAddressContextAfterPIAdded(List<PIDLResource> pidlResources, string paymentInstrumentId, string partner)
        {
            foreach (var pidlResource in pidlResources)
            {
                // There might be some pidl resources that might not have display pages.
                if (pidlResource.DisplayPages == null)
                {
                    continue;
                }

                ButtonDisplayHint displayHintButton = null;
                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
                {
                    displayHintButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint, GroupDisplayHint>(Constants.ButtonDisplayHintIds.CancelButton, Constants.DisplayHintIds.SaveCancelGroup, pidlResource.DisplayPages);
                }
                else
                {
                    displayHintButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint, GroupDisplayHint>(Constants.ButtonDisplayHintIds.CancelButton, Constants.DisplayHintIds.CancelSaveGroup, pidlResource.DisplayPages);
                    if (displayHintButton == null)
                    {
                        displayHintButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint, GroupDisplayHint>(Constants.ButtonDisplayHintIds.CancelButton, Constants.DisplayHintIds.CancelAddGroup, pidlResource.DisplayPages);
                    }
                }

                if (displayHintButton != null)
                {
                    if (displayHintButton.Action != null)
                    {
                        displayHintButton.Action.Context = new { id = paymentInstrumentId };
                        displayHintButton.Action.ActionType = "success";
                    }
                }
            }
        }

        /// <summary>
        /// Adds a pidlAction to pidl
        /// </summary>
        /// <param name="pidlResources">The resource to change the property to</param>
        /// <param name="type">Specifies type of profile</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="partner">The name of the partner</param>
        /// <param name="setting">Payment experience setting</param>
        public static void AddPidlActionToEditProfileButton(List<PIDLResource> pidlResources, string type, string language, string country, string partner, PaymentExperienceSetting setting = null)
        {
            if (string.Equals(type, Constants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase) || string.Equals(type, Constants.ProfileTypes.Legal, StringComparison.OrdinalIgnoreCase))
            {
                AddPartnerActionToDisplayHint<HyperlinkDisplayHint>(
                    pidlResources: pidlResources,
                    displayHintId: Constants.DisplayHintIds.ProfileEditLEHyperlinkId,
                    actionType: PIActionType.UpdateResource,
                    resourceType: Constants.DescriptionTypes.ProfileDescription,
                    country: country,
                    language: language,
                    partner: partner,
                    type: type);
            }
        }

        /// <summary>
        /// Adds a pidlAction to PossibleOptions in pidl
        /// Adds a currency text to stored value PossibleOption
        /// </summary>
        /// <param name="pidlResources">The resource to change the property to</param>
        /// <param name="displayHintId">The displayHint of the possibleOptions</param>
        /// <param name="country">The country</param>
        /// <param name="language">The language</param>
        /// <param name="paymentInstrumentList">The list of paymentInstruments</param>
        public static void ProcessPossibleOptions(List<PIDLResource> pidlResources, string displayHintId, string country, string language, List<PaymentInstrument> paymentInstrumentList = null)
        {
            foreach (var pidlResource in pidlResources)
            {
                var paymentInstrumentListPi = pidlResource.GetDisplayHintById(displayHintId) as PropertyDisplayHint;
                foreach (SelectOptionDescription possibleOption in paymentInstrumentListPi?.PossibleOptions.Values)
                {
                    var paymentInstrumentItemGroup = possibleOption?.DisplayContent?.Members?.FirstOrDefault() as GroupDisplayHint;
                    foreach (var member in paymentInstrumentItemGroup?.Members)
                    {
                        GroupDisplayHint memberGroup = member as GroupDisplayHint;
                        if (memberGroup != null)
                        {
                            var actionTypeName = DisplayHintActionType.success.ToString();
                            if (string.Equals(Constants.GroupDisplayHintIds.PaymentInstrumentItemExpiredCCGroup, memberGroup.HintId, StringComparison.OrdinalIgnoreCase))
                            {
                                actionTypeName = PaymentInstrumentActions.ToString(PIActionType.UpdateResource);
                            }

                            if (string.Equals(Constants.GroupDisplayHintIds.PaymentInstrumentItemStoredValueGroup, memberGroup.HintId, StringComparison.OrdinalIgnoreCase) &&
                                paymentInstrumentList != null)
                            {
                                PaymentInstrument storedValue = PaymentSelectionHelper.GetStoredValuePI(paymentInstrumentList);
                                GroupDisplayHint storedValueInfoGroup = memberGroup?.Members[1] as GroupDisplayHint;
                                TextGroupDisplayHint storedValueTextGroup = storedValueInfoGroup?.Members[0] as TextGroupDisplayHint;
                                PaymentSelectionHelper.UpdateStoredValuePIDisplayHintForCart(storedValue, storedValueTextGroup, country, language);
                            }

                            ActionContext context = new ActionContext()
                            {
                                Id = "({contextData.id})",
                                Instance = "({contextData})",
                                Action = actionTypeName,
                            };

                            memberGroup.Action = new DisplayHintAction(DisplayHintActionType.success.ToString(), false, context, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a pidlAction to a displayHint in pidl
        /// </summary>
        /// <typeparam name="T"> generic type</typeparam>
        /// <param name="pidlResources">The resource to change the property to</param>
        /// <param name="displayHintId">The displayHint to add the pidlAction to</param>
        /// <param name="actionType">Action type in the context of pidlAction</param>
        public static void AddPidlActionToDisplayHint<T>(
           List<PIDLResource> pidlResources,
           string displayHintId,
           PIActionType actionType) where T : DisplayHint
        {
            foreach (var pidlResource in pidlResources)
            {
                // There might be some pidl resources that might not have display pages.
                if (pidlResource.DisplayPages != null)
                {
                    var displayHint = pidlResource.GetDisplayHintById(displayHintId) as T;
                    if (displayHint != null)
                    {
                        var actionTypeName = PaymentInstrumentActions.ToString(actionType);
                        ActionContext context = new ActionContext()
                        {
                            Action = actionTypeName,
                        };

                        displayHint.Action = new DisplayHintAction(DisplayHintActionType.success.ToString(), false, context, null);
                    }
                }
            }
        }

        /// <summary>
        /// Remove data descriptions from PIDL
        /// This does not take care of any display that may be referring to the data description being removed (caller is responsible to ensure such issues dont occur)
        /// </summary>
        /// <param name="pidlResource">The resource to remove the property from</param>
        /// <param name="path">The path to removed properties</param>
        /// <param name="propertiesToRemove">Name of properties to remove</param>
        /// <param name="descriptionType">Description Type (prefix) of properties to remove</param>
        public static void RemoveDataDescriptionWithFullPath(PIDLResource pidlResource, string path, string[] propertiesToRemove, string descriptionType = null)
        {
            foreach (string propertyName in propertiesToRemove)
            {
                pidlResource.RemoveDataDescription(path, propertyName, descriptionType);
            }
        }

        /// <summary>
        /// Given a pi id and sessionId, returns the PIDL to redirect the user to complete
        /// an action required to complete the purchase flow.
        /// This funtion is used for creating the client action to redirect the user to disclaimer page.
        /// </summary>
        /// <param name="pi">Payment instrument where action needs to be taken</param>
        /// <param name="sessionId">Identity of the user's purchase session</param>
        /// <param name="exposedFlightFeatures">All flighting features exposed from the current flight context</param>
        /// <returns>Returns PIDL with client action</returns>
        public static List<PIDLResource> GetChallengePidlsForSession(PaymentInstrument pi, string sessionId, List<string> exposedFlightFeatures = null)
        {
            PIDLResource retVal = new PIDLResource();
            ClientAction clientAction;

            string redirectUrl = Constants.RedirectUrls.RedirectTemplate;
            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableRedirectionV2Url, StringComparer.OrdinalIgnoreCase))
            {
                redirectUrl = Constants.RedirectUrls.RedirectTemplateV2;
            }

            RedirectionServiceLink dpaRedirectLink = new RedirectionServiceLink
            {
                BaseUrl = string.Format(redirectUrl, sessionId)
            };

            dpaRedirectLink.RuParameters.Add("id", pi.PaymentInstrumentId);
            dpaRedirectLink.RuParameters.Add("family", pi.PaymentMethod.PaymentMethodFamily);
            dpaRedirectLink.RuParameters.Add("type", pi.PaymentMethod.PaymentMethodType);

            clientAction = new ClientAction(ClientActionType.Redirect);
            clientAction.Context = dpaRedirectLink;

            retVal.ClientAction = clientAction;
            List<PIDLResource> retList = new List<PIDLResource>();
            retList.Add(retVal);
            return retList;
        }

        public static PIDLResource GetRedirectPidl(string redirectionUrl, bool noCallbackParams = false)
        {
            ClientAction action;

            RedirectionServiceLink redirectLink = new RedirectionServiceLink
            {
                BaseUrl = redirectionUrl,
                NoCallbackParams = noCallbackParams
            };

            action = new ClientAction(ClientActionType.Redirect)
            {
                Context = redirectLink
            };

            return new PIDLResource
            {
                ClientAction = action,
            };
        }

        /// <summary>
        /// Given a pi id and sessionId, returns the PIDL to render a QR code to redirect the user to complete
        /// an action required to complete the purchase flow.
        /// </summary>
        /// <param name="pi">Payment instrument where action needs to be taken</param>
        /// <param name="sessionId">Identity of the user's purchase session</param>
        /// <param name="exposedFlightFeatures">All flighting features exposed from the current flight context</param>
        /// <param name="partner">The name of the partner</param>
        /// <param name="language">Code specifying the language for PIDL localization</param>
        /// <param name="orderId">Identity of user's order</param>
        /// <param name="shortUrl">short url generated for redirect</param>
        /// <param name="setting">Payment experience setting</param>
        /// <returns>Returns PIDL with client action</returns>
        public static List<PIDLResource> GetPurchaseConfirmationPidl(PaymentInstrument pi, string sessionId, List<string> exposedFlightFeatures = null, string partner = null, string language = null, string orderId = null, string shortUrl = null, PaymentExperienceSetting setting = null)
        {
            PIDLResource retVal = new PIDLResource();
            ClientAction clientAction;

            clientAction = new ClientAction(ClientActionType.Pidl);
            clientAction.Context = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForPI(pi, language, Constants.ChallengeDescriptionTypes.GlobalPIQrCode, partner, null, null, null, false, null, exposedFlightFeatures, sessionId, null, orderId, shortUrl, setting: setting);

            retVal.ClientAction = clientAction;
            List<PIDLResource> retList = new List<PIDLResource>();
            retList.Add(retVal);
            return retList;
        }

        public static List<PIDLResource> GetPaymentSessionPidl(object paymentSession)
        {
            PIDLResource pidlResource = new PIDLResource
            {
                ClientAction = new ClientAction(ClientActionType.ReturnContext, paymentSession)
            };

            List<PIDLResource> retList = new List<PIDLResource> { pidlResource };
            return retList;
        }

        public static Dictionary<string, string> GetCopiedDictionaryFromDomainDictionaries(string key)
        {
            Dictionary<string, string> copiedDictionary = null;

            if (PIDLResourceFactory.domainDictionaries.ContainsKey(key))
            {
                copiedDictionary = new Dictionary<string, string>(PIDLResourceFactory.domainDictionaries[key]);
            }

            return copiedDictionary;
        }

        /// <summary>
        /// Get the payment select description object for given partner's list payment methods or list payment instruments page
        /// </summary>
        /// <param name="paymentMethods">Set of payment methods to include in PIDL</param>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="operation">This is the operation type for which the pidl is requested, select for PM, selectInstance for PI</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="allowedPaymentMethods">Inclusion list from client</param>
        /// <param name="defaultPaymentMethod">PaymentMethod to display first, when applicable</param>
        /// <param name="filters">Filters to be applied to payment methods</param>
        /// <param name="paymentInstruments">Set of payment instruments for List PI</param>
        /// <param name="disabledPaymentInstruments">Set of payment instruments which should be shown to user but disabled</param>
        /// <param name="exposedFlightFeatures">All flighting features exposed from the current flight context</param>
        /// <param name="scenario">This is the scenario for which the pidl is requested</param>
        /// <param name="setting">This is the setting used for PIDL generation and feature enablement</param>
        /// <returns>Returns a dictionary with the key is the id of the PaymentMethodDescription</returns>
        public static List<PIDLResource> GetPaymentSelectDescriptions(
            HashSet<PaymentMethod> paymentMethods,
            string country,
            string operation,
            string language,
            string partnerName,
            string allowedPaymentMethods = null,
            string defaultPaymentMethod = null,
            string filters = null,
            List<PaymentInstrument> paymentInstruments = null,
            List<PaymentInstrument> disabledPaymentInstruments = null,
            List<string> exposedFlightFeatures = null,
            string scenario = null,
            PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);
            operation = Helper.TryToLower(operation);

            // Validate input parameters
            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            List<PIDLResource> retList = new List<PIDLResource>();
            HashSet<PaymentMethod> filteredPaymentMethods = PaymentSelectionHelper.GetFilteredPaymentMethods(paymentMethods, allowedPaymentMethods, filters, operation, partnerName, country, setting);

            if (operation.Equals(Constants.PidlOperationTypes.Select))
            {
                retList = PaymentSelectionHelper.GetPaymentMethodSelectPidls(partnerName, country, language, filteredPaymentMethods, defaultPaymentMethod, exposedFlightFeatures, scenario, setting);
            }
            else if (operation.Equals(Constants.PidlOperationTypes.ValidateInstance))
            {
                List<PaymentInstrument> filteredPaymentInstruments = PaymentSelectionHelper.GetFilteredPaymentInstrumentsForChallenge(paymentInstruments, filteredPaymentMethods, allowedPaymentMethods, filters, country, partnerName, setting: setting);
                retList = PaymentSelectionHelper.GetValidateInstancePidls(filteredPaymentInstruments, filters, partnerName, country, language);
            }
            else if ((Constants.PartnersEnabledSinglePiDisplay.Contains(partnerName, StringComparer.OrdinalIgnoreCase)
                    || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableSelectSingleInstancePiDisplay, country, setting))
                && operation.Equals(Constants.PidlOperationTypes.SelectSingleInstance))
            {
                retList = PaymentSelectionHelper.GetSinglePiDisplayPidl(partnerName, country, language, paymentInstruments, disabledPaymentInstruments, filters, setting: setting);
            }
            else
            {
                List<PaymentInstrument> filteredPaymentInstruments = PaymentSelectionHelper.GetFilteredPaymentInstruments(paymentInstruments, disabledPaymentInstruments, filteredPaymentMethods, allowedPaymentMethods, filters, partnerName, country, setting: setting);
                retList = PaymentSelectionHelper.GetPaymentInstrumentSelectPidls(partnerName, country, language, filteredPaymentInstruments, disabledPaymentInstruments, filteredPaymentMethods, filters, exposedFlightFeatures, scenario, setting: setting);
                if (string.Equals(partnerName, PXCommon.Constants.PartnerNames.XboxSettings, StringComparison.OrdinalIgnoreCase))
                {
                    retList?.First()?.AddDataSource(Constants.RestResourceNames.PaymentInstruments, new DataSource(filteredPaymentInstruments?.Cast<object>()?.ToList()));
                }
            }

            // TODO (48097281): Post-process pidl here to add the username
            return retList;
        }

        /// <summary>
        /// Get the payment instrument list description object
        /// </summary>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="scenario">The name of the scenario</param>
        /// <param name="classicProduct">classicProduct needs to be passed down to PIFD / PIMS</param>
        /// <param name="billableAccountId">billableAccountId of the user needs to be passed down to PIFD / PIMS</param>
        /// <param name="exposedFlightFeatures">exposedFlightFeatures need to be passed to handle flighting of the DisplaySequences</param>
        /// <param name="setting">setting from partner setting service to generate PIDL and enable feture</param>
        /// <returns>Returns a dictionary with the key is the id of the PaymentInstrumentDescription</returns>
        public static List<PIDLResource> GetPaymentInsturmentSelectDescriptions(
            string country,
            string language,
            string partnerName,
            string scenario,
            string classicProduct = null,
            string billableAccountId = null,
            List<string> exposedFlightFeatures = null,
            PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);

            // Validate input parameters
            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);
            Context.PartnerName = partnerName;

            List<PIDLResource> retList = PaymentSelectionHelper.GetPaymentInstrumentListPidls(partnerName, country, scenario, language, classicProduct, billableAccountId, exposedFlightFeatures, setting: setting);

            if (IsNorthStarWebPartner(partnerName))
            {
                UpdateListPIActionsForNorthStarWeb(retList, country, language);
            }
            else if (PXCommon.Constants.PartnerGroups.IsAzureBasedPartner(partnerName)
                || IsOneDrivePartner(partnerName)
                || IsPayinPartner(partnerName)
                || IsSetupOfficePartner(partnerName)
                || IsStoreOfficePartner(partnerName)
                || (IsCommercialStorePartner(partnerName) && (string.Equals(scenario, Constants.ScenarioNames.DepartmentalPurchase, StringComparison.OrdinalIgnoreCase) || string.Equals(scenario, Constants.ScenarioNames.EligiblePI, StringComparison.OrdinalIgnoreCase)))
                || TemplateHelper.IsListPiTemplate(setting))
            {
                AddPartnerActionToDisplayHint<ButtonDisplayHint>(
                    pidlResources: retList,
                    displayHintId: Constants.DisplayHintIds.NewPaymentMethodLink,
                    actionType: PIActionType.SelectResourceType,
                    resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                    country: country,
                    language: language,
                    partner: partnerName,
                    exposedFlightFeatures: exposedFlightFeatures);
            }
            else if (IsGGPDEDSPartner(partnerName))
            {
                AddPartnerActionToDisplayHint<ButtonDisplayHint>(
                    pidlResources: retList,
                    displayHintId: Constants.DisplayHintIds.NewPaymentMethodLink,
                    actionType: PIActionType.AddResource,
                    resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                    country: country,
                    language: language,
                    family: Constants.PaymentMethodFamilyNames.CreditCard,
                    partner: partnerName);

                AddPartnerActionToDisplayHint<ButtonDisplayHint>(
                    pidlResources: retList,
                    displayHintId: Constants.DisplayHintIds.EditPaymentMethodLink,
                    actionType: PIActionType.UpdateResource,
                    resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                    country: country,
                    language: language,
                    partner: partnerName);

                AddPartnerActionToDisplayHint<ButtonDisplayHint>(
                    pidlResources: retList,
                    displayHintId: Constants.DisplayHintIds.SelectResourceNextLink,
                    actionType: PIActionType.SelectResource,
                    resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                    country: country,
                    language: language,
                    partner: partnerName);
            }

            return retList;
        }

        /// <summary>
        /// Get the payment instrument search transactions description object
        /// </summary>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="paymentInstruments">Payment Instrument list from PIMS</param>
        /// <param name="flightNames">Flightnames need to be passed to handle flighting of the DisplaySequences</param>
        /// <param name="setting">setting from partner setting service to generate PIDL and enable feture</param>
        /// <returns>Returns a list of PIDL resource of the PaymentInstrumentSearchTransactionsDescription</returns>
        public static List<PIDLResource> GetPaymentInsturmentSearchTransactionsDescriptions(
            string country,
            string language,
            string partnerName,
            List<PaymentInstrument> paymentInstruments,
            List<string> flightNames = null,
            PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);

            // Validate input parameters
            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);
            Context.PartnerName = partnerName;

            List<PIDLResource> retList = PaymentSelectionHelper.GetPaymentInstrumentSearchTransactionsPidls(partnerName, country, language, paymentInstruments, flightNames: flightNames, setting: setting);

            AddPartnerActionToDisplayHint<HyperlinkDisplayHint>(
                    pidlResources: retList,
                    displayHintId: Constants.DisplayHintIds.NewPaymentStatementLink,
                    actionType: PIActionType.AddResource,
                    resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                    country: country,
                    language: language,
                    family: Constants.PaymentMethodFamilyNames.CreditCard,
                    partner: partnerName);

            return retList;
        }

        /// <summary>
        /// Get the payment instrument show description
        /// </summary>
        /// <param name="family">Specifies payment method family</param>
        /// <param name="type">Specifies payment method type</param>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="setting">Payment experience setting</param>
        /// <returns>Returns show PI pidl</returns>
        public static List<PIDLResource> GetPaymentMethodShowDescriptions(
            string family,
            string type,
            string country,
            string language,
            string partnerName,
            PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);

            // Validate input parameters
            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            List<PIDLResource> retList = PaymentSelectionHelper.GetPaymentMethodShowOrSearchPidls(family, type, partnerName, Constants.PidlOperationTypes.Show, country, language, setting: setting);

            AddPartnerActionToDisplayHint<HyperlinkDisplayHint>(
                pidlResources: retList,
                displayHintId: Constants.DisplayHintIds.PaymentInstrumentShowPIChangeLink,
                actionType: PIActionType.SelectResource,
                resourceType: Constants.DescriptionTypes.PaymentMethodDescription,
                country: country,
                language: language,
                partner: partnerName);

            return retList;
        }

        /// <summary>
        /// Get the payment instrument fundStoredValue description for selection stage
        /// </summary>
        /// <param name="family">Specifies payment method family</param>
        /// <param name="type">Specifies payment method type</param>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="piid">ID of PI to redeem against for funding stored value</param>
        /// <param name="fundingOptions">Options for the funding amount selection pidl</param>
        /// <param name="setting">setting from partner setting service</param>
        /// <returns>Returns fundStoredValue with PI pidl</returns>
        public static List<PIDLResource> GetPaymentMethodFundStoredValueSelectDescriptions(
            string family,
            string type,
            string country,
            string language,
            string partnerName,
            string piid,
            Dictionary<string, string> fundingOptions,
            PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);

            // Validate input parameters
            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            List<PIDLResource> retList = PaymentSelectionHelper.GetPaymentMethodFundStoredValuePidls(family, type, country, language, partnerName, Constants.PidlOperationTypes.FundStoredValue, setting: setting);

            if (string.Equals(family, Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(type, Constants.PaymentMethodTypeNames.Bitcoin, StringComparison.InvariantCultureIgnoreCase) &&
                retList != null && retList.Count == 1 && retList[0].DisplayPages.Count == 1)
            {
                var links = GetBitcoinRedeemLink(piid, country, language, partnerName);
                AddSubmitLinks(links, retList);

                var bitcoinRedeemAmountPropertyDataDescription = retList[0].DataDescription[Constants.DataDescriptionIds.Amount] as PropertyDescription;
                var bitcoinRedeemAmountPropertyDisplayHint = retList[0].GetDisplayHintById(Constants.DisplayHintIds.FundStoredValueWithBitcoinRedeemAmountProperty) as PropertyDisplayHint;
                if (fundingOptions != null && bitcoinRedeemAmountPropertyDisplayHint != null && bitcoinRedeemAmountPropertyDataDescription != null)
                {
                    var fundingValues = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, string> fundingOption in fundingOptions)
                    {
                        fundingValues.Add(fundingOption.Key, fundingOption.Key);
                    }

                    bitcoinRedeemAmountPropertyDataDescription.UpdatePossibleValues(fundingValues);
                    bitcoinRedeemAmountPropertyDisplayHint.SetPossibleOptions(fundingOptions);
                }
            }
            else if (string.Equals(family, Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.Equals(type, Constants.PaymentMethodTypeNames.StoredValue, StringComparison.InvariantCultureIgnoreCase))
                {
                    var links = GetCsvVerifyLink();
                    AddSubmitLinks(links, retList);
                }
                else if (string.Equals(type, Constants.PaymentMethodTypeNames.StoredValueRedeem, StringComparison.InvariantCultureIgnoreCase))
                {
                    var links = GetCsvRedeemLink();
                    AddSubmitLinks(links, retList);
                }
            }

            return retList;
        }

        /// <summary>
        /// Get the payment instrument fundStoredValue description for redemption stage
        /// </summary>
        /// <param name="amount">Specifies amount being redeemed</param>
        /// <param name="family">Specifies payment method family</param>
        /// <param name="type">Specifies payment method type</param>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="currency">Specifies the currency being redeemed</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="piid">ID of PI to redeem against for funding stored value</param>
        /// <param name="redirectContentUrl">Url to be shown to user for redemption</param>
        /// <param name="referenceId">Id of the funding process being done</param>
        /// <param name="greenId">Id of the funding session</param>
        /// <param name="setting">setting from partner setting service</param>
        /// <returns>Returns fundStoredValue with PI pidl</returns>
        public static List<PIDLResource> GetPaymentMethodFundStoredValueRedeemDescriptions(
            string amount,
            string family,
            string type,
            string country,
            string currency,
            string language,
            string partnerName,
            string piid,
            string redirectContentUrl,
            string referenceId,
            string greenId,
            PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);

            // Validate input parameters
            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            List<PIDLResource> retList = PaymentSelectionHelper.GetPaymentMethodFundStoredValuePidls(family, type, country, language, partnerName, Constants.PidlOperationTypes.FundStoredValue, true, setting);

            if (string.Equals(family, Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(type, Constants.PaymentMethodTypeNames.Bitcoin, StringComparison.InvariantCultureIgnoreCase) &&
                retList != null && retList.Count == 1 && retList[0].DisplayPages.Count == 1)
            {
                IFrameDisplayHint bitpayIframe = new IFrameDisplayHint
                {
                    HintId = Constants.DisplayHintIds.BitpayIframe,
                    SourceUrl = string.Format("{0}&view=iframe&lang={1}", redirectContentUrl, language),
                };

                var links = GetBitcoinRedeemLink(piid, country, language, partnerName, amount, currency, referenceId, greenId);
                string pollUrl = links[Constants.ButtonDisplayHintIds.SaveNextButton].Href;
                DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());
                PollActionContext pollActionContext = new PollActionContext()
                {
                    Href = pollUrl,
                    Method = Constants.HTTPVerbs.GET,
                    Interval = Constants.PollingIntervals.BitpayPollingInterval,
                    CheckPollingTimeOut = false,
                    ResponseResultExpression = Constants.PollingResponseResultExpression.BitcoinResponseResultExpression,
                };

                pollActionContext.AddResponseActionsItem(Constants.PollingResponseActionKey.BitcoinFundStoredValueSuccess, new DisplayHintAction(DisplayHintActionType.success.ToString()));
                pollAction.Context = pollActionContext;
                bitpayIframe.Action = pollAction;

                var redeemPage = retList[0].DisplayPages[0];
                if (redeemPage != null)
                {
                    redeemPage.AddDisplayHint(bitpayIframe);
                }
            }

            return retList;
        }

        /// <summary>
        /// Get the payment instrument show description
        /// </summary>
        /// <param name="family">Specifies payment method family</param>
        /// <param name="type">Specifies payment method type</param>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="setting">Payment experience setting</param>
        /// <returns>Returns show PI pidl</returns>
        public static List<PIDLResource> GetPaymentMethodSearchDescriptions(
            string family,
            string type,
            string country,
            string language,
            string partnerName,
            PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);

            // Validate input parameters
            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            List<PIDLResource> retList = PaymentSelectionHelper.GetPaymentMethodShowOrSearchPidls(family, type, partnerName, Constants.PidlOperationTypes.Search, country, language, setting: setting);

            return retList;
        }

        /// <summary>
        /// Get the billing group list description object
        /// </summary>
        /// <param name="type">Specifies billing group type</param>
        /// <param name="operation">Specifies operation</param>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="exposedFlightFeatures">All flighting features exposed from the current flight context</param>
        /// <param name="setting">Setting data from Partner Settings Service</param>
        /// <returns>Returns a dictionary with the key is the id of the BillingGroupDescription</returns>
        public static List<PIDLResource> GetBillingGroupListDescriptions(
            string type,
            string operation,
            string country,
            string language,
            string partnerName,
            List<string> exposedFlightFeatures = null,
            PaymentExperienceSetting setting = null)
        {
            type = Helper.TryToLower(type);
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);

            // Validate input parameters
            ValidateCountry(country);

            if (string.IsNullOrWhiteSpace(type))
            {
                type = Constants.BillingGroupTypeNames.LightWeight;
            }

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            List<PIDLResource> retList = new List<PIDLResource>();
            retList = PaymentSelectionHelper.GetBillingGroupListPidls(type, partnerName, country, language, exposedFlightFeatures, setting);

            bool isSelectOperation = string.Equals(operation, Constants.PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase);
            bool hasAddAndEditBillingGroupFeature = PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.AddPartnerActionToBillingGroupAddAndUpdate, country, setting);
            bool isCommercialStores = string.Equals(partnerName, Constants.PartnerNames.Commercialstores, StringComparison.OrdinalIgnoreCase);
            if (isSelectOperation && (hasAddAndEditBillingGroupFeature || isCommercialStores))
            {
                if (retList != null)
                {
                    // Add Pidl action to "add billing group" button
                    AddPartnerActionToDisplayHint<HyperlinkDisplayHint>(
                        pidlResources: retList,
                        displayHintId: Constants.DisplayHintIds.BillingGroupListAddBGHyperlinkId,
                        actionType: PIActionType.AddResource,
                        resourceType: Constants.DescriptionTypes.BillingGroupDescription,
                        language: language,
                        country: country,
                        partner: partnerName,
                        type: type);

                    // Add Pidl action to "Edit billing details" button
                    AddPartnerActionToDisplayHint<HyperlinkDisplayHint>(
                        pidlResources: retList,
                        displayHintId: Constants.DisplayHintIds.BillingGroupListEditBillingDetailsHyperlinkId,
                        actionType: PIActionType.UpdateResource,
                        resourceType: Constants.DescriptionTypes.BillingGroupDescription,
                        language: language,
                        country: country,
                        partner: partnerName,
                        type: type,
                        scenario: Constants.ScenarioNames.BillingGroupPONumber);
                }
            }

            return retList;
        }

        /// <summary>
        /// Get the address group select description object
        /// </summary>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="addressGroup">Set of address entities to include in PIDL</param>
        /// <returns>Returns a dictionary with the key is the id of the AddressGroupDescription</returns>
        public static List<PIDLResource> GetAddressGroupSelectDescriptions(
            string country,
            string language,
            string partnerName,
            CMResources<AddressInfo> addressGroup)
        {
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);

            // Validate input parameters
            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            List<PIDLResource> retList = new List<PIDLResource>();

            retList = PaymentSelectionHelper.GetAddressGroupSelectPidls(partnerName, country, language, addressGroup);

            return retList;
        }

        public static List<PIDLResource> GetAddressV3GroupSelectDescriptions(
            string type,
            string country,
            string language,
            string partnerName,
            CMResources<PXAddressV3Info> addressGroup,
            AccountProfileV3 profile,
            PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);

            // Validate input parameters
            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            return AddressSelectionHelper.GetAddressGroupSelectPidls(partnerName, country, language, type, addressGroup, profile, setting);
        }

        public static List<PIDLResource> GetThreeDSChallengeIFrameDescription(
            string acsChallengeURL,
            string creqData,
            string threeDSSessionData,
            string threeDSSessionId,
            string cspStep,
            string width,
            string height,
            string testHeader) => CreateThreeSDChallengeIFrameDescription(acsChallengeURL, creqData, threeDSSessionData, threeDSSessionId, cspStep, width, height, testHeader, asUrlIFrame: false);

        public static List<PIDLResource> GetThreeDSChallengeUrlIFrameDescription(
            string acsChallengeURL,
            string creqData,
            string threeDSSessionData,
            string threeDSSessionId,
            string cspStep,
            string width,
            string height,
            string testHeader,
            List<string> exposedFlightFeatures = null) => CreateThreeSDChallengeIFrameDescription(acsChallengeURL, creqData, threeDSSessionData, threeDSSessionId, cspStep, width, height, testHeader, asUrlIFrame: true, exposedFlightFeatures);

        public static List<PIDLResource> GetThreeDSOneChallengeIFrameDescription(
            string acsChallengeURL,
            string creqData,
            string threeDSSessionId,
            string language,
            string width,
            string height,
            string partner,
            string testHeader)
        {
            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.ThreeDSChallangeIFrameDescription }
            });

            PageDisplayHint page = new PageDisplayHint
            {
                HintId = Constants.DisplayHintIds.IFramePageId,
                DisplayName = Constants.DisplayHintIds.ThreeDSChallengePageName
            };

            string formData = string.Empty;
            var formDataParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(creqData);
            foreach (var key in formDataParams.Keys)
            {
                formData += string.Format(Constants.IFrameContentTemplates.PostThreeDSOneFormData, key, formDataParams[key]);
            }

            IFrameDisplayHint challengeIFrame = new IFrameDisplayHint
            {
                HintId = Constants.DisplayHintIds.ThreeDSChallengeIFrameId,
                DisplayContent = string.Format(Constants.IFrameContentTemplates.PostThreeDSOneSessionData, acsChallengeURL, formData),
            };

            challengeIFrame.AddDisplayTag("accessibilityName", LocalizationRepository.Instance.GetLocalizedString("The bank purchase authentication dialog", language));
            if (width != null)
            {
                challengeIFrame.Width = width;
            }

            if (height != null)
            {
                challengeIFrame.Height = height;
            }

            challengeIFrame.ExpectedClientActionId = threeDSSessionId;

            page.AddDisplayHint(challengeIFrame);

            // add poll action to query session state
            page.Action = Build3DSOnePollingAction(threeDSSessionId, partner);

            retVal.AddDisplayPages(new List<PageDisplayHint> { page });

            List<PIDLResource> retList = new List<PIDLResource> { retVal };
            AddTestHeaderAsIFrameFormInput(retList, Constants.DisplayHintIds.ThreeDSChallengeIFrameId, testHeader);
            return retList;
        }

        public static string ComposeHtmlAuthenticateRedirectionThreeDSOne(string acsChallengeURL, string creqData)
        {
            string formData = string.Empty;
            var formDataParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(creqData);
            foreach (var key in formDataParams.Keys)
            {
                formData += string.Format(Constants.IFrameContentTemplates.PostThreeDSOneFormData, key, formDataParams[key]);
            }

            return string.Format(Constants.IFrameContentTemplates.PostThreeDSOneSessionData, acsChallengeURL, formData);
        }

        public static string ComposeHtmlNotifyThreeDSOneChallengeCompleted(string redirectionUrl)
        {
            return string.Format(Constants.IFrameContentTemplates.ThreeDSOneRedirect, redirectionUrl);
        }

        // for CSP 3DS2 fingerprint
        public static string ComposeHtmlCSPThreeDSFingerprintIFrameContent(
            string threeDSMethodURL,
            string threeDSMethodData)
        {
            return string.Format(Constants.IFrameContentTemplates.CSPPostThreeDSMethodData, threeDSMethodURL, threeDSMethodData);
        }

        // for CSP 3DS2 fingerprint with src parameter
        public static string ComposeHtmlCSPThreeDSFingerprintSrcIFrameContent(
            string threeDSMethodURL,
            string threeDSMethodData)
        {
            return string.Format(Constants.IFrameContentTemplates.CSPPostThreeDSMethodDataSrc, threeDSMethodURL, threeDSMethodData);
        }

        // for CSP 3DS2 Challenge
        public static string ComposeHtmlCSPThreeDSChallengeIFrameDescription(
            string acsChallengeURL,
            string creqData,
            string threeDSSessionData)
        {
            return string.Format(Constants.IFrameContentTemplates.CSPPostThreeDSSessionData, acsChallengeURL, creqData, threeDSSessionData);
        }

        // for CSP 3DS2 Challenge with src parameter
        public static string ComposeHtmlCSPThreeDSChallengeSrcIFrameDescription(
            string acsChallengeURL,
            string creqData,
            string threeDSSessionData)
        {
            return string.Format(Constants.IFrameContentTemplates.CSPPostThreeDSSessionDataSrc, acsChallengeURL, creqData, threeDSSessionData);
        }

        public static ClientAction GetThreeDSChallengeIFrameClientAction(
            string acsChallengeURL,
            string creqData,
            string threeDSSessionData,
            string threeDSSessionId,
            string cspStep,
            string width = null,
            string height = null,
            string testHeader = null)
        {
            List<PIDLResource> retList = GetThreeDSChallengeIFrameDescription(acsChallengeURL, creqData, threeDSSessionData, threeDSSessionId, cspStep, width, height, testHeader);
            ClientAction clientAction = new ClientAction(ClientActionType.Pidl, retList);
            return clientAction;
        }

        public static ClientAction GetThreeDSChallengeUrlIFrameClientAction(
            string acsChallengeURL,
            string creqData,
            string threeDSSessionData,
            string threeDSSessionId,
            string cspStep,
            string width = null,
            string height = null,
            string testHeader = null,
            List<string> exposedFlightFeatures = null)
        {
            List<PIDLResource> retList = GetThreeDSChallengeUrlIFrameDescription(acsChallengeURL, creqData, threeDSSessionData, threeDSSessionId, cspStep, width, height, testHeader, exposedFlightFeatures);
            ClientAction clientAction = new ClientAction(ClientActionType.Pidl, retList);
            return clientAction;
        }

        public static ClientAction GetThreeDSOneChallengeIFrameClientAction(
            string acsChallengeURL,
            string creqData,
            string threeDSSessionId,
            string language,
            string width = null,
            string height = null,
            string partner = null,
            string testHeader = null)
        {
            List<PIDLResource> retList = GetThreeDSOneChallengeIFrameDescription(
                acsChallengeURL,
                creqData,
                threeDSSessionId,
                language,
                width,
                height,
                partner,
                testHeader);
            ClientAction clientAction = new ClientAction(ClientActionType.Pidl, retList);
            return clientAction;
        }

        public static List<PIDLResource> GetChallengeRedirectAndStatusCheckDescriptionForCheckout(string checkoutId, string partnerName, string paymentProviderId, string redirectionUrl, string partnerRedirectionUrl)
        {
            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.ThirdPartyPaymentsCheckoutChallangeIFrame }
            });

            PageDisplayHint page = new PageDisplayHint
            {
                HintId = Constants.DisplayHintIds.IFramePageId,
                DisplayName = Constants.DisplayHintIds.ThirdPartyPaymentsCheckoutChallengePageName
            };

            IFrameDisplayHint challengeIFrame = new IFrameDisplayHint
            {
                HintId = Constants.DisplayHintIds.ThirdPartyPaymentsCheckoutChallengeIFrameId,
                DisplayContent = string.Format(Constants.IFrameContentTemplates.ThreeDSOneRedirect, redirectionUrl),
                ExpectedClientActionId = checkoutId
            };

            challengeIFrame.AddDisplayTag("accessibilityName", LocalizationRepository.Instance.GetLocalizedString(Constants.CheckoutChallengeLabels.ChallengeIFrameDisplayLabel, "en-us"));

            page.AddDisplayHint(challengeIFrame);

            // add poll action to query checkout status
            page.Action = CheckoutChallengePollingAction(checkoutId, partnerName, paymentProviderId, partnerRedirectionUrl);

            retVal.AddDisplayPages(new List<PageDisplayHint> { page });

            List<PIDLResource> retList = new List<PIDLResource> { retVal };

            return retList;
        }

        public static List<PIDLResource> GetThreeDSFingerprintIFrameDescription(string threeDSMethodURL, string threeDSMethodData, string threeDSSessionId, string pxAuthURL, string cspStep, string testHeader, object timeoutErrorDetails = null, List<string> exposedFlightFeatures = null)
        {
            // PXAuth Timeout error clientAction
            ClientAction pxAuthTimeoutClientAction = (timeoutErrorDetails == null) ? null : new ClientAction(ClientActionType.Failure, timeoutErrorDetails);

            // ACS fingerprint Timeout error clientAction
            List<PIDLResource> timeoutPidl = GetTimeoutThreeDSFingerprintIFrameDescription(pxAuthURL, threeDSMethodData, threeDSSessionId, pxAuthTimeoutClientAction, exposedFlightFeatures);
            AddTestHeaderAsIFrameFormInput(timeoutPidl, Constants.DisplayHintIds.ThreeDSTimeoutFingerprintIFrameId, testHeader);

            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.FingerprintIFrameDescription }
            });

            PageDisplayHint page = new PageDisplayHint
            {
                HintId = Constants.DisplayHintIds.IFramePageId,
                DisplayName = Constants.DisplayHintIds.IFramePageId
            };

            var timeoutRegex = new Regex("PXPSD2ThreeDSFingerprintTimeout_([0-9]+)", RegexOptions.IgnoreCase);

            IFrameDisplayHint fingerprintingIFrame = new IFrameDisplayHint
            {
                HintId = Constants.DisplayHintIds.ThreeDSFingerprintIFrameId,
                DisplayContent = string.Format(Constants.IFrameContentTemplates.PostThreeDSMethodData, threeDSMethodURL, threeDSMethodData, cspStep),
                Width = "0px",
                Height = "0px",
                ExpectedClientActionId = threeDSSessionId,
                MessageTimeout = ParseIFrameTimeout(timeoutRegex, exposedFlightFeatures), /*timeout in milliseconds*/
                MessageTimeoutClientAction = new ClientAction(ClientActionType.Pidl, timeoutPidl)
            };

            page.AddDisplayHint(fingerprintingIFrame);
            retVal.AddDisplayPages(new List<PageDisplayHint> { page });

            List<PIDLResource> retList = new List<PIDLResource> { retVal };
            AddTestHeaderAsIFrameFormInput(retList, Constants.DisplayHintIds.ThreeDSFingerprintIFrameId, testHeader);
            return retList;
        }

        public static List<PIDLResource> GetThreeDSFingerprintUrlIFrameDescription(string threeDSMethodURL, string threeDSMethodData, string threeDSSessionId, string pxAuthURL, string cspStep, string testHeader, object timeoutErrorDetails = null, List<string> exposedFlightFeatures = null)
        {
            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.FingerprintIFrameDescription }
            });

            PageDisplayHint page = new PageDisplayHint
            {
                HintId = Constants.DisplayHintIds.IFramePageId,
                DisplayName = Constants.DisplayHintIds.IFramePageId
            };

            string sourceUrl = Constants.IFrameContentUrlTemplates.PostThreeDSMethodData;
            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXPSD2EnableCSPUrlProxyFrameWithSanitizedInput, StringComparer.OrdinalIgnoreCase))
            {
                sourceUrl = Constants.IFrameContentUrlSanitizedInputTemplates.PostThreeDSMethodData;
            }

            IFrameDisplayHint fingerprintingIFrame = new IFrameDisplayHint
            {
                HintId = Constants.DisplayHintIds.ThreeDSFingerprintIFrameId,
                Width = "0px",
                Height = "0px",
                ExpectedClientActionId = threeDSSessionId,
                SourceUrl = string.Format(sourceUrl, threeDSMethodURL, threeDSMethodData, cspStep)
            };

            page.AddDisplayHint(fingerprintingIFrame);
            retVal.AddDisplayPages(new List<PageDisplayHint> { page });

            List<PIDLResource> retList = new List<PIDLResource> { retVal };
            AddTestHeaderAsIFrameFormInput(retList, Constants.DisplayHintIds.ThreeDSFingerprintIFrameId, testHeader);
            return retList;
        }

        public static List<PIDLResource> GetTimeoutThreeDSFingerprintIFrameDescription(string pxAuthURL, string threeDSMethodData, string threeDSSessionId, ClientAction messageTimeoutClientAction, List<string> exposedFlightFeatures = null)
        {
            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.TimeoutFingerprintIFrameDescription }
            });

            PageDisplayHint page = new PageDisplayHint
            {
                HintId = Constants.DisplayHintIds.IFramePageId,
                DisplayName = Constants.DisplayHintIds.IFramePageId
            };

            string timeoutIframeTemplate = exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXPSD2TimeoutOnPostViaSrc, StringComparer.OrdinalIgnoreCase)
                ? Constants.IFrameContentTemplates.TimeoutOnPostThreeDSMethodDataViaSrc
                : Constants.IFrameContentTemplates.TimeoutOnPostThreeDSMethodData;

            var timeoutRegex = new Regex(@"PXPSD2ThreeDSTimeoutFingerprintTimeout_([0-9]+)", RegexOptions.IgnoreCase);

            IFrameDisplayHint timeoutFingerprintIFrame = new IFrameDisplayHint
            {
                HintId = Constants.DisplayHintIds.ThreeDSTimeoutFingerprintIFrameId,
                DisplayContent = string.Format(timeoutIframeTemplate, pxAuthURL, threeDSMethodData),
                Width = "0px",
                Height = "0px",
                ExpectedClientActionId = threeDSSessionId,
                MessageTimeout = messageTimeoutClientAction == null ? (int?)null : ParseIFrameTimeout(timeoutRegex, exposedFlightFeatures),
                MessageTimeoutClientAction = messageTimeoutClientAction
            };

            page.AddDisplayHint(timeoutFingerprintIFrame);
            retVal.AddDisplayPages(new List<PageDisplayHint> { page });

            List<PIDLResource> retList = new List<PIDLResource> { retVal };
            return retList;
        }

        public static int ParseIFrameTimeout(Regex regex, List<string> exposedFlightFeatures = null)
        {
            int defaultTimeout = 60000;
            int flightTimeout = defaultTimeout;

            if (exposedFlightFeatures != null)
            {
                try
                {
                    // Find the first matching feature and extract the timeout value
                    flightTimeout = exposedFlightFeatures
                        .Where(x => regex.IsMatch(x))
                        .Select(x => int.Parse(regex.Match(x).Groups[1].Value))
                        .FirstOrDefault();
                }
                catch
                {
                    flightTimeout = defaultTimeout;
                }
            }

            return flightTimeout > 0 ? flightTimeout : defaultTimeout;
        }

        /// <summary>
        /// Set the DisplayContent for the given element
        /// </summary>
        /// <param name="resource">PIDL resource</param>
        /// <param name="elementId">element to be searched</param>
        /// <param name="content">New display content to be set</param>
        public static void SetDisplayContent(PIDLResource resource, string elementId, string content)
        {
            // Find element in pidl resource
            var element = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<TextDisplayHint>(elementId, resource.DisplayPages);

            // if element found set display
            if (element != null)
            {
                element.DisplayContent = content;
            }
        }

        /// <summary>
        /// Add submit links to save context
        /// </summary>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="type">Specifies operation</param>
        /// <param name="overrideJarvisVersionToV3">Override targeted jarvis endpoint version</param>
        /// <param name="resource">PIDL resource to add submit links to</param>
        /// <param name="scenario">Scenario name</param>
        public static void AddSubmitLinksToContext(string partnerName, string type, bool overrideJarvisVersionToV3, List<PIDLResource> resource, string scenario = null)
        {
            var links = GetAddressSubmitLink(partnerName, type, overrideJarvisVersionToV3, scenario: scenario);
            AddSubmitLinks(links, resource);
        }

        public static void AddSetAsDefaultBillingDataDescription(PIDLResource resource, string partner, bool avsSuggest, bool setAsDefaultBilling, string scenario = null, PaymentExperienceSetting setting = null)
        {
            if (SetAddressToDefault(avsSuggest, setAsDefaultBilling, partner, scenario, setting))
            {
                resource.DataDescription["set_as_default_billing_address"] = new PropertyDescription()
                {
                    PropertyType = "clientData",
                    DataType = "hidden",
                    PropertyDescriptionType = "hidden",
                    IsUpdatable = false,
                    DefaultValue = setAsDefaultBilling
                };
            }
        }

        /// <summary>
        /// Gets redirect URL used in QR code challenge or short URL generation
        /// </summary>
        /// <param name="paymentInstrument">the PI</param>
        /// <param name="language">language for localization</param>
        /// <param name="challengeDescriptionType">challenge description type (only Paypal as of now)</param>
        /// <param name="redirectType">used for logging, either QR code or web</param>
        /// <param name="sessionId">used for sessionId</param>
        /// <param name="partnerName">partner name</param>
        /// <param name="setting">setting from partner setting service</param>
        /// <returns>a redirect URL</returns>
        public static string GetRedirectURL(PaymentInstrument paymentInstrument, string language, string challengeDescriptionType, string redirectType = null, string sessionId = null, string partnerName = null, PaymentExperienceSetting setting = null)
        {
            string paymentMethod = paymentInstrument.PaymentMethod.PaymentMethodType;

            if (string.Equals(paymentMethod, Constants.PaymentMethodTypeNames.Paypal, StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.PaypalQrCode, StringComparison.OrdinalIgnoreCase))
                {
                    // The Ru (Success URL) and Rx (Failure URL) have been excluded from the PayPal Redirect URL for Template Partner.
                    // It is now the responsibility of the partner to manage the handling of the PayPal Redirect URL for the Hyperlink, hence Ru and Rx have been omitted from the URL for the Template Partner.
                    if (IsTemplateInList(partnerName, setting, Constants.DescriptionTypes.ChallengeDescription, challengeDescriptionType?.ToLower()))
                    {
                        return paymentInstrument.PaymentInstrumentDetails.RedirectUrl;
                    }
                    else
                    {
                        return paymentInstrument.PaymentInstrumentDetails.RedirectUrl + string.Format(Constants.RedirectUrlStaticRuRx.PaypalQrcodeChallengeRedirectUrlRuRx, language, paymentInstrument.PaymentInstrumentId);
                    }
                }
                else if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.PaypalQrCodeXboxNative, StringComparison.OrdinalIgnoreCase))
                {
                    return paymentInstrument.PaymentInstrumentDetails.RedirectUrl + string.Format(Constants.RedirectUrlStaticRuRx.PaypalQrcodeChallengeXboxNativeRedirectUrlRuRx, language, paymentInstrument.PaymentInstrumentId, redirectType);
                }
            }
            else if (string.Equals(paymentMethod, Constants.PaymentMethodTypeNames.Venmo, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.VenmoQrCode, StringComparison.OrdinalIgnoreCase))
            {
                return paymentInstrument.PaymentInstrumentDetails.RedirectUrl + string.Format(Constants.RedirectUrlStaticRuRx.ThreeDSOneChallengeXboxNativeRedirectUrlRuRx, language, sessionId, paymentInstrument.PaymentMethod.PaymentMethodType, paymentInstrument.PaymentMethod.PaymentMethodFamily, redirectType);
            }

            return string.Empty;
        }

        public static List<PIDLResource> GetCc3DSIframeStatusCheckDescriptionForPI(
            string piid,
            string language,
            string partnerName,
            string scenario,
            string classicProduct,
            bool completePrerequisites,
            string country,
            string sessionQueryUrl)
        {
            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.ThreeDSAddPIIFrameDescription }
            });

            PageDisplayHint page = new PageDisplayHint
            {
                HintId = Constants.DisplayHintIds.IFramePollStatusPageId,
                DisplayName = Constants.DisplayHintIds.ThreeDSIframeAddPIPollStatusPageName
            };

            var polllinks = GetPILink(Constants.RestResourceNames.PaymentInstrumentsEx, language, partnerName, piid, completePrerequisites, country, Constants.ScenarioNames.ThreeDSOnePolling, sessionQueryUrl, classicProduct);
            string pollUrl = polllinks[Constants.ButtonDisplayHintIds.Cc3DSYesButton].Href;
            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());
            PollActionContext pollActionContext = new PollActionContext()
            {
                Href = pollUrl,
                Method = Constants.HTTPVerbs.GET,
                Interval = Constants.PollingIntervals.ThreeDSOnePollingInterval,
                CheckPollingTimeOut = false,
                ResponseResultExpression = Constants.PollingResponseResultExpression.ThreeDSOneResponseResultExpression,
            };

            pollActionContext.AddResponseActionsItem("Active", new DisplayHintAction(DisplayHintActionType.success.ToString()));
            pollActionContext.AddResponseActionsItem("Declined", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
            pollAction.Context = pollActionContext;
            page.Action = pollAction;
            retVal.AddDisplayPages(new List<PageDisplayHint> { page });
            List<PIDLResource> retList = new List<PIDLResource> { retVal };
            return retList;
        }

        public static List<PIDLResource> GetCc3DSIframeRedirectAndStatusCheckDescriptionForPI(
            PaymentInstrument paymentInstrument,
            string language,
            string partnerName,
            string scenario,
            string classicProduct,
            bool completePrerequisites,
            string country,
            string pidlBaseUrl)
        {
            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.ThreeDSAddPIIFrameDescription }
            });

            PageDisplayHint page = new PageDisplayHint
            {
                HintId = Constants.DisplayHintIds.IFramePageId,
                DisplayName = Constants.DisplayHintIds.ThreeDSIframeAddPIPageName
            };

            string ruAnonymousResumePaymentInstumentsExUrl = string.Format(Constants.AnonymousResumePaymentInstumentsExUrls.ThreeDSOne, pidlBaseUrl, paymentInstrument.PaymentInstrumentId, country, language, partnerName, true, paymentInstrument.PaymentInstrumentDetails.SessionQueryUrl);
            string rxAnonymousResumePaymentInstumentsExUrl = string.Format(Constants.AnonymousResumePaymentInstumentsExUrls.ThreeDSOne, pidlBaseUrl, paymentInstrument.PaymentInstrumentId, country, language, partnerName, false, paymentInstrument.PaymentInstrumentDetails.SessionQueryUrl);

            string rdsSessionId;
            try
            {
                rdsSessionId = paymentInstrument.PaymentInstrumentDetails.SessionQueryUrl.Split('/')[1];
            }
            catch
            {
                rdsSessionId = string.Empty;
            }

            IFrameDisplayHint india3DSIframe = new IFrameDisplayHint
            {
                HintId = Constants.DisplayHintIds.ThreeDSIframe,
                DisplayContent = string.Format(Constants.IFrameContentTemplates.ThreeDSOneRedirect, string.Format("{0}?ru={1}&rx={2}", paymentInstrument.PaymentInstrumentDetails.RedirectUrl, WebUtility.UrlEncode(ruAnonymousResumePaymentInstumentsExUrl), WebUtility.UrlEncode(rxAnonymousResumePaymentInstumentsExUrl))),
                ExpectedClientActionId = rdsSessionId
            };

            india3DSIframe.AddDisplayTag("accessibilityName", LocalizationRepository.Instance.GetLocalizedString("The bank authentication dialog", language));
            page.AddDisplayHint(india3DSIframe);
            retVal.AddDisplayPages(new List<PageDisplayHint> { page });
            List<PIDLResource> retList = new List<PIDLResource> { retVal };
            return retList;
        }

        public static List<PIDLResource> RewireXboxNativeAddressPageSaveAction(string operation, List<PIDLResource> retVal)
        {
            if (operation == Constants.PidlOperationTypes.Update || operation == Constants.PidlOperationTypes.Add)
            {
                foreach (PIDLResource resource in retVal)
                {
                    ButtonDisplayHint saveButtonProperties = resource.GetDisplayHintById(Constants.ButtonDisplayHintIds.SaveButton) as ButtonDisplayHint;
                    ButtonDisplayHint addressPageButtonProperties = resource.GetDisplayHintById(Constants.ButtonDisplayHintIds.NextModernValidateButton) as ButtonDisplayHint;

                    if (addressPageButtonProperties != null && saveButtonProperties != null)
                    {
                        addressPageButtonProperties.Action = saveButtonProperties.Action;
                        addressPageButtonProperties.DisplayContent = saveButtonProperties.DisplayContent;
                        addressPageButtonProperties.HintId = saveButtonProperties.HintId;
                    }
                    else
                    {
                        throw new PIDLConfigException(
                            string.Format("xboxsettings add or update pidl incorrectly formatted"),
                            Constants.ErrorCodes.PIDLConfigInvalidPageConfiguration);
                    }
                }
            }

            return retVal;
        }

        public static void RemoveIndiaEditExpiry(List<PIDLResource> retVal, List<string> exposedFlightFeatures = null)
        {
            // disable expiry month and year field for Update CC when IndiaExpiryGroupDelete flight is turned on
            foreach (var pidl in retVal)
            {
                // Set expiry month and year to be optional values to allow for submission
                List<PIDLResource> details = pidl.DataDescription[Constants.DataDescriptionVariableNames.Details] as List<PIDLResource>;
                PropertyDescription expiryMonth = details?.First()?.DataDescription[Constants.ExpiryPrefixes.ExpiryMonth] as PropertyDescription;
                PropertyDescription expiryYear = details?.First()?.DataDescription[Constants.ExpiryPrefixes.ExpiryYear] as PropertyDescription;

                if (expiryMonth != null && expiryYear != null)
                {
                    expiryMonth.IsOptional = true;
                    expiryYear.IsOptional = true;
                }
                else
                {
                    throw new PIDLArgumentException(
                    string.Format("No expiry month or year found "),
                    Constants.ErrorCodes.PIDLConfigMissingDisplayDescriptions);
                }

                // Remove expiry month + year fields
                var expiryGroup = pidl?.GetDisplayHintById(Constants.ExpiryPrefixes.ExpiryGroup) as GroupDisplayHint;
                bool styleHintsEnabled = exposedFlightFeatures != null && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableXboxNativeStyleHints);

                if (expiryGroup == null || exposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableXboxAccessibilityHint))
                {
                    // For xboxnative partners, group hint id is changed as per BUG 46942016
                    if (string.Equals(pidl.Identity[Constants.PaymentMethodKeyNames.Type], Constants.PaymentMethodTypeNames.Amex))
                    {
                        expiryGroup = pidl?.GetDisplayHintById(Constants.ExpiryPrefixes.ExpiryGroupAmexNoLive) as GroupDisplayHint;
                    }
                    else
                    {
                        expiryGroup = pidl?.GetDisplayHintById(Constants.ExpiryPrefixes.ExpiryGroupNoLive) as GroupDisplayHint;
                    }
                }

                var cvv = pidl?.GetDisplayHintById(Constants.DisplayHintIds.CVV) as PropertyDisplayHint;
                if (cvv == null || exposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableXboxAccessibilityHint))
                {
                    if (string.Equals(pidl.Identity[Constants.PaymentMethodKeyNames.Type], Constants.PaymentMethodTypeNames.Amex))
                    {
                        cvv = pidl?.GetDisplayHintById(Constants.DisplayHintIds.CVVAmexWithHint) as PropertyDisplayHint;
                    }
                    else
                    {
                        cvv = pidl?.GetDisplayHintById(Constants.DisplayHintIds.CVVWithHint) as PropertyDisplayHint;
                    }
                }

                GroupDisplayHint creditCardWhereCVVGroup = null;

                switch (pidl.Identity[Constants.PaymentMethodKeyNames.Type])
                {
                    case Constants.PaymentMethodTypeNames.Visa:
                        creditCardWhereCVVGroup = pidl.GetDisplayHintById(Constants.DisplayHintIds.CreditCardVisaWhereCVVGroup) as GroupDisplayHint;
                        if (styleHintsEnabled)
                        {
                            creditCardWhereCVVGroup = creditCardWhereCVVGroup ?? pidl.GetDisplayHintById(Constants.DisplayHintIds.CreditCardVisaWhereCVVGroupUpdate) as GroupDisplayHint;
                        }

                        break;
                    case Constants.PaymentMethodTypeNames.MasterCard:
                        creditCardWhereCVVGroup = pidl.GetDisplayHintById(Constants.DisplayHintIds.CvvHelpGroup) as GroupDisplayHint;
                        if (creditCardWhereCVVGroup == null || exposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableXboxAccessibilityHint))
                        {
                            creditCardWhereCVVGroup = pidl.GetDisplayHintById(Constants.DisplayHintIds.CvvHelpNoLiveGroup) as GroupDisplayHint;
                        }

                        if (styleHintsEnabled)
                        {
                            creditCardWhereCVVGroup = creditCardWhereCVVGroup ?? pidl.GetDisplayHintById(Constants.DisplayHintIds.CreditCardMCWhereCVVGroupUpdate) as GroupDisplayHint;
                        }

                        break;
                    case Constants.PaymentMethodTypeNames.Amex:
                        creditCardWhereCVVGroup = pidl.GetDisplayHintById(Constants.DisplayHintIds.CvvAmexHelpGroup) as GroupDisplayHint;
                        if (styleHintsEnabled)
                        {
                            creditCardWhereCVVGroup = creditCardWhereCVVGroup ?? pidl.GetDisplayHintById(Constants.DisplayHintIds.CreditCardAmexWhereCVVGroupUpdate) as GroupDisplayHint;
                        }

                        break;
                    default:
                        creditCardWhereCVVGroup = null;
                        break;
                }

                PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<PropertyDisplayHint>(Constants.ExpiryPrefixes.ExpiryMonth, pidl.DisplayPages);
                PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<PropertyDisplayHint>(Constants.ExpiryPrefixes.ExpiryYear, pidl.DisplayPages);
                PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<ExpressionDisplayHint>(Constants.ExpiryPrefixes.CreditCardExpiration, pidl.DisplayPages);

                // Change postion of whereCvvGroup is in the pidl (needed for formatting) 
                if (creditCardWhereCVVGroup != null)
                {
                    PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<GroupDisplayHint>(creditCardWhereCVVGroup.HintId, pidl.DisplayPages);
                    expiryGroup.AddDisplayHint(creditCardWhereCVVGroup);
                    creditCardWhereCVVGroup.HintId = Constants.IndiaTokenizationHintIds.CreditCardWhereCVVGroupIndiaTokenization;

                    expiryGroup.StyleHints = new List<string>() { "width-triquarter", "gap-medium" };
                    creditCardWhereCVVGroup.StyleHints = new List<string>() { "margin-top-negative-small" };
                }

                // Update hint id's to match client side
                if (expiryGroup != null)
                {
                    expiryGroup.HintId = Constants.IndiaTokenizationHintIds.ExpiryGroupIndiaTokenization;
                }

                if (cvv != null)
                {
                    cvv.HintId = Constants.IndiaTokenizationHintIds.CvvIndiaTokenization;
                }
            }
        }

        public static void IndiaEditSummaryFooter(List<PIDLResource> retVal)
        {
            foreach (PIDLResource pidl in retVal)
            {
                PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<GroupDisplayHint>(Constants.DisplayHintIds.TokenizationGroup, pidl.DisplayPages);
                PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<TextDisplayHint>(Constants.DisplayHintIds.IndiaTokenConsentMessage, pidl.DisplayPages);
                GroupDisplayHint disclaimerGroup = pidl.GetDisplayHintById(Constants.DisplayHintIds.DisclaimerGroup) as GroupDisplayHint;
                GroupDisplayHint summaryFooterDonePreviousGroup = pidl.GetDisplayHintById(Constants.DisplayHintIds.SummaryFooterDonePreviousGroup) as GroupDisplayHint;
                summaryFooterDonePreviousGroup.AddDisplayHint(disclaimerGroup);
            }
        }

        public static void RemoveIndiaDeleteExpiry(List<PIDLResource> retVal)
        {
            PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<TextDisplayHint>(Constants.ExpiryPrefixes.DeleteExpiry, retVal.First().DisplayPages);
        }

        /// <summary>
        /// Returns true if a partner has a PidlFactory config, is a test partner, or does not require a PidlFactory config
        /// </summary>
        /// <param name="partnerName">name of partner</param>
        /// /// <returns>bool for whether partner config file exists</returns>
        public static bool CheckPartnerPIDLConfigurationExists(string partnerName)
        {
            if (partnerNames == null)
            {
                string fullPathToPartnersDirectories = Helper.GetFullPath(Constants.PidlConfig.DisplayDescriptionFolderRootPath);
                partnerNames = Directory.GetDirectories(fullPathToPartnersDirectories).Select(x => (new DirectoryInfo(x)).Name).ToList();
            }

            // Using MapPartnerName to map console partners with PROD traffic but not in PidlFactory config to a partner with a PidlFactory config
            partnerName = MapPartnerName(partnerName);

            return partnerNames.Contains(partnerName, StringComparer.OrdinalIgnoreCase);
        }

        public static List<PIDLResource> XboxCoBrandedCardQRCodeRestAction(
            string partnerName,
            string country,
            string language,
            List<string> exposedFlightFeatures,
            string channel,
            string referrerId,
            string ocid)
        {
            List<PIDLResource> retList = new List<PIDLResource>()
            {
                new PIDLResource()
            };

            var submitLinks = GetPIApplyLink(Constants.RestResourceNames.PaymentInstrumentsEx, country, language, Constants.PidlOperationTypes.Apply, partnerName, exposedFlightFeatures, channel, referrerId, null, ocid);
            RestLink link = submitLinks[Constants.AutoSubmitIds.XboxCoBrandedCardQrCode];

            retList.First().ClientAction = new ClientAction(ClientActionType.RestAction)
            {
                Context = link
            };

            return retList;
        }

        public static void UpdateXboxCoBrandedCardQrCodeDescription(
            List<PIDLResource> retList,
            string qrCodeURL,
            string webviewURL,
            string language,
            string challengeDescriptionType,
            bool useIntPolling,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            string emailAddress = null,
            string country = null,
            List<string> exposedFlightFeatures = null,
            string sessionId = null,
            string shortUrl = null,
            string channel = null,
            string referrerId = null)
        {
            if (!string.IsNullOrEmpty(emailAddress))
            {
                Context.EmailAddress = emailAddress;
            }

            PIDLResource retVal = retList[0];

            if (retVal.DisplayPages == null)
            {
                return;
            }

            // add the tooltip on the Apply On Console button
            ButtonDisplayHint applyOnConsoleButton = retVal.GetDisplayHintById(Constants.DisplayHintIds.XboxCoBrandedCardQrCodeRedirectButton) as ButtonDisplayHint;
            applyOnConsoleButton.TooltipText = LocalizationRepository.Instance.GetLocalizedString(Constants.StandardizedDisplayText.XboxCoBrandedCardApplyOnConsoleTooltipText, language);

            List<DisplayHint> qrCodeChallengeImages = retVal.GetAllDisplayHintsOfId(Constants.DisplayHintIds.XboxCoBrandedCardQrCodeImage);

            if (qrCodeChallengeImages.Count() == 0)
            {
                throw new PIDLConfigException(
                        string.Format("No Display Hint found for Id \"{0}\"", Constants.DisplayHintIds.XboxCoBrandedCardQrCodeImage),
                        Constants.ErrorCodes.PIDLConfigUnknownDisplayHintId);
            }

            foreach (ImageDisplayHint qrCodeChallengeImage in qrCodeChallengeImages)
            {
                string qrCodeChallengeImageSourceUrl = GetUrlQrCodeImage(qrCodeURL);
                qrCodeChallengeImage.SourceUrl = qrCodeChallengeImageSourceUrl;
                qrCodeChallengeImage.AccessibilityName = LocalizationRepository.Instance.GetLocalizedString(Constants.StandardizedDisplayText.XboxCoBrandedCardQRCodeAccessibilityName, language);
            }

            // mark the text on the 2nd qr code page as live region none so it doesn't get read twice
            PageDisplayHint page2 = retVal.DisplayPages[1];
            string[] textDisplayHintIds =
                {
                    Constants.DisplayHintIds.XboxCoBrandedCardQrCodeHeading,
                    Constants.DisplayHintIds.XboxCoBrandedCardQrCodeSubheading,
                    Constants.DisplayHintIds.XboxCoBrandedCardQrCodeText,
                    Constants.DisplayHintIds.XboxCoBrandedCardQrCodeBodyText
                };

            foreach (string id in textDisplayHintIds)
            {
                retVal.GetDisplayHintFromContainer(page2, id).AddDisplayTag("noPidlddc.disableRegion", "pidlddc-disable-live");
            }

            SetupRedirectWithIframe(retVal, webviewURL, hintId: Constants.DisplayHintIds.XboxCoBrandedCardIframe);

            var submitLinks = GetPIApplyLink(Constants.RestResourceNames.PaymentInstrumentsEx, country, language, Constants.PidlOperationTypes.Apply, partnerName, exposedFlightFeatures, channel, referrerId);
            RestLink submissionLink = submitLinks[Constants.AutoSubmitIds.XboxCoBrandedCardQrCode];
            ButtonDisplayHint webviewBackButton = retVal.GetDisplayHintById(Constants.DisplayHintIds.XboxCoBrandedCardQrCodeWebviewCancelButton) as ButtonDisplayHint;
            webviewBackButton.Action.ActionType = DisplayHintActionType.restAction.ToString();
            webviewBackButton.Action.Context = submissionLink;

            // Add pidl action for b press on the webview page
            PageDisplayHint webviewPage = retVal.GetDisplayHintById(Constants.DisplayHintIds.XboxCoBrandedCardQrCodePage3) as PageDisplayHint;

            if (webviewPage != null)
            {
                DisplayHintAction webviewAction = new DisplayHintAction(DisplayHintActionType.restAction.ToString());
                webviewAction.Context = submitLinks[Constants.AutoSubmitIds.XboxCoBrandedCardQrCode];
                webviewPage.AddKeyPidlAction(Constants.ControllerKeyCodes.GamePadB, webviewAction);
            }

            string pollingUrlTemplate = null;
            if (useIntPolling)
            {
                pollingUrlTemplate = Constants.HandleGlobalQrCodePIPendingPurchaseUrls.XboxCoBrandedCardRedirectionUrlTemplate.Replace("{{redirection-endpoint}}", Constants.PidlUrlConstants.PIFDIntDomain);
            }

            AddQrCodePollActionContext(challengeDescriptionType, retVal, Constants.RestResourceNames.PaymentInstrumentsEx, language, partnerName, string.Empty, false, country, exposedFlightFeatures, sessionId, null, string.Empty, null, pollingUrlTemplate, restLink: submissionLink);

            IFrameDisplayHint webview = retVal.GetDisplayHintById(Constants.DisplayHintIds.XboxCoBrandedCardIframe) as IFrameDisplayHint;
            webview.UseAuth = true;
            webview.LoadingMessage = LocalizationRepository.Instance.GetLocalizedString(Constants.UnlocalizedDisplayText.XboxCardApplyIFrameLoadingtext, language);
        }

        public static List<PIDLResource> BuildItalyTaxIDForm(
            List<PIDLResource> taxIdPidls,
            List<string> exposedFlightFeatures,
            string partner,
            string country,
            PaymentExperienceSetting setting,
            bool addVatIdAsLinkedPidl,
            bool isScenarioCheckPassed)
        {
            PIDLResource currentItalyVatIdPidl = null;
            PIDLResource currentItalyCodiceFiscalePidl = null;
            foreach (PIDLResource taxIdPidl in taxIdPidls)
            {
                string taxIdPidlType = null;
                taxIdPidl.Identity.TryGetValue(Constants.DescriptionIdentityFields.Type, out taxIdPidlType);
                if (string.Equals(taxIdPidlType, Constants.PidlResourceIdentities.VatId, StringComparison.InvariantCultureIgnoreCase))
                {
                    currentItalyVatIdPidl = taxIdPidl;
                }
                else if (string.Equals(taxIdPidlType, Constants.PidlResourceIdentities.ItalyCodiceFiscale, StringComparison.InvariantCultureIgnoreCase))
                {
                    currentItalyCodiceFiscalePidl = taxIdPidl;
                }
            }

            // Enable the template partner check, to sync with the EnableItalyCodiceFiscale flighting, utilized for the profile.
            if (exposedFlightFeatures.Contains(Constants.PartnerFlightValues.EnableItalyCodiceFiscale)
                || IsTemplateInList(partner, setting, Constants.DescriptionTypes.ProfileDescription, Constants.ProfileTypes.Organization))
            {
                if (currentItalyCodiceFiscalePidl != null && currentItalyVatIdPidl != null)
                {
                    if (addVatIdAsLinkedPidl)
                    {
                        currentItalyVatIdPidl.UpdateIsOptionalProperty(Constants.DescriptionTypes.TaxIdDescription, true);
                    }

                    taxIdPidls = new List<PIDLResource> { currentItalyCodiceFiscalePidl, currentItalyVatIdPidl };

                    // If flight PXSetItalyTaxIdValuesByFunction is enabled, then value for DefaultValue is changed for TaxId property.
                    if (exposedFlightFeatures.Contains(Flighting.Features.PXSetItalyTaxIdValuesByFunction) && isScenarioCheckPassed)
                    {
                        currentItalyVatIdPidl.UpdateDefaultValueForProperty(Constants.DescriptionTypes.TaxIdDescription, "(<|getVatId|>)");
                        currentItalyCodiceFiscalePidl.UpdateDefaultValueForProperty(Constants.DescriptionTypes.TaxIdDescription, "(<|getNationalIdentificationNumber|>)");
                    }

                    if (addVatIdAsLinkedPidl)
                    {
                        // For partners handling callbacks on the client side, linked pidl should not have secondary resource scenario context.
                        // Setting linkedPidl as secondary resource will invoke only one callback and trigger api call for another action.
                        if (!PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.PXTaxIdFormSkipSecondaryResourceContext, country, setting))
                        {
                            currentItalyVatIdPidl.MakeSecondaryResource();
                        }

                        currentItalyVatIdPidl.HideDisplayHintsById(new List<string> { Constants.ButtonDisplayHintIds.SaveButton, Constants.ButtonDisplayHintIds.SaveButtonSuccess, Constants.ButtonDisplayHintIds.CancelButton });

                        taxIdPidls = new List<PIDLResource> { currentItalyCodiceFiscalePidl };
                        foreach (PIDLResource taxIdPidl in taxIdPidls)
                        {
                            taxIdPidl.HideDisplayHintsById(new List<string> { Constants.DisplayHintIds.HapiTaxCountryProperty });
                        }

                        AddLinkedPidlToResourceList(taxIdPidls, currentItalyVatIdPidl, partner);
                    }
                }
            }
            else
            {
                taxIdPidls = new List<PIDLResource> { currentItalyVatIdPidl };
            }

            return taxIdPidls;
        }

        public static List<PIDLResource> AddCCQrCodeInAddConsole(
            ref List<PIDLResource> retVal,
            string scenario,
            string challengeDescriptionType,
            string templateName,
            string type,
            string family,
            string language,
            string country,
            List<string> exposedFlightFeatures,
            string sessionId,
            string accountId,
            string partner)
        {
            foreach (PIDLResource pidl in retVal)
            {
                PimsModel.V4.PaymentInstrument newPi = new PimsModel.V4.PaymentInstrument()
                {
                    PaymentMethod = new PaymentMethod()
                    {
                        PaymentMethodType = type,
                        PaymentMethodFamily = family
                    }
                };
                List<PIDLResource> qrCodeInfo = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForPI(newPi, language, challengeDescriptionType, templateName, country: country, exposedFlightFeatures: exposedFlightFeatures, sessionId: sessionId, accountId: accountId, originalPartner: partner);

                ImageDisplayHint addCCQrCodeImage = pidl.GetDisplayHintById(Constants.DisplayHintIds.AddCCQrCodeImage) as ImageDisplayHint;
                ImageDisplayHint qrCodeInfoImage = qrCodeInfo?[0].GetDisplayHintById(Constants.DisplayHintIds.AddCCQrCodeImage) as ImageDisplayHint;

                if (addCCQrCodeImage != null && qrCodeInfoImage != null)
                {
                    addCCQrCodeImage.SourceUrl = qrCodeInfoImage.SourceUrl;
                }

                // Add polling action
                pidl.DisplayPages.First().Action = qrCodeInfo?.First().DisplayPages.First().Action;
            }

            return retVal;
        }

        public static List<PIDLResource> BuildEgyptTaxIDForm(
            List<PIDLResource> taxIdPidls,
            List<string> exposedFlightFeatures,
            string partner,
            string country,
            PaymentExperienceSetting setting,
            bool addVatIdAsLinkedPidl,
            bool isScenarioCheckPassed)
        {
            PIDLResource currentEgyptVatIdPidl = null;
            PIDLResource currentEgyptNationalIdPidl = null;
            foreach (PIDLResource taxIdPidl in taxIdPidls)
            {
                string taxIdPidlType = null;
                taxIdPidl.Identity.TryGetValue(Constants.DescriptionIdentityFields.Type, out taxIdPidlType);
                if (string.Equals(taxIdPidlType, Constants.PidlResourceIdentities.VatId, StringComparison.InvariantCultureIgnoreCase))
                {
                    currentEgyptVatIdPidl = taxIdPidl;
                }
                else if (string.Equals(taxIdPidlType, Constants.PidlResourceIdentities.EgyptNationalIdentificationNumber, StringComparison.InvariantCultureIgnoreCase))
                {
                    currentEgyptNationalIdPidl = taxIdPidl;
                }
            }

            if (currentEgyptNationalIdPidl != null && currentEgyptVatIdPidl != null)
            {
                if (exposedFlightFeatures.Contains(Flighting.Features.PXEnableEGTaxIdsRequired))
                {
                    currentEgyptVatIdPidl.UpdateIsOptionalProperty(Constants.DescriptionTypes.TaxIdDescription, false);
                    currentEgyptNationalIdPidl.UpdateIsOptionalProperty(Constants.DescriptionTypes.TaxIdDescription, false);
                }
                else
                {
                    currentEgyptVatIdPidl.UpdateIsOptionalProperty(Constants.DescriptionTypes.TaxIdDescription, true);
                    currentEgyptNationalIdPidl.UpdateIsOptionalProperty(Constants.DescriptionTypes.TaxIdDescription, true);
                }                

                taxIdPidls = new List<PIDLResource> { currentEgyptNationalIdPidl, currentEgyptVatIdPidl };               
               
                if (addVatIdAsLinkedPidl)
                {
                    // For partners handling callbacks on the client side, linked pidl should not have secondary resource scenario context.
                    // Setting linkedPidl as secondary resource will invoke only one callback and trigger api call for another action.
                    if (!PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.PXTaxIdFormSkipSecondaryResourceContext, country, setting))
                    {
                        currentEgyptVatIdPidl.MakeSecondaryResource();
                    }

                    currentEgyptVatIdPidl.HideDisplayHintsById(new List<string> { Constants.ButtonDisplayHintIds.SaveButton, Constants.ButtonDisplayHintIds.SaveButtonSuccess, Constants.ButtonDisplayHintIds.CancelButton });

                    taxIdPidls = new List<PIDLResource> { currentEgyptNationalIdPidl };
                    foreach (PIDLResource taxIdPidl in taxIdPidls)
                    {
                        taxIdPidl.HideDisplayHintsById(new List<string> { Constants.DisplayHintIds.HapiTaxCountryProperty });
                    }
                    
                    if (exposedFlightFeatures.Contains(Flighting.Features.PXSubmitEGTaxIdsInSequence))
                    {
                        AddLinkedPidlToResourceList(taxIdPidls, currentEgyptVatIdPidl, partner, PidlContainerDisplayHint.SubmissionOrder.AfterBase);
                    }
                    else
                    {
                        AddLinkedPidlToResourceList(taxIdPidls, currentEgyptVatIdPidl, partner);
                    }                   
                }
            }

            return taxIdPidls;
        }

        /// <summary>
        /// Get the billing group lightweight add description object
        /// </summary>
        /// <param name="type">Specifies type of billing group</param>
        /// <param name="operation">Specifies operation</param>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="scenario">The name of the scenario</param>
        /// <param name="setting">Setting data from Partner Settings Service</param>
        /// <returns>Returns add BillingGroupDescription</returns>
        public List<PIDLResource> GetBillingGroupDescriptions(
            string type,
            string operation,
            string country,
            string language,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            string scenario = null,
            PaymentExperienceSetting setting = null)
        {
            type = Helper.TryToLower(type);
            operation = Helper.TryToLower(operation);
            Context.Culture = Helper.GetCultureInfo(language);
            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.BillingGroupDescription }
            });

            this.GetPIDLResourceRecursive(partnerName, Constants.DescriptionTypes.BillingGroupDescription, type, country, operation, retVal, scenario: scenario, setting: setting);

            List<PIDLResource> retList = new List<PIDLResource>()
            {
                retVal
            };

            bool isAddOperation = string.Equals(operation, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase);
            bool hasAddSelectResourceFeature = PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.AddSelectResourcePartnerActionToBillingGroupAddPi, country, setting);
            bool isCommercialStores = string.Equals(partnerName, Constants.PartnerNames.Commercialstores, StringComparison.OrdinalIgnoreCase);
            bool isLightWeightType = string.Equals(type, Constants.BillingGroupTypeNames.LightWeight, StringComparison.OrdinalIgnoreCase) || string.Equals(type, Constants.BillingGroupTypeNames.LightWeightV7, StringComparison.OrdinalIgnoreCase);

            if ((isLightWeightType && isAddOperation) && (hasAddSelectResourceFeature || isCommercialStores))
            {
                AddActionToAddPaymentInstrument(retList, language, country, partnerName);
            }

            // Add the submitUrl to save Billing Group
            AddSubmitLinks(GetBillingGroupSubmitLink(type, operation), retList);
            return retList;
        }

        public List<PIDLResource> XboxCardUpsell(string language)
        {
            // TODO (48215071): Needs to be defined in CSVs instead of hardcoded
            // TODO (48215346): Add issuer service logic for serving Upsell 
            List<PIDLResource> retVal = this.GetStaticPidlDescriptions(Constants.StaticDescriptionTypes.XboxCardUpsellBuyNowPidl, language, PXCommon.Constants.PartnerNames.XboxNative);

            PIDLResource pidlResource = retVal.First<PIDLResource>();
            ButtonDisplayHint button = pidlResource.GetDisplayHintById("xboxCardUpsellBuyNowButton") as ButtonDisplayHint;

            GroupDisplayHint contentWrapperGroup = new GroupDisplayHint();
            contentWrapperGroup.HintId = Constants.XboxCardUpsellDisplayHintIds.XboxCardUpsellBuyNowContentWrapperGroup;
            GroupDisplayHint contentGroup = new GroupDisplayHint();
            contentGroup.HintId = Constants.XboxCardUpsellDisplayHintIds.XboxCardUpsellInnerContentGroup;
            GroupDisplayHint leftContentGroup = new GroupDisplayHint();
            leftContentGroup.HintId = Constants.XboxCardUpsellDisplayHintIds.XboxCardUpsellInnerLeftContentGroup;
            GroupDisplayHint rightContentGroup = new GroupDisplayHint();
            rightContentGroup.HintId = Constants.XboxCardUpsellDisplayHintIds.XboxCardUpsellInnerRightContentGroup;

            ImageDisplayHint backgroundImage = new ImageDisplayHint();
            backgroundImage.HintId = Constants.XboxCardUpsellDisplayHintIds.XboxCardUpsellBackground;
            backgroundImage.SourceUrl = Constants.PidlUrlConstants.XboxCardApplyBackgroundImage;

            ImageDisplayHint cardImage = new ImageDisplayHint();
            cardImage.HintId = Constants.XboxCardUpsellDisplayHintIds.XboxCardUpsellCardImage;
            cardImage.SourceUrl = Constants.PidlUrlConstants.XboxCardImage;

            TextDisplayHint applyNowText = new TextDisplayHint();
            applyNowText.HintId = Constants.XboxCardUpsellDisplayHintIds.XboxCardUpsellApplyNowText;
            applyNowText.DisplayContent = LocalizationRepository.Instance.GetLocalizedString(Constants.XboxCardUpsellDisplayText.ApplyNow, language);

            TextDisplayHint mainText = new TextDisplayHint();
            mainText.HintId = Constants.XboxCardUpsellDisplayHintIds.XboxCardUpsellMainText;
            mainText.DisplayContent = LocalizationRepository.Instance.GetLocalizedString(Constants.XboxCardUpsellDisplayText.MainText, language);

            TextDisplayHint subText = new TextDisplayHint();
            subText.HintId = Constants.XboxCardUpsellDisplayHintIds.XboxCardUpsellSubText;
            subText.DisplayContent = LocalizationRepository.Instance.GetLocalizedString(Constants.XboxCardUpsellDisplayText.SubText, language);

            leftContentGroup.AddDisplayHint(cardImage);
            leftContentGroup.AddDisplayHint(applyNowText);
            rightContentGroup.AddDisplayHint(mainText);
            rightContentGroup.AddDisplayHint(subText);

            contentGroup.AddDisplayHint(leftContentGroup);
            contentGroup.AddDisplayHint(rightContentGroup);
            contentWrapperGroup.AddDisplayHint(contentGroup);
            contentWrapperGroup.AddDisplayHint(backgroundImage);

            button.DisplayContentGroup = contentWrapperGroup;

            return retVal;
        }

        public void Initialize()
        {
            // -----------------------------------------------
            // Read DomainDictionaries CSV file
            ReadDomainDictionaryConfig();

            // ---------------------------------------------------
            // Read and parse PropertyDescriptions
            this.ReadPropertyDescriptionsConfig();

            // -----------------------------------------------
            // Read TaxIdsInCountries CSV file
            this.ReadTaxIdsInCountriesConfig();

            // -----------------------------------------------
            // Read ValidationChallengeTypes CSV file
            this.ReadValidationChallengeTypesConfig();

            // ---------------------------------------------------
            // Read and parse PropertyValidation
            this.ReadPropertyValidationsConfig();

            // ---------------------------------------------------
            // Read and parse PropertyTransformation
            this.ReadPropertyTransformationConfig();

            // ---------------------------------------------------
            // Read and parse PropertyDataProtection
            this.ReadPropertyDataProtectionsConfig();

            // ---------------------------------------------------
            // Read and parse InfoDescriptions
            PIDLResourceConfig.ReadFromConfig(Helper.GetFullPath(Constants.DataDescriptionFilePaths.PIDLResourcesCSV), out this.pidlResourceConfigs);

            // ---------------------------------------------------
            // Read and parse DataSources
            DataSourcesConfig.ReadFromConfig(Helper.GetFullPath(Constants.DataSourcesFilePaths.DataSourcesCSV), out this.dataSources);

            // ---------------------------------------------------
            // Read and parse SubmitLinks
            SubmitLink.ReadFromConfig(Helper.GetFullPath(Constants.SubmitLinksFilePaths.SubmitLinksCSV), out this.submitLinks);
        }

        /// <summary>
        /// Gets a specified PaymentMethodDescription object by a given id
        /// </summary>
        /// <param name="paymentMethods">Specifies the list of payment methods for which the description needs to be returned</param>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="family">This is the payment_method_family property</param>
        /// <param name="type">This is the payment_method_type property</param>
        /// <param name="operation">This is the operation type for which the pidl is requested</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="emailAddress">The name of the email address</param>
        /// <param name="classicProduct">The name of the classic product</param>
        /// <param name="billableAccountId">The name of the billableAccountId</param>
        /// <param name="clientContext">Client context info</param>
        /// <param name="completePrerequisites">Specifies whether pidl should handle prerequisites within adding pi flow</param>
        /// <param name="exposedFlightFeatures">All flighting features exposed from the current flight context</param>
        /// <param name="scenario">Specifies scenario for display description behaviour</param>
        /// <param name="orderId">Specifies orderId of the purchase attempt</param>
        /// <param name="channel">Specifies channel for the xbox card apply request</param>
        /// <param name="referrerId">Specifies referrerId for the xbox card apply request</param>
        /// <param name="sessionId">Specifies sessionId for the xbox card apply request</param>
        /// <param name="setting">Setting which includes template and features used for PIDL generation</param>
        /// <param name="pxChallengeSessionId">PXChallengeSessionId which is used for Challenge Management flow</param>
        /// <param name="deviceClass">Channel to pass to over downstream service</param>
        /// <param name="originalPartnerName">The name of the original partner</param>
        /// <param name="firstName">The first name of the user</param>
        /// <param name="lastName">The last name of the user</param>
        /// <returns>Returns a dictionary with the key is the id of the PaymentMethodDescription</returns>
        public List<PIDLResource> GetPaymentMethodDescriptions(
            HashSet<PaymentMethod> paymentMethods,
            string country,
            string family,
            string type,
            string operation,
            string language,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            string emailAddress = null,
            string classicProduct = null,
            string billableAccountId = null,
            Dictionary<string, object> clientContext = null,
            bool completePrerequisites = false,
            List<string> exposedFlightFeatures = null,
            string scenario = null,
            string orderId = null,
            string channel = null,
            string referrerId = null,
            string sessionId = null,
            PaymentExperienceSetting setting = null,
            string pxChallengeSessionId = null,
            string deviceClass = null,
            string originalPartnerName = null,
            string firstName = null,
            string lastName = null)
        {
            country = Helper.TryToLower(country);
            family = Helper.TryToLower(family);
            type = Helper.TryToLower(type);
            language = Helper.TryToLower(language);
            operation = Helper.TryToLower(operation);

            // Validate input parameters
            ValidateCountry(country);
            ValidateOperation(operation);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);
            Context.PartnerName = partnerName;

            if (!string.IsNullOrEmpty(emailAddress))
            {
                Context.EmailAddress = emailAddress;
            }

            List<PIDLResource> retVal = new List<PIDLResource>();

            HashSet<PaymentMethod> resultSet = paymentMethods;
            if (resultSet.Count == 0)
            {
                throw new PIDLArgumentException(
                    string.Format("No results found for PaymentMethodFamily and PaymentMethodType for the provided country."),
                    Constants.ErrorCodes.PIDLArgumentFamilyIsNotSupportedForStoreInCountry);
            }

            foreach (PaymentMethod currentPaymentMethod in resultSet)
            {
                IEnumerable<string> pidlIds = GetPIDLIds(partnerName, currentPaymentMethod.PaymentMethodFamily, currentPaymentMethod.PaymentMethodType, exposedFlightFeatures, scenario, country, operation, setting);
                foreach (string id in pidlIds)
                {
                    Dictionary<string, string> identityTable = new Dictionary<string, string>
                    {
                        { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.PaymentMethodDescription },
                        { Constants.DescriptionIdentityFields.Family, currentPaymentMethod.PaymentMethodFamily },
                        { Constants.DescriptionIdentityFields.Type, currentPaymentMethod.PaymentMethodType },
                        { Constants.DescriptionIdentityFields.Operation, operation },
                        { Constants.DescriptionIdentityFields.Country, country },
                        { Constants.DescriptionIdentityFields.ResourceIdentity, id }
                    };

                    if (string.Equals(currentPaymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.InvariantCultureIgnoreCase) &&
                        Constants.KoreaCreditCardType.TypeNames.Contains(currentPaymentMethod.PaymentMethodType) &&
                        string.Equals(country, "kr", StringComparison.InvariantCultureIgnoreCase) &&
                        string.Equals(operation, Constants.PidlOperationTypes.Add, StringComparison.InvariantCultureIgnoreCase))
                    {
                        identityTable.Remove(Constants.DescriptionIdentityFields.Type);
                    }

                    PIDLResource newPMD = new PIDLResource(identityTable);
                    newPMD.InitClientContext(clientContext);

                    string logo = FindSvgLogoOrDefault(currentPaymentMethod.Display.Logos, currentPaymentMethod.Display.Logo);
                    logo = logo != null ? new Uri(logo).PathAndQuery : GlobalConstants.Defaults.Logo;

                    Dictionary<string, string> context = new Dictionary<string, string>()
                    {
                        {
                            Constants.ConfigSpecialStrings.CountryId, country
                        },
                        {
                            Constants.ConfigSpecialStrings.Language, Context.Culture.Name
                        },
                        {
                            Constants.ConfigSpecialStrings.Operation, operation
                        },
                        {
                            Constants.ConfigSpecialStrings.EmailAddress, emailAddress
                        },
                        {
                            Constants.HiddenOptionalFields.ContextKey, string.Empty
                        },
                        {
                            Constants.ConfigSpecialStrings.PaymentMethodDisplayName, currentPaymentMethod.Display.Name
                        },
                        {
                            Constants.ConfigSpecialStrings.PaymentMethodSvgLogo, logo
                        },
                        {
                            Constants.ConfigSpecialStrings.Channel, deviceClass
                        },
                        {
                            Constants.ConfigSpecialStrings.FirstName, firstName
                        },
                        {
                            Constants.ConfigSpecialStrings.LastName, lastName
                        },
                    };

                    if (currentPaymentMethod.Properties?.ChargeThreshold != null && currentPaymentMethod.Properties?.ChargeThreshold.Count > 0)
                    {
                        context.Add(Constants.ConfigSpecialStrings.ChargeThresholdsMaxPrice, CurrencyHelper.FormatCurrency(country, language, currentPaymentMethod.Properties.ChargeThreshold.First().MaxPrice, currentPaymentMethod.Properties.ChargeThreshold.First().Currency));
                    }

                    retVal.Add(newPMD);
                    this.GetPIDLResourceRecursive(
                        partnerName,
                        Constants.DescriptionTypes.PaymentMethodDescription,
                        id,
                        country,
                        operation,
                        newPMD,
                        null,
                        true,
                        null,
                        Constants.HiddenOptionalFields.AddressDescriptionPropertyNames,
                        context,
                        scenario,
                        exposedFlightFeatures,
                        setting: setting);
                }
            }

            // If the Payment Method family is of type Mobile Billing Non Sim then populate the Payment Methods in the list of Possible Values
            UpdatePaymentMethodForBillingNonSim(retVal, operation, family, partnerName, resultSet, exposedFlightFeatures);

            // Keeping the following specific dataDescriptions breaks the delete flow. Removing here instead of from PIDLResource.csv
            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
            {
                if (operation == Constants.PidlOperationTypes.Delete)
                {
                    if (family == Constants.PaymentMethodFamilyNames.MobileBillingNonSim)
                    {
                        RemoveDataDescriptionWithFullPath(retVal?.FirstOrDefault(), null, new string[] { "paymentMethodType" }, null);
                        var dataDescription_details = retVal?.FirstOrDefault()?.DataDescription["details"] as List<PIDLResource>;
                        RemoveDataDescriptionWithFullPath(dataDescription_details?.FirstOrDefault(), null, new string[] { "msisdn" }, null);
                    }
                }
                else if (operation == Constants.PidlOperationTypes.Update && type != Constants.PaymentMethodTypeNames.Alipay && type != Constants.PaymentMethodTypeNames.CupDebitCard)
                {
                    var dataDescription_details = retVal?.FirstOrDefault()?.DataDescription["details"] as List<PIDLResource>;
                    PropertyTransformationInfo nonEditablePiDisplayTransformation = new PropertyTransformationInfo()
                    {
                        TransformCategory = "regex",
                        InputRegex = "^(.*)",
                        TransformRegex = "$1"
                    };

                    if (family == Constants.PaymentMethodFamilyNames.CreditCard)
                    {
                        var dataDescription_details_cvvTokenProperties = dataDescription_details?.FirstOrDefault()?.DataDescription["cvvToken"] as PropertyDescription;
                        dataDescription_details_cvvTokenProperties.IsOptional = false;

                        var dataDescription_details_lastFourDigitsProperties = dataDescription_details?.FirstOrDefault()?.DataDescription["lastFourDigits"] as PropertyDescription;
                        dataDescription_details_lastFourDigitsProperties.IsKey = true;
                        dataDescription_details_lastFourDigitsProperties.AddAdditionalValidation(new PropertyValidation("^(.*?)$", "invalid_lastFourDigits", "Invalid card number"));
                        UpdatePageTokenChangeBasedOnCardType(retVal, type, language);
                    }
                    else if (type == Constants.PaymentMethodTypeNames.Paypal)
                    {
                        var dataDescription_details_email = dataDescription_details?.FirstOrDefault()?.DataDescription["email"] as PropertyDescription;
                        dataDescription_details_email.AddTransformation(new Dictionary<string, PropertyTransformationInfo>()
                        {
                            { "forDisplay", nonEditablePiDisplayTransformation }
                        });
                    }
                    else if (family == Constants.PaymentMethodFamilyNames.MobileBillingNonSim)
                    {
                        var dataDescription_details_msisdn = dataDescription_details?.FirstOrDefault()?.DataDescription["msisdn"] as PropertyDescription;
                        dataDescription_details_msisdn.AddTransformation(new Dictionary<string, PropertyTransformationInfo>()
                        {
                            { "forDisplay", nonEditablePiDisplayTransformation }
                        });
                    }
                }
                else if (operation == Constants.PidlOperationTypes.Add && family == Constants.PaymentMethodFamilyNames.CreditCard && country.ToLower() == Constants.CountryCodes.India)
                {
                    foreach (PIDLResource pidl in retVal)
                    {
                        try
                        {
                            var indiaTokenConsentMessageHyperlinkAction = pidl.GetDisplayHintById(Constants.DisplayHintIds.IndiaTokenConsentMessageHyperlink).Action;
                            var redirectURL = indiaTokenConsentMessageHyperlinkAction.Context.ToString();
                            indiaTokenConsentMessageHyperlinkAction.ActionType = Constants.DisplayHintIds.MoveNext;
                            SetupRedirectWithIframe(pidl, redirectURL);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            else if ((exposedFlightFeatures?.Contains(Constants.PartnerFlightValues.PXEnableUpdateCCLogo) == true || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableUpdateCCLogo, country, setting))
                     && !string.IsNullOrEmpty(operation) && !string.IsNullOrEmpty(family) && !string.IsNullOrEmpty(type) && string.Equals(operation, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase) 
                     && string.Equals(family, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase) && Constants.PaymentInstrumentLogos.IsTypeSupported(type))
            {
                // For Update CC flow, Add the logo and make lastFourDigits as key so the logo gets displayed on the card number textbox
                PIDLResource resource = retVal?.FirstOrDefault();
                if (resource != null && resource.DataDescription != null && resource.DisplayPages != null)
                {
                    PropertyDescription dataDescription_details_lastFourDigitsProperties = null;

                    if (resource.DataDescription.ContainsKey(Constants.DataDescriptionVariableNames.Details))
                    {
                        var dataDescription_details = resource.DataDescription[Constants.DataDescriptionVariableNames.Details] as List<PIDLResource>;
                        if (dataDescription_details?.FirstOrDefault()?.DataDescription?.ContainsKey(Constants.DataDescriptionVariableNames.LastFourDigits) == true)
                        {
                            dataDescription_details_lastFourDigitsProperties = dataDescription_details?.FirstOrDefault()?.DataDescription[Constants.DataDescriptionVariableNames.LastFourDigits] as PropertyDescription;
                        }
                    }

                    if (dataDescription_details_lastFourDigitsProperties != null)
                    {
                        dataDescription_details_lastFourDigitsProperties.IsKey = true;
                        dataDescription_details_lastFourDigitsProperties.AddAdditionalValidation(new PropertyValidation("^(.*?)$", "invalid_lastFourDigits", "Invalid card number"));

                        bool? isLogoAvailable = resource.GetAllDisplayHints()?.Where(hint => hint is LogoDisplayHint)?.Any();
                        if (!isLogoAvailable.GetValueOrDefault())
                        {
                            string logoUrl = Constants.PaymentInstrumentLogos.GetLogoUrl(type);
                            if (logoUrl != null)
                            {
                                LogoDisplayHint updateCCLogo = new LogoDisplayHint()
                                {
                                    HintId = Constants.DisplayHintIds.UpdateCCLogo,
                                    SourceUrl = logoUrl,
                                    IsHidden = true
                                };

                                if (resource.DisplayPages.FirstOrDefault() != null && resource.DisplayPages.FirstOrDefault().Members != null)
                                {
                                    resource.DisplayPages.FirstOrDefault().Members.Add(updateCCLogo);
                                }
                            }
                        }
                    }
                }
            }

            if (string.Equals(partnerName, Constants.PartnerNames.Xbox, StringComparison.OrdinalIgnoreCase) && exposedFlightFeatures != null && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.XboxOOBE) && string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase) && retVal != null && retVal.Count > 0)
            {
                foreach (PIDLResource resource in retVal)
                {
                    PaymentSelectionHelper.SetMoveNextAction(resource);
                }
            }

            // If originalPartnerName is not null, then it means the partnerName is a template partner like defaulttemplate and we need to use the original partner name in the submit links like battlenet etc.
            var partnerNameForLinks = originalPartnerName ?? partnerName;
            Dictionary<string, RestLink> links;
            if (string.Equals(operation, Constants.PidlOperationTypes.Apply, StringComparison.OrdinalIgnoreCase))
            {
                links = GetPIApplyLink(Constants.RestResourceNames.PaymentInstrumentsEx, country, language, operation, partnerNameForLinks, exposedFlightFeatures, channel, referrerId, sessionId);
            }
            else
            {
                links = GetPISubmitLink(Constants.RestResourceNames.PaymentInstrumentsEx, country, Context.Culture.Name, operation, partnerNameForLinks, classicProduct, billableAccountId, completePrerequisites, scenario, exposedFlightFeatures, orderId, pxChallengeSessionId);
            }

            AddSubmitLinks(links, retVal);

            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName)
                && family == Constants.PaymentMethodFamilyNames.CreditCard
                && (type != Constants.PaymentMethodTypeNames.CupCreditCard && type != Constants.PaymentMethodTypeNames.CupDebitCard)
                && (exposedFlightFeatures != null && !(bool)exposedFlightFeatures?.Contains(Flighting.Features.ShowSummaryPage, StringComparer.OrdinalIgnoreCase)))
            {
                // Xboxsettings partners should not hit the summary page. Replaced 2nd page MoveNext action 3rd page submit action
                retVal = RewireXboxNativeAddressPageSaveAction(operation, retVal);
            }

            if ((string.Equals(partnerName, Constants.PartnerNames.Commercialstores, StringComparison.OrdinalIgnoreCase)
                    || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableVirtualFamilyPM, country, setting))
                && string.Equals(family, Constants.PaymentMethodFamilyNames.Virtual, StringComparison.OrdinalIgnoreCase)
                && (string.Equals(type, Constants.PaymentMethodTypeNames.InvoiceBasicVirtual, StringComparison.OrdinalIgnoreCase) || string.Equals(type, Constants.PaymentMethodTypeNames.InvoiceCheckVirtual, StringComparison.OrdinalIgnoreCase)))
            {
                AddAddressValidationThenSuccessWithPayloadLinks(retVal, Constants.AddressTypes.SoldTo, partnerName, language, country, exposedFlightFeatures);
            }

            return retVal;
        }

        public List<PIDLResource> GetAddressDescriptions(
            string country,
            string type,
            string language,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            string displayDescriptionId = null,
            bool overrideJarvisVersionToV3 = false,
            string scenario = null,
            List<string> exposedFlightFeatures = null,
            string operation = null,
            bool avsSuggest = false,
            bool setAsDefaultBilling = false,
            PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            type = Helper.TryToLower(type);
            language = Helper.TryToLower(language);

            // Validate input parameter
            ValidateAddressType(type);
            ValidateCountry(country);
            Context.Culture = Helper.GetCultureInfo(language);
            Context.Country = country;

            var types = new List<string>() { type };

            // Enable the tempalte partner check, to sync with the commercialstores partner.
            bool hasAddHapiSUADisabledTaxResourceIdFeature = PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.AddHapiSUADisabledTaxResourceId, country, setting);
            if ((string.Equals(partnerName, Constants.PartnerNames.Commercialstores, StringComparison.OrdinalIgnoreCase)
                || hasAddHapiSUADisabledTaxResourceIdFeature)
                && string.Equals(type, Constants.AddressTypes.HapiServiceUsageAddress, StringComparison.OrdinalIgnoreCase)
                && Constants.AllCountriesEnabledTaxIdCheckbox.Contains(country))
            {
                types.Add(Constants.PidlResourceIdentities.HapiSUADisabledTax);
            }

            List<PIDLResource> retList = new List<PIDLResource>();
            foreach (string pidlType in types)
            {
                PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
                {
                    { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.AddressDescription },
                    { Constants.DescriptionIdentityFields.Type, pidlType },
                    { Constants.DescriptionIdentityFields.Country, country }
                });

                this.GetPIDLResourceRecursive(partnerName, Constants.DescriptionTypes.AddressDescription, pidlType, country, operation ?? GlobalConstants.Defaults.OperationKey, retVal, null, true, displayDescriptionId, Constants.HiddenOptionalFields.AddressDescriptionPropertyNames, null, scenario, exposedFlightFeatures, setting: setting);

                PIDLResourceFactory.AddSetAsDefaultBillingDataDescription(retVal, partnerName, avsSuggest, setAsDefaultBilling, scenario, setting);
                retList.Add(retVal);
            }

            if (exposedFlightFeatures?.Contains(Flighting.Features.PXEnableCSVSubmitLinks, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                this.ConstructRestLinks(retList, overrideJarvisVersionToV3, scenario, null, null);
            }
            else
            {
                // until we refactor submit links, we need this special case for xbox v3 shipping address defined in code because we don't want xbox to universally use v3 for completePrerequisites
                // same as xbox we do not want consumersupport to universally use v3 for completeprerequisites.
                if ((string.Equals(partnerName, Constants.PartnerNames.Xbox, StringComparison.OrdinalIgnoreCase) || string.Equals(partnerName, Constants.PartnerNames.ConsumerSupport, StringComparison.OrdinalIgnoreCase))
                    && string.Equals(type, "shipping", StringComparison.OrdinalIgnoreCase))
                {
                    overrideJarvisVersionToV3 = true;
                }
                else if ((string.Equals(partnerName, Constants.PartnerNames.Webblends, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partnerName, Constants.PartnerNames.OXOWebDirect, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partnerName, Constants.PartnerNames.OXODIME, StringComparison.OrdinalIgnoreCase))
                    && string.Equals(type, "shipping_v3", StringComparison.OrdinalIgnoreCase)
                    && (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXUseShippingV3ForCompletePrerequisites, StringComparer.OrdinalIgnoreCase)))
                {
                    overrideJarvisVersionToV3 = true;
                }

                var links = GetAddressSubmitLink(partnerName, type, overrideJarvisVersionToV3, avsSuggest, scenario, setting, country, exposedFlightFeatures);
                AddSubmitLinks(links, retList);
            }

            AddAddressValidationLinks(retList, partnerName, type, scenario, setting);
            AddAddressValidationThenSumbitLinks(retList, partnerName, type, operation, scenario, exposedFlightFeatures, setting);
            AddAddressValidationThenSuccessWithPayloadLinks(retList, type, partnerName, language, country, exposedFlightFeatures, setting);

            return retList;
        }

        public List<PIDLResource> GetChallengeDescriptions(string type, string language, string partnerName = Constants.PidlConfig.DefaultPartnerName, PaymentExperienceSetting setting = null, List<string> exposedFlightFeatures = null)
        {
            type = Helper.TryToLower(type);
            language = Helper.TryToLower(language);

            // Validate input parameter
            this.ValidateChallengeType(type);

            // Set context
            Context.Culture = Helper.GetCultureInfo(language);

            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
                {
                    { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.ChallengeDescription },
                    { Constants.DescriptionIdentityFields.Type, type }
                });
            this.GetPIDLResourceRecursive(partnerName, Constants.DescriptionTypes.ChallengeDescription, type, GlobalConstants.Defaults.CountryKey, GlobalConstants.Defaults.OperationKey, retVal, setting: setting, flightNames: exposedFlightFeatures);
            List<PIDLResource> retList = new List<PIDLResource>();
            retList.Add(retVal);
            return retList;
        }

        /// <summary>
        /// Returns the challenge PIDL of the given type and customized to the given pi
        /// </summary>
        /// <param name="pi">Payment instrument where action needs to be taken</param>
        /// <param name="type">Name of the challenge that needs to be completed</param>
        /// <param name="language">Code of language for PIDL localization</param>
        /// <param name="partnerName">Partner name</param>
        /// <param name="sessionId">Identity of the user's purchase session</param>
        /// <param name="scenario">The name of the pidl scenario</param>
        /// <param name="classicProduct">classicProduct passed by partner</param>
        /// <param name="session">payment session created</param>
        /// <param name="emailAddress">user profile email address</param>
        /// <param name="exposedFlightFeatures">Enabled Flights</param>
        /// <param name="setting">Partner settings from PSS</param>
        /// <returns>Returns challenge PIDL for given type and pi</returns>
        public List<PIDLResource> GetChallengeDescriptionsForPi(
            PaymentInstrument pi,
            string type,
            string language,
            string partnerName,
            string sessionId,
            string scenario = null,
            string classicProduct = null,
            object session = null,
            string emailAddress = null,
            List<string> exposedFlightFeatures = null,
            PaymentExperienceSetting setting = null)
        {
            type = Helper.TryToLower(type);
            language = Helper.TryToLower(language);

            // Validate input parameter
            this.ValidateChallengeType(type);
            ValidatePiAndChallengeType(type, pi);

            // Set context
            Context.Culture = Helper.GetCultureInfo(language);

            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.ChallengeDescription },
                { Constants.DescriptionIdentityFields.Type, type },
            });

            string pidlOperationType = Constants.PidlOperationTypes.Purchase;

            if (string.Equals(type, Constants.ChallengeDescriptionTypes.Sms, StringComparison.OrdinalIgnoreCase) &&
                (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName)
                    || PXCommon.Constants.PartnerGroups.IsWindowsNativePartner(partnerName)
                    || (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableSMSChallengeValidation, StringComparer.OrdinalIgnoreCase))))
            {
                // Change operation type so "otp" instead of "pin" is listed in the data description
                pidlOperationType = Constants.PidlOperationTypes.ValidatePurchase;

                retVal.DataDescription[Constants.DescriptionIdentityFields.SessionId] = new PropertyDescription()
                {
                    PropertyType = "userData",
                    DataType = "hidden",
                    PropertyDescriptionType = "hidden",
                    IsUpdatable = false,
                    DefaultValue = sessionId
                };

                PIDLResource xboxSettingsSmsValidateRetVal = new PIDLResource(new Dictionary<string, string>()
                {
                    { Constants.DescriptionIdentityFields.SessionId, sessionId }
                });

                xboxSettingsSmsValidateRetVal.Identity.ToList().ForEach(x => retVal.Identity.Add(x.Key, x.Value));
            }

            if (!string.IsNullOrEmpty(emailAddress))
            {
                Context.EmailAddress = emailAddress;
            }

            this.GetPIDLResourceRecursive(partnerName, Constants.DescriptionTypes.ChallengeDescription, type, GlobalConstants.Defaults.CountryKey, pidlOperationType, retVal, scenario: scenario, setting: setting, flightNames: exposedFlightFeatures);

            var expiryPrefix = LocalizationRepository.Instance.GetLocalizedString(Constants.ExpiryPrefixes.Exp, language);
            ChallengeDisplayHelper.PopulateChallengePidl(retVal, pi, type, partnerName, language, sessionId, session, expiryPrefix, scenario, exposedFlightFeatures, setting);

            List<PIDLResource> retList = new List<PIDLResource>
            {
                retVal
            };

            var links = GetCvv3DSSubmitLink(sessionId, partnerName, scenario, classicProduct);

            // Add the pifd link on the submit button to validate sms challenges for xbox settings and windows settings partners 
            if (string.Equals(type, Constants.ChallengeDescriptionTypes.Sms, StringComparison.OrdinalIgnoreCase) &&
                (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName)
                    || PXCommon.Constants.PartnerGroups.IsWindowsNativePartner(partnerName)
                    || (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableSMSChallengeValidation, StringComparer.OrdinalIgnoreCase))))
            {
                List<KeyValuePair<string, RestLink>> smsValidationSubmitLink = GetSmsValidationSubmitLink(sessionId, pi).ToList();
                smsValidationSubmitLink.ForEach(pair => links.Add(pair.Key, pair.Value));
            }

            AddSubmitLinks(links, retList);
            return retList;
        }

        public List<PIDLResource> GetSmsChallengeDescriptionForPI(
            PaymentInstrument paymentInstrument,
            string language,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            string classicProduct = null,
            string billableAccountId = null,
            string emailAddress = null,
            bool completePrerequisites = false,
            string country = null,
            PaymentExperienceSetting setting = null,
            List<string> exposedFlightFeatures = null)
        {
            if (!string.IsNullOrEmpty(emailAddress))
            {
                Context.EmailAddress = emailAddress;
            }

            List<PIDLResource> retList = this.GetChallengeDescriptions(Constants.ChallengeDescriptionTypes.Sms, language, partnerName, setting, exposedFlightFeatures);
            PIDLResource retVal = retList[0];

            if (retVal.DisplayPages == null)
            {
                return retList;
            }

            var challengeText = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<TextDisplayHint>(Constants.DisplayHintIds.SmsChallengeText, retVal.DisplayPages);

            if (challengeText != null)
            {
                challengeText.DisplayContent = string.Format(challengeText.DisplayContent, ChallengeDisplayHelper.GetPhoneNumberFromPi(paymentInstrument));
            }

            var links = GetPIResumeSubmitLink(Constants.RestResourceNames.PaymentInstrumentsEx, language, partnerName, classicProduct, billableAccountId, paymentInstrument.PaymentInstrumentId, completePrerequisites, country);
            AddSubmitLinks(links, retList);
            return retList;
        }

        public List<PIDLResource> GetSmsChallengeDescriptionForDeviceBinding(
            string ntsId,
            string language,
            string challengeId,
            string challengeMethodId,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            string country = null,
            PaymentExperienceSetting setting = null,
            List<string> exposedFlightFeatures = null)
        {
            List<PIDLResource> retList = this.GetChallengeDescriptions(Constants.ChallengeDescriptionTypes.TokensSms, language, partnerName, setting, exposedFlightFeatures);
            var links = GetDeviceBindingSubmitLink(ntsId, challengeId, partnerName, country, language, challengeMethodId);
            AddSubmitLinks(links, retList);
            return retList;
        }

        public List<PIDLResource> GetQrCodeChallengeDescriptionForThreeDSOnePurchase(
            string rdsUrl,
            string challengeDescriptionType,
            string language,
            string country,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            string sessionId = null,
            object session = null)
        {
            var qrCodePageIndex = 1;

            List<PIDLResource> retList = this.GetChallengeDescriptions(challengeDescriptionType, language, partnerName);
            PIDLResource retVal = retList[0];

            string rdsUrlWithRedirects = rdsUrl + string.Format(Constants.RedirectUrlStaticRuRx.ThreeDSOnePurchaseRedirectUrlRuRx, language);

            if (retVal.DisplayPages == null)
            {
                return retList;
            }

            if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.ThreeDSOneQrCode, StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(partnerName, Constants.PartnerNames.Xbox, StringComparison.OrdinalIgnoreCase) || string.Equals(partnerName, Constants.PartnerNames.AmcXbox, StringComparison.OrdinalIgnoreCase))
                {
                    // setup bank webview
                    var bankWebviewPage = retVal.DisplayPages[2];
                    IFrameDisplayHint bankWebview = new IFrameDisplayHint
                    {
                        HintId = Constants.DisplayHintIds.ThreeDSOneBankFrame,
                        SourceUrl = rdsUrlWithRedirects
                    };
                    bankWebviewPage.Members.Insert(0, bankWebview);

                    qrCodePageIndex = 1;
                }
                else if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                {
                    // setup bank webview
                    // xboxnative partners will no longer have a viewTermWebviewPage and will only have 3 pages
                    var bankWebviewPage = retVal.DisplayPages[2];
                    IFrameDisplayHint bankWebview = new IFrameDisplayHint
                    {
                        HintId = Constants.DisplayHintIds.ThreeDSOneBankFrame,
                        SourceUrl = rdsUrlWithRedirects
                    };
                    bankWebviewPage.Members.Insert(0, bankWebview);

                    qrCodePageIndex = 2;

                    // Purchase flow has 4 page pidl, with page 2 being a view terms iframe
                    // If you selects next button on page 1, skip over the iframe and move to page 3
                    var instructionsNextButton = retVal.GetDisplayHintById(Constants.DisplayHintIds.MoveNext2Button);
                    if (instructionsNextButton != null)
                    {
                        instructionsNextButton.Action.NextAction = new DisplayHintAction("moveNext");
                    }

                    var qrCodePageBackButton = retVal.GetDisplayHintById(Constants.DisplayHintIds.MoveBack2Button);
                    if (qrCodePageBackButton != null)
                    {
                        qrCodePageBackButton.Action.NextAction = new DisplayHintAction("movePrevious");
                    }
                }

                // Attach payment session to back button so if user backs out of flow, payment session is returned
                // in callback
                var backButton = retVal.GetDisplayHintById(Constants.DisplayHintIds.SuccessBackButton);
                if (backButton != null)
                {
                    backButton.Action.Context = session;
                }

                ImageDisplayHint qrCodeImage = retVal.GetDisplayHintById(Constants.DisplayHintIds.CCThreeDSQrCodeChallengeImage) as ImageDisplayHint;
                if (qrCodeImage != null)
                {
                    var qrCodeUrl = GetUrlQrCodeImage(rdsUrlWithRedirects);
                    qrCodeImage.SourceUrl = qrCodeUrl;
                }

                // Purchase QR Code Polling
                var qrCodePollingPage = retVal.DisplayPages[qrCodePageIndex];
                if (qrCodePollingPage != null)
                {
                    qrCodePollingPage.Action = Build3DSOnePollingAction(sessionId, partnerName);
                }

                ChallengeDisplayHelper.Populate3DS1XboxNativeQrCodeAccessibilityLabels(retList[0], language);
            }

            return retList;
        }

        public List<PIDLResource> GetKakaopayChallengeDescriptionForPI(
            PaymentInstrument paymentInstrument,
            string language,
            string challengeDescriptionType,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            string classicProduct = null,
            string billableAccountId = null,
            string emailAddress = null,
            bool completePrerequisites = false,
            string country = null,
            List<string> exposedFlightFeatures = null,
            string sessionId = null,
            string scenario = null,
            string orderId = null)
        {
            if (!string.IsNullOrEmpty(emailAddress))
            {
                Context.EmailAddress = emailAddress;
            }

            List<PIDLResource> retList = this.GetChallengeDescriptions(challengeDescriptionType, language, partnerName);
            PIDLResource retVal = retList[0];

            if (retVal.DisplayPages == null)
            {
                return retList;
            }

            // Add additional parameters to the url for telemetry purposes
            string redirectURL = paymentInstrument.PaymentInstrumentDetails.RedirectUrl + string.Format(Constants.RedirectUrlStaticRuRx.KakaopayQrcodeChallengeXboxNativeRedirectUrlRuRx, language, paymentInstrument.PaymentInstrumentId, "webPage");

            // Kakaopay iframe page will have a heading, so this iframe should be the second element in the PIDL
            SetupRedirectWithIframe(retVal, redirectURL, 1);
            AddGenericPollActionContext(retList[0], Constants.RestResourceNames.PaymentInstrumentsEx, language, partnerName, paymentInstrument.PaymentInstrumentId, country);

            return retList;
        }

        public List<PIDLResource> GetEditPhoneQRCodeDescriptions(
            string language,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retList = this.GetChallengeDescriptions(Constants.ChallengeDescriptionTypes.RewardsPhoneNumberQrCode, language, partnerName, setting: setting);
            PIDLResource retVal = retList[0];
            if (retVal.DisplayPages == null)
            {
                return retList;
            }

            AddQrCodeImageToDisplayHint(retVal, Constants.DisplayHintIds.RewardsEditPhoneNumberQrCodeImage, Constants.PidlUrlConstants.EditPhoneMSAPath);
            return retList;
        }

        [SuppressMessage("Microsoft.Performance", "CA1809", Justification = "Excess locals warning irrelevant")]
        public List<PIDLResource> GetQrCodeChallengeDescriptionForPI(
            PaymentInstrument paymentInstrument,
            string language,
            string challengeDescriptionType,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            string classicProduct = null,
            string billableAccountId = null,
            string emailAddress = null,
            bool completePrerequisites = false,
            string country = null,
            List<string> exposedFlightFeatures = null,
            string sessionId = null,
            string scenario = null,
            string orderId = null,
            string shortUrl = null,
            PaymentExperienceSetting setting = null,
            PIDLGeneratorContext context = null,
            string accountId = null,
            string originalPartner = null)
        {
            if (!string.IsNullOrEmpty(emailAddress))
            {
                Context.EmailAddress = emailAddress;
            }

            List<PIDLResource> retList = this.GetChallengeDescriptions(challengeDescriptionType, language, partnerName, setting: setting, exposedFlightFeatures: exposedFlightFeatures);
            PIDLResource retVal = retList[0];

            if (retVal.DisplayPages == null)
            {
                return retList;
            }

            string qrCodeChallengeImageURL = null;
            string qrCodeChallengeImageDisplayHintId = null;
            string redirectURL = null;
            string redirectURLDisplayHintId = null;
            string secondQrCodeChallengeImageDisplayHintId = null;
            string qrCodeRedirectButton = null;
            string sessionQueryUrl = null;
            bool showRedirectURLAsHyperLink;
            bool showRedirectURLAsButton = false;
            bool showRedirectURLInIframe = false;
            string[] qrCodeChallengeImageStyleHints = null;

            if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.AlipayQrCode, StringComparison.OrdinalIgnoreCase))
            {
                qrCodeChallengeImageURL = paymentInstrument.PaymentInstrumentDetails.AppSignUrl;
                qrCodeChallengeImageDisplayHintId = Constants.DisplayHintIds.AlipayQrCodeChallengeImage;
                qrCodeChallengeImageStyleHints = new string[] { "image-large" };
                redirectURL = paymentInstrument.PaymentInstrumentDetails.RedirectUrl;
                redirectURLDisplayHintId = Constants.DisplayHintIds.AlipayQrCodeChallengeRedirectionLink;
                showRedirectURLAsHyperLink = true;
            }
            else if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.PaypalQrCode, StringComparison.OrdinalIgnoreCase))
            {
                // TODO: remove the static RuRx when we can get the redirect Url with desired RuRx for paypal QrCode from PIMS
                qrCodeChallengeImageURL = paymentInstrument.PaymentInstrumentDetails.RedirectUrl + string.Format(Constants.RedirectUrlStaticRuRx.PaypalQrcodeChallengeRedirectUrlRuRx, language, paymentInstrument.PaymentInstrumentId);
                qrCodeChallengeImageDisplayHintId = Constants.DisplayHintIds.PaypalQrCodeChallengeImage;
                qrCodeChallengeImageStyleHints = new string[] { "image-large" };
                redirectURL = GetRedirectURL(paymentInstrument, language, challengeDescriptionType, partnerName: partnerName, setting: setting);
                redirectURLDisplayHintId = Constants.DisplayHintIds.PaypalQrCodeChallengeURLText;
                showRedirectURLAsHyperLink = false;

                if (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ShowRedirectURLInIframe, country, setting))
                {
                    PayPalQRCodeAddIframePage(retVal, language);
                }

                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) ||
                    PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ShowRedirectURLInIframe, country, setting))
                {
                    // Add additional parameters to the url for telemetry purposes
                    showRedirectURLInIframe = true;
                    qrCodeChallengeImageURL = paymentInstrument.PaymentInstrumentDetails.RedirectUrl + string.Format(Constants.RedirectUrlStaticRuRx.PaypalQrcodeChallengeXboxNativeRedirectUrlRuRx, language, paymentInstrument.PaymentInstrumentId, "qrCode");
                    redirectURL = paymentInstrument.PaymentInstrumentDetails.RedirectUrl + string.Format(Constants.RedirectUrlStaticRuRx.PaypalQrcodeChallengeXboxNativeRedirectUrlRuRx, language, paymentInstrument.PaymentInstrumentId, "webPage");
                }
                else if (Constants.PartnersToEnablePayPal2ndScreenRedirectButton.Contains(partnerName, StringComparer.OrdinalIgnoreCase))
                {
                    showRedirectURLAsButton = true;
                }
                else if (string.Equals(partnerName, Constants.PidlConfig.OXOWebDirectPartnerName, StringComparison.InvariantCultureIgnoreCase) || string.Equals(partnerName, Constants.PidlConfig.OXODIMEPartnerName, StringComparison.InvariantCultureIgnoreCase)
                    || IsTemplateInList(partnerName, setting, Constants.DescriptionTypes.PaymentMethodDescription, $"{paymentInstrument.PaymentMethod.PaymentMethodFamily}.{paymentInstrument.PaymentMethod.PaymentMethodType}"))
                {
                    var qrCodeChallengeLoginLinkDisplayHintId = Constants.DisplayHintIds.PaypalQrCodeChallengeLoginRedirectionLink;
                    var qrCodeChallengeLoginLink = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<HyperlinkDisplayHint>(qrCodeChallengeLoginLinkDisplayHintId, retVal.DisplayPages);
                    if (qrCodeChallengeLoginLink != null)
                    {
                        SetupRedirectWithHyperlink(retVal, paymentInstrument, redirectURL, qrCodeChallengeLoginLinkDisplayHintId);
                    }
                    else
                    {
                        throw new PIDLConfigException(
                                string.Format("No Display Hint found for Id \"{0}\"", qrCodeChallengeLoginLinkDisplayHintId),
                                Constants.ErrorCodes.PIDLConfigUnknownDisplayHintId);
                    }
                }

                qrCodeRedirectButton = Constants.DisplayHintIds.PaypalQrCodeRedirectButton;

                // if flight PXDisableTwoPagePidlForPaypal2ndScreenQrcodePage is on, remove the second displaypage, and change the backbutton action type to gohome
                if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXDisableTwoPagePidlForPaypal2ndScreenQrcodePage, StringComparer.OrdinalIgnoreCase))
                {
                    if (retVal.DisplayPages != null && retVal.DisplayPages.Count > 1)
                    {
                        retVal.DisplayPages.Remove(retVal.DisplayPages[1]);
                    }

                    var paypalQrCodeBackButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.PaypalQrCodeBackButton, retVal.DisplayPages);
                    if (paypalQrCodeBackButton != null && paypalQrCodeBackButton.Action != null)
                    {
                        paypalQrCodeBackButton.Action.ActionType = DisplayHintActionType.gohome.ToString();
                    }
                }
            }
            else if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.CreditCardQrCode, StringComparison.OrdinalIgnoreCase) && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.PxEnableAddCcQrCode))
            {
                qrCodeChallengeImageDisplayHintId = Constants.DisplayHintIds.AddCCQrCodeImage;
                qrCodeChallengeImageStyleHints = new string[] { "image-large" };
                redirectURLDisplayHintId = Constants.DisplayHintIds.AddCCQrCodeImage;
                showRedirectURLAsHyperLink = false;

                // TODO: Update once paymicrosoft is in prod
                ////string url = string.Format(Constants.SecondScreenQRCode.PayMicrosoftEndpoint, sessionId, country, language, partnerName, paymentInstrument.PaymentMethod.PaymentMethodType, paymentInstrument.PaymentMethod.PaymentMethodFamily);
                string url = string.Format(Constants.SecondScreenQRCode.PayMicrosoftPPE, sessionId, country, language, originalPartner, paymentInstrument.PaymentMethod.PaymentMethodType, paymentInstrument.PaymentMethod.PaymentMethodFamily, Constants.ScenarioNames.SecondScreenAddPi);
                qrCodeChallengeImageURL = url;
                redirectURL = url;
            }
            else if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.VenmoQrCode, StringComparison.OrdinalIgnoreCase))
            {
                showRedirectURLAsHyperLink = false;

                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) ||
                    PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ShowRedirectURLInIframe, country, setting))
                {
                    qrCodeChallengeImageDisplayHintId = Constants.DisplayHintIds.VenmoQrCodeChallengeImage;
                    redirectURLDisplayHintId = Constants.DisplayHintIds.VenmoQrCodeChallengeURLText;
                    showRedirectURLInIframe = true;

                    // TODO: Change to appropriate Venmo Redirect URL
                    qrCodeChallengeImageURL = GetRedirectURL(paymentInstrument, language, challengeDescriptionType, "qrCode");
                    redirectURL = GetRedirectURL(paymentInstrument, language, challengeDescriptionType, "webPage");
                }

                qrCodeRedirectButton = Constants.DisplayHintIds.VenmoQrCodeRedirectButton;
            }
            else if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.GlobalPIQrCode, StringComparison.OrdinalIgnoreCase))
            {
                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName)
                    || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ShowRedirectURLInIframe, country, setting))
                {
                    showRedirectURLInIframe = true;
                }

                string baseUrl = string.Format(Constants.HandleGlobalQrCodePIPendingPurchaseUrls.HandleGlobalPendingPurchaseQrCodeUrlTemplate, sessionId);
                qrCodeChallengeImageURL = baseUrl + string.Format(Constants.RedirectUrlStaticRuRx.GlobalPIQrcodeChallengeRedirectUrlRuRx, language, sessionId);
                qrCodeChallengeImageDisplayHintId = Constants.DisplayHintIds.GlobalPIQrCodeChallengeImage;
                redirectURL = baseUrl + string.Format(Constants.RedirectUrlStaticRuRx.GlobalPIQrcodeChallengeRedirectUrlRuRx, language, sessionId);
                showRedirectURLAsHyperLink = false;
                secondQrCodeChallengeImageDisplayHintId = Constants.DisplayHintIds.GlobalPIQrCodeChallengeSecondImage;
            }
            else if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.GenericQrCode, StringComparison.OrdinalIgnoreCase))
            {
                // TODO: remove the static RuRx when we can get the redirect Url with desired RuRx for paypal QrCode from PIMS
                qrCodeChallengeImageURL = paymentInstrument.PaymentInstrumentDetails.RedirectUrl + Constants.RedirectUrlStaticRuRx.GenericQrcodeChallengeRedirectUrlRuRx;
                qrCodeChallengeImageDisplayHintId = Constants.DisplayHintIds.GenericQrCodeChallengeImage;
                redirectURL = paymentInstrument.PaymentInstrumentDetails.RedirectUrl + Constants.RedirectUrlStaticRuRx.GenericQrcodeChallengeRedirectUrlRuRx;
                redirectURLDisplayHintId = Constants.DisplayHintIds.GenericQrCodeChallengeURLText;
                showRedirectURLAsHyperLink = false;
            }
            else if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.ThreeDSOneQrCode, StringComparison.OrdinalIgnoreCase))
            {
                showRedirectURLAsHyperLink = false;

                if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.XboxOOBE))
                {
                    // Xbox oobe flow does not support edge browser, remove redirect button
                    showRedirectURLInIframe = false;
                    showRedirectURLAsButton = false;
                    qrCodeRedirectButton = Constants.DisplayHintIds.GoToBankButton;
                }
                else if ((paymentInstrument.PaymentInstrumentDetails.IsFullPageRedirect == null || paymentInstrument.PaymentInstrumentDetails.IsFullPageRedirect == false)
                    || PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                {
                    // Xbox native partners render in environment without edge browser so must default to iframe
                    showRedirectURLInIframe = true;
                }
                else
                {
                    showRedirectURLAsButton = true;
                    qrCodeRedirectButton = Constants.DisplayHintIds.GoToBankButton;
                }

                qrCodeChallengeImageURL = paymentInstrument.PaymentInstrumentDetails.RedirectUrl + string.Format(Constants.RedirectUrlStaticRuRx.ThreeDSOneChallengeRedirectUrlRuRx, language, sessionId, paymentInstrument.PaymentMethod.PaymentMethodType, paymentInstrument.PaymentMethod.PaymentMethodFamily);
                redirectURL = qrCodeChallengeImageURL;

                // Add additional parameters to the url for telemetry purposes
                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                {
                    qrCodeChallengeImageURL = paymentInstrument.PaymentInstrumentDetails.RedirectUrl + string.Format(Constants.RedirectUrlStaticRuRx.ThreeDSOneChallengeXboxNativeRedirectUrlRuRx, language, sessionId, paymentInstrument.PaymentMethod.PaymentMethodType, paymentInstrument.PaymentMethod.PaymentMethodFamily, "qrCode");
                    redirectURL = paymentInstrument.PaymentInstrumentDetails.RedirectUrl + string.Format(Constants.RedirectUrlStaticRuRx.ThreeDSOneChallengeXboxNativeRedirectUrlRuRx, language, sessionId, paymentInstrument.PaymentMethod.PaymentMethodType, paymentInstrument.PaymentMethod.PaymentMethodFamily, "webPage");
                }

                qrCodeChallengeImageDisplayHintId = Constants.DisplayHintIds.CCThreeDSQrCodeChallengeImage;
                sessionQueryUrl = WebUtility.UrlEncode(paymentInstrument.PaymentInstrumentDetails.SessionQueryUrl);
            }
            else
            {
                throw new PIDLConfigException(
                    string.Format("No QR code challenge description for \"{0}\"", challengeDescriptionType),
                    Constants.ErrorCodes.PIDLArgumentChallengeTypeIsInvalid);
            }

            var qrCodeChallengeImage = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ImageDisplayHint>(qrCodeChallengeImageDisplayHintId, retVal.DisplayPages);
            if (qrCodeChallengeImage == null)
            {
                throw new PIDLConfigException(
                        string.Format("No Display Hint found for Id \"{0}\"", qrCodeChallengeImageDisplayHintId),
                        Constants.ErrorCodes.PIDLConfigUnknownDisplayHintId);
            }

            if (!string.IsNullOrEmpty(qrCodeChallengeImageURL))
            {
                string qrCodeChallengeImageSourceUrl = GetUrlQrCodeImage(qrCodeChallengeImageURL);
                qrCodeChallengeImage.SourceUrl = qrCodeChallengeImageSourceUrl;
                qrCodeChallengeImage.StyleHints = qrCodeChallengeImage.StyleHints ?? qrCodeChallengeImageStyleHints;

                if (!string.IsNullOrEmpty(secondQrCodeChallengeImageDisplayHintId))
                {
                    var qrCodeChallengeSecondImage = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ImageDisplayHint>(secondQrCodeChallengeImageDisplayHintId, retVal.DisplayPages);

                    if (qrCodeChallengeSecondImage == null)
                    {
                        throw new PIDLConfigException(
                                string.Format("No Display Hint found for Id \"{0}\"", secondQrCodeChallengeImageDisplayHintId),
                                Constants.ErrorCodes.PIDLConfigUnknownDisplayHintId);
                    }

                    qrCodeChallengeSecondImage.SourceUrl = qrCodeChallengeImageSourceUrl;
                }
            }

            if (showRedirectURLAsHyperLink)
            {
                SetupRedirectWithHyperlink(retVal, paymentInstrument, redirectURL, redirectURLDisplayHintId);
            }

            if (showRedirectURLAsButton)
            {
                SetupRedirectURLButton(retVal, paymentInstrument, partnerName, redirectURL, qrCodeRedirectButton, qrCodeChallengeImageURL, challengeDescriptionType);
            }
            else if (showRedirectURLInIframe)
            {
                SetupRedirectWithIframe(retVal, redirectURL);
            }
            else
            {
                if (qrCodeRedirectButton != null && string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.ThreeDSOneQrCode, StringComparison.OrdinalIgnoreCase) && (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.XboxOOBE)))
                {
                    PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<ButtonDisplayHint>(qrCodeRedirectButton, retVal.DisplayPages);
                    PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<GroupDisplayHint>(Constants.DisplayHintIds.CCThreeDSWebviewInstructionGroup, retVal.DisplayPages);
                    retVal.DisplayPages.RemoveAt(retVal.DisplayPages.Count - 1);
                }
                else if (qrCodeRedirectButton != null)
                {
                    PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<ButtonDisplayHint>(qrCodeRedirectButton, retVal.DisplayPages);
                    PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<TextDisplayHint>(Constants.DisplayHintIds.PaypalQrCodeUseBrowserText, retVal.DisplayPages);
                }
            }

            var links = GetPILink(Constants.RestResourceNames.PaymentInstrumentsEx, language, partnerName, paymentInstrument.PaymentInstrumentId, completePrerequisites, country);
            AddSubmitLinks(links, retList);

            string nonTemplatePartnerName = context?.OriginalPartner ?? partnerName;
            AddQrCodePollActionContext(challengeDescriptionType, retVal, Constants.RestResourceNames.PaymentInstrumentsEx, language, nonTemplatePartnerName, paymentInstrument.PaymentInstrumentId, completePrerequisites, country, exposedFlightFeatures, sessionId, scenario, orderId, sessionQueryUrl);
            return retList;
        }

        public List<PIDLResource> GetPicvChallengeDescriptionForPI(PaymentInstrument paymentInstrument, string type, string language, string partnerName = Constants.PidlConfig.DefaultPartnerName, string classicProduct = null, string billableAccountId = null, PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retList = this.GetChallengeDescriptions(type, language, partnerName, setting: setting);
            PIDLResource retVal = retList[0];

            if (retVal.DisplayPages == null)
            {
                return retList;
            }

            if (paymentInstrument == null || paymentInstrument.PaymentInstrumentDetails == null || paymentInstrument.PaymentInstrumentDetails.PicvDetails == null || string.IsNullOrEmpty(paymentInstrument.PaymentInstrumentDetails.PicvDetails.RemainingAttempts))
            {
                throw new PIDLArgumentException(
                    "Sepa Picv validation requires a valid retry time",
                    Constants.ErrorCodes.PIDLArgumentSepaPicvRetryTimeIsInvalid);
            }

            string retryTimes = paymentInstrument.PaymentInstrumentDetails.PicvDetails.RemainingAttempts;

            if (string.Equals(retryTimes, Constants.RetryCount.MinRetryCountOnPicvChallenge.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<TextDisplayHint>(Constants.DisplayHintIds.PicvRetryCount, retVal.DisplayPages);
            }
            else
            {
                PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<TextDisplayHint>(Constants.DisplayHintIds.PicvLastRetryCount, retVal.DisplayPages);
                var retryText = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<TextDisplayHint>(Constants.DisplayHintIds.PicvRetryCount, retVal.DisplayPages);

                if (retryText != null)
                {
                    retryText.DisplayContent = string.Format(retryText.DisplayContent, retryTimes);
                }
            }

            var links = GetPIResumeSubmitLink(Constants.RestResourceNames.PaymentInstrumentsEx, language, partnerName, classicProduct, billableAccountId, paymentInstrument.PaymentInstrumentId, false, null);
            AddSubmitLinks(links, retList);
            return retList;
        }

        public List<PIDLResource> GetUpdateAgreementChallengeDescriptionForPI(PaymentInstrument paymentInstrument, string type, string language, string partnerName = Constants.PidlConfig.DefaultPartnerName)
        {
            List<PIDLResource> retList = this.GetChallengeDescriptions(type, language, partnerName);
            PIDLResource retVal = retList[0];

            if (retVal.DisplayPages == null)
            {
                return retList;
            }

            var links = GetUpdateBillingAgreementTypeLink(Constants.RestResourceNames.PaymentInstruments, paymentInstrument.PaymentInstrumentId);
            AddSubmitLinks(links, retList);
            return retList;
        }

        public PIDLResource GetCc3DSStatusCheckDescriptionForPaymentSession(string sessionId, string language, string partnerName, string pxBrowserAuthenticateRedirectionUrl, PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> pidlList = this.GetStaticPidlDescriptions(Constants.StaticDescriptionTypes.Cc3DSStatusCheckPidl, language, partnerName, setting: setting);

            UpdateYesImDoneWithBankButtonWithSubmitLink(sessionId, pidlList);

            RedirectionServiceLink redirectionLink = new RedirectionServiceLink
            {
                BaseUrl = pxBrowserAuthenticateRedirectionUrl,
            };

            var retryRedirectionButtonDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.Cc3DSRetryButton, pidlList[0].DisplayPages);
            if (retryRedirectionButtonDisplayHint != null)
            {
                retryRedirectionButtonDisplayHint.Action.Context = redirectionLink;
                retryRedirectionButtonDisplayHint.Action.ActionType = Constants.ActionType.Redirect;
            }

            ClientAction action = new ClientAction(ClientActionType.Pidl)
            {
                Context = pidlList,
            };

            return new PIDLResource { ClientAction = action };
        }

        public PIDLResource GetCc3DSRedirectAndStatusCheckDescriptionForPaymentSession(string sessionId, string language, string partnerName, string redirectionUrl, PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> pidlList = this.GetStaticPidlDescriptions(Constants.StaticDescriptionTypes.Cc3DSRedirectAndStatusCheckPidl, language, partnerName, setting: setting);

            ButtonDisplayHint goToBankButton = pidlList.First().GetDisplayHintById(Constants.ButtonDisplayHintIds.Cc3DSGoToBankButton) as ButtonDisplayHint;
            RedirectionServiceLink redirectionLink = new RedirectionServiceLink
            {
                BaseUrl = redirectionUrl,
            };

            if (goToBankButton != null)
            {
                goToBankButton.Action.Context = redirectionLink;
            }

            var tryAgainRedirectionButtonDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.Cc3DSTryAgainButton, pidlList[0].DisplayPages);
            if (tryAgainRedirectionButtonDisplayHint != null)
            {
                tryAgainRedirectionButtonDisplayHint.Action.Context = redirectionLink;
                tryAgainRedirectionButtonDisplayHint.Action.ActionType = Constants.ActionType.Redirect;
            }

            var retryRedirectionButtonDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.Cc3DSRetryButton, pidlList[0].DisplayPages);
            if (retryRedirectionButtonDisplayHint != null)
            {
                retryRedirectionButtonDisplayHint.Action.Context = redirectionLink;
                retryRedirectionButtonDisplayHint.Action.ActionType = Constants.ActionType.Redirect;
            }

            UpdateYesImDoneWithBankButtonWithSubmitLink(sessionId, pidlList);

            RestLink pollUrl = GetPaymentSessionPollingLink(sessionId);

            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());
            PollActionContext pollActionContext = new PollActionContext()
            {
                Href = pollUrl.Href,
                Method = Constants.HTTPVerbs.GET,
                Interval = Constants.PollingIntervals.GenericPollingInterval,
                CheckPollingTimeOut = false,
                ResponseResultExpression = Constants.PollingResponseResultExpression.ThreeDSOnePurchaseResponseResultExpression,
            };

            pollActionContext.AddResponseActionsItem("Succeeded", new DisplayHintAction(DisplayHintActionType.success.ToString()));
            pollActionContext.AddResponseActionsItem("Failed", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
            pollActionContext.AddResponseActionsItem("TimedOut", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
            pollActionContext.AddResponseActionsItem("Cancelled", new DisplayHintAction(DisplayHintActionType.gohome.ToString()));
            pollActionContext.AddResponseActionsItem("InternalServerError", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));

            pollAction.Context = pollActionContext;

            // Yes I am done with bank verification button is on page 2
            pidlList[0].DisplayPages[1].Action = pollAction;

            ClientAction action = new ClientAction(ClientActionType.Pidl)
            {
                Context = pidlList,
            };

            return new PIDLResource { ClientAction = action };
        }

        public List<PIDLResource> GetCc3DSRedirectAndStatusCheckDescriptionForPI(
            PaymentInstrument paymentInstrument,
            string language,
            string partnerName,
            string scenario,
            string classicProduct,
            bool completePrerequisites,
            string country,
            bool enablePolling = false,
            PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retList = this.GetStaticPidlDescriptions(Constants.StaticDescriptionTypes.Cc3DSRedirectAndStatusCheckPidl, language, partnerName, setting: setting);

            RedirectionServiceLink redirectionLink = new RedirectionServiceLink()
            {
                BaseUrl = paymentInstrument.PaymentInstrumentDetails.RedirectUrl
            };

            redirectionLink.RuParameters.Add("family", paymentInstrument.PaymentMethod.PaymentMethodFamily);
            redirectionLink.RuParameters.Add("type", paymentInstrument.PaymentMethod.PaymentMethodType);
            redirectionLink.RuParameters.Add("pendingOn", paymentInstrument.PaymentInstrumentDetails.PendingOn);

            foreach (var pidlResource in retList)
            {
                var goToBankButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.ButtonDisplayHintIds.Cc3DSGoToBankButton, pidlResource.DisplayPages);
                if (goToBankButton != null)
                {
                    goToBankButton.Action.Context = redirectionLink;
                }

                var hyperlinkRedirectionHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<HyperlinkDisplayHint>(Constants.DisplayHintIds.Cc3DSRedirectLink, retList[0].DisplayPages);
                if (hyperlinkRedirectionHint != null)
                {
                    hyperlinkRedirectionHint.Action.Context = redirectionLink;
                }

                var tryAgainRedirectionButtonDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.Cc3DSTryAgainButton, retList[0].DisplayPages);
                if (tryAgainRedirectionButtonDisplayHint != null)
                {
                    tryAgainRedirectionButtonDisplayHint.Action.Context = redirectionLink;
                    tryAgainRedirectionButtonDisplayHint.Action.ActionType = Constants.ActionType.Redirect;
                }

                var retryRedirectionButtonDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.Cc3DSRetryButton, retList[0].DisplayPages);
                if (retryRedirectionButtonDisplayHint != null)
                {
                    retryRedirectionButtonDisplayHint.Action.Context = redirectionLink;
                    retryRedirectionButtonDisplayHint.Action.ActionType = Constants.ActionType.Redirect;
                }
            }

            var links = GetPILink(Constants.RestResourceNames.PaymentInstrumentsEx, language, partnerName, paymentInstrument.PaymentInstrumentId, completePrerequisites, country, scenario, paymentInstrument.PaymentInstrumentDetails.SessionQueryUrl, classicProduct);
            AddSubmitLinks(links, retList);

            if (enablePolling)
            {
                var pollLinks = GetPILink(Constants.RestResourceNames.PaymentInstrumentsEx, language, partnerName, paymentInstrument.PaymentInstrumentId, completePrerequisites, country, Constants.ScenarioNames.ThreeDSOnePolling, paymentInstrument.PaymentInstrumentDetails.SessionQueryUrl, classicProduct);
                string pollUrl = pollLinks[Constants.ButtonDisplayHintIds.Cc3DSYesButton].Href;
                DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());
                PollActionContext pollActionContext = new PollActionContext()
                {
                    Href = pollUrl,
                    Method = Constants.HTTPVerbs.GET,
                    Interval = Constants.PollingIntervals.GenericPollingInterval,
                    CheckPollingTimeOut = false,
                    ResponseResultExpression = Constants.PollingResponseResultExpression.ThreeDSOneResponseResultExpression,
                };
                pollActionContext.AddResponseActionsItem("Active", new DisplayHintAction(DisplayHintActionType.success.ToString()));
                pollActionContext.AddResponseActionsItem("Declined", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
                pollAction.Context = pollActionContext;
                retList[0].DisplayPages[0].Action = pollAction;
            }

            return retList;
        }

        public List<PIDLResource> GetUPIRedirectAndStatusCheckDescriptionForPI(
           PaymentInstrument paymentInstrument,
           string language,
           string partnerName,
           string sessionId,
           string challengeType,
           PaymentExperienceSetting setting = null,
           string orderId = null,
           List<string> flights = null)
        {
            List<PIDLResource> retList = this.GetChallengeDescriptions(challengeType, language, partnerName, setting: setting);

            RedirectionServiceLink redirectionLink = new RedirectionServiceLink()
            {
                BaseUrl = string.Format(Constants.ConfirmPaymentForUPIUrls.HandleRedirectUrlTemplate, sessionId)
            };

            foreach (var pidlResource in retList)
            {
                var goToBankButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.ButtonDisplayHintIds.UPIGoToBankButton, pidlResource.DisplayPages);
                if (goToBankButton != null)
                {
                    goToBankButton.Action.Context = redirectionLink;
                }

                var tryAgainRedirectionButtonDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.UPITryAgainButton, retList[0].DisplayPages);
                if (tryAgainRedirectionButtonDisplayHint != null)
                {
                    tryAgainRedirectionButtonDisplayHint.Action.Context = redirectionLink;
                    tryAgainRedirectionButtonDisplayHint.Action.ActionType = Constants.ActionType.Redirect;
                }

                var yesButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.ButtonDisplayHintIds.UPIYesVerificationButton, retList[0].DisplayPages);
                if (yesButton != null)
                {
                    RestLink submitUrlLink = UrlForPurchaseConfrimButton(sessionId, flights, orderId);

                    yesButton.Action.Context = submitUrlLink;
                    yesButton.Action.ActionType = Constants.ActionType.Submit;
                }
            }

            // Polling
            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());
            var upiPollingPage = retList[0].DisplayPages[0];
            if (upiPollingPage != null)
            {
                string pollUrl = string.Format(Constants.ConfirmPaymentForUPIUrls.QueryUPIPendingStateRedirectionServicePollUrlTemplate, sessionId);
                PollActionContext pollActionContex = new PollActionContext()
                {
                    Href = pollUrl,
                    Method = Constants.HTTPVerbs.GET,
                    Interval = Constants.PollingIntervals.GlobalPollingInterval,
                    ResponseResultExpression = Constants.PollingResponseResultExpression.GlobalPIResponseKeyExpressionForRedirectionService,
                    CheckPollingTimeOut = false,
                };

                pollActionContex.AddResponseActionsItem("pending", new DisplayHintAction(DisplayHintActionType.updatePoll.ToString()));
                pollActionContex.AddResponseActionsItem("failure", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
                pollAction.Context = pollActionContex;
                upiPollingPage.Action = pollAction;
                pollAction.NextAction = AddUPISecondPollActionContextConfirmPayment(sessionId, orderId, paymentInstrument.PaymentInstrumentId, flights);
            }

            return retList;
        }

        public List<PIDLResource> GetCc3DSStatusCheckDescriptionForPI(
            PaymentInstrument paymentInstrument,
            string language,
            string partnerName,
            string scenario,
            string classicProduct,
            bool completePrerequisites,
            string country,
            string sessionQueryUrl,
            PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retList = this.GetStaticPidlDescriptions(Constants.StaticDescriptionTypes.Cc3DSStatusCheckPidl, language, partnerName, setting: setting);

            RedirectionServiceLink redirectionLink = new RedirectionServiceLink()
            {
                // PI returned from PIMS GetPI currently doesn't include RedirectUrl, so need to construct redirectUrl by using sessionid in sessionQueryUrl
                BaseUrl = ConstructRedirectURLFromSessionQueryUrl(sessionQueryUrl)
            };

            redirectionLink.RuParameters.Add("family", paymentInstrument.PaymentMethod.PaymentMethodFamily);
            redirectionLink.RuParameters.Add("type", paymentInstrument.PaymentMethod.PaymentMethodType);
            redirectionLink.RuParameters.Add("pendingOn", paymentInstrument.PaymentInstrumentDetails.PendingOn);

            foreach (var pidlResource in retList)
            {
                var hyperlinkRedirectionHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<HyperlinkDisplayHint>(Constants.DisplayHintIds.Cc3DSRedirectLink, retList[0].DisplayPages);
                if (hyperlinkRedirectionHint != null)
                {
                    hyperlinkRedirectionHint.Action.Context = redirectionLink;
                }

                var tryAgainRedirectionButtonDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.Cc3DSTryAgainButton, retList[0].DisplayPages);
                if (tryAgainRedirectionButtonDisplayHint != null)
                {
                    tryAgainRedirectionButtonDisplayHint.Action.Context = redirectionLink;
                    tryAgainRedirectionButtonDisplayHint.Action.ActionType = Constants.ActionType.Redirect;
                }

                var retryRedirectionButtonDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.Cc3DSRetryButton, retList[0].DisplayPages);
                if (retryRedirectionButtonDisplayHint != null)
                {
                    retryRedirectionButtonDisplayHint.Action.Context = redirectionLink;
                    retryRedirectionButtonDisplayHint.Action.ActionType = Constants.ActionType.Redirect;
                }
            }

            var links = GetPILink(Constants.RestResourceNames.PaymentInstrumentsEx, language, partnerName, paymentInstrument.PaymentInstrumentId, completePrerequisites, country, scenario, sessionQueryUrl, classicProduct);
            AddSubmitLinks(links, retList);
            return retList;
        }

        public List<PIDLResource> Get3DSRedirectAndStatusCheckDescriptionForPaymentAuth(
                    string redirectUrl,
                    string rdsSessionId,
                    string paymentSessionId,
                    string partnerName,
                    string language,
                    string country,
                    string resourceType,
                    string scenario = null,
                    string paymentMethodFamilyTypeId = null,
                    PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retList = this.GetStaticPidlDescriptions(resourceType, language, partnerName, setting: setting);

            foreach (var pidlResource in retList)
            {
                RedirectionServiceLink redirectionLink = new RedirectionServiceLink()
                {
                    BaseUrl = redirectUrl
                };

                var goToBankButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.ButtonDisplayHintIds.Cc3DSGoToBankButton, pidlResource.DisplayPages);
                if (goToBankButton != null)
                {
                    goToBankButton.Action.Context = redirectionLink;
                }

                var legacyBillDeskGoToBankButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.ButtonDisplayHintIds.LegacyBillDesk3DSGoToBankButton, pidlResource.DisplayPages);
                if (legacyBillDeskGoToBankButton != null)
                {
                    legacyBillDeskGoToBankButton.Action.Context = redirectionLink;
                }

                var tryAgainRedirectionButtonDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.Cc3DSTryAgainButton, pidlResource.DisplayPages);
                if (tryAgainRedirectionButtonDisplayHint != null)
                {
                    tryAgainRedirectionButtonDisplayHint.Action.Context = redirectionLink;
                    tryAgainRedirectionButtonDisplayHint.Action.ActionType = Constants.ActionType.Redirect;
                }

                var retryRedirectionButtonDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.Cc3DSRetryButton, pidlResource.DisplayPages);
                if (retryRedirectionButtonDisplayHint != null)
                {
                    retryRedirectionButtonDisplayHint.Action.Context = redirectionLink;
                    retryRedirectionButtonDisplayHint.Action.ActionType = Constants.ActionType.Redirect;
                }
            }

            var links = GetRDSSessionStatusLink(rdsSessionId, paymentSessionId, partnerName, country, language, scenario, paymentMethodFamilyTypeId);
            AddSubmitLinks(links, retList);
            if (string.Equals(paymentMethodFamilyTypeId, GlobalConstants.PaymentMethodFamilyTypeIds.EwalletLegacyBilldeskPayment, StringComparison.OrdinalIgnoreCase))
            {
                retList[0].DisplayPages[0].Action = BuildAPMPollingAction(links[Constants.ButtonDisplayHintIds.Cc3DSYesButton]);
            }

            return retList;
        }

        public List<PIDLResource> Get3DSStatusCheckDescriptionForPaymentAuth(
            string rdsSessionId,
            string paymentSessionId,
            string partnerName,
            string language,
            string country,
            string resourceType,
            string scenario = null,
            string paymentMethodFamilyTypeId = null,
            PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retList = this.GetStaticPidlDescriptions(resourceType, language, partnerName, setting: setting);

            var links = GetRDSSessionStatusLink(rdsSessionId, paymentSessionId, partnerName, country, language, scenario, paymentMethodFamilyTypeId);
            AddSubmitLinks(links, retList);
            return retList;
        }

        public List<PIDLResource> GetRedirectPidlForPI(
            PaymentInstrument paymentInstrument,
            string type,
            string language,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            bool completePrerequisites = false,
            string country = null,
            List<string> flightNames = null,
            PaymentExperienceSetting setting = null,
            string sessionId = null,
            RedirectionServiceLink redirectLink = null)
        {
            List<PIDLResource> retList = this.GetStaticPidlDescriptions(
                type: type,
                language: language,
                partnerName: partnerName,
                redirectLink: redirectLink,
                paymentInstrument: paymentInstrument,
                flightNames: flightNames,
                setting: setting);

            var links = GetPILink(Constants.RestResourceNames.PaymentInstrumentsEx, language, partnerName, paymentInstrument.PaymentInstrumentId, completePrerequisites, country);
            AddSubmitLinks(links, retList);

            if (string.Equals(type, Constants.PidlResourceIdentities.GenericRedirectStaticPidl, StringComparison.OrdinalIgnoreCase))
            {
                RedirectionServiceLink redirectionLink = new RedirectionServiceLink()
                {
                    BaseUrl = paymentInstrument.PaymentInstrumentDetails.RedirectUrl
                };

                var hyperlinkRedirectionHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<HyperlinkDisplayHint>(Constants.DisplayHintIds.GenericRedirectLink, retList[0].DisplayPages);
                if (hyperlinkRedirectionHint != null)
                {
                    hyperlinkRedirectionHint.Action.Context = redirectionLink;
                }
            }

            // Now sending the setting as a parameter because the partner currently uses the original partner name instead of the template partner. This setting allows the function to change the partner to use the template partner as required.
            SetPidlRedirectionLinkFromPaymentInstrument(retList[0], paymentInstrument, type, partnerName, flightNames, setting: setting);

            if (string.Equals(paymentInstrument?.PaymentMethod?.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase)
                && string.Equals(paymentInstrument?.PaymentMethod.PaymentMethodType, Constants.PaymentMethodTypeNames.Venmo, StringComparison.OrdinalIgnoreCase))
            {
                AddPollActionContext(retList[0], Constants.RestResourceNames.PaymentInstrumentsEx, language, partnerName, paymentInstrument.PaymentInstrumentId, false, country, sessionId, Constants.ScenarioNames.VenmoWebPolling, flightNames);
            }

            return retList;
        }

        public List<PIDLResource> GetProfileDescriptions(
            string country,
            string type,
            string operation,
            string language,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            RestLink nextPidlLink = null,
            string profileId = null,
            Dictionary<string, string> profileV3Headers = null,
            bool overrideJarvisVersionToV3 = false,
            List<string> exposedFlightFeatures = null,
            string scenario = null,
            HashSet<string> partnerFlights = null,
            PaymentExperienceSetting setting = null,
            bool isGuestAccount = false)
        {
            country = Helper.TryToLower(country);
            type = Helper.TryToLower(type);
            language = Helper.TryToLower(language);
            scenario = Helper.TryToLower(scenario);

            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);
            List<PIDLResource> retList = new List<PIDLResource>();

            // for legal entity update opertion, there won't be any profileId.
            // TODO: consider removing this check once we move everthing to client binding.
            if ((profileId == null && !string.Equals(type, Constants.ProfileTypes.Legal, StringComparison.OrdinalIgnoreCase) && !string.Equals(type, Constants.ProfileTypes.Consumer, StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.PidlOperationTypes.Update, StringComparison.InvariantCultureIgnoreCase))
                || (profileId != null && string.Equals(operation, Constants.PidlOperationTypes.Add, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new PIDLArgumentException(
                    "Parameter operation is not valid with the content of profile object",
                    Constants.ErrorCodes.PIDLArgumentOperationIsInvalid);
            }

            // If partner enables multiple profiles 
            // and partner doesn't pass standaloneProfile in the x-ms-flighting header to override the setting
            // then we return multiple profiles for certain markets
            // Enable the Template based check, to sync with the commercialstore partner.
            if ((partnerFlights == null
                || !partnerFlights.Contains(Constants.PartnerFlightValues.StandaloneProfile))
                && (Constants.MultipleProfilesEnabledPartners.Contains(partnerName, StringComparer.OrdinalIgnoreCase)
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseMultipleProfile, country, setting)))
            {
                IEnumerable<string> profileIds = GetProfileTypeIds(partnerName, type, country, operation, setting);
                foreach (string id in profileIds)
                {
                    PIDLResource retVal = this.GetProfilePIDLResource(type, partnerName, country, operation, scenario, nextPidlLink, id, exposedFlightFeatures, setting);
                    retList.Add(retVal);
                }
            }
            else
            {
                PIDLResource retVal = this.GetProfilePIDLResource(type, partnerName, country, operation, scenario, nextPidlLink, null, exposedFlightFeatures, setting, overrideJarvisVersionToV3, isGuestAccount);
                retList.Add(retVal);
            }

            if (exposedFlightFeatures?.Contains(Flighting.Features.PXEnableCSVSubmitLinks, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                this.ConstructRestLinks(retList, overrideJarvisVersionToV3, scenario, profileId, profileV3Headers);
            }
            else
            {
                var links = GetProfileSubmitLink(partnerName, type, profileId, profileV3Headers, overrideJarvisVersionToV3, operation, exposedFlightFeatures, scenario, country: country, setting: setting);
                AddSubmitLinks(links, retList);
            }

            // For msfb show profile (organization/legalentity) scenario, add extra pidl action info to edit button
            // Enable the tempalte partner check, to sync with the commercialstores partner.
            bool isShowOperation = string.Equals(operation, Constants.PidlOperationTypes.Show, StringComparison.OrdinalIgnoreCase);
            bool hasAddUpdateProfileFeature = PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.AddUpdatePartnerActionToEditProfileHyperlink, country, setting);
            bool isCommercialStores = string.Equals(partnerName, Constants.PartnerNames.Commercialstores, StringComparison.OrdinalIgnoreCase);
            if (isShowOperation && (hasAddUpdateProfileFeature || isCommercialStores))
            {
                AddPidlActionToEditProfileButton(retList, type, language, country, partnerName, setting);
            }

            if (string.Equals(type, Constants.ProfileTypes.Consumer, StringComparison.OrdinalIgnoreCase))
            {
                string descriptionType = "profile";
                string[] profileMetaProperties = { Constants.DescriptionIdentityFields.Type, Constants.DescriptionIdentityFields.Country, Constants.DescriptionIdentityFields.Operation };
                foreach (PIDLResource pidl in retList)
                {
                    RemoveDataDescriptionWithFullPath(pidl, null, profileMetaProperties, descriptionType);
                }
            }

            return retList;
        }

        public List<PIDLResource> GetMiscellaneousDescriptions(string country, string type, string language, string partnerName = Constants.PidlConfig.DefaultPartnerName)
        {
            country = Helper.TryToLower(country);
            type = Helper.TryToLower(type);
            language = Helper.TryToLower(language);

            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.MiscellaneousDescription },
                { Constants.DescriptionIdentityFields.Type, type },
                { Constants.DescriptionIdentityFields.Country, country }
            });
            this.GetPIDLResourceRecursive(partnerName, Constants.DescriptionTypes.MiscellaneousDescription, type, country, GlobalConstants.Defaults.OperationKey, retVal);
            List<PIDLResource> retList = new List<PIDLResource>();
            retList.Add(retVal);
            return retList;
        }

        public List<PIDLResource> GetStaticPidlDescriptions(string type, string language, string partnerName = Constants.PidlConfig.DefaultPartnerName, RedirectionServiceLink redirectLink = null, PaymentInstrument paymentInstrument = null, List<string> flightNames = null, PaymentExperienceSetting setting = null)
        {
            type = Helper.TryToLower(type);
            language = Helper.TryToLower(language);

            Context.Culture = Helper.GetCultureInfo(language);

            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.StaticDescription },
                { Constants.DescriptionIdentityFields.Type, type },
            });

            string displayName = paymentInstrument != null ? paymentInstrument.PaymentMethod.Display.Name : GlobalConstants.Defaults.DisplayName;
            string logo = paymentInstrument != null ? FindSvgLogoOrDefault(paymentInstrument.PaymentMethod.Display.Logos, paymentInstrument.PaymentMethod.Display.Logo) : null;
            logo = logo != null ? new Uri(logo).PathAndQuery : GlobalConstants.Defaults.Logo;

            Dictionary<string, string> context = new Dictionary<string, string>()
            {
                {
                    Constants.ConfigSpecialStrings.CountryId, GlobalConstants.Defaults.CountryKey
                },
                {
                    Constants.ConfigSpecialStrings.Language, Context.Culture.Name
                },
                {
                    Constants.ConfigSpecialStrings.Operation, GlobalConstants.Defaults.OperationKey
                },
                {
                    Constants.HiddenOptionalFields.ContextKey, string.Empty
                },
                {
                    Constants.ConfigSpecialStrings.PaymentMethodDisplayName, displayName
                },
                {
                    Constants.ConfigSpecialStrings.PaymentMethodSvgLogo, logo
                }
            };

            this.GetPIDLResourceRecursive(
                partnerName: partnerName,
                descriptionType: Constants.DescriptionTypes.StaticDescription,
                id: type,
                country: GlobalConstants.Defaults.CountryKey,
                operation: GlobalConstants.Defaults.OperationKey,
                retVal: retVal,
                context: context,
                flightNames: flightNames,
                setting: setting);

            if (retVal.DisplayPages == null || retVal.DisplayPages.Count == 0)
            {
                throw new PIDLConfigException(string.Format("No Display Description for a Static Pidl for Type:{0}", type), Constants.ErrorCodes.PIDLDisplayDescriptionNotFound);
            }

            // TODO: I am still thinking about how to deal with this constant string
            // One solution is to add it as constant in project PidlFactory, another solution is to make it as a global constant for both PidlFactory and PxService
            // Leave it as todo for now
            if (string.Equals(type, "sepaPicvStatic", StringComparison.InvariantCultureIgnoreCase) && redirectLink != null)
            {
                var redirectButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.SepaRedirectButton, retVal.DisplayPages);
                if (redirectButton != null)
                {
                    redirectButton.Action.Context = redirectLink;
                }

                var successButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.SepaSuccessButton, retVal.DisplayPages);
                if (successButton != null && paymentInstrument != null)
                {
                    successButton.Action.Context = paymentInstrument.GetShallowCopyObj();
                }

                // Add context to Redirect hyperlink
                var hyperlinkRedirectionHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<HyperlinkDisplayHint>(Constants.DisplayHintIds.SepaRedirectLink, retVal?.DisplayPages);
                if (hyperlinkRedirectionHint != null)
                {
                    RedirectionServiceLink loginRedirectionLink = CreateLoginRedirectionLink(paymentInstrument, paymentInstrument.PaymentInstrumentDetails.RedirectUrl);
                    hyperlinkRedirectionHint.Action.Context = loginRedirectionLink;
                }
            }

            List<PIDLResource> retList = new List<PIDLResource>();
            retList.Add(retVal);

            return retList;
        }

        public List<PIDLResource> GetGenericPollPidlDescriptions(
            string type,
            string language,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            RedirectionServiceLink redirectLink = null,
            PaymentInstrument paymentInstrument = null,
            bool completePrerequisites = false,
            string country = null,
            PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retList = this.GetStaticPidlDescriptions(type, language, partnerName, redirectLink, paymentInstrument, setting: setting);

            var links = GetPILink(Constants.RestResourceNames.PaymentInstrumentsEx, language, partnerName, paymentInstrument?.PaymentInstrumentId, completePrerequisites, country);
            AddSubmitLinks(links, retList);
            AddGenericPollActionContext(retList[0], Constants.RestResourceNames.PaymentInstrumentsEx, language, partnerName, paymentInstrument.PaymentInstrumentId, country);

            return retList;
        }

        public List<PIDLResource> GetTaxIdDescriptions(
            string country,
            string type,
            string language,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            string profileType = null,
            string operation = GlobalConstants.Defaults.OperationKey,
            bool isStandalone = false,
            List<string> flightNames = null,
            string scenario = null,
            PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            type = Helper.TryToLower(type);
            language = Helper.TryToLower(language);

            // Validate input parameter
            ValidateCountry(country);
            ValidateTaxIdType(type);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            IEnumerable<string> taxIdTypes = this.GetTaxIdTypes(country, type, partnerName, profileType, isStandalone, flightNames, setting);
            List<PIDLResource> retVal = null;

            retVal = this.GetTaxIdPidls(type, taxIdTypes, country, partnerName, operation, isStandalone, scenario, setting);

            var billingLinks = GetTaxIDSubmitLink(Constants.RestResourceNames.TaxIds, partnerName, country, language, profileType, isStandalone, operation, scenario);
            AddSubmitLinks(billingLinks, retVal);

            return retVal;
        }

        public List<PIDLResource> GetPaymentTokenDescriptions(
            string country,
            string type,
            string language,
            string action,
            string operation,
            string partnerName = Constants.PidlConfig.DefaultPartnerName,
            List<string> flightNames = null,
            PaymentExperienceSetting setting = null,
            string piid = null)
        {
            country = Helper.TryToLower(country);
            type = Helper.TryToLower(type);
            language = Helper.TryToLower(language);

            // Validate input parameter
            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            List<PIDLResource> retList = new List<PIDLResource>();

            var types = new List<string>() { type };

            foreach (string pidlType in types)
            {
                PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
                {
                    { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.PaymentTokenDescription },
                    { Constants.DescriptionIdentityFields.Type, pidlType },
                    { Constants.DescriptionIdentityFields.Country, country }
                });

                this.GetPIDLResourceRecursive(partnerName, Constants.DescriptionTypes.PaymentTokenDescription, pidlType, country, operation ?? GlobalConstants.Defaults.OperationKey, retVal, null, true, null, null, null, null, flightNames, setting: setting);

                IFrameDisplayHint visaTokenIFrame = new IFrameDisplayHint
                {
                    HintId = Constants.DisplayHintIds.VisaTokenIFrame,
                    SourceUrl = string.Format(Common.Environments.Environment.IsProdOrPPEEnvironment ? Constants.IFrameContentTemplates.VisaTokenIframePROD : Constants.IFrameContentTemplates.VisaTokenIframeINT, action),
                };

                visaTokenIFrame.AddDisplayTag("accessibilityName", LocalizationRepository.Instance.GetLocalizedString("The visa passkey iframe", language));

                var redeemPage = retVal.DisplayPages[0];
                if (redeemPage != null)
                {
                    redeemPage.AddDisplayHint(visaTokenIFrame);
                }

                retList.Add(retVal);
            }

            var tokenSubmitLinks = GetPaymentTokenSubmitLink(Constants.RestResourceNames.Tokens, partnerName, country, language, piid);
            AddSubmitLinks(tokenSubmitLinks, retList);

            return retList;
        }

        public List<PIDLResource> GetTenantDescription(string tenantType, string country, string language, string partnerName = Constants.PidlConfig.DefaultPartnerName)
        {
            tenantType = Helper.TryToLower(tenantType);
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);

            // Validate input parameter
            ValidateTenantType(tenantType);
            ValidateCountry(country);

            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.TenantDescription },
                { Constants.DescriptionIdentityFields.Type, tenantType },
                { Constants.DescriptionIdentityFields.Country, country }
            });
            this.GetPIDLResourceRecursive(partnerName, Constants.DescriptionTypes.TenantDescription, tenantType, country, GlobalConstants.Defaults.OperationKey, retVal);
            return new List<PIDLResource> { retVal };
        }

        public List<PIDLResource> GetRewardsDescriptions(string rewardsType, string operation, string country, string language, string partnerName = Constants.PidlConfig.DefaultPartnerName)
        {
            rewardsType = Helper.TryToLower(rewardsType);
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);

            // Validate input parameter
            ValidateRewardsType(rewardsType);
            ValidateCountry(country);
            Context.Culture = Helper.GetCultureInfo(language);
            Context.Country = country;

            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.RewardsDescription },
                { Constants.DescriptionIdentityFields.Type, rewardsType },
                { Constants.DescriptionIdentityFields.Operation, operation },
                { Constants.DescriptionIdentityFields.Country, country }
            });

            // PIDL page for "redeem-GET" operation is empty and not made from CSVs
            if (!string.Equals(Constants.PidlOperationTypes.Redeem, operation, StringComparison.InvariantCultureIgnoreCase))
            {
                this.GetPIDLResourceRecursive(partnerName, Constants.DescriptionTypes.RewardsDescription, rewardsType, country, operation, retVal);

                // Add QR code image to display hint
                if (string.Equals(Constants.PidlOperationTypes.SelectChallengeType, operation, StringComparison.InvariantCultureIgnoreCase))
                {
                    AddQrCodeImageToDisplayHint(retVal, Constants.DisplayHintIds.RewardsEditPhoneNumberQrCodeImage, Constants.PidlUrlConstants.EditPhoneMSAPath);
                }
            }

            return new List<PIDLResource> { retVal };
        }

        public List<PIDLResource> GetConfirmCSVRedeemDescriptions(string country, string language, string partnerName = Constants.PidlConfig.DefaultPartnerName, PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);

            // Validate input parameter
            ValidateCountry(country);
            Context.Culture = Helper.GetCultureInfo(language);
            Context.Country = country;

            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.PaymentMethodDescription },
                { Constants.DescriptionIdentityFields.Family, Constants.PaymentMethodFamilyNames.Ewallet },
                { Constants.DescriptionIdentityFields.Type, Constants.PaymentMethodTypeNames.StoredValue },
                { Constants.DescriptionIdentityFields.Operation, Constants.PidlOperationTypes.ConfirmRedeem },
                { Constants.DescriptionIdentityFields.Country, country }
            });

            string resourceId = string.Join(".", Constants.PaymentMethodFamilyNames.Ewallet, Constants.PaymentMethodTypeNames.StoredValue);

            this.GetPIDLResourceRecursive(partnerName, Constants.DescriptionTypes.PaymentMethodDescription, resourceId, country, Constants.PidlOperationTypes.ConfirmRedeem, retVal, setting: setting);

            return new List<PIDLResource> { retVal };
        }

        public string GetSupportedValidationChallenge(string country, string paymentMethodFamily, string paymentMethodType)
        {
            string countryKey = this.validationChallengeTypes.ContainsKey(country) ? country : GlobalConstants.Defaults.CountryKey;
            if (this.validationChallengeTypes.ContainsKey(countryKey))
            {
                if (this.validationChallengeTypes[countryKey].ContainsKey(paymentMethodFamily))
                {
                    if (this.validationChallengeTypes[countryKey][paymentMethodFamily].ContainsKey(paymentMethodType))
                    {
                        return this.validationChallengeTypes[countryKey][paymentMethodFamily][paymentMethodType];
                    }
                }
            }

            return null;
        }

        public void GetPIDLResourceRecursive(
            string partnerName,
            string descriptionType,
            string id,
            string country,
            string operation,
            PIDLResource retVal,
            Dictionary<string, RestLink> overrideLinks = null,
            bool includeDisplayDescriptions = true,
            string displayDescriptionId = null,
            IList<string> hiddenOptionalPropertyNames = null,
            Dictionary<string, string> context = null,
            string scenario = null,
            List<string> flightNames = null,
            string classicProduct = null,
            string billableAccountId = null,
            PaymentExperienceSetting setting = null)
        {
            string originalPartner = partnerName;

            // Override the pidl partner name to look into correct PIDL config files
            partnerName = MapPartnerName(partnerName);

            this.GetPIDLResourceRecursiveInternal(
                TemplateHelper.GetSettingTemplate(partnerName, setting, descriptionType, id),
                descriptionType,
                WrapAccountRelatedDescriptionType(partnerName, descriptionType, id, country, setting),
                country,
                operation,
                retVal,
                overrideLinks,
                includeDisplayDescriptions,
                displayDescriptionId,
                hiddenOptionalPropertyNames,
                context,
                scenario,
                flightNames,
                classicProduct,
                billableAccountId,
                originalPartner: originalPartner,
                setting: setting);
        }

        public PropertyDescription GetPropertyDescriptionByPropertyName(string descriptionType, string id, string operation, string country, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(descriptionType))
            {
                throw new ArgumentException(
                    string.Format("Parameter DescriptionType \"{0}\" in GetPropertyDescriptionByPropertyName is null or whitespaces.", descriptionType));
            }

            // id and country could be empty (e.g. "ad.." which is address description with no specific id or country)
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            if (country == null)
            {
                throw new ArgumentNullException("country");
            }

            PIDLResourceConfig descriptionConfig = this.GetPIDLResourceConfig(descriptionType, id, operation, country);

            var propertyDescriptionId = (from d in descriptionConfig.DataDescriptionConfig
                                         where string.Equals(d[0], propertyName, StringComparison.InvariantCultureIgnoreCase)
                                         select d[1]).FirstOrDefault();

            if (string.IsNullOrEmpty(propertyDescriptionId))
            {
                throw new PIDLConfigException(
                    string.Format("Property with name '{0}' not found", propertyName),
                    Constants.ErrorCodes.PIDLProperyDescriptionNotFound);
            }

            return this.GetPropertyDescription(propertyDescriptionId, country);
        }

        public List<PropertyValidation> GetPropertyValidationList(string id, string country, string partner = GlobalConstants.Defaults.PartnerKey)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Parameter \"id\" in GetPropertyValidation is null or whitespaces.");
            }

            // country could be empty (e.g. where a validation does not require country-specific specialization)
            if (country == null)
            {
                throw new ArgumentNullException("country");
            }

            if (!this.propertyValidationLists.ContainsKey(id))
            {
                return null;
            }

            if (this.propertyValidationLists[id].ContainsKey(partner))
            {
                var partnerSpecificRules = this.propertyValidationLists[id][partner];
                if (partnerSpecificRules.ContainsKey(country))
                {
                    return partnerSpecificRules[country];
                }
                else if (partnerSpecificRules.ContainsKey(GlobalConstants.Defaults.CountryKey))
                {
                    return partnerSpecificRules[GlobalConstants.Defaults.CountryKey];
                }
            }

            if (this.propertyValidationLists[id].ContainsKey(GlobalConstants.Defaults.PartnerKey))
            {
                var defaultRules = this.propertyValidationLists[id][GlobalConstants.Defaults.PartnerKey];
                if (defaultRules.ContainsKey(country))
                {
                    return defaultRules[country];
                }
                else if (defaultRules.ContainsKey(GlobalConstants.Defaults.CountryKey))
                {
                    return defaultRules[GlobalConstants.Defaults.CountryKey];
                }
            }

            return null;
        }

        public PropertyDataProtection GetPropertyDataProtection(string id, string country)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            if (country == null)
            {
                throw new ArgumentNullException("country");
            }

            if (!this.propertyDataProtections.ContainsKey(id))
            {
                return null;
            }

            string countryKey = country;
            if (!this.propertyDataProtections[id].ContainsKey(countryKey))
            {
                countryKey = GlobalConstants.Defaults.CountryKey;
                if (!this.propertyDataProtections[id].ContainsKey(countryKey))
                {
                    return null;
                }
            }

            return this.propertyDataProtections[id][countryKey];
        }

        public Dictionary<string, PropertyTransformationInfo> GetPropertyTransformation(string id, string country)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Parameter \"id\" in GetPropertyTransformation is null or whitespaces.");
            }

            // processor could be empty (e.g. where a validation does not require processor-specific specialization)
            if (country == null)
            {
                throw new ArgumentNullException("country");
            }

            if (!this.propertyTransformations.ContainsKey(id))
            {
                return null;
            }

            string countryKey = country;
            if (!this.propertyTransformations[id].ContainsKey(countryKey))
            {
                countryKey = GlobalConstants.Defaults.CountryKey;
                if (!this.propertyTransformations[id].ContainsKey(countryKey))
                {
                    return null;
                }
            }

            return this.propertyTransformations[id][countryKey];
        }

        public List<PIDLResource> GetCheckoutDescriptions(
            HashSet<PaymentMethod> paymentMethods,
            string operation,
            string partnerName,
            string country,
            string language,
            string paySubmitUrl,
            string scenario,
            Dictionary<string, object> clientContext = null,
            string backButtonUrl = null,
            PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            language = Helper.TryToLower(language);
            operation = Helper.TryToLower(operation);

            // Validate input parameters
            ValidateCountry(country);
            ValidateOperation(operation);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            List<PIDLResource> retVal = new List<PIDLResource>();

            HashSet<PaymentMethod> resultSet = paymentMethods;
            if (resultSet.Count == 0)
            {
                throw new PIDLArgumentException(
                    string.Format("No results found for PaymentMethodFamily and PaymentMethodType for the provided country."),
                    Constants.ErrorCodes.PIDLArgumentFamilyIsNotSupportedForStoreInCountry);
            }

            foreach (PaymentMethod currentPaymentMethod in resultSet)
            {
                IEnumerable<string> pidlIds = GetPIDLIds(partnerName, currentPaymentMethod.PaymentMethodFamily, currentPaymentMethod.PaymentMethodType, null, null, country, operation, setting);
                foreach (string id in pidlIds)
                {
                    Dictionary<string, string> identityTable = new Dictionary<string, string>
                    {
                        { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.PaymentMethodDescription },
                        { Constants.DescriptionIdentityFields.Family, currentPaymentMethod.PaymentMethodFamily },
                        { Constants.DescriptionIdentityFields.Type, currentPaymentMethod.PaymentMethodType },
                        { Constants.DescriptionIdentityFields.Operation, Constants.PidlOperationTypes.RenderPidlPage },
                        { Constants.DescriptionIdentityFields.Country, country },
                        { Constants.DescriptionIdentityFields.ResourceIdentity, id }
                    };

                    if (string.Equals(currentPaymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.InvariantCultureIgnoreCase) &&
                        Constants.KoreaCreditCardType.TypeNames.Contains(currentPaymentMethod.PaymentMethodType) &&
                        string.Equals(country, "kr", StringComparison.InvariantCultureIgnoreCase) &&
                        string.Equals(operation, Constants.PidlOperationTypes.Add, StringComparison.InvariantCultureIgnoreCase))
                    {
                        identityTable.Remove(Constants.DescriptionIdentityFields.Type);
                    }

                    PIDLResource newPMD = new PIDLResource(identityTable);
                    newPMD.InitClientContext(clientContext);

                    string logo = FindSvgLogoOrDefault(currentPaymentMethod.Display.Logos, GlobalConstants.Defaults.Logo);
                    logo = logo != null ? new Uri(logo).PathAndQuery : GlobalConstants.Defaults.Logo;

                    Dictionary<string, string> context = new Dictionary<string, string>()
                    {
                        {
                            Constants.ConfigSpecialStrings.CountryId, country
                        },
                        {
                            Constants.ConfigSpecialStrings.Language, Context.Culture.Name
                        },
                        {
                            Constants.ConfigSpecialStrings.Operation, operation
                        },
                        {
                            Constants.ConfigSpecialStrings.EmailAddress, string.Empty
                        },
                        {
                            Constants.HiddenOptionalFields.ContextKey, string.Empty
                        },
                        {
                            Constants.ConfigSpecialStrings.PaymentMethodDisplayName, currentPaymentMethod.Display.Name
                        },
                        {
                            Constants.ConfigSpecialStrings.PaymentMethodSvgLogo, logo
                        }
                    };
                    retVal.Add(newPMD);
                    this.GetPIDLResourceRecursive(
                        partnerName,
                        Constants.DescriptionTypes.CheckoutDescription,
                        id,
                        country,
                        operation,
                        newPMD,
                        null,
                        true,
                        null,
                        Constants.HiddenOptionalFields.AddressDescriptionPropertyNames,
                        context,
                        scenario: scenario,
                        setting: setting);
                }
            }

            var links = GetPaySubmitLink(paySubmitUrl, backButtonUrl);
            AddSubmitLinks(links, retVal);

            return retVal;
        }

        public List<PIDLResource> GetPaymentClientDescriptions(
           string descriptionType,
           string country,
           string id,
           string operation,
           string language,
           string partnerName = Constants.PidlConfig.DefaultPartnerName,
           IList<PaymentMethod> paymentMethods = null,
           string defaultPaymentMethod = null,
           IList<PaymentInstrument> paymentInstruments = null,
           IList<PaymentInstrument> disabledPaymentInstruments = null,
           List<string> exposedFlightFeatures = null,
           string scenario = null,
           PaymentExperienceSetting setting = null)
        {
            country = Helper.TryToLower(country);
            descriptionType = Helper.TryToLower(descriptionType);
            id = Helper.TryToLower(id);
            language = Helper.TryToLower(language);
            scenario = Helper.TryToLower(scenario);

            ValidateCountry(country);

            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);
            List<PIDLResource> retVal = new List<PIDLResource>();

            var identityTable = new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, descriptionType }
            };

            if (descriptionType.Equals(Constants.DescriptionTypes.AddressDescription, StringComparison.OrdinalIgnoreCase))
            {
                identityTable.Add(Constants.DescriptionIdentityFields.Type, id.Split('.')[0]);
                identityTable.Add(Constants.DescriptionIdentityFields.ResourceIdentity, id);
            }

            if (descriptionType.Equals(Constants.DescriptionTypes.CheckoutDescription, StringComparison.OrdinalIgnoreCase))
            {
                identityTable.Add(Constants.DescriptionIdentityFields.ResourceIdentity, id);
            }

            identityTable.Add(Constants.DescriptionIdentityFields.Operation, operation);
            identityTable.Add(Constants.DescriptionIdentityFields.Country, country);

            PIDLResource pidlResource = new PIDLResource(identityTable);

            this.GetPIDLResourceRecursive(partnerName, descriptionType, id, country, operation, pidlResource, scenario: scenario, flightNames: exposedFlightFeatures, setting: setting);

            if (id.Equals(Constants.PidlResourceIdentities.PaymentMethodSelectPidl, StringComparison.OrdinalIgnoreCase))
            {
                PaymentSelectionHelper.PopulatePaymentMethods(pidlResource, new HashSet<PaymentMethod>(paymentMethods?.ToList() ?? new List<PaymentMethod>()), defaultPaymentMethod, language, country, partnerName, exposedFlightFeatures, scenario, setting: setting);
            }
            else if (id.Equals(Constants.PidlResourceIdentities.PaymentInstrumentSelectPidl, StringComparison.OrdinalIgnoreCase)
                || id.Equals(Constants.PidlResourceIdentities.SinglePaymentInstrumentNoPiPidl, StringComparison.OrdinalIgnoreCase))
            {
                PaymentSelectionHelper.PopulateSelectInstancePidl(
                    pidlResource,
                    partnerName,
                    country,
                    language,
                    paymentInstruments?.ToList(),
                    disabledPaymentInstruments?.ToList(),
                    new HashSet<PaymentMethod>(paymentMethods?.ToList() ?? new List<PaymentMethod>()),
                    null,
                    null,
                    exposedFlightFeatures,
                    null,
                    scenario,
                    setting: setting);
            }

            retVal.Add(pidlResource);
            return retVal;
        }

        public List<PIDLResource> GetStaticCheckoutErrorDescriptions(string language, string redirectUrl, string partnerName = Constants.PidlConfig.DefaultPartnerName)
        {
            List<PIDLResource> retVal = this.GetStaticPidlDescriptions(Constants.StaticDescriptionTypes.ThirdPartyPaymentsCheckoutErrorPidl, language, partnerName);

            var closeButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.ThirdPartyPaymentsErrorPageCloseButton, retVal[0].DisplayPages);

            if (closeButton != null)
            {
                closeButton.Action.Context = redirectUrl;
            }

            return retVal;
        }

        internal static bool ShouldGeneratePidlClientActionForAddPIPicv(PIDLGeneratorContext context)
        {
            if (IsSepa(context.PaymentInstrument) && context?.PaymentInstrument?.Status != null && string.Equals(context.PaymentInstrument.Status.ToString(), PaymentInstrumentStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (context.PaymentInstrument.PaymentInstrumentDetails.PicvDetails != null && string.Equals(context.PaymentInstrument.PaymentInstrumentDetails.PicvDetails.Status, Constants.PicvStatus.InProgress, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SetAddressToDefault(bool avsSuggest, bool setAsDefaultBilling, string partner, string scenario, PaymentExperienceSetting setting)
        {
            if ((avsSuggest && setAsDefaultBilling && Constants.AvsSuggestEnabledPartners.Contains(partner, StringComparer.OrdinalIgnoreCase))
                || (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) && string.Equals(scenario, Constants.ScenarioNames.Profile, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            else if (setAsDefaultBilling && setting.Features.ContainsKey(FeatureConfiguration.FeatureNames.UseAddressesExSubmit))
            {
                return true;
            }

            return false;
        }

        private static bool IsSepa(PaymentInstrument pi)
        {
            return string.Equals(pi.PaymentMethod.PaymentMethodFamily, V7.Constants.PaymentMethodFamilyNames.DirectDebit, StringComparison.OrdinalIgnoreCase)
               && string.Equals(pi.PaymentMethod.PaymentMethodType, V7.Constants.PaymentMethodTypeNames.Sepa, StringComparison.OrdinalIgnoreCase);
        }

        private static RestLink UrlForPurchaseConfrimButton(string sessionId, List<string> flights, string orderId)
        {
            RestLink submitUrlLink;
            string urlLink;

            if (flights?.Contains(V7.Constants.PartnerFlightValues.PXEnablePurchasePollingForUPIConfirmPayment, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                urlLink = string.Format(Constants.HandleGlobalQrCodePIPendingPurchaseUrls.QueryGlobalPendingPurchaseStatePurchaseServiceUrlTemplate, orderId);
                submitUrlLink = new RestLink() { Href = urlLink, Method = Constants.HTTPVerbs.PUT };
            }
            else
            {
                urlLink = string.Format(Constants.ConfirmPaymentForUPIUrls.QueryUPIPendingStateRedirectionServicePollUrlTemplate, sessionId);
                submitUrlLink = new RestLink() { Href = urlLink, Method = Constants.HTTPVerbs.GET };
            }

            return submitUrlLink;
        }

        private static void PayPalQRCodeAddIframePage(PIDLResource retVal, string language)
        {
            HyperlinkDisplayHint loginLink = retVal.GetDisplayHintById(Constants.DisplayHintIds.PaypalQrCodeChallengeLoginRedirectionLink) as HyperlinkDisplayHint;
            loginLink.Action.Context = new DisplayHintAction();
            loginLink.Action.ActionType = DisplayHintActionType.moveLast.ToString();

            PageDisplayHint iframePage = new PageDisplayHint();
            iframePage.HintId = Constants.DisplayHintIds.PaypalQrCodeChallengePage3;
            retVal.AddDisplayPages(new List<PageDisplayHint>() { iframePage });

            ButtonDisplayHint iframeBackButton = new ButtonDisplayHint();
            iframeBackButton.DisplayContent = LocalizationRepository.Instance.GetLocalizedString("Back", language);
            iframeBackButton.HintId = Constants.DisplayHintIds.BackButton;
            iframeBackButton.Action = new DisplayHintAction()
            {
                ActionType = DisplayHintActionType.moveFirst.ToString()
            };

            iframePage.AddDisplayHint(iframeBackButton);
        }

        private static void SetPidlRedirectionLinkFromPaymentInstrument(PIDLResource pidlResource, PaymentInstrument paymentInstrument, string type, string partnerName, List<string> flightNames, PaymentExperienceSetting setting = null)
        {
            bool isTemplateBasedFlow = IsTemplateInList(partnerName, setting, Constants.DescriptionTypes.StaticDescription, $"{Constants.DescriptionTypes.StaticDescription}.{type}");

            bool payPalRedirectStaticPidlShouldHaveRedirectText = string.Equals(type, Constants.PidlResourceIdentities.PaypalRedirectStaticPidl, StringComparison.OrdinalIgnoreCase) &&
                (flightNames.Contains(Flighting.Features.PXEnablePaypalRedirectUrlText, StringComparer.OrdinalIgnoreCase) ||
                isTemplateBasedFlow ||
                Constants.PartnersToEnablePaypalRedirectOnTryAgain.Contains(partnerName, StringComparer.OrdinalIgnoreCase));

            PaymentInstrumentDetails paymentInstrumentDetails = paymentInstrument.PaymentInstrumentDetails;
            if (flightNames != null && payPalRedirectStaticPidlShouldHaveRedirectText)
            {
                // Prepare RDS link to attach to button or hyperlink display hint
                RedirectionServiceLink loginRedirectionLink = CreateLoginRedirectionLink(paymentInstrument, paymentInstrumentDetails.RedirectUrl);

                if (Constants.PartnersToEnablePaypalRedirectOnTryAgain.Contains(partnerName, StringComparer.OrdinalIgnoreCase) || isTemplateBasedFlow)
                {
                    var tryAgainRedirectionButtonDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.PaypalNoButton, pidlResource.DisplayPages);
                    if (tryAgainRedirectionButtonDisplayHint != null)
                    {
                        tryAgainRedirectionButtonDisplayHint.Action.Context = loginRedirectionLink;
                        tryAgainRedirectionButtonDisplayHint.Action.ActionType = Constants.ActionType.Redirect;
                    }
                }

                if (flightNames.Contains(Flighting.Features.PXEnablePaypalRedirectUrlText, StringComparer.OrdinalIgnoreCase) || isTemplateBasedFlow)
                {
                    // either of the 2 flights means that PaypalRedirectLink needs to be configured
                    var hyperlinkRedirectionHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<HyperlinkDisplayHint>(Constants.DisplayHintIds.PaypalRedirectLink, pidlResource.DisplayPages);
                    if (hyperlinkRedirectionHint != null)
                    {
                        hyperlinkRedirectionHint.Action.Context = loginRedirectionLink;
                    }
                }
            }

            bool isVenmoPi = string.Equals(paymentInstrument?.PaymentMethod?.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(paymentInstrument?.PaymentMethod?.PaymentMethodType, Constants.PaymentMethodTypeNames.Venmo, StringComparison.OrdinalIgnoreCase);

            if (isVenmoPi)
            {
                HyperlinkDisplayHint venmoRedirectLink = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.VenmoRedirectLink) as HyperlinkDisplayHint;

                if (venmoRedirectLink != null)
                {
                    RedirectionServiceLink loginRedirectionLink = CreateLoginRedirectionLink(paymentInstrument, paymentInstrumentDetails.RedirectUrl);
                    venmoRedirectLink.Action.Context = loginRedirectionLink;
                }
            }
        }

        private static void UpdatePaymentMethodForBillingNonSim(List<PIDLResource> retVal, string operation, string family, string partnerName, HashSet<PaymentMethod> paymentMethods, List<string> exposedFlightFeatures = null)
        {
            if (family == Constants.PaymentMethodFamilyNames.MobileBillingNonSim)
            {
                foreach (var pr in retVal)
                {
                    // Override the hidden paymentMethodType property for Non Sim Mobi
                    foreach (var property in pr.GetPropertyDescriptionOfIdentity(Constants.DescriptionTypes.PaymentMethodDescription, Constants.DescriptionIdentityFields.Type))
                    {
                        var mobileOperatorDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<PropertyDisplayHint>(Constants.DisplayHintIds.NonSimMobiPhoneOperator, pr.DisplayPages);
                        property.PropertyType = "userData";
                        property.PropertyDescriptionType = "string";
                        property.DataType = "string";
                        property.IsKey = true;
                        Dictionary<string, SelectOptionDescription> possibleOptions = new Dictionary<string, SelectOptionDescription>();
                        foreach (var paymentMethod in paymentMethods)
                        {
                            property.AddPossibleValue(paymentMethod.PaymentMethodType, paymentMethod.Display.Name);
                            possibleOptions.Add(paymentMethod.PaymentMethodType, new SelectOptionDescription { DisplayImageUrl = GetDisplayLogoUrl(paymentMethod), DisplayText = paymentMethod.Display.Name });
                        }

                        if (mobileOperatorDisplayHint != null)
                        {
                            mobileOperatorDisplayHint.SetPossibleOptions(possibleOptions);
                        }

                        if (property.Validation == null)
                        {
                            property.AddAdditionalValidation(new PropertyValidation(property.DefaultValue.ToString()));
                        }
                        else
                        {
                            property.Validation.Regex = property.DefaultValue.ToString();
                        }

                        if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                        {
                            PaymentMethod pm = new PaymentMethod()
                            {
                                PaymentMethodType = property.DefaultValue.ToString()
                            };

                            var logoURL = possibleOptions[property.DefaultValue.ToString()].DisplayImageUrl;
                            var alternateLogo = PaymentSelectionHelper.CheckForReactNativeAlternatePaymentMethodLogoUrl(pm, partnerName, exposedFlightFeatures);
                            if (!string.IsNullOrEmpty(alternateLogo))
                            {
                                logoURL = alternateLogo;
                            }

                            var logo = new LogoDisplayHint
                            {
                                HintId = property.DefaultValue.ToString(),
                                SourceUrl = logoURL,
                            };

                            if (operation == Constants.PidlOperationTypes.Add)
                            {
                                var nonSimMobiOperatorGroup = pr.GetDisplayHintById(Constants.DisplayHintIds.NonSimMobiOperatorGroup) as ContainerDisplayHint;
                                logo.HintId = string.Format(Constants.DisplayHintIdPrefixes.AddNSMLogo, logo.HintId);
                                logo.StyleHints = new List<string> { "image-small", "margin-top-2x-small", "margin-end-x-small" };
                                nonSimMobiOperatorGroup?.Members?.Add(logo);
                            }
                            else if (operation == Constants.PidlOperationTypes.Update)
                            {
                                var nsmShowInfoHeaderGroup = pr.GetDisplayHintById(Constants.DisplayHintIds.NsmShowInfoHeaderGroup) as ContainerDisplayHint;
                                logo.StyleHints = new List<string> { "image-small-400" };
                                nsmShowInfoHeaderGroup?.Members?.Add(logo);
                            }
                            else if (operation == Constants.PidlOperationTypes.Delete)
                            {
                                var deleteHeaderGroup = pr.GetDisplayHintById(Constants.DisplayHintIds.DeletePageHeaderGroup) as ContainerDisplayHint;
                                logo.StyleHints = new List<string> { "image-small-400" };
                                deleteHeaderGroup?.Members?.Add(logo);
                            }
                        }

                        property.DefaultValue = null;
                    }
                }
            }
        }

        /// <summary>
        /// Determine if partner requires tax ID collection.
        /// </summary>
        /// <param name="partner">Partner name</param>
        /// <param name="linkedPidl">The linked PIDL for access to type and country.</param>
        /// <returns>True if the tax ID collection is enabled for a particular partner and country. For Brazil, we have a special ID, so the country is implicit.</returns>
        private static bool IsSplitPageTaxIdCollectionEnabled(string partner, PIDLResource linkedPidl)
        {
            return (linkedPidl.Identity[Constants.DescriptionIdentityFields.Type] == "brazil_cpf_id"
                || (linkedPidl.Identity[Constants.DescriptionIdentityFields.Type] == "vat_id" && string.Equals(linkedPidl.Identity[Constants.DescriptionIdentityFields.CountryCode], "pt", StringComparison.InvariantCulture)))
                && Constants.PartnersWithPageSplits.Contains(partner);
        }

        private static void UpdatePageTokenChangeBasedOnCardType(List<PIDLResource> retVal, string type, string language)
        {
            string headerName, summaryPageHeading;
            switch (type)
            {
                case Constants.PaymentMethodTypeNames.Visa:
                    headerName = Constants.XboxNativeSummaryPageHeading.SummaryPageHeadingVisa;
                    summaryPageHeading = LocalizationRepository.Instance.GetLocalizedString(Constants.XboxNativeEditPIHeadings.EditVisa, language);
                    break;
                case Constants.PaymentMethodTypeNames.MasterCard:
                    headerName = Constants.XboxNativeSummaryPageHeading.SummaryPageHeadingMC;
                    summaryPageHeading = LocalizationRepository.Instance.GetLocalizedString(Constants.XboxNativeEditPIHeadings.EditMC, language);
                    break;
                case Constants.PaymentMethodTypeNames.Discover:
                    headerName = Constants.XboxNativeSummaryPageHeading.SummaryPageHeadingDiscover;
                    summaryPageHeading = LocalizationRepository.Instance.GetLocalizedString(Constants.XboxNativeEditPIHeadings.EditDiscover, language);
                    break;
                case Constants.PaymentMethodTypeNames.Amex:
                    headerName = Constants.XboxNativeSummaryPageHeading.SummaryPageHeadingAmex;
                    summaryPageHeading = LocalizationRepository.Instance.GetLocalizedString(Constants.XboxNativeEditPIHeadings.EditAmex, language);
                    break;
                case Constants.PaymentMethodTypeNames.Verve:
                    headerName = Constants.XboxNativeSummaryPageHeading.SummaryPageHeadingVerve;
                    summaryPageHeading = LocalizationRepository.Instance.GetLocalizedString(Constants.XboxNativeEditPIHeadings.EditVerve, language);
                    break;
                case Constants.PaymentMethodTypeNames.Elo:
                    headerName = Constants.XboxNativeSummaryPageHeading.SummaryPageHeadingElo;
                    summaryPageHeading = LocalizationRepository.Instance.GetLocalizedString(Constants.XboxNativeEditPIHeadings.EditElo, language);
                    break;
                case Constants.PaymentMethodTypeNames.HiperCard:
                    headerName = Constants.XboxNativeSummaryPageHeading.SummaryPageHeadingHipercard;
                    summaryPageHeading = LocalizationRepository.Instance.GetLocalizedString(Constants.XboxNativeEditPIHeadings.EditHiperCard, language);
                    break;
                case Constants.PaymentMethodTypeNames.JapanCreditBureau:
                    headerName = Constants.XboxNativeSummaryPageHeading.SummaryPageHeadingJCB;
                    summaryPageHeading = LocalizationRepository.Instance.GetLocalizedString(Constants.XboxNativeEditPIHeadings.EditJCB, language);
                    break;
                default:
                    headerName = null;
                    summaryPageHeading = null;
                    break;
            }

            if (headerName != null)
            {
                foreach (PIDLResource pidl in retVal)
                {
                    ContentDisplayHint headerVariableChange = pidl.GetDisplayHintById(headerName) as ContentDisplayHint;
                    ContentDisplayHint accountToken = pidl.GetDisplayHintById(Constants.DisplayHintIds.CreditCardAccountToken) as ContentDisplayHint;
                    ContentDisplayHint contentDisplay = pidl.GetDisplayHintById(Constants.DisplayHintIds.BillingAddressPageHeader) as ContentDisplayHint;

                    // BUG 42879564: Both Add PI and Edit PI flows uses the same literal for titles in billing address and summary pages.
                    // In Edit PI flow, replacing the header in billing address and summary pages.
                    if (contentDisplay != null)
                    {
                        string editYourBillingAddress = LocalizationRepository.Instance.GetLocalizedString(Constants.XboxNativeEditPIHeadings.EditYourBillingAddress, language);
                        contentDisplay.DisplayContent = editYourBillingAddress;
                    }

                    if (headerVariableChange != null)
                    {
                        summaryPageHeading = summaryPageHeading.Replace(Constants.DataDescriptionVariableNames.AccountToken, Constants.DataDescriptionVariableNames.LastFourDigits);
                        summaryPageHeading = LocalizationRepository.Instance.GetLocalizedString(summaryPageHeading, language);
                        headerVariableChange.DisplayContent = summaryPageHeading;
                    }

                    if (accountToken != null)
                    {
                        accountToken.DisplayContent = accountToken.DisplayContent.Replace(Constants.DataDescriptionVariableNames.AccountToken, Constants.DataDescriptionVariableNames.LastFourDigits);
                    }
                }
            }
        }

        private static void UpdatePaymentSelectPidl(PIDLResource pidl, int index, string paymentProviderId, string redirectUrl, string language, string country, string partnerName, string checkoutId)
        {
            // set the option's key as the resource_id in identity, as a key to switch between pidls when different option is selected
            var propertyDisplayHint = pidl.GetDisplayHintById("paymentMethod") as PropertyDisplayHint;

            if (propertyDisplayHint == null)
            {
                propertyDisplayHint = pidl.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodTppSelect) as PropertyDisplayHint;
            }

            Dictionary<string, SelectOptionDescription> possibleOptions = propertyDisplayHint.PossibleOptions;
            string resource_id = possibleOptions.ElementAt(index).Key;
            pidl.Identity["resource_id"] = resource_id;

            // contrucet the request url of next button
            ActionContext optionContext = possibleOptions.ElementAt(index).Value.PidlAction.Context as ActionContext;
            string paymentMethodFamily = optionContext.PaymentMethodFamily;
            string paymentMethodType = optionContext.PaymentMethodType;
            var nextButton = pidl.GetDisplayHintById("nextButton") as ButtonDisplayHint;

            if (nextButton == null)
            {
                nextButton = pidl.GetDisplayHintById("nextButtontpp") as ButtonDisplayHint;
            }

            if (nextButton != null)
            {
                RestLink submitUrlLink = new RestLink() { Href = $"https://{{pifd-endpoint}}/CheckoutDescriptions?redirectUrl={redirectUrl}&paymentProviderId={paymentProviderId}&partner={partnerName}&operation=RenderPidlPage&country={country}&language={language}&checkoutId={checkoutId}&family={paymentMethodFamily}&type={paymentMethodType}&scenario=pidlContext", Method = Constants.HTTPVerbs.GET };
                nextButton.Action.Context = submitUrlLink;
            }

            // add regex validation for "id" property
            var propertyDescription = pidl.DataDescription["id"] as PropertyDescription;
            propertyDescription.IsKey = true;
            propertyDescription.DataType = "returnObject";
            propertyDescription.PropertyDescriptionType = "returnObject";
            string regex = string.Format("^{0}$", resource_id);
            propertyDescription.AddAdditionalValidation(new PropertyValidation(regex));
        }

        private static PIDLResource GetCheckoutPaymentSelectPidl(
            string partnerName,
            string country,
            string operation,
            string pidlId,
            HashSet<PaymentMethod> filteredPaymentMethods,
            string defaultPaymentMethod,
            string language,
            string paymentProviderId,
            string redirectUrl,
            string checkoutId,
            string resourceId = null,
            string descriptionType = Constants.DescriptionTypes.PaymentMethodDescription,
            int pageIndex = 0,
            List<string> flightNames = null,
            string classicProduct = null,
            string billableAccountId = null,
            string scenario = null,
            PaymentExperienceSetting setting = null)
        {
            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>
                {
                        { Constants.DescriptionIdentityFields.DescriptionType, descriptionType },
                        { Constants.DescriptionIdentityFields.Operation, Constants.PidlOperationTypes.RenderPidlPage },
                        { Constants.DescriptionIdentityFields.Country, country }
                });

            PIDLResourceFactory.Instance.GetPIDLResourceRecursive(
                partnerName,
                descriptionType,
                pidlId,
                country,
                operation,
                retVal,
                flightNames: flightNames,
                classicProduct: classicProduct,
                billableAccountId: billableAccountId,
                scenario: scenario,
                setting: setting);

            PaymentSelectionHelper.PopulatePaymentMethods(retVal, filteredPaymentMethods, defaultPaymentMethod, language, country, partnerName, flightNames, Constants.ScenarioNames.SelectPMWithLogo);

            UpdatePaymentSelectPidl(retVal, pageIndex, paymentProviderId, redirectUrl, language, country, partnerName, checkoutId);

            return retVal;
        }

        private static string FindSvgLogoOrDefault(List<Logo> logos, string defaultLogo)
        {
            string logoUrl = defaultLogo;
            if (logos != null)
            {
                foreach (Logo logo in logos)
                {
                    if (string.Equals(logo.MimeType, "image/svg+xml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        logoUrl = logo.Url;
                    }
                }
            }

            return Uri.IsWellFormedUriString(logoUrl, UriKind.Absolute) ? logoUrl : null;
        }

        private static string ConstructRedirectURLFromSessionQueryUrl(string sessionQueryUrl)
        {
            string rdsSessionId;
            try
            {
                rdsSessionId = sessionQueryUrl.Split('/')[1];
            }
            catch
            {
                return string.Empty;
            }

            return string.Format(Constants.RedirectUrls.RedirectTemplate, rdsSessionId);
        }

        /// <summary>
        /// Adds a pidlAction to pidl
        /// </summary>
        /// <param name="pidlResources">The resource to change the property to</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="partner">The name of the partner</param>
        private static void AddActionToAddPaymentInstrument(List<PIDLResource> pidlResources, string language, string country, string partner)
        {
            AddPartnerActionToDisplayHint<HyperlinkDisplayHint>(
                pidlResources: pidlResources,
                displayHintId: Constants.DisplayHintIds.BillingGroupLightWeightAddNewPaymentInstrument,
                actionType: PIActionType.SelectResourceType,
                resourceType: Constants.DescriptionTypes.PaymentMethodDescription,
                language: language,
                country: country,
                partner: partner);
        }

        private static void AddQrCodeImageToDisplayHint(PIDLResource pidlResource, string qrCodeChallengeImageDisplayHintId, string qrCodeChallengeImageURL)
        {
            if (pidlResource.DisplayPages != null)
            {
                var qrCodeChallengeImage = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ImageDisplayHint>(qrCodeChallengeImageDisplayHintId, pidlResource.DisplayPages);
                if (qrCodeChallengeImage == null)
                {
                    throw new PIDLConfigException(
                            string.Format("No Display Hint found for Id \"{0}\"", qrCodeChallengeImageDisplayHintId),
                            Constants.ErrorCodes.PIDLConfigUnknownDisplayHintId);
                }

                string qrCodeChallengeImageSourceUrl = GetUrlQrCodeImage(qrCodeChallengeImageURL);
                qrCodeChallengeImage.SourceUrl = qrCodeChallengeImageSourceUrl;
            }
        }

        private static void SetupRedirectWithIframe(PIDLResource retVal, string redirectURL, int position = 0, string hintId = null)
        {
            var pageCount = retVal.DisplayPages.Count;
            var iframePage = retVal.DisplayPages[pageCount > 0 ? pageCount - 1 : pageCount];

            if (iframePage != null)
            {
                IFrameDisplayHint redirectIFrame = new IFrameDisplayHint
                {
                    HintId = hintId ?? Constants.DisplayHintIds.GlobalPIQrCodeIframe,
                    SourceUrl = redirectURL
                };

                iframePage.Members.Insert(position, redirectIFrame);
            }
        }

        private static void SetupRedirectWithHyperlink(PIDLResource retVal, PaymentInstrument paymentInstrument, string redirectURL, string redirectURLDisplayHintId)
        {
            var hyperLink = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<HyperlinkDisplayHint>(redirectURLDisplayHintId, retVal.DisplayPages);
            if (hyperLink != null)
            {
                RedirectionServiceLink loginRedirectionLink = CreateLoginRedirectionLink(paymentInstrument, redirectURL);

                hyperLink.Action.Context = loginRedirectionLink;
            }
        }

        private static RedirectionServiceLink CreateLoginRedirectionLink(PaymentInstrument paymentInstrument, string redirectUrl)
        {
            RedirectionServiceLink loginRedirectionLink = new RedirectionServiceLink()
            {
                BaseUrl = redirectUrl
            };

            PaymentInstrumentDetails details = paymentInstrument.PaymentInstrumentDetails;

            loginRedirectionLink.RuParameters.Add("id", paymentInstrument.PaymentInstrumentId);
            loginRedirectionLink.RuParameters.Add("family", paymentInstrument.PaymentMethod.PaymentMethodFamily);
            loginRedirectionLink.RuParameters.Add("type", paymentInstrument.PaymentMethod.PaymentMethodType);
            loginRedirectionLink.RuParameters.Add("pendingOn", details.PendingOn);
            loginRedirectionLink.RuParameters.Add("picvRequired", details.PicvRequired.ToString());

            return loginRedirectionLink;
        }

        private static void SetupRedirectURLButton(PIDLResource retVal, PaymentInstrument paymentInstrument, string partnerName, string redirectURL, string qrCodeRedirectButton, string qrCodeChallengeImageURL, string challengeDescriptionType)
        {
            var redirectButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(qrCodeRedirectButton, retVal.DisplayPages);

            if (redirectButton != null)
            {
                if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.ThreeDSOneQrCode, StringComparison.OrdinalIgnoreCase))
                {
                    // The original PIDL has the redirect button with a moveNext action,
                    // Change to navigate/redirect and remove the iframe page
                    if (string.Equals(partnerName, Constants.PartnerNames.AmcXbox, StringComparison.OrdinalIgnoreCase))
                    {
                        redirectButton.Action.Context = qrCodeChallengeImageURL;
                        redirectButton.Action.ActionType = Constants.ActionType.Navigate;
                    }
                    else if (string.Equals(partnerName, Constants.PartnerNames.Xbox, StringComparison.OrdinalIgnoreCase))
                    {
                        RedirectionServiceLink loginRedirectionLink = new RedirectionServiceLink()
                        {
                            BaseUrl = qrCodeChallengeImageURL
                        };

                        redirectButton.Action.Context = loginRedirectionLink;
                        redirectButton.Action.ActionType = Constants.ActionType.Redirect;
                    }

                    retVal.DisplayPages.RemoveAt(retVal.DisplayPages.Count - 1);
                }
                else if (string.Equals(partnerName, Constants.PartnerNames.Xbox, StringComparison.OrdinalIgnoreCase))
                {
                    RedirectionServiceLink loginRedirectionLink = CreateLoginRedirectionLink(paymentInstrument, paymentInstrument.PaymentInstrumentDetails.RedirectUrl);

                    redirectButton.Action.Context = loginRedirectionLink;
                }
                else if (string.Equals(partnerName, Constants.PartnerNames.AmcXbox, StringComparison.OrdinalIgnoreCase))
                {
                    redirectButton.Action.Context = redirectURL;
                }
            }
        }

        private static DisplayHintAction Build3DSOnePollingAction(
            string sessionId,
            string partnerName)
        {
            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());
            string pollUrl = $"https://{{pifd-endpoint}}/users/{{userId}}/paymentSessions/{sessionId}/status";
            PollActionContext pollActionContext = new PollActionContext()
            {
                Href = pollUrl,
                Method = Constants.HTTPVerbs.GET,
                Interval = Constants.PollingIntervals.ThreeDSOnePollingInterval,
                ResponseResultExpression = Constants.PollingResponseResultExpression.ThreeDSOnePurchaseResponseResultExpression,
                CheckPollingTimeOut = false,
            };

            // Xbox polling returns success for all so PIDL passes the paymentsession in the response payload in callbacks
            // to the integrating partner. gohome action does not return payload and handleFailure stringifies whole
            // response message which is undesired.
            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName)
                || string.Equals(partnerName, Constants.PartnerNames.Xbox, StringComparison.OrdinalIgnoreCase))
            {
                pollActionContext.AddResponseActionsItem("Succeeded", new DisplayHintAction(DisplayHintActionType.success.ToString()));
                pollActionContext.AddResponseActionsItem("Failed", new DisplayHintAction(DisplayHintActionType.success.ToString()));
                pollActionContext.AddResponseActionsItem("TimedOut", new DisplayHintAction(DisplayHintActionType.success.ToString()));
                pollActionContext.AddResponseActionsItem("Cancelled", new DisplayHintAction(DisplayHintActionType.success.ToString()));
                pollActionContext.AddResponseActionsItem("InternalServerError", new DisplayHintAction(DisplayHintActionType.success.ToString()));
            }
            else
            {
                pollActionContext.AddResponseActionsItem("Succeeded", new DisplayHintAction(DisplayHintActionType.success.ToString()));
                pollActionContext.AddResponseActionsItem("Failed", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
                pollActionContext.AddResponseActionsItem("TimedOut", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
                pollActionContext.AddResponseActionsItem("Cancelled", new DisplayHintAction(DisplayHintActionType.gohome.ToString()));
                pollActionContext.AddResponseActionsItem("InternalServerError", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
            }

            pollAction.Context = pollActionContext;
            return pollAction;
        }

        private static DisplayHintAction BuildAPMPollingAction(
            RestLink restLink)
        {
            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());

            string pollingActionUrl = restLink.Href;
            pollingActionUrl = AddOrUpdateQueryParam(pollingActionUrl, GlobalConstants.QueryParams.Scenario, Constants.ScenarioNames.PollingAction);
            PollActionContext pollActionContext = new PollActionContext()
            {
                Href = pollingActionUrl,
                Method = restLink.Method,
                Payload = restLink.Payload,
                Interval = Constants.PollingIntervals.GenericPollingInterval,
                ResponseResultExpression = Constants.PollingResponseResultExpression.APMStatusResultExpression,
                CheckPollingTimeOut = false,
            };

            pollActionContext.AddResponseActionsItem("Succeeded", new DisplayHintAction(DisplayHintActionType.success.ToString()));
            pollActionContext.AddResponseActionsItem("Failed", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));

            pollAction.Context = pollActionContext;
            return pollAction;
        }

        private static string AddOrUpdateQueryParam(string pollingActionUrl, string queryParam, string queryParamValue)
        {
            if (string.IsNullOrEmpty(pollingActionUrl))
            {
                return pollingActionUrl;
            }

            var uri = new Uri(pollingActionUrl);
            string query = uri.Query.TrimStart('?');
            Dictionary<string, string> parameters = new (StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(query))
            {
                foreach (string part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = part.Split('=', 2);
                    string key = WebUtility.UrlDecode(kv[0]);
                    string value = kv.Length > 1 ? WebUtility.UrlDecode(kv[1]) : string.Empty;
                    parameters[key] = value;
                }
            }

            parameters[queryParam] = queryParamValue;

            var sb = new StringBuilder();
            foreach (var kvp in parameters)
            {
                if (sb.Length > 0)
                {
                    sb.Append('&');
                }

                sb.Append(WebUtility.UrlEncode(kvp.Key));
                sb.Append('=');
                sb.Append(WebUtility.UrlEncode(kvp.Value));
            }

            var builder = new UriBuilder(uri)
            {
                Query = sb.ToString()
            };

            return builder.Uri.ToString();
        }

        private static DisplayHintAction CheckoutChallengePollingAction(
            string checkoutId,
            string partnerName,
            string paymentProviderId,
            string partnerRedirectUrl)
        {
            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());
            string pollUrl = GetCheckoutPollingLink(checkoutId, paymentProviderId);
            PollActionContext pollActionContext = new PollActionContext()
            {
                Href = pollUrl,
                Method = Constants.HTTPVerbs.GET,
                Interval = Constants.PollingIntervals.ThreeDSOnePollingInterval,
                ResponseResultExpression = Constants.PollingResponseResultExpression.CheckoutStatusResultExpression,
                CheckPollingTimeOut = false,
            };

            pollActionContext.AddResponseActionsItem("Failed", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
            pollAction.Context = pollActionContext;

            return pollAction;
        }

        private static void IncludeDisplayDescriptions(
            PIDLResource retVal,
            string partnerName,
            string descriptionType,
            string id,
            string country,
            string operation,
            string displayDescriptionId,
            Dictionary<string, string> context,
            SubmitLink submitLink,
            string scenario = null,
            List<string> flightNames = null)
        {
            List<PageDisplayHint> displayPages = null;
            List<DisplayStringMap> displayStringMap = null;
 
            displayPages = PIDLResourceDisplayHintFactory.Instance.GetDisplayPages(
                partnerName,
                descriptionType,
                id,
                country,
                operation,
                context,
                displayDescriptionId,
                scenario,
                submitLink,
                flightNames);
            displayStringMap = PIDLResourceDisplayHintFactory.Instance.GetPidlResourceDisplayStringMap(
                partnerName,
                descriptionType,
                id,
                country,
                operation,
                scenario);

            // Display description might or might not exist for a PIDL resource.
            if (displayPages != null)
            {
                retVal.AddDisplayPages(displayPages);
            }

            if (displayStringMap != null)
            {
                retVal.PidlResourceStrings = new PidlResourceStrings();
                retVal.PidlResourceStrings.AddDisplayStringMapFromList(displayStringMap);
            }
        }

        private static string GetDisplayLogoUrl(PaymentMethod paymentMethod)
        {
            string[] preferredMimeTypes = new string[] { "image/svg+xml", "image/png" };
            if (paymentMethod != null && paymentMethod.Display != null)
            {
                if (paymentMethod.Display.Logos != null)
                {
                    foreach (string mimeType in preferredMimeTypes)
                    {
                        Logo displayLogo = paymentMethod.Display.Logos.FirstOrDefault(i => string.Equals(i.MimeType, mimeType, StringComparison.OrdinalIgnoreCase));
                        if (displayLogo != null && !string.IsNullOrWhiteSpace(displayLogo.Url))
                        {
                            return displayLogo.Url;
                        }
                    }
                }

                return paymentMethod.Display.Logo;
            }

            return null;
        }

        private static void ValidateCountry(string country)
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                throw new PIDLArgumentException(
                    "Parameter country is Null or Whitespaces",
                    Constants.ErrorCodes.PIDLArgumentCountryIsInvalid);
            }

            if (!domainDictionaries[Constants.DomainDictionaryNames.MSFTCommerceCountries].ContainsKey(country))
            {
                throw new PIDLArgumentException(
                    string.Format("Country parameter is not valid"),
                    Constants.ErrorCodes.PIDLArgumentCountryIsInvalid);
            }
        }

        private static void ValidatePiAndChallengeType(string type, PaymentInstrument pi)
        {
            if (((string.Equals(type, Constants.ChallengeDescriptionTypes.Cvv) || string.Equals(type, Constants.ChallengeDescriptionTypes.ThreeDS)) &&
                !string.Equals(pi.PaymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.InvariantCultureIgnoreCase)) ||
                (string.Equals(type, Constants.ChallengeDescriptionTypes.Sms) &&
                !string.Equals(pi.PaymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.MobileBillingNonSim, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new PIDLArgumentException(
                    "Challenge type does not match the payment instrument type.",
                    Constants.ErrorCodes.PIDLArgumentChallengeDescriptionIdInvalidForPi);
            }
        }

        private static void ValidateOperation(string operation)
        {
            if (string.IsNullOrWhiteSpace(operation))
            {
                throw new PIDLArgumentException(
                    "Parameter operation is Null or Whitespaces",
                    Constants.ErrorCodes.PIDLArgumentOperationIsInvalid);
            }

            foreach (var operationType in Constants.PidlPossibleOperationsTypes)
            {
                if (string.Equals(operation, operationType, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            throw new PIDLArgumentException(
                "Parameter operation is not in the list of supported operation types",
                Constants.ErrorCodes.PIDLArgumentOperationIsInvalid);
        }

        private static void ValidateTenantType(string tenantType)
        {
            if (string.IsNullOrWhiteSpace(tenantType))
            {
                throw new PIDLArgumentException(
                    "Tenant type is either null, empty or whitespaces.",
                    Constants.ErrorCodes.PIDLArgumentTenantTypeIsNullOrEmpty);
            }
        }

        private static void ValidateRewardsType(string rewardsType)
        {
            if (string.IsNullOrWhiteSpace(rewardsType))
            {
                throw new PIDLArgumentException(
                    "Rewards type is either null, empty or whitespaces.",
                    Constants.ErrorCodes.PIDLArgumentRewardsTypeIsNullOrEmpty);
            }
        }

        private static void ValidateTaxIdType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new PIDLArgumentException(
                    "Type is either null, empty or whitespaces.",
                    Constants.ErrorCodes.PIDLArgumentScenarioIsNullOrEmpty);
            }

            if (!domainDictionaries[Constants.DomainDictionaryNames.TaxIdTypes].ContainsKey(type))
            {
                throw new PIDLArgumentException(
                    string.Format("Type \"{0}\" is invalid or not found.", type),
                    Constants.ErrorCodes.PIDLArgumentScenarioIsInvalid);
            }
        }

        private static void ReadDomainDictionaryConfig()
        {
            domainDictionaries = new Dictionary<string, Dictionary<string, string>>(StringComparer.CurrentCultureIgnoreCase);
            using (PIDLConfigParser dictionaryParser = new PIDLConfigParser(
                Helper.GetFullPath(Constants.DataDescriptionFilePaths.DomainDictionariesCSV),
                new ColumnDefinition[]
                {
                    new ColumnDefinition("DictionaryName", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Key",            ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Name",           ColumnConstraint.Optional, ColumnFormat.AlphaNumeric)
                },
                true))
            {
                Dictionary<string, string> currentDictionary = null;
                while (!dictionaryParser.EndOfData)
                {
                    string[] cells = dictionaryParser.ReadValidatedFields();

                    if (string.IsNullOrWhiteSpace(cells[0]))
                    {
                        if (currentDictionary == null)
                        {
                            throw new PIDLConfigException(
                                Constants.DataDescriptionFilePaths.DomainDictionariesCSV,
                                dictionaryParser.LineNumber,
                                string.Format("Name of the first dictionary is missing."),
                                Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                        }
                    }
                    else
                    {
                        if (!domainDictionaries.TryGetValue(cells[0], out currentDictionary))
                        {
                            currentDictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                            domainDictionaries[cells[0]] = currentDictionary;
                        }
                    }

                    string key = cells[1].ToLower();
                    if (currentDictionary.ContainsKey(key))
                    {
                        throw new PIDLConfigException(
                            Constants.DataDescriptionFilePaths.DomainDictionariesCSV,
                            dictionaryParser.LineNumber,
                            string.Format("Duplicate Key \"{0}\" found in the config file.", key),
                            Constants.ErrorCodes.PIDLConfigDuplicateId);
                    }
                    else
                    {
                        currentDictionary[key] = cells[2];
                    }
                }
            }
        }

        private static void AddLinks(string descriptionType, PIDLResource resource, Dictionary<string, RestLink> overrideLinks)
        {
            // Self URLs should not exist for PIDLs where description_type is data. They should be present for other PIDL resources.
            if (descriptionType != Constants.DescriptionTypes.MiscellaneousDescription)
            {
                if (overrideLinks != null)
                {
                    foreach (var key in overrideLinks.Keys)
                    {
                        resource.AddLink(key, overrideLinks[key]);
                    }
                }
            }
        }

        // TODO refactor this code out of the PIDL resource Factory
        private static Dictionary<string, RestLink> GetPISubmitLink(string restResourceName, string country, string language, string operation, string partnerName, string classicProduct, string billableAccountId, bool completePrerequisites, string scenario, List<string> exposedFlightFeatures, string orderId, string pxChallengeSessionId = null)
        {
            // TODO : Generalize this code to arbitriary pidl resource and standardize on the Ids
            var retLinks = new Dictionary<string, RestLink>();

            var usePifdEdge = exposedFlightFeatures?.Contains(Flighting.Features.PXUseEdgePIFD, StringComparer.OrdinalIgnoreCase) ?? false;

            string pifdbaseUrl = usePifdEdge ? @"https://{pifdedge-endpoint}/" : @"https://{pifd-endpoint}/";

            string submitUrl;
            string method;
            if (string.Equals(operation, Constants.PidlOperationTypes.Add) || string.Equals(operation, Constants.PidlOperationTypes.AddAdditional, StringComparison.OrdinalIgnoreCase))
            {
                submitUrl = pifdbaseUrl + "users/{userId}" + "/" + restResourceName + "?country=" + country + "&language=" + language + "&partner=" + partnerName;
                if (!string.IsNullOrEmpty(classicProduct))
                {
                    submitUrl += "&classicProduct=" + classicProduct;
                }

                if (!string.IsNullOrEmpty(billableAccountId))
                {
                    submitUrl += "&billableAccountId=" + WebUtility.UrlEncode(billableAccountId);
                }

                if (completePrerequisites)
                {
                    submitUrl += "&completePrerequisites=" + completePrerequisites;
                }

                if (!string.IsNullOrEmpty(scenario))
                {
                    submitUrl += "&scenario=" + scenario;
                }

                if (!string.IsNullOrEmpty(pxChallengeSessionId))
                {
                    submitUrl += "&pxChallengeSessionId=" + pxChallengeSessionId;
                }

                method = Constants.HTTPVerbs.POST;
            }
            else
            {
                submitUrl = pifdbaseUrl + "users/{userId}" + "/" + restResourceName + "/{id}" + "?language=" + language + "&partner=" + partnerName;

                if (!string.IsNullOrEmpty(classicProduct))
                {
                    submitUrl += "&classicProduct=" + classicProduct;
                }

                if (!string.IsNullOrEmpty(billableAccountId))
                {
                    submitUrl += "&billableAccountId=" + WebUtility.UrlEncode(billableAccountId);
                }

                if (string.Equals(operation, Constants.PidlOperationTypes.Delete) && string.Equals(partnerName, PXCommon.Constants.PartnerNames.XboxSettings, StringComparison.OrdinalIgnoreCase))
                {
                    method = Constants.HTTPVerbs.DELETE;
                }
                else
                {
                    method = Constants.HTTPVerbs.PUT;
                }
            }

            RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = method };

            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.XboxOOBE))
            {
                submitUrlLink.AddHeader("x-ms-flight", Constants.PartnerFlightValues.XboxOOBE);
            }

            retLinks[Constants.ButtonDisplayHintIds.SaveButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.AddButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.SubmitButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.SaveNextButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.SaveContinueButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.PaypalSaveNextButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.PaypalSignInButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.VenmoSignInButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.PaypalQrCodeSignInButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.SendCodeButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.ConfirmationButton] = submitUrlLink;

            var submitLink = new RestLink() { Href = submitUrl, Method = method };
            submitLink.SetErrorCodeExpressions(new[] { "({contextData.ErrorCode})", "({contextData.innererror.code})" });
            submitLink.PropertyName = GetActionPropertyName(Constants.DataDescriptionIds.Address);
            return retLinks;
        }

        private static Dictionary<string, RestLink> GetPIApplyLink(string restResourceName, string country, string language, string operation, string partnerName, List<string> exposedFlightFeatures, string channel = null, string referrerId = null, string sessionId = null, string ocid = null)
        {
            var retLinks = new Dictionary<string, RestLink>();

            var usePifdEdge = exposedFlightFeatures?.Contains(Flighting.Features.PXUseEdgePIFD, StringComparer.OrdinalIgnoreCase) ?? false;
            string pifdbaseUrl = usePifdEdge ? @"https://{pifdedge-endpoint}/" : @"https://{pifd-endpoint}/";

            string url = pifdbaseUrl + "users/{userId}" + "/" + restResourceName + "?language=" + language + "&partner=" + partnerName + "&operation=" + operation + "&country=" + country;

            if (!string.IsNullOrEmpty(sessionId))
            {
                url += "&sessionId=" + sessionId;
            }

            if (!string.IsNullOrEmpty(ocid))
            {
                url += "&ocid=" + ocid;
            }

            string method = Constants.HTTPVerbs.POST;

            RestLink applyUrlLink = new RestLink()
            {
                Href = url,
                Method = method,
                Payload = new
                {
                    cardProduct = Constants.PaymentMethodCardProductTypes.XboxCreditCard,
                    channel = channel,
                    referrerId = referrerId,
                    market = country
                }
            };

            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.XboxOOBE))
            {
                applyUrlLink.AddHeader("x-ms-flight", Constants.PartnerFlightValues.XboxOOBE);
            }

            retLinks[Constants.ButtonDisplayHintIds.SaveContinueButton] = applyUrlLink;
            retLinks[Constants.AutoSubmitIds.XboxCoBrandedCardQrCode] = applyUrlLink;

            return retLinks;
        }

        private static Dictionary<string, RestLink> GetProfileSubmitLink(
            string partnerName,
            string profileType,
            string profileId = null,
            Dictionary<string, string> profileV3Headers = null,
            bool overrideJarvisVersionToV3 = false,
            string operation = null,
            List<string> exposedFlightFeatures = null,
            string scenario = null,
            string country = null,
            PaymentExperienceSetting setting = null)
        {
            // For LegalEntity, the post/put request goes to HAPI
            // For other types, the request goes to Javis if version is V3, or PIFD if version is V2
            if (string.Equals(profileType, Constants.ProfileTypes.Legal, StringComparison.OrdinalIgnoreCase))
            {
                var submitUrlLink = new RestLink()
                {
                    Href = Constants.SubmitUrls.HapiLegalEntityProfileUrlTemplate,
                    Method = GlobalConstants.HttpMethods.Put
                };
                submitUrlLink.SetErrorCodeExpressions(new[] { "({contextData.code}.{contextData.parameters.property_name})", "({contextData.code})" });

                return GetRestLinks(submitUrlLink);
            }
            else
            {
                string templateOrPartner = TemplateHelper.GetSettingTemplate(partnerName, setting, Constants.DescriptionTypes.ProfileDescription, profileType);

                // Operation sets to update if:
                // 1, profileId is passed, it indicates that profile exists and it should be an update operation.
                // 2, profileId is null but operation is set to update/update_patch, this is used by client side prefill update scenario
                if (profileId != null
                    || (string.Equals(operation, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(operation, Constants.PidlOperationTypes.UpdatePatch, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(operation, Constants.PidlOperationTypes.UpdatePartial, StringComparison.OrdinalIgnoreCase)))
                {
                    // For prerequisite scenario, it depends on profileId to decide the operation, set operation to update here for prerequisite scenairo
                    if (string.IsNullOrEmpty(operation))
                    {
                        operation = Constants.AccountResourceOperation.Update;
                    }

                    return GetSubmitLink(GetAccountProfileServiceApiVersion(templateOrPartner, overrideJarvisVersionToV3, country, setting), partnerName, Constants.AccountResourceType.Profile, operation, profileId, profileV3Headers, exposedFlightFeatures, profileType, scenario, setting);
                }
                else
                {
                    return GetSubmitLink(GetAccountProfileServiceApiVersion(templateOrPartner, overrideJarvisVersionToV3, country, setting), partnerName, Constants.AccountResourceType.Profile, Constants.AccountResourceOperation.Create, scenario: scenario, setting: setting, exposedFlightFeatures: exposedFlightFeatures);
                }
            }
        }

        private static Dictionary<string, RestLink> GetSubmitLink(string version, string partner, string resourceType, string operation, string resourceId = null, Dictionary<string, string> extraHeaders = null, List<string> exposedFlightFeatures = null, string profileType = null, string scenario = null, PaymentExperienceSetting setting = null, bool avsSuggest = false, string country = null)
        {
            RestLink submitUrlLink = null;

            string key = string.Format("{0}-{1}-{2}", version, resourceType, operation);

            // TODO: after moving both organization profile and employee profile to Patch, "version3ProfileUpdateKey" should be removed.
            string version3ProfileUpdateKey = Constants.AccountServiceApiVersion.V3 + "-profiles-update";
            string version3ProfileUpdatePartialKey = Constants.AccountServiceApiVersion.V3 + "-profiles-update_partial";
            string version3ProfileUpdatePatchKey = Constants.AccountServiceApiVersion.V3 + "-profiles-update_patch";
            string version3AddressCreate = Constants.AccountServiceApiVersion.V3 + "-addresses-create";

            // Jarvis submit urls must start with Constants.AccountServiceApiVersion.V3 to have the right headers for it
            Dictionary<string, RestLink> submitUrls = new Dictionary<string, RestLink>(StringComparer.CurrentCultureIgnoreCase)
            {
                { "v2-profiles-create", new RestLink() { Href = Constants.SubmitUrls.PifdProfileCreateUrlTemplate, Method = GlobalConstants.HttpMethods.Post } },
                { "v2-profiles-update", new RestLink() { Href = string.Format(Constants.SubmitUrls.PifdProfileUpdateUrlTemplate, resourceId), Method = GlobalConstants.HttpMethods.Post } },
                { "v2-addresses-create", new RestLink() { Href = Constants.SubmitUrls.PifdAddressCreateUrlTemplate, Method = GlobalConstants.HttpMethods.Post } },
                { Constants.AccountServiceApiVersion.V3 + "-profiles-create", new RestLink() { Href = Constants.SubmitUrls.JarvisFdProfileCreateUrlTemplate, Method = GlobalConstants.HttpMethods.Post } },
                { version3ProfileUpdateKey, new RestLink() { Href = string.Format(Constants.SubmitUrls.JarvisFdProfileUpdateUrlTemplate, resourceId), Method = GlobalConstants.HttpMethods.Put } },
                { version3ProfileUpdatePartialKey, new RestLink() { Href = string.Format(Constants.SubmitUrls.JarvisFdProfileUpdateUrlTemplate, resourceId), Method = GlobalConstants.HttpMethods.Patch } },
                { version3ProfileUpdatePatchKey, new RestLink() { Href = Constants.SubmitUrls.JarvisFdProfileUpdateClientPrefillingUrlTemplate, Method = GlobalConstants.HttpMethods.Patch } },
                { version3AddressCreate, new RestLink() { Href = Constants.SubmitUrls.JarvisFdAddressCreateUrlTemplate, Method = GlobalConstants.HttpMethods.Post } }
            };

            foreach (KeyValuePair<string, RestLink> submitUrl in submitUrls)
            {
                // Add the headers for Jarvis submit urls
                if (submitUrl.Key.StartsWith(Constants.AccountServiceApiVersion.V3) && !avsSuggest && !ServerSidePostAddress(key, partner, scenario, setting))
                {
                    submitUrl.Value.AddHeader(Constants.CustomHeaders.ApiVersion, Constants.ApiVersions.JarvisV3);
                    submitUrl.Value.AddHeader(Constants.CustomHeaders.MsCorrelationId, Guid.NewGuid().ToString());
                    submitUrl.Value.AddHeader(Constants.CustomHeaders.MsTrackingId, Guid.NewGuid().ToString());
                }
            }

            if (submitUrls.ContainsKey(key))
            {
                submitUrlLink = submitUrls[key];

                if (extraHeaders != null)
                {
                    foreach (KeyValuePair<string, string> header in extraHeaders)
                    {
                        submitUrlLink.AddHeader(header.Key, header.Value);
                    }
                }

                if ((string.Equals(key, version3AddressCreate, StringComparison.OrdinalIgnoreCase) && PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
                    || IsAVSSubmitButtonUpdateEnabledByPartner(country, setting)
                    || (string.Equals(key, version3AddressCreate, StringComparison.OrdinalIgnoreCase) && avsSuggest && Constants.AvsSuggestEnabledPartners.Contains(partner)))
                {
                    // The condition IsAVSSubmitButtonUpdateEnabledByPartner evaluates to true for playxbox for both address and profile flows
                    // and we don't want to overwrite the submit URL for profile flow as this block updates the submitUrl with PIFD Address Post URL
                    // To achieve that we introduced a new flight feature PXSkipPifdAddressPostForNonAddressesType which will skip the PIFD Address Post URL update for non-address types
                    bool skipPifdAddressPost = !string.IsNullOrEmpty(resourceType) && !string.Equals(resourceType, Constants.AccountResourceType.Address, StringComparison.OrdinalIgnoreCase) && exposedFlightFeatures?.Contains(Flighting.Features.PXSkipPifdAddressPostForNonAddressesType, StringComparer.OrdinalIgnoreCase) == true;
                    if (!skipPifdAddressPost)
                    {
                        // avsSuggest not required if AddressValidation is already implemented
                        string avsSuggestRequired = IsAVSSubmitButtonUpdateEnabledByPartner(country, setting) ? "false" : "true";

                        submitUrlLink.Href = Constants.SubmitUrls.PifdAddressPostUrlTemplate + "?partner=" + partner + "&language=" + Context.Culture + "&avsSuggest=" + avsSuggestRequired;

                        if (!string.IsNullOrEmpty(scenario))
                        {
                            submitUrlLink.Href = submitUrlLink.Href + "&scenario=" + scenario;
                        }

                        submitUrlLink.Method = GlobalConstants.HttpMethods.Post;

                        // Remove the Jarvis V3 ApiVersion header when redirecting to PIFD Address Post URL 
                        // because PIFD doesn't recognize Jarvis-specific header and they cause request failures
                        if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXRemoveJarvisHeadersFromSubmitUrl, StringComparer.OrdinalIgnoreCase) && submitUrlLink.Headers != null && submitUrlLink.Headers.ContainsKey(Constants.CustomHeaders.ApiVersion))
                        {
                            submitUrlLink.Headers.Remove(Constants.CustomHeaders.ApiVersion);
                        }
                    }
                }

                // for profileAddress scenario and amcweb partner, post address to Jarvis through AddressEx API
                if (ServerSidePostAddress(key, partner, scenario, setting))
                {
                    submitUrlLink.Href = $"{Constants.SubmitUrls.PifdAddressPostUrlTemplate}?partner={partner}&language={Context.Culture}&scenario={scenario}&avsSuggest=false";
                    submitUrlLink.Method = GlobalConstants.HttpMethods.Post;
                }

                // Flight to switch update operation from Jarvis to Hapi, define error code expresion to handle Hapi error response
                // Enable the tempalte partner check, to sync with the PXProfileUpdateToHapi flighting, utilized for the profile.
                if (string.Equals(key, version3ProfileUpdatePartialKey, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(profileType, Constants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase)
                    && ((exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase))
                    || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseProfileUpdateToHapi, country, setting)
                    || string.Equals(scenario, Constants.ScenarioNames.TwoColumns, StringComparison.OrdinalIgnoreCase)))
                {
                    submitUrlLink.Href = string.Format(Constants.SubmitUrls.HapiProfileUpdateUrlTemplate, Constants.UseridTypes.MyOrg);
                    submitUrlLink.SetErrorCodeExpressions(new[] { "({contextData.error_code})" });
                }

                // TODO: Once both flights 'PXProfileUpdateToHapi' and 'PXEmployeeProfileUpdateToHapi' are merged, remove the following if condition and merge it to the above condition.
                // Enable the tempalte partner check, to sync with the PXEmployeeProfileUpdateToHapi flighting, utilized for the profile.
                if (string.Equals(key, version3ProfileUpdatePartialKey, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(profileType, Constants.ProfileTypes.Employee, StringComparison.OrdinalIgnoreCase)
                    && ((exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEmployeeProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase))
                    || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseEmployeeProfileUpdateToHapi, country, setting)))
                {
                    submitUrlLink.Href = string.Format(Constants.SubmitUrls.HapiProfileUpdateUrlTemplate, Constants.UseridTypes.Me);
                    submitUrlLink.SetErrorCodeExpressions(new[] { "({contextData.error_code})" });
                }

                // TODO: remove this after switch organization profile and employee profile to PATCH operation
                // TODO: This if block to set the submit action for JarvisForProfile is covered under UpdatePidlSubmitLink-UpdateConsumerProfileSubmitLinkToJarvisPatch feature
                // It can be removed once below partners migrated to use the PSS feature.
                if (string.Equals(version3ProfileUpdateKey, key, StringComparison.OrdinalIgnoreCase)
                    && (string.Equals(partner, Constants.PartnerNames.Webblends, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partner, Constants.PartnerNames.OXOWebDirect, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partner, Constants.PartnerNames.OXODIME, StringComparison.OrdinalIgnoreCase)
                    || PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)))
                {
                    submitUrlLink.Href = Constants.SubmitUrls.JarvisFdProfileUpdateClientPrefillingUrlTemplate;
                    submitUrlLink.Method = GlobalConstants.HttpMethods.Patch;
                }

                if (string.Equals(partner, Constants.PartnerNames.Webblends, StringComparison.OrdinalIgnoreCase) && exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXUseJarvisV3ForCompletePrerequisites, StringComparer.OrdinalIgnoreCase))
                {
                    submitUrlLink.Href = string.Format(Constants.SubmitUrls.JarvisFdProfileUpdateUrlTemplate, resourceId);
                    submitUrlLink.Method = GlobalConstants.HttpMethods.Patch;
                }
            }
            else
            {
                submitUrlLink = new RestLink();
            }

            return GetRestLinks(submitUrlLink);
        }

        private static Dictionary<string, RestLink> GetRestLinks(RestLink submitUrlLink)
        {
            return new Dictionary<string, RestLink>
            {
                { Constants.ButtonDisplayHintIds.SubmitButton, submitUrlLink },
                { Constants.ButtonDisplayHintIds.DoneSubmitButton, submitUrlLink },
                { Constants.ButtonDisplayHintIds.SubmitButtonHidden, submitUrlLink },
                { Constants.ButtonDisplayHintIds.SaveButton, submitUrlLink },
                { Constants.ButtonDisplayHintIds.SaveButtonHidden, submitUrlLink },
                { Constants.ButtonDisplayHintIds.SaveNextButton, submitUrlLink },
                { Constants.ButtonDisplayHintIds.PaypalSaveNextButton, submitUrlLink },
                { Constants.ButtonDisplayHintIds.SaveContinueButton, submitUrlLink },
                { Constants.ButtonDisplayHintIds.SaveAddressButton, submitUrlLink }
            };
        }

        private static bool ServerSidePostAddress(string key, string partner, string scenario, PaymentExperienceSetting setting = null)
        {
            string version3AddressCreate = Constants.AccountServiceApiVersion.V3 + "-addresses-create";
            string templateOrPartner = TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.AddressDescription, Constants.AddressTypes.ShippingV3);

            List<string> supportedScenarios = null;

            return string.Equals(key, version3AddressCreate, StringComparison.OrdinalIgnoreCase)
                    && Constants.ServiceSidePostAddressSupportedPartnersAndScenarios.TryGetValue(templateOrPartner, out supportedScenarios)
                    && !string.IsNullOrEmpty(scenario) && supportedScenarios != null && supportedScenarios.Contains(scenario, StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, RestLink> GetAddressSubmitLink(string partner, string addressType, bool overrideJarvisVersionToV3 = false, bool avsSuggest = false, string scenario = null, PaymentExperienceSetting setting = null, string country = null, List<string> exposedFlightFeatures = null)
        {
            // Add submit links
            Dictionary<string, RestLink> links = new Dictionary<string, RestLink>();
            if (string.Equals(addressType, Constants.AddressTypes.BillingGroup, StringComparison.OrdinalIgnoreCase))
            {
                var submitLink = new RestLink();
                submitLink.SetErrorCodeExpressions(new[] { "({contextData.code}.{contextData.parameters.property_name})", "({contextData.code})" });
                links = GetRestLinks(submitLink);
            }
            else if (string.Equals(addressType, Constants.AddressTypes.HapiServiceUsageAddress, StringComparison.OrdinalIgnoreCase))
            {
                var submitLink = new RestLink() { Href = Constants.SubmitUrls.HapiUpdateServiceUsageAddressUrlTemplate, Method = Constants.HTTPVerbs.POST };
                submitLink.SetErrorCodeExpressions(new[] { "({contextData.error.code})" });
                links = GetRestLinks(submitLink);
            }
            else
            {
                links = GetSubmitLink(GetAccountAddressServiceApiVersion(partner, country, setting, overrideJarvisVersionToV3), partner, Constants.AccountResourceType.Address, Constants.AccountResourceOperation.Create, avsSuggest: avsSuggest, scenario: scenario, setting: setting, country: country, exposedFlightFeatures: exposedFlightFeatures);
            }

            return links;
        }

        private static bool IsModernAccountAddress(string addressType)
        {
            return string.Equals(addressType, Constants.AddressTypes.HapiV1SoldToOrganization, StringComparison.OrdinalIgnoreCase)
                || string.Equals(addressType, Constants.AddressTypes.HapiV1SoldToOrganizationCSP, StringComparison.OrdinalIgnoreCase)
                || string.Equals(addressType, Constants.AddressTypes.HapiV1ShipToOrganization, StringComparison.OrdinalIgnoreCase)
                || string.Equals(addressType, Constants.AddressTypes.HapiV1BillToOrganization, StringComparison.OrdinalIgnoreCase)
                || string.Equals(addressType, Constants.AddressTypes.HapiV1SoldToIndividual, StringComparison.OrdinalIgnoreCase)
                || string.Equals(addressType, Constants.AddressTypes.HapiV1ShipToIndividual, StringComparison.OrdinalIgnoreCase)
                || string.Equals(addressType, Constants.AddressTypes.HapiV1BillToIndividual, StringComparison.OrdinalIgnoreCase)
                || string.Equals(addressType, Constants.AddressTypes.HapiV1, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetModernAccountAddressSubmitHref(string partner, string addressType, string operation)
        {
            if (string.Equals(partner, Constants.PartnerNames.Commercialstores, StringComparison.OrdinalIgnoreCase))
            {
                if ((string.Equals(addressType, Constants.AddressTypes.HapiV1SoldToOrganization, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(operation, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase))
                    || (string.Equals(addressType, Constants.AddressTypes.HapiV1SoldToIndividual, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(operation, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase)))
                {
                    return Constants.SubmitUrls.HapiV1SoldToOrganization;
                }
                else if (string.Equals(addressType, Constants.AddressTypes.HapiV1ShipToOrganization, StringComparison.OrdinalIgnoreCase))
                {
                    return string.Equals(operation, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase)
                        ? Constants.SubmitUrls.HapiV1ShipToOrganizationAdd
                        : Constants.SubmitUrls.HapiV1ShipToOrganizationUpdate;
                }
                else if ((string.Equals(addressType, Constants.AddressTypes.HapiV1BillToOrganization, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(operation, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase))
                        || (string.Equals(addressType, Constants.AddressTypes.HapiV1BillToIndividual, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(operation, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase)))
                {
                    return Constants.SubmitUrls.HapiV1BillToOrganization;
                }
            }

            return null;
        }

        private static Dictionary<string, RestLink> GetBillingGroupSubmitLink(string type, string operation)
        {
            var retLinks = new Dictionary<string, RestLink>();

            if (string.Equals(operation, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase))
            {
                var submitUrl = string.Equals(type, Constants.BillingGroupTypeNames.LightWeightV7, StringComparison.OrdinalIgnoreCase) ? Constants.SubmitUrls.HapiBillingGroupV7BaseUrl : Constants.SubmitUrls.HapiBillingGroupBaseUrl;
                RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.POST };
                retLinks[Constants.ButtonDisplayHintIds.SaveButton] = submitUrlLink;
            }
            else if (string.Equals(operation, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase))
            {
                var submitUrl = string.Equals(type, Constants.BillingGroupTypeNames.LightWeightV7, StringComparison.OrdinalIgnoreCase) ? Constants.SubmitUrls.HapiBillingGroupV7UpdatePONumberUrlTemplate : Constants.SubmitUrls.HapiBillingGroupUpdatePONumberUrlTemplate;
                RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.PATCH };
                retLinks[Constants.ButtonDisplayHintIds.SaveButton] = submitUrlLink;
            }

            return retLinks;
        }

        private static Dictionary<string, RestLink> GetPaySubmitLink(string paySubmitUrl, string backButtonUrl = null)
        {
            var retLinks = new Dictionary<string, RestLink>();
            RestLink submitUrlLink = new RestLink() { Href = paySubmitUrl, Method = Constants.HTTPVerbs.POST };
            RestLink backButtonUrlLink;
            if (backButtonUrl != null)
            {
                backButtonUrlLink = new RestLink() { Href = backButtonUrl, Method = Constants.HTTPVerbs.GET };
                retLinks[Constants.ButtonDisplayHintIds.BackButtonFromPaypal] = backButtonUrlLink;
            }

            retLinks[Constants.ButtonDisplayHintIds.PaySubmitButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.NextToPaypalButton] = submitUrlLink;
            return retLinks;
        }

        private static Dictionary<string, RestLink> GetPaymentTokenSubmitLink(string restResourceName, string partnerName, string country, string language, string piid)
        {
            var retLinks = new Dictionary<string, RestLink>();

            string submitUrl = null;
            RestLink submitUrlLink = null;
            string pifdbaseUrl = @"https://{pifd-endpoint}/";
            submitUrl = pifdbaseUrl + "users/{userId}" + "/" + restResourceName + "?partner=" + partnerName + "&country=" + country + "&language=" + language;
            if (!string.IsNullOrWhiteSpace(piid))
            {
                submitUrl += "&piid=" + piid;
            }

            submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.POST };

            retLinks[Constants.ButtonDisplayHintIds.SubmitButtonHidden] = submitUrlLink;
            return retLinks;
        }

        private static Dictionary<string, RestLink> GetTaxIDSubmitLink(string restResourceName, string partnerName, string country, string language, string profileType, bool isStandalone, string operation, string scenario)
        {
            // TODO : Generalize this code to arbitriary pidl resource and standardize on the Ids
            var retLinks = new Dictionary<string, RestLink>();

            string submitUrl = null;
            RestLink submitUrlLink = null;
            string[] profileTypesForHAPI = { Constants.ProfileTypes.Organization, Constants.ProfileTypes.Employee, Constants.ProfileTypes.Legal };
            if (profileTypesForHAPI.Contains(profileType, StringComparer.OrdinalIgnoreCase))
            {
                string hapiUrl = @"https://{hapi-endpoint}/";
                submitUrl = hapiUrl + "{userId}" + "/taxids";

                if (isStandalone && string.Equals(scenario, Constants.ScenarioNames.WithCountryDropdown, StringComparison.OrdinalIgnoreCase))
                {
                    submitUrl += "?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.organizationId}";
                }

                submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.POST };

                if (PXCommon.Constants.PartnerGroups.IsAzureBasedPartner(partnerName))
                {
                    // Order of the ErrorCodeExpressions is important and changing it can result in unexpected behavior
                    submitUrlLink.SetErrorCodeExpressions(new[] { "({contextData.error.details[0].errorCode})", "({contextData.error.detail.code})", "({contextData.error.code})" });
                }
                else
                {
                    submitUrlLink.SetErrorCodeExpressions(new[] { "({contextData.error.code})" });
                }
            }
            else
            {
                string pifdbaseUrl = @"https://{pifd-endpoint}/";
                submitUrl = pifdbaseUrl + "users/{userId}" + "/" + restResourceName + "?country=" + country + "&language=" + language + "&profileType=" + profileType;
                submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.POST };
            }

            retLinks[Constants.ButtonDisplayHintIds.SaveButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.SaveButtonHidden] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.SubmitButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.SaveNextButton] = submitUrlLink;
            return retLinks;
        }

        private static Dictionary<string, RestLink> GetDeviceBindingSubmitLink(string ntid, string challengeId, string partnerName, string country, string language, string challengeMethodId)
        {
            var retLinks = new Dictionary<string, RestLink>();
            string pifdbaseUrl = @"https://{pifd-endpoint}/";

            // Construct url for submit url
            string submitUrl = pifdbaseUrl + "users/{userId}/tokensEx/" + ntid + "/challenges/" + challengeId + "/validate" + "?country=" + country + "&language=" + language + "&challengeMethodId=" + challengeMethodId + "&partner=" + partnerName;
            
            // Construct url for new code url
            string sendNewCodeUrl = string.Empty; //// TODO: will we get it from NTS/Visa for this?

            RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.POST };
            RestLink sendNewCodeUrlLink = new RestLink() { Href = sendNewCodeUrl, Method = Constants.HTTPVerbs.POST };

            retLinks[Constants.ButtonDisplayHintIds.SaveNextButton] = submitUrlLink;
            retLinks[Constants.DisplayHintIds.SmsNewCodeLink] = sendNewCodeUrlLink;

            return retLinks;
        }

        private static Dictionary<string, RestLink> GetPIResumeSubmitLink(string restResourceName, string language, string partnerName, string classicProduct, string billableAccountId, string piid, bool completePrerequisites, string country)
        {
            var retLinks = new Dictionary<string, RestLink>();
            string pifdbaseUrl = @"https://{pifd-endpoint}/";

            // Construct url for submit url
            string submitUrl = pifdbaseUrl + "users/{userId}" + "/" + restResourceName + "/" + piid + "/resume?language=" + language + "&partner=" + partnerName;
            if (!string.IsNullOrEmpty(classicProduct))
            {
                submitUrl += "&classicProduct=" + classicProduct;
            }

            if (!string.IsNullOrEmpty(billableAccountId))
            {
                submitUrl += "&billableAccountId=" + WebUtility.UrlEncode(billableAccountId);
            }

            if (completePrerequisites)
            {
                submitUrl += string.Format("&completePrerequisites={0}&country={1}", completePrerequisites, country);
            }

            // Construct url for new code url
            string sendNewCodeUrl = pifdbaseUrl + "users/{userId}" + "/" + "paymentInstruments" + "/" + piid + "/sendActivationCode";

            RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.POST };
            RestLink sendNewCodeUrlLink = new RestLink() { Href = sendNewCodeUrl, Method = Constants.HTTPVerbs.POST };

            retLinks[Constants.ButtonDisplayHintIds.OkButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.SaveNextButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.VerifyCodeButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.VerifyPicvButton] = submitUrlLink;
            retLinks[Constants.DisplayHintIds.SmsNewCodeLink] = sendNewCodeUrlLink;

            return retLinks;
        }

        private static RestLink GetPaymentSessionPollingLink(string sessionId)
        {
            string pifdbaseUrl = @"https://{pifd-endpoint}/";
            string submitUrl = pifdbaseUrl + "users/{userId}/paymentSessions/" + sessionId + "/status";

            RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.GET };

            return submitUrlLink;
        }

        private static RestLink GetQrCodeSessionPollingLink(string sessionId)
        {
            string pifdbaseUrl = @"https://{pifd-endpoint}/";
            string pollUrl = pifdbaseUrl + "users/{userId}/secondScreenSessions/" + sessionId + "/qrCodeStatus";

            RestLink pollUrlLink = new RestLink() { Href = pollUrl, Method = Constants.HTTPVerbs.GET };

            return pollUrlLink;
        }

        private static string GetCheckoutPollingLink(string checkoutId, string paymentProviderId)
        {
            string pifdbaseUrl = @"https://{pifd-endpoint}/";
            string pollUrl = pifdbaseUrl + "checkoutsEx/" + checkoutId + "/status?paymentProviderId=" + paymentProviderId;
            return pollUrl;
        }

        private static Dictionary<string, RestLink> GetPILink(string restResourceName, string language, string partnerName, string piid, bool completePrerequisites, string country, string scenario = null, string sessionQueryUrl = null, string classicProduct = null)
        {
            var retLinks = new Dictionary<string, RestLink>();
            string pifdbaseUrl = @"https://{pifd-endpoint}/";

            string submitUrl = pifdbaseUrl + "users/{userId}" + "/" + restResourceName + "/" + piid + "?language=" + language + "&partner=" + partnerName + "&country=" + country;

            if (completePrerequisites)
            {
                submitUrl += string.Format("&completePrerequisites={0}", completePrerequisites);
            }

            RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.GET };

            retLinks[Constants.ButtonDisplayHintIds.IdealYesButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.IdealDoneButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.GenericYesButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.PaypalYesButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.PaypalRedirectSubmitButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.VenmoYesButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.AlipayContinueButton] = submitUrlLink;

            if (!string.IsNullOrEmpty(scenario))
            {
                // Currently scenario = "azureibiza" is used only in India 3DS scenario
                submitUrlLink.Href = submitUrlLink.Href + "&scenario=" + scenario;
            }

            if (!string.IsNullOrEmpty(sessionQueryUrl))
            {
                // Currently sessionQueryUrl is used only in India 3DS scenario
                submitUrlLink.Href = submitUrlLink.Href + "&sessionQueryUrl=" + WebUtility.UrlEncode(sessionQueryUrl);
            }

            if (!string.IsNullOrEmpty(classicProduct))
            {
                // Currently classicProduct is used only in Azure scenario
                submitUrlLink.Href = submitUrlLink.Href + "&classicProduct=" + classicProduct;
            }

            retLinks[Constants.ButtonDisplayHintIds.Cc3DSYesButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.Cc3DSYesVerificationButton] = submitUrlLink;

            return retLinks;
        }

        private static Dictionary<string, RestLink> GetRDSSessionStatusLink(string rdsSessionId, string paymentSessionId, string partnerName, string country, string language, string scenario, string paymentMethodFamilyTypeId = null)
        {
            var retLinks = new Dictionary<string, RestLink>();
            string pifdbaseUrl = @"https://{pifd-endpoint}/";
            string submitUrl = pifdbaseUrl + "anonymous/rdssession/query?sessionid=" + rdsSessionId + "&country=" + country + "&language=" + language + "&partner=" + partnerName + "&scenario=" + scenario;

            RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.POST, Payload = new { paymentSessionId = paymentSessionId, paymentMethodFamilyTypeId = paymentMethodFamilyTypeId } };
            retLinks[Constants.ButtonDisplayHintIds.LegacyBillDesk3DSYesButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.Cc3DSYesButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.Cc3DSYesVerificationButton] = submitUrlLink;

            return retLinks;
        }

        private static Dictionary<string, RestLink> GetBitcoinRedeemLink(string piid, string country, string language, string partnerName, string amount = null, string currency = null, string referenceId = null, string greenId = null)
        {
            var retLinks = new Dictionary<string, RestLink>();
            string pifdbaseUrl = @"https://{pifd-endpoint}/";

            string submitUrl = pifdbaseUrl + "users/{userId}" + "/" + Constants.RestResourceNames.PaymentInstrumentsEx + "/" + piid + "/redeem?language=" + language + "&partner=" + partnerName + "&country=" + country;

            if (!string.IsNullOrEmpty(amount))
            {
                submitUrl = submitUrl + "&amount=" + amount;
            }

            if (!string.IsNullOrEmpty(currency))
            {
                submitUrl = submitUrl + "&currency=" + currency;
            }

            if (!string.IsNullOrEmpty(referenceId))
            {
                submitUrl = submitUrl + "&referenceId=" + referenceId;
            }

            if (!string.IsNullOrEmpty(greenId))
            {
                submitUrl = submitUrl + "&greenId=" + greenId;
            }

            RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.POST };

            retLinks[Constants.ButtonDisplayHintIds.SaveNextButton] = submitUrlLink;

            return retLinks;
        }

        private static Dictionary<string, RestLink> GetCvv3DSSubmitLink(string sessionId, string partner, string scenario, string classicProduct = null)
        {
            var retLinks = new Dictionary<string, RestLink>();
            string pifdbaseUrl = @"https://{pifd-endpoint}/";

            string submitUrl;

            submitUrl = string.Equals(scenario, Constants.ScenarioNames.IndiaThreeDS, StringComparison.OrdinalIgnoreCase)
                ? pifdbaseUrl + "users/{userId}" + "/" + Constants.RestResourceNames.PaymentSessions + "/" + sessionId + "/browserAuthenticateThreeDSOne?partner=" + partner
                : pifdbaseUrl + "users/{userId}" + "/" + Constants.RestResourceNames.PaymentSessions + "/" + sessionId + "/authenticateIndiaThreeDS?partner=" + partner;

            if (!string.IsNullOrEmpty(classicProduct))
            {
                submitUrl = submitUrl + "&classicProduct=" + classicProduct;
            }

            RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.POST };

            string submitButtonId = Constants.ButtonDisplayHintIds.Cvv3DSSubmitButton;

            retLinks[submitButtonId] = submitUrlLink;

            return retLinks;
        }

        private static Dictionary<string, RestLink> GetSmsValidationSubmitLink(string sessionId, PaymentInstrument pi)
        {
            var retLinks = new Dictionary<string, RestLink>();
            string pifdbaseUrl = @"https://{pifd-endpoint}/";

            string submitUrl;

            submitUrl = pifdbaseUrl + "users/me/challenge/Sms/validate";

            RestLink submitUrlLink = new RestLink()
            {
                Href = submitUrl,
                Method = Constants.HTTPVerbs.POST
            };

            // TODO: Check if trycatch is needed. The other link functions do not have it. 
            try
            {
                string submitButtonId = Constants.ButtonDisplayHintIds.OkButton;
                retLinks[submitButtonId] = submitUrlLink;
            }
            catch (Exception ex)
            {
                throw new PIDLException("Could not find okButton id." + ex.ToString(), Constants.ErrorCodes.PIDLConfigUnknownDisplayHintId);
            }

            return retLinks;
        }

        private static Dictionary<string, RestLink> GetCsvVerifyLink()
        {
            var retLinks = new Dictionary<string, RestLink>();
            string pifdbaseUrl = @"https://{tops-endpoint}/";

            string submitUrl = pifdbaseUrl + "users/{userId}" + "/" + Constants.RestResourceNames.TokenDescriptionRequests;

            RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.POST };

            retLinks[Constants.ButtonDisplayHintIds.CsvRedeemVerifyBalanceButton] = submitUrlLink;

            return retLinks;
        }

        private static Dictionary<string, RestLink> GetCsvRedeemLink()
        {
            var retLinks = new Dictionary<string, RestLink>();
            string pifdbaseUrl = @"https://{purchase-endpoint}/";

            string submitUrl = pifdbaseUrl + "users/{userId}" + "/" + Constants.RestResourceNames.Orders;

            RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.POST };

            retLinks[Constants.ButtonDisplayHintIds.CsvRedeemAddBalanceButton] = submitUrlLink;

            return retLinks;
        }

        private static void AddPollActionContext(PIDLResource pidlResource, string restResourceName, string language, string partnerName, string piid, bool completePrerequisites, string country, string sessionId, string scenario, List<string> exposedFeatureFlights)
        {
            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());

            string pifdbaseUrl = @"https://{pifd-endpoint}/";
            string pollUrl = pifdbaseUrl + "users/{userId}" + "/" + restResourceName + "/" + piid + "?language=" + language + "&partner=" + partnerName + "&country=" + country + "&sessionQueryUrl=" + WebUtility.UrlEncode(string.Format("sessions/{0}", sessionId)) + string.Format("&scenario={0}", scenario);

            if (completePrerequisites)
            {
                pollUrl += string.Format("&completePrerequisites={0}", completePrerequisites);
            }

            PollActionContext pollActionContext = new PollActionContext()
            {
                Href = pollUrl,
                Method = Constants.HTTPVerbs.GET,
                Interval = Constants.PollingIntervals.PaypalPollingIntervalDefault,
                ResponseResultExpression = Constants.PollingResponseResultExpression.GenericResponseResultExpression,
                CheckPollingTimeOut = false,
            };

            pollActionContext.Interval = Constants.PollingIntervals.GenericPollingInterval;

            pollActionContext.CheckPollingTimeOut = true;
            pollActionContext.MaxPollingAttempts = Constants.PollingMaxAttempts.GenericPollingMaxTimes;

            pollActionContext.AddResponseActionsItem("Active", new DisplayHintAction(DisplayHintActionType.success.ToString()));
            pollActionContext.AddResponseActionsItem("Declined", new DisplayHintAction(DisplayHintActionType.gohome.ToString()));
            pollAction.Context = pollActionContext;

            PageDisplayHint pollingPage = pidlResource.DisplayPages[0];
            pollingPage.Action = pollAction;
        }

        private static void AddQrCodePollActionContext(string challengeDescriptionType, PIDLResource pidlResource, string restResourceName, string language, string partnerName, string piid, bool completePrerequisites, string country, List<string> exposedFlightFeatures, string sessionId, string scenario = null, string orderId = null, string sessionQueryUrl = null, string pollingUrlTemplate = null, RestLink restLink = null, string accountId = null)
        {
            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());
            if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.PaypalQrCode, StringComparison.OrdinalIgnoreCase)
                || (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.AlipayQrCode, StringComparison.OrdinalIgnoreCase) && string.Equals(partnerName, PXCommon.Constants.PartnerNames.XboxSettings, StringComparison.OrdinalIgnoreCase))
                || string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.VenmoQrCode, StringComparison.OrdinalIgnoreCase))
            {
                var qrCodePollingPage = pidlResource.DisplayPages[0];
                if (qrCodePollingPage != null)
                {
                    if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.VenmoQrCode, StringComparison.OrdinalIgnoreCase))
                    {
                        scenario = Constants.ScenarioNames.VenmoQRCode;
                    }

                    string pifdbaseUrl = @"https://{pifd-endpoint}/";
                    string pollUrl = pifdbaseUrl + "users/{userId}" + "/" + restResourceName + "/" + piid + "?language=" + language + "&partner=" + partnerName + "&country=" + country + "&" + "sessionQueryUrl=" + WebUtility.UrlEncode(string.Format("sessions/{0}", sessionId)) + "&scenario=" + scenario;

                    // Alipay shouldn't have sessionQueryUrl or scenario in the url. Everything else remains the same as Paypal 
                    if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.AlipayQrCode, StringComparison.OrdinalIgnoreCase))
                    {
                        pollUrl = pifdbaseUrl + "users/{userId}" + "/" + restResourceName + "/" + piid + "?language=" + language + "&partner=" + partnerName + "&country=" + country;
                    }

                    if (completePrerequisites)
                    {
                        pollUrl += string.Format("&completePrerequisites={0}", completePrerequisites);
                    }

                    PollActionContext pollActionContext = new PollActionContext()
                    {
                        Href = pollUrl,
                        Method = Constants.HTTPVerbs.GET,
                        Interval = Constants.PollingIntervals.PaypalPollingIntervalDefault,
                        ResponseResultExpression = Constants.PollingResponseResultExpression.PaypalResponseResultExpression,
                        CheckPollingTimeOut = false,
                    };

                    if (exposedFlightFeatures?.Contains(Flighting.Features.PXSetPayPal2ndScreenPollingIntervalFiveSeconds, StringComparer.OrdinalIgnoreCase) ?? false)
                    {
                        pollActionContext.Interval = Constants.PollingIntervals.PaypalPollingIntervalFiveSeconds;
                    }
                    else if (exposedFlightFeatures?.Contains(Flighting.Features.PXSetPayPal2ndScreenPollingIntervalTenSeconds, StringComparer.OrdinalIgnoreCase) ?? false)
                    {
                        pollActionContext.Interval = Constants.PollingIntervals.PaypalPollingIntervalTenSeconds;
                    }
                    else if (exposedFlightFeatures?.Contains(Flighting.Features.PXSetPayPal2ndScreenPollingIntervalFifteenSeconds, StringComparer.OrdinalIgnoreCase) ?? false)
                    {
                        pollActionContext.Interval = Constants.PollingIntervals.PaypalPollingIntervalFifteenSeconds;
                    }

                    pollActionContext.CheckPollingTimeOut = true;
                    pollActionContext.MaxPollingAttempts = Constants.PollingMaxAttempts.PayPalPollingMaxTimeSixHundred;

                    pollActionContext.AddResponseActionsItem("Active", new DisplayHintAction(DisplayHintActionType.success.ToString()));
                    pollActionContext.AddResponseActionsItem("Declined", new DisplayHintAction(DisplayHintActionType.gohome.ToString()));
                    pollAction.Context = pollActionContext;
                    qrCodePollingPage.Action = pollAction;
                }
            }
            else if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.CreditCardQrCode, StringComparison.OrdinalIgnoreCase) && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.PxEnableAddCcQrCode))
            {
                var qrCodePollingPage = pidlResource.DisplayPages[0];
                if (qrCodePollingPage != null)
                {
                    RestLink pollUrl = GetQrCodeSessionPollingLink(sessionId);

                    PollActionContext pollActionContext = new PollActionContext()
                    {
                        Href = pollUrl.Href,
                        Method = Constants.HTTPVerbs.GET,
                        Interval = Constants.PollingIntervals.PaypalPollingIntervalDefault,
                        ResponseResultExpression = Constants.PollingResponseResultExpression.PaypalResponseResultExpression,
                        CheckPollingTimeOut = true,
                        MaxPollingAttempts = Constants.PollingMaxAttempts.PayPalPollingMaxTimeSixHundred,
                    };

                    pollActionContext.AddResponseActionsItem("Active", new DisplayHintAction(DisplayHintActionType.success.ToString()));
                    pollActionContext.AddResponseActionsItem("Declined", new DisplayHintAction(DisplayHintActionType.gohome.ToString()));
                    pollActionContext.AddResponseActionsItem("Banned", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));

                    pollAction.Context = pollActionContext;
                    qrCodePollingPage.Action = pollAction;
                }
            }
            else if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.GlobalPIQrCode, StringComparison.OrdinalIgnoreCase))
            {
                var qrCodePollingPage = pidlResource.DisplayPages[0];
                if (qrCodePollingPage != null)
                {
                    string pollUrl = string.Format(Constants.HandleGlobalQrCodePIPendingPurchaseUrls.QueryGlobalPendingPurchaseStateRedirectionServiceUrlTemplate, sessionId);
                    PollActionContext pollActionContex = new PollActionContext()
                    {
                        Href = pollUrl,
                        Method = Constants.HTTPVerbs.GET,
                        Interval = Constants.PollingIntervals.GlobalPollingInterval,
                        ResponseResultExpression = Constants.PollingResponseResultExpression.GlobalPIResponseKeyExpressionForRedirectionService,
                        CheckPollingTimeOut = false,
                    };

                    pollActionContex.AddResponseActionsItem("pending", new DisplayHintAction(DisplayHintActionType.moveNextAndPoll.ToString()));
                    pollActionContex.AddResponseActionsItem("failure", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
                    pollAction.Context = pollActionContex;
                    qrCodePollingPage.Action = pollAction;
                    pollAction.NextAction = AddGlobalPISecondPollActionContext(sessionId, orderId, piid);
                }
            }
            else if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.ThreeDSOneQrCode, StringComparison.OrdinalIgnoreCase))
            {
                var qrCodePollingPage = pidlResource.DisplayPages[1];
                if (qrCodePollingPage != null)
                {
                    string pifdbaseUrl = @"https://{pifd-endpoint}/";
                    string pollUrl = pifdbaseUrl + "users/{userId}" + "/" + restResourceName + "/" + piid + "?language=" + language + "&partner=" + partnerName + "&country=" + country + "&" + "sessionQueryUrl=" + sessionQueryUrl + "&scenario=" + Constants.ScenarioNames.ThreeDSOnePolling;

                    if (completePrerequisites)
                    {
                        pollUrl += string.Format("&completePrerequisites={0}", completePrerequisites);
                    }

                    PollActionContext pollActionContext = new PollActionContext()
                    {
                        Href = pollUrl,
                        Method = Constants.HTTPVerbs.GET,
                        Interval = Constants.PollingIntervals.ThreeDSOnePollingInterval,
                        ResponseResultExpression = Constants.PollingResponseResultExpression.ThreeDSOneResponseResultExpression,
                        CheckPollingTimeOut = false,
                    };

                    pollActionContext.AddResponseActionsItem("Active", new DisplayHintAction(DisplayHintActionType.success.ToString()));
                    pollActionContext.AddResponseActionsItem("Declined", new DisplayHintAction(DisplayHintActionType.gohome.ToString()));
                    pollAction.Context = pollActionContext;
                    qrCodePollingPage.Action = pollAction;
                }
            }
            else if (string.Equals(challengeDescriptionType, Constants.ChallengeDescriptionTypes.XboxCoBrandedCard, StringComparison.OrdinalIgnoreCase))
            {
                var qrCodePollingPage = pidlResource.DisplayPages[0];
                if (qrCodePollingPage != null)
                {
                    string pollUrl = pollingUrlTemplate == null ?
                        string.Format(Constants.HandleGlobalQrCodePIPendingPurchaseUrls.XboxCoBrandedCardRedirectionUrlTemplate, sessionId) :
                        string.Format(pollingUrlTemplate, sessionId);

                    PollActionContext pollActionContex = new PollActionContext()
                    {
                        Href = pollUrl,
                        Method = Constants.HTTPVerbs.GET,
                        Interval = Constants.PollingIntervals.XboxCardPollingInterval,
                        ResponseResultExpression = Constants.PollingResponseResultExpression.XboxCoBrandedCardPendingResultExpression,
                        CheckPollingTimeOut = false,
                    };

                    pollActionContex.AddResponseActionsItem("pending", new DisplayHintAction(DisplayHintActionType.moveNextAndPoll.ToString()));
                    pollActionContex.AddResponseActionsItem("failure", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
                    pollAction.Context = pollActionContex;
                    qrCodePollingPage.Action = pollAction;
                    pollAction.NextAction = AddXboxCoBrandedCardPollSecondActionContext(sessionId, orderId, piid, partnerName, country, pollingUrlTemplate, restLink);
                }
            }
        }

        private static DisplayHintAction AddXboxCoBrandedCardPollSecondActionContext(string sessionId, string orderId, string piid, string partnerName, string country, string pollingUrlTemplate = null, RestLink restLink = null)
        {
            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());
            string pollUrl = pollingUrlTemplate == null ?
                string.Format(Constants.HandleGlobalQrCodePIPendingPurchaseUrls.XboxCoBrandedCardRedirectionUrlTemplate, sessionId) :
                string.Format(pollingUrlTemplate, sessionId);

            PollActionContext pollActionContex = new PollActionContext()
            {
                Href = pollUrl,
                Method = Constants.HTTPVerbs.GET,
                Interval = Constants.PollingIntervals.GlobalPollingInterval,
                ResponseResultExpression = Constants.PollingResponseResultExpression.XboxCoBrandedCardPendingResultExpression,
                CheckPollingTimeOut = false,
            };

            pollActionContex.AddResponseActionsItem("success", new DisplayHintAction(DisplayHintActionType.updatePoll.ToString()));
            pollActionContex.AddResponseActionsItem("failure", new DisplayHintAction(DisplayHintActionType.updatePoll.ToString()));
            pollAction.Context = pollActionContex;
            pollAction.NextAction = AddXboxCoBrandedCardPollThirdActionContext(sessionId, orderId, piid, partnerName, country, pollingUrlTemplate, restLink);

            return pollAction;
        }

        private static DisplayHintAction AddXboxCoBrandedCardPollThirdActionContext(string sessionId, string orderId, string piid, string partnerName, string country, string pollingUrlTemplate = null, RestLink restLink = null)
        {
            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());
            string pifdbaseUrl = @"https://{pifd-endpoint}/";
            string pollUrl = pifdbaseUrl + "users/{userId}" + "/paymentInstrumentsEx/notApplicable?partner=" + partnerName + "&country=" + country + "&sessionId=" + sessionId + "&scenario=xboxCoBrandedCard";

            PollActionContext pollActionContex = new PollActionContext()
            {
                Href = pollUrl,
                Method = Constants.HTTPVerbs.GET,
                Interval = Constants.PollingIntervals.GlobalPollingInterval,
                ResponseResultExpression = Constants.PollingResponseResultExpression.XboxCoBrandedCardFinalResultExpression,
                CheckPollingTimeOut = false,
            };

            DisplayHintAction cancelledDisplayAction = new DisplayHintAction(
                restLink != null ?
                DisplayHintActionType.restAction.ToString() :
                DisplayHintActionType.gohome.ToString());
            if (restLink != null)
            {
                cancelledDisplayAction.Context = restLink;
            }

            pollActionContex.AddResponseActionsItem("Active", new DisplayHintAction(DisplayHintActionType.success.ToString()));
            pollActionContex.AddResponseActionsItem("Pending", new DisplayHintAction(DisplayHintActionType.success.ToString()));
            pollActionContex.AddResponseActionsItem("Declined", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
            pollActionContex.AddResponseActionsItem("Unknown", new DisplayHintAction(DisplayHintActionType.success.ToString()));
            pollActionContex.AddResponseActionsItem("Cancelled", cancelledDisplayAction);
            pollAction.Context = pollActionContex;

            return pollAction;
        }

        private static DisplayHintAction AddGlobalPISecondPollActionContext(string sessionId, string orderId, string piid)
        {
            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());
            string pollUrl = string.Format(Constants.HandleGlobalQrCodePIPendingPurchaseUrls.QueryGlobalPendingPurchaseStateRedirectionServiceUrlTemplate, sessionId);
            PollActionContext pollActionContex = new PollActionContext()
            {
                Href = pollUrl,
                Method = Constants.HTTPVerbs.GET,
                Interval = Constants.PollingIntervals.GlobalPollingInterval,
                ResponseResultExpression = Constants.PollingResponseResultExpression.GlobalPIResponseKeyExpressionForRedirectionService,
                CheckPollingTimeOut = false,
            };

            pollActionContex.AddResponseActionsItem("success", new DisplayHintAction(DisplayHintActionType.updatePoll.ToString()));
            pollActionContex.AddResponseActionsItem("failure", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
            pollAction.Context = pollActionContex;
            pollAction.NextAction = AddGlobalPIThirdPollActionContext(sessionId, orderId, piid);
            return pollAction;
        }

        private static DisplayHintAction AddUPISecondPollActionContextConfirmPayment(string sessionId, string orderId, string piid, List<string> flights = null)
        {
            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());
            string pollUrl = string.Format(Constants.ConfirmPaymentForUPIUrls.QueryUPIPendingStateRedirectionServicePollUrlTemplate, sessionId);
            PollActionContext pollActionContex = new PollActionContext()
            {
                Href = pollUrl,
                Method = Constants.HTTPVerbs.GET,
                Interval = Constants.PollingIntervals.GlobalPollingInterval,
                ResponseResultExpression = Constants.PollingResponseResultExpression.GlobalPIResponseKeyExpressionForRedirectionService,
                CheckPollingTimeOut = false,
            };

            pollActionContex.AddResponseActionsItem("failure", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));

            // If the purchase flight is enabled, then add the next action to poll the purchase service
            if (flights?.Contains(V7.Constants.PartnerFlightValues.PXEnablePurchasePollingForUPIConfirmPayment, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                pollActionContex.AddResponseActionsItem("success", new DisplayHintAction(DisplayHintActionType.updatePoll.ToString()));
                pollAction.NextAction = AddGlobalPIThirdPollActionContext(sessionId, orderId, piid);
            }
            else
            {
                pollActionContex.AddResponseActionsItem("success", new DisplayHintAction(DisplayHintActionType.success.ToString()));
            }

            pollAction.Context = pollActionContex;
            return pollAction;
        }

        private static DisplayHintAction AddGlobalPIThirdPollActionContext(string sessionId, string orderId, string piid)
        {
            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());

            string pollUrl = string.Format(Constants.HandleGlobalQrCodePIPendingPurchaseUrls.QueryGlobalPendingPurchaseStatePurchaseServiceUrlTemplate, orderId);
            MicrosoftMarketplaceServicesPurchaseServiceContractsV7ClientContextV7 clientContext = new MicrosoftMarketplaceServicesPurchaseServiceContractsV7ClientContextV7(Constants.ClientName.SecondScreen);
            MicrosoftMarketplaceServicesPurchaseServiceContractsV7BillingInformationV7 billingInformation = new MicrosoftMarketplaceServicesPurchaseServiceContractsV7BillingInformationV7(piid, sessionId);
            MicrosoftMarketplaceServicesPurchaseServiceContractsV7UpdateOrderRequestV7 updateOrderRequestPayload = new MicrosoftMarketplaceServicesPurchaseServiceContractsV7UpdateOrderRequestV7(clientContext, billingInformation, orderState: Constants.PurchaseOrderState.Purchased);
            updateOrderRequestPayload.Validate();
            PollActionContext pollActionContex = new PollActionContext()
            {
                Href = pollUrl,
                Method = Constants.HTTPVerbs.PUT,
                Interval = Constants.PollingIntervals.GlobalPollingInterval,
                ResponseResultExpression = Constants.PollingResponseResultExpression.GlobalPIResponseKeyExpressionForPurchaseService,
                CheckPollingTimeOut = false,
                Payload = updateOrderRequestPayload
            };

            pollActionContex.AddResponseActionsItem("Purchased", new DisplayHintAction(DisplayHintActionType.success.ToString()));
            pollActionContex.AddResponseActionsItem("Canceled", new DisplayHintAction(DisplayHintActionType.handleFailure.ToString()));
            pollAction.Context = pollActionContex;

            return pollAction;
        }

        private static void AddGenericPollActionContext(PIDLResource pidlResource, string restResourceName, string language, string partnerName, string piid, string country)
        {
            DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());

            var pollingPage = pidlResource.DisplayPages[0];
            if (pollingPage != null)
            {
                string pifdbaseUrl = @"https://{pifd-endpoint}/";
                string pollUrl = pifdbaseUrl + "users/{userId}" + "/" + restResourceName + "/" + piid + "?language=" + language + "&partner=" + partnerName + "&country=" + country;
                PollActionContext pollActionContext = new PollActionContext()
                {
                    Href = pollUrl,
                    Method = Constants.HTTPVerbs.GET,
                    Interval = Constants.PollingIntervals.GenericPollingInterval,
                    ResponseResultExpression = Constants.PollingResponseResultExpression.GenericResponseResultExpression,
                    CheckPollingTimeOut = false,
                    MaxPollingAttempts = 10
                };

                pollActionContext.AddResponseActionsItem("Active", new DisplayHintAction(DisplayHintActionType.success.ToString()));
                pollActionContext.AddResponseActionsItem("Removed", new DisplayHintAction(DisplayHintActionType.success.ToString()));
                pollAction.Context = pollActionContext;
                pollingPage.Action = pollAction;
            }
        }

        private static Dictionary<string, RestLink> GetUpdateBillingAgreementTypeLink(string restResourceName, string piid)
        {
            var retLinks = new Dictionary<string, RestLink>();
            string pifdbaseUrl = @"https://{pifd-endpoint}/";

            string submitUrl = pifdbaseUrl + "users/{userId}" + "/" + restResourceName + "/" + piid + "/updateBillingAgreementType";

            RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.POST };

            retLinks[Constants.ButtonDisplayHintIds.AgreeAndContinueButton] = submitUrlLink;
            retLinks[Constants.ButtonDisplayHintIds.AgreeAndPayButton] = submitUrlLink;

            return retLinks;
        }

        private static void AddSubmitLinks(Dictionary<string, RestLink> links, List<PIDLResource> pidlResources, bool addSecondaryLinks = false)
        {
            if (links == null)
            {
                return;
            }

            foreach (var pidlResource in pidlResources)
            {
                // There might be some pidl resources that might not have display pages. In that case do not try to add a submit link
                if (pidlResource.DisplayPages == null)
                {
                    continue;
                }

                foreach (var linkId in links.Keys)
                {
                    var displayHintButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(linkId, pidlResource.DisplayPages);
                    if (displayHintButton != null)
                    {
                        if (displayHintButton.Action != null)
                        {
                            if (addSecondaryLinks)
                            {
                                displayHintButton.Action.Context2 = links[linkId];
                            }
                            else
                            {
                                displayHintButton.Action.Context = links[linkId];
                            }
                        }
                    }
                    else
                    {
                        var displayHintText = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<HyperlinkDisplayHint>(linkId, pidlResource.DisplayPages);
                        if (displayHintText != null)
                        {
                            if (displayHintText.Action != null)
                            {
                                if (addSecondaryLinks)
                                {
                                    displayHintText.Action.Context2 = links[linkId];
                                }
                                else
                                {
                                    displayHintText.Action.Context = links[linkId];
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void AddAddressValidationLinks(List<PIDLResource> retList, string partner, string type, string scenario = null, PaymentExperienceSetting setting = null)
        {
            RestLink validateLink = GetValidationLink(partner, type, scenario, null, setting);

            string buttonId = Constants.ButtonDisplayHintIds.SaveWithValidationButton;

            // Enable the tempalte partner check, to sync with the commercialstores partner.
            if (string.Equals(partner, Constants.PartnerNames.Azure, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partner, Constants.PartnerNames.AzureSignup, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partner, Constants.PartnerNames.AzureIbiza, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partner, Constants.PartnerNames.CommercialSupport, StringComparison.OrdinalIgnoreCase)
                || ((string.Equals(partner, Constants.PartnerNames.Commercialstores, StringComparison.OrdinalIgnoreCase)
                || TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.AddressDescription, type)))
                && string.Equals(scenario, Constants.ScenarioNames.ModernAccount, StringComparison.OrdinalIgnoreCase))
                || IsModernAccountAddress(type))
            {
                buttonId = Constants.ButtonDisplayHintIds.ValidateButtonHidden;
            }

            foreach (PIDLResource resource in retList)
            {
                if (resource.DisplayPages != null)
                {
                    var validateButton = resource.GetDisplayHintById(buttonId) as ButtonDisplayHint;
                    if (validateButton != null)
                    {
                        validateButton.Action.Context = validateLink;
                        if (IsModernAccountAddress(type))
                        {
                            validateButton.Action.ActionType = DisplayHintActionType.validate.ToString();
                        }
                    }
                }
            }
        }

        private static void AddAddressValidationThenSumbitLinks(List<PIDLResource> retList, string partner, string type, string operation, string scenario, List<string> exposedFlightFeatures = null, PaymentExperienceSetting setting = null)
        {
            if ((string.Equals(partner, Constants.PartnerNames.Commercialstores, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partner, Constants.PartnerNames.Azure, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partner, Constants.PartnerNames.AzureSignup, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partner, Constants.PartnerNames.AzureIbiza, StringComparison.OrdinalIgnoreCase)
                    || TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.AddressDescription, type)))
                && IsModernAccountAddress(type))
            {
                string href = GetModernAccountAddressSubmitHref(partner, type, operation);
                RestLink submitLink = string.IsNullOrEmpty(href) ? GetEmptySubmitLink() : GetHapiModernAccountLink(href);
                RestLink validateLink = GetValidationLink(partner, type, scenario, exposedFlightFeatures);

                string buttonId = Constants.ButtonDisplayHintIds.ValidateThenSubmitButtonHidden;
                foreach (PIDLResource resource in retList)
                {
                    if (resource.DisplayPages != null)
                    {
                        var validateWithSubmitButton = resource.GetDisplayHintById(buttonId) as ButtonDisplayHint;
                        if (validateWithSubmitButton == null)
                        {
                            validateWithSubmitButton = resource.GetDisplayHintById(Constants.ButtonDisplayHintIds.ValidateThenSubmitButton) as ButtonDisplayHint;
                        }

                        if (validateWithSubmitButton != null)
                        {
                            validateWithSubmitButton.Action = new DisplayHintAction(DisplayHintActionType.validate.ToString());
                            validateWithSubmitButton.Action.Context = validateLink;
                            validateWithSubmitButton.Action.NextAction = new DisplayHintAction(DisplayHintActionType.submit.ToString());
                            validateWithSubmitButton.Action.NextAction.Context = submitLink;
                        }
                    }
                }
            }
        }

        private static void AddAddressValidationThenSuccessWithPayloadLinks(List<PIDLResource> retList, string type, string partner, string language, string country, List<string> exposedFlightFeatures, PaymentExperienceSetting setting = null)
        {
            var validateLink = new RestLink();
            if (IsModernAccountAddress(type))
            {
                validateLink = GetValidationLink(partner, type, null, exposedFlightFeatures);
            }
            else
            {
                validateLink.Method = Constants.HTTPVerbs.POST;
                validateLink.SetErrorCodeExpressions(new[] { "({contextData.innererror.code})", "({contextData.code})" });
                validateLink.PropertyName = GetActionPropertyName(Constants.DataDescriptionIds.Address);
                validateLink.Href = Constants.SubmitUrls.PifdAnonymousLegacyAddressValidationUrl;

                if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.ShowAVSSuggestions, StringComparer.OrdinalIgnoreCase))
                {
                    string modernAVSForTradeScenario = Constants.ScenarioNames.SuggestAddressesTradeAVS;
                    if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.TradeAVSUsePidlModalInsteadofPidlPage, StringComparer.OrdinalIgnoreCase))
                    {
                        modernAVSForTradeScenario = Constants.ScenarioNames.SuggestAddressesTradeAVSUsePidlModal;
                    }

                    validateLink.Href = string.Format(Constants.SubmitUrls.PifdAnonymousModernAVSForTrade, type, partner, language, modernAVSForTradeScenario, country);
                }
            }

            string buttonId = Constants.ButtonDisplayHintIds.ValidateThenSuccessWithPayloadButton;
            string hiddenButtonId = Constants.ButtonDisplayHintIds.ValidateThenSuccessWithPayloadButtonHidden;
            foreach (PIDLResource resource in retList)
            {
                if (resource.DisplayPages != null)
                {
                    var validateWithSubmitButton = resource.GetDisplayHintById(buttonId) as ButtonDisplayHint;
                    if (validateWithSubmitButton == null)
                    {
                        validateWithSubmitButton = resource.GetDisplayHintById(hiddenButtonId) as ButtonDisplayHint;
                    }

                    if (validateWithSubmitButton != null)
                    {
                        validateWithSubmitButton.Action = new DisplayHintAction(DisplayHintActionType.validate.ToString());
                        validateWithSubmitButton.Action.Context = validateLink;
                        validateWithSubmitButton.Action.IsDefault = true;
                        validateWithSubmitButton.Action.NextAction = new DisplayHintAction(DisplayHintActionType.successWithPidlPayload.ToString());
                    }
                }
            }
        }

        private static string GetActionPropertyName(string type)
        {
            string propertyName = null;
            if (string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganization, StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganizationCSP, StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1ShipToOrganization, StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1SoldToIndividual, StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1ShipToIndividual, StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1, StringComparison.OrdinalIgnoreCase))
            {
                propertyName = Constants.DataDescriptionIds.Address;
            }

            return propertyName;
        }

        private static RestLink GetHapiModernAccountLink(string href)
        {
            var submitLink = new RestLink() { Href = href, Method = Constants.HTTPVerbs.POST };

            submitLink.AddHeader(Constants.CustomHeaders.ApiVersion, Constants.ApiVersions.ModernAccountV20190531);
            submitLink.AddHeader(Constants.CustomHeaders.MsCorrelationId, Guid.NewGuid().ToString());
            submitLink.AddHeader(Constants.CustomHeaders.MsTrackingId, Guid.NewGuid().ToString());

            return submitLink;
        }

        private static RestLink GetEmptySubmitLink()
        {
            var submitLink = new RestLink()
            {
                Href = Constants.SubmitUrls.MockUrl,
                Method = Constants.HTTPVerbs.POST
            };

            return submitLink;
        }

        private static RestLink GetValidationLink(string partner, string type, string scenario = null, List<string> exposedFlightFeatures = null, PaymentExperienceSetting setting = null)
        {
            var validateLink = new RestLink();
            validateLink.Method = Constants.HTTPVerbs.POST;
            validateLink.SetErrorCodeExpressions(new[] { "({contextData.innererror.code})", "({contextData.code})" });
            validateLink.PropertyName = GetActionPropertyName(type);

            if (IsModernAccountAddress(type))
            {
                if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.ShowAVSSuggestions))
                {
                    string modernAVSForTradeScenario = Constants.ScenarioNames.SuggestAddressesTradeAVS;
                    if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.TradeAVSUsePidlModalInsteadofPidlPage, StringComparer.OrdinalIgnoreCase))
                    {
                        modernAVSForTradeScenario = Constants.ScenarioNames.SuggestAddressesTradeAVSUsePidlModal;
                    }

                    validateLink.Href = string.Format(Constants.SubmitUrls.PifdAnonymousModernAVSForTrade, type, partner, Context.Culture.Name, modernAVSForTradeScenario, Context.Country);
                }
                else
                {
                    validateLink.Href = string.Format(Constants.SubmitUrls.PifdAnonymousLegacyAddressValidationWithTypeUrl, type);
                }
            }
            else if (PXCommon.Constants.PartnerGroups.IsAzureBasedPartner(partner)
                || IsBingPartner(partner)
                || (IsCommercialStorePartner(partner) && string.Equals(scenario, Constants.ScenarioNames.ModernAccount)))
            {
                // TODO: This if block to set the submit action for legacyValidate is covered under CustomizeAddressForm-SetSubmitActionType feature
                // It can be removed once below partners migrated to use the PSS feature
                validateLink.Href = Constants.SubmitUrls.PifdAnonymousLegacyAddressValidationUrl;
            }
            else if (IsCommercialStorePartner(partner) && string.Equals(scenario, Constants.ScenarioNames.Commercialhardware, StringComparison.OrdinalIgnoreCase) && !string.Equals(Context.Country, "US", StringComparison.OrdinalIgnoreCase))
            {
                validateLink.Href = Constants.SubmitUrls.PifdAnonymousLegacyAddressValidationUrl;
            }
            else
            {
                validateLink.Href = Constants.SubmitUrls.PifdAnonymousModernAddressValidationUrl;
            }

            return validateLink;
        }

        private static IEnumerable<string> GetPIDLIds(
            string partnerName,
            string paymentMethodFamily,
            string paymentMethodType,
            List<string> exposedFlightFeatures = null,
            string scenario = null,
            string country = null,
            string operation = null,
            PaymentExperienceSetting setting = null)
        {
            ////TODO : All these exceptional cases should be driven from config
            List<string> paymentMethodTypeIds = new List<string>();

            // Use Paypal redirect on Xbox
            if (!(string.Equals(partnerName, Constants.PidlConfig.XboxPartnerName, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, Constants.PidlConfig.AmcXboxPartnerName, StringComparison.InvariantCultureIgnoreCase) ||
                PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) ||
                ((string.Equals(partnerName, Constants.PidlConfig.OXOWebDirectPartnerName, StringComparison.InvariantCultureIgnoreCase) || string.Equals(partnerName, Constants.PidlConfig.OXODIMEPartnerName, StringComparison.InvariantCultureIgnoreCase)) && string.Equals(scenario, Constants.ScenarioNames.PaypalQrCode, StringComparison.InvariantCultureIgnoreCase))) &&
                string.Equals(paymentMethodType, Constants.PaymentMethodTypeNames.Paypal, StringComparison.InvariantCultureIgnoreCase))
            {
                paymentMethodTypeIds.Add(Constants.PIDLPaymentMethodTypeNames.PaypalRedirect);
            }
            else if (string.Equals(paymentMethodType, Constants.PaymentMethodTypeNames.MasterCard, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(operation, Constants.PidlOperationTypes.Apply, StringComparison.InvariantCultureIgnoreCase))
            {
                paymentMethodTypeIds.Add(Constants.PIDLPaymentMethodTypeNames.XboxCardRedirect);
            }
            else if (string.Equals(paymentMethodType, Constants.PaymentMethodTypeNames.Paypal, StringComparison.InvariantCultureIgnoreCase) &&
                (((string.Equals(partnerName, Constants.PidlConfig.XboxPartnerName, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, Constants.PidlConfig.AmcXboxPartnerName, StringComparison.InvariantCultureIgnoreCase) ||
                PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) ||
                string.Equals(partnerName, Constants.PidlConfig.OXOWebDirectPartnerName, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, Constants.PidlConfig.OXODIMEPartnerName, StringComparison.InvariantCultureIgnoreCase)) &&
                string.Equals(scenario, Constants.ScenarioNames.PaypalQrCode, StringComparison.InvariantCultureIgnoreCase)) ||
                (IsTemplateInList(partnerName, setting, Constants.DescriptionTypes.PaymentMethodDescription, $"{paymentMethodFamily}.{paymentMethodType}") &&
                IsQRcodePayPalFlowInTemplate(setting))))
            {
                paymentMethodTypeIds.Add(Constants.PIDLPaymentMethodTypeNames.PaypalQrCode);
            }
            else if (string.Equals(paymentMethodType, Constants.PaymentMethodTypeNames.Alipay, StringComparison.InvariantCultureIgnoreCase))
            {
                paymentMethodTypeIds.AddRange(new[] { Constants.PIDLPaymentMethodTypeNames.AlipayQrCode });
            }
            else if (string.Equals(paymentMethodFamily, Constants.PaymentMethodFamilyNames.MobileBillingNonSim, StringComparison.InvariantCultureIgnoreCase))
            {
                paymentMethodTypeIds.Add(string.Empty);
            }
            else if (string.Equals(paymentMethodFamily, Constants.PaymentMethodFamilyNames.CreditCard) && string.Equals(scenario, Constants.ScenarioNames.WithProfileAddress))
            {
                paymentMethodTypeIds.Add(paymentMethodType + Constants.PIDLPaymentMethodTypeNames.PaymentMethodWithProfileAddressSuffix);
                paymentMethodTypeIds.Add(paymentMethodType);
            }
            else if (string.Equals(paymentMethodType, Constants.PaymentMethodTypeNames.Klarna)
                && (Constants.PartnersToEnableKlarnaCheckout.Contains(partnerName, StringComparer.OrdinalIgnoreCase)
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableKlarnaCheckout, country, setting))
                && Constants.CountriesToEnableKlarnaCheckout.Contains(country, StringComparer.OrdinalIgnoreCase))
            {
                paymentMethodTypeIds.Add(Constants.PIDLPaymentMethodTypeNames.KlarnaCheckout);
            }
            else if (QRCodeRedirection.ShouldUseVenmoQRCodeTemplate(setting, paymentMethodFamily, paymentMethodType))
            {
                paymentMethodTypeIds.Add(Constants.PIDLPaymentMethodTypeNames.VenmoQrCode);
            }
            else
            {
                paymentMethodTypeIds.Add(paymentMethodType);
            }

            foreach (string paymentMethodTypeId in paymentMethodTypeIds)
            {
                yield return string.IsNullOrEmpty(paymentMethodTypeId) ? paymentMethodFamily : string.Join(".", paymentMethodFamily, paymentMethodTypeId);
            }
        }

        private static IEnumerable<string> GetProfileTypeIds(string partnerName, string profileType, string country, string operation, PaymentExperienceSetting setting = null)
        {
            List<string> profileTypeIds = new List<string> { profileType };

            // Enable the tempalte partner check, to sync with the commercialstores partner.
            if ((string.Equals(partnerName, Constants.PartnerNames.Commercialstores, StringComparison.OrdinalIgnoreCase) ||
                PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseMultipleProfile, country, setting)) &&
                !string.Equals(operation, Constants.PidlOperationTypes.Show, StringComparison.OrdinalIgnoreCase) &&
                Constants.AllCountriesEnabledTaxIdCheckbox.Contains(country))
            {
                if (string.Equals(profileType, Constants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase))
                {
                    profileTypeIds.Add(Constants.PidlResourceIdentities.OrganizationDisableTax);
                }
                else if (string.Equals(profileType, Constants.ProfileTypes.Legal, StringComparison.OrdinalIgnoreCase))
                {
                    profileTypeIds.Add(Constants.PidlResourceIdentities.LegalEntityDisableTax);
                }
            }

            return profileTypeIds;
        }

        private static void ValidateAddressType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new PIDLArgumentException(
                    "Parameter \"type\" is null or Whitespaces.",
                    Constants.ErrorCodes.PIDLArgumentAddressTypeIsNullOrBlank);
            }
        }

        private static string WrapAccountRelatedDescriptionType(string partner, string descriptionType, string descriptionId, string country, PaymentExperienceSetting setting = null)
        {
            string key = string.Format("{0}-{1}", descriptionType, descriptionId);
            string[] targetDescription = { "address-shipping", };
            string templateOrPartner = TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.AddressDescription, Constants.AddressTypes.ShippingV3);

            if ((GetAccountAddressServiceApiVersion(templateOrPartner, country, setting) == Constants.AccountServiceApiVersion.V3 || string.Equals(partner, Constants.PartnerNames.Xbox, StringComparison.OrdinalIgnoreCase) || string.Equals(partner, Constants.PartnerNames.ConsumerSupport, StringComparison.OrdinalIgnoreCase))
                && string.Equals(descriptionId, "shipping", StringComparison.OrdinalIgnoreCase) && targetDescription.Contains(key) && !descriptionId.Contains(Constants.AccountServiceApiVersion.V3.ToLower()))
            {
                descriptionId = string.Format("{0}_{1}", descriptionId, Constants.AccountServiceApiVersion.V3.ToLower());
            }

            return descriptionId;
        }

        private static string GetAccountProfileServiceApiVersion(string partner, bool overrideJarvisVersionToV3 = false, string country = null, PaymentExperienceSetting setting = null)
        {
            if (IsJarvisProfileV3Partner(partner, country, setting) || overrideJarvisVersionToV3)
            {
                return Constants.AccountServiceApiVersion.V3;
            }
            else
            {
                return Constants.AccountServiceApiVersion.V2;
            }
        }

        private static string GetAccountAddressServiceApiVersion(string partner, string country, PaymentExperienceSetting setting, bool overrideJarvisVersionToV3 = false)
        {
            if (IsJarvisAddressV3Partner(partner, country, setting) || overrideJarvisVersionToV3)
            {
                return Constants.AccountServiceApiVersion.V3;
            }
            else
            {
                return Constants.AccountServiceApiVersion.V2;
            }
        }

        private static bool? GetNullableBooleanAttributeValue(string attributeValue)
        {
            if (string.IsNullOrEmpty(attributeValue))
            {
                return null;
            }
            else
            {
                bool result;
                if (bool.TryParse(attributeValue, out result))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        private static void AddValidationToDictionary(
            Dictionary<string, Dictionary<string, List<PropertyValidation>>> validationForId,
            string countryId,
            PropertyValidation validation,
            string partner = GlobalConstants.Defaults.CommonKey)
        {
            if (!validationForId.ContainsKey(partner))
            {
                validationForId[partner] = new Dictionary<string, List<PropertyValidation>>(StringComparer.CurrentCultureIgnoreCase);
            }

            if (!validationForId[partner].ContainsKey(countryId))
            {
                validationForId[partner][countryId] = new List<PropertyValidation>();
            }

            validationForId[partner][countryId].Add(validation);
        }

        private static Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, PropertyDescription>>>> ReadPropertyDescriptionsConfig(
            string fullFilePath,
            Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, PropertyDescription>>>> descriptions,
            string partner = GlobalConstants.Defaults.CommonKey,
            bool hasScenario = false)
        {
            var columns = new List<ColumnDefinition>
                {
                    new ColumnDefinition("PropertyDescriptionId",   ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",              ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("PropertyType",            ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DataType",                ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplayProperty",         ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("IsKey",                   ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("IsOptional",              ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("IsUpdatable",             ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DataProtection",          ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DefaultValue",            ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("PossibleValues",          ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("PidlDownloadEnabled",     ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("PidlDownloadParameter",   ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplayOnly",             ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                };
            if (hasScenario)
            {
                columns.Add(new ColumnDefinition("Scenario", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric));
            }

            using (PIDLConfigParser parser = new PIDLConfigParser(
                fullFilePath,
                columns.ToArray(),
                true))
            {
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    string id = cells[PropertyDescriptionCellIndexDescription.PropertyDescriptionId];
                    string countryConfig = cells[PropertyDescriptionCellIndexDescription.CountryId];
                    string propertyType = cells[PropertyDescriptionCellIndexDescription.PropertyType];
                    string dataType = string.IsNullOrEmpty(cells[PropertyDescriptionCellIndexDescription.DataType]) ? null
                        : cells[PropertyDescriptionCellIndexDescription.DataType];
                    string displayProperty = string.IsNullOrEmpty(cells[PropertyDescriptionCellIndexDescription.DisplayProperty]) ? null
                        : cells[PropertyDescriptionCellIndexDescription.DisplayProperty];

                    bool isKey = string.Equals(cells[PropertyDescriptionCellIndexDescription.IsKey], "true", StringComparison.OrdinalIgnoreCase);
                    bool isOptional = string.Equals(cells[PropertyDescriptionCellIndexDescription.IsOptional], "true", StringComparison.OrdinalIgnoreCase);
                    bool isUpdatable = string.Equals(cells[PropertyDescriptionCellIndexDescription.IsUpdatable], "true", StringComparison.OrdinalIgnoreCase);
                    string dataProtection = string.IsNullOrEmpty(cells[PropertyDescriptionCellIndexDescription.DataProtection]) ? null
                        : cells[PropertyDescriptionCellIndexDescription.DataProtection];
                    string defaultValue = string.IsNullOrEmpty(cells[PropertyDescriptionCellIndexDescription.DefaultValue]) ? null
                        : cells[PropertyDescriptionCellIndexDescription.DefaultValue];
                    Dictionary<string, string> possibleValues = string.IsNullOrEmpty(cells[PropertyDescriptionCellIndexDescription.PossibleValues]) ? null
                        : PIDLResourceFactory.GetDictionaryFromConfigString(cells[PropertyDescriptionCellIndexDescription.PossibleValues]);
                    bool? pidlDownloadEnabled = PIDLResourceFactory.GetNullableBooleanAttributeValue(cells[PropertyDescriptionCellIndexDescription.PidlDownloadEnabled]);
                    string pidlDownloadParameter = string.IsNullOrEmpty(cells[PropertyDescriptionCellIndexDescription.PidlDownloadParameter]) ? null
                        : cells[PropertyDescriptionCellIndexDescription.PidlDownloadParameter];
                    bool? displayOnly = PIDLResourceFactory.GetNullableBooleanAttributeValue(cells[PropertyDescriptionCellIndexDescription.DisplayOnly]);

                    string scenario = hasScenario ? cells[PropertyDescriptionCellIndexDescription.Scenario] : GlobalConstants.Defaults.CommonKey;

                    PropertyDescription newPropertyDescription = new PropertyDescription(
                        id,
                        propertyType,
                        dataType,
                        displayProperty,
                        isKey,
                        isOptional,
                        isUpdatable,
                        dataProtection,
                        defaultValue,
                        possibleValues,
                        pidlDownloadEnabled,
                        pidlDownloadParameter,
                        displayOnly);

                    if (!descriptions.ContainsKey(id))
                    {
                        descriptions[id] = new Dictionary<string, Dictionary<string, Dictionary<string, PropertyDescription>>>(StringComparer.CurrentCultureIgnoreCase);
                    }

                    Dictionary<string, string> countries = GetCountriesList(countryConfig);
                    foreach (string countryId in countries.Keys)
                    {
                        if (!descriptions[id].ContainsKey(countryId))
                        {
                            descriptions[id][countryId] = new Dictionary<string, Dictionary<string, PropertyDescription>>(StringComparer.CurrentCultureIgnoreCase);
                        }

                        if (!descriptions[id][countryId].ContainsKey(partner))
                        {
                            descriptions[id][countryId][partner] = new Dictionary<string, PropertyDescription>(StringComparer.CurrentCultureIgnoreCase);
                        }

                        descriptions[id][countryId][partner][scenario] = newPropertyDescription;
                    }
                }

                return descriptions;
            }
        }

        private static Dictionary<string, string> GetCountriesList(string countryConfig)
        {
            Dictionary<string, string> countries;
            if (string.IsNullOrWhiteSpace(countryConfig))
            {
                countries = new Dictionary<string, string>() { { GlobalConstants.Defaults.CountryKey, GlobalConstants.Defaults.CountryKey } };
            }
            else
            {
                countries = PIDLResourceFactory.GetDictionaryFromConfigString(countryConfig);
            }

            return countries;
        }

        private static Dictionary<string, Dictionary<string, Dictionary<string, List<PropertyValidation>>>> ReadPropertyValidationsConfig(
            string fullFilePath,
            Dictionary<string, Dictionary<string, Dictionary<string, List<PropertyValidation>>>> validationLists,
            string partner = GlobalConstants.Defaults.CommonKey)
        {
            using (PIDLConfigParser parser = new PIDLConfigParser(
                fullFilePath,
                new ColumnDefinition[]
                {
                    new ColumnDefinition("PropertyDescriptionId",   ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",              ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ValidationType",          ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ValidationRegEx",         ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("UrlValidationType",       ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ValidationFunction",      ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ErrorCode",               ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ErrorMessage",            ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ResolutionRegEx",         ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                string currentId = null;
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    string id = cells[PropertyValidationCellIndexDescription.PropertyDescriptionId];
                    string countryConfig = cells[PropertyValidationCellIndexDescription.CountryId];
                    string validationType = cells[PropertyValidationCellIndexDescription.ValidationType];
                    string validationRegex = string.IsNullOrEmpty(cells[PropertyValidationCellIndexDescription.ValidationRegEx]) ? null
                        : cells[PropertyValidationCellIndexDescription.ValidationRegEx];
                    string validationUrlType = string.IsNullOrEmpty(cells[PropertyValidationCellIndexDescription.UrlValidationType]) ? null
                        : cells[PropertyValidationCellIndexDescription.UrlValidationType];
                    string validationUrl = string.IsNullOrEmpty(cells[PropertyValidationCellIndexDescription.UrlValidationType]) ? null
                    : Constants.PidlUrlConstants.ValidationUrlSubPath;
                    string validationFunction = string.IsNullOrEmpty(cells[PropertyValidationCellIndexDescription.ValidationFunction]) ? null
                        : cells[PropertyValidationCellIndexDescription.ValidationFunction];
                    string errorCode = cells[PropertyValidationCellIndexDescription.ErrorCode];
                    string errorMessage = cells[PropertyValidationCellIndexDescription.ErrorMessage];
                    string resolutionRegex = string.IsNullOrEmpty(cells[PropertyValidationCellIndexDescription.ResolutionRegEx]) ? null
                        : cells[PropertyValidationCellIndexDescription.ResolutionRegEx];

                    PropertyValidation newValidation = new PropertyValidation()
                    {
                        ValidationType = validationType,
                        Regex = validationRegex,
                        ValidationUrl = validationUrl,
                        UrlValidationType = validationUrlType,
                        ValidationFunction = validationFunction,
                        ErrorCode = errorCode,
                        ErrorMessage = errorMessage,
                        ResolutionRegex = resolutionRegex
                    };

                    if (string.IsNullOrWhiteSpace(id))
                    {
                        if (currentId == null)
                        {
                            throw new PIDLConfigException(
                                Constants.DataDescriptionFilePaths.PropertyValidationCSV,
                                parser.LineNumber,
                                string.Format("Name of the first validation property is missing."),
                                Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(countryConfig))
                            {
                                AddValidationToDictionary(validationLists[currentId], GlobalConstants.Defaults.CountryKey, newValidation, partner);
                            }
                            else
                            {
                                Dictionary<string, string> parsedCountries = PIDLResourceFactory.GetDictionaryFromConfigString(countryConfig);
                                foreach (string countryId in parsedCountries.Keys)
                                {
                                    AddValidationToDictionary(validationLists[currentId], countryId, newValidation, partner);
                                }
                            }
                        }
                    }
                    else
                    {
                        currentId = id;
                        if (!validationLists.ContainsKey(id))
                        {
                            validationLists[id] = new Dictionary<string, Dictionary<string, List<PropertyValidation>>>(StringComparer.CurrentCultureIgnoreCase);
                        }

                        if (string.IsNullOrEmpty(countryConfig))
                        {
                            AddValidationToDictionary(validationLists[id], GlobalConstants.Defaults.CountryKey, newValidation, partner);
                        }
                        else
                        {
                            Dictionary<string, string> parsedCountries = PIDLResourceFactory.GetDictionaryFromConfigString(countryConfig);
                            foreach (string countryId in parsedCountries.Keys)
                            {
                                AddValidationToDictionary(validationLists[id], countryId, newValidation, partner);
                            }
                        }
                    }
                }

                return validationLists;
            }
        }

        private static IEnumerable<DirectoryInfo> GetFullPathToPartnersDirectories()
        {
            string fullPathToPartnersDirectories = Helper.GetFullPath(Constants.PidlConfig.DisplayDescriptionFolderRootPath);
            IEnumerable<DirectoryInfo> partnerDirectories = Directory.GetDirectories(fullPathToPartnersDirectories).Select(x => new DirectoryInfo(x));

            return partnerDirectories;
        }

        private static void AddPartnerActionToDisplayHint<T>(
            List<PIDLResource> pidlResources,
            string displayHintId,
            PIActionType actionType,
            string resourceType,
            string language,
            string country,
            string partner,
            string type = null,
            string family = null,
            string scenario = null,
            List<string> exposedFlightFeatures = null) where T : DisplayHint
        {
            foreach (var pidlResource in pidlResources)
            {
                // There might be some pidl resources that might not have display pages.
                if (pidlResource.DisplayPages != null)
                {
                    var displayHint = pidlResource.GetDisplayHintById(displayHintId) as T;
                    if (displayHint != null)
                    {
                        var actionTypeName = PaymentInstrumentActions.ToString(actionType);

                        if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.UpdateNewPaymentMethodLinkActionContext, StringComparer.OrdinalIgnoreCase))
                        {
                            ActionContext context = new ActionContext()
                            {
                                Id = Constants.DisplayHintIds.NewPaymentMethodLink,
                                Instance = Constants.DisplayHintIds.NewPaymentMethodLink,
                            };

                            displayHint.Action = new DisplayHintAction(DisplayHintActionType.success.ToString(), false, context, null);
                        }
                        else
                        {
                            PidlDocInfo pidlDocInfo = new PidlDocInfo(resourceType, language, country, partner, type, family, scenario);
                            ActionContext context = new ActionContext()
                            {
                                Action = actionTypeName,
                                ResourceActionContext = new ResourceActionContext(actionTypeName, pidlDocInfo),
                            };

                            displayHint.Action = new DisplayHintAction(DisplayHintActionType.partnerAction.ToString(), false, context, null);
                        }
                    }
                }
            }
        }

        private static void AddResourceActionToDisplayHint<T>(
           List<PIDLResource> pidlResources,
           string displayHintId,
           PIActionType actionType,
           string resourceType,
           string id,
           string language,
           string country,
           string partner,
           string resourceObjPath = null) where T : DisplayHint
        {
            foreach (var pidlResource in pidlResources)
            {
                // There might be some pidl resources that might not have display pages.
                if (pidlResource.DisplayPages != null)
                {
                    var displayHint = pidlResource.GetDisplayHintById(displayHintId) as T;
                    if (displayHint != null)
                    {
                        var actionTypeName = PaymentInstrumentActions.ToString(actionType);
                        ResourceInfo resourceInfo = new ResourceInfo(resourceType, id, language, country, partner);
                        ActionContext context = new ActionContext()
                        {
                            Action = actionTypeName,
                            ResourceActionContext = new ResourceActionContext { Action = actionTypeName, ResourceInfo = resourceInfo, ResourceObjPath = resourceObjPath }
                        };

                        displayHint.Action = new DisplayHintAction(DisplayHintActionType.success.ToString(), false, context, null);
                    }
                }
            }
        }

        private static string MapPartnerName(string partner)
        {
            if (string.Equals(partner, Constants.PartnerNames.AzureSignup, StringComparison.OrdinalIgnoreCase) || string.Equals(partner, Constants.PartnerNames.AzureIbiza, StringComparison.OrdinalIgnoreCase))
            {
                return Constants.PartnerNames.Azure;
            }

            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
            {
                return Constants.PartnerNames.XboxNative;
            }

            if (PXCommon.Constants.PartnerGroups.IsWindowsNativePartner(partner))
            {
                return Constants.PartnerNames.WindowsNative;
            }

            return partner;
        }

        private static string MapDataSourcesPartner(string partner)
        {
            if (string.Equals(partner, Constants.PartnerNames.AzureSignup, StringComparison.OrdinalIgnoreCase) || string.Equals(partner, Constants.PartnerNames.AzureIbiza, StringComparison.OrdinalIgnoreCase))
            {
                return Constants.PartnerNames.Azure;
            }

            return partner;
        }

        private static bool IsBingPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.Bing, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsPayinPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.Payin, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsGGPDEDSPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.GGPDEDS, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsOneDrivePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.OneDrive, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsSetupOfficePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.SetupOffice, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsSetupOfficeSdxPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.SetupOfficeSdx, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsStoreOfficePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.StoreOffice, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsCommercialStorePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.Commercialstores, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsNorthStarWebPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.NorthStarWeb, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsAVSSubmitButtonUpdateEnabledByPartner(string country, PaymentExperienceSetting setting)
        {
            return PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.AddressValidation, country, setting) && PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseAddressesExSubmit, country, setting);
        }

        private static bool IsCommercialTaxPartner(string partnerName, PaymentExperienceSetting setting)
        {
            return string.Equals(partnerName, Constants.PartnerNames.Azure, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerNames.AzureSignup, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerNames.AzureIbiza, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerNames.Commercialstores, StringComparison.InvariantCultureIgnoreCase)
                || TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(TemplateHelper.GetSettingTemplate(partnerName, setting, null, null));
        }

        private static void AddTestHeaderAsIFrameFormInput(List<PIDLResource> pidls, string iframeHintId, string testHeader)
        {
            if (pidls != null && !string.IsNullOrWhiteSpace(testHeader))
            {
                foreach (PIDLResource pidl in pidls)
                {
                    IFrameDisplayHint iframe = pidl.GetDisplayHintById(iframeHintId) as IFrameDisplayHint;
                    if (iframe != null && iframe.DisplayContent != null)
                    {
                        string base64EncodedTestHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes(testHeader));
                        string base64UrlEncodedHeader = EncodeBase64AsUrl(base64EncodedTestHeader);
                        string replaceString = string.Format("<input type=\"hidden\" name=\"x-ms-test\" value=\"{0}\" /></form>", base64UrlEncodedHeader);
                        iframe.DisplayContent = Regex.Replace(iframe.DisplayContent, "</form>", replaceString);
                    }
                }
            }
        }

        private static List<PIDLResource> CreateThreeSDChallengeIFrameDescription(string acsChallengeURL, string creqData, string threeDSSessionData, string threeDSSessionId, string cspStep, string width, string height, string testHeader, bool asUrlIFrame, List<string> exposedFlightFeatures = null)
        {
            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.ThreeDSChallangeIFrameDescription }
            });

            PageDisplayHint page = new PageDisplayHint
            {
                HintId = Constants.DisplayHintIds.IFramePageId,
                DisplayName = Constants.DisplayHintIds.ThreeDSChallengePageName
            };

            IFrameDisplayHint challengeIFrame = new IFrameDisplayHint()
            {
                HintId = Constants.DisplayHintIds.ThreeDSChallengeIFrameId,
            };

            if (asUrlIFrame)
            {
                string sourceUrl = Constants.IFrameContentUrlTemplates.PostThreeDSSessionData;
                if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXPSD2EnableCSPUrlProxyFrameWithSanitizedInput, StringComparer.OrdinalIgnoreCase))
                {
                    sourceUrl = Constants.IFrameContentUrlSanitizedInputTemplates.PostThreeDSSessionData;
                }

                challengeIFrame.SourceUrl = string.Format(sourceUrl, acsChallengeURL, creqData, threeDSSessionData, cspStep);
            }
            else
            {
                challengeIFrame.DisplayContent = string.Format(Constants.IFrameContentTemplates.PostThreeDSSessionData, acsChallengeURL, creqData, threeDSSessionData, cspStep);
            }

            if (width != null)
            {
                challengeIFrame.Width = width;
            }

            if (height != null)
            {
                challengeIFrame.Height = height;
            }

            if (!string.IsNullOrEmpty(challengeIFrame.SourceUrl) && !string.IsNullOrEmpty(testHeader))
            {
                challengeIFrame.SourceUrl += "&x-ms-test=" + testHeader;
            }

            challengeIFrame.ExpectedClientActionId = threeDSSessionId;

            page.AddDisplayHint(challengeIFrame);
            retVal.AddDisplayPages(new List<PageDisplayHint> { page });

            List<PIDLResource> retList = new List<PIDLResource> { retVal };
            AddTestHeaderAsIFrameFormInput(retList, Constants.DisplayHintIds.ThreeDSChallengeIFrameId, testHeader);
            return retList;
        }

        private static string EncodeBase64AsUrl(string base64Val)
        {
            if (base64Val == null)
            {
                throw new ArgumentNullException("base64Val");
            }

            return base64Val
                .Replace("=", string.Empty)
                .Replace("/", "_")
                .Replace("+", "-");
        }

        private static void UpdateListPIActionsForNorthStarWeb(List<PIDLResource> retList, string country, string language)
        {
            // Add new PM Link
            AddPartnerActionToDisplayHint<ButtonDisplayHint>(
                    pidlResources: retList,
                    displayHintId: Constants.DisplayHintIds.NewPaymentMethodLink,
                    actionType: PIActionType.SelectResourceType,
                    resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                    country: country,
                    language: language,
                    partner: Constants.PartnerNames.NorthStarWeb);

            // make the new payment method link as default success button
            ButtonDisplayHint newPMLink = retList.Where(i => i.DisplayPages != null).Select(j => j.GetDisplayHintById(Constants.DisplayHintIds.NewPaymentMethodLink)).OfType<ButtonDisplayHint>().FirstOrDefault();
            if (newPMLink != null)
            {
                newPMLink.Action.ActionType = DisplayHintActionType.success.ToString();
                newPMLink.Action.IsDefault = true;
            }

            // CC Actions
            AddResourceActionToDisplayHint<ButtonDisplayHint>(
                pidlResources: retList,
                displayHintId: Constants.DisplayHintIds.PaymentInstrumentItemUpdateLink,
                actionType: PIActionType.UpdateResource,
                resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                id: "({contextData.id})",
                resourceObjPath: "{contextData.details}",
                country: country,
                language: language,
                partner: Constants.PartnerNames.NorthStarWeb);

            AddResourceActionToDisplayHint<ButtonDisplayHint>(
                pidlResources: retList,
                displayHintId: Constants.DisplayHintIds.PaymentInstrumentItemReplaceLink,
                actionType: PIActionType.ReplaceResource,
                resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                id: "({contextData.id})",
                resourceObjPath: "{contextData.details}",
                country: country,
                language: language,
                partner: Constants.PartnerNames.NorthStarWeb);

            AddResourceActionToDisplayHint<ButtonDisplayHint>(
                pidlResources: retList,
                displayHintId: Constants.DisplayHintIds.PaymentInstrumentItemDeleteLink,
                actionType: PIActionType.DeleteResource,
                resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                id: "({contextData.id})",
                resourceObjPath: "{contextData.details}",
                country: country,
                language: language,
                partner: Constants.PartnerNames.NorthStarWeb);

            // CSV Actions
            AddResourceActionToDisplayHint<ButtonDisplayHint>(
                pidlResources: retList,
                displayHintId: Constants.DisplayHintIds.PaymentInstrumentItemCSVAddMoneyLink,
                actionType: PIActionType.AddMoney,
                resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                id: "({contextData.id})",
                resourceObjPath: "{contextData.details}",
                country: country,
                language: language,
                partner: Constants.PartnerNames.NorthStarWeb);

            AddResourceActionToDisplayHint<ButtonDisplayHint>(
                pidlResources: retList,
                displayHintId: Constants.DisplayHintIds.PaymentInstrumentItemCSVRedeemLink,
                actionType: PIActionType.Redeem,
                resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                id: "({contextData.id})",
                resourceObjPath: "{contextData.details}",
                country: country,
                language: language,
                partner: Constants.PartnerNames.NorthStarWeb);

            AddResourceActionToDisplayHint<ButtonDisplayHint>(
                pidlResources: retList,
                displayHintId: Constants.DisplayHintIds.PaymentInstrumentItemCSVShopLink,
                actionType: PIActionType.Shop,
                resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                id: "({contextData.id})",
                resourceObjPath: "{contextData.details}",
                country: country,
                language: language,
                partner: Constants.PartnerNames.NorthStarWeb);

            // ACH Actions
            AddResourceActionToDisplayHint<ButtonDisplayHint>(
                pidlResources: retList,
                displayHintId: Constants.DisplayHintIds.PaymentInstrumentItemACHUpdateLink,
                actionType: PIActionType.UpdateResource,
                resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                id: "({contextData.id})",
                resourceObjPath: "{contextData.details}",
                country: country,
                language: language,
                partner: Constants.PartnerNames.NorthStarWeb);

            AddResourceActionToDisplayHint<ButtonDisplayHint>(
                pidlResources: retList,
                displayHintId: Constants.DisplayHintIds.PaymentInstrumentItemACHViewMandateLink,
                actionType: PIActionType.ViewMandate,
                resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                id: "({contextData.id})",
                resourceObjPath: "{contextData.details}",
                country: country,
                language: language,
                partner: Constants.PartnerNames.NorthStarWeb);

            AddResourceActionToDisplayHint<ButtonDisplayHint>(
                pidlResources: retList,
                displayHintId: Constants.DisplayHintIds.PaymentInstrumentItemACHDeleteLink,
                actionType: PIActionType.DeleteResource,
                resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                id: "({contextData.id})",
                resourceObjPath: "{contextData.details}",
                country: country,
                language: language,
                partner: Constants.PartnerNames.NorthStarWeb);

            // Paypal Actions
            AddResourceActionToDisplayHint<ButtonDisplayHint>(
                pidlResources: retList,
                displayHintId: Constants.DisplayHintIds.PaymentInstrumentItemPaypalDeleteLink,
                actionType: PIActionType.DeleteResource,
                resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                id: "({contextData.id})",
                resourceObjPath: "{contextData.details}",
                country: country,
                language: language,
                partner: Constants.PartnerNames.NorthStarWeb);

            // NSM Actions
            AddResourceActionToDisplayHint<ButtonDisplayHint>(
                pidlResources: retList,
                displayHintId: Constants.DisplayHintIds.PaymentInstrumentItemNSMDeleteLink,
                actionType: PIActionType.DeleteResource,
                resourceType: Constants.DescriptionTypes.PaymentInstrumentDescription,
                id: "({contextData.id})",
                resourceObjPath: "{contextData.details}",
                country: country,
                language: language,
                partner: Constants.PartnerNames.NorthStarWeb);
        }

        private static void UpdateYesImDoneWithBankButtonWithSubmitLink(string sessionId, List<PIDLResource> pidlList)
        {
            RestLink restLink = new RestLink()
            {
                Href = @"https://{pifd-endpoint}/users/{userId}/paymentSessions/" + sessionId + "/status",
                Method = Constants.HTTPVerbs.GET,
            };

            ButtonDisplayHint yesImDoneWithBankButton = pidlList.Last().GetDisplayHintById(Constants.ButtonDisplayHintIds.Cc3DSYesButton) as ButtonDisplayHint;

            if (yesImDoneWithBankButton != null)
            {
                yesImDoneWithBankButton.Action.Context = restLink;
            }

            ButtonDisplayHint yesImDoneWithBankTryAgainButton = pidlList.Last().GetDisplayHintById(Constants.ButtonDisplayHintIds.Cc3DSYesVerificationButton) as ButtonDisplayHint;

            if (yesImDoneWithBankTryAgainButton != null)
            {
                yesImDoneWithBankTryAgainButton.Action.Context = restLink;
            }
        }

        private void ConstructRestLinks(List<PIDLResource> retList, bool overrideJarvisVersionToV3, string scenario, string profileId, Dictionary<string, string> extraHeaders)
        {
            foreach (PIDLResource pidl in retList)
            {
                foreach (DisplayHint displayHint in pidl.GetAllDisplayHints().Where(displayHint => displayHint.Action?.Context as SubmitLink != null))
                {
                    SubmitLink submitLink = (SubmitLink)displayHint.Action.Context;

                    if (overrideJarvisVersionToV3)
                    {
                        displayHint.Action.Context = this.GetSubmitLinkFromStore(submitLink.ResourceType, submitLink.ResourceIdentity, submitLink.ResourceOperation, scenario, SubmitLink.EndpointResourceType.OverrideToV3.ToString());

                        if (displayHint.Action.Context == null)
                        {
                            throw new PIDLConfigException(
                                string.Format("No OverrideToV3 submit link found for submit link composite key of type \"{0}\", id \"{1}\", operation \"{2}\", and scenario \"{3}\"", submitLink.ResourceType, submitLink.ResourceIdentity, submitLink.ResourceOperation, scenario),
                                Constants.ErrorCodes.PIDLConfigPIDLResourceForIdIsMissing);
                        }

                        submitLink = (SubmitLink)displayHint.Action.Context;
                    }

                    submitLink.ProfileId = profileId;
                    submitLink.ConstructRestLink(extraHeaders);
                }
            }
        }

        private DataSourcesConfig GetDataSourcesConfig(string descriptionType, string id, string operation, string country)
        {
            DataSourcesConfig retVal = null;
            if (this.dataSources.ContainsKey(descriptionType))
            {
                string idKey = id;

                if (!this.dataSources[descriptionType].ContainsKey(idKey))
                {
                    idKey = GlobalConstants.Defaults.InfoDescriptorIdKey;
                }

                if (this.dataSources[descriptionType].ContainsKey(idKey))
                {
                    if (this.dataSources[descriptionType][idKey].ContainsKey(operation))
                    {
                        string countryKey = country;

                        if (!this.dataSources[descriptionType][idKey][operation].ContainsKey(countryKey))
                        {
                            countryKey = GlobalConstants.Defaults.CountryKey;
                        }

                        if (this.dataSources[descriptionType][idKey][operation].ContainsKey(countryKey))
                        {
                            string scenarioKey = string.Empty;
                            retVal = this.dataSources[descriptionType][idKey][operation][countryKey][scenarioKey];
                        }
                    }
                }
            }

            return retVal;
        }

        private PIDLResource GetProfilePIDLResource(string type, string partnerName, string country, string operation, string scenario, RestLink nextPidlLink, string resourceId = null, List<string> exposedFlightFeatures = null, PaymentExperienceSetting setting = null, bool overrideJarvisVersionToV3 = false, bool isGuestAccount = false)
        {
            // pidlsdk client can't recognize the operation 'update_patch'. convert it to 'update' operation
            string operationIdentity = operation;
            if (string.Equals(operation, Constants.PidlOperationTypes.UpdatePatch, StringComparison.OrdinalIgnoreCase))
            {
                operationIdentity = Constants.PidlOperationTypes.Update;
            }

            var identityTable = new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.ProfileDescription },
            };

            // Jarvis v3 does not accept the property keys automatically created from these identity table entries
            // Ideally, type,country,operation keys should be excluded for all jarvis v3 calls.
            // But this codepath is being called for scenario="withProfileAddress". So, we are excluding these keys only for guest account.
            if (!(overrideJarvisVersionToV3 && isGuestAccount))
            {
                identityTable[Constants.DescriptionIdentityFields.Type] = type;
                identityTable[Constants.DescriptionIdentityFields.Country] = country;
                identityTable[Constants.DescriptionIdentityFields.Operation] = operationIdentity;
            }

            if (resourceId != null)
            {
                identityTable[Constants.DescriptionIdentityFields.ResourceIdentity] = resourceId;
            }

            PIDLResource retVal = new PIDLResource(identityTable);

            Dictionary<string, RestLink> overrideLinks = null;
            if (nextPidlLink != null)
            {
                overrideLinks = new Dictionary<string, RestLink>()
                        {
                            { Constants.LinkNames.NextPidl, nextPidlLink }
                        };
            }

            // Wallet requires consumer profile with address, SFPex requires consumer profile without address
            // Use the same solution for PaypalHEC and PaypalRedirect, if request comes from wallet, change type from "consumer" to "consumerWallet"
            if (string.Equals(partnerName, Constants.PartnerNames.Wallet, StringComparison.InvariantCultureIgnoreCase))
            {
                type = Constants.PIDLProfileTypeNames.ConsumerWallet;
            }

            this.GetPIDLResourceRecursive(partnerName, Constants.DescriptionTypes.ProfileDescription, resourceId ?? type, country, operation, retVal, overrideLinks, true, null, Constants.HiddenOptionalFields.AddressDescriptionPropertyNames, null, scenario, exposedFlightFeatures, setting: setting);

            return retVal;
        }

        private void GetPIDLResourceRecursiveInternal(
            string partnerName,
            string descriptionType,
            string id,
            string country,
            string operation,
            PIDLResource retVal,
            Dictionary<string, RestLink> overrideLinks = null,
            bool includeDisplayDescriptions = true,
            string displayDescriptionId = null,
            IList<string> hiddenOptionalPropertyNames = null,
            Dictionary<string, string> context = null,
            string scenario = null,
            List<string> flightNames = null,
            string classicProduct = null,
            string billableAccountId = null,
            PaymentExperienceSetting setting = null,
            string originalPartner = null)
        {
            if (string.IsNullOrWhiteSpace(descriptionType))
            {
                throw new ArgumentException(
                    string.Format("Parameter DescriptionType \"{0}\" in GetPIDLResourceRecursive is null or whitespaces.", descriptionType));
            }

            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            if (country == null)
            {
                throw new ArgumentNullException("country");
            }

            string language = Context.Culture.Name;
            string emailAddress = Context.EmailAddress;

            context = context != null ? context : new Dictionary<string, string>()
            {
                {
                    Constants.ConfigSpecialStrings.CountryId, country
                },
                {
                    Constants.ConfigSpecialStrings.Language, language
                },
                {
                    Constants.ConfigSpecialStrings.Operation, operation
                },
                {
                    Constants.ConfigSpecialStrings.EmailAddress, emailAddress
                },
                {
                    Constants.HiddenOptionalFields.ContextKey, string.Empty
                },
                {
                    Constants.ConfigSpecialStrings.PaymentMethodDisplayName, GlobalConstants.Defaults.DisplayName
                }
            };

            PIDLResourceConfig descriptionConfig = this.GetPIDLResourceConfig(descriptionType, id, operation, country);
            foreach (string[] propertyConfig in descriptionConfig.DataDescriptionConfig)
            {
                string propertyName = propertyConfig[0];
                string propertyDescriptionId = propertyConfig[1];

                if (!string.IsNullOrEmpty(propertyDescriptionId))
                {
                    if (propertyDescriptionId.IndexOf('.') > -1)
                    {
                        // This means that the propertyName actually is an id of another PIDLResource or
                        // an array of PIDLResource Ids that need to be expanded into its/their constituent
                        // property descriptions recursively.
                        string[] pidlResourceIds = propertyDescriptionId.Split(new char[] { '|' });
                        List<PIDLResource> subResources = new List<PIDLResource>();
                        foreach (string resourceId in pidlResourceIds)
                        {
                            string[] resourceIdParts = resourceId.Split(new char[] { '.' });
                            resourceIdParts[2] = resourceIdParts[2].Replace(Constants.ConfigSpecialStrings.Operation, operation);
                            resourceIdParts[3] = resourceIdParts[3].Replace(Constants.ConfigSpecialStrings.CountryId, country);
                            PIDLResource subResource = new PIDLResource(new Dictionary<string, string>()
                            {
                                { Constants.DescriptionIdentityFields.DescriptionType, resourceIdParts[0] },
                                { Constants.DescriptionIdentityFields.Type, resourceIdParts[1] },
                                { Constants.DescriptionIdentityFields.Operation, resourceIdParts[2] },
                                { Constants.DescriptionIdentityFields.Country, resourceIdParts[3] }
                            });
                            this.GetPIDLResourceRecursive(
                                partnerName,
                                resourceIdParts[0],
                                resourceIdParts[1],
                                resourceIdParts[3],
                                resourceIdParts[2],
                                subResource,
                                null,
                                false,
                                null,
                                hiddenOptionalPropertyNames,
                                context,
                                scenario,
                                flightNames,
                                setting: setting);
                            subResources.Add(subResource);
                        }

                        if (string.IsNullOrEmpty(propertyName))
                        {
                            // propertyName is null or empty, indicates that data descriptions of the subResources should be added to the
                            // data descriptions of the parent resource.
                            foreach (var subResource in subResources)
                            {
                                foreach (var subResourcePropertyName in subResource.DataDescription.Keys)
                                {
                                    var propertyDescription = subResource.DataDescription[subResourcePropertyName] as PropertyDescription;
                                    if (propertyDescription == null || !propertyDescription.IsIdentityProperty)
                                    {
                                        // propertyDescription == null, indicates the data description is a sub PIDLResource, which should be added to the data description of the
                                        // parent PIDLResource as is.
                                        // propertyDescription.IsIdentityProperty, indicates that the data descirption is an identity property and identity properties of the sub PIDLResource
                                        // should not be added to the parent PIDLResource.
                                        if (retVal.DataDescription.ContainsKey(subResourcePropertyName))
                                        {
                                            throw new PIDLConfigException(
                                                string.Format("Property with name '{0}' already exists in the data description of the PIDL Resource.", subResourcePropertyName),
                                                Constants.ErrorCodes.PIDLConfigDuplicateDataDescription);
                                        }

                                        retVal.DataDescription[subResourcePropertyName] = subResource.DataDescription[subResourcePropertyName];
                                    }
                                }
                            }
                        }
                        else
                        {
                            retVal.DataDescription[propertyName] = subResources;
                        }
                    }
                    else
                    {
                        // Try to find a country specific property description template first.  If not found, fall-back to
                        // a generic teamplate.  If a generic template is also not found, we want the dictionary to throw an
                        // exception.
                        List<string> skipPossibleValuesLocalizationDataDescriptionIds = new List<string> { Constants.DataDescriptionIds.PaymentInstrumentFamily, Constants.DataDescriptionIds.PaymentInstrumentType };
                        bool skipPossibleValuesLocalization = false;
                        if (skipPossibleValuesLocalizationDataDescriptionIds.Contains(propertyName))
                        {
                            if ((flightNames?.Contains(Flighting.Features.PXSelectPMSkipLocalization, StringComparer.OrdinalIgnoreCase) ?? false)
                                || IsSetupOfficePartner(partnerName)
                                || IsSetupOfficeSdxPartner(partnerName)
                                || IsStoreOfficePartner(partnerName))
                            {
                                skipPossibleValuesLocalization = true;
                            }
                        }

                        PropertyDescription propertyDescription = new PropertyDescription(
                            this.GetPropertyDescription(propertyDescriptionId, country, partnerName, scenario),
                            context,
                            skipPossibleValuesLocalization);

                        // Try to find a country-specific property validation.  If not found, fall-back to a generic validation.
                        List<PropertyValidation> validations = this.GetPropertyValidationList(propertyDescriptionId, country, partnerName);
                        propertyDescription.SetValidationList(validations, context);

                        // To fix bug #18834348 for now, this code updates the validation regex for all country property so that it
                        // would accept both upper and lower case.
                        // e.g. for Turkey while we have ^tr$ as the validation regex from the csv, this changes that to ^tr|TR$
                        if (string.Equals(propertyDescriptionId, "address_country") || string.Equals(propertyDescriptionId, "profile_address_country_databinding"))
                        {
                            foreach (PropertyValidation validation in propertyDescription.Validations)
                            {
                                if (string.Equals(validation.ValidationType, Constants.ValidationTypes.Regex) &&
                                    string.Equals(validation.ErrorCode, "invalid_country") &&
                                    !string.IsNullOrEmpty(validation.Regex))
                                {
                                    string countryCode = validation.Regex.Substring(validation.Regex.IndexOf("^") + 1, 2);
                                    validation.Regex = "^" + countryCode.ToLower() + "|" + countryCode.ToUpper() + "$";
                                }
                            }
                        }

                        if ((!string.Equals(propertyDescription.PropertyDescriptionId, "profile_employee_culture", StringComparison.OrdinalIgnoreCase)
                                && !string.Equals(propertyDescription.PropertyDescriptionId, "profile_employee_language", StringComparison.OrdinalIgnoreCase))
                            || (flightNames != null && flightNames.Contains(Flighting.Features.PXIncludeCultureAndLanguageTransformation, StringComparer.OrdinalIgnoreCase))
                            || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableCultureAndLanguageTransformation, country, setting))
                        {
                            // Try to find a country-specific property transformation.  If not found, fall-back to a generic transformation.
                            var transformationInfo = PidlFactoryHelper.GetPropertyTransformationInfo(
                            propertyDescription.PropertyDescriptionId,
                            country);

                            if (transformationInfo != null)
                            {
                                propertyDescription.AddTransformation(transformationInfo);
                            }
                        }

                        PropertyDataProtection protectionTemplate = this.GetPropertyDataProtection(propertyDescription.TokenSet, country);

                        if (protectionTemplate != null)
                        {
                            protectionTemplate = new PropertyDataProtection(protectionTemplate);

                            propertyDescription.DataProtection = protectionTemplate;
                            propertyDescription.TokenSet = string.IsNullOrEmpty(protectionTemplate.ProtectionName) ? null : propertyDescription.TokenSet;
                        }

                        retVal.DataDescription[propertyName] = propertyDescription;

                        if (hiddenOptionalPropertyNames != null && hiddenOptionalPropertyNames.Contains(propertyName) && propertyDescription.IsOptional.GetValueOrDefault())
                        {
                            context[Constants.HiddenOptionalFields.ContextKey] = context[Constants.HiddenOptionalFields.ContextKey] + Constants.HiddenOptionalFields.ContextDelimiter + propertyName;
                        }
                    }
                }
            }

            // remove issuerId if flight is enabled for ideal billing agreement type method
            if (flightNames?.Contains(Flighting.Features.PXEnableModernIdealPayment, StringComparer.OrdinalIgnoreCase) ?? false && (retVal?.DataDescription?.ContainsKey("issuerId") ?? false))
            {
                retVal.DataDescription.Remove("issuerId");
            }

            DataSourcesConfig dataSourcesConfig = this.GetDataSourcesConfig(descriptionType, id, operation, country);

            if (dataSourcesConfig != null)
            {
                foreach (string[] config in dataSourcesConfig.Config)
                {
                    string href = config[1];
                    foreach (KeyValuePair<string, string> contextKeyValue in context)
                    {
                        href = href == null ? null : href.Replace(contextKeyValue.Key, contextKeyValue.Value);
                    }

                    if (string.Equals(descriptionType, Constants.DataSourceTypes.PaymentInstrument, StringComparison.OrdinalIgnoreCase) &&
                        (string.Equals(id, Constants.DataSourceIdentities.List, StringComparison.OrdinalIgnoreCase) || string.Equals(id, Constants.DataSourceIdentities.ListModern, StringComparison.OrdinalIgnoreCase) || string.Equals(id, Constants.DataSourceIdentities.ListAdditionalPI, StringComparison.OrdinalIgnoreCase)) &&
                        string.Equals(operation, Constants.DataSourceOperations.Selectinstance, StringComparison.OrdinalIgnoreCase))
                    {
                        // For Azure flow, partner passes other partners such as "azureIbiza" and "azuresignup".
                        // If originalPartner is used then the partner param will be "azureIbiza" and "azuresignup", instead of "azure".
                        // PIMS does have partner specific logic for commercial partners.  We must map azureibiza and azuresignup to azure
                        // or else functionality will be broken.
                        string mappedPartner = MapDataSourcesPartner(originalPartner);

                        if (!string.IsNullOrEmpty(mappedPartner))
                        {
                            href += $"&partner={mappedPartner}";
                        }
                        else if (!string.IsNullOrEmpty(partnerName))
                        {
                            href += $"&partner={partnerName}";
                        }

                        if (!string.IsNullOrEmpty(classicProduct))
                        {
                            href += $"&classicProduct={classicProduct}";
                        }

                        if (!string.IsNullOrEmpty(billableAccountId))
                        {
                            href += $"&billableAccountId={WebUtility.UrlEncode(billableAccountId)}";
                        }
                    }

                    retVal.AddDataSource(config[0], new DataSource(href, config[2], new Dictionary<string, string>(dataSourcesConfig.Headers)));
                }
            }

            if (includeDisplayDescriptions)
            {
                SubmitLink submitLink = null;

                if (flightNames?.Contains(Flighting.Features.PXEnableCSVSubmitLinks, StringComparer.OrdinalIgnoreCase) ?? false)
                {
                    submitLink = this.GetSubmitLinkFromStore(descriptionType, id, operation, scenario);
                }

                IncludeDisplayDescriptions(retVal, partnerName, descriptionType, id, country, operation, displayDescriptionId, context, submitLink, scenario, flightNames);
            }

            AddLinks(descriptionType, retVal, overrideLinks);
        }

        /// <summary>
        /// Deep copies a submit link object from the static backing store built from the SubmitLinks.csv on startup.
        /// This object contains all the information necessary to build the rest link and to bind it to a display hint of a PIDL.
        /// </summary>
        /// <param name="descriptionType">The PIDLResources.csv PIDLResourceType</param>
        /// <param name="id">The PIDLResources.csv PidlResourceIdentity</param>
        /// <param name="operation">The PIDLResources.csv Operation</param>
        /// <param name="scenario">The PIDLResources.csv Scenario</param>
        /// <param name="endpointResourceType">The SubmitLinks.csv EndpointResourceType</param>
        /// <returns>Returns a unique submit link for a given valid composite key, if one exists. If the key is invalid, an exception is thrown.</returns>
        private SubmitLink GetSubmitLinkFromStore(
            string descriptionType,
            string id,
            string operation,
            string scenario,
            string endpointResourceType = null)
        {
            SubmitLink submitLink = null;

            if (scenario == null)
            {
                scenario = string.Empty;
            }

            if (endpointResourceType == null)
            {
                endpointResourceType = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(descriptionType))
            {
                throw new ArgumentException(string.Format("Parameter descriptionType \"{0}\" in GetSubmitLink is null or whitespaces.", descriptionType));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(string.Format("Parameter id \"{0}\" in GetSubmitLink is null or whitespaces.", id));
            }

            if (string.IsNullOrWhiteSpace(operation))
            {
                throw new ArgumentException(string.Format("Parameter operation \"{0}\" in GetSubmitLink is null or whitespaces.", operation));
            }

            if (this.submitLinks.ContainsKey(descriptionType))
            {
                if (this.submitLinks[descriptionType].ContainsKey(id))
                {
                    if (this.submitLinks[descriptionType][id].ContainsKey(operation))
                    {
                        string scenarioKey = scenario;

                        if (!this.submitLinks[descriptionType][id][operation].ContainsKey(scenario))
                        {
                            scenarioKey = GlobalConstants.Defaults.CommonKey;
                        }

                        if (this.submitLinks[descriptionType][id][operation].ContainsKey(scenarioKey))
                        {
                            if (this.submitLinks[descriptionType][id][operation][scenarioKey].ContainsKey(endpointResourceType))
                            {
                                submitLink = this.submitLinks[descriptionType][id][operation][scenarioKey][endpointResourceType].DeepCopy();
                            }
                        }
                    }
                }
            }

            return submitLink;
        }

        private IEnumerable<string> GetTaxIdTypes(string country, string type, string partnerName = Constants.PidlConfig.DefaultPartnerName, string profileType = null, bool isStandalone = false, List<string> flightNames = null, PaymentExperienceSetting setting = null)
        {
            Dictionary<string, HashSet<string>> typeTaxIds = null;
            if (this.taxIdsInCountries.ContainsKey(country))
            {
                typeTaxIds = this.taxIdsInCountries[country];
            }
            else if (this.taxIdsInCountries.ContainsKey(string.Empty))
            {
                typeTaxIds = this.taxIdsInCountries[string.Empty];
            }
            else
            {
                return null;
            }

            HashSet<string> taxIds = null;
            if (typeTaxIds.ContainsKey(type))
            {
                taxIds = new HashSet<string>(typeTaxIds[type]);
            }
            else if (typeTaxIds.ContainsKey(string.Empty))
            {
                taxIds = new HashSet<string>(typeTaxIds[string.Empty]);
            }
            else
            {
                return null;
            }

            // For TR, due to a PIDL SDK limitation, it depends on PxService to return two Pidls, one with checkbox enabled, one with checkbox disabled.
            // The following code will be revisited one PIDL SDK fixes this issue
            // TODO: for now, leave it for commercialstores only, will revisit it later for new partners
            if (IsCommercialTaxPartner(partnerName, setting) &&
                string.Equals(profileType, Constants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase) &&
                Constants.AllCountriesEnabledTaxIdCheckbox.Contains(country) && isStandalone)
            {
                taxIds.Add(Constants.PidlResourceIdentities.VatIdDisableTax);
            }

            // for TW, the hapi vat tax pidl linked to the profile pidl depends whether or not the profile is LE or org
            // LE has additional fields so it uses a different pidl idType to differentiate
            // Enable the tempalte partner check, to sync with the PXProfileUpdateToHapi flighting, utilized for the profile.
            if (IsCommercialTaxPartner(partnerName, setting) &&
                (string.Equals(profileType, Constants.ProfileTypes.Legal, StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(profileType, Constants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase) &&
                ((flightNames?.Contains(Flighting.Features.PXProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase) ?? false) ||
                PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseProfileUpdateToHapi, country, setting)))) &&
                string.Equals(country, "tw", StringComparison.OrdinalIgnoreCase) && !isStandalone)
            {
                taxIds.Remove(Constants.PidlResourceIdentities.VatId);
                taxIds.Add(Constants.PidlResourceIdentities.VatIdAdditionalData);
            }

            return taxIds;
        }

        private PIDLResourceConfig GetPIDLResourceConfig(string descriptionType, string id, string operation, string country)
        {
            // Try to find a country specific description teamplate first.  If such a teamplate is not found,
            // fall-back to a generic description teamplate.
            PIDLResourceConfig retVal = null;
            if (!this.pidlResourceConfigs.ContainsKey(descriptionType))
            {
                throw new ArgumentException(
                    string.Format("DescriptionType is invalid or not yet supported."));
            }

            string idKey = id;
            if (!this.pidlResourceConfigs[descriptionType].ContainsKey(idKey))
            {
                idKey = GlobalConstants.Defaults.InfoDescriptorIdKey;
                if (!this.pidlResourceConfigs[descriptionType].ContainsKey(idKey))
                {
                    throw new ArgumentException(
                        string.Format("InfoDescription: Parameters are invalid or were not found."));
                }
            }

            if (!this.pidlResourceConfigs[descriptionType][idKey].ContainsKey(operation))
            {
                throw new ArgumentException(
                    string.Format("InfoDescription: Parameters are invalid or were not found."));
            }

            string countryKey = country;
            if (!this.pidlResourceConfigs[descriptionType][idKey][operation].ContainsKey(countryKey))
            {
                countryKey = GlobalConstants.Defaults.CountryKey;
                if (!this.pidlResourceConfigs[descriptionType][idKey][operation].ContainsKey(countryKey))
                {
                    throw new ArgumentException(
                        string.Format("InfoDescription: Parameters are invalid or were not found."));
                }
            }

            string scenarioKey = string.Empty;
            retVal = this.pidlResourceConfigs[descriptionType][idKey][operation][countryKey][scenarioKey];

            return retVal;
        }

        private PropertyDescription GetPropertyDescription(
            string id,
            string country,
            string partner = GlobalConstants.Defaults.CommonKey,
            string scenario = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Parameter \"id\" in GetPropertyDescription is null or whitespaces");
            }

            // country could be empty.  (e.g. where a property does not require country specific specialization)
            if (country == null)
            {
                throw new ArgumentNullException("country");
            }

            if (!this.propertyDescriptions.ContainsKey(id))
            {
                throw new ArgumentException(
                    string.Format("Parameter id \"{0}\" was not found in PropertyDescriptions", id));
            }

            string countryKey = country;
            if (!this.propertyDescriptions[id].ContainsKey(countryKey))
            {
                countryKey = GlobalConstants.Defaults.CountryKey;
                if (!this.propertyDescriptions[id].ContainsKey(countryKey))
                {
                    throw new ArgumentException(
                        string.Format("PropertyDescription \"{0}.{1}\" or its fall-back default \"{0}.\" were not found.", id, countryKey));
                }
            }

            string partnerKey = partner;
            if (!this.propertyDescriptions[id][countryKey].ContainsKey(partnerKey))
            {
                partnerKey = GlobalConstants.Defaults.CommonKey;
                if (!this.propertyDescriptions[id][countryKey].ContainsKey(partnerKey))
                {
                    // Check the property description of "id" for fall-back defaults by setting the country to its default value.
                    countryKey = GlobalConstants.Defaults.CountryKey;
                    if (!this.propertyDescriptions[id][countryKey].ContainsKey(partnerKey))
                    {
                        throw new ArgumentException($"PropertyDescription \"{id}.{countryKey}\" or its fall-back default \"{id}.\" were not found. Partner: {partnerKey}");
                    }
                }
            }

            string scenarioKey = scenario ?? GlobalConstants.Defaults.CommonKey;
            if (!this.propertyDescriptions[id][countryKey][partnerKey].ContainsKey(scenarioKey))
            {
                scenarioKey = GlobalConstants.Defaults.CommonKey;
                if (!this.propertyDescriptions[id][countryKey][partnerKey].ContainsKey(scenarioKey))
                {
                    partnerKey = GlobalConstants.Defaults.CommonKey;
                    if (!this.propertyDescriptions[id][countryKey][partnerKey].ContainsKey(scenarioKey))
                    {
                        throw new ArgumentException($"PropertyDescription \"{id}.{countryKey}\" or its fall-back default \"{id}.\" were not found. Partner: {partnerKey}, scenario: {scenarioKey}");
                    }
                }
            }

            return this.propertyDescriptions[id][countryKey][partnerKey][scenarioKey];
        }

        private void ValidateChallengeType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new PIDLArgumentException(
                    "Parameter \"type\" is null or Whitespaces.",
                    Constants.ErrorCodes.PIDLArgumentChallengeDescriptionIdIsNullOrBlank);
            }

            if (!this.pidlResourceConfigs[Constants.DescriptionTypes.ChallengeDescription].ContainsKey(type))
            {
                throw new PIDLArgumentException(
                    string.Format("Parameter \"type\"=\"{0}\" is invalid.  No such challenge_id was found.", type),
                    Constants.ErrorCodes.PIDLArgumentChallengeDescriptionIdInvalid);
            }
        }

        private void ReadTaxIdsInCountriesConfig()
        {
            this.taxIdsInCountries = new Dictionary<string, Dictionary<string, HashSet<string>>>(StringComparer.CurrentCultureIgnoreCase);
            using (PIDLConfigParser taxIdsInCountriesParser = new PIDLConfigParser(
            Helper.GetFullPath(Constants.DataDescriptionFilePaths.TaxIdsInCountriesCSV),
            new ColumnDefinition[]
                {
                    new ColumnDefinition("CountryId", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Scenario",  ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("TaxIds",    ColumnConstraint.Required, ColumnFormat.Text),
                    new ColumnDefinition("Enabled",   ColumnConstraint.Optional, ColumnFormat.Text)
                },
            true))
            {
                // Get the list of all countries where MSFT has a commerce presence.
                Dictionary<string, string> msftCommerceCountries = domainDictionaries[Constants.DomainDictionaryNames.MSFTCommerceCountries];
                while (!taxIdsInCountriesParser.EndOfData)
                {
                    string[] cells = taxIdsInCountriesParser.ReadValidatedFields();

                    bool enabled = true;
                    if (!string.IsNullOrEmpty(cells[3]) && !bool.TryParse(cells[3], out enabled))
                    {
                        throw new PIDLConfigException(
                            Constants.DataDescriptionFilePaths.TaxIdsInCountriesCSV,
                            taxIdsInCountriesParser.LineNumber,
                            string.Format(
                                "'Enabled' value \"{0}\" is not 'true' or 'false'.",
                                cells[3]),
                            Constants.ErrorCodes.PIDLConfigUnknownBooleanValue);
                    }
                    else if (!enabled)
                    {
                        continue;
                    }

                    if (string.Equals(cells[3], bool.FalseString, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    string country = string.IsNullOrWhiteSpace(cells[0]) ? string.Empty : cells[0];
                    if (!string.IsNullOrEmpty(country) && !msftCommerceCountries.ContainsKey(cells[0]))
                    {
                        throw new PIDLConfigException(
                            Constants.DataDescriptionFilePaths.TaxIdsInCountriesCSV,
                            taxIdsInCountriesParser.LineNumber,
                            string.Format(
                                "CountryId \"{0}\" has not been defined in the {1} domain table in file {2}.",
                                country,
                                Constants.DomainDictionaryNames.MSFTCommerceCountries,
                                Constants.DataDescriptionFilePaths.DomainDictionariesCSV),
                            Constants.ErrorCodes.PIDLConfigUnknownCountryId);
                    }

                    Dictionary<string, HashSet<string>> countryScenarios;
                    if (!this.taxIdsInCountries.TryGetValue(country, out countryScenarios))
                    {
                        countryScenarios = new Dictionary<string, HashSet<string>>(StringComparer.CurrentCultureIgnoreCase);
                        this.taxIdsInCountries[country] = countryScenarios;
                    }

                    string scenario = string.IsNullOrWhiteSpace(cells[1]) ? string.Empty : cells[1];
                    HashSet<string> taxIds;
                    if (!this.taxIdsInCountries[country].TryGetValue(scenario, out taxIds))
                    {
                        taxIds = new HashSet<string>();
                        this.taxIdsInCountries[country][scenario] = taxIds;
                    }

                    taxIds.Add(cells[2]);
                }
            }
        }

        private void ReadValidationChallengeTypesConfig()
        {
            this.validationChallengeTypes = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(StringComparer.CurrentCultureIgnoreCase);
            using (PIDLConfigParser challengeTypesParser = new PIDLConfigParser(
            Helper.GetFullPath(Constants.DataDescriptionFilePaths.ValidationChallengeTypesCSV),
            new ColumnDefinition[]
                {
                                new ColumnDefinition("CountryId",              ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                                new ColumnDefinition("PaymentMethodFamily",    ColumnConstraint.Required, ColumnFormat.Text),
                                new ColumnDefinition("PaymentMethodType",      ColumnConstraint.Required, ColumnFormat.Text),
                                new ColumnDefinition("ChallengeType",          ColumnConstraint.Required, ColumnFormat.Text)
                },
            true))
            {
                // Get the list of all countries where MSFT has a commerce presence.
                Dictionary<string, string> msftCommerceCountries = domainDictionaries[Constants.DomainDictionaryNames.MSFTCommerceCountries];
                while (!challengeTypesParser.EndOfData)
                {
                    string[] cells = challengeTypesParser.ReadValidatedFields();
                    string country = string.IsNullOrWhiteSpace(cells[0]) ? string.Empty : cells[0];
                    if (!string.IsNullOrEmpty(country) && !msftCommerceCountries.ContainsKey(cells[0]))
                    {
                        throw new PIDLConfigException(
                            Constants.DataDescriptionFilePaths.ValidationChallengeTypesCSV,
                            challengeTypesParser.LineNumber,
                            string.Format(
                                "CountryId \"{0}\" has not been defined in the {1} domain table in file {2}.",
                                country,
                                Constants.DomainDictionaryNames.MSFTCommerceCountries,
                                Constants.DataDescriptionFilePaths.DomainDictionariesCSV),
                            Constants.ErrorCodes.PIDLConfigUnknownCountryId);
                    }

                    if (!this.validationChallengeTypes.ContainsKey(country))
                    {
                        this.validationChallengeTypes[country] = new Dictionary<string, Dictionary<string, string>>();
                    }

                    string family = cells[1];
                    Dictionary<string, string> paymentTypeToChallengeType;
                    if (!this.validationChallengeTypes[country].TryGetValue(family, out paymentTypeToChallengeType))
                    {
                        paymentTypeToChallengeType = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                        this.validationChallengeTypes[country][family] = paymentTypeToChallengeType;
                    }

                    string paymentType = cells[2];
                    string challengeType = cells[3];
                    this.validationChallengeTypes[country][family][paymentType] = challengeType;
                }
            }
        }

        private void ReadPropertyDescriptionsConfig()
        {
            var propertyDescriptionConfigs = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, PropertyDescription>>>>(StringComparer.CurrentCultureIgnoreCase);
            ReadPropertyDescriptionsConfig(Helper.GetFullPath(Constants.DataDescriptionFilePaths.PropertyDescriptionsCSV), propertyDescriptionConfigs);

            IEnumerable<DirectoryInfo> fullPathToPartnersDirectories = GetFullPathToPartnersDirectories();
            foreach (DirectoryInfo partnerDirectory in fullPathToPartnersDirectories)
            {
                string propertyDescriptionAbsolutePath = Path.Combine(partnerDirectory.FullName, Constants.DataDescriptionOverrideFileNames.PropertyDescriptionsCSV);
                ReadPropertyDescriptionsConfig(propertyDescriptionAbsolutePath, propertyDescriptionConfigs, partnerDirectory.Name, hasScenario: true);
            }

            this.propertyDescriptions = propertyDescriptionConfigs;
        }

        private void ReadPropertyDataProtectionsConfig()
        {
            using (PIDLConfigParser parser = new PIDLConfigParser(
                Helper.GetFullPath(Constants.DataDescriptionFilePaths.PropertyDataProtectionsCSV),
                new ColumnDefinition[]
                {
                    new ColumnDefinition("DataProtectionId",        ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",              ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ProtectionType",          ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ParameterKey",            ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ParameterValue",          ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DataProtectionName",      ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                Dictionary<string, Dictionary<string, PropertyDataProtection>> dataProtections = new Dictionary<string, Dictionary<string, PropertyDataProtection>>(StringComparer.CurrentCultureIgnoreCase);
                Dictionary<string, PropertyDataProtection> currentDictionary = null;
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    string dataProtectionId = cells[PropertyDataProtectionCellIndexDescription.DataProtectionId];
                    string countryConfig = cells[PropertyDataProtectionCellIndexDescription.CountryId];
                    string dataProtectionType = cells[PropertyDataProtectionCellIndexDescription.ProtectionType];
                    string parameterKey = cells[PropertyDataProtectionCellIndexDescription.ParameterKey];
                    string parameterValue = cells[PropertyDataProtectionCellIndexDescription.ParameterValue];
                    string dataProtectionName = cells[PropertyDataProtectionCellIndexDescription.DataProtectionName];

                    if (string.IsNullOrWhiteSpace(dataProtectionId))
                    {
                        if (currentDictionary == null)
                        {
                            throw new PIDLConfigException(
                                Constants.DisplayDescriptionFileNames.DisplaySequencesCSV,
                                parser.LineNumber,
                                string.Format("Name of the first group DataProtectionId is missing."),
                                Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                        }
                    }
                    else
                    {
                        if (!dataProtections.TryGetValue(dataProtectionId, out currentDictionary))
                        {
                            currentDictionary = new Dictionary<string, PropertyDataProtection>(StringComparer.CurrentCultureIgnoreCase);
                            dataProtections[dataProtectionId] = currentDictionary;
                        }
                    }

                    List<string> parsedCountries = null;
                    if (string.IsNullOrEmpty(countryConfig))
                    {
                        parsedCountries = new List<string>() { string.Empty };
                    }
                    else
                    {
                        parsedCountries = PIDLResourceFactory.GetDictionaryFromConfigString(countryConfig).Keys.ToList();
                    }

                    foreach (string countryId in parsedCountries)
                    {
                        if (!currentDictionary.ContainsKey(countryId))
                        {
                            currentDictionary[countryId] = new PropertyDataProtection(dataProtectionId, dataProtectionType, dataProtectionName);
                        }

                        // Don't store a value without a key
                        if (!string.IsNullOrEmpty(parameterKey))
                        {
                            currentDictionary[countryId].Parameters.Add(parameterKey, parameterValue);
                        }
                    }
                }

                this.propertyDataProtections = dataProtections;
            }
        }

        private void ReadPropertyValidationsConfig()
        {
            var propertyValidationConfigs = new Dictionary<string, Dictionary<string, Dictionary<string, List<PropertyValidation>>>>(StringComparer.CurrentCultureIgnoreCase);
            ReadPropertyValidationsConfig(Helper.GetFullPath(Constants.DataDescriptionFilePaths.PropertyValidationCSV), propertyValidationConfigs);

            IEnumerable<DirectoryInfo> fullPathToPartnersDirectories = GetFullPathToPartnersDirectories();
            foreach (DirectoryInfo partnerDirectory in fullPathToPartnersDirectories)
            {
                string propertyDescriptionAbsolutePath = Path.Combine(partnerDirectory.FullName, Constants.DataDescriptionOverrideFileNames.PropertyValidationCSV);
                ReadPropertyValidationsConfig(propertyDescriptionAbsolutePath, propertyValidationConfigs, partnerDirectory.Name);
            }

            this.propertyValidationLists = propertyValidationConfigs;
        }

        private void ReadPropertyTransformationConfig()
        {
            string filePath = Helper.GetFullPath(Constants.DataDescriptionFilePaths.PropertyTransformationCSV);
            using (PIDLConfigParser parser = new PIDLConfigParser(
                filePath,
                new ColumnDefinition[]
                {
                    new ColumnDefinition("PropertyDescriptionId",   ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",              ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Target",                  ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Category",                ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("UrlTransformationType",   ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("InputRegex",              ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("TransformationRegex",     ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                },
            true))
            {
                Dictionary<string, Dictionary<string, Dictionary<string, PropertyTransformationInfo>>> transformations = new Dictionary<string, Dictionary<string, Dictionary<string, PropertyTransformationInfo>>>(StringComparer.CurrentCultureIgnoreCase);
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    string id = cells[PropertyTransformationCellIndexDescription.PropertyDescriptionId];
                    string countryConfig = cells[PropertyTransformationCellIndexDescription.CountryIds];
                    string targetType = cells[PropertyTransformationCellIndexDescription.Target];
                    string transformCategory = cells[PropertyTransformationCellIndexDescription.Category];
                    string transformUrl = string.IsNullOrEmpty(cells[PropertyTransformationCellIndexDescription.UrlTransformationType]) ? null
                        : Constants.PidlUrlConstants.TransformationUrlSubPath;
                    string transformUrlType = string.IsNullOrEmpty(cells[PropertyTransformationCellIndexDescription.UrlTransformationType]) ? null
                    : cells[PropertyTransformationCellIndexDescription.UrlTransformationType];
                    string inputRegex = string.IsNullOrEmpty(cells[PropertyTransformationCellIndexDescription.InputRegex]) ? null
                    : cells[PropertyTransformationCellIndexDescription.InputRegex];
                    string transformRegex = string.IsNullOrEmpty(cells[PropertyTransformationCellIndexDescription.TransformationRegex]) ? null
                    : cells[PropertyTransformationCellIndexDescription.TransformationRegex];

                    if (!PropertyTransformationInfo.IsValidTransformationTarget(targetType))
                    {
                        throw new PIDLConfigException(
                            filePath,
                            parser.LineNumber,
                            string.Format("Invalid property transformation target."),
                            Constants.ErrorCodes.PIDLConfigInvalidTransformationTarget);
                    }
                    else if (!PropertyTransformationInfo.IsValidTransformationCategory(transformCategory))
                    {
                        throw new PIDLConfigException(
                            filePath,
                            parser.LineNumber,
                            string.Format("Invalid property transformation category."),
                            Constants.ErrorCodes.PIDLConfigInvalidTransformationCategory);
                    }

                    PropertyTransformationInfo newTransformation = new PropertyTransformationInfo()
                    {
                        TransformCategory = transformCategory,
                        TransformUrl = transformUrl,
                        UrlTransformationType = transformUrlType,
                        InputRegex = inputRegex,
                        TransformRegex = transformRegex
                    };

                    if (!transformations.ContainsKey(id))
                    {
                        transformations[id] = new Dictionary<string, Dictionary<string, PropertyTransformationInfo>>(StringComparer.CurrentCultureIgnoreCase);
                    }

                    List<string> parsedCountries = null;
                    if (string.IsNullOrEmpty(countryConfig))
                    {
                        parsedCountries = new List<string>() { string.Empty };
                    }
                    else
                    {
                        parsedCountries = PIDLResourceFactory.GetDictionaryFromConfigString(countryConfig).Keys.ToList();
                    }

                    foreach (string countryId in parsedCountries)
                    {
                        if (!transformations[id].ContainsKey(countryId))
                        {
                            transformations[id][countryId] = new Dictionary<string, PropertyTransformationInfo>();
                        }

                        if (!transformations[id][countryId].ContainsKey(targetType))
                        {
                            transformations[id][countryId][targetType] = newTransformation;
                        }
                    }
                }

                this.propertyTransformations = transformations;
            }
        }

        private List<PIDLResource> GetTaxIdPidls(string type, IEnumerable<string> taxIdTypes, string country, string partnerName, string operation = GlobalConstants.Defaults.OperationKey, bool isStandalone = false, string scenario = null, PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retVal = new List<PIDLResource>();
            if (taxIdTypes == null)
            {
                // taxId PIDL should be returned as empty for the countries where tax_id is not applicable.
                retVal.Add(new PIDLResource());
            }
            else
            {
                foreach (string taxIdType in taxIdTypes)
                {
                    string descriptionType = Constants.DescriptionTypes.TaxIdDescription;

                    if (IsCommercialTaxPartner(partnerName, setting) && string.Equals(type, Constants.TaxIdTypes.Commercial, StringComparison.OrdinalIgnoreCase))
                    {
                        descriptionType = "hapi" + descriptionType;
                        if (isStandalone)
                        {
                            descriptionType += "Standalone";
                        }
                    }

                    PIDLResource taxId = new PIDLResource(
                        new Dictionary<string, string>
                        {
                            { Constants.DescriptionIdentityFields.DescriptionType, descriptionType },
                            { Constants.DescriptionIdentityFields.Type, taxIdType },
                            { Constants.DescriptionIdentityFields.CountryCode, country }
                        });

                    retVal.Add(taxId);
                    this.GetPIDLResourceRecursive(partnerName, descriptionType, taxIdType, country, operation, taxId, scenario: scenario, setting: setting);

                    // For HAPI endpoint, it rejects the request if PIDL sends extra parameters
                    // The following code remove all extra metadata fields in PIDL
                    if (IsCommercialTaxPartner(partnerName, setting) && string.Equals(type, Constants.TaxIdTypes.Commercial, StringComparison.OrdinalIgnoreCase))
                    {
                        string[] taxidMetaProperties = { Constants.DescriptionIdentityFields.Type, Constants.DescriptionIdentityFields.CountryCode };
                        RemoveDataDescriptionWithFullPath(taxId, null, taxidMetaProperties, descriptionType);

                        if (string.Equals(country, "tw", StringComparison.OrdinalIgnoreCase))
                        {
                            string twDescriptionPath = "additionalData";
                            string twDescriptionType = "data";
                            string[] twTaxidMetaProperties = { Constants.DescriptionIdentityFields.Type, Constants.DescriptionIdentityFields.Country, Constants.DescriptionIdentityFields.Operation };
                            RemoveDataDescriptionWithFullPath(taxId, twDescriptionPath, twTaxidMetaProperties, twDescriptionType);
                        }
                    }
                }
            }

            return retVal;
        }

        private static class PropertyDescriptionCellIndexDescription
        {
            public const int PropertyDescriptionId = 0;
            public const int CountryId = 1;
            public const int PropertyType = 2;
            public const int DataType = 3;
            public const int DisplayProperty = 4;
            public const int IsKey = 5;
            public const int IsOptional = 6;
            public const int IsUpdatable = 7;
            public const int DataProtection = 8;
            public const int DefaultValue = 9;
            public const int PossibleValues = 10;
            public const int PidlDownloadEnabled = 11;
            public const int PidlDownloadParameter = 12;
            public const int DisplayOnly = 13;
            public const int Scenario = 14;
        }

        private static class PropertyValidationCellIndexDescription
        {
            public const int PropertyDescriptionId = 0;
            public const int CountryId = 1;
            public const int ValidationType = 2;
            public const int ValidationRegEx = 3;
            public const int UrlValidationType = 4;
            public const int ValidationFunction = 5;
            public const int ErrorCode = 6;
            public const int ErrorMessage = 7;
            public const int ResolutionRegEx = 8;
        }

        private static class PropertyTransformationCellIndexDescription
        {
            public const int PropertyDescriptionId = 0;
            public const int CountryIds = 1;
            public const int Target = 2;
            public const int Category = 3;
            public const int UrlTransformationType = 4;
            public const int InputRegex = 5;
            public const int TransformationRegex = 6;
        }

        private static class PropertyDataProtectionCellIndexDescription
        {
            public const int DataProtectionId = 0;
            public const int CountryId = 1;
            public const int ProtectionType = 2;
            public const int ParameterKey = 3;
            public const int ParameterValue = 4;
            public const int DataProtectionName = 5;
        }
    }
}
