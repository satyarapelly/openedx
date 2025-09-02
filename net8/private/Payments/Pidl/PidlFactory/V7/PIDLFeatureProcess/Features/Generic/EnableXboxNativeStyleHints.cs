// <copyright file="EnableXboxNativeStyleHints.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    // The flight PXEnableXboxNativeStyleHints will be enabled for all the operations for xboxnative partners.
    // Few operations like SelectInstance (Select PI) needs to be handled specifically for xboxnative style hints.
    // Creating a generic feature which will pass the stylehints for the xboxnative based on the operation.
    internal class EnableXboxNativeStyleHints : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                PassStyleHintsForNative,
            };
        }

        internal static void PassStyleHintsForNative(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (featureContext == null || inputResources == null)
            {
                return;
            }

            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.PXEnableXboxNativeStyleHints, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayHintCustomizationDetail?.AddStyleHints == true && !string.IsNullOrEmpty(featureContext.OperationType) && !string.IsNullOrEmpty(featureContext.ResourceType) && string.Equals(featureContext.OperationType, Constants.PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase) && string.Equals(featureContext.ResourceType, Constants.ResourceTypes.PaymentMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessSelectPIForXboxNative(inputResources, featureContext);
                    }
                    else if (displayHintCustomizationDetail?.AddStyleHints == true && !string.IsNullOrEmpty(featureContext.OperationType) && !string.IsNullOrEmpty(featureContext.ResourceType) && string.Equals(featureContext.OperationType, Constants.PidlOperationTypes.ValidateInstance, StringComparison.OrdinalIgnoreCase) && string.Equals(featureContext.ResourceType, Constants.ResourceTypes.Address, StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessValidateAddressForXboxNative(inputResources, featureContext);
                    }

                    // Make sure every display hint has at least one style hint. If not, add a dummy style hint so the client will not use the scenario styles.
                    if (displayHintCustomizationDetail?.AddStyleHints == true)
                    {
                        AddDummyStyleHintToDisplayHints(inputResources, featureContext);
                    }

                    if (displayHintCustomizationDetail?.RemoveStyleHints == true)
                    {
                        RemoveStyleHints(inputResources, featureContext);
                    }
                }
            }
        }

        internal static void ProcessSelectPIForXboxNative(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            Dictionary<string, List<string>> styleHints = new Dictionary<string, List<string>>()
            {
                { Constants.DisplayHintIds.BackupPaymentInstrumentSelect, Constants.NativeStyleHints.SelectPIButtonListStyleHints },
                { Constants.DisplayHintIds.PaymentInstrumentSelect, Constants.NativeStyleHints.SelectPIButtonListStyleHints },
                { Constants.DisplayHintIds.PaymentInstrumentSelectHeading, new List<string>() { "margin-bottom-small" } },
                { Constants.DisplayHintIdPrefixes.PaymentOptionLogoType, new List<string>() { "image-small" } },
                { Constants.DisplayHintIdPrefixes.PaymentOptionContainer, new List<string>() { "width-fill", "height-fill" } },
                { Constants.DisplayHintIdPrefixes.PaymentOptionDisplayGroup, new List<string>() { "height-fill", "padding-top-small" } },
                { Constants.DisplayHintIdPrefixes.PaymentOptionExpiredInlineGroup, new List<string>() { "alignment-vertical-center" } },
                { string.Format("{0}-group", Constants.GroupDisplayHintIds.WarningIcon), new List<string>() { "alignment-vertical-center", "alignment-horizontal-center", "margin-start-small", "margin-end-small" } },
                { string.Format("{0}{1}", Constants.DisplayHintIdPrefixes.PaymentOptionContainer, Constants.DisplayHintIds.RedeemGiftCardLink), new List<string>() { "alignment-horizontal-center", "height-fill", "alignment-content-space-between" } },
                { string.Format("{0}{1}", Constants.DisplayHintIdPrefixes.PaymentOptionContainer, Constants.DisplayHintIds.NewPaymentMethodLink), new List<string>() { "alignment-horizontal-center", "height-fill", "alignment-content-space-between" } },
                { string.Format("{0}{1}", Constants.DisplayHintIdPrefixes.PaymentOptionLogo, Constants.DisplayHintIds.RedeemGiftCardLink), new List<string>() { "image-small-400" } },
                { string.Format("{0}{1}", Constants.DisplayHintIdPrefixes.PaymentOptionLogo, Constants.DisplayHintIds.NewPaymentMethodLink), new List<string>() { "image-small-400" } },
                { string.Format("{0}{1}", Constants.DisplayHintIdPrefixes.PaymentOptionDisplayGroup, Constants.DisplayHintIds.RedeemGiftCardLink), new List<string>() { "height-fill", "alignment-vertical-center" } },
                { string.Format("{0}{1}", Constants.DisplayHintIdPrefixes.PaymentOptionDisplayGroup, Constants.DisplayHintIds.NewPaymentMethodLink), new List<string>() { "height-fill", "alignment-vertical-center" } }
            };

            foreach (PIDLResource resource in inputResources)
            {
                if (resource?.DisplayPages == null)
                {
                    continue;
                }

                foreach (KeyValuePair<string, List<string>> styleHintMapper in styleHints)
                {
                    List<DisplayHint> displayHints = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(resource, styleHintMapper.Key);
                    if (displayHints != null && displayHints.Count > 0)
                    {
                        foreach (DisplayHint displayHint in displayHints)
                        {
                            displayHint.StyleHints = styleHintMapper.Value;
                        }
                    }
                }

                List<DisplayHint> backupPaymentInstruments = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(resource, Constants.DisplayHintIds.BackupPaymentInstrumentSelect) ?? new List<DisplayHint>();
                List<DisplayHint> paymentInstruments = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(resource, Constants.DisplayHintIds.PaymentInstrumentSelect) ?? new List<DisplayHint>();
                List<DisplayHint> optionTextGroups = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(resource, Constants.DisplayHintIdPrefixes.PaymentOptionTextGroup) ?? new List<DisplayHint>();
                List<DisplayHint> verticalTextGroups = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(resource, Constants.DisplayHintIdPrefixes.PaymentOptionDisplayGroup) ?? new List<DisplayHint>();
                List<DisplayHint> selectOptions = backupPaymentInstruments.Concat(paymentInstruments).ToList();

                // Pass stylehints for each of the select option and make all the text elements bold.
                foreach (DisplayHint displayHint in selectOptions)
                {
                    PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                    if (propertyDisplayHint != null && propertyDisplayHint.PossibleOptions != null)
                    {
                        var possibleOptions = propertyDisplayHint.PossibleOptions;

                        foreach (var option in possibleOptions)
                        {
                            if (option.Value != null)
                            {
                                option.Value.StyleHints = Constants.NativeStyleHints.SelectPIOptionStyleHints;

                                if (option.Value.DisplayContent != null)
                                {
                                    option.Value.DisplayContent.LayoutOrientation = Constants.PartnerHintsValues.VerticalPlacement;
                                    StyleSelectPIOptionTextElements(option.Value.DisplayContent);
                                }
                            }
                        }
                    }
                }

                foreach (DisplayHint optionTextGroup in optionTextGroups)
                {
                    FeatureHelper.ConvertToGroupDisplayHint(optionTextGroup as ContainerDisplayHint, Constants.PartnerHintsValues.VerticalPlacement);
                }

                // Re-arrange the elements such that top group contains all the elements except optionSelectedTextGroup, which will be a bottom group.
                foreach (DisplayHint displayHint in verticalTextGroups)
                {
                    GroupDisplayHint verticalTextGroup = displayHint as GroupDisplayHint;
                    if (verticalTextGroup?.Members != null)
                    {
                        int optionSelectedTextGroupIndex = verticalTextGroup.Members.FindIndex(hint => (hint?.HintId != null && hint.HintId.StartsWith(Constants.DisplayHintIdPrefixes.PaymentOptionSelectedTextGroup)));
                        if (optionSelectedTextGroupIndex != -1)
                        {
                            GroupDisplayHint optionSelectedTextGroup = verticalTextGroup.Members[optionSelectedTextGroupIndex] as GroupDisplayHint;
                            List<DisplayHint> verticalTextMembers = verticalTextGroup.Members.GetRange(0, optionSelectedTextGroupIndex);
                            verticalTextGroup.Members.RemoveRange(0, optionSelectedTextGroupIndex);

                            GroupDisplayHint verticalTextTopGroup = new GroupDisplayHint()
                            {
                                HintId = Constants.DisplayHintIds.VerticalTextTopGroup,
                                Members = verticalTextMembers,
                                LayoutOrientation = Constants.PartnerHintsValues.VerticalPlacement,
                                StyleHints = new List<string>() { "height-fill" }
                            };

                            verticalTextGroup.Members.Insert(0, verticalTextTopGroup);
                        }
                    }
                }
            }
        }

        internal static void ProcessValidateAddressForXboxNative(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            foreach (PIDLResource resource in inputResources ?? new List<PIDLResource>())
            {
                if (resource?.DisplayPages == null)
                {
                    continue;
                }

                // Having the try-catch block to not break the flow if there is any exception (like container is null, or no hint id for display hint) while getting the display hint.
                try
                {
                    DisplayHint addressValidationMessage = resource.GetDisplayHintById(Constants.SuggestAddressDisplayIds.AddressValidationMessage);
                    DisplayHint suggestedAddresses = resource.GetDisplayHintById(Constants.DisplayHintIds.SuggestedAddresses);

                    // The styling for the address validation message and suggested addresses button list is different for Add, Edit Address and Add, Edit CC flows.
                    // Changing the stylehints for these elements in Add, Edit CC flow where the operation is ValidateInstance. For Add, Edit Address the operation is Add.
                    if (addressValidationMessage != null)
                    {
                        addressValidationMessage.StyleHints = new List<string>() { "margin-bottom-small" };
                    }

                    if (suggestedAddresses != null)
                    {
                        suggestedAddresses.StyleHints = Constants.NativeStyleHints.AddPISuggestedAddressOptionsListStyleHints;
                    }
                }
                catch (Exception ex)
                {
                    SllWebLogger.TracePXServiceException(ex.ToString(), featureContext?.TraceActivityId ?? new EventTraceActivity());
                }
            }
        }

        internal static void AddDummyStyleHintToDisplayHints(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (inputResources == null)
            {
                return;
            }

            foreach (PIDLResource resource in inputResources)
            {
                // Having a try-catch block to not break the flow if there is any exception.
                try
                {
                    if (resource == null)
                    {
                        continue;
                    }

                    List<DisplayHint> displayHints = PIDLResourceDisplayHintFactory.GetAllDisplayHints(resource, true);
                    if (displayHints != null && displayHints.Count > 0)
                    {
                        foreach (DisplayHint displayHint in displayHints)
                        {
                            if (displayHint != null)
                            {
                                List<string> styleHints = displayHint.StyleHints?.ToList();
                                PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;
                                if (styleHints == null || styleHints.Count == 0)
                                {
                                    displayHint.StyleHints = Constants.NativeStyleHints.DummyStyleHint;
                                }

                                if (propertyDisplayHint != null && propertyDisplayHint.PossibleOptions != null)
                                {
                                    foreach (var option in propertyDisplayHint.PossibleOptions)
                                    {
                                        if (option.Value != null && option.Value.DisplayContent != null)
                                        {
                                            List<string> optionStyleHints = option.Value.StyleHints?.ToList();
                                            if (optionStyleHints == null || optionStyleHints.Count == 0)
                                            {
                                                option.Value.StyleHints = Constants.NativeStyleHints.DummyStyleHint;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SllWebLogger.TracePXServiceException(ex.ToString(), featureContext?.TraceActivityId ?? new EventTraceActivity());
                }
            }
        }

        internal static void RemoveStyleHints(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (inputResources == null)
            {
                return;
            }

            foreach (PIDLResource resource in inputResources)
            {
                // Having a try-catch block to not break the flow if there is any exception.
                try
                {
                    if (resource == null)
                    {
                        continue;
                    }

                    List<DisplayHint> displayHints = PIDLResourceDisplayHintFactory.GetAllDisplayHints(resource, true);
                    if (displayHints != null && displayHints.Count > 0)
                    {
                        foreach (DisplayHint displayHint in displayHints)
                        {
                            if (displayHint != null)
                            {
                                displayHint.StyleHints = null;
                                PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                                if (propertyDisplayHint != null && propertyDisplayHint.PossibleOptions != null)
                                {
                                    foreach (var option in propertyDisplayHint.PossibleOptions)
                                    {
                                        if (option.Value != null && option.Value.DisplayContent != null)
                                        {
                                            option.Value.StyleHints = null;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SllWebLogger.TracePXServiceException(ex.ToString(), featureContext?.TraceActivityId ?? new EventTraceActivity());
                }
            }
        }

        internal static void StyleSelectPIOptionTextElements(GroupDisplayHint groupDisplayHint)
        {
            if (groupDisplayHint == null || groupDisplayHint.Members == null)
            {
                return;
            }

            List<DisplayHint> displayHints = PIDLResourceDisplayHintFactory.GetAllDisplayHints(groupDisplayHint);
            if (displayHints != null && displayHints.Count > 0)
            {
                foreach (DisplayHint displayHint in displayHints)
                {
                    TextDisplayHint textDisplayHint = displayHint as TextDisplayHint;
                    if (textDisplayHint == null)
                    {
                        continue;
                    }

                    if (PIDLResourceDisplayHintFactory.IsDisplayHintMatch(textDisplayHint, $"{Constants.GroupDisplayHintIds.WarningIcon}-text"))
                    {
                        textDisplayHint.StyleHints = Constants.NativeStyleHints.WarningIcon;
                    }
                    else if (PIDLResourceDisplayHintFactory.IsDisplayHintMatch(textDisplayHint, Constants.DisplayHintIdPrefixes.PaymentOptionFontIcon))
                    {
                        textDisplayHint.StyleHints = Constants.NativeStyleHints.FontIcon;
                    }
                    else
                    {
                        textDisplayHint.StyleHints = Constants.NativeStyleHints.SmallBoldText;
                    }
                }
            }
        }
    }
}
