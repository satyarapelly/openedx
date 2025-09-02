// <copyright file="SwapLogoSource.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the SwapLogoSource, which used to swap PI logo source url.
    /// </summary>
    internal class SwapLogoSource : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ReplaceCCPidlLogo
            };
        }

        internal static void ReplaceCCPidlLogo(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig = null;
            featureContext?.FeatureConfigs?.TryGetValue(FeatureConfiguration.FeatureNames.SwapLogoSource, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayHintCustomizationDetail.UseFixedSVGForMC != null && displayHintCustomizationDetail.UseFixedSVGForMC.Value)
                    {
                        foreach (PIDLResource ccPidl in inputResources)
                        {
                            LogoDisplayHint mcLogo = ccPidl.GetDisplayHintById(Constants.AddCCDisplayHintIds.MCLogo) as LogoDisplayHint;
                            if (mcLogo != null)
                            {
                                mcLogo.SourceUrl = mcLogo.SourceUrl.Replace(Constants.StaticResourceNames.MCSvg, Constants.StaticResourceNames.MasterCardLogoLeftAligned);
                            }
                        }
                    }
                }
            }
        }
    }
}