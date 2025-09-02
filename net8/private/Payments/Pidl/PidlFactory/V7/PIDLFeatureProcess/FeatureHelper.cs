// <copyright file="FeatureHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// This class contains helper functions to suport PIDL templates
    /// </summary>
    public class FeatureHelper
    {
        private static readonly List<string> XboxNativeMediumWidthLogos = new List<string>()
        {
            "ewallet_stored_value_logo",
            "large_logo",
            "single-ewallet_",
            "single-online_",
            "single-credit_",
            "single-mobile_billing_non_sim_",
        };

        public static bool IsMediumWidthLogo(string logoId)
        {
            foreach (string mediumLogoId in XboxNativeMediumWidthLogos)
            {
                if (string.Equals(logoId, mediumLogoId) || logoId.StartsWith(mediumLogoId))
                {
                    return true;
                }
            }

            return false;
        }

        public static void HideDisplayHint(PIDLResource resource, string hintId)
        {
            ModifyDisplayHint(resource, hintId, hint => hint.IsHidden = true);
        }

        public static void DisableDisplayHint(PIDLResource resource, string hintId)
        {
            ModifyDisplayHint(resource, hintId, hint => hint.IsDisabled = true);
        }

        public static void EnableDisplayHint(PIDLResource resource, string hintId)
        {
            ModifyDisplayHint(resource, hintId, hint => hint.IsDisabled = false);
        }

        public static void PassStyleHints(PIDLResource resource, Dictionary<string, Dictionary<string, IEnumerable<string>>> styleHints, FeatureContext featureContext)
        {
            if (resource != null)
            {
                List<DisplayHint> displayHints = PIDLResourceDisplayHintFactory.GetAllDisplayHints(resource);

                foreach (DisplayHint displayHint in displayHints)
                {
                    displayHint.StyleHints = GetStyleHintsForElement(displayHint.HintId, featureContext, styleHints);
                }
            }
        }

        public static string AppendAsteriskToText(string text, Dictionary<string, string> displayTags)
        {
            if (!string.IsNullOrWhiteSpace(text) && !text.TrimEnd().EndsWith(Constants.DisplayNames.Asterisk))
            {
                var modifiedText = text + Constants.SuggestedAddressesStaticText.Spacer + Constants.DisplayNames.Asterisk;

                if (displayTags != null && displayTags.TryGetValue(Constants.DiplayHintProperties.AccessibilityName, out var existingValue))
                {
                    displayTags[Constants.DiplayHintProperties.AccessibilityName] = existingValue + Constants.SuggestedAddressesStaticText.Spacer + Constants.DisplayNames.Asterisk;
                }

                return modifiedText;
            }

            return text;
        }

        public static IEnumerable<string> GetStyleHintsForElement(string hintId, FeatureContext featureContext, Dictionary<string, Dictionary<string, IEnumerable<string>>> styleHints)
        {
            string country = !string.IsNullOrEmpty(featureContext.Country) ? featureContext.Country : string.Empty;
            string hintIdPrefix = string.Empty;
            string[] hintIdParts = hintId.Split('_');
            if (hintIdParts.Length > 1)
            {
                hintIdPrefix = hintIdParts[0] + "_";
            }

            if (styleHints.ContainsKey(hintId))
            {
                return styleHints[hintId].ContainsKey(country) ?
                    styleHints[hintId][country] :
                    styleHints[hintId][string.Empty];
            }
            else if (styleHints.ContainsKey(hintIdPrefix))
            {
                return styleHints[hintIdPrefix].ContainsKey(country) ?
                    styleHints[hintIdPrefix][country] :
                    styleHints[hintIdPrefix][string.Empty];
            }

            return null;
        }

        public static void EditDisplayContent(PIDLResource pidlResource, string hintId, string unlocalizedNewDisplayText)
        {
            DisplayHint displayHint = pidlResource.GetDisplayHintById(hintId);
            if (displayHint != null)
            {
                string accessibilityTagKey = Constants.DiplayHintProperties.AccessibilityName;
                string newDisplayText = PidlModelHelper.GetLocalizedString(unlocalizedNewDisplayText);

                var prefillControlDisplayHint = displayHint as PrefillControlDisplayHint;
                if (prefillControlDisplayHint != null)
                {
                    prefillControlDisplayHint.DisplayName = newDisplayText;
                    if (prefillControlDisplayHint.DisplayTags != null &&
                        prefillControlDisplayHint.DisplayTags.ContainsKey(accessibilityTagKey))
                    {
                        prefillControlDisplayHint.DisplayTags[accessibilityTagKey] = newDisplayText;
                    }
                }
                else
                {
                    var contentDisplayHint = displayHint as ContentDisplayHint;
                    if (contentDisplayHint != null)
                    {
                        contentDisplayHint.DisplayContent = newDisplayText;
                        if (contentDisplayHint.DisplayTags != null &&
                            contentDisplayHint.DisplayTags.ContainsKey(accessibilityTagKey))
                        {
                            contentDisplayHint.DisplayTags[accessibilityTagKey] = newDisplayText;
                        }
                    }
                }
            }
        }

        public static void UpdateLogoUrl(PIDLResource resource, Dictionary<string, string> logoUrls)
        {
            // Some logos have fixed width and height because of which they are not resized properly.
            // So, we need to update the source url to the new image.
            foreach (string logoHintId in logoUrls.Keys)
            {
                List<DisplayHint> displayHints = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(resource, logoHintId);

                if (displayHints != null)
                {
                    foreach (DisplayHint displayHint in displayHints)
                    {
                        UpdateSourceUrlForLogoAndImageDisplayHint(displayHint, logoUrls[logoHintId]);
                    }
                }
            }
        }

        // Logos could be of type ImageDisplayHint or LogoDisplayHint, so overloading this method to handle both types of logos.
        public static void UpdateSourceUrlForLogoAndImageDisplayHint(DisplayHint hint, string url)
        {
            LogoDisplayHint logo = hint as LogoDisplayHint;
            ImageDisplayHint image = hint as ImageDisplayHint;

            if (logo != null)
            {
                int staticResourceIndex = logo.SourceUrl.IndexOf("/staticresourceservice/", System.StringComparison.OrdinalIgnoreCase);
                if (staticResourceIndex > 0)
                {
                    logo.SourceUrl = logo.SourceUrl.Substring(0, staticResourceIndex) + url;
                }
            }
            else if (image != null)
            {
                int staticResourceIndex = image.SourceUrl.IndexOf("/staticresourceservice/", System.StringComparison.OrdinalIgnoreCase);
                if (staticResourceIndex > 0)
                {
                    image.SourceUrl = image.SourceUrl.Substring(0, staticResourceIndex) + url;
                }
            }
        }

        public static void ConvertToGroupDisplayHint(List<DisplayHint> displayHints, string layoutOrientation = null)
        {
            if (displayHints != null)
            {
                foreach (DisplayHint displayHint in displayHints)
                {
                    ConvertToGroupDisplayHint(displayHint as ContainerDisplayHint, layoutOrientation);
                }
            }
        }

        /// <summary>
        /// Retrieves the DisplayHint based on the feature flight.
        /// </summary>
        /// <param name="hintId">Contains the hintId to be used to obtain the DisplaySequenceId.</param>
        /// <param name="featureContext">Holds all additional information required for the PIDL.</param>
        /// <param name="featureFlightValue">Stores the name of the feature flight.</param>
        /// <returns>Returns the DisplayHint based on the HintId and feature flight.</returns>
        public static List<DisplayHint> GetDisplayHintByFeatureFlight(string hintId, FeatureContext featureContext, string featureFlightValue)
        {
            List<string> featureFlighting = new List<string>(featureContext.ExposedFlightFeatures);

            if (featureFlightValue != null)
            {
                featureFlighting.Add(featureFlightValue);
            }

            Dictionary<string, string> context = new Dictionary<string, string>()
            {
                { Constants.ConfigSpecialStrings.CountryId, featureContext.Country },
                { Constants.ConfigSpecialStrings.Language, featureContext.Language },
                { Constants.ConfigSpecialStrings.Operation, featureContext.OperationType },
                { Constants.ConfigSpecialStrings.EmailAddress, Context.EmailAddress },
                { Constants.HiddenOptionalFields.ContextKey, string.Empty },
                { Constants.ConfigSpecialStrings.PaymentMethodDisplayName, GlobalConstants.Defaults.DisplayName }
            };

            List<DisplayHint> pidlResourceByFeatureFlight = PIDLResourceDisplayHintFactory.Instance.GetDisplayHints(
                partnerName: Constants.TemplateName.DefaultTemplate,
                displayHintId: hintId,
                country: featureContext.Country,
                operation: featureContext.OperationType,
                context: context,
                scenario: featureContext.Scenario,
                flightNames: featureFlighting,
                pidlResourceType: featureContext.TypeName,
                pidlResourceIdentity: featureContext.ResourceType).ToList();

            return pidlResourceByFeatureFlight;
        }

        public static DisplayHint GetButtonDisplayHintInPIDLResource(PIDLResource pidl)
        {
            DisplayHint buttonDisplayHint;

            List<string> buttonDisplayHintNames = new List<string>
            {
                Constants.ButtonDisplayHintIds.SaveButton,
                Constants.ButtonDisplayHintIds.ValidateButtonHidden,
                Constants.ButtonDisplayHintIds.SubmitButtonHidden,
                Constants.ButtonDisplayHintIds.ValidateThenSubmitButtonHidden,
                Constants.ButtonDisplayHintIds.ValidateThenSubmitButton
            };

            foreach (string buttonDisplayHintName in buttonDisplayHintNames)
            {
                buttonDisplayHint = pidl.GetDisplayHintById(buttonDisplayHintName);
                if (buttonDisplayHint != null)
                {
                    return buttonDisplayHint;
                }
            }

            return null;
        }

        public static void ConvertToGroupDisplayHint(ContainerDisplayHint displayHint, string layoutOrientation = null)
        {
            if (displayHint != null)
            {
                displayHint.DisplayHintType = Constants.DisplayHintTypes.Group;
                displayHint.LayoutOrientation = layoutOrientation;
            }
        }

        public static void SetIsSubmitGroupFalse(PIDLResource pidl, string displayHintId)
        {
            GroupDisplayHint groupDisplayHint = pidl.GetPidlContainerDisplayHintbyDisplayId(displayHintId) as GroupDisplayHint;
            if (groupDisplayHint != null)
            {
                groupDisplayHint.IsSumbitGroup = false;
            }
        }

        public static GroupDisplayHint CreateGroupDisplayHint(string hintId, string orientation = null, bool? isSubmitGroup = null)
        {
            return new GroupDisplayHint
            {
                HintId = hintId,
                LayoutOrientation = orientation,
                IsSumbitGroup = isSubmitGroup,
                StyleHints = new List<string>() { "width-fill" }
            };
        }

        public static ButtonDisplayHint CreateButtonDisplayHint(string hintId, string displayContent)
        {
            var button = new ButtonDisplayHint
            {
                HintId = hintId,
                DisplayContent = displayContent,
            };

            button.AddDisplayTag("accessibilityName", hintId);

            return button;
        }

        private static void ModifyDisplayHint(PIDLResource resource, string hintId, Action<DisplayHint> modifyAction)
        {
            DisplayHint displayHint = resource.GetDisplayHintById(hintId);
            if (displayHint != null)
            {
                modifyAction(displayHint);
            }
        }
    }
}