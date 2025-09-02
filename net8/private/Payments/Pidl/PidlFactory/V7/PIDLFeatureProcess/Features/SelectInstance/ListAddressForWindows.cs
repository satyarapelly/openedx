// <copyright file="ListAddressForWindows.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    internal class ListAddressForWindows : IFeature
    {
        private static Dictionary<string, Dictionary<string, IEnumerable<string>>> styleHints = new Dictionary<string, Dictionary<string, IEnumerable<string>>>()
        {
            {
                "cancelButton", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "width-fill" } }
                }
            },
            {
                "cancelGroup", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "dock-bottom" } }
                }
            }
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ProcessListAddressForWindows,
            };
        }

        internal static void ProcessListAddressForWindows(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (featureContext == null || featureContext.ResourceType != Constants.ResourceTypes.Address)
            {
                return;
            }

            foreach (PIDLResource pidl in inputResources)
            {
                FeatureHelper.PassStyleHints(pidl, styleHints, featureContext);
                List<DisplayHint> listAddresses = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(pidl, "listAddresses");
                List<DisplayHint> optionAddresses = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(pidl, "optionAddress_");

                foreach (DisplayHint hint in listAddresses)
                {
                    PropertyDisplayHint listAddress = hint as PropertyDisplayHint;
                    if (listAddress != null && listAddress.PossibleOptions?.Count > 0)
                    {
                        Dictionary<string, SelectOptionDescription> possibleOptions = listAddress.PossibleOptions;
                        foreach (KeyValuePair<string, SelectOptionDescription> option in possibleOptions)
                        {
                            if (option.Value != null)
                            {
                                option.Value.StyleHints = new List<string> { "height-auto", "padding-small" };
                            }

                            TextDisplayHint addNewAddress = PIDLResourceDisplayHintFactory.GetDisplayHintById(option.Value.DisplayContent, "add_new_address") as TextDisplayHint;
                            if (addNewAddress != null)
                            {
                                option.Value.StyleHints = option.Value.StyleHints.Concat(new List<string> { "display-button-primary" }).ToList();
                            }
                            else
                            {
                                option.Value.StyleHints = option.Value.StyleHints.Concat(new List<string> { "alignment-start", "selection-highlight-blue", "selection-highlight-border-accent" });
                            }
                        }
                    }
                }

                // Remove the add new address Image from add new address option and useThisAddress from all options
                foreach (DisplayHint displayHint in optionAddresses)
                {
                    GroupDisplayHint optionAddress = displayHint as GroupDisplayHint;
                    if (optionAddress != null)
                    {
                        ImageDisplayHint addNewAddressImage = PIDLResourceDisplayHintFactory.GetDisplayHintById(optionAddress, "addNewAddressLink") as ImageDisplayHint;
                        if (addNewAddressImage != null)
                        {
                            optionAddress.Members.Remove(addNewAddressImage);
                        }

                        GroupDisplayHint useThisAddressGroup = PIDLResourceDisplayHintFactory.GetDisplayHintById(optionAddress, "useThisAddressGroup_") as GroupDisplayHint;
                        if (useThisAddressGroup != null)
                        {
                            TextDisplayHint useThisAddressText = PIDLResourceDisplayHintFactory.GetDisplayHintById(useThisAddressGroup, "useThisAddressText_") as TextDisplayHint;
                            if (useThisAddressText != null)
                            {
                                useThisAddressGroup.Members.Remove(useThisAddressText);
                            }
                        }
                    }
                }
            }
        }
    }
}
