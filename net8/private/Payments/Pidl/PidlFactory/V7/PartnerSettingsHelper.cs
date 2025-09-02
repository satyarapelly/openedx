// <copyright file="PartnerSettingsHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;

    /// <summary>
    /// This class contains helper functions to suport partner settings
    /// </summary>
    public class PartnerSettingsHelper
    {
        public static bool IsFeatureEnabledUsingPartnerSettings(string featureName, string country, PaymentExperienceSetting setting, string displayCustomizationDetail)
        {
            if (setting?.Features != null)
            {
                FeatureConfig featureConfig;
                setting.Features.TryGetValue(featureName, out featureConfig);

                if (featureConfig != null && featureConfig.DisplayCustomizationDetailEnabled(displayCustomizationDetail))
                {
                    if (featureConfig.ApplicableMarkets == null || featureConfig.ApplicableMarkets.Count == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return featureConfig.ApplicableMarkets.Contains(country, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }

            return false;
        }

        public static bool IsFeatureEnabledUsingPartnerSettings(string featureName, string country, PaymentExperienceSetting setting)
        {
            if (setting?.Features != null)
            {
                FeatureConfig featureConfig;
                setting.Features.TryGetValue(featureName, out featureConfig);

                if (featureConfig != null)
                {
                    if (featureConfig.ApplicableMarkets == null || featureConfig.ApplicableMarkets.Count == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return featureConfig.ApplicableMarkets.Contains(country, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }

            return false;
        }

        public static bool IsFeatureEnabledUsingPartnerSettings(string featureName, string country, PaymentExperienceSetting setting, Dictionary<string, object> displayCustomizationDetails)
        {
            if (setting?.Features != null)
            {
                FeatureConfig featureConfig;
                setting.Features.TryGetValue(featureName, out featureConfig);

                if (featureConfig != null && featureConfig.IsDisplayCustomizationDetailEnabledForFeature(featureConfig, displayCustomizationDetails))
                {
                    if (featureConfig.ApplicableMarkets == null || featureConfig.ApplicableMarkets.Count == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return featureConfig.ApplicableMarkets.Contains(country, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }

            return false;
        }

        // Ensure that any modifications to the Features class are documented in doc-feature-list.md to maintain a record of active features.
        public static class Features
        {
            // Enable SMD for EU markets
            public const string SingleMarketDirective = "singleMarketDirective";

            // Enable india 3ds challenge for handlePaymentChallenge
            public const string ThreeDSOne = "threeDSOne";

            // To show the card information for handle purchase risk
            public const string ShowCardInformationInChallenge = "showCardInformationInChallenge";

            // Enables to support PSD2IgnorePIAuthorizationPartners flow
            public const string PSD2 = "psd2";

            // Enabling the PXEnabledNoSubmitIfGSTIDEmpty flow through the feature
            public const string NoSubmitIfGSTIDEmpty = "noSubmitIfGSTIDEmpty";

            // To add new payment option
            public const string AddNewPaymentMethodOption = "addNewPaymentMethodOption";

            // Enables redirection URL into the back button pay pal flow
            public const string ShowRedirectURLInIframe = "showRedirectURLInIframe";

            // Enable / update fields for address toggle
            public const string EnableConditionalFieldsForBillingAddress = "enableConditionalFieldsForBillingAddress";

            // Enables hide/show address group by using a checkbox in Add CC flow
            public const string UseListModernResource = "useListModernResource";

            // Enables address validation and emission on focus out
            public const string EnablePaymentRequestAddressValidation = "enablePaymentRequestAddressValidation";

            // Enable to use client side prefill pidl
            public const string UseClientSidePrefill = "useClientSidePrefill";

            // Applies style hints specific to windows partner
            public const string AddVenmoForWindows = "addVenmoForWindows";

            // Enable a new layout for the global PI QR code flow & moves navigate action to a move last action
            public const string UseIFrameForPiLogOn = "useIFrameForPiLogOn";

            // Short URL for PayPal QR
            public const string ShortURLPaypal = "shortURLPaypal";
            public const string ShortURLTextPaypal = "shortURLTextPaypal";

            // Enable short url for qrcodes (paypal, venmo)
            public const string EnableShortURL = "enableShortURL";

            // This feature enables the virtual family paymentMethods
            public const string EnableVirtualFamilyPM = "enableVirtualFamilyPM";

            // Enabling addResource for global PI
            public const string EnableGlobalPiInAddResource = "enableGlobalPiInAddResource";

            // To create virtual payment method family in standard template
            public const string UpdatePIaddressToAccount = "updatePIaddressToAccount";

            // Only use displayText in the select option (by removing displayContent) for JS or classic element factory which can't render displayContent properly
            public const string UseTextOnlyForPaymentOption = "useTextOnlyForPaymentOption";

            // Enable ListAddress flow
            public const string SelectInstanceForAddress = "selectInstanceForAddress";

            // Enable v3 billing address pidls
            public const string UseV3AddressPIDL = "useV3AddressPIDL";

            // Use profileType + prerequisitesV3 form instead of profileType + prerequisites while adding/updating pi
            public const string UseProfilePrerequisitesV3 = "useProfilePrerequisitesV3";

            // Enables submit link for add adress with validation xpay
            public const string AddressValidation = "addressValidation";
            public const string UseAddressesExSubmit = "useAddressesExSubmit";

            // Allows Visa and Mastercard in China. (Only for commercial and legacy consumer partners)
            public const string ChinaAllowVisaMasterCard = "chinaAllowVisaMasterCard";

            // Adds stylehints to redeem PIDL
            public const string RedeemGiftCard = "redeemGiftCard";

            // Prevents the default selection of AddNewPaymentMethod when PI list is empty
            public const string PreventAddNewPaymentMethodDefaultSelection = "preventAddNewPaymentMethodDefaultSelection";

            // Enables Short URL for  Venmo QR
            public const string ShortURLVenmo = "shortURLVenmo";
            public const string ShortURLTextVenmo = "shortURLTextVenmo";

            // Use the OMS Transaction Service Store in india3DS validation parameters
            public const string UseOmsTransactionServiceStore = "useOmsTransactionServiceStore";

            // Use the AzureTransaction Service Store in india3DS validation parameters
            public const string UseAzureTransactionServiceStore = "useAzureTransactionServiceStore";

            // To enable BillingGroup resource and address type
            public const string AddUpdatePartnerActionToEditProfileHyperlink = "addUpdatePartnerActionToEditProfileHyperlink";
            public const string AddSelectResourcePartnerActionToBillingGroupAddPi = "addSelectResourcePartnerActionToBillingGroupAddPi";
            public const string AddPartnerActionToBillingGroupAddAndUpdate = "addPartnerActionToBillingGroupAddAndUpdate";

            // Used to override display name of check pm to "Wire Transfer"
            public const string OverrideCheckDisplayNameToWireTransfer = "overrideCheckDisplayNameToWireTransfer";

            // Used to implement SelectSingleInstance
            public const string EnableSingleInstancePidls = "enableSingleInstancePidls";

            // Enables the SinglePiDisplayPidl for selectinstance/selectSingleinstance operation flow
            public const string EnableSelectSingleInstancePiDisplay = "enableSelectSingleInstancePiDisplay";

            public const string UseDisabledPIsForSelectInstance = "useDisabledPIsForSelectInstance";

            // Enables AddHapiSUADisabledTaxResourceId
            public const string AddHapiSUADisabledTaxResourceId = "addHapiSUADisabledTaxResourceId";

            // Use jarvis v3 for profile flows requiring jarvis
            public const string UseJarvisV3ForProfile = "useJarvisV3ForProfile";

            // Use jarvis v3 for address flows requiring jarvis
            public const string UseJarvisV3ForAddress = "useJarvisV3ForAddress";

            // Enables to include culture and language transformation
            public const string EnableCultureAndLanguageTransformation = "enableCultureAndLanguageTransformation";
            public const string OverrideLinkedProfileCultureAndLanguageTransformation = "overrideLinkedProfileCultureAndLanguageTransformation";

            // To enable added for CreateLegacyAccountAndSync for address shipping_v3 and scenario ProfileAddress
            public const string UseLegacyAccountAndSync = "useLegacyAccountAndSync";

            // To disable prefill user data
            public const string DisablePrefillUserData = "disablePrefillUserData";

            // Multiple options available for customizing address forms like removing DisplayHint AddressSuggestionMessage / Change SuggestedAddressText text to "We suggest:"
            public const string CustomizeAddressForm = "customizeAddressForm";

            // Changes the partner name to mapped partner name in URL while sending request to PIMS
            public const string ChangePartnerNameForPims = "changePartnerNameForPims";

            // These inline feature is added for the profile type organization as replacement for the feature flight - PXProfileUpdateToHapi
            public const string UseProfileUpdateToHapi = "useProfileUpdateToHapi";

            // These inline feature is added for the profile type employee as replacement for the feature flight - PXProfileUpdateToHapi
            public const string UseEmployeeProfileUpdateToHapi = "useEmployeeProfileUpdateToHapi";

            // If a partner enables multiple profiles and doesn’t pass
            public const string UseMultipleProfile = "useMultipleProfile";

            // To use if server side prefill if not existing
            public const string UseServerSidePrefill = "useServerSidePrefill";

            // To enable the klarna checkout
            public const string EnableKlarnaCheckout = "enableKlarnaCheckout";

            // To disable country in TaxID form
            public const string PXTaxIdFormSkipSecondaryResourceContext = "PXTaxIdFormSkipSecondaryResourceContext";

            // To return BackupPidl for split payment supported in listpi
            public const string ReturnBackupPidlForSplitPaymentSupported = "returnBackupPidlForSplitPaymentSupported";

            // Enable to show the card logo in update cc cardnumber textbox
            public const string EnableUpdateCCLogo = "enableUpdateCCLogo";

            // This feature sets the challenge type in all instances where ValidatePIOnAttachEnabledPartners is used.
            // It will set the challenge type to ValidatePIOnAttachChallenge or PSD2Challenge.If the challenge status fails, it will set the error codes to ValidatePIOnAttachFailed.
            public const string PXUsePSSToEnableValidatePIOnAttachChallenge = "PXUsePSSToEnableValidatePIOnAttachChallenge";

            // To enable payment collection functionality for payment client.
            public const string PaymentClientHandlePaymentCollection = "paymentClientHandlePaymentCollection";

            // Feature for the configuration of components used in paymentClient.
            public const string ComponentSetting = "componentSetting";

            // Enables use of express checkout HTML files with inline JS
            public const string PXUseInlineExpressCheckoutHtml = "PXUseInlineExpressCheckoutHtml";

            // Enables use of express checkout static resources from int
            public const string PXExpressCheckoutUseIntStaticResources = "PXExpressCheckoutUseIntStaticResources";

            // Enables use of express checkout static resources from prod
            public const string PXExpressCheckoutUseProdStaticResources = "PXExpressCheckoutUseProdStaticResources";

            // Removes zero balance stored_value PIs from the available payment options
            public const string RemoveZeroBalanceCsv = "removeZeroBalanceCsv";

            // Changes expiry month and year. e.g. dropdown to single expiry date textbox
            public const string CombineExpiryMonthYearToDateTextBox = "combineExpiryMonthYearToDateTextBox";

            // Enables the two static page redirection for sepa
            public const string UseTwoStaticPageRedirection = "useTwoStaticPageRedirection";
        }
    }
}