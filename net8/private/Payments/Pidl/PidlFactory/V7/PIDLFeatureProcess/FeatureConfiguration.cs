// <copyright file="FeatureConfiguration.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PXCommon;

    /// <summary>
    /// Class maintaining the configuration of features
    /// </summary>
    internal static class FeatureConfiguration
    {
        private static List<string> pmGroupingEnabledPartners = new List<string>()
        {
            Constants.PartnerNames.Cart,
            Constants.PartnerNames.Webblends,
        };

        internal static bool IsEnabled(string featureName, FeatureContext featureParams)
        {
            switch (featureName)
            {
                // SetButtonActionToSuccessType is a feature to force the pidlAction to be success type for a button in button list of PM selection form
                // This feature is used to prevent unexpected pidlActions (e.g. addResource/MoveToPageIndex) being added for default buttonList flow.
                case FeatureNames.SetButtonActionToSuccessType:
                    return !featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXSelectPMInvokeResourceAction, StringComparer.OrdinalIgnoreCase)
                           && !(PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(featureParams.Partner)
                                || (featureParams.OriginalPartner != null && PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(featureParams.OriginalPartner)))
                           && !(pmGroupingEnabledPartners.Contains(featureParams.Partner, StringComparer.OrdinalIgnoreCase)
                                && featureParams.ExposedFlightFeatures.Contains(V7.Constants.PartnerFlightValues.EnablePaymentMethodGrouping, StringComparer.OrdinalIgnoreCase)
                                && string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Select, StringComparison.OrdinalIgnoreCase));
                case FeatureNames.PaymentMethodGrouping:
                    if ((pmGroupingEnabledPartners.Contains(featureParams.Partner, StringComparer.OrdinalIgnoreCase) || PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(featureParams.Partner) || (featureParams.OriginalPartner != null && PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(featureParams.OriginalPartner)))
                            && featureParams.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.EnablePaymentMethodGrouping, StringComparer.OrdinalIgnoreCase)
                            && string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Select, StringComparison.OrdinalIgnoreCase))
                    {
                        // if we have all payment methods having credit_card as family and flight is enable then we need skip to apply PaymentMethodGrouping feature
                        if (featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXDisablePMGrouping, StringComparer.OrdinalIgnoreCase) && featureParams.PaymentMethods != null && !featureParams.PaymentMethods.Where(e => e.PaymentMethodFamily != Constants.PaymentMethodFamilyNames.CreditCard).Any())
                        {
                            return false;
                        }

                        if (featureParams.Partner.Equals(Constants.PartnerNames.WindowsSettings, StringComparison.OrdinalIgnoreCase))
                        {
                            SetGroupedSelectOptionTextBeforeLogoState(featureParams, FeatureNames.PaymentMethodGrouping);
                            SetMatchSelectPMMainPageStructureForSubPage(featureParams, FeatureNames.PaymentMethodGrouping);
                        }

                        if (string.Equals(featureParams.Scenario, Constants.ScenarioNames.SelectPMWithLogo))
                        {
                            SetSelectPMWithLogo(featureParams, FeatureNames.PaymentMethodGrouping);
                        }

                        return true;
                    }

                    return false;
                case FeatureNames.SetDefaultPaymentMethod:
                    if (featureParams?.ExposedFlightFeatures != null && featureParams.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableDefaultPaymentMethod, StringComparer.OrdinalIgnoreCase)
                        && !featureParams.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.EnablePaymentMethodGrouping, StringComparer.OrdinalIgnoreCase)
                        && string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Select, StringComparison.OrdinalIgnoreCase))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { }, featureName);
                        return true;
                    }

                    return false;
                case FeatureNames.GroupedPaymentOptionsTextBeforeLogo:
                case FeatureNames.UseTextForCVVHelpLink:
                case FeatureNames.SwapLogoSource:
                    return featureParams.Partner.Equals(Constants.PartnerNames.WindowsSettings, StringComparison.OrdinalIgnoreCase);
                case FeatureNames.ChangeDisplayHintToText:
                    if (featureParams.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.UpdateDisplayHint, StringComparer.OrdinalIgnoreCase))
                    {
                        ChangeDisplayHintToText(featureParams);
                        return true;
                    }

                    return false;
                case FeatureNames.EnableSecureField:
                    if (IsSecureFieldEnabled(featureParams))
                    {
                        SetSecureFieldState(featureParams);
                        return true;
                    }

                    return false;
                case FeatureNames.EnableUpdateCreditCardRegex:
                    bool isUpdateCreditCardRegexEnabled = false;
                    
                    var displayCustomizationDetail = new DisplayCustomizationDetail() { UpdateRegexesForCards = new List<KeyValuePair<string, string>>() };
                    var regexList = displayCustomizationDetail.UpdateRegexesForCards as List<KeyValuePair<string, string>>;

                    if (IsUpdateCreditCardRegexEnabled(featureParams, Constants.PartnerFlightValues.PXEnableUpdateDiscoverCreditCardRegex))
                    {
                        regexList.Add(new KeyValuePair<string, string>(Constants.PaymentMethodTypeNames.Discover, "^3(?:6|8|9[0-9])[0-9]{12}|6(?:011|4[4-9][0-9]|5[0-9]{2})[0-9]{12}|62[4-6][0-9]{13}|628[2-8][0-9]{12}|622(1(2[6-9]|[3-9][0-9])|[2-8][0-9]{2}|9([01][0-9]|2[0-5]))[0-9]{10}$"));
                        isUpdateCreditCardRegexEnabled = true;
                    }

                    // Once the PXEnableUpdateVisaCreditCardRegex flight is fully rolled out (100 %) and confirmed to be stable, 
                    // update the CSV logic to make this change independent of the flight configuration and remove the below if conditions and related code.
                    if (IsUpdateCreditCardRegexEnabled(featureParams, Constants.PartnerFlightValues.PXEnableUpdateVisaCreditCardRegex))
                    {
                        regexList.Add(new KeyValuePair<string, string>(Constants.PaymentMethodTypeNames.Visa, "^4[0-9]{15}$"));
                        isUpdateCreditCardRegexEnabled = true;
                    }

                    //// ToDo: Here we can add more regexes for other cards if needed

                    if (isUpdateCreditCardRegexEnabled)
                    {
                        // Set the display customization detail for the feature
                        SetDisplayCustomizationDetail(featureParams, displayCustomizationDetail, FeatureNames.EnableUpdateCreditCardRegex);
                    }

                    return isUpdateCreditCardRegexEnabled;
                case FeatureNames.CustomizeDisplayContent:
                    bool isEnabled = false;
                    if (featureParams.Partner.Equals(Constants.PartnerNames.WindowsSettings, StringComparison.OrdinalIgnoreCase))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { AddressSuggestionMessage = true }, FeatureNames.CustomizeDisplayContent);

                        if (featureParams.OperationType.Equals(Constants.PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase))
                        {
                            SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { UpdateSelectPiButtonText = true }, FeatureNames.CustomizeDisplayContent);
                        }

                        isEnabled = true;
                    }

                    if (featureParams.IsGuestAccount)
                    {
                        // For Guest account, we need to change the save button text from 'Save' to 'Next'
                        // We are updating featureContext to enable CustomizeDisplayContent for SetSaveButtonDisplayContentAsNext
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { SetSaveButtonDisplayContentAsNext = true, UpdateCvvChallengeTextForGCO = true }, FeatureNames.CustomizeDisplayContent);
                        isEnabled = true;
                    }

                    if (featureParams.ExposedFlightFeatures != null && featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableAddAllFieldsRequiredText, StringComparer.OrdinalIgnoreCase))
                    {
                        // For partners who have the feature flag PXEnableAddAllFieldsRequiredText enabled, we need to add the text 'All fields are required' to the display content.
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { AddAllFieldsRequiredText = true }, FeatureNames.CustomizeDisplayContent);
                        isEnabled = true;
                    }

                    if (featureParams.ExposedFlightFeatures != null && featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableAddAsteriskToAllMandatoryFields, StringComparer.OrdinalIgnoreCase))
                    {
                        // For partners who have the feature flag PXEnableAddAsteriskToAllMandatoryFields enabled, we need to add an asterisk to all mandatory fields.
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { AddAsteriskToAllMandatoryFields = true }, FeatureNames.CustomizeDisplayContent);
                        isEnabled = true;
                    }

                    return isEnabled;
                case FeatureNames.DisableElement:
                    if (featureParams.Partner.Equals(Constants.PartnerNames.WindowsSettings, StringComparison.OrdinalIgnoreCase) &&
                        featureParams.OperationType.Equals(Constants.PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { DisableSelectPiRadioOption = true }, FeatureNames.DisableElement);

                        return true;
                    }

                    return false;
                case FeatureNames.CustomizeDisplayTag:
                    bool retVal = false;
                    if (featureParams.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableXboxAccessibilityHint))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { UpdateXboxElementsAccessibilityHints = true }, featureName);
                        retVal = true;
                    }

                    // Adding accessibility label with count for Handle Challenge is done in ChallengeDisplayHelper class
                    // We don't need to add accessibility label with count for Handle Challenge again with this feature.
                    // TODO: Remove the resource check here once we remove the code for adding accessibility label with count for Handle Challenge in ChallengeDisplayHelper class
                    if (featureParams.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.UpdateAccessibilityNameWithPosition)
                        && !string.Equals(featureParams.ResourceType, Constants.ResourceTypes.Challenge, StringComparison.OrdinalIgnoreCase))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { UpdateAccessibilityNameWithPosition = true }, featureName);
                        retVal = true;
                    }

                    if (featureParams.ExposedFlightFeatures?.Contains(Constants.PartnerFlightValues.ApplyAccentBorderWithGutterOnFocus) == true)
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { DisplayAccentBorderWithGutterOnFocus = true }, featureName);
                        retVal = true;
                    }

                    return retVal;
                case FeatureNames.UpdateStaticResourceServiceEndPoint:
                    return featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXUseCDNForStaticResourceService, StringComparer.OrdinalIgnoreCase);
                case FeatureNames.RemoveElement:
                    if (featureParams.Partner.Equals(Constants.PartnerNames.WindowsSettings, StringComparison.OrdinalIgnoreCase)
                        && featureParams.OperationType.Equals(Constants.PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { RemoveSelectPiEditButton = true }, FeatureNames.RemoveElement);
                        return true;
                    }

                    return false;
                case FeatureNames.SkipJarvisAddressSyncToLegacy:
                    return featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXSkipJarvisAddressSyncToLegacy, StringComparer.OrdinalIgnoreCase);
                case FeatureNames.CustomizeActionContext:
                    if (featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableReplaceContextInstanceWithPaymentInstrumentId, StringComparer.OrdinalIgnoreCase))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { ReplaceContextInstanceWithPaymentInstrumentId = true }, FeatureNames.CustomizeActionContext);
                        return true;
                    }

                    return false;
                case FeatureNames.EnableShortURL:
                    bool addShortUrlItemsPayPal = featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableShortUrlPayPal, StringComparer.OrdinalIgnoreCase) && featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableShortUrlPayPalText, StringComparer.OrdinalIgnoreCase);
                    bool addShortUrlItemsVenmo = featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableShortUrlVenmo, StringComparer.OrdinalIgnoreCase) && featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableShortUrlVenmoText, StringComparer.OrdinalIgnoreCase);
                    if (addShortUrlItemsPayPal || addShortUrlItemsVenmo)
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { }, FeatureNames.EnableShortURL);
                        return true;
                    }

                    return false;
                case FeatureNames.EnableTokenizationEncryption:
                    if (IsTokenizationEncryptionEnabled(featureParams))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail(), FeatureNames.EnableTokenizationEncryption);
                        return true;
                    }

                    return false;
                case FeatureNames.EnableTokenizationEncryptionFetchConfig:
                    if (IsTokenizationEncryptionFetchConfigEnabled(featureParams))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail(), FeatureNames.EnableTokenizationEncryptionFetchConfig);
                        return true;
                    }

                    return false;
                case FeatureNames.PXEnableXboxNativeStyleHints:
                    bool isXboxNativeStyleHintsFeatureEnabled = false;
                    bool useStyleHints = string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Apply, StringComparison.OrdinalIgnoreCase) ? 
                        featureParams.ExposedFlightFeatures != null && featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableApplyPIXboxNativeStyleHints) : 
                        featureParams.ExposedFlightFeatures != null && featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableXboxNativeStyleHints);
                    if (useStyleHints)
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { AddStyleHints = true }, FeatureNames.PXEnableXboxNativeStyleHints);
                        isXboxNativeStyleHintsFeatureEnabled = true;
                    }
                    else if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(featureParams.Partner))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { RemoveStyleHints = true }, FeatureNames.PXEnableXboxNativeStyleHints);
                        isXboxNativeStyleHintsFeatureEnabled = true;
                    }

                    return isXboxNativeStyleHintsFeatureEnabled;
                case FeatureNames.CustomizeAddressForm:
                    if (featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableSetCancelButtonDisplayContentAsBack, StringComparer.OrdinalIgnoreCase))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { SetCancelButtonDisplayContentAsBack = true, AddressType = featureParams.TypeName }, FeatureNames.CustomizeAddressForm);
                        return true;
                    }

                    return false;

                case FeatureNames.UpdateAddressline1Length:
                    if (featureParams.ExposedFlightFeatures.Contains(Flighting.Features.UpdateAddressline1MaxLength, StringComparer.OrdinalIgnoreCase))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail(), FeatureNames.UpdateAddressline1Length);
                        return true;
                    }

                    return false;

                case FeatureNames.ChangeExpiryMonthYearToExpiryDateTextBox:
                    if (featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXChangeExpiryMonthYearToExpiryDateTextBox, StringComparer.OrdinalIgnoreCase)
                        && string.Equals(featureParams.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase)
                        && (string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase)))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail(), FeatureNames.ChangeExpiryMonthYearToExpiryDateTextBox);
                        return true;
                    }

                    return false;
                
                case FeatureNames.CombineExpiryMonthYearToDateTextBox:
                    if (featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXCombineExpiryMonthYearToDateTextBox, StringComparer.OrdinalIgnoreCase)
                        && string.Equals(featureParams.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase)
                        && (string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase)))
                    {
                        SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail(), FeatureNames.CombineExpiryMonthYearToDateTextBox);
                        return true;
                    }

                    return false;

                default:
                    return false;
            }
        }

        internal static bool IsEnabledUsingPartnerSettings(string featureName, FeatureContext featureContext)
        {
            if (featureContext.FeatureConfigs != null)
            {
                FeatureConfig featureConfig;
                featureContext.FeatureConfigs.TryGetValue(featureName, out featureConfig);

                if (featureConfig != null)
                {
                    if (featureConfig.ApplicableMarkets == null || featureConfig.ApplicableMarkets.Count == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return featureConfig.ApplicableMarkets.Contains(featureContext.Country, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }

            return false;
        }

        private static bool IsSecureFieldEnabled(FeatureContext featureParams)
        {
            bool enableSecureField = false;
            if (featureParams.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableSecureFieldAddCreditCard, StringComparer.OrdinalIgnoreCase)
                && string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase)
                && string.Equals(featureParams.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase))
            {
                enableSecureField = true;
            }
            else if (featureParams.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableSecureFieldUpdateCreditCard, StringComparer.OrdinalIgnoreCase)
                && string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase)
                && string.Equals(featureParams.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase))
            {
                enableSecureField = true;
            }
            else if (featureParams.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableSecureFieldReplaceCreditCard, StringComparer.OrdinalIgnoreCase)
                && string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Replace, StringComparison.OrdinalIgnoreCase)
                && string.Equals(featureParams.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase))
            {
                enableSecureField = true;
            }
            else if (featureParams.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableSecureFieldSearchTransaction, StringComparer.OrdinalIgnoreCase)
                && string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.SearchTransactions, StringComparison.OrdinalIgnoreCase))
            {
                enableSecureField = true;
            }
            else if (featureParams.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableSecureFieldCvvChallenge, StringComparer.OrdinalIgnoreCase)
                && string.Equals(featureParams.ResourceType, Constants.DescriptionTypes.ChallengeDescription, StringComparison.OrdinalIgnoreCase))
            {
                enableSecureField = true;
            }
            else if (featureParams.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableSecureFieldIndia3DSChallenge, StringComparer.OrdinalIgnoreCase)
                && string.Equals(featureParams.ResourceType, Constants.ChallengeDescriptionTypes.Cvv, StringComparison.OrdinalIgnoreCase))
            {
                enableSecureField = true;
            }

            return enableSecureField;
        }

        private static bool IsUpdateCreditCardRegexEnabled(FeatureContext featureParams, string flightName)
        {
            bool enableUpdateCreditCardRegex = false;
            if (featureParams.ExposedFlightFeatures.Contains(flightName, StringComparer.OrdinalIgnoreCase)
                && string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase)
                && string.Equals(featureParams.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase))
            {
                enableUpdateCreditCardRegex = true;
            }

            return enableUpdateCreditCardRegex;
        }

        private static void ChangeDisplayHintToText(FeatureContext featureParams)
        {
            Dictionary<string, FeatureConfig> featureConfig;
            featureConfig = new Dictionary<string, FeatureConfig>
            {
                {
                    FeatureNames.ChangeDisplayHintToText,
                    new FeatureConfig()
                    {
                        DisplayCustomizationDetail = new List<DisplayCustomizationDetail>()
                        {
                            new DisplayCustomizationDetail() { UpdatePaymentMethodHeadingTypeToText = true }
                        }
                    }
                }
            };
            if (featureParams.FeatureConfigs != null)
            {
                foreach (string key in featureConfig.Keys)
                {
                    if (!featureParams.FeatureConfigs.ContainsKey(key))
                    {
                        featureParams.FeatureConfigs.Add(key, featureConfig[key]);
                    }
                }
            }
            else
            {
                featureParams.FeatureConfigs = featureConfig;
            }
        }

        private static void SetSecureFieldState(FeatureContext featureParams)
        {
            var featureConfig = new Dictionary<string, FeatureConfig>
            {
                {
                    FeatureNames.EnableSecureField,
                    new FeatureConfig()
                    {
                        DisplayCustomizationDetail = new List<DisplayCustomizationDetail>()
                        {
                            new DisplayCustomizationDetail() { EnableSecureFieldAddCC = true }
                        }
                    }
                }
            };
            if (featureParams.FeatureConfigs != null)
            {
                foreach (string key in featureConfig.Keys)
                {
                    if (!featureParams.FeatureConfigs.ContainsKey(key))
                    {
                        featureParams.FeatureConfigs.Add(key, featureConfig[key]);
                    }
                }
            }
            else
            {
                featureParams.FeatureConfigs = featureConfig;
            }
        }

        private static void SetDisplayCustomizationDetail(FeatureContext featureParams, DisplayCustomizationDetail displayCustomizationDetail, string featureName)
        {
            if (featureParams.FeatureConfigs == null)
            {
                featureParams.FeatureConfigs = new Dictionary<string, FeatureConfig>();
            }

            FeatureConfig featureConfig = null;
            if (featureParams.FeatureConfigs.TryGetValue(featureName, out featureConfig))
            {
                if (featureConfig.DisplayCustomizationDetail == null)
                {
                    featureConfig.DisplayCustomizationDetail = new List<DisplayCustomizationDetail>()
                    {
                        displayCustomizationDetail
                    };

                    featureParams.FeatureConfigs.Add(featureName, featureConfig);
                }
                else
                {
                    featureConfig.DisplayCustomizationDetail.Add(displayCustomizationDetail);
                }
            }
            else
            {
                featureParams.FeatureConfigs.Add(
                    featureName,
                    new FeatureConfig()
                    {
                        DisplayCustomizationDetail = new List<DisplayCustomizationDetail>()
                        {
                            displayCustomizationDetail
                        }
                    });
            }
        }

        private static void SetGroupedSelectOptionTextBeforeLogoState(FeatureContext featureParams, string featureName)
        {
            SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { SetGroupedSelectOptionTextBeforeLogo = true }, featureName);
        }

        private static void SetMatchSelectPMMainPageStructureForSubPage(FeatureContext featureParams, string featureName)
        {
            SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { MatchSelectPMMainPageStructureForSubPage = true }, featureName);
        }

        private static void SetSelectPMWithLogo(FeatureContext featureParams, string featureName)
        {
            SetDisplayCustomizationDetail(featureParams, new DisplayCustomizationDetail() { SetSelectPMWithLogo = true }, featureName);
        }

        private static bool IsTokenizationEncryptionEnabled(FeatureContext featureParams)
        {
            if (featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncryptionAddUpdateCC, StringComparer.OrdinalIgnoreCase)
                && string.Equals(featureParams.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase)
                && (string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            else if (featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncryptionOtherOperation, StringComparer.OrdinalIgnoreCase)
                && (string.Equals(featureParams.ResourceType, Constants.ChallengeDescriptionTypes.Cvv, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(featureParams.ResourceType, Constants.DescriptionTypes.ChallengeDescription, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.SearchTransactions, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsTokenizationEncryptionFetchConfigEnabled(FeatureContext featureParams)
        {
            if (string.Equals(featureParams.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase)
                && (featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncryptionFetchConfigAddUpdateCC, StringComparer.OrdinalIgnoreCase)
                    || featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncFetchConfigAddCCPiAuthKey, StringComparer.OrdinalIgnoreCase))
                && (string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            else if (featureParams.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncryptionFetchConfigOtherOperation, StringComparer.OrdinalIgnoreCase)
                && (string.Equals(featureParams.ResourceType, Constants.ChallengeDescriptionTypes.Cvv, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(featureParams.ResourceType, Constants.DescriptionTypes.ChallengeDescription, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(featureParams.OperationType, Constants.PidlOperationTypes.SearchTransactions, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static class FeatureNames
        {
            internal const string SetButtonActionToSuccessType = "setButtonActionToSuccessType";
            internal const string PaymentMethodGrouping = "paymentMethodGrouping";
            internal const string SetDefaultPaymentMethod = "setDefaultPaymentMethod";
            internal const string SkipSelectPM = "skipSelectpm";
            internal const string SkipSelectInstanceNoPI = "skipSelectInstanceNoPI";
            internal const string CustomizeDisplayContent = "customizeDisplayContent";
            internal const string CustomizeElementLocation = "customizeElementLocation";
            internal const string CustomizeProfileForm = "customizeProfileForm";
            internal const string CustomizeSubmitButtonContext = "customizeSubmitButtonContext";
            internal const string AddressValidation = "addressValidation";
            internal const string EnableElement = "enableElement";
            internal const string HideElement = "hideElement";
            internal const string SingleMarketDirective = "singleMarketDirective";
            internal const string GroupedPaymentOptionsTextBeforeLogo = "groupedPaymentOptionsTextBeforeLogo";
            internal const string UseTextForCVVHelpLink = "useTextForCVVHelpLink";
            internal const string ShowPIExpirationInformation = "showPIExpirationInformation";
            internal const string SplitListPIInformationIntoTwoLines = "splitListPIInformationIntoTwoLines";
            internal const string SwapLogoSource = "swapLogoSource";
            internal const string ChangeDisplayHintToText = "changeDisplayHintToText";
            internal const string RemoveElement = "removeElement";
            internal const string EnableSecureField = "enableSecureField";
            internal const string CustomizeAddressForm = "customizeAddressForm";
            internal const string CustomizeVerifyAddressDisplayContent = "customizeVerifyAddressDisplayContent";
            internal const string CustomizeDisplayTag = "customizeDisplayTag";
            internal const string UpdateStaticResourceServiceEndPoint = "UpdateStaticResourceServiceEndPoint";
            internal const string DisableElement = "disableElement";
            internal const string DpHideCountry = "dpHideCountry";
            internal const string CustomizeStructure = "customizeStructure";
            internal const string AddPMButtonWithPlusIcon = "addPMButtonWithPlusIcon";
            internal const string NoSubmitIfGSTIDEmpty = "noSubmitIfGSTIDEmpty";
            internal const string EnableConditionalFieldsForBillingAddress = "enableConditionalFieldsForBillingAddress";
            internal const string SkipJarvisAddressSyncToLegacy = "skipJarvisAddressSyncToLegacy";
            internal const string AddBillingAddressForWindows = "addBillingAddressForWindows";
            internal const string AddCCTwoPageForWindows = "addCCTwoPageForWindows";
            internal const string AddVenmoForWindows = "addVenmoForWindows";
            internal const string SelectPMButtonListStyleForWindows = "selectPMButtonListStyleForWindows";
            internal const string UpdateCCTwoPageForWindows = "updateCCTwoPageForWindows";
            internal const string UpdatePidlSubmitLink = "updatePidlSubmitLink";
            internal const string AddPayPalForWindows = "addPayPalForWindows";
            internal const string ListPIForWindows = "listPIForWindows";
            internal const string RemoveDataSource = "removeDataSource";
            internal const string UseIFrameForPiLogOn = "useIFrameForPiLogOn";
            internal const string EnableVirtualFamilyPM = "enableVirtualFamilyPM";
            internal const string UpdatePIaddressToAccount = "updatePIaddressToAccount";
            internal const string VerifyAddressStyling = "verifyAddressStyling";
            internal const string UseTextOnlyForPaymentOption = "useTextOnlyForPaymentOption";
            internal const string CvvChallengeForWindows = "cvvChallengeForWindows";
            internal const string ListAddressForWindows = "listAddressForWindows";
            internal const string RedeemGiftCard = "redeemGiftCard";
            internal const string AddStyleHintsToDisplayHints = "addStyleHintsToDisplayHints";
            internal const string AddRedeemGiftCardButton = "addRedeemGiftCardButton";
            internal const string UnhideElements = "unhideElements";
            internal const string UseAddressesExSubmit = "useAddressesExSubmit";
            internal const string EnablePlaceholder = "enablePlaceholder";
            internal const string GroupAddressFields = "groupAddressFields";
            internal const string EnableShortURL = "enableShortURL";
            internal const string UseLegacyAccountAndSync = "useLegacyAccountAndSync";
            internal const string CustomizeTaxIdForm = "customizeTaxIdForm";
            internal const string AddElement = "addElement";
            internal const string MoveSelectedPIToFirstOption = "moveSelectedPIToFirstOption";
            internal const string EnableUpdateCreditCardRegex = "enableUpdateCreditCardRegex";
            internal const string UpdateAddressline1Length = "updateAddressline1Length";

            // SelectSingleInstance features
            internal const string EnableSingleInstancePidls = "enableSingleInstancePidls";
            internal const string EnableSelectSingleInstancePiDisplay = "enableSelectSingleInstancePiDisplay";
            internal const string UseDisabledPIsForSelectInstance = "useDisabledPIsForSelectInstance";

            // Feature enables the use of profileType + prerequisitesV3 form instead of profileType + prerequisites while adding/updating pi
            internal const string UseProfilePrerequisitesV3 = "useProfilePrerequisitesV3";
            internal const string CustomizeActionContext = "customizeActionContext";
            internal const string AddDisplayCustomDetailsToButtonListOption = "addDisplayCustomDetailsToButtonListOption";
            internal const string EnableTokenizationEncryption = "enableTokenizationEncryption";
            internal const string EnableTokenizationEncryptionFetchConfig = "enableTokenizationEncryptionFetchConfig";
            internal const string DisableIndiaTokenization = "disableIndiaTokenization";
            internal const string SetIsSubmitGroupFalse = "setIsSubmitGroupFalse";

            // Edge Wallet Card related
            internal const string AddLocalCardFiltering = "addLocalCardFiltering";
            internal const string InlineLocalCardDetails = "inlineLocalCardDetails";
            internal const string IncludeCreditCardLogos = "includeCreditCardLogos";

            // Feature to pass the stylehints for xboxnative partners to style the form
            internal const string PXEnableXboxNativeStyleHints = "PXEnableXboxNativeStyleHints";

            // Changes style structure of expiry month and year. e.g. dropdown to text
            internal const string ChangeExpiryStyleToTextBox = "changeExpiryStyleToTextBox";

            // Removes the regex, mandatory, min/max length etc. validations for creditcard address fields
            internal const string RemoveAddressFieldsValidationForCC = "removeAddressFieldsValidationForCC";

            // Feature to enable delete payment instrument button for select instance PIDL
            internal const string EnableDeletePaymentInstrument = "enableDeletePaymentInstrument";

            // Changes expiry month and year. e.g. dropdown to single expiry date textbox
            internal const string ChangeExpiryMonthYearToExpiryDateTextBox = "ChangeExpiryMonthYearToExpiryDateTextBox";

            // Changes expiry month and year. e.g. dropdown to single expiry date textbox
            internal const string CombineExpiryMonthYearToDateTextBox = "combineExpiryMonthYearToDateTextBox";

            // Update the SEPA form to allow customization of the form fields
            internal const string CustomizeSEPAForm = "customizeSEPAForm";
        }
    }
}
