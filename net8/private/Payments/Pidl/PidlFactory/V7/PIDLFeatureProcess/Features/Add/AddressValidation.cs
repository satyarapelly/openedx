// <copyright file="AddressValidation.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the AddressValidation feature, which is to show avs for add PI/address form.
    /// </summary>
    internal class AddressValidation : IFeature
    {
        private static readonly List<string> enabledAddressTypesForValidation = new List<string>
        {
            Constants.AddressTypes.HapiV1SoldToIndividual,
            Constants.AddressTypes.HapiV1SoldToOrganization,
            Constants.AddressTypes.HapiV1BillToIndividual,
            Constants.AddressTypes.HapiV1BillToOrganization,
            Constants.AddressTypes.HapiV1,
            Constants.AddressTypes.OrgAddress,
            Constants.AddressTypes.HapiServiceUsageAddress,
            Constants.AddressTypes.Billing,
            Constants.AddressTypes.ShippingV3
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                AddModernAVSValidationAction
            };
        }

        internal static void AddModernAVSValidationAction(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            PartnerSettingsModel.FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.AddressValidation, out featureConfig);
            string resourceType = featureContext.ResourceType;
            bool isAddOrUpdateOperation = string.Equals(featureContext?.OperationType, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase) || string.Equals(featureContext?.OperationType, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase);

            switch (resourceType)
            {                                                                                                                                                                                                                                                                                                               
                case Constants.ResourceTypes.PaymentMethod:
                    if ((string.Equals(featureContext?.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard.ToString(), StringComparison.OrdinalIgnoreCase) 
                            || Constants.IsVirtualLegacyInvoice(featureContext?.PaymentMethodfamily, featureContext?.PaymentMethodType)
                            || string.Equals(featureContext?.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.DirectDebit, StringComparison.OrdinalIgnoreCase))
                        && isAddOrUpdateOperation)
                    {
                        // Address field is named as "address" in credit card PIDL data description, and as "default_address" in profile PIDL data description (linked PIDL).
                        const string AddressPropertyName = "address";
                        const string DefaultAddressPropertyName = "default_address";
                        foreach (PIDLResource paymentMethodPidl in inputResources)
                        {
                            // Add AVS validation endpoint only if it has address property name
                            if (paymentMethodPidl != null)
                            {
                                var propertyName = paymentMethodPidl.HasDataDescriptionWithKey(AddressPropertyName) ? AddressPropertyName : (paymentMethodPidl.HasDataDescriptionWithKey(DefaultAddressPropertyName) ? DefaultAddressPropertyName : string.Empty);
                                if (!string.IsNullOrEmpty(propertyName))
                                {
                                    AddModernValidationActionForButton(GetButtonToAppendModernValidationAction(paymentMethodPidl), propertyName, Constants.AddressTypes.Internal, featureContext?.OriginalPartner, featureContext?.Language, featureContext?.Country, featureContext, featureConfig);

                                    if (paymentMethodPidl.LinkedPidls != null)
                                    {
                                        EnableAVSAdditionalFlags(paymentMethodPidl, true, Constants.DataDescriptionIds.DefaultAddress);
                                    }
                                }
                            }
                        }
                    }

                    break;
                case Constants.ResourceTypes.Address:
                    if (enabledAddressTypesForValidation.Contains(featureContext.OriginalTypeName ?? featureContext.TypeName, StringComparer.OrdinalIgnoreCase)
                        && isAddOrUpdateOperation)
                    {
                        foreach (PIDLResource addressPIdl in inputResources)
                        {
                            if (addressPIdl != null && addressPIdl.DisplayPages != null)
                            {
                                DisplayHint buttonDisplayHint = GetButtonToAppendModernValidationAction(addressPIdl);

                                if (string.Equals(featureContext.TypeName, Constants.AddressTypes.OrgAddress, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(featureContext.TypeName, Constants.AddressTypes.HapiServiceUsageAddress, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(featureContext.TypeName, Constants.AddressTypes.Billing, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(featureContext.TypeName, Constants.AddressTypes.ShippingV3, StringComparison.OrdinalIgnoreCase))
                                {
                                    AddModernValidationActionForButton(buttonDisplayHint, string.Empty, featureContext.TypeName, featureContext?.Partner, featureContext?.Language, featureContext?.Country, featureContext);
                                }
                                else
                                {
                                    ReplaceLegacyValidationActionWithModernValidationActionForButton(buttonDisplayHint, "address", featureContext.TypeName, featureContext?.Partner, featureContext?.Language, featureContext?.Country, featureContext);
                                }

                                EnableAVSAdditionalFlags(addressPIdl, false, Constants.DataDescriptionIds.Address);
                            }
                        }
                    }
                    else if (string.Equals(featureContext.OperationType, Constants.PidlOperationTypes.ValidateInstance, StringComparison.OrdinalIgnoreCase)
                             && string.Equals(featureContext.Scenario, Constants.ScenarioNames.SuggestAddressesTradeAVS, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (PIDLResource profilePidl in inputResources)
                        {
                            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
                            {
                                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                                {
                                    if (displayHintCustomizationDetail?.AddCCAddressValidationPidlModification != null && TryParseDisplayDetailCustomization(displayHintCustomizationDetail))
                                    {
                                        ModifyAddCCAdressValidationPIDL(inputResources, featureContext);
                                    }
                                }
                            }
                        }
                    }
                    else if (string.Equals(featureContext.OperationType, Constants.PidlOperationTypes.ValidateInstance, StringComparison.OrdinalIgnoreCase))
                    {
                        if (featureConfig?.DisplayCustomizationDetail?.Count > 0)
                        {
                            foreach (var displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                            {
                                if (displayHintCustomizationDetail?.VerifyAddressPidlModification ?? false)
                                {
                                    foreach (var profilePidl in inputResources)
                                    {
                                        ModifyVerifyAdressPIDL(inputResources, featureContext);
                                    }
                                }
                            }
                        }
                    }
                    else if (string.Equals(featureContext.TypeName, Constants.AddressTypes.PXV3Billing, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (PIDLResource addressPidl in inputResources)
                        {
                            if (addressPidl != null)
                            {
                                AddModernValidationActionForButton(GetButtonToAppendModernValidationAction(addressPidl), Constants.DescriptionTypes.AddressBillingV3, Constants.AddressTypes.Internal, featureContext?.Partner, featureContext?.Language, featureContext?.Country, featureContext, featureConfig);
                                EnableAVSAdditionalFlags(addressPidl, false, Constants.DataDescriptionIds.DefaultAddress);
                            }
                        }
                    }

                    break;
                case Constants.ResourceTypes.Profile:
                    foreach (PIDLResource profilePidl in inputResources)
                    {
                        if (profilePidl != null)
                        {
                            var propertyName = profilePidl.HasDataDescriptionWithKey(Constants.DataDescriptionIds.DefaultAddress) ? Constants.DataDescriptionIds.DefaultAddress : string.Empty;

                            if (!string.IsNullOrEmpty(propertyName))
                            {
                                AddModernValidationActionForButton(GetButtonToAppendModernValidationAction(profilePidl), propertyName, Constants.AddressTypes.Internal, featureContext?.Partner, featureContext?.Language, featureContext?.Country, featureContext);
                                EnableAVSAdditionalFlags(profilePidl, false, Constants.DataDescriptionIds.DefaultAddress);
                            }
                        }
                    }

                    break;
                case Constants.ResourceTypes.TaxId:
                case Constants.ResourceTypes.PaymentInstrument:
                    break;
                default:
                    throw TraceCore.TraceException<InvalidOperationException>(new InvalidOperationException(string.Format("The resource type '{0}' is not supported for address validation", resourceType)));
            }
        }

        private static bool TryParseDisplayDetailCustomization(DisplayCustomizationDetail displayHintCustomizationDetail)
        {
            if (bool.TryParse(displayHintCustomizationDetail?.AddCCAddressValidationPidlModification.ToString(), out bool hasDisplayCustomizationHint))
            {
                return hasDisplayCustomizationHint;
            }

            return false; 
        }

        /// <summary>
        /// Gets the button to append modern validation action.
        /// </summary>
        /// <param name="pidl">Pidl resource</param>
        /// <returns>Returns button display hint if found from list of buttons or else null.</returns>
        private static DisplayHint GetButtonToAppendModernValidationAction(PIDLResource pidl)
        {
            DisplayHint buttonDisplayHint;

            List<string> buttonDisplayHintNames = new List<string>
            {
                Constants.ButtonDisplayHintIds.NextModernValidateButton,
                Constants.ButtonDisplayHintIds.SaveButton,
                Constants.ButtonDisplayHintIds.SaveNextButton,
                Constants.ButtonDisplayHintIds.ValidateButtonHidden,
                Constants.ButtonDisplayHintIds.SubmitButtonHidden,
                Constants.ButtonDisplayHintIds.ValidateThenSubmitButtonHidden,
                Constants.ButtonDisplayHintIds.ValidateThenSubmitButton
            };

            foreach (string buttonDisplayHintName in buttonDisplayHintNames)
            {
                buttonDisplayHint = pidl.GetDisplayHintById(buttonDisplayHintName);
                if (buttonDisplayHint != null)
                {
                    return buttonDisplayHint;
                }
            }

            return null;
        }

        private static DisplayHintAction BuildModernValidateAction(string propertyName, string type, string partner, string language, string country, FeatureContext featureContext, PartnerSettingsModel.FeatureConfig featureConfig = null)
        {
            var validateLink = new PXCommon.RestLink();
            validateLink.Method = "POST";
            validateLink.SetErrorCodeExpressions(new[] { "({contextData.innererror.code})", "({contextData.code})" });
            if (!string.IsNullOrEmpty(propertyName))
            {
                validateLink.PropertyName = propertyName;
            }

            string modernAVSForTradeScenario = Constants.ScenarioNames.SuggestAddressesTradeAVSUsePidlPageV2;

            if (featureContext.ExposedFlightFeatures != null && featureContext.ExposedFlightFeatures.Contains(PXCommon.Flighting.Features.TradeAVSUsePidlModalInsteadofPidlPage, StringComparer.OrdinalIgnoreCase))
            {
                modernAVSForTradeScenario = Constants.ScenarioNames.SuggestAddressesTradeAVSUsePidlModal;
            }

            validateLink.Href = string.Format(Constants.SubmitUrls.PifdAnonymousModernAVSForTrade, type, partner, language, modernAVSForTradeScenario, country);

            //// Adding the PXUsePartnerSettingsService flight directly to the ModernValidate call given the available displayCustomizationDetail.
            //// This flight is not automatically passed through from Azure Exp so needs to be manually added here.
            if (featureConfig?.DisplayCustomizationDetail?.Count > 0)
            {
                foreach (var displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayHintCustomizationDetail?.VerifyAddressPidlModification ?? false)
                    {
                        validateLink.AddHeader(Constants.CustomHeaders.MSFlight, Constants.PartnerFlightValues.PXUsePartnerSettingsService);
                    }
                }
            }

            return new DisplayHintAction(DisplayHintActionType.validate.ToString(), true, validateLink, null);
        }

        private static void AddModernValidationActionForButton(DisplayHint displayHintToAddModernValidationAction, string propertyName, string type, string partner, string language, string country, FeatureContext featureContext, PartnerSettingsModel.FeatureConfig featureConfig = null)
        {
            if (displayHintToAddModernValidationAction != null)
            {
                DisplayHintAction originalPidlAction = displayHintToAddModernValidationAction.Action;
                DisplayHintAction modernValidateAction = BuildModernValidateAction(propertyName, type, partner, language, country, featureContext, featureConfig);
                modernValidateAction.NextAction = originalPidlAction;
                displayHintToAddModernValidationAction.Action = modernValidateAction;
            }
        }

        private static void ReplaceLegacyValidationActionWithModernValidationActionForButton(DisplayHint displayHintToAddModernValidationAction, string propertyName, string type, string partner, string language, string country, FeatureContext featureContext)
        {
            if (displayHintToAddModernValidationAction != null)
            {
                DisplayHintAction originalNextPidlAction = displayHintToAddModernValidationAction.Action.NextAction;
                DisplayHintAction modernValidateAction = BuildModernValidateAction(propertyName, type, partner, language, country, featureContext);
                modernValidateAction.NextAction = originalNextPidlAction;
                displayHintToAddModernValidationAction.Action = modernValidateAction;
            }
        }

        private static void EnableAVSAdditionalFlags(PIDLResource pidlResources, bool inLinkedPidl, string path)
        {
            AddHiddenCheckBoxElement(
                pidlResources,
                Constants.CommercialZipPlusFourPropertyNames.IsUserConsented,
                path,
                inlinkedPidl: inLinkedPidl);
            AddHiddenCheckBoxElement(
                pidlResources,
                Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded,
                path,
                inlinkedPidl: inLinkedPidl);
        }

        private static void AddHiddenCheckBoxElement(
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

        private static string BuildHiddenCheckboxHintId(string propertyName)
        {
            return $"HiddenCheckbox_{propertyName}";
        }

        private static void AddHiddenCheckBoxElement(PIDLResource pidl, string propertyName, string path)
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

        private static void ModifyAddCCAdressValidationPIDL(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            foreach (var pidl in inputResources)
            {
                ButtonDisplayHint editButton = pidl.GetDisplayHintById(Constants.ButtonDisplayHintIds.AddressChangeTradeAVSV2Button) as ButtonDisplayHint
                                                           ?? pidl.GetDisplayHintById(Constants.ButtonDisplayHintIds.AddressChangeButton) as ButtonDisplayHint;
                if (editButton != null)
                {
                    editButton.DisplayHintType = Constants.DisplayHintTypes.Hyperlink;
                }

                ButtonDisplayHint nextButton = pidl.GetDisplayHintById(Constants.ButtonDisplayHintIds.AddressNextButton) as ButtonDisplayHint;

                if (nextButton != null)
                {
                    nextButton.DisplayContent = PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesStaticText.UseThisAddress, featureContext.Language);
                    nextButton.AddOrUpdateDisplayTag(Constants.DiplayHintProperties.AccessibilityName, PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesStaticText.UseThisAddress));
                }

                TextDisplayHint addressRecommendationMessage = pidl.GetDisplayHintById(Constants.TextDisplayHintIds.AddressRecommandationMessage) as TextDisplayHint;

                if (addressRecommendationMessage != null)
                {
                    addressRecommendationMessage.DisplayContent = PidlModelHelper.GetLocalizedString(Constants.SuggestedAddressesStaticText.WhichAddressSuggestionMessage, featureContext.Language);
                }

                GroupDisplayHint addressOptionsTradeAVSV2Group = pidl.GetDisplayHintById(Constants.DisplayHintIds.AddressOptionsTradeAVSV2Group) as GroupDisplayHint;
                if (addressOptionsTradeAVSV2Group != null)
                {
                    addressOptionsTradeAVSV2Group.LayoutOrientation = null;
                }
            }
        }

        private static void ModifyVerifyAdressPIDL(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            foreach (var pidl in inputResources)
            {
                // Makes the edit button a link.
                ButtonDisplayHint editButton = pidl.GetDisplayHintById(Constants.ButtonDisplayHintIds.AddressChangeButton) as ButtonDisplayHint;
                if (editButton != null)
                {
                    editButton.DisplayHintType = Constants.DisplayHintTypes.Hyperlink;
                }

                // Wraps the privacy notice with a inline group for proper rendering.
                if (pidl.DisplayPages != null)
                {
                    foreach (var page in pidl.DisplayPages)
                    {
                        var textGroupIndex = page.Members.FindIndex(m => m.HintId == Constants.DisplayHintIds.MicrosoftPrivacyTextGroup);
                        if (textGroupIndex != -1)
                        {
                            var textGroup = page.Members[textGroupIndex] as TextGroupDisplayHint;
                            if (textGroup != null)
                            {
                                var inlineGroup = new GroupDisplayHint()
                                {
                                    HintId = textGroup.HintId,
                                    LayoutOrientation = "inline",
                                    StyleHints = textGroup.StyleHints,
                                    DisplayTags = textGroup.DisplayTags,
                                    Members = textGroup.Members,
                                };

                                page.Members[textGroupIndex] = inlineGroup;
                            }
                        }
                    }
                }

                // Makes the address suggestions to show on the same column as address entered.
                var addressOptionsGroup = pidl.GetDisplayHintById(Constants.GroupDisplayHintIds.AddressOptionsGroup) as GroupDisplayHint;
                if (addressOptionsGroup != null)
                {
                    addressOptionsGroup.LayoutOrientation = null;
                }
            }
        }
    }
}