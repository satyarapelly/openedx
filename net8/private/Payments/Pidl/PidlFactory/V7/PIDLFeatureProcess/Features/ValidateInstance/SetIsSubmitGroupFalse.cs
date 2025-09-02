// <copyright file="SetIsSubmitGroupFalse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the SetIsSubmitGroupFalse, which is to set submit group to false.
    /// </summary>
    internal class SetIsSubmitGroupFalse : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                SetIsSubmitGroup
            };
        }

        internal static void SetIsSubmitGroup(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (!featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.SetIsSubmitGroupFalse, out FeatureConfig featureConfig))
            {
                return;
            }

            var fieldsToSetIsSubmitGroupFalse = featureConfig.DisplayCustomizationDetail
                .SelectMany(detail => detail.FieldsToSetIsSubmitGroupFalse)
                .Distinct();

            foreach (var fieldName in fieldsToSetIsSubmitGroupFalse)
            {
                if (!Constants.SetIsSubmitGroupFalseMappings.TryGetValue(fieldName, out string displayHintId))
                {
                    continue;
                }

                foreach (var pidlResource in inputResources)
                {
                    FeatureHelper.SetIsSubmitGroupFalse(pidlResource, displayHintId);
                }
            }
        }
    }
}