// <copyright file="ChangeExpiryMonthYearToExpiryDateTextBox.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the Expiry Date TextBox - Enable feature, which is to change style structure of expiry month and year from dropdown to one text box for month and year.
    /// </summary>
    internal class ChangeExpiryMonthYearToExpiryDateTextBox : IFeature
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
                featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.ChangeExpiryMonthYearToExpiryDateTextBox, out FeatureConfig featureConfig);
                featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.CustomizeDisplayContent, out FeatureConfig featureConfigDisplayContent);

                if (featureConfig != null)
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
                            expiryMonthDataDescription.AddTransformation(
                                GetTransformation(Constants.TransformationRegex.ExpiryMonth, "$1"));

                            expiryYear.IsHidden = true;
                            expiryYearDataDescription.PropertyType = PXCommon.Constants.DataDescriptionPropertyType.ClientData;
                            expiryYearDataDescription.IsOptional = true;
                            expiryYearDataDescription.AddTransformation(
                                GetTransformation(Constants.TransformationRegex.ExpiryYear, "20$2"));

                            var propertyValidation = new PropertyValidation()
                            {
                                Regex = Constants.ValidationRegex.ExpiryDate,
                                ErrorCode = Constants.ExpiryDateErrorMessages.ExpiryDateInvalidCode,
                                ErrorMessage = PidlModelHelper.GetLocalizedString(Constants.ExpiryDateErrorMessages.ExpiryDateFormatMessage)
                            };

                            var expiryDateDataDescription = GetExpiryDateDataDescription(expiryMonth, expiryYear, propertyValidation);

                            var details = pidlResource.GetTargetDataDescription(Constants.DataDescriptionIds.Details);
                            if (details != null)
                            {
                                details.Add(Constants.ExpiryPrefixes.ExpiryDate, expiryDateDataDescription);
                                PropertyDisplayHint expiryDate = CreateExpiryDateDisplayHint();

                                if (featureConfigDisplayContent != null && featureConfigDisplayContent.DisplayCustomizationDetail != null && featureConfigDisplayContent.DisplayCustomizationDetail.Any(x => x.AddAsteriskToAllMandatoryFields == true))
                                {
                                    expiryDate.DisplayName = FeatureHelper.AppendAsteriskToText(expiryDate.DisplayName, expiryDate.DisplayTags);
                                }

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
                                        var expiryDateIndex = displayPage.Members?.FindIndex(hid => hid.HintId == expiryGroup.HintId);
                                        if (expiryDateIndex.HasValue && expiryDateIndex.Value >= 0)
                                        {
                                            displayPage.Members?.Remove(expiryGroup);
                                            displayPage.Members?.Remove(cvvGroup);
                                            displayPage.Members?.Insert(expiryDateIndex.Value, expiryCvvGroupHint);
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

        private static PropertyDescription GetExpiryDateDataDescription(PropertyDisplayHint expiryMonth, PropertyDisplayHint expiryYear, PropertyValidation propertyValidation)
        {
            return new PropertyDescription()
            {
                PropertyType = PXCommon.Constants.DataDescriptionPropertyType.UserData,
                DataType = PXCommon.Constants.DataDescriptionDataType.TypeString,
                PropertyDescriptionType = PXCommon.Constants.DataDescriptionDataType.TypeString,
                IsOptional = false,
                IsUpdatable = true,
                IsKey = false,
                IsConditionalFieldValue = true,
                Validation = propertyValidation,
                Transformation = GetTransformation(Constants.TransformationRegex.ExpiryMonth, "$1/$2"),
                Validations = new List<PropertyValidation>
                                {
                                    propertyValidation
                                },
                SideEffects = new Dictionary<string, string>()
                                {
                                    { expiryMonth.PropertyName,  "{" + Constants.ExpiryPrefixes.ExpiryDate + "}" },
                                    { expiryYear.PropertyName,  "{" + Constants.ExpiryPrefixes.ExpiryDate + "}" }
                                }
            };
        }

        private static PropertyDisplayHint CreateExpiryDateDisplayHint()
        {
            return new PropertyDisplayHint()
            {
                HintId = Constants.ExpiryPrefixes.ExpiryDate,
                DisplayHintType = HintType.Property.ToString().ToLower(),
                DisplayName = PidlModelHelper.GetLocalizedString("Expiration date"),
                ShowDisplayName = "true",
                MinLength = 5,
                MaxLength = 5,
                DisplaySelectionText = "MM/YY",
                DisplayDescription = "MM/YY",
                PropertyName = Constants.ExpiryPrefixes.ExpiryDate,
                DisplayErrorMessages = CreateExpiryDateErrorMessage()
            };
        }

        private static Dictionary<string, PropertyTransformationInfo> GetTransformation(string inputRegex, string transformRegex)
        {
            return new Dictionary<string, PropertyTransformationInfo>()
                    {
                        {
                            PXCommon.Constants.DataDescriptionTransformationTarget.ForSubmit, new PropertyTransformationInfo()
                            {
                                TransformCategory = PXCommon.Constants.DataDescriptionTransformationType.RegexTransformation,
                                InputRegex = inputRegex,
                                TransformRegex = transformRegex
                            }
                        }
                    };
        }
    }
}