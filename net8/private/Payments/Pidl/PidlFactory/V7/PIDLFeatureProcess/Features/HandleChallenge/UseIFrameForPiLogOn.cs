// <copyright file="UseIFrameForPiLogOn.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the UpdateGlobalPiLoginLink feature, which changes the login link to a move last action.
    /// </summary>
    internal class UseIFrameForPiLogOn : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                UpdateGlobalPILogicPidlAction
            };
        }

        internal static void UpdateGlobalPILogicPidlAction(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig = null;
            featureContext?.FeatureConfigs?.TryGetValue(FeatureConfiguration.FeatureNames.UseIFrameForPiLogOn, out featureConfig);

            if (featureConfig != null)
            {
                foreach (PIDLResource resource in inputResources)
                {
                    if (resource?.ClientAction?.ActionType == PXCommon.ClientActionType.Pidl)
                    {
                        var clientActionContexts = resource.ClientAction.Context as List<PIDLResource>;
                        foreach (var clientActionContext in clientActionContexts)
                        {
                            if (string.Equals(clientActionContext.Identity[Constants.DescriptionIdentityFields.DescriptionType], Constants.DescriptionTypes.ChallengeDescription, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(clientActionContext.Identity[Constants.DescriptionIdentityFields.Type], Constants.ChallengeDescriptionTypes.GlobalPIQrCode, StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (var displayPage in clientActionContext.DisplayPages)
                                {
                                    var link = UpdateLinkAction(displayPage, Constants.DisplayHintIds.GlobalPIQrCodeRedirectButton, "updatePollAndMoveLast", "globalPIQrCodeChallengePage", "globalPIQrCodeChallengePage3");
                                    link?.DisplayTags.Add("pidlReact.noUrl", "pidlReact.noUrl");

                                    UpdateLinkAction(displayPage, Constants.DisplayHintIds.GlobalPIQrCodeRedirectButtonPage2, "moveNext", string.Empty);
                                }

                                UpdatePrivacyTextGroup(clientActionContext);
                            }
                        }
                    }
                }
            }
        }

        private static DisplayHint UpdateLinkAction(PageDisplayHint displayPage, string hintId, string actionType, string context = null, string destinationId = null)
        {
            var link = displayPage?.Members?.FirstOrDefault(member => member.HintId == hintId);

            if (link != null)
            {
                link.Action.ActionType = actionType;
                link.Action.Context = context;
                link.Action.DestinationId = destinationId;
            }

            return link;
        }

        private static void UpdatePrivacyTextGroup(PIDLResource pidl)
        {
            var privacyGroups = pidl.GetAllDisplayHintsOfId(Constants.DisplayHintIds.MicrosoftPrivacyTextGroup);

            foreach (ContainerDisplayHint privacyGroup in privacyGroups)
            {
                privacyGroup.DisplayHintType = Constants.DisplayHintTypes.Group;
                privacyGroup.LayoutOrientation = Constants.PartnerHintsValues.InlinePlacement;
                privacyGroup.StyleHints = new List<string>() { "display-textgroup" };

                var privacyText = privacyGroup.Members?.FirstOrDefault(member => member?.HintId == Constants.DisplayHintIds.MicrosoftPrivacyStaticText);
                if (privacyText != null)
                {
                    var styleHints = new List<string>();
                    if (privacyText.StyleHints != null)
                    {
                        styleHints.AddRange(privacyText.StyleHints);
                    }

                    styleHints.Add("margin-end-2x-small");

                    privacyText.StyleHints = styleHints;
                }
            }
        }
    }
}
