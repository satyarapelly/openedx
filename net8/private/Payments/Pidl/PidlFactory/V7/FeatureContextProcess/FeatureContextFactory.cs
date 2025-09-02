// <copyright file="FeatureContextFactory.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.FeatureContextProcess
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class maintaining the features to be enabled for feature context process
    /// </summary>
    public class FeatureContextFactory : IFeatureContextFactory
    {
        private readonly Dictionary<string, IFeatureContextFeature> features = new Dictionary<string, IFeatureContextFeature>()
        {
            { FeatureConfiguration.FeatureNames.CustomizeAddressForm, new CustomizeAddressForm() },
        };

        public Dictionary<string, IFeatureContextFeature> GetFeatures(PIDLFeatureProcess.FeatureContext featureContext)
        {
            // Logic if FeatureConfigs are provided in featureContext
            if (featureContext.FeatureConfigs != null && featureContext.FeatureConfigs.Count > 0)
            {
                return this.features.Where(feature => FeatureConfiguration.IsEnabledUsingPartnerSettings(feature.Key, featureContext)).ToDictionary(feature => feature.Key, feature => feature.Value);
            }

            return this.features.Where(feature => FeatureConfiguration.IsEnabled(feature.Key, featureContext)).ToDictionary(feature => feature.Key, feature => feature.Value);
        }
    }
}