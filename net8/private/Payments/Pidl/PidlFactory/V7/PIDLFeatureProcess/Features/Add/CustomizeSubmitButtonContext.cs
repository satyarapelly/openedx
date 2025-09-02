// <copyright file="CustomizeSubmitButtonContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;

    /// <summary>
    /// Class representing the CustomizeDisplayContent, which is to change display content of a PIDL element.
    /// </summary>
    internal class CustomizeSubmitButtonContext : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ChangeSubmitButtonContext
            };
        }

        internal static void ChangeSubmitButtonContext(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.CustomizeSubmitButtonContext, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    string targetUri = GetTargetUriForEndPoint(displayHintCustomizationDetail.EndPoint);

                    if (string.Equals(featureContext.OriginalTypeName, displayHintCustomizationDetail.ProfileType, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (PIDLResource pidlResource in inputResources)
                        {
                            var buttonDisplayHint = pidlResource.GetDisplayHintById(V7.Constants.ButtonDisplayHintIds.SaveButton);

                            if (buttonDisplayHint != null)
                            {
                                var buttonAction = buttonDisplayHint?.Action;

                                if (buttonAction != null)
                                {
                                    UpdateButtonSubmitAction(buttonAction, targetUri, displayHintCustomizationDetail.Operation);
                                }

                                if (buttonAction?.NextAction != null)
                                {
                                    UpdateButtonSubmitAction(buttonAction.NextAction, targetUri, displayHintCustomizationDetail.Operation);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string GetTargetUriForEndPoint(string endPoint)
        {
            string targetUri = string.Empty;

            if (string.Equals(endPoint, V7.Constants.JarvisEndpoints.MyFamily, StringComparison.OrdinalIgnoreCase))
            {
                targetUri = string.Format(V7.Constants.SubmitUrls.JarvisFdFamilyProfileUpdateUrlTemplate, endPoint);
            }

            return targetUri;
        }

        private static void UpdateButtonSubmitAction(DisplayHintAction buttonAction, string targetUriTemplate, string operation)
        {
            if (buttonAction != null && string.Equals(buttonAction.ActionType, V7.Constants.ActionType.Submit, StringComparison.OrdinalIgnoreCase))
            {
                var context = buttonAction.Context as RestLink;

                if (context != null)
                {
                    context.Href = targetUriTemplate;
                    context.Method = operation;
                    buttonAction.Context = context;
                }
            }
        }
    }
}
