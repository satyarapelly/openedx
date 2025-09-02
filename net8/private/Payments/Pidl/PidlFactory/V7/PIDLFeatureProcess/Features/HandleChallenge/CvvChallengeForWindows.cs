// <copyright file="CvvChallengeForWindows.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    internal class CvvChallengeForWindows : IFeature
    {
        private static Dictionary<string, Dictionary<string, IEnumerable<string>>> styleHints = new Dictionary<string, Dictionary<string, IEnumerable<string>>>()
        {
            {
                "purchaseChallengeCvvPage", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "gap-small" } }
                }
            },
            {
                "challengeCardDetailsGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "gap-x-small", "alignment-vertical-center" } }
                }
            },
            {
                "challengeCardLogo", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "image-small" } }
                }
            },
            {
                "starRequiredTextGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "gap-2x-small" } }
                }
            },
            {
                "starLabel", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "text-alert" } }
                }
            },
            {
                "cvv", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-quarter" } }
                }
            },
            {
                "cancelCvv3DSSubmitGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "anchor-bottom", "gap-small" } }
                }
            },
            {
                "cancelBackButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill" } }
                }
            },
            {
                "cvv3DSSubmitButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill" } }
                }
            },
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ProcessCvvChallengeForWindows,
            };
        }

        internal static void ProcessCvvChallengeForWindows(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (featureContext == null || featureContext.ResourceType != Constants.DescriptionTypes.ChallengeDescription || featureContext.TypeName != Constants.ChallengeDescriptionTypes.Cvv)
            {
                return;
            }

            foreach (PIDLResource pidl in inputResources)
            {
                if (pidl?.DisplayPages == null)
                {
                    continue;
                }

                List<DisplayHint> starRequiredTextGroups = pidl.GetAllDisplayHintsOfId("starRequiredTextGroup");

                FeatureHelper.ConvertToGroupDisplayHint(starRequiredTextGroups, "inline");
                FeatureHelper.PassStyleHints(pidl, styleHints, featureContext);
            }
        }
    }
}
