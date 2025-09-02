// <copyright file="VerifyAddressStyling.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Used for reorganizing display content in Add CC
    /// </summary>
    internal class VerifyAddressStyling : IFeature
    {
        // Key: hintId
        // Value: Dictionary where country is the key and the value is a list of style hints. Better to have country as key as there might be a case where a country has a different style hint than the default.
        private static Dictionary<string, Dictionary<string, IEnumerable<string>>> styleHints = new Dictionary<string, Dictionary<string, IEnumerable<string>>>()
        {
            {
                "addressNoSuggestionMessage", new Dictionary<string, IEnumerable<string>>() 
                { 
                    { string.Empty, new List<string> { "margin-bottom-small" } }
                } 
            },
            {
                "userEnteredButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill", "button-color" } }
                }
            },
            {
                "addressChangeTradeAVSButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill" } }
                }
            },
            {
                "addressUseEnteredGroupSequence", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill" } }
                }
            },
            {
                "addressEnteredTradeAVSGroupSequence", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-top-x-small" } }
                }
            },

            // Verify Address PIDL - Page 1
            {
                "addressOptionsGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-top-medium", "alignment-start" } }
                }
            },
            {
                "addressEnteredGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-bottom-medium" } }
                }
            },
            {
                "addressUseButton", new Dictionary<string, IEnumerable<string>>()
                {
                     { string.Empty, new List<string> { "width-fill" } }
                }
            },

            // Verify Address PIDL - Page 2
            {
                "addressDetailsDataGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-top-medium" } }
                }
            },
            {
                "addressBackSaveGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-top-medium" } }
                }
            },
            {
                "addressBackButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill", "margin-end-medium" } }
                }
            },
            {
                "saveButton", new Dictionary<string, IEnumerable<string>>()
                {
                     { string.Empty, new List<string> { "width-fill" } }
                }
            },
            {
                "addressSuggestionMessage", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-bottom-small" } }
                }
            },
            {
                "addressOptionsTradeAVSGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill" } }
                }
            },
            {
                "addressEntered", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "radio-container-align-items-flex-start", "radio-layout-column", "radio-location-before-label", "radio-label-container-marginHorizontal-none" } }
                }
            },
            {
                "addressSuggested", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "radio-container-align-items-flex-start", "radio-layout-column", "radio-location-before-label", "radio-label-container-marginHorizontal-none" } }
                }
            },
            {
                "optionAddress_entered", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-start-small", "width-fill" } }
                }
            },
            {
                "address_type_entered", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-start-small", "width-fill" } }
                }
            },
            {
                "addressChangeTradeAVSV2Group", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-top-small", "margin-bottom-small" } }
                }
            },
            {
                "addressEnteredOnlyTradeAVSGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-top-small" } }
                }
            },
            {
                "addressSuggestedTradeAVSV2", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "radio-container-align-items-flex-start", "radio-layout-column", "radio-location-before-label", "radio-label-container-marginHorizontal-none" } }
                }
            },
            {
                "addressNextButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill" } }
                }
            },
            {
                "addressRecommandationMessage", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-top-medium" } }
                }
            },
            {
                "optionAddress_suggested_0", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-start-small", "width-fill" } }
                }
            },
            {
                "optionAddress_suggested_1", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-start-small", "width-fill" } }
                }
            },
            {
                "optionAddress_suggested_2", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-start-small", "width-fill" } }
                }
            },
            {
                "suggestBlockV2", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill" } }
                }
            },
            {
                "addressNextGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill" } }
                }
            },
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                VerifyAddressStylingAction,
            };
        }

        internal static void VerifyAddressStylingAction(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            foreach (PIDLResource pidl in inputResources)
            {
                FeatureHelper.PassStyleHints(pidl, styleHints, featureContext);

                UpdateRadioOptions(Constants.DisplayHintIds.AddressEntered, pidl);
                UpdateRadioOptions(Constants.DisplayHintIds.AddressSuggested, pidl);
            }
        }

        internal static void UpdateRadioOptions(string radioHintId, PIDLResource pidl)
        {
            var radio = pidl.GetDisplayHintById(radioHintId) as PropertyDisplayHint;
            if (radio?.PossibleOptions != null)
            {
                foreach (var possibleOption in radio.PossibleOptions)
                {
                    var optionContentGroup = possibleOption.Value?.DisplayContent as GroupDisplayHint;

                    if (optionContentGroup != null)
                    {
                        optionContentGroup.StyleHints = new List<string> { "margin-start-small" };
                    }
                }
            }
        }
    }
}
