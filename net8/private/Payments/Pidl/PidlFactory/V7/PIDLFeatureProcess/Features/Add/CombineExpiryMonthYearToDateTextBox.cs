// <copyright file="CombineExpiryMonthYearToDateTextBox.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the Expiry Date TextBox - Enable feature, which is to change style structure of expiry month and year from dropdown to one text box for month and year.
    /// </summary>
    internal class CombineExpiryMonthYearToDateTextBox : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                UpdateExpiryMonthYearToExpiryDateTextBox
            };
        }

        internal static void UpdateExpiryMonthYearToExpiryDateTextBox(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (inputResources != null && featureContext != null)
            {
                featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.CombineExpiryMonthYearToDateTextBox, out FeatureConfig featureConfig);

                if (featureConfig != null && string.Equals(featureContext.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase)
                && (string.Equals(featureContext.OperationType, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(featureContext.OperationType, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase)))
                {
                    foreach (PIDLResource pidlResource in inputResources)
                    {
                        ContainerDisplayHint expiryGroup = pidlResource.GetPidlContainerDisplayHintbyDisplayId(Constants.ExpiryPrefixes.ExpiryGroup);
                        ContainerDisplayHint cvvGroup = pidlResource.GetPidlContainerDisplayHintbyDisplayId(Constants.ExpiryPrefixes.CvvGroup);
                        PropertyDisplayHint expiryMonth = pidlResource.GetDisplayHintById(Constants.ExpiryPrefixes.ExpiryMonth) as PropertyDisplayHint;
                        PropertyDisplayHint expiryYear = pidlResource.GetDisplayHintById(Constants.ExpiryPrefixes.ExpiryYear) as PropertyDisplayHint;

                        PropertyDescription expiryMonthDataDescription = pidlResource.GetPropertyDescriptionByPropertyName(Constants.ExpiryPrefixes.ExpiryMonth);
                        PropertyDescription expiryYearDataDescription = pidlResource.GetPropertyDescriptionByPropertyName(Constants.ExpiryPrefixes.ExpiryYear);

                        if (expiryGroup != null && expiryMonth != null && expiryYear != null
                            && expiryMonthDataDescription != null && expiryYearDataDescription != null)
                        {
                            expiryMonth.IsHidden = true;
                            expiryMonthDataDescription.IsOptional = true;
                            expiryMonthDataDescription.PropertyType = PXCommon.Constants.DataDescriptionPropertyType.ClientData;

                            expiryYear.IsHidden = true;
                            expiryYearDataDescription.PropertyType = PXCommon.Constants.DataDescriptionPropertyType.ClientData;
                            expiryYearDataDescription.IsOptional = true;

                            var propertyValidation = new PropertyValidation()
                            {
                                ValidationType = Constants.ValidationTypes.Regex,
                                Regex = Constants.ValidationRegex.ExpiryDate,
                                ErrorCode = Constants.ExpiryDateErrorMessages.ExpiryDateInvalidCode,
                                ErrorMessage = PidlModelHelper.GetLocalizedString(Constants.ExpiryDateErrorMessages.ExpiryDateFormatMessage)
                            };

                            var expiryDateDataDescription = GetExpiryDateDataDescription(propertyValidation);

                            var details = pidlResource.GetTargetDataDescription(Constants.DataDescriptionIds.Details);
                            if (details != null)
                            {
                                details.Add(Constants.ExpiryPrefixes.ExpiryDate, expiryDateDataDescription);
                                PropertyDisplayHint expiryDate = CreateExpiryDateDisplayHint();

                                expiryGroup?.Members?.Add(expiryDate);

                                if (cvvGroup != null)
                                {
                                    var expiryCvvGroupHint = new GroupDisplayHint()
                                    {
                                        HintId = string.Format("{0}_{1}", expiryGroup.HintId, cvvGroup?.HintId),
                                        LayoutOrientation = Constants.LayoutOrientations.Inline,
                                        Members = new List<DisplayHint>()
                                        {
                                            expiryGroup,
                                            cvvGroup
                                        }
                                    };

                                    pidlResource.DisplayPages?.ForEach(displayPage =>
                                    {
                                        var expiryGroupIndex = displayPage.Members?.FindIndex(hid => hid.HintId == expiryGroup.HintId);
                                        if (expiryGroupIndex.HasValue && expiryGroupIndex.Value >= 0)
                                        {
                                            displayPage.Members?.Remove(expiryGroup);
                                            displayPage.Members?.Remove(cvvGroup);
                                            displayPage.Members?.Insert(expiryGroupIndex.Value, expiryCvvGroupHint);
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        private static PropertyDisplayErrorMessageMap CreateExpiryDateErrorMessage()
        {
            return new PropertyDisplayErrorMessageMap()
            {
                DefaultErrorMessage = PidlModelHelper.GetLocalizedString(Constants.ExpiryDateErrorMessages.ExpiryDateRequiredMessage),
                ErrorCodeMessages = new List<PropertyDisplayErrorMessage>()
                {
                    new PropertyDisplayErrorMessage()
                    {
                        ErrorCode = Constants.ExpiryDateErrorMessages.ExpiryDateRequiredCode,
                        ErrorMessage = PidlModelHelper.GetLocalizedString(Constants.ExpiryDateErrorMessages.ExpiryDateRequiredMessage)
                    }
                },

                RegexMessages = new List<PropertyDisplayErrorMessage>()
                {
                    new PropertyDisplayErrorMessage()
                    {
                        ErrorCode = Constants.ExpiryDateErrorMessages.ExpiryDateInvalidCode,
                        ErrorMessage = PidlModelHelper.GetLocalizedString(Constants.ExpiryDateErrorMessages.ExpiryDateInvalidMessage)
                    }
                }
            };
        }

        private static PropertyDescription GetExpiryDateDataDescription(PropertyValidation propertyValidation)
        {
            return new PropertyDescription()
            {
                PropertyType = PXCommon.Constants.DataDescriptionPropertyType.ClientData,
                DataType = PXCommon.Constants.DataDescriptionDataType.TypeString,
                PropertyDescriptionType = PXCommon.Constants.DataDescriptionDataType.TypeString,
                IsOptional = false,
                IsUpdatable = true,
                IsKey = false,
                Validation = propertyValidation,
                Validations = new List<PropertyValidation>
                {
                    propertyValidation
                },
            };
        }

        private static PropertyDisplayHint CreateExpiryDateDisplayHint()
        {
            return new PropertyDisplayHint()
            {
                HintId = Constants.ExpiryPrefixes.ExpiryDate,
                DisplayHintType = HintType.Property.ToString().ToLower(),
                DisplayName = PidlModelHelper.GetLocalizedString(Constants.UnlocalizedDisplayText.ExpiryDateText),
                ShowDisplayName = "true",
                MinLength = 5,
                MaxLength = 7,
                DisplaySelectionText = PidlModelHelper.GetLocalizedString(Constants.UnlocalizedDisplayText.ExpiryDatePlaceholder),
                DisplayDescription = PidlModelHelper.GetLocalizedString(Constants.UnlocalizedDisplayText.ExpiryDatePlaceholder),
                PropertyName = Constants.ExpiryPrefixes.ExpiryDate,
                DisplayErrorMessages = CreateExpiryDateErrorMessage()
            };
        }
    }
}