// <copyright file="AddressSelectionHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using PidlModel;
    using PidlModel.V7;
    using PimsModel.V4;
    using PXCommon;

    public class AddressSelectionHelper
    {
        public static List<PIDLResource> GetSuggestedAddressesPidls(
            List<PXAddressV3Info> suggestedAddresses,
            PXAddressV3Info userEnteredAddress,
            string partner,
            string language,
            string operation,
            string avsStatus,
            List<string> exposedFlightingFeatures,
            string addressType = Constants.PidlResourceIdentities.PXV3,
            bool setAsDefaultBilling = false,
            string addressId = null,
            string scenario = null,
            bool useValidateInstanceV2UXForPidlPage = false,
            PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> resourceList = new List<PIDLResource>();
            List<PXAddressV3Info> addressList = suggestedAddresses;
            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)
                && (string.Equals(operation, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(operation, Constants.PidlOperationTypes.ValidateInstance, StringComparison.OrdinalIgnoreCase)))
            {
                userEnteredAddress.IsCustomerConsented = true;

                // Display only the first 3 suggestedAddresses;
                if (addressList.Count > Constants.MaxAddressCount.SuggestAddressMaxCount)
                {
                    addressList = addressList.Take(Constants.MaxAddressCount.SuggestAddressMaxCount).ToList();
                }

                addressList.Add(userEnteredAddress);

                PIDLResource resource = GetAddressSuggestionPidlResource(partner, userEnteredAddress.Country, language, addressType, operation, exposedFlightingFeatures, scenario);

                // Set AVS status message to subheader element
                PIDLResourceFactory.SetDisplayContent(resource, Constants.SuggestAddressDisplayIds.AddressValidationMessage, FetchAVSStatusMessage(avsStatus));
                PopulateSuggestedAddressesForNativeUX(partner, userEnteredAddress.Country, resource, addressList, language, addressType, addressId, scenario, operation);
                PIDLResourceFactory.AddSetAsDefaultBillingDataDescription(resource, partner, avsSuggest: true, setAsDefaultBilling, scenario);

                // populate address text group for SuggestAddressesTradeAVS scenario if isUserEnterdedAddressOnly
                AddressSelectionHelper.AddAddressText(resource, userEnteredAddress, Constants.AddressTextDisplayIdToPropertyNameMappings);

                if (string.Equals(operation, Constants.PidlOperationTypes.ValidateInstance, StringComparison.OrdinalIgnoreCase))
                {
                    PopulateAddressButtonPidlActions(resource, userEnteredAddress, addressList, addressType);
                }

                resourceList.Add(resource);
            }
            else if (string.Equals(operation, Constants.PidlOperationTypes.ValidateInstance, StringComparison.OrdinalIgnoreCase))
            {
                userEnteredAddress.IsUserEntered = true;

                // Display only the user entered address if avs status is 'none'
                if (string.Equals(avsStatus, Constants.LookupResponseStatus.None, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(avsStatus, Constants.LookupResponseStatus.NotValidated, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(avsStatus, Constants.LookupResponseStatus.Multiple, StringComparison.OrdinalIgnoreCase))
                {
                    resourceList.Add(GetSingleSuggestedAddressPIDL(
                        userEnteredAddress,
                        addressList,
                        userEnteredAddress.Country,
                        partner,
                        language,
                        operation,
                        exposedFlightingFeatures,
                        setAsDefaultBilling,
                        isUserEnterdedAddressOnly: true,
                        scenario,
                        setting,
                        useValidateInstanceV2UXForPidlPage));
                }
                else
                {
                    addressList.Add(userEnteredAddress);
                    addressList.ForEach(address =>
                    {
                        resourceList.Add(GetSingleSuggestedAddressPIDL(
                            address,
                            addressList,
                            userEnteredAddress.Country,
                            partner,
                            language,
                            operation,
                            exposedFlightingFeatures,
                            setAsDefaultBilling,
                            isUserEnterdedAddressOnly: false,
                            scenario,
                            setting,
                            useValidateInstanceV2UXForPidlPage));
                    });
                }
            }

            return resourceList;
        }

        public static List<PIDLResource> GetSuggestedAddressesTradeAvsPidls(
            List<PXAddressV3Info> addressList,
            PXAddressV3Info userEnteredAddress,
            string partner,
            string language,
            string operation,
            List<string> exposedFlightingFeatures,
            string addressType,
            string scenario,
            bool usePidlPage,
            bool useV2UXForPidlPage,
            PaymentExperienceSetting setting)
        {
            List<PIDLResource> resourceList = new List<PIDLResource>();

            userEnteredAddress.IsUserEntered = true;
            string templateOrPartnerName = TemplateHelper.GetSettingTemplate(partner, setting, Constants.ResourceTypes.Address, addressType);
            var partnerNameInConfig = string.Equals(partner, templateOrPartnerName, StringComparison.OrdinalIgnoreCase) ? Constants.PartnerNames.DefaultPartner : templateOrPartnerName;

            bool isUserEnteredAddressOnly = addressList.Count == 1;

            string resourceId = isUserEnteredAddressOnly
            ? Constants.PidlResourceIdentities.AddressNoAVSSuggestions
            : Constants.PidlResourceIdentities.AddressAVSSuggestions;

            if (useV2UXForPidlPage)
            {
                resourceId = isUserEnteredAddressOnly
                ? Constants.PidlResourceIdentities.AddressNoAVSSuggestionsV2
                : Constants.PidlResourceIdentities.AddressAVSSuggestionsV2;
            }

            // load address suggested pidl template from pidl configuration.
            PIDLResource resource = AddressSelectionHelper.GetAddressSuggestionPidlResource(partnerNameInConfig, userEnteredAddress.Country, language, resourceId, operation, exposedFlightingFeatures, scenario: scenario);

            if (isUserEnteredAddressOnly)
            {
                // for suggestAddressesTradeAVS scenario, when there are no suggested addresses, UserEnteredButton won't contain mergeData action and will only have closeModalDialog/closePidlAction action and continueSuspendedAction action
                var userEnteredButton = resource.GetDisplayHintById(Constants.ButtonDisplayHintIds.UserEnteredButton);
                if (userEnteredButton != null)
                {
                    userEnteredButton.Action = GenerateUserEnteredButtonPidlAction(usePidlPage);
                }

                // populate address text group for SuggestAddressesTradeAVS scenario if isUserEnterdedAddressOnly
                AddressSelectionHelper.AddAddressText(resource, userEnteredAddress, Constants.AddressTextDisplayIdToPropertyNameMappings);
            }
            else
            {
                // fill user enter address and suggested addresses
                AddressSelectionHelper.PopulateAddressSelectOption(
                    resource,
                    addressList,
                    useV2UXForPidlPage ? Constants.DisplayHintIds.AddressSuggestedTradeAVSV2 : Constants.DisplayHintIds.AddressSuggestedTradeAVS,
                    partner,
                    scenario,
                    exposedFlightingFeatures,
                    useV2UXForPidlPage,
                    setting: setting);

                // populate submit pidl action with adddress payload
                var button = resource.GetDisplayHintById(useV2UXForPidlPage ? Constants.ButtonDisplayHintIds.AddressNextButton : Constants.ButtonDisplayHintIds.AddressUseButton);
                if (button != null)
                {
                    button.Action = GenerateBindingPidlAction(userEnteredAddress, addressList, addressType, usePidlPage);
                }

                // populate address text group
                if (useV2UXForPidlPage)
                {
                    AddressSelectionHelper.AddAddressText(resource, userEnteredAddress, Constants.AddressTextDisplayIdToPropertyNameMappings);
                }

                // populate datadescription "address_suggest_id" section with possible value and default value.
                AddressSelectionHelper.PopulateIdDataDescriptionPossibleValues(resource, addressList, scenario: scenario);
                AddressSelectionHelper.PopulateDataDescriptionDefaultValue(resource, Constants.DataDescriptionIds.AddressSuggestId, defaultValue: "suggested_0");
            }

            if (IsAmcWebReactTradeAVS(partner, scenario))
            {
                AddAdditionalTagsForAMCWebReactTradeAVS(resource);
            }

            resourceList.Add(resource);

            return resourceList;
        }

        public static void PopulateSuggestedAddressesForNativeUX(string partner, string country, PidlModel.V7.PIDLResource pidlResource, List<PXAddressV3Info> addressList, string language, string addressType = Constants.PidlResourceIdentities.PXV3, string addressId = null, string scenario = null, string operation = null)
        {
            string dataHintId = Constants.DataDescriptionIds.AddressGroupId;
            string displayHintId = Constants.DisplayHintIds.SuggestedAddresses;

            PropertyDisplayHint displayHint = pidlResource.GetDisplayHintById(displayHintId) as PropertyDisplayHint;
            if (displayHint != null)
            {
                displayHint.StyleHints = Constants.NativeStyleHints.SuggestedAddressOptionsListStyleHints;
            }

            var addressV3Type = addressType == Constants.PidlResourceIdentities.PXV3Billing ? Constants.DescriptionTypes.AddressBillingV3 : Constants.DescriptionTypes.AddressShippingV3;
            var subPidls = pidlResource.DataDescription[addressV3Type] as List<PIDLResource>;
            if (subPidls == null || subPidls.Count == 0)
            {
                throw new PIDLConfigException(
                    Constants.DataDescriptionFilePaths.PIDLResourcesCSV,
                    0,
                    string.Format(
                        "Details is not defined in the \"{0}\" operation for {1} domain table in file {2}.",
                        "suggest",
                        country,
                        Constants.DataDescriptionFilePaths.PIDLResourcesCSV),
                    Constants.ErrorCodes.PIDLConfigMissingDataDescription);
            }

            List<PIDLResource> retList = new List<PIDLResource>();
            retList.Add(pidlResource);
            PIDLResourceFactory.AddSubmitLinksToContext(partner, Constants.AddressTypes.ShippingV3, true, retList, scenario);

            int userEnteredAddressIndex = addressList.Count - 1;

            SetDataPropertyDefaultValue(subPidls[0], "first_name", addressList[userEnteredAddressIndex].FirstName);
            SetDataPropertyDefaultValue(subPidls[0], "last_name", addressList[userEnteredAddressIndex].LastName);
            SetDataPropertyDefaultValue(subPidls[0], "phone_number", addressList[userEnteredAddressIndex].PhoneNumber);

            SetDataPropertyDefaultValue(subPidls[0], "address_line1", addressList[userEnteredAddressIndex].AddressLine1);

            if (!string.IsNullOrEmpty(addressList[userEnteredAddressIndex].AddressLine2))
            {
                SetDataPropertyDefaultValue(subPidls[0], "address_line2", addressList[userEnteredAddressIndex].AddressLine2);
            }

            if (!string.IsNullOrEmpty(addressList[userEnteredAddressIndex].AddressLine3))
            {
                SetDataPropertyDefaultValue(subPidls[0], "address_line3", addressList[userEnteredAddressIndex].AddressLine3);
            }

            if (!string.IsNullOrEmpty(addressList[userEnteredAddressIndex].City))
            {
                SetDataPropertyDefaultValue(subPidls[0], "city", addressList[userEnteredAddressIndex].City);
            }

            if (!string.IsNullOrEmpty(addressList[userEnteredAddressIndex].Region))
            {
                SetDataPropertyDefaultValue(subPidls[0], "region", addressList[userEnteredAddressIndex].Region);
            }

            if (!string.IsNullOrEmpty(addressList[userEnteredAddressIndex].PostalCode))
            {
                SetDataPropertyDefaultValue(subPidls[0], "postal_code", addressList[userEnteredAddressIndex].PostalCode);
            }

            SetDataPropertyDefaultValue(subPidls[0], "country", addressList[userEnteredAddressIndex].Country);

            // sdk expects empty string for false
            SetDataPropertyDefaultValue(pidlResource, "set_as_default_shipping_address", addressList[userEnteredAddressIndex].DefaultShippingAddress ? "true" : string.Empty);
            SetDataPropertyDefaultValue(pidlResource, "set_as_default_billing_address", addressList[userEnteredAddressIndex].DefaultBillingAddress ? "true" : string.Empty);

            var dataPropertyHint = subPidls[0].DataDescription[dataHintId] as PropertyDescription;

            Dictionary<string, SelectOptionDescription> possibleOptions = new Dictionary<string, SelectOptionDescription>();
            Dictionary<string, string> possibleValues = new Dictionary<string, string>();
            Dictionary<string, string> textTags = new Dictionary<string, string>
            {
                { Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Disable }
            };

            // create the list of possible options along with the display hints
            int index = 1;
            foreach (var address in addressList)
            {
                string addressPidlId = address.Id;
                PidlDocInfo pidlDocInfo = new PidlDocInfo(Constants.DescriptionTypes.AddressDescription, null, address.Country, partner);

                PidlIdentity targetPidlIdentity = new PidlIdentity(
                        Constants.DescriptionTypes.AddressDescription,
                        Constants.PidlOperationTypes.SelectSingleInstance,
                        address.Country,
                        address.Id);

                RestLink actionContext = new RestLink();

                actionContext.Href = Constants.SubmitUrls.PifdAddressPostUrlTemplate + "?partner=" + partner + "&language=" + Context.Culture + "&avsSuggest=false";

                // XboxNative partners needs addressId attached to all payloads in order to send it to Jarvis when doing an UpdateAddress
                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) && !string.IsNullOrEmpty(addressId))
                {
                    actionContext.Payload = address.GetPropertyDictionaryWithId(addressId);
                }
                else
                {
                    actionContext.Payload = address.GetPropertyDictionary();
                }

                actionContext.Method = Constants.HTTPVerbs.POST;

                // XboxNative partners needs addressId attached to all payloads in order to send it to Jarvis when doing an UpdateAddress
                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) && !string.IsNullOrEmpty(addressId))
                {
                    Dictionary<string, string> addressProperties = address.GetPropertyDictionary();
                    addressProperties["id"] = addressId;

                    actionContext.Payload = addressProperties;
                }

                SelectOptionDescription selectOption = new SelectOptionDescription();
                selectOption.PidlAction = new DisplayHintAction("restAction", false, actionContext, null);
                selectOption.StyleHints = Constants.NativeStyleHints.SuggestedAddressOptionStyleHints;

                // create the Address Group Display Hint container
                selectOption.DisplayContent = new GroupDisplayHint
                {
                    HintId = Constants.DisplayHintIdPrefixes.AddressOptionContainer + address.Id,
                    LayoutOrientation = Constants.LayoutOrientations.Vertical,
                    StyleHints = Constants.NativeStyleHints.AddressOptionContainerStyleHints
                };

                string headerText = string.Empty;
                if (string.Equals(address.Id, PidlFactory.GlobalConstants.SuggestedAddressesIds.UserEntered, StringComparison.OrdinalIgnoreCase))
                {
                    headerText = PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesStaticText.EnteredHeader, language);
                    selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = headerText, HintId = "title_" + address.Id, StyleHints = new List<string> { "text-bold" }, DisplayTags = textTags });
                }
                else
                {
                    // More than 1 suggested addresses have their option number displayed in the header.
                    headerText = PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesStaticText.SuggestHeader, language);

                    if (addressList.Count > 2)
                    {
                        selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = FormatHeaderText(headerText, language, index), HintId = "title_" + address.Id, StyleHints = new List<string> { "text-bold" }, DisplayTags = textTags });
                    }
                    else
                    {
                        selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = headerText, HintId = "title_" + address.Id, StyleHints = new List<string> { "text-bold" }, DisplayTags = textTags });
                    }
                }

                selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = Constants.SuggestedAddressesStaticText.Spacer, HintId = "spacer_" + address.Id, StyleHints = new List<string> { "text-bold" }, DisplayTags = textTags });

                selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = address.AddressLine1, HintId = "address_line1_" + address.Id, StyleHints = new List<string> { "text-bold" }, DisplayTags = textTags });

                if (!string.IsNullOrEmpty(address.AddressLine2))
                {
                    selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = address.AddressLine2, HintId = "address_line2_" + address.Id, StyleHints = new List<string> { "text-bold" }, DisplayTags = textTags });
                }

                if (!string.IsNullOrEmpty(address.AddressLine3))
                {
                    selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = address.AddressLine3, HintId = "address_line3_" + address.Id, StyleHints = new List<string> { "text-bold" }, DisplayTags = textTags });
                }

                string cityPostalCode = string.Format("{0}, {1} {2}", address.City, address.Region?.ToUpper(), address.PostalCode);
                selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = cityPostalCode, HintId = "cityPostal_" + address.Id, StyleHints = new List<string> { "text-bold" }, DisplayTags = textTags });

                selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = address.Country?.ToUpper(), HintId = "country_" + address.Id, StyleHints = new List<string> { "text-bold" }, DisplayTags = textTags });

                string displayContent = PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesStaticText.UseThisAddress, language);
                GroupDisplayHint focusTextGroup = CreateFocusTextGroupToAddressOption(displayContent, address.Id);

                selectOption.DisplayContent.Members.Add(focusTextGroup);
                selectOption.AccessibilityTag = FormatSuggestedAddressOptionAriaLabel(headerText, address, language, index, addressList.Count, displayContent);

                possibleValues.Add(address.Id, selectOption.DisplayText);
                possibleOptions.Add(address.Id, selectOption);
                ++index;
            }

            dataPropertyHint.UpdatePossibleValues(possibleValues);
            displayHint.SetPossibleOptions(possibleOptions);
            displayHint.Action = null;
        }

        public static PIDLResource GetAddressSuggestionPidlResource(
            string partnerName,
            string country,
            string language,
            string resourceId,
            string operation = Constants.PidlOperationTypes.Add,
            List<string> exposedFlightFeatures = null,
            string scenario = null,
            PaymentExperienceSetting setting = null)
        {
            Context.Country = country;
            Context.Culture = Helper.GetCultureInfo(language);

            // for consumer zip+4, the default scenario name used is "suggestAddresses" in the csv config.
            // paynow scenario for consumer zip+4 also uses "suggestAddresses" scenario in the csv config. In order to not breadk paynow scenario, use "suggestAddresses" as scenarioConfig for buynow scenario as well.
            // for tradeAvs, the scenario name is "SuggestAddressesTradeAVS"
            string scenarioConfig = Constants.ScenarioNames.SuggestAddresses;
            if (string.Equals(scenario, Constants.ScenarioNames.SuggestAddressesTradeAVS))
            {
                scenarioConfig = Constants.ScenarioNames.SuggestAddressesTradeAVS;
            }
            else if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) && string.Equals(scenario, Constants.ScenarioNames.Profile, StringComparison.OrdinalIgnoreCase))
            {
                scenarioConfig = Constants.ScenarioNames.SuggestAddressesProfile;
            }

            return GetPidlResource(partnerName, country, operation, null, resourceId, Constants.DescriptionTypes.AddressDescription, exposedFlightFeatures, null, null, scenario: scenarioConfig, setting: setting);
        }

        public static string FetchAVSStatusMessage(string status)
        {
            return PidlModelHelper.GetLocalizedString(Constants.AVSStatus.GetAVSMessage(status));
        }

        public static List<PIDLResource> GetAddressGroupSelectPidls(string partnerName, string country, string language, string type, CMResources<PXAddressV3Info> addressGroup, AccountProfileV3 profile, PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retList = new List<PIDLResource>();

            PIDLResource primarySelect = GetAddressGroupPidlResource(
                partnerName,
                country,
                Constants.PidlOperationTypes.SelectInstance,
                string.Equals(type, Constants.AddressTypes.Shipping, StringComparison.OrdinalIgnoreCase)
                    ? Constants.PidlResourceIdentities.PXV3Shipping : Constants.PidlResourceIdentities.PXV3Billing,
                type,
                setting: setting);

            PopulateAddressGroupPidl(
                primarySelect,
                partnerName,
                country,
                language,
                type,
                addressGroup,
                profile);

            retList.Add(primarySelect);
            return retList;
        }

        public static PIDLData ExtractAddressInfoForTradeAvs(PXAddressV3Info pxAddressV3Info, string type)
        {
            PIDLData address = new PIDLData();
            Dictionary<string, object> tradeAVSAddressPropertyDictionary = pxAddressV3Info.GetPropertyDictionaryForAVSAddress();
            foreach (KeyValuePair<string, object> entry in tradeAVSAddressPropertyDictionary)
            {
                address[entry.Key] = entry.Value;
            }

            // For the following type of addresses have differnty format of property names
            if (string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganization, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganizationCSP, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1ShipToOrganization, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1BillToOrganization, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1SoldToIndividual, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1ShipToIndividual, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1BillToIndividual, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1, StringComparison.OrdinalIgnoreCase))
            {
                address.RenameProperty(Constants.LegacyAVSPropertyNames.AddressLine1, Constants.HapiV1ModernAccountAddressDataDescriptionPropertyNames.AddressLine1);
                address.RenameProperty(Constants.LegacyAVSPropertyNames.AddressLine2, Constants.HapiV1ModernAccountAddressDataDescriptionPropertyNames.AddressLine2);
                address.RenameProperty(Constants.LegacyAVSPropertyNames.AddressLine3, Constants.HapiV1ModernAccountAddressDataDescriptionPropertyNames.AddressLine3);
                address.RenameProperty(Constants.LegacyAVSPropertyNames.PostalCode, Constants.HapiV1ModernAccountAddressDataDescriptionPropertyNames.PostalCode);
            }

            if (string.Equals(type, Constants.AddressTypes.HapiServiceUsageAddress, System.StringComparison.OrdinalIgnoreCase))
            {
                address.RenameProperty(Constants.LegacyAVSPropertyNames.AddressLine1, Constants.HapiServiceUsageAddressPropertyNames.AddressLine1);
                address.RenameProperty(Constants.LegacyAVSPropertyNames.AddressLine2, Constants.HapiServiceUsageAddressPropertyNames.AddressLine2);
                address.RenameProperty(Constants.LegacyAVSPropertyNames.AddressLine3, Constants.HapiServiceUsageAddressPropertyNames.AddressLine3);
                address.RenameProperty(Constants.LegacyAVSPropertyNames.PostalCode, Constants.HapiServiceUsageAddressPropertyNames.PostalCode);
                address.RenameProperty(Constants.LegacyAVSPropertyNames.Country, Constants.HapiServiceUsageAddressPropertyNames.Country);
                address.RenameProperty(Constants.LegacyAVSPropertyNames.Region, Constants.HapiServiceUsageAddressPropertyNames.Region);
            }

            return address;
        }

        private static GroupDisplayHint CreateFocusTextGroupToAddressOption(string displayContent, string addressId, string hintId = "")
        {
            string textHintId = !string.IsNullOrEmpty(hintId) ? hintId : "useThisAddressText_" + addressId;
            string groupHintId = "useThisAddressGroup_" + addressId;
            Dictionary<string, string> textTags = new Dictionary<string, string>()
            {
                { Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Disable }
            };

            TextDisplayHint focusText = new TextDisplayHint { DisplayContent = displayContent, HintId = textHintId, StyleHints = new List<string>() { "text-bold" }, DisplayTags = textTags };

            GroupDisplayHint focusTextGroup = new GroupDisplayHint { HintId = groupHintId, StyleHints = new List<string>() { "height-fill", "alignment-bottom" } };
            focusTextGroup.Members.Add(focusText);

            return focusTextGroup;
        }

        private static string FormatHeaderText(string localizedHeaderText, string language, int index)
        {
            var localizedOption = PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesStaticText.Option + " {0}", language);
            var localizedIndex = PidlModelHelper.GetLocalizedString(index.ToString());

            return string.Format("{0} ({1})", localizedHeaderText, string.Format(localizedOption, localizedIndex));
        }

        private static PIDLResource GetAddressGroupPidlResource(string partnerName, string country, string operation, string pidlId, string scenario, string resourceId = null, PaymentExperienceSetting setting = null)
        {
            return GetPidlResource(partnerName, country, operation, pidlId, resourceId, Constants.DescriptionTypes.AddressDescription, null, null, null, scenario, setting: setting);
        }

        private static PIDLResource GetPidlResource(
            string partnerName,
            string country,
            string operation,
            string pidlId,
            string resourceId = null,
            string descriptionType = Constants.DescriptionTypes.AddressDescription,
            List<string> flightNames = null,
            string classicProduct = null,
            string billableAccountId = null,
            string scenario = null,
            PaymentExperienceSetting setting = null)
        {
            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>
                {
                        { Constants.DescriptionIdentityFields.DescriptionType, descriptionType },
                        { Constants.DescriptionIdentityFields.Operation, operation },
                        { Constants.DescriptionIdentityFields.Country, country },
                        { Constants.DescriptionIdentityFields.Type, resourceId }
                });

            if (descriptionType.Equals(Constants.DescriptionTypes.PaymentInstrumentDescription, StringComparison.Ordinal))
            {
                retVal.Identity.Add(Constants.DescriptionIdentityFields.ResourceIdentity, resourceId != null ? resourceId : pidlId);
            }
            else
            {
                retVal.Identity.Add(Constants.DescriptionIdentityFields.ResourceIdentity, resourceId);
            }

            PIDLResourceFactory.Instance.GetPIDLResourceRecursive(
                partnerName,
                descriptionType,
                pidlId ?? resourceId,
                country,
                operation,
                retVal,
                flightNames: flightNames,
                classicProduct: classicProduct,
                billableAccountId: billableAccountId,
                scenario: scenario,
                setting: setting);

            return retVal;
        }

        private static void PopulateAddressGroupPidl(PIDLResource pidlResource, string partner, string country, string language, string type, CMResources<PXAddressV3Info> addressGroups, AccountProfileV3 profile)
        {
            string dataHintId = Constants.DataDescriptionIds.AddressGroupId;
            string displayHintId = Constants.DisplayHintIds.ListAddress;
            string defaultAddressId = profile.DefaultAddressId;
            string defaultShippingAddressId = profile.DefaultShippingAddressId;
            int index = 0;
            int count = addressGroups.Items.Count + 1; // Adding 1 to total count, to include add a new address option

            PropertyDisplayHint displayHint = pidlResource.GetDisplayHintById(displayHintId) as PropertyDisplayHint;

            Dictionary<string, SelectOptionDescription> possibleOptions = new Dictionary<string, SelectOptionDescription>();
            Dictionary<string, string> possibleValues = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(defaultAddressId) && type.Equals(Constants.AddressTypes.Billing))
            {
                int defaultBillingAddressIndex = addressGroups.Items.FindIndex(address => defaultAddressId.Equals(address.Id));

                // It's possible for the user to have a default address in another country
                if (defaultBillingAddressIndex > -1)
                {
                    // Get default address, remove it from current spot in the list and add as first option
                    PXAddressV3Info defaultBillingAddress = addressGroups.Items[defaultBillingAddressIndex];
                    addressGroups.Items.RemoveAt(defaultBillingAddressIndex);
                    string defaultMessage = PidlModelHelper.GetLocalizedString(Constants.ListAddressStaticElements.UseDefaultBilling, language);
                    AddAddressToPossibleOptions(defaultBillingAddress, possibleOptions, possibleValues, language, ++index, count, defaultMessage);
                }
            }

            if (!string.IsNullOrEmpty(defaultShippingAddressId) && type.Equals(Constants.AddressTypes.Shipping))
            {
                int defaultShippingAddressIndex = addressGroups.Items.FindIndex(address => defaultShippingAddressId.Equals(address.Id));
                if (defaultShippingAddressIndex > -1)
                {
                    PXAddressV3Info defaultShippingAddress = addressGroups.Items[defaultShippingAddressIndex];
                    addressGroups.Items.RemoveAt(defaultShippingAddressIndex);
                    string defaultMessage = PidlModelHelper.GetLocalizedString(Constants.ListAddressStaticElements.UseDefaultShipping, language);
                    AddAddressToPossibleOptions(defaultShippingAddress, possibleOptions, possibleValues, language, ++index, count, defaultMessage);
                }
            }

            foreach (PXAddressV3Info address in addressGroups.Items)
            {
                AddAddressToPossibleOptions(address, possibleOptions, possibleValues, language, ++index, count);
            }

            // Add New Address Button
            SelectOptionDescription addNewAddressOption = new SelectOptionDescription();
            ActionContext actionContext = new ActionContext();
            actionContext.Id = Constants.DisplayHintIds.NewAddressLink;
            addNewAddressOption.PidlAction = new DisplayHintAction("success", false, actionContext, null);
            addNewAddressOption.StyleHints = Constants.NativeStyleHints.ListAddressSelectOptionStyleHints;

            // create the Address Group Display Hint container
            addNewAddressOption.DisplayContent = new GroupDisplayHint
            {
                HintId = Constants.DisplayHintIdPrefixes.AddressOptionContainer + Constants.ListAddressDisplayHintIds.AddNewAddress,
                StyleHints = Constants.NativeStyleHints.AddressOptionContainerStyleHints.Concat(new List<string>() { "alignment-horizontal-center", "padding-top-medium" })
            };

            ImageDisplayHint addNewImageHint = new ImageDisplayHint
            {
                SourceUrl = $"{Constants.PidlUrlConstants.StaticResourceServiceImagesV4}/{Constants.StaticResourceNames.AddBoldSvg}",
                HintId = Constants.DisplayHintIds.AddNewAddressLink,
                StyleHints = new List<string>() { "image-quarter", "margin-end-none", "margin-top-large" }
            };

            addNewAddressOption.DisplayContent.Members.Add(addNewImageHint);

            string addAddressButtonText = type.Equals(Constants.AddressTypes.Shipping)
                ? PidlModelHelper.GetLocalizedString(Constants.ListAddressStaticElements.AddNewShipping, language)
                : PidlModelHelper.GetLocalizedString(Constants.ListAddressStaticElements.AddNewBilling, language);

            GroupDisplayHint focusTextGroup = CreateFocusTextGroupToAddressOption(addAddressButtonText, string.Empty, Constants.ListAddressDisplayHintIds.AddNewAddress);

            addNewAddressOption.DisplayContent.Members.Add(focusTextGroup);
            addNewAddressOption.AccessibilityTag = addAddressButtonText;

            possibleValues.Add(Constants.DisplayHintIds.NewAddressLink, addNewAddressOption.DisplayText);
            possibleOptions.Add(Constants.DisplayHintIds.NewAddressLink, addNewAddressOption);

            var dataPropertyHint = pidlResource.DataDescription[dataHintId] as PropertyDescription;
            dataPropertyHint.UpdatePossibleValues(possibleValues);
            displayHint.SetPossibleOptions(possibleOptions);
            displayHint.Action = null;
        }

        private static void AddAddressToPossibleOptions(PXAddressV3Info address, Dictionary<string, SelectOptionDescription> possibleOptions, Dictionary<string, string> possibleValues, string language, int index, int count, string isDefaultMessage = "")
        {
            ActionContext actionContext = new ActionContext();
            actionContext.Instance = address;
            Dictionary<string, string> textTags = new Dictionary<string, string>()
            {
                { Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Disable },
            };

            SelectOptionDescription selectOption = new SelectOptionDescription();
            selectOption.PidlAction = new DisplayHintAction(Constants.ActionType.Success, false, actionContext, null);
            selectOption.StyleHints = Constants.NativeStyleHints.ListAddressSelectOptionStyleHints;

            // create the Address Group Display Hint container
            selectOption.DisplayContent = new GroupDisplayHint
            {
                HintId = Constants.DisplayHintIdPrefixes.AddressOptionContainer + address.Id,
                StyleHints = Constants.NativeStyleHints.AddressOptionContainerStyleHints
            };

            if (!string.IsNullOrEmpty(address.FirstName) && !string.IsNullOrEmpty(address.LastName))
            {
                string formattedName = FormatFullName(address.FirstName, address.LastName);
                TextDisplayHint firstName = new TextDisplayHint { DisplayContent = formattedName, HintId = "Name_" + address.Id, StyleHints = new List<string>() { "text-bold" }, DisplayTags = textTags };
                selectOption.DisplayContent.Members.Add(firstName);
            }

            TextDisplayHint addressLine1 = new TextDisplayHint { DisplayContent = address.AddressLine1, HintId = "address_line1_" + address.Id, StyleHints = new List<string>() { "text-bold" }, DisplayTags = textTags };
            selectOption.DisplayContent.Members.Add(addressLine1);

            if (!string.IsNullOrEmpty(address.AddressLine2))
            {
                TextDisplayHint addressLine2 = new TextDisplayHint { DisplayContent = address.AddressLine2, HintId = "address_line2_" + address.Id, StyleHints = new List<string>() { "text-bold" }, DisplayTags = textTags };
                selectOption.DisplayContent.Members.Add(addressLine2);
            }

            if (!string.IsNullOrEmpty(address.AddressLine3))
            {
                TextDisplayHint addressLine3 = new TextDisplayHint { DisplayContent = address.AddressLine3, HintId = "address_line3_" + address.Id, StyleHints = new List<string>() { "text-bold" }, DisplayTags = textTags };
                selectOption.DisplayContent.Members.Add(addressLine3);
            }

            string cityPostalCode = string.Format("{0}, {1} {2}", address.City, address.Region?.ToUpper(), address.PostalCode);
            TextDisplayHint cityPostalCodeTextHint = new TextDisplayHint { DisplayContent = cityPostalCode, HintId = "cityPostal_" + address.Id, StyleHints = new List<string>() { "text-bold" }, DisplayTags = textTags };
            selectOption.DisplayContent.Members.Add(cityPostalCodeTextHint);

            TextDisplayHint country = new TextDisplayHint { DisplayContent = address.Country?.ToUpper(), HintId = "country_" + address.Id, StyleHints = new List<string>() { "text-bold" }, DisplayTags = textTags };
            selectOption.DisplayContent.Members.Add(country);

            string displayContent = !string.IsNullOrEmpty(isDefaultMessage) ? isDefaultMessage : PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesStaticText.UseThisAddress, language);
            GroupDisplayHint focusTextGroup = CreateFocusTextGroupToAddressOption(displayContent, address.Id);

            selectOption.DisplayContent.Members.Add(focusTextGroup);

            string accessibilityString = string.Empty;
            selectOption.AccessibilityTag = FormatSuggestedAddressOptionAriaLabel(accessibilityString, address, language, index, count, displayContent);

            possibleValues.Add(address.Id, selectOption.DisplayText);
            possibleOptions.Add(address.Id, selectOption);
        }

        private static string FormatFullName(string first, string last)
        {
            return string.Format("{0} {1}", first, last);
        }

        private static void SetDataPropertyDefaultValue(PIDLResource pidlResource, string propertyName, string defaultValue)
        {
            var propDescription = pidlResource.DataDescription.ContainsKey(propertyName) ? pidlResource.DataDescription[propertyName] as PropertyDescription : null;
            if (propDescription != null)
            {
                propDescription.DefaultValue = defaultValue;
            }
        }

        private static string FormatSuggestedAddressOptionAriaLabel(string headerText, PXAddressV3Info address, string language, int index, int total, string footerDisplayText)
        {
            // Options do not have labels displayed, add labels with content as an aria-label
            // e.x. "Test person" will be read by narrator as "Name: Test Person"
            string localizedOption = PidlModelHelper.GetLocalizedString("option {0} of {1}", language);
            string formattedHeaderText = string.Format("{0} {1}", headerText, string.Format(localizedOption, index, total));
            string accessibilityString = formattedHeaderText + " ";

            if (!string.IsNullOrEmpty(address.FirstName))
            {
                accessibilityString += PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesAccessibilitySummaryLabels.FirstName, language) + ": " + address.FirstName + ", ";
            }

            if (!string.IsNullOrEmpty(address.LastName))
            {
                accessibilityString += PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesAccessibilitySummaryLabels.LastName, language) + ": " + address.LastName + ", ";
            }

            if (!string.IsNullOrEmpty(address.AddressLine1))
            {
                accessibilityString += PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesAccessibilitySummaryLabels.AddressLine1, language) + ": " + address.AddressLine1 + ", ";
            }

            if (!string.IsNullOrEmpty(address.AddressLine2))
            {
                accessibilityString += PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesAccessibilitySummaryLabels.AddressLine2, language) + ": " + address.AddressLine2 + ", ";
            }

            if (!string.IsNullOrEmpty(address.AddressLine3))
            {
                accessibilityString += PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesAccessibilitySummaryLabels.AddressLine3, language) + ": " + address.AddressLine3 + ", ";
            }

            if (!string.IsNullOrEmpty(address.City))
            {
                accessibilityString += PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesAccessibilitySummaryLabels.City, language) + ": " + address.City + ", ";
            }

            if (!string.IsNullOrEmpty(address.Region))
            {
                accessibilityString += PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesAccessibilitySummaryLabels.State, language) + ": " + address.Region + ", ";
            }

            if (!string.IsNullOrEmpty(address.PostalCode))
            {
                accessibilityString += PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesAccessibilitySummaryLabels.PostalCode, language) + ": " + address.PostalCode + ", ";
            }

            if (!string.IsNullOrEmpty(address.Country))
            {
                accessibilityString += PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesAccessibilitySummaryLabels.Country, language) + ": " + address.Country + ", ";
            }

            if (!string.IsNullOrEmpty(footerDisplayText))
            {
                accessibilityString += PidlModelHelper.GetLocalizedString(footerDisplayText);
            }

            return accessibilityString;
        }

        private static PIDLResource GetSingleSuggestedAddressPIDL(
            PXAddressV3Info curAddress,
            List<PXAddressV3Info> addressList,
            string country,
            string partner,
            string language,
            string operation,
            List<string> exposedFlightingFeatures,
            bool setAsDefaultBilling,
            bool isUserEnterdedAddressOnly,
            string scenario,
            PaymentExperienceSetting setting = null,
            bool useValidateInstanceV2UXForPidlPage = false)
        {
            // Xbox needs a unique pidl layout 
            var partnerNameInConfig = partner.Equals(Constants.PartnerNames.Xbox, StringComparison.OrdinalIgnoreCase)
                                    || !Constants.AvsSuggestEnabledPartners.Contains(partner, StringComparer.OrdinalIgnoreCase)
                ? partner
                : Constants.PartnerNames.DefaultPartner;

            string resourceId = isUserEnterdedAddressOnly
                ? Constants.PidlResourceIdentities.UserEnteredAddressOnly
                : curAddress.IsUserEntered ? Constants.PidlResourceIdentities.UserEnteredAddress : curAddress.Id;

            // 1. load address suggested pidl template from pidl configuration.
            PIDLResource resource = AddressSelectionHelper.GetAddressSuggestionPidlResource(partnerNameInConfig, country, language, resourceId, operation, exposedFlightingFeatures, setting: setting);

            if (string.Equals(partner, Constants.PartnerNames.XboxWeb, StringComparison.OrdinalIgnoreCase) && resource?.DisplayPages?.First()?.Members?.Count > 0)
            {
                var originalObject = resource.GetDisplayHintById(Constants.TextDisplayHintIds.AddressSuggestionMessage) as TextDisplayHint;
                if (originalObject != null)
                {
                    originalObject.DisplayContent = PidlModelHelper.GetLocalizedString(Constants.XboxWebSpecificLabels.XboxwebZipPlusFourAdditionalInstructions, language);
                }
            }

            // 2. fill user enter address and suggested addresses
            if (!isUserEnterdedAddressOnly)
            {
                List<PXAddressV3Info> suggestedAddressOptions = addressList.Where(address => !address.IsUserEntered).ToList();

                if (useValidateInstanceV2UXForPidlPage)
                {
                    // If the partner is template based, we need to populate the user entered address without radio button
                    suggestedAddressOptions = addressList;
                    PXAddressV3Info userEnteredAddress = addressList.FirstOrDefault(address => address.IsUserEntered);
                    AddressSelectionHelper.AddAddressText(resource, userEnteredAddress, Constants.AddressTextDisplayIdToPropertyNameMappings);
                }
                else
                {
                    // If the partner is not template based, we need to populate the user entered address as radio button
                    AddressSelectionHelper.PopulateAddressSelectOption(
                        resource,
                        addressList.Where(address => address.IsUserEntered).ToList(),
                        Constants.DisplayHintIds.AddressEntered,
                        partner,
                        scenario,
                        exposedFlightingFeatures,
                        setting: setting);
                }

                AddressSelectionHelper.PopulateAddressSelectOption(
                    resource,
                    suggestedAddressOptions,
                    Constants.DisplayHintIds.AddressSuggested,
                    partner,
                    scenario,
                    exposedFlightingFeatures,
                    useValidateInstanceV2UXForPidlPage: useValidateInstanceV2UXForPidlPage,
                    setting: setting);
            }
            else if (isUserEnterdedAddressOnly && PXCommon.Constants.PartnerGroups.IsStandardizedPartner(partner))
            {
                if (useValidateInstanceV2UXForPidlPage)
                {
                    PXAddressV3Info userEnteredAddress = addressList.FirstOrDefault(address => address.IsUserEntered);
                    AddressSelectionHelper.AddAddressText(resource, userEnteredAddress, Constants.AddressTextDisplayIdToPropertyNameMappings);
                }
                else
                {
                    AddressSelectionHelper.PopulateAddressSelectOption(
                        resource,
                        addressList.Where(address => address.IsUserEntered).ToList(),
                        Constants.DisplayHintIds.AddressEntered,
                        partner,
                        scenario,
                        exposedFlightingFeatures,
                        setting: setting);
                }
            }

            // 3. populate submit pidl action with address payload
            var button = resource.GetDisplayHintById(Constants.ButtonDisplayHintIds.AddressUseButton);
            if (button != null)
            {
                button.Action = GeneratePidlAction(curAddress, partner, language, setAsDefaultBilling, scenario: scenario);
            }

            // 4. populate datadescription "id" section with possible value, using validate regex to load different pidl when user pick different options.
            AddressSelectionHelper.PopulateIdDataDescriptionPossibleValues(resource, addressList);
            AddressSelectionHelper.PopulateIdDataDescriptionSelectValidation(resource, curAddress.Id, defaultValue: isUserEnterdedAddressOnly ? curAddress.Id : "suggested_0");

            bool hasAddressChangePage = resource.DisplayPages?.Count > 1;
            if (hasAddressChangePage)
            {
                // 5. Populate second page
                PXAddressV3Info userEnteredAddress = addressList[addressList.Count - 1];
                AddressSelectionHelper.PopulateSecondPage(
                    resource,
                    userEnteredAddress,
                    country,
                    partner,
                    language,
                    exposedFlightingFeatures,
                    setAsDefaultBilling,
                    scenario);
            }
            else
            {
                // 5. Remove extra data description if there isn't an address change page
                resource.DataDescription.Remove(Constants.DescriptionTypes.AddressBillingV3);
            }

            if (IsAmcWebReact(partner, scenario, setting))
            {
                AddAdditionalTagsForAMCWebReact(resource, isUserEnterdedAddressOnly);
            }

            // add check for partners who are on standard template
            if (PXCommon.Constants.PartnerGroups.IsStandardizedPartner(partner) && !useValidateInstanceV2UXForPidlPage)
            {
                if (isUserEnterdedAddressOnly)
                {
                    NoAddressSuggestionPidlMod(resource);
                }

                ModifyToStandardAddressValidation(resource);
            }

            return resource;
        }

        private static void AddAdditionalTagsForAMCWebReact(PIDLResource resource, bool isUserEnteredAddressOnly)
        {
            var addressOptionsGroup = resource.GetDisplayHintById("addressOptionsGroup") as GroupDisplayHint;
            if (addressOptionsGroup != null)
            {
                addressOptionsGroup.LayoutOrientation = null;
            }

            MoveElementsToBottomWithFullWidth(resource, new string[] { "addressUseCloseGroup", "addressBackSaveGroup" });

            string[] fullWidthElements = new string[] { "saveButton", "addressBackButton", "addressUseButton" };
            foreach (var element in fullWidthElements)
            {
                ApplyFullWidthToElement(resource, element);
            }

            DisplayHint changeButton = resource.GetDisplayHintById("addressChangeButton");
            changeButton?.AddDisplayTag("button-blue", "button-blue");
            if (!isUserEnteredAddressOnly)
            {
                changeButton?.AddDisplayTag("margin-left", "margin-left");
            }
            else
            {
                changeButton?.AddDisplayTag("no-padding", "no-padding");
            }
        }

        private static void AddAdditionalTagsForAMCWebReactTradeAVS(PIDLResource resource)
        {
            MoveElementsToBottomWithFullWidth(resource, new string[] { "addressUseCloseGroup", "addressNextGroup", "addressUseEnteredGroup" });

            string[] fullWidthElements = new string[] { "addressUseButton", "addressNextButton", "userEnteredButton" };
            foreach (var element in fullWidthElements)
            {
                ApplyFullWidthToElement(resource, element);
            }

            // For AMC React, the heading needs to be put in AMC owned dialog componenet by partner, so need to hide heading here
            string[] elementsToBeHidden = new string[] { "addressSuggestionHeading" };
            foreach (var element in elementsToBeHidden)
            {
                HideElement(resource, element);
            }
        }

        private static void MoveElementsToBottomWithFullWidth(PIDLResource resource, string[] hintIds)
        {
            foreach (string hintId in hintIds)
            {
                ApplyFullWidthToElement(resource, hintId);
                resource.GetDisplayHintById(hintId)?.AddDisplayTag("absolute-bottom", "absolute-bottom");
            }
        }

        private static void ApplyFullWidthToElement(PIDLResource resource, string hintId)
        {
            resource.GetDisplayHintById(hintId)?.AddDisplayTag("full-width", "full-width");
        }

        private static void HideElement(PIDLResource resource, string hintId)
        {
            DisplayHint displayHint = resource.GetDisplayHintById(hintId);
            if (displayHint != null)
            {
                displayHint.IsHidden = true;
            }
        }

        private static bool IsAmcWebReact(string partner, string scenario, PaymentExperienceSetting setting = null)
        {
            return !string.IsNullOrEmpty(scenario) && Constants.AmcWebReactScenarios.Contains(scenario, StringComparer.OrdinalIgnoreCase) &&
                   (partner.Equals(Constants.PartnerNames.AmcWeb, StringComparison.OrdinalIgnoreCase) ||
                   (TemplateHelper.IsTemplateBasedPIDL(TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.AddressDescription, Constants.AddressTypes.ShippingV3)) &&
                   string.Equals(scenario, Constants.ScenarioNames.ProfileAddress, StringComparison.OrdinalIgnoreCase)));
        }

        private static bool IsAmcWebReactTradeAVS(string partner, string scenario)
        {
            return partner.Equals(Constants.PartnerNames.AmcWeb, StringComparison.OrdinalIgnoreCase)
                   && !string.IsNullOrEmpty(scenario) && Constants.AmcWebReactTradeAVSScenarios.Contains(scenario, StringComparer.OrdinalIgnoreCase);
        }

        private static void PopulateSecondPage(
            PIDLResource resource,
            PXAddressV3Info userEnteredAddress,
            string country,
            string partner,
            string language,
            List<string> exposedFlightingFeatures,
            bool setAsDefaultBilling,
            string scenario)
        {
            // 1. Add set_as_default_billing_address data description to resource
            PIDLResourceFactory.AddSetAsDefaultBillingDataDescription(resource, partner, avsSuggest: true, setAsDefaultBilling);

            // 2. fill user enterd address
            AddressSelectionHelper.PopulateAddressDataProperty(resource, country, userEnteredAddress);

            // 3. populate submit pidl action with user entered address payload
            var saveButton = resource.GetDisplayHintById(Constants.ButtonDisplayHintIds.SaveButton) as ButtonDisplayHint;
            if (saveButton != null)
            {
                AddressSelectionHelper.PopulateAddressChangeSaveButtonPidlAction(
                    saveButton,
                    userEnteredAddress,
                    partner,
                    language,
                    scenario);
            }
        }

        private static void PopulateAddressDataProperty(PIDLResource resource, string country, PXAddressV3Info address)
        {
            var subPidls = resource.DataDescription[Constants.DescriptionTypes.AddressBillingV3] as List<PIDLResource>;
            if (subPidls == null || subPidls.Count == 0)
            {
                throw new PIDLConfigException(
                    Constants.DataDescriptionFilePaths.PIDLResourcesCSV,
                    0,
                    string.Format(
                        "Details is not defined in the \"{0}\" operation for {1} domain table in file {2}.",
                        "suggest",
                        country,
                        Constants.DataDescriptionFilePaths.PIDLResourcesCSV),
                    Constants.ErrorCodes.PIDLConfigMissingDataDescription);
            }

            SetDataPropertyDefaultValue(subPidls[0], "address_line1", address.AddressLine1);

            if (!string.IsNullOrEmpty(address.AddressLine2))
            {
                SetDataPropertyDefaultValue(subPidls[0], "address_line2", address.AddressLine2);
            }

            if (!string.IsNullOrEmpty(address.City))
            {
                SetDataPropertyDefaultValue(subPidls[0], "city", address.City);
            }

            if (!string.IsNullOrEmpty(address.Region))
            {
                SetDataPropertyDefaultValue(subPidls[0], "region", address.Region);
            }

            if (!string.IsNullOrEmpty(address.PostalCode))
            {
                SetDataPropertyDefaultValue(subPidls[0], "postal_code", address.PostalCode);
            }

            SetDataPropertyDefaultValue(subPidls[0], "country", address.Country);
        }

        private static void PopulateIdDataDescriptionPossibleValues(PIDLResource pidlResource, List<PXAddressV3Info> addressList, string scenario = null)
        {
            string dataDescriptionId = Constants.DataDescriptionIds.Id;
            if (string.Equals(scenario, Constants.ScenarioNames.SuggestAddressesTradeAVS, StringComparison.OrdinalIgnoreCase))
            {
                dataDescriptionId = Constants.DataDescriptionIds.AddressSuggestId;
            }

            var idDataDescription = pidlResource.DataDescription[dataDescriptionId] as PropertyDescription;
            Dictionary<string, string> possibleValues = new Dictionary<string, string>();
            addressList.ForEach(address => possibleValues.Add(address.Id, address.Id));
            idDataDescription.UpdatePossibleValues(possibleValues);
        }

        private static void PopulateIdDataDescriptionSelectValidation(PIDLResource pidlResource, string id, string defaultValue)
        {
            string escapeId = Regex.Replace(id, @"[*+?|{[()^$.#]", m => string.Format(@"\{0}", m.Value));
            string regex = string.Format("^{0}$", escapeId);
            var idDataDescription = pidlResource.DataDescription["id"] as PropertyDescription;
            idDataDescription.DefaultValue = defaultValue;
            idDataDescription.IsKey = true;
            idDataDescription.AddAdditionalValidation(new PropertyValidation(regex));
        }

        private static void PopulateDataDescriptionDefaultValue(PIDLResource pidlResource, string propertyId, string defaultValue)
        {
            var idDataDescription = pidlResource.DataDescription[propertyId] as PropertyDescription;
            if (idDataDescription != null)
            {
                idDataDescription.DefaultValue = defaultValue;
            }
        }

        private static DisplayHintAction GenerateBindingPidlAction(
            PXAddressV3Info curAddress,
            List<PXAddressV3Info> addressList,
            string addressType,
            bool usePidlPage)
        {
            // when user click the button, pidl sdk would update address properties' values based on which suggested address ridio button is selected (mergeData pidlAction),
            // then close the pop-up(closeModalDialog/closePidlPage pidlAction), then continue the previous suspended pidlAction (continueSuspendedAction pidlAction)
            // If user entered address is selected, then mergeData step is not needed because no change for current address properties' values.
            PropertyBindingActionContext bindingActionContext = new PropertyBindingActionContext();
            bindingActionContext.BindingPropertyName = Constants.DataDescriptionIds.AddressSuggestId;
            foreach (PXAddressV3Info address in addressList)
            {
                DisplayHintAction mergeDataAction = new DisplayHintAction(DisplayHintActionType.mergeData.ToString());
                MergeDataActionContext mergeDataActionContext = new MergeDataActionContext();

                // Set "is_customer_consented" to be true if user entered address can't be validated and user still want to use the address
                // set "is_avs_full_validation_succeeded" to be true for suggested addresses
                if (string.Equals(address.Id, curAddress.Id, StringComparison.OrdinalIgnoreCase))
                {
                    address.IsCustomerConsented = true;
                }
                else
                {
                    address.IsAVSFullValidationSucceeded = true;
                }

                mergeDataActionContext.Payload = ExtractAddressInfoForTradeAvs(address, addressType);
                mergeDataAction.Context = mergeDataActionContext;

                DisplayHintAction continueSuspendedAction = new DisplayHintAction(DisplayHintActionType.continueSuspendedAction.ToString());
                DisplayHintAction closeAction = new DisplayHintAction(usePidlPage ? DisplayHintActionType.closePidlPage.ToString() : DisplayHintActionType.closeModalDialog.ToString());
                closeAction.NextAction = continueSuspendedAction;
                mergeDataAction.NextAction = closeAction;
                bindingActionContext.AddActionItem(address.Id, mergeDataAction);
            }

            return new DisplayHintAction(DisplayHintActionType.propertyBindingAction.ToString(), true, bindingActionContext, null);
        }

        private static DisplayHintAction GenerateBindingPidlActionXboxNative(
            PXAddressV3Info curAddress,
            PXAddressV3Info addressListElement,
            string addressType,
            bool usePidlPage)
        {
            // similar to GenerateBindingPidlAction(), but instead of having the continue attached to the "use this address" button it should be individually attached to the
            // address buttons
            PropertyBindingActionContext bindingActionContext = new PropertyBindingActionContext();
            bindingActionContext.BindingPropertyName = Constants.DataDescriptionIds.AddressSuggestId;

            DisplayHintAction mergeDataAction = new DisplayHintAction(DisplayHintActionType.mergeData.ToString());
            MergeDataActionContext mergeDataActionContext = new MergeDataActionContext();

            // Set "is_customer_consented" to be true if user entered address can't be validated and user still want to use the address
            // set "is_avs_full_validation_succeeded" to be true for suggested addresses
            if (string.Equals(addressListElement.Id, curAddress.Id, StringComparison.OrdinalIgnoreCase))
            {
                addressListElement.IsCustomerConsented = true;
            }
            else
            {
                addressListElement.IsAVSFullValidationSucceeded = true;
            }

            mergeDataActionContext.Payload = ExtractAddressInfoForTradeAvs(addressListElement, addressType);
            mergeDataAction.Context = mergeDataActionContext;

            DisplayHintAction continueSuspendedAction = new DisplayHintAction(DisplayHintActionType.continueSuspendedAction.ToString());
            DisplayHintAction closeAction = new DisplayHintAction(usePidlPage ? DisplayHintActionType.closePidlPage.ToString() : DisplayHintActionType.closeModalDialog.ToString());
            closeAction.NextAction = continueSuspendedAction;
            mergeDataAction.NextAction = closeAction;
            bindingActionContext.AddActionItem(addressListElement.Id, mergeDataAction);

            return new DisplayHintAction(DisplayHintActionType.propertyBindingAction.ToString(), true, bindingActionContext, null);
        }

        private static DisplayHintAction GenerateUserEnteredButtonPidlAction(bool usePidlPage)
        {
            DisplayHintAction mergeDataAction = new DisplayHintAction(DisplayHintActionType.mergeData.ToString());
            MergeDataActionContext mergeDataActionContext = new MergeDataActionContext();

            // Set "is_customer_consented" to be true if user entered address can't be validated and user still want to use the address
            PIDLData address = new PIDLData();
            address[Constants.CommercialZipPlusFourPropertyNames.IsUserConsented] = true;

            mergeDataActionContext.Payload = address;
            mergeDataAction.Context = mergeDataActionContext;

            DisplayHintAction continueSuspendedAction = new DisplayHintAction(DisplayHintActionType.continueSuspendedAction.ToString());
            DisplayHintAction closeAction = new DisplayHintAction(usePidlPage ? DisplayHintActionType.closePidlPage.ToString() : DisplayHintActionType.closeModalDialog.ToString());
            closeAction.NextAction = continueSuspendedAction;
            closeAction.IsDefault = true;
            mergeDataAction.NextAction = closeAction;
            return mergeDataAction;
        }

        private static DisplayHintAction GeneratePidlAction(
            PXAddressV3Info address,
            string partner,
            string language,
            bool setAsDefaultBilling,
            string scenario)
        {
            if (address == null)
            {
                return null;
            }

            bool isExistingAddress = !string.Equals(address.Id, GlobalConstants.SuggestedAddressesIds.UserEntered, StringComparison.InvariantCultureIgnoreCase);
            if (address.IsUserEntered)
            {
                address.IsCustomerConsented = true;
                RestLink restLink = new RestLink();
                if (isExistingAddress)
                {
                    restLink.Href = AppendParameterScenario(
                        $"{Constants.SubmitUrls.PifdAddressPostUrlTemplate}/{address.Id}?partner={partner}&language={language}",
                        scenario);

                    restLink.Payload = new
                    {
                        is_customer_consented = address.IsCustomerConsented,
                    };

                    restLink.AddHeader(Constants.CustomHeaders.IfMatch, address.Etag);
                    restLink.Method = Constants.HTTPVerbs.PATCH;

                    if (partner == Constants.PartnerNames.WindowsSettings)
                    {
                        return new DisplayHintAction(Constants.ActionType.Submit, true, restLink, null);
                    }

                    return new DisplayHintAction(Constants.ActionType.RestAction, true, restLink, null);
                }
            }

            address.DefaultBillingAddress = setAsDefaultBilling;
            var actionContext = new RestLink();
            actionContext.Href = AppendParameterScenario(
                $"{Constants.SubmitUrls.PifdAddressPostUrlTemplate}?partner={partner}&language={language}&avsSuggest={false}",
                scenario);
            actionContext.Payload = address;
            actionContext.Method = Constants.HTTPVerbs.POST;

            if (partner == Constants.PartnerNames.WindowsSettings)
            {
                return new DisplayHintAction(Constants.ActionType.Submit, true, actionContext, null);
            }

            return new DisplayHintAction(Constants.ActionType.RestAction, true, actionContext, null);
        }

        private static string AppendParameterScenario(string url, string scenario)
        {
            if (!string.IsNullOrEmpty(scenario))
            {
                url += $"&scenario={scenario}";
            }

            return url;
        }

        private static void PopulateAddressChangeSaveButtonPidlAction(
            ButtonDisplayHint button,
            PXAddressV3Info address,
            string partner,
            string language,
            string scenario)
        {
            if (address == null)
            {
                return;
            }

            // 1. Populate PIDL action
            RestLink actionContext = new RestLink();
            actionContext.Href = AppendParameterScenario(
                    $"{Constants.SubmitUrls.PifdAddressPostUrlTemplate}?partner={partner}&language={language}&avsSuggest={true}",
                    scenario);

            actionContext.Method = Constants.HTTPVerbs.POST;
            button.Action = new DisplayHintAction("submit", true, actionContext, null);
        }

        private static void AddOption(
            Dictionary<string, SelectOptionDescription> possibleOptions,
            PXAddressV3Info address,
            string partner,
            string scenario,
            List<string> exposedFlightingFeatures,
            string index,
            bool useV2UXForPidlPage = false,
            bool useValidateInstanceV2UXForPidlPage = false,
            PaymentExperienceSetting setting = null)
        {
            SelectOptionDescription selectOption = new SelectOptionDescription();
            selectOption.DisplayText = IsAmcWebReact(partner, scenario, setting) ||
                exposedFlightingFeatures.Contains(Flighting.Features.PXZip4RemoveDisplayTextAddressID, StringComparer.OrdinalIgnoreCase) ||
                string.Equals(scenario, Constants.ScenarioNames.SuggestAddressesTradeAVS, StringComparison.OrdinalIgnoreCase) ||
                useValidateInstanceV2UXForPidlPage ?
                string.Empty :
                address.Id;

            selectOption.DisplayContent = new GroupDisplayHint
            {
                HintId = Constants.DisplayHintIdPrefixes.AddressOptionContainer + address.Id
            };

            // For SuggestAddressesTradeAVS senario, add "Current address" or "Suggested address" before the address text for each option
            if (string.Equals(scenario, Constants.ScenarioNames.SuggestAddressesTradeAVS, StringComparison.OrdinalIgnoreCase) || useValidateInstanceV2UXForPidlPage)
            {
                if (address.IsUserEntered)
                {
                    var currentAddressDisplayHint = new TextDisplayHint { HintId = "address_type_" + address?.Id };
                    if (useV2UXForPidlPage)
                    {
                        currentAddressDisplayHint.DisplayContent = PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesStaticText.KeepCurrentAddress);
                    }
                    else if (useValidateInstanceV2UXForPidlPage)
                    {
                        currentAddressDisplayHint.DisplayContent = PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesStaticText.KeepAddressEntered);
                    }
                    else
                    {
                        currentAddressDisplayHint.DisplayContent = PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesStaticText.CurrentAddress);
                        currentAddressDisplayHint.AddDisplayTag("pidlReact.labelText", "pidlReact.labelText");
                    }

                    selectOption.DisplayContent.Members.Add(currentAddressDisplayHint);
                }
                else
                {
                    if (!useV2UXForPidlPage && !useValidateInstanceV2UXForPidlPage)
                    {
                        var suggestedAddressDisplayHint = new TextDisplayHint { DisplayContent = PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesStaticText.SuggestedAddress + index), HintId = "address_type_" + address?.Id };
                        suggestedAddressDisplayHint.AddDisplayTag("pidlReact.labelText", "pidlReact.labelText");
                        selectOption.DisplayContent.Members.Add(suggestedAddressDisplayHint);
                    }
                }
            }

            // For TradeAVS/ValidateInstance V2 UX, we will just show a single line text to inform user that this option is for using the entered address, instead of showing the entered address details.
            if (!((useV2UXForPidlPage || useValidateInstanceV2UXForPidlPage) && address.IsUserEntered))
            {
                string region = PopulateRegionName(address);
                selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = address?.AddressLine1, HintId = "address_line1_" + address?.Id });
                selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = address?.AddressLine2, HintId = "address_line2_" + address?.Id });
                selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = address?.AddressLine3, HintId = "address_line3_" + address?.Id });
                selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = address?.City + ((region != null) ? ", " + region : region), HintId = "city_region_" + address?.Id });
                selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = address?.PostalCode, HintId = "postal_code_" + address.Id });
            }

            selectOption.DisplayContent.AddDisplayTag("address-container", "address-container");
            possibleOptions.Add(address.Id, selectOption);
        }

        private static string RemoveRegionName(string country, string region)
        {
            return Constants.CountriesToExcludeRegionName.Contains(country) ? null : region;
        }

        private static string ConvertRegionCodeToRegionName(string country, string region)
        {
            if (region != null)
            {
                if (Constants.CountriesToNotCapitalizeRegionNames.ContainsKey(country))
                {
                    string regionsCollectionName = Constants.CountriesToNotCapitalizeRegionNames[country];
                    Dictionary<string, string> regions = PIDLResourceFactory.GetDictionaryFromConfigString("{}" + regionsCollectionName);
                    if (regions.ContainsKey(region))
                    {
                        region = regions[region];
                    }
                }
                else
                {
                    region = region.ToUpper();
                }
            }

            return region;
        }

        private static string PopulateRegionName(PXAddressV3Info address)
        {
            string region = address?.Region;
            string country = address?.Country;
            region = RemoveRegionName(country, region);
            region = ConvertRegionCodeToRegionName(country, region);
            return region;
        }

        private static void AddAddressText(
            PIDLResource pidlResource,
            PXAddressV3Info address,
            Dictionary<string, List<string>> displayIdToPropertyMappings)
        {
            Dictionary<string, string> addressPropertyDictionary = address.GetPropertyDictionary();
            if (addressPropertyDictionary.ContainsKey(Constants.DataDescriptionIds.Region))
            {
                addressPropertyDictionary[Constants.DataDescriptionIds.Region] = PopulateRegionName(address);
            }

            foreach (KeyValuePair<string, List<string>> displayIdToPropertyMapping in displayIdToPropertyMappings)
            {
                var textDisplayHint = pidlResource.GetDisplayHintById(displayIdToPropertyMapping.Key) as TextDisplayHint;
                if (textDisplayHint != null)
                {
                    List<string> addrestStrings = new List<string>();
                    foreach (string propertyName in displayIdToPropertyMapping.Value)
                    {
                        string addressString;
                        if (addressPropertyDictionary.TryGetValue(propertyName, out addressString))
                        {
                            addrestStrings.Add(addressString);
                        }
                    }

                    textDisplayHint.DisplayContent = string.Join(",", addrestStrings);
                }
            }
        }

        private static void PopulateAddressSelectOption(
            PIDLResource pidlResource,
            List<PXAddressV3Info> addressList,
            string displayHintId,
            string partner,
            string scenario,
            List<string> exposedFlightingFeatures,
            bool useV2UXForPidlPage = false,
            bool useValidateInstanceV2UXForPidlPage = false,
            PaymentExperienceSetting setting = null)
        {
            int index = 0;
            PropertyDisplayHint displayHint = pidlResource.GetDisplayHintById(displayHintId) as PropertyDisplayHint;
            Dictionary<string, SelectOptionDescription> possibleOptions = new Dictionary<string, SelectOptionDescription>();
            addressList.ForEach(address => AddOption(possibleOptions, address, partner, scenario, exposedFlightingFeatures, $"{" " + index++}", useV2UXForPidlPage, useValidateInstanceV2UXForPidlPage, setting));
            displayHint.SetPossibleOptions(possibleOptions);
        }

        private static void PopulateAddressButtonPidlActions(
            PIDLResource resource,
            PXAddressV3Info userEnteredAddress,
            List<PXAddressV3Info> addressList,
            string addressType)
        {
            var property = resource.GetDisplayHintById(V7.Constants.DisplayHintIds.SuggestedAddresses) as PropertyDisplayHint;
            var buttonList = property.PossibleOptions;
            var count = 0;
            foreach (var addressButton in buttonList)
            {
                var xboxNativeAddressPidl = GenerateBindingPidlActionXboxNative(userEnteredAddress, addressList[count++], addressType, true).Context as PropertyBindingActionContext;
                addressButton.Value.PidlAction = xboxNativeAddressPidl.ActionMap.First().Value as DisplayHintAction;
            }
        }

        private static void NoAddressSuggestionPidlMod(PIDLResource resource)
        {
            // For addresses without any suggestions, remove unnecessary displayHints
            resource.RemoveDisplayHintById(Constants.DisplayHintIds.SuggestedAddressText);
            resource.RemoveDisplayHintById(Constants.DisplayHintIds.AddressSuggested);

            // Curently, userEnteredAddress is in radioButton format. Change to text format 

            // Get the core userEnteredAddress 
            PropertyDisplayHint addressEntered = resource.GetDisplayHintById(Constants.DisplayHintIds.AddressEntered) as PropertyDisplayHint;
            GroupDisplayHint entered = addressEntered.PossibleOptions[Constants.DisplayHintIds.Entered].DisplayContent;
            GroupDisplayHint addressChangeGroup = resource.GetDisplayHintById(Constants.DisplayHintIds.AddressChangeGroup) as GroupDisplayHint;

            resource.RemoveDisplayHintById(Constants.DisplayHintIds.AddressEntered); //// Remove unnecessary radio button
            resource.RemoveDisplayHintById(Constants.DisplayHintIds.AddressChangeGroup); //// Remove unnecessary radio button

            GroupDisplayHint addressEnteredGroup = resource.GetDisplayHintById(Constants.DisplayHintIds.AddressEnteredGroup) as GroupDisplayHint;
            addressEnteredGroup.Members.Add(entered);
            addressEnteredGroup.Members.Add(addressChangeGroup);

            // without any possible options, no need for the following data description 
            PIDLResourceFactory.RemoveDataDescriptionWithFullPath(resource, null, new string[] { Constants.DataDescriptionIds.Id });
        }

        private static void ModifyToStandardAddressValidation(PIDLResource resource)
        {
            //// Remove the assigned name before the addy 
            //// TODO: Could remove foreach, depending on element factory
            PropertyDisplayHint addressSuggested = resource.GetDisplayHintById(Constants.DisplayHintIds.AddressSuggested) as PropertyDisplayHint;

            if (addressSuggested != null)
            {
                foreach (var displayId in addressSuggested.PossibleOptions)
                {
                    displayId.Value.DisplayText = string.Empty;
                }

                PropertyDisplayHint addressEntered = resource.GetDisplayHintById(Constants.DisplayHintIds.AddressEntered) as PropertyDisplayHint;
                GroupDisplayHint addressEnteredGroup = resource.GetDisplayHintById(Constants.DisplayHintIds.AddressEnteredGroup) as GroupDisplayHint;

                addressEnteredGroup.Members.Add(addressEntered.PossibleOptions.FirstOrDefault().Value.DisplayContent); //// add a copy of the user entered addy to the userEntered group. This doesnt have possible options attached therefore won't have a radio button  

                PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<PropertyDisplayHint>(Constants.DisplayHintIds.AddressEntered, resource.DisplayPages);

                //// Add the last option to the suggested group 
                SelectOptionDescription currentAddressSelection = new SelectOptionDescription();
                currentAddressSelection.DisplayText = Constants.StandardizedDisplayText.AddressValidationUserEntered;
                addressSuggested.PossibleOptions.Add(addressEntered.PossibleOptions.FirstOrDefault().Key, currentAddressSelection);

                GroupDisplayHint addressChangeGroup = resource.GetDisplayHintById(Constants.DisplayHintIds.AddressChangeGroup) as GroupDisplayHint;
                addressEnteredGroup.Members.Add(addressChangeGroup); // Add change button to the end of the user entered group (would be duplicated) 
                addressEnteredGroup.Members.Remove(addressChangeGroup); // remove the change button from the middle of the group

                AddAccessbilityLabelToVerifyAddressFlow(resource);
            }
            else
            {
                // without any possible options, no need for the following data description 
                // TODO: update the above comment with why data descriptions is not needed
                PIDLResourceFactory.RemoveDataDescriptionWithFullPath(resource, null, new string[] { Constants.DataDescriptionIds.Id });
                resource.DisplayPages[0].HintId = Constants.PageDisplayHintIds.NoAddressSuggestionsPage; //// needed to identify page with no address suggestion on client side
            }
        }

        private static void AddAccessbilityLabelToVerifyAddressFlow(PIDLResource resource)
        {
            PropertyDisplayHint addressSuggested = resource.GetDisplayHintById(Constants.DisplayHintIds.AddressSuggested) as PropertyDisplayHint;

            foreach (var suggestedOption in addressSuggested?.PossibleOptions)
            {
                SelectOptionDescription address = suggestedOption.Value as SelectOptionDescription;
                string accessibilityName = string.Empty;
                if (suggestedOption.Key.Contains(Constants.SuggestedAddressesStaticText.SuggestedLabel))
                {
                    foreach (TextDisplayHint addressPortion in address.DisplayContent.Members)
                    {
                        accessibilityName = addressPortion.DisplayContent != null ? accessibilityName + addressPortion.DisplayContent + "," : accessibilityName;
                    }

                    address.DisplayContent.AddDisplayTag(Constants.DiplayHintProperties.AccessibilityName, accessibilityName.Substring(0, accessibilityName.Length - 1));
                }
            }

            GroupDisplayHint addressDetailsDataGroup = resource.GetDisplayHintById(Constants.DisplayHintIds.AddressDetailsDataGroup) as GroupDisplayHint;
            foreach (PropertyDisplayHint addressField in addressDetailsDataGroup.Members)
            {
                if (!addressField.DisplayName.Contains(Constants.SuggestedAddressesAccessibilitySummaryLabels.Optional))
                {
                    addressField.DisplayTags[Constants.DiplayHintProperties.AccessibilityName] = addressField.DisplayTags[Constants.DiplayHintProperties.AccessibilityName] + " (" + Constants.SuggestedAddressesAccessibilitySummaryLabels.Required + ")";
                }
            }
        }
    }
}
