// <copyright file="EnableTokenizationEncryption.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    internal class EnableTokenizationEncryption : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                AddTokenizationEncryption
            };
        }

        internal static void AddTokenizationEncryption(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.EnableTokenizationEncryption, out featureConfig);

            if (featureConfig != null)
            {
                foreach (PIDLResource paymentPidlResource in inputResources)
                {
                    var dataDescription = paymentPidlResource.GetTargetDataDescription(Constants.DataDescriptionVariableNames.Details);

                    if (dataDescription == null)
                    {
                        continue;
                    }

                    if (dataDescription.ContainsKey(Constants.DataDescriptionVariableNames.AccountToken))
                    {
                        var propertyDescription = (PropertyDescription)dataDescription[Constants.DataDescriptionVariableNames.AccountToken];
                        propertyDescription.DataProtection = GetDataProtections(featureContext.TokenizationPublicKey);
                    }

                    if (dataDescription.ContainsKey(Constants.DataDescriptionVariableNames.CvvToken))
                    {
                        var propertyDescription = (PropertyDescription)dataDescription[Constants.DataDescriptionVariableNames.CvvToken];
                        propertyDescription.DataProtection = GetDataProtections(featureContext.TokenizationPublicKey);
                    }
                }
            }
        }

        private static PropertyDataProtection GetDataProtections(string tokenizationPublicKey)
        {
            return new PropertyDataProtection()
            {
                ProtectionType = Constants.PropertyDataProtectionType.MSREncrypt,
                Parameters = new Dictionary<string, string>()
                {
                    { Constants.PropertyDataProtectionParamName.EncryptionScript, Constants.PropertyDataProtectionParamValue.EncryptionScript },
                    { Constants.PropertyDataProtectionParamName.EncryptionLibrary, Constants.PropertyDataProtectionParamValue.EncryptionLibrary },
                    { Constants.PropertyDataProtectionParamName.PublicKey, tokenizationPublicKey }
                }
            };
        }
    }
}