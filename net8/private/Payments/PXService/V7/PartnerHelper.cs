// <copyright file="PartnerHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PXCommon;

    internal static class PartnerHelper
    {
        internal static bool IsAzurePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.Azure, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerName.AzureSignup, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerName.AzureIbiza, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsAzureBasedPartner(string partnerName)
        {
            return IsAzurePartner(partnerName)
                || string.Equals(partnerName, Constants.PartnerName.AppSource, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsCommercialStoresPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.CommercialStores, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsMarketPlacePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.MarketPlace, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsGGPDEDSPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.GGPDEDS, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsOneDrivePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.OneDrive, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsBingPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.Bing, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsPayinPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.Payin, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsSetupOfficePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.SetupOffice, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsStoreOfficePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.StoreOffice, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsNorthStarWebPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.NorthStarWeb, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsOfficeOobePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.OfficeOobe, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsOXOOobePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.OXOOobe, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsSmbOobePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.SmbOobe, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsCartPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.Cart, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsOXOWebDirectPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.OXOWebDirect, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsIndiaThreeDSCommercialPartner(string partnerName)
        {
            return string.Equals(partnerName, V7.Constants.PartnerName.Azure, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, V7.Constants.PartnerName.CommercialStores, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsWindowsStorePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.WindowsStore, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsThreeDSOneQrCodeBasedPurchasePartner(string partnerName, PaymentExperienceSetting setting = null)
        {
            if (setting != null && setting.RedirectionPattern != null)
            {
                return setting.RedirectionPattern.Equals(V7.Constants.RedirectionPatterns.QRCode, StringComparison.InvariantCultureIgnoreCase);
            }

            return string.Equals(partnerName, V7.Constants.PartnerName.Storify, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, V7.Constants.PartnerName.Saturn, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, V7.Constants.PartnerName.XboxSubs, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, V7.Constants.PartnerName.Xbox, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, V7.Constants.PartnerName.AmcXbox, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, V7.Constants.PartnerName.Xbet, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsThreeDSOneIframeBasedPartner(string partnerName, PaymentExperienceSetting setting = null)
        {
            if (setting != null && setting.RedirectionPattern != null)
            {
                return setting.RedirectionPattern.Equals(V7.Constants.RedirectionPatterns.IFrame, StringComparison.InvariantCultureIgnoreCase);
            }

            return string.Equals(partnerName, V7.Constants.PartnerName.OfficeOobe, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, V7.Constants.PartnerName.OXOOobe, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, V7.Constants.PartnerName.Payin, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, V7.Constants.PartnerName.WebPay, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, V7.Constants.PartnerName.ConsumerSupport, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, V7.Constants.PartnerName.XboxWeb, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, V7.Constants.PartnerName.SetupOffice, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, V7.Constants.PartnerName.SetupOfficeSdx, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsIndiaThreeDSAddPIRedirectionInNewPagePartner(string partnerName, PaymentExperienceSetting setting = null)
        {
            if (setting != null && setting.RedirectionPattern != null)
            {
                return setting.RedirectionPattern.Equals(V7.Constants.RedirectionPatterns.FullPage, StringComparison.InvariantCultureIgnoreCase);
            }

            return PartnerHelper.IsCartPartner(partnerName) || PartnerHelper.IsOXOWebDirectPartner(partnerName);
        }

        internal static bool IsPaypayQrCodeBasedAddPIPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerName.Xbox, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(partnerName, Constants.PartnerName.AmcXbox, StringComparison.OrdinalIgnoreCase) ||
                PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName);
        }

        internal static bool IsIndiaThreeDSFlightedInlinePartner(string partnerName, PaymentExperienceSetting setting = null)
        {
            if (setting != null && setting.RedirectionPattern != null)
            {
                return setting.RedirectionPattern.Equals(V7.Constants.RedirectionPatterns.Inline, StringComparison.InvariantCultureIgnoreCase);
            }

            // These partners will have the bank page open in the same page where the cvv was submited
            return string.Equals(partnerName, V7.Constants.PartnerName.WebblendsInline) ||
                string.Equals(partnerName, V7.Constants.PartnerName.Cart) ||
                string.Equals(partnerName, V7.Constants.PartnerName.AmcWeb, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool IsClientSideListPIPrefillRequired(string partner)
        {
            return PartnerHelper.IsCommercialStoresPartner(partner)
                || PartnerHelper.IsMarketPlacePartner(partner)
                || PartnerHelper.IsAzureBasedPartner(partner)
                || PartnerHelper.IsGGPDEDSPartner(partner)
                || PartnerHelper.IsPayinPartner(partner)
                || PartnerHelper.IsOneDrivePartner(partner)
                || PartnerHelper.IsSetupOfficePartner(partner)
                || PartnerHelper.IsStoreOfficePartner(partner)
                || PartnerHelper.IsNorthStarWebPartner(partner)
                || PartnerHelper.IsWindowsStorePartner(partner);
        }

        internal static bool IsValidatePIOnAttachEnabled(string partner, List<string> exposedFlightFeatures = null)
        {
            // Remove the ValidatePIOnAttachEnabledPartners conditions once the flight is fully deployed or the migration of partners under ValidatePIOnAttachEnabledPartners is completed.
            return V7.Constants.ValidatePIOnAttachEnabledPartners.Any(p => string.Equals(p, partner, StringComparison.OrdinalIgnoreCase))
                || (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXUsePSSToEnableValidatePIOnAttachChallenge, StringComparer.OrdinalIgnoreCase));
        }
    }
}