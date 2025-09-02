// <copyright file="CustomizeDisplayContent.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the CustomizeDisplayContent, which is to change display content of a PIDL element.
    /// </summary>
    internal class CustomizeDisplayContent : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ChangeDisplayContent
            };
        }

        internal static void ChangeDisplayContent(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.CustomizeDisplayContent, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    foreach (PIDLResource pidlResource in inputResources)
                    {
                        if (displayHintCustomizationDetail.SetSaveButtonDisplayContentAsNext)
                        {
                            if (featureContext?.PaymentMethodfamily != null && !string.Equals(featureContext?.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase))
                            {
                                return;
                            }

                            FeatureHelper.EditDisplayContent(pidlResource, Constants.ButtonDisplayHintIds.SaveButton, Constants.UnlocalizedDisplayText.NextButtonDisplayText);
                        }

                        if (displayHintCustomizationDetail.SetButtonDisplayContent != null)
                        {
                            var buttons = pidlResource.GetAllDisplayHints()?.OfType<ButtonDisplayHint>().ToList();

                            if (buttons != null)
                            {
                                foreach (var button in buttons)
                                {
                                    // Check if the button's HintId matches any key in SetButtonDisplayContent.
                                    if (displayHintCustomizationDetail.SetButtonDisplayContent.TryGetValue(button.HintId, out var displayContent))
                                    {
                                        FeatureHelper.EditDisplayContent(pidlResource, button.HintId, displayContent);
                                    }
                                }
                            }
                        }

                        if (displayHintCustomizationDetail.UpdateCvvChallengeTextForGCO)
                        {
                            FeatureHelper.EditDisplayContent(pidlResource, Constants.TextDisplayHintIds.CvvChallengeText, Constants.UnlocalizedDisplayText.PurchaseCvvChallengeText);
                        }

                        if (displayHintCustomizationDetail.SetBackButtonDisplayContentAsCancel)
                        {
                            FeatureHelper.EditDisplayContent(pidlResource, Constants.ButtonDisplayHintIds.CancelBackButton, Constants.UnlocalizedDisplayText.CancelButtonDisplayText);
                        }

                        if (displayHintCustomizationDetail.SetSaveButtonDisplayContentAsBook)
                        {
                            FeatureHelper.EditDisplayContent(pidlResource, Constants.ButtonDisplayHintIds.SaveButton, Constants.UnlocalizedDisplayText.BookButtonDisplayText);
                        }

                        if (displayHintCustomizationDetail.AddressSuggestionMessage)
                        {
                            RemoveDisplayContent(pidlResource, Constants.TextDisplayHintIds.AddressSuggestionMessage);
                            EditTextDisplayHint(pidlResource, Constants.DisplayHintIds.SuggestedAddressText, Constants.SuggestedAddressesStaticText.SuggestHeaderWithColon);
                        }

                        if (displayHintCustomizationDetail?.AddAllFieldsRequiredText == true)
                        {
                            AddAllFieldsRequiredText(pidlResource, featureContext);
                        }
                      
                        if (displayHintCustomizationDetail?.AddAsteriskToAllMandatoryFields == true)
                        {
                            AddAsteriskToAllMandatoryFields(pidlResource, featureContext);
                        }

                        if (displayHintCustomizationDetail?.UpdateSelectPiButtonText != null && bool.Parse(displayHintCustomizationDetail?.UpdateSelectPiButtonText.ToString()))
                        {
                            FeatureHelper.EditDisplayContent(pidlResource, Constants.DisplayHintIds.NewPaymentMethodLink, Constants.StandardizedDisplayText.AddUpdatePaymentMethod);
                        }

                        if (displayHintCustomizationDetail?.UpdatePrefillCheckboxText == true)
                        {
                            // Use partner-provided text if available, otherwise fall back to the default
                            var prefillText = !string.IsNullOrEmpty(displayHintCustomizationDetail.PrefillCheckboxText)
                                ? displayHintCustomizationDetail.PrefillCheckboxText
                                : Constants.UnlocalizedDisplayText.PrefillCheckboxText;

                            FeatureHelper.EditDisplayContent(pidlResource, Constants.DisplayHintIds.PrefillBillingAddressCheckbox, prefillText);
                        }
                    }
                }
            }
        }

        private static void RemoveDisplayContent(PIDLResource pidlResource, string hintId)
        {
            pidlResource.RemoveDisplayHintById(hintId);
        }

        private static void EditTextDisplayHint(PIDLResource pidlResource, string hintId, string unlocalizedNewDisplayString)
        {
            TextDisplayHint textDisplayHint = pidlResource.GetDisplayHintById(hintId) as TextDisplayHint;
            if (textDisplayHint != null)
            {
                textDisplayHint.DisplayContent = PidlModelHelper.GetLocalizedString(unlocalizedNewDisplayString);
            }
        }

        /// <summary>
        /// This method adds an asterisk to all mandatory fields in the PIDL resource.
        /// </summary>
        /// <param name="pidlResource">The PIDL resource whose mandatory fields will be updated to include an asterisk.</param>
        /// <param name="featureContext">The feature context containing configuration details for the operation.</param>
        private static void AddAsteriskToAllMandatoryFields(PIDLResource pidlResource, FeatureContext featureContext)
        {
            var allHints = pidlResource?.GetAllDisplayHints()?.OfType<PropertyDisplayHint>().ToList();
            
            if (allHints != null && allHints.Any())
            {
                foreach (var displayHint in allHints)
                {
                    var propertyValue = pidlResource.GetPropertyDescriptionByPropertyName(displayHint.PropertyName);
                    
                    if (propertyValue != null && propertyValue.IsOptional != true)
                    {
                        // Prioritize DisplayName, fallback to DisplayDescription
                        if (!string.IsNullOrEmpty(displayHint.DisplayName))
                        {
                            displayHint.DisplayName = FeatureHelper.AppendAsteriskToText(displayHint.DisplayName, displayHint.DisplayTags);
                        }
                        else if (!string.IsNullOrEmpty(displayHint.DisplayDescription))
                        {
                            displayHint.DisplayDescription = FeatureHelper.AppendAsteriskToText(displayHint.DisplayDescription, displayHint.DisplayTags);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a mandatory fields message to the PIDL resource if it does not already exist.
        /// The method checks for the presence of specific display hints and inserts the mandatory fields message
        /// at the appropriate position in the display page members.
        /// </summary>
        /// <param name="pidlResource">The PIDL resource to which the mandatory fields message will be added.</param>
        /// <param name="featureContext">The feature context containing language and other configuration details.</param>
        private static void AddAllFieldsRequiredText(PIDLResource pidlResource, FeatureContext featureContext)
        {
            if (pidlResource?.DisplayPages != null)
            {
                foreach (PageDisplayHint pidlDisplayPage in pidlResource.DisplayPages)
                {
                    PropertyDisplayHint propertyNameValue = FindPropertyDisplayHintByDisplayHintType(pidlDisplayPage?.Members, Constants.DisplayHintTypes.Property);

                    // Check if the property is not disabled, hidden, and not a button list, radio, or dropdown
                    if (propertyNameValue != null && !(string.Equals(propertyNameValue.SelectType, Constants.PaymentMethodSelectType.ButtonList, StringComparison.OrdinalIgnoreCase)
                             || string.Equals(propertyNameValue.SelectType, Constants.PaymentMethodSelectType.Radio, StringComparison.OrdinalIgnoreCase)
                             || string.Equals(propertyNameValue.SelectType, Constants.PaymentMethodSelectType.DropDown, StringComparison.OrdinalIgnoreCase)))
                    {
                        TextGroupDisplayHint starTextGroupDisplayHint = pidlDisplayPage?.Members.Find(displayHint => displayHint.HintId.Equals(Constants.DisplayHintIds.StarRequiredTextGroup, StringComparison.OrdinalIgnoreCase)) as TextGroupDisplayHint;

                        // Find the index of the first element with SelectType "logo"
                        int index = pidlDisplayPage?.Members.FindIndex(member => member?.DisplayHintType == Constants.DisplayHintTypes.Logo) ?? -1;

                        TextDisplayHint mandatoryFieldsMessageHint = pidlDisplayPage?.Members.Find(displayHint => displayHint.HintId.Equals(Constants.DisplayHintIds.MandatoryFieldsMessage, StringComparison.OrdinalIgnoreCase)) as TextDisplayHint;

                        TextDisplayHint mandatoryFieldsMessageHintNew = new TextDisplayHint
                        {
                            HintId = Constants.DisplayHintIds.MandatoryFieldsMessage,
                            DisplayContent = PidlModelHelper.GetLocalizedString(Constants.DisplayNames.AllMandatoryFieldsText, featureContext.Language)
                        };

                        if (starTextGroupDisplayHint != null && mandatoryFieldsMessageHint == null)
                        {
                            var starTextGroupDisplayHintIndex = pidlDisplayPage.Members.IndexOf(starTextGroupDisplayHint);

                            pidlDisplayPage.Members.RemoveAt(starTextGroupDisplayHintIndex);
                            pidlDisplayPage.Members.Insert(starTextGroupDisplayHintIndex, mandatoryFieldsMessageHintNew);
                        }
                        else if (pidlDisplayPage != null && mandatoryFieldsMessageHint == null && starTextGroupDisplayHint == null)
                        {
                            if (index > 0)
                            {
                                pidlDisplayPage.Members.Insert(++index, mandatoryFieldsMessageHintNew);
                            }
                            else
                            {
                                // These conditions handle cases where the PIDL does not have a logo (e.g. update pi credit card flow).
                                // If a heading is present at the top, the mandatory text is added after it; otherwise, it is added at the top of the page.
                                int indexValue = (pidlDisplayPage.Members.Count > 0 && pidlDisplayPage.Members[0]?.DisplayHintType == Constants.DisplayHintTypes.Heading) ? 1 : 0;
                                pidlDisplayPage.Members.Insert(indexValue, mandatoryFieldsMessageHintNew);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method will find the first PropertyDisplayHint with the given display hint type in the list of display hints.
        /// </summary>
        /// <param name="members">The list of display hints to search through.</param>
        /// <param name="displayHintTypeName">The display hint type name to search for.</param>
        /// <returns>The first PropertyDisplayHint that matches the given display hint type, or null if no match is found.</returns>
        private static PropertyDisplayHint FindPropertyDisplayHintByDisplayHintType(List<DisplayHint> members, string displayHintTypeName)
        {
            foreach (var member in members)
            {
                // Explicitly cast and check for null instead of using "is" pattern matching
                PropertyDisplayHint propertyDisplayHint = member as PropertyDisplayHint;
                if (propertyDisplayHint != null &&
                    string.Equals(propertyDisplayHint.DisplayHintType, displayHintTypeName, StringComparison.OrdinalIgnoreCase) &&
                    !(propertyDisplayHint.IsDisabled ?? false) &&
                    !(propertyDisplayHint.IsHidden ?? false))
                {
                    return propertyDisplayHint;
                }

                // Explicitly cast and check for GroupDisplayHint
                GroupDisplayHint groupDisplayHint = member as GroupDisplayHint;
                if (groupDisplayHint != null)
                {
                    // The foundHint can be of either GroupDisplayHint or PropertyDisplayHint type.
                    var foundHint = FindPropertyDisplayHintByDisplayHintType(groupDisplayHint.Members, displayHintTypeName);
                    if (foundHint != null)
                    {
                        return foundHint;
                    }
                }
            }

            return null;
        }
    }
}