// <copyright file="RemoveDataSource.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the RemoveDataSource, which is removes the datasources from the pidl for update op.
    /// </summary>
    internal class RemoveDataSource : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                RemoveDataSourceFromPidls
            };
        }

        internal static void RemoveDataSourceFromPidls(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.RemoveDataSource, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayHintCustomizationDetail?.RemoveDataSourceResources != null
                        && displayHintCustomizationDetail.RemoveDataSourceResources.Contains(featureContext.ResourceType, StringComparer.OrdinalIgnoreCase))
                    {
                        foreach (PIDLResource pidlResource in inputResources)
                        {
                            pidlResource?.RemoveDataSource();

                            if (pidlResource?.LinkedPidls != null && string.Equals(featureContext.ResourceType, Constants.DescriptionTypes.TaxIdDescription, StringComparison.OrdinalIgnoreCase))
                            {
                                pidlResource.LinkedPidls.ForEach(linkedPidl => linkedPidl?.RemoveDataSource());
                            }
                        }
                    }
                }
            }
        }
    }
}