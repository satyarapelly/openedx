// <copyright file="CustomizeAddressForm.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.FeatureContextProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;

    /// <summary>
    /// Class representing the CustomizeAddressForm, which is used to Customize Address form.
    /// </summary>
    internal class CustomizeAddressForm : IFeatureContextFeature
    {
        public List<Action<FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<FeatureContext>>()
            {
                SetTypeName
            };
        }

        internal static void SetTypeName(FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.CustomizeAddressForm, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayHintCustomizationDetail.DataSource != null && string.Equals(displayHintCustomizationDetail.AddressType, featureContext.TypeName, StringComparison.OrdinalIgnoreCase))
                    {
                        string newTypeName;

                        if (Constants.DataSourceToAddressTypeMappings.TryGetValue(displayHintCustomizationDetail.DataSource, out newTypeName))
                        {
                            featureContext.OriginalTypeName = featureContext.TypeName;
                            featureContext.TypeName = newTypeName;
                        }
                    }
                }
            }
        }
    }
}