// <copyright file="ChangeExpiryStyleToTextBox.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the ChangeExpiryStyle, which is to change style structure of expiry month and year. e.g. dropdown to text
    /// </summary>
    internal class ChangeExpiryStyleToTextBox : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ChangeExpiryStyleStructure
            };
        }

        internal static void ChangeExpiryStyleStructure(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.ChangeExpiryStyleToTextBox, out FeatureConfig featureConfig);

            if (featureConfig != null && inputResources != null)
            {
                foreach (PIDLResource pidlResource in inputResources)
                {
                    ContainerDisplayHint expiryGroup = pidlResource.GetPidlContainerDisplayHintbyDisplayId(Constants.ExpiryPrefixes.ExpiryGroup);
                    PropertyDisplayHint expiryMonth = pidlResource.GetDisplayHintById(Constants.ExpiryPrefixes.ExpiryMonth) as PropertyDisplayHint;
                    PropertyDisplayHint expiryYear = pidlResource.GetDisplayHintById(Constants.ExpiryPrefixes.ExpiryYear) as PropertyDisplayHint;

                    PropertyDescription expiryMonthDataDescription = pidlResource.GetPropertyDescriptionByPropertyName(Constants.ExpiryPrefixes.ExpiryMonth);
                    PropertyDescription expiryYearDataDescription = pidlResource.GetPropertyDescriptionByPropertyName(Constants.ExpiryPrefixes.ExpiryYear);

                    if (expiryGroup != null && expiryMonth != null && expiryYear != null
                        && expiryMonthDataDescription != null && expiryYearDataDescription != null)
                    {
                        expiryGroup.Members[0] = expiryMonth;
                        expiryGroup.Members[1] = expiryYear;
                        expiryGroup.AddDisplayTag("pidlReact.fluent-ui.innerStackFlex.1", "pidlReact.fluent-ui.innerStackFlex.1");
                        expiryGroup.StyleHints = null;

                        RemoveStyleHintsAndPossibleCollection(expiryMonth, "MM");
                        RemoveStyleHintsAndPossibleCollection(expiryYear, "YY");

                        expiryMonth.DisplayErrorMessages = new PropertyDisplayErrorMessageMap()
                        {
                            DefaultErrorMessage = "Expiration month is a required field",
                            ErrorCodeMessages = new List<PropertyDisplayErrorMessage>()
                            {
                                new PropertyDisplayErrorMessage()
                                {
                                    ErrorCode = "required_field_empty",
                                    ErrorMessage = "Expiration month is a required field"
                                }
                            },

                            RegexMessages = new List<PropertyDisplayErrorMessage>()
                            {
                                new PropertyDisplayErrorMessage()
                                {
                                    ErrorCode = "expiry_month_invalid",
                                    ErrorMessage = "This is not a valid expiration month"
                                }
                            }
                        };

                        expiryMonthDataDescription.AddAdditionalValidation(
                            new PropertyValidation()
                            {
                                ValidationType = "regex",
                                Regex = "^(([1-9])|(0[1-9])|(1[0-2]))$",
                                ErrorCode = "expiry_month_invalid",
                                ErrorMessage = "Incorrectly formatted expiration month"
                            });

                        expiryMonthDataDescription.AddTransformation(
                            new Dictionary<string, PropertyTransformationInfo>()
                            {
                                {
                                    "forSubmit", new PropertyTransformationInfo()
                                    {
                                        UrlTransformationType = "regex",
                                        InputRegex = "^0?([1-9]|1[0-2])$",
                                        TransformRegex = "$1"
                                    }
                                },
                                {
                                    "forDisplay", new PropertyTransformationInfo()
                                    {
                                        UrlTransformationType = "regex",
                                        InputRegex = "^([1-9]|1[0-2])$",
                                        TransformRegex = "0$1"
                                    }
                                }
                            });

                        expiryYear.DisplayErrorMessages = new PropertyDisplayErrorMessageMap()
                        {
                            DefaultErrorMessage = "Expiration year is a required field",
                            ErrorCodeMessages = new List<PropertyDisplayErrorMessage>()
                            {
                                new PropertyDisplayErrorMessage()
                                {
                                    ErrorCode = "required_field_empty",
                                    ErrorMessage = "Expiration year is a required field"
                                }
                            },

                            RegexMessages = new List<PropertyDisplayErrorMessage>()
                            {
                                new PropertyDisplayErrorMessage()
                                {
                                    ErrorCode = "expiry_year_invalid",
                                    ErrorMessage = "This is not a valid expiration year"
                                }
                            }
                        };

                        expiryYearDataDescription.AddAdditionalValidation(
                            new PropertyValidation()
                            {
                                ValidationType = "regex",
                                Regex = "^(2[5-9]|3[0-9]|4[0-8])$",
                                ErrorCode = "expiry_year_invalid",
                                ErrorMessage = "Incorrectly formatted expiration year"
                            });

                        expiryYearDataDescription.AddTransformation(
                            new Dictionary<string, PropertyTransformationInfo>()
                            {
                                {
                                    "forSubmit", new PropertyTransformationInfo()
                                    {
                                        UrlTransformationType = "regex",
                                        InputRegex = "^\\d{0,2}(\\d{2})",
                                        TransformRegex = "20$1"
                                    }
                                },
                                {
                                    "forDisplay", new PropertyTransformationInfo()
                                    {
                                        UrlTransformationType = "regex",
                                        InputRegex = "^\\d{0,2}(\\d{2})",
                                        TransformRegex = "$1"
                                    }
                                }
                            });
                    }
                }
            }
        }

        private static void RemoveStyleHintsAndPossibleCollection(PropertyDisplayHint displayHint, string displayDescription)
        {
            displayHint.StyleHints = null;
            displayHint.PossibleOptions = null;
            displayHint.PossibleValues = null;
            displayHint.DisplayDescription = displayDescription;
        }
    }
}