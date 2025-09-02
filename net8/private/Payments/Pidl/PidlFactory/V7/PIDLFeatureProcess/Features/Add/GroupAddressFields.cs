// <copyright file="GroupAddressFields.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7; 

    /// <summary>
    /// Used for grouping address fields on add creditcard form
    /// </summary>
    internal class GroupAddressFields : IFeature
    {
        private static readonly List<string> addressFields = new List<string>()
        {
            // For Billing, Internal Address
            Constants.AddressProperty.AddressLine1,
            Constants.AddressProperty.AddressLine2,
            Constants.AddressProperty.AddressLine3,
            Constants.AddressProperty.AddressCity,
            Constants.GroupDisplayHintIds.AddressStatePostalCodeGroup,
            Constants.GroupDisplayHintIds.AddressPostalCodeStateGroup,
            Constants.GroupDisplayHintIds.AddressProvincePostalCodeGroup,
            Constants.GroupDisplayHintIds.AddressPostalCodeProvinceGroup,
            Constants.GroupDisplayHintIds.AddressPostalCodeGroup,
            Constants.DisplayHintIds.AddressState,
            Constants.AddressProperty.AddressProvince,
            Constants.AddressProperty.AddressPostalCode,
            Constants.DisplayHintIds.AddressCountry,
            Constants.DisplayHintIds.AddressCounty,

            // for Klarna Address
            Constants.DisplayHintIds.KlarnaAddressLine1,
            Constants.DisplayHintIds.KlarnaAddressLine2,
            Constants.DisplayHintIds.KlarnaAddressPostalCode,

            // For Address hapiV1ModernAccountV20190531Address
            Constants.DisplayHintIds.HapiAddressLine1,
            Constants.DisplayHintIds.HapiAddressLine2,
            Constants.DisplayHintIds.HapiAddressLine3,
            Constants.DisplayHintIds.HapiV1ModernAccountV20190531AddressCity,
            Constants.GroupDisplayHintIds.HapiV1ModernAccountV20190531AddressRegionAndPostalCodeGroup,
            Constants.GroupDisplayHintIds.HapiV1ModernAccountV20190531AddressRegionGroup,
            Constants.GroupDisplayHintIds.HapiV1ModernAccountV20190531AddressPostalCodeGroup,
            Constants.DisplayHintIds.HapiCountry,
            Constants.DisplayHintIds.HapiEmail,
            Constants.DisplayHintIds.HapiPhoneNumber,
            Constants.DisplayHintIds.HapiCompanyName,
            Constants.DisplayHintIds.HapiIndividualCompanyName,
            Constants.DisplayHintIds.HapiIndividualFirstName,
            Constants.DisplayHintIds.HapiIndividualLastName,
            Constants.GroupDisplayHintIds.HapiV1ModernAccountV20190531AddressPostalCodeAndRegionGroup,
            Constants.GroupDisplayHintIds.HapiFirstNameLastNameGroup,
            Constants.GroupDisplayHintIds.HapiV1ModernAccountV20190531IndividualAddressFirstAndLastNameGroup,

            // For address legalentity
            Constants.DisplayHintIds.UpdateLegalEntityAddressLine1,
            Constants.DisplayHintIds.UpdateLegalEntityAddressLine2,
            Constants.DisplayHintIds.UpdateLegalEntityAddressLine3,
            Constants.DisplayHintIds.UpdateProfileAddressCity,
            Constants.DisplayHintIds.UpdateProfileAddressState,
            Constants.DisplayHintIds.UpdateLegalEntityAddressPostalCode,
            Constants.DisplayHintIds.UpdateProfileAddressCountry,
            Constants.DisplayHintIds.UpdateProfileAddressCounty,
            Constants.DisplayHintIds.UpdateProfileAddressProvince,

            // For shipping_patch
            Constants.DisplayHintIds.AddressFirstName,
            Constants.DisplayHintIds.AddressMiddleName,
            Constants.DisplayHintIds.AddressLastName,
            Constants.DisplayHintIds.AddressPhoneNumber,
            Constants.DisplayHintIds.AddressStateGroup,
            Constants.DisplayHintIds.UpdateProfileAddressLine1,
            Constants.DisplayHintIds.UpdateProfileAddressLine2,
            Constants.DisplayHintIds.UpdateProfileAddressLine3,
            Constants.DisplayHintIds.UpdateProfileAddressPostalCode,

            // For orgAddressModern
            Constants.DisplayHintIds.OrgAddressModernAddressLine1,
            Constants.DisplayHintIds.OrgAddressModernAddressLine2,
            Constants.DisplayHintIds.OrgAddressModernAddressLine3,
            Constants.DisplayHintIds.OrgAddressModernCity,
            Constants.DisplayHintIds.OrgAddressModernRegion,
            Constants.DisplayHintIds.OrgAddressModernPostalCode,
            Constants.DisplayHintIds.OrgAddressModernCountry,
            Constants.DisplayHintIds.OrgAddressModernEmail,
            Constants.DisplayHintIds.OrgAddressModernPhoneNumber,

            // For Address SoldTo
            Constants.DisplayHintIds.AddressCorrespondenceName,
            Constants.DisplayHintIds.AddressMobile,
            Constants.DisplayHintIds.AddressFax,
            Constants.DisplayHintIds.AddressTelex,
            Constants.DisplayHintIds.AddressEmailAddress,
            Constants.DisplayHintIds.AddressWebSiteUrl,
            Constants.DisplayHintIds.AddressStreetSupplement,
            Constants.DisplayHintIds.AddressIsWithinCityLimits,
            Constants.DisplayHintIds.AddressFormOfAddress,
            Constants.DisplayHintIds.AddressAddressNotes,
            Constants.DisplayHintIds.AddressTimeZone,
            Constants.DisplayHintIds.AddressLatitude,
            Constants.DisplayHintIds.AddressLongitude,

            // For Address HapiSua
            Constants.DisplayHintIds.HapiSUALine1,
            Constants.DisplayHintIds.HapiSUALine2,
            Constants.DisplayHintIds.HapiSUALine3,
            Constants.DisplayHintIds.HapiSUACity,
            Constants.DisplayHintIds.HapiSUAState,
            Constants.DisplayHintIds.HapiSUAPostalCode,
            Constants.DisplayHintIds.HapiSUACountryCode,
            Constants.DisplayHintIds.HapiSUACounty,
            Constants.DisplayHintIds.HapiSUAPhoneNumber,
            Constants.DisplayHintIds.HapiSUAProvince,

            // For Shipping_v3, shippingAddressHardware
            Constants.DisplayHintIds.ShippingAddressState,
            Constants.DisplayHintIds.AddressCountyGB,
            Constants.DisplayHintIds.AddressFirstNameOptional,
            Constants.DisplayHintIds.AddressLastNameOptional,
            Constants.DisplayHintIds.AddressEmailOptional,
            Constants.DisplayHintIds.AddressPhoneNumberOptional,
            Constants.DisplayHintIds.AddressPhoneNumberWithExplanation,
            Constants.DisplayHintIds.ShippingAddressLine1,
            Constants.DisplayHintIds.ShippingAddressLine2,
            Constants.DisplayHintIds.ProfileAddressFirstName,
            Constants.DisplayHintIds.ProfileAddressLastName,
            Constants.DisplayHintIds.ProfileAddressLine1,
            Constants.DisplayHintIds.ProfileAddressLine2,
            Constants.DisplayHintIds.ProfileAddressLine3,
            Constants.DisplayHintIds.ProfileAddressCity,
            Constants.DisplayHintIds.ProfileAddressState,
            Constants.DisplayHintIds.ProfileAddressPostalCode,
            Constants.DisplayHintIds.ProfileAddressPhoneNumber,
            Constants.DisplayHintIds.ProfileAddressCounty,
            Constants.DisplayHintIds.ProfileAddressProvince
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ProcessGroupAddressFields,
            };
        }

        internal static void ProcessGroupAddressFields(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            foreach (PIDLResource pidl in inputResources)
            {
                if (pidl?.DisplayPages == null)
                {
                    continue;
                }

                foreach (PageDisplayHint pidlDisplayPage in pidl.DisplayPages)
                {
                    DisplayHint addressGroup = pidlDisplayPage?.Members
                        .FirstOrDefault(displayHint => displayHint.HintId.Equals(Constants.DisplayHintIds.AddressGroup, StringComparison.OrdinalIgnoreCase));

                    if (addressGroup == null)
                    {
                        var newAddressGroup = new GroupDisplayHint
                        {
                            DisplayHintType = Constants.DisplayHintTypes.Group,
                            DisplayName = Constants.DisplayNames.BillingAddress,
                            ShowDisplayName = "false",
                            HintId = Constants.DisplayHintIds.AddressGroup,
                            StyleHints = new List<string>() { Constants.StyleHints.GapSmall },
                        };

                        newAddressGroup.AddDisplayTags(new Dictionary<string, string>() { { Constants.DisplayTags.AddressGroup, Constants.DisplayTags.AddressGroup } });

                        // Using SortedDictionary to retain the original order of address fields
                        SortedDictionary<int, DisplayHint> addressFieldsWithOrder = new SortedDictionary<int, DisplayHint>();

                        int index = 0;
                        foreach (DisplayHint displayHint in pidlDisplayPage.Members)
                        {
                            if (addressFields.Contains(displayHint.HintId, StringComparer.OrdinalIgnoreCase))
                            {
                                addressFieldsWithOrder.Add(index, displayHint);
                            }

                            index++;
                        }

                        if (addressFieldsWithOrder.Count > 0)
                        {
                            foreach (var addressField in addressFieldsWithOrder.Values)
                            {
                                newAddressGroup.Members.Add(addressField);
                                pidlDisplayPage.RemoveDisplayHint(addressField);
                            }

                            pidlDisplayPage.Members.Insert(addressFieldsWithOrder.Keys.First(), newAddressGroup);
                        }
                    }
                }
            }
        }
    }
}
