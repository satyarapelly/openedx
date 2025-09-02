// <copyright file="EnableUpdateCreditCardRegex.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    internal class EnableUpdateCreditCardRegex : IFeature
    {        
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                UpdateCreditCardRegex
            };
        }

        internal static void UpdateCreditCardRegex(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (inputResources == null || inputResources.Count == 0)
            {
                return;
            }

            if (featureContext == null || featureContext.FeatureConfigs == null)
            {
                return;
            }

            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.EnableUpdateCreditCardRegex, out featureConfig);

            var displayCustomizationDetails = featureConfig?.DisplayCustomizationDetail.FirstOrDefault(d => d.UpdateRegexesForCards != null);

            if (displayCustomizationDetails != null)
            {
                foreach (var inputResource in inputResources)
                {
                    UpdateRegex(inputResource, displayCustomizationDetails.UpdateRegexesForCards);
                }
            }
        }

        private static void UpdateRegex(PIDLResource inputResource, IEnumerable<KeyValuePair<string, string>> regexes)
        {
            const string Type = "type";

            if (regexes == null || !regexes.Any())
            {
                return;
            }

            if (inputResource.Identity != null && inputResource.Identity.ContainsKey(Type))
            {                
                var cardType = inputResource.Identity[Type].ToLower();
                var regex = regexes.FirstOrDefault(kv => kv.Key == cardType).Value;
                if (regex != null)
                {
                    var dataDescription = inputResource.GetTargetDataDescription(Constants.DataDescriptionVariableNames.Details);
                    if (dataDescription != null && dataDescription.ContainsKey(Constants.DataDescriptionVariableNames.AccountToken))
                    {
                        var propertyDescription = (PropertyDescription)dataDescription[Constants.DataDescriptionVariableNames.AccountToken];
                        propertyDescription.Validation.Regex = regex;
                        foreach (var item in propertyDescription.Validations)
                        {
                            if (item.ValidationType == "regex")
                            {
                                item.Regex = regex;
                            }
                        }
                    }                    
                }
            }
        }
    }
}