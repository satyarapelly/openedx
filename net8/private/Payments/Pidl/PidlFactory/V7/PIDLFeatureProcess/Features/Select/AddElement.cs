// <copyright file="AddElement.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the AddElement feature, which is used to add elements in to a PIDL.
    /// </summary>
    internal class AddElement : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                AddDisplayHints
            };
        }

        private static void AddDisplayHints(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig = null;
            featureContext?.FeatureConfigs?.TryGetValue(FeatureConfiguration.FeatureNames.AddElement, out featureConfig);

            if (featureConfig?.DisplayCustomizationDetail?.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    foreach (PIDLResource resource in inputResources)
                    {
                        if (displayHintCustomizationDetail != null && displayHintCustomizationDetail.AddCancelButton)
                        {
                            GroupDisplayHint cancelGroup = FeatureHelper.CreateGroupDisplayHint(Constants.DisplayHintIds.CancelGroup, Constants.LayoutOrientations.Inline, true);
                            cancelGroup.AddStyleHints(new List<string> { "padding-vertical-medium", "gap-medium" });

                            ButtonDisplayHint cancelButton = FeatureHelper.CreateButtonDisplayHint(Constants.DisplayHintIds.CancelButton, LocalizationRepository.Instance.GetLocalizedString("Cancel", featureContext.Language));
                            cancelButton.AddStyleHint("width-third");
                            cancelButton.Action = new DisplayHintAction()
                            {
                                ActionType = DisplayHintActionType.gohome.ToString()
                            };

                            cancelGroup.Members.Add(cancelButton);

                            PageDisplayHint page = resource.DisplayPages.First();
                            page?.AddDisplayHint(cancelGroup);
                        }

                        if (displayHintCustomizationDetail != null && (displayHintCustomizationDetail.AddPickAWayToPayHeading ?? false) && string.Equals(featureContext.ResourceType, Constants.ResourceTypes.PaymentMethod, StringComparison.OrdinalIgnoreCase))
                        {
                            HeadingDisplayHint headingDisplayHint = new HeadingDisplayHint()
                            {
                                HintId = Constants.DisplayHintIds.PaymentInstrumentSelectHeading,
                                DisplayContent = PidlModelHelper.GetLocalizedString(Constants.UnlocalizedDisplayText.PaymentInstrumentSelectHeading, featureContext.Language),
                            };

                            PageDisplayHint firstPage = resource.DisplayPages?.FirstOrDefault();
                            firstPage?.Members?.Insert(0, headingDisplayHint);
                        }
                    }
                }
            }
        }
    }
}
