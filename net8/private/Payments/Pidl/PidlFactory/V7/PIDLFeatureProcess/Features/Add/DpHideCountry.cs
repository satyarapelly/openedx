// <copyright file="DpHideCountry.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the DpHideCountry, which is to hide country in PIDL form.
    /// </summary>
    internal class DpHideCountry : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                HideCountryFromDisplay
            };
        }

        internal static void HideCountryFromDisplay(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (string.Equals(featureContext.ResourceType, Constants.ResourceTypes.TaxId, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource pidlResource in inputResources)
                {
                    // Hide the state field for India, which is in the linked Pidls.
                    if (string.Equals(featureContext.Country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase) && pidlResource?.LinkedPidls != null)
                    {
                        foreach (PIDLResource linkedPidl in pidlResource?.LinkedPidls)
                        {
                            if (linkedPidl != null)
                            {
                                SetIsHiddenPropertyByDisplayHint(linkedPidl, Constants.DisplayHintIds.AddressState, true);
                            }
                        }
                    }
                    else if (string.Equals(featureContext.Country, Constants.CountryCodes.Italy, StringComparison.OrdinalIgnoreCase))
                    {
                        SetIsHiddenPropertyByDisplayHint(pidlResource, Constants.DisplayHintIds.HapiTaxCountryProperty, true);

                        foreach (PIDLResource linkedPidl in pidlResource?.LinkedPidls)
                        {
                            if (linkedPidl != null)
                            {
                                SetIsHiddenPropertyByDisplayHint(linkedPidl, Constants.DisplayHintIds.HapiTaxCountryProperty, true);
                            }
                        }
                    }

                    if (string.Equals(featureContext.Scenario, Constants.ScenarioNames.WithCountryDropdown, StringComparison.OrdinalIgnoreCase) || string.Equals(featureContext.Scenario, Constants.ScenarioNames.DepartmentalPurchase, StringComparison.OrdinalIgnoreCase))
                    {
                        SetIsHiddenPropertyByDisplayHint(pidlResource, Constants.DisplayHintIds.HapiTaxCountryProperty, true);

                        // If the PSS feature DpHideCountry is enabled and the flight DpHideCountry is not present,
                        // then the logic to hide SaveButton, CancelButton, and SaveButtonSuccess will execute, which is not expected for the feature DpHideCountry.
                        // To solve this issue, added the logic below to not hide SaveButton, CancelButton, and SaveButtonSuccess.
                        SetIsHiddenPropertyByDisplayHint(pidlResource, Constants.ButtonDisplayHintIds.SaveButton, false);
                        SetIsHiddenPropertyByDisplayHint(pidlResource, Constants.ButtonDisplayHintIds.CancelButton, false);
                        SetIsHiddenPropertyByDisplayHint(pidlResource, Constants.ButtonDisplayHintIds.SaveButtonSuccess, false);
                    }
                }
            }
        }

        private static void SetIsHiddenPropertyByDisplayHint(PIDLResource resource, string hintId, bool isHidden)
        {
            DisplayHint displayHint = resource.GetDisplayHintById(hintId);
            if (displayHint != null)
            {
                displayHint.IsHidden = isHidden;
            }
        }
    }
}