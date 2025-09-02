// <copyright file="CustomizeDisplayTag.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    internal class CustomizeDisplayTag : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                CustomizeElementDisplayTag
            };
        }

        internal static void CustomizeElementDisplayTag(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.CustomizeDisplayTag, out featureConfig);

            if (featureConfig?.DisplayCustomizationDetail?.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayHintCustomizationDetail.UpdateAccessibilityNameWithPosition != null && bool.Parse(displayHintCustomizationDetail?.UpdateAccessibilityNameWithPosition.ToString()))
                    {
                        UpdateButtonAccessibilityNameWithPosition(inputResources);
                    }

                    if (displayHintCustomizationDetail.UpdateXboxElementsAccessibilityHints != null && bool.Parse(displayHintCustomizationDetail?.UpdateXboxElementsAccessibilityHints.ToString()))
                    {
                        UpdateDisclaimerGroup(inputResources);
                    }

                    if (displayHintCustomizationDetail.DisplayTagsToBeRemoved != null)
                    {
                        RemoveDisplayTag(inputResources, displayHintCustomizationDetail.DisplayTagsToBeRemoved);
                    }

                    if (displayHintCustomizationDetail?.AddAccessibilityNameExpressionToNegativeValue == true && featureContext?.PaymentMethodType == Constants.PaymentMethodTypeNames.MSRewards)
                    {
                        AddOrUpdateElementDisplayTag(inputResources, Constants.DisplayHintIds.PointsRedemptionNegativeFormattedPointsValueTotalExpression, Constants.DisplayTagKeys.AccessibilityNameExpression, Constants.AccessibilityLabelExpressions.NegativeFormattedPointsValueTotal);
                        AddOrUpdateElementDisplayTag(inputResources, Constants.DisplayHintIds.PointsRedemptionNegativeFormattedPointsValueTotalAccentedExpression, Constants.DisplayTagKeys.AccessibilityNameExpression, Constants.AccessibilityLabelExpressions.NegativeFormattedPointsValueTotal);
                        AddOrUpdateElementDisplayTag(inputResources, Constants.DisplayHintIds.UseCsvNegativeFormattedCsvTotalExpression, Constants.DisplayTagKeys.AccessibilityNameExpression, Constants.AccessibilityLabelExpressions.NegativeFormattedCSVTotal);
                        AddOrUpdateElementDisplayTag(inputResources, Constants.DisplayHintIds.UseCsvNegativeFormattedCsvTotalAccentedExpression, Constants.DisplayTagKeys.AccessibilityNameExpression, Constants.AccessibilityLabelExpressions.NegativeFormattedCSVTotal);
                    }

                    if (displayHintCustomizationDetail?.DisplayAccentBorderWithGutterOnFocus == true)
                    {
                        SetAccentBorderWithGutterOnFocus(inputResources);
                    }

                    if (displayHintCustomizationDetail?.DisplayTagsToBeAdded != null)
                    {
                        foreach (var hintId in displayHintCustomizationDetail.DisplayTagsToBeAdded.Keys)
                        {
                            var tags = displayHintCustomizationDetail.DisplayTagsToBeAdded[hintId];
                            if (tags != null)
                            {
                                foreach (var tag in tags)
                                {
                                    var key = tag.Key;
                                    var value = tag.Value;
                                    AddOrUpdateElementDisplayTag(inputResources, hintId, key, value);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static void RemoveDisplayTag(List<PIDLResource> inputResources, IEnumerable<KeyValuePair<string, string>> displayTagsToBeRemoved)
        {
            foreach (PIDLResource resource in inputResources)
            {
                foreach (DisplayHint displayhint in resource?.GetAllDisplayHints())
                {
                    Dictionary<string, string> displayTagslist = displayhint?.DisplayTags;
                    
                    if (displayTagslist != null)
                    {
                        foreach (KeyValuePair<string, string> displayTagToBeRemoved in displayTagsToBeRemoved)
                        {
                            string displayTagKey = displayTagToBeRemoved.Key;
                            string displayTagExpectedValue = string.Empty;
                            if (displayTagslist.TryGetValue(displayTagKey, out displayTagExpectedValue))
                            {
                                if (string.Equals(displayTagExpectedValue, displayTagToBeRemoved.Value, StringComparison.OrdinalIgnoreCase))
                                {
                                    displayTagslist.Remove(displayTagKey);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static void UpdateButtonAccessibilityNameWithPosition(List<PIDLResource> inputResources)
        {
            foreach (PIDLResource resource in inputResources)
            {
                List<PageDisplayHint> displayPages = resource.DisplayPages;

                if (displayPages == null)
                {
                    continue;
                }

                string language = Context.Culture?.Name != null ? Context.Culture.Name : null;
                var buttonHashSet = new HashSet<ButtonDisplayHint>();

                foreach (PageDisplayHint displayHint in displayPages)
                {
                    List<ButtonDisplayHint> buttonList = PIDLResourceDisplayHintFactory.GetButtonDisplayHints(displayHint);
                    int totalButtonCount = buttonList.Count;

                    for (int buttonPosition = 0; buttonPosition < totalButtonCount; buttonPosition++)
                    {
                        ButtonDisplayHint buttonDisplayHint = buttonList.ElementAt(buttonPosition);
                        if (buttonHashSet.Contains(buttonDisplayHint))
                        {
                            continue;
                        }

                        buttonHashSet.Add(buttonDisplayHint);
                        Dictionary<string, string> displayTagslist = buttonDisplayHint?.DisplayTags;
                        if (displayTagslist?.Count > 0)
                        {
                            string accessibilityTagKey = Constants.DiplayHintProperties.AccessibilityName;
                            if (displayTagslist.ContainsKey(accessibilityTagKey))
                            {
                                string accessibilityName = displayTagslist[accessibilityTagKey];
                                string localizedString = LocalizationRepository.Instance.GetLocalizedString("{0} {1} of {2}", language);
                                localizedString = localizedString.Replace("{0}", accessibilityName);
                                localizedString = localizedString.Replace("{1}", (buttonPosition + 1).ToString());
                                localizedString = localizedString.Replace("{2}", totalButtonCount.ToString());
                                buttonDisplayHint.AddOrUpdateDisplayTag(accessibilityTagKey, localizedString);
                            }
                        }
                    }
                }
            }
        }

        internal static void UpdateDisclaimerGroup(List<PIDLResource> inputResources)
        {
            foreach (PIDLResource resource in inputResources)
            {
                if (resource?.DisplayPages == null)
                {
                    continue;
                }

                List<DisplayHint> displayHints = resource?.GetAllDisplayHintsOfId(Constants.DisplayHintIds.DisclaimerGroup);

                if (displayHints != null)
                {
                    foreach (DisplayHint hint in displayHints)
                    {
                        GroupDisplayHint disclaimerGroup = hint as GroupDisplayHint;

                        if (disclaimerGroup != null)
                        {
                            ButtonDisplayHint viewTermsButton = disclaimerGroup.Members?.Find(member => member?.HintId == Constants.DisplayHintIds.ViewTermsButton) as ButtonDisplayHint;
                            TextDisplayHint privacyStatementText = disclaimerGroup.Members?.Find(member => member?.HintId == Constants.DisplayHintIds.PrivacyStatementText) as TextDisplayHint;

                            if (viewTermsButton != null && privacyStatementText != null)
                            {
                                string language = Context.Culture?.Name != null ? Context.Culture.Name : null;
                                string localizedViewTermsString = LocalizationRepository.Instance.GetLocalizedString(Constants.AccessibilityLabels.ViewTems, language);

                                viewTermsButton.AddOrUpdateDisplayTag(Constants.DisplayTagKeys.AccessibilityName, localizedViewTermsString);
                                viewTermsButton.AddOrUpdateDisplayTag(Constants.DisplayTagKeys.AccessibilityHint, privacyStatementText.DisplayContent);
                                privacyStatementText.AddOrUpdateDisplayTag(Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Disable);
                            }
                        }
                    }
                }
            }
        }

        internal static void AddOrUpdateElementDisplayTag(List<PIDLResource> inputResources, string hintId, string accessibilityTagKey, string newAccessibilityTagValue)
        {
            foreach (PIDLResource resource in inputResources)
            {
                if (resource?.DisplayPages == null)
                {
                    continue;
                }

                List<DisplayHint> displayHints = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(resource, hintId);
                foreach (DisplayHint displayHint in displayHints)
                {
                    if (displayHint != null)
                    {
                        displayHint.AddOrUpdateDisplayTag(accessibilityTagKey, newAccessibilityTagValue);
                    }
                }
            }
        }

        internal static void SetAccentBorderWithGutterOnFocus(List<PIDLResource> inputResources)
        {
            if (inputResources == null)
            {
                return;
            }

            foreach (PIDLResource resource in inputResources)
            {
                if (resource == null)
                {
                    continue;
                }

                List<DisplayHint> displayHints = PIDLResourceDisplayHintFactory.GetAllDisplayHints(resource, true) ?? new List<DisplayHint>();
                foreach (DisplayHint displayHint in displayHints)
                {
                    ButtonDisplayHint buttonDisplayHint = displayHint as ButtonDisplayHint;
                    PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                    if (buttonDisplayHint != null)
                    {
                        buttonDisplayHint.AddOrUpdateDisplayTag(Constants.DisplayTagKeys.DisplayTagStyleHints, Constants.NativeDisplayTagValues.SelectionBorderGutterAccent);
                    }
                    else if (propertyDisplayHint != null)
                    {
                        string elementType = resource.GetElementTypeByPropertyDisplayHint(propertyDisplayHint);

                        if (elementType == Constants.ElementTypes.ButtonList)
                        {
                            Dictionary<string, SelectOptionDescription> possibleOptions = propertyDisplayHint.PossibleOptions ?? new Dictionary<string, SelectOptionDescription>();
                            foreach (var option in possibleOptions)
                            {
                                if (option.Value != null)
                                {
                                    option.Value.AddOrUpdateDisplayTag(Constants.DisplayTagKeys.DisplayTagStyleHints, Constants.NativeDisplayTagValues.SelectionBorderGutterAccent);
                                }
                            }
                        }
                        else if (elementType == Constants.ElementTypes.Dropdown || elementType == Constants.ElementTypes.Textbox)
                        {
                            propertyDisplayHint.AddOrUpdateDisplayTag(Constants.DisplayTagKeys.DisplayTagStyleHints, Constants.NativeDisplayTagValues.SelectionBorderGutterAccent);
                        }
                    }
                }
            }
        }
    }
}
