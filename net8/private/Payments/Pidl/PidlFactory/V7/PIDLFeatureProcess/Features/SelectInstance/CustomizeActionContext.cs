// <copyright file="CustomizeActionContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the CustomizeActionContext, which is to update the action context properties.
    /// </summary>
    internal class CustomizeActionContext : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                UpdateActionContext
            };
        }

        internal static void UpdateActionContext(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.CustomizeActionContext, out featureConfig);

            if (featureConfig?.DisplayCustomizationDetail?.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    foreach (PIDLResource pidlResource in inputResources)
                    {
                        if (displayHintCustomizationDetail?.ReplaceContextInstanceWithPaymentInstrumentId == true)
                        {
                            ReplaceContextInstanceWithPaymentInstrumentId(pidlResource);
                        }

                        // The following validation ensures that only the handlePurchaseRiskChallenge is called when the resource type is a challenge.
                        if (displayHintCustomizationDetail?.SetActionContextEmpty == true 
                            && string.Equals(featureContext.ResourceType, Constants.DescriptionTypes.ChallengeDescription, StringComparison.OrdinalIgnoreCase))
                        {
                            if (pidlResource?.DisplayPages != null)
                            {
                                foreach (PageDisplayHint pidlDisplayPage in pidlResource.DisplayPages)
                                {
                                    SetContextEmpty(pidlDisplayPage, Constants.ActionType.Submit);                                    
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void ReplaceContextInstanceWithPaymentInstrumentId(PIDLResource pidlResource)
        {
            PropertyDisplayHint piDisplayHint = pidlResource.GetDisplayHintById(V7.Constants.DisplayHintIds.PaymentInstrument) as PropertyDisplayHint;            

            if (piDisplayHint != null)
            {
                foreach (var possibleOption in piDisplayHint?.PossibleOptions)
                {
                    ActionContext context = possibleOption.Value.PidlAction?.Context as ActionContext;
                    if (context != null)
                    {
                        context.PaymentInstrumentId = context.Instance;
                        context.Instance = null;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the context of the specified action type to an empty string for the given display hint members.
        /// </summary>
        /// <param name="pidlDisplayPage">The list of display pages.</param>
        /// <param name="displayHintActionType">The action type whose context needs to be set to empty.</param>
        private static void SetContextEmpty(PageDisplayHint pidlDisplayPage, string displayHintActionType)
        {
            ButtonDisplayHint buttonDisplayHint = PIDLResourceDisplayHintFactory.GetButtonDisplayHintByActionType(pidlDisplayPage, displayHintActionType);

            if (buttonDisplayHint?.Action?.Context != null)
            {
                buttonDisplayHint.Action.Context = string.Empty;
            }              
        }
    }
}