// <copyright file="Flighting.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System.Collections.Generic;

    /// <summary>
    /// This class wraps All feature names and context keys in one place
    /// </summary>
    public static class Flighting
    {
        // These should match feature names created on the carbon flight tower website.
        public static class Features
        {
            // Used to flight adding resourceActionContext to options in select PM
            public const string PXSelectPMInvokeResourceAction = "PXSelectPMInvokeResourceAction";

            // Used to flight the merged implementation for select PM dropdowns to buttonlist
            public const string PXSelectPMDropdownButtonListMerge = "PXSelectPMDropdownButtonListMerge";

            // Used to disable ACH as PaymentInstrument for all markets
            public const string PXDisableAch = "PXDisableAch";

            // Used to disable SEPA as PaymentInstrument for Germany and Austria markets for configured partners
            public const string PXDisableSepaATDE = "PXDisableSepaATDE";

            // Used to disable SEPA as PaymentInstrument for NOT Germany and Austria markets for configured partners
            public const string PXDisableSepaNonATDE = "PXDisableSepaNonATDE";

            // Used to flight add cards operation (for both guest and signed-in shoppers)
            public const string PXAddCard = "PXAddCard";

            // Used to flight add Device Fingerprint Iframe to BattleNet's AddCC
            public const string PXPaasAddCCDfpIframe = "PXPaasAddCCDfpIframe";

            // Used to flight add Device Fingerprint Iframe to AddCC
            public const string PXAddCCDfpIframe = "PXAddCCDfpIframe";

            // Used to flight add Device Fingerprint Iframe to Confirm PIDL
            public const string PXConfirmDfpIframe = "PXConfirmDfpIframe";

            // Used to flight list cards (for signed-in users)
            public const string PXListCards = "PXListCards";

            // Used to override HasAnyPI fals to True during rewards redemption
            public const string PXOverrideHasAnyPIToTrue = "PXOverrideHasAnyPIToTrue";

            // Used to flight the "v-next" header to PIMS when sending a request with the "GetPaymentMethods" action. The resulting response from PIMS depends on what PIMS currently flights under "v-next"
            public const string VNextToPIMS = "VNextToPIMS";

            // Used to flight the usage of V3 profiles for the CompletePrerequisites flow for PaymentMethods
            public const string PXUseJarvisV3ForCompletePrerequisites = "PXUseJarvisV3ForCompletePrerequisites";

            // Used to flight the update employee/organization profile to Hapi service
            public const string PXProfileUpdateToHapi = "PXProfileUpdateToHapi";

            // Used to flight the update employee profile to Hapi service
            // The flight name will stay, since it is moved to central local configuration
            public const string PXEmployeeProfileUpdateToHapi = "PXEmployeeProfileUpdateToHapi";

            // Used to flight certain languages as RTL
            public const string PXRtlLanguages = "PXRtlLanguages";

            // Used to flight ltrinrtl tag for accountToken property in PIDLs for RTL languages
            public const string PXAddltrinrtlTag = "PXAddltrinrtlTag";

            // Used to flight the use of aligned version of some of the logos depending on whether the language is LTR (standard) or RTL
            public const string PXUseAlignedLogos = "PXUseAlignedLogos";

            // Used to flight the usage of a SubmitLinks csv to append submit links to pidls instead of defining it by code path and hard coded logic
            public const string PXEnableCSVSubmitLinks = "PXEnableCSVSubmitLinks";

            // Used to enable the redeem gift card(csv) feature
            public const string PXEnableRedeemCSVFlow = "PXEnableRedeemCSVFlow";

            // Used to enabled post processing features for remove PI operation
            public const string PXUsePostProcessingFeatureForRemovePI = "PXUsePostProcessingFeatureForRemovePI";

            // Used to create a PaymentInstrumentSession session
            public const string PXEnablePSD2PaymentInstrumentSession = "PXEnablePSD2PaymentInstrumentSession";

            // Used to flight the usage of SearchTransactionParallelRequest for optimization of code
            public const string PXEnableSearchTransactionParallelRequest = "PXEnableSearchTransactionParallelRequest";

            // Used to flight the add/update PI to pass ip address to PIMS
            // NOTE: Per carbon flighting decomissioning, we're leaving this flight on purpose since it's only being whe request is from corpnet
            public const string PXPassIpAddressToPIMSForAddUpdatePI = "PXPassIpAddressToPIMSForAddUpdatePI";

            // Used to flight the add/update PI to pass user agent to PIMS
            // NOTE: Per carbon flighting decomissioning, we're leaving this flight on purpose since it's only being whe request is from corpnet
            public const string PXPassUserAgentToPIMSForAddUpdatePI = "PXPassUserAgentToPIMSForAddUpdatePI";

            // Used to flight the pds2 prod integeration
            public const string PXPSD2ProdIntegration = "PXPSD2ProdIntegration";

            // Used to enable 0INR no challenge flow for webblends in India market
            public const string PXSkipChallengeForZeroAmountIndiaAuth = "PXSkipChallengeForZeroAmountIndiaAuth";

            // Used to flight the pds2 CSP Related Proxy Frame, it overcomes the CSP by the partner host web servers
            public const string PXPSD2EnableCSPProxyFrame = "PXPSD2EnableCSPProxyFrame";

            // Used to flight the pds2 CSP Related Proxy Frame with a set URL, it overcomes the CSP by the partner host web servers
            public const string PXPSD2EnableCSPUrlProxyFrame = "PXPSD2EnableCSPUrlProxyFrame";

            // Used to flight the pds2 CSP Related Proxy Frame with a set URL that sets input to DOM after sanitization, it overcomes the CSP by the partner host web servers
            public const string PXPSD2EnableCSPUrlProxyFrameWithSanitizedInput = "PXPSD2EnableCSPUrlProxyFrameWithSanitizedInput";

            // Used to flight the pds2 CSPPostThreeDSMethodData with src parameter
            public const string PXPSD2EnableCSPPostThreeDSMethodDataSrc = "PXPSD2EnableCSPPostThreeDSMethodDataSrc";

            // Used to flight the pds2 CSPPostThreeDSSessionData with src parameter
            public const string PXPSD2EnableCSPPostThreeDSSessionDataSrc = "PXPSD2EnableCSPPostThreeDSSessionDataSrc";

            // Used to disable the variable amount during MS Rewards redempotion
            public const string PXDisableMSRewardsVariableAmount = "PXDisableMSRewardsVariableAmount";

            // Used to flight the update address checkbox in add credit card pidl
            public const string PXIncludeUpdateAddressCheckboxInAddCC = "PXIncludeUpdateAddressCheckboxInAddCC";

            // The change to skip localization of possible_values for paymentMethodFamily and paymentMethodType is being kept under this flight
            public const string PXSelectPMSkipLocalization = "PXSelectPMSkipLocalization";

            // Even if PI extended view had no 3ds2 in requiredChallenges, this flight causes requests to proceed with processing as though 3ds2 was required
            public const string PXPSD2PretendPIMSReturned3DS2 = "PXPSD2PretendPIMSReturned3DS2";
            
            // Force Payer auth to return transStatus = C for challenge requests
            public const string PXPSD2EnforcePreferredChallengeIndicator = "PXPSD2EnforcePreferredChallengeIndicator";

            // Ignores PiCid mismatch error during PSD2 createpaymentsession calls
            public const string PXPSD2IgnorePiCidMismatch = "PXPSD2IgnorePiCidMismatch";

            // Used to flight X-Content-Type-Options: nosniff in the responses from PX
            public const string PXSendContentTypeOptionsHeader = "PXSendContentTypeOptionsHeader";

            // NorthStar PI Expiry override scenario
            public const string PXNSSetExpiry = "PXNSSetExpiry";

            // Compliance - CELA - Use AVS Suggested address with 9-digit
            public const string PXAddressZipCodeUpdateTo9Digit = "PXAddressZipCodeUpdateTo9Digit";

            // If PXAddressZipCodeUpdateTo9Digit is enable, pass "verified" to PIMS.AddPI
            public const string PXAddressZipCodeUpdateTo9DigitPassVerfiedToPIMS = "PXAddressZipCodeUpdateTo9DigitPassVerfiedToPIMS";

            // Used to flight the usage of ShippingV3 profiles for the CompletePrerequisites flow for XAA
            public const string PXUseShippingV3ForCompletePrerequisites = "PXUseShippingV3ForCompletePrerequisites";

            // Used to flight edge endpoint for Tokenization calls
            public const string PXUseEdgeTokenization = "PXUseEdgeTokenization";

            // Used to flight edge endpoint for PIFD calls
            public const string PXUseEdgePIFD = "PXUseEdgePIFD";

            // Used to flight list pending orders from D365 on PaymentTransactions calls
            public const string PXEnableListD365PendingOrders = "PXEnableListD365PendingOrders";

            // Name is set to be obscure on purpose
            public const string PXRateLimitPerAccountOnChallengeApis = "PX802";

            // Name is set to be obscure on purpose
            // Used by POST PI and POST address to block identified bot testing accounts especially in holiday season
            // The flight will be kept until abnormal client sdk or app configuration is ready.
            // It is ok the flight doesn't work after Jan 1 2021
            // Task 36928901: Remove flight PXRateLimitPerAccount
            public const string PXRateLimitPerAccount = "PX9002311";

            // When this is set, PidlSdk will not ignore the errors on linked profile pidl submits and won't proceed with main pidl submit
            public const string PXThrowOnLinkedProfilePidlErrors = "PXThrowOnLinkedProfilePidlErrors";

            // Used to flight x-ms-px-retry-servererr: false in the responses from PX
            public const string PXSendNoRetryOnServerErrorHeader = "PXSendNoRetryOnServerErrorHeader";

            // Used to save user entered address if AVS returns that address is verified and shippable
            public const string PXSkipSuggestedAddressPageIfAVSVerified = "PXSkipSuggestIfVerified";

            public const string PXZip4RemoveDisplayTextAddressID = "PXZip4RemoveDisplayTextAddressID";

            // Enable paypal second screen flow for xbox
            public const string PXEnablePaypalSecondScreenForXbox = "PXEnablePaypalSecondScreenForXbox";

            // Enable MS Rewards Redemption challenge flow
            public const string PXEnableMSRewardsChallenge = "PXEnableMSRewardsChallenge";

            // Enable additional redirect URL text for PayPal as last item before the buttons
            public const string PXEnablePaypalRedirectUrlText = "PXEnablePaypalRedirectUrlText";

            // Enables the SEPA redirect URL text for SEPA payment method on last page for redirection
            public const string PXEnableSepaRedirectUrlText = "PXEnableSepaRedirectUrlText";

            // Update the context of newPaymentMethodLink button in selectInstance flow for payin partner
            public const string UpdateNewPaymentMethodLinkActionContext = "UpdateNewPaymentMethodLinkActionContext";

            // Return 502 for malicious imcoming requests,
            // returning 502 since 500 will trigger pidlsdk retry which we don't want to have for malicious request.
            // Although the PR never be enabled, potentially holiday season we may consider to use it
            // change status code from 503 -> 502, since we need 503 to be set only PX is unavailable and auto restart is needed.
            public const string PXReturn502ForMaliciousRequest = "PXReturn502ForMaliciousRequest";

            // Enable additional redirect URL for DPA
            public const string PXEnableRedirectionV2Url = "PXEnableRedirectionV2Url";

            // Disable 2 page pidl for PayPal 2nd screen QR code page
            public const string PXDisableTwoPagePidlForPaypal2ndScreenQrcodePage = "PXDisableTwoPagePidlForPaypal2ndScreenQrcodePage";

            // Used to override disabled AmEx for IN market
            public const string PXEnableAmexForIN = "PXEnableAmexForIN";

            // Enable LTS and disable STS flow for UpiQR consumer
            public const string EnableLtsUpiQRConsumer = "EnableLtsUpiQRConsumer";

            // Set PayPal 2nd screen polling internal as 5 seconds
            public const string PXSetPayPal2ndScreenPollingIntervalFiveSeconds = "PXSetPayPal2ndScreenPollingIntervalFiveSeconds";

            // Set PayPal 2nd screen polling internal as 10 seconds
            public const string PXSetPayPal2ndScreenPollingIntervalTenSeconds = "PXSetPayPal2ndScreenPollingIntervalTenSeconds";

            // Set PayPal 2nd screen polling internal as 15 seconds
            public const string PXSetPayPal2ndScreenPollingIntervalFifteenSeconds = "PXSetPayPal2ndScreenPollingIntervalFifteenSeconds";

            // PX flighting to remove mandatory fields message
            public const string PXRemoveMandatoryFieldsMessage = "PXRemoveMandatoryFieldsMessage";

            // Partner flighting to use AVS suggestions in address
            public const string ShowAVSSuggestions = "showAVSSuggestions";

            // Partner flighting to use AVS suggestions in address
            public const string ShowSummaryPage = "showSummaryPage";

            // PX flighting to use AVS suggestions in address
            public const string PXEnableAVSSuggestions = "PXEnableAVSSuggestions";

            // For trade AVS, use PidlModal instead of PidlPage
            public const string TradeAVSUsePidlModalInsteadofPidlPage = "TradeAVSUsePidlModalInsteadofPidlPage";

            // For trade AVS, use V2 UX for PIDLPage
            public const string TradeAVSUsePidlPageV2 = "TradeAVSUsePidlPageV2";

            // Use Patch instead Put Javis request for Consumer profile
            public const string UseJarvisPatchForConsumerProfile = "UseJarvisPatchForConsumerProfile";

            // Flight to enable root certificate validation in Payment Sessions
            public const string PXEnablePSD2ServiceSideCertificateValidation = "PXEnablePSD2ServiceSideCertificateValidation";

            // Used to flight India 3DS1 pidls for certain partners
            public const string PXEnableIndia3DS1Challenge = "PXEnableIndia3DS1Challenge";

            // Used to enable Single Market Directive for Commercial partners
            public const string PXEnableSmdCommercial = "PXEnableSmdCommercial";

            // PX flighting to reject malicious accounts based on anomaly detection
            public const string PXEnableMaliciousAccountIdRejection = "PXEnableMaliciousAccountIdRejection";

            // PX flighting to enable rejection for malicious accounts based on anomaly detection
            public const string PXEnableMaliciousAccountIdRejectionEffect = "PXEnableMaliciousAccountIdRejectionEffect";

            // Used to skip the duplicate process process on success call for Moto and Rewards
            public const string PXSkipDuplicatePostProcessForMotoAndRewards = "PXSkipDuplicatePostProcessForMotoAndRewards";

            // PX flighting to reject malicious clients based on anomaly detection
            public const string PXEnableMaliciousClientIPRejection = "PXEnableMaliciousClientIPRejection";

            // PX flighting to enable rejection for malicious clients based on anomaly detection
            public const string PXEnableMaliciousClientIPRejectionEffect = "PXEnableMaliciousClientIPRejectionEffect";

            // Used to enable Xbox Native Delete error pages
            public const string XboxNativeBaseErrorPage = "XboxNativeBaseErrorPage";

            // Used to enable Image Captcha for Malicious users
            public const string PXEnableHIPCaptcha = "PXEnableHIPCaptcha";

            // Used to enable Image and Audio Captcha for Malicious users
            public const string PXEnableHIPCaptchaGroup = "PXEnableHIPCaptchaGroup";

            // used to include a PIDL along with the list of Payment Instruments in the response
            public const string IncludePIDLWithPaymentInstrumentList = "IncludePIDLWithPaymentInstrumentList";

            // used to prevent default selection of Add new Payment Method when PI list is empty
            public const string PXPreventAddNewPaymentMethodDefaultSelection = "PXPreventAddNewPaymentMethodDefaultSelection";

            // Used for attaching View Terms custom event to viewTermsButton
            public const string PXViewTermsTriggerCustomEvent = "PXViewTermsTriggerCustomEvent";

            // Use to enable font icon logos in xboxnative SelectPaymentInstrument flows
            public const string PXUseFontIcons = "PXUseFontIcons";

            // Used for not showing expiry date stored on PI for IN market.
            public const string IndiaExpiryGroupDelete = "IndiaExpiryGroupDelete";

            // Used for showing 'Unknown' expiry date for IN market and Commercialstores partner.
            public const string IndiaCvvChallengeExpiryGroupDelete = "IndiaCvvChallengeExpiryGroupDelete";

            // Used for enabling UPI payment method for IN
            public const string IndiaUPIEnable = "IndiaUPIEnable";

            // Used for enabling UPI payment method for IN
            public const string EnableIndiaTokenExpiryDetails = "EnableIndiaTokenExpiryDetails";

            // Enable 3ds challenge for IN - to control challenge flow even when PIMS sends 3ds in required.
            public const string India3dsEnableForBilldesk = "India3dsEnableForBilldesk";

            // PX flighting to add Jarvis Account Id HMAC
            public const string PXEnableJarvisHMAC = "PXEnableJarvisHMAC";

            // PX flighting to reject malicious requests based on the provided Jarvis Account Id HMAC
            public const string PXEnableJarvisHMACValidation = "PXEnableJarvisHMACValidation";

            // PX flighting to disable baseline check for rate limiting logic
            public const string PXRateLimitDisableBaselineCheck = "PXRateLimitDisableBaselineCheck";

            // PX flighting to disable submisson of GST ID if GST ID is empty
            public const string PXEnabledNoSubmitIfGSTIDEmpty = "PXEnabledNoSubmitIfGSTIDEmpty";

            // PX flighting to enable use of Partner Settings Service
            public const string PXUsePartnerSettingsService = "PXUsePartnerSettingsService";

            // PX flighting to support captcha error code signal from PIMS
            public const string PXSupportCaptchaSignalFromPIMS = "PXSupportCaptchaSignalFromPIMS";

            // PX flighting to enable tax ID collection for Portuguese consumers
            public const string PXEnableTaxIdInPT = "PXEnableTaxIdInPT";

            // PX flighting to add LinkedPidl to collect Tax ID for the newly enabled countries in Zinc
            public const string PXEnableVATID = "PXEnableVATID";

            // PX flighting to add secondary_validation_mode for the modernValidate flow.
            public const string PXEnableSecondaryValidationMode = "PXEnableSecondaryValidationMode";

            // PX flighting when enabled then PX will throw the BadRequest exception for the invalid parameter.
            public const string PXEnableThrowInvalidUrlParameterException = "PXEnableThrowInvalidUrlParameterException";

            // Partner should use alternate svg logos, for example on SelectPM
            public const string PXUseAlternateSVG = "PXUseAlternateSVG";

            // PX flighting to set terminatingErrorHandling to ignore for Secondary Resources when adding PI without existing ProfileAddress
            public const string PXAddSecondaryClientActionWithIgnoreTerminatingErrorHandling = "PXAddSecondaryClientActionWithIgnoreTerminatingErrorHandling";

            // PX flighting to update the stored session with challenge type for PIOnAttachChallenge partners
            public const string PXUpdateStoredSessionWithChallengeType = "PXUpdateStoredSessionWithChallengeType";

            // PX flighting to validate the ChallengeType of the storedSession on Authenticate
            public const string PXAuthenticateChallengeTypeOnStoredSession = "PXAuthenticateChallengeTypeOnStoredSession";

            // Used to route PX service partner settings service to PPE environment
            public const string PXServicePSSPPEEnvironment = "PXServicePSSPPEEnvironment";

            // Used to disable PX cache for Partner Settings Service:
            public const string PXDisablePSSCache = "PXDisablePSSCache";

            // add "syncLegacyAddress=false" to the end of Jarvis v2 profile POSTs
            public const string PXJarvisProfileCallSyncLegacyAddressFalse = "PXJarvisProfileCallSyncLegacyAddressFalse";

            // add rewards points information to ListPI xboxNative flows
            public const string PXEnableXboxNativeListPIRewardsPointsDisplay = "PXEnableXboxNativeListPIRewardsPointsDisplay";

            // add rewards points information to ListPI xboxNative flows
            public const string PXXboxCardApplicationOriginPPEUrl = "PXXboxCardApplicationOriginPPEUrl";

            // Enable webview in fincastle flow
            public const string PXXboxCardApplicationEnableWebview = "PXXboxCardApplicationEnableWebview";

            // Enable short url for fincastle flow
            public const string PXXboxCardApplicationEnableShortUrl = "PXXboxCardApplicationEnableShortUrl";

            // Enable Venmo select pm / list pi
            public const string PXEnableVenmo = "PXEnableVenmo";

            // Enable Venmo for add pi
            public const string PxEnableSelectPMAddPIVenmo = "PxEnableSelectPMAddPIVenmo";

            // Enable short url text for fincastle flow
            public const string PXXboxCardApplicationEnableShortUrlText = "PXXboxCardApplicationEnableShortUrlText";

            // Enable short url for xbox paypal flow
            public const string PXEnableShortUrlPayPal = "PXEnableShortUrlPayPal";

            // Enable short url text for xbox paypal flow
            public const string PXEnableShortUrlPayPalText = "PXEnableShortUrlPayPalText";

            // Enable short url for xbox venmo flow
            public const string PXEnableShortUrlVenmo = "PXEnableShortUrlVenmo";

            // Enable short url text for xbox venmo flow
            public const string PXEnableShortUrlVenmoText = "PXEnableShortUrlVenmoText";

            // Enable to show inlineLogo for card number in Update CC flow
            public const string PXEnableUpdateCCLogo = "PXEnableUpdateCCLogo";

            // Enable Challenge Management feature
            public const string PXChallengeSwitch = "PXChallengeSwitch";

            // Enable to gracefully handle session validation failure
            public const string PXSessionValidationFailureHandling = "PXSessionValidationFailureHandling";

            // Enable to gracefully handle challenge validation failure
            public const string PXChallengeValidationFailureHandling = "PXChallengeValidationFailureHandling";

            // Enable to gracefully handle challenge creation failure
            public const string PXChallengeCreationFailureHandling = "PXChallengeCreationFailureHandling";

            // Enable Challenge by default
            public const string PXEnableChallenge = "PXEnableChallenge";

            // Enable Challenge with low complexity
            public const string PXChallengeComplexityLow = "PXChallengeComplexityLow";

            // Enable Challenge with medium complexity
            public const string PXChallengeComplexityMedium = "PXChallengeComplexityMedium";

            // Enable Challenge with high complexity
            public const string PXChallengeComplexityHigh = "PXChallengeComplexityHigh";

            // Enable HIP Challenge Provider for PX
            public const string PXChallengeProviderEnableHIP = "PXChallengeProviderEnableHIP";

            // Enable multipage Challenge for PX
            public const string PXChallengeMultipageChallenge = "PXChallengeMultipageChallenge";

            // Enable iframe based flow for gpay
            public const string PXEnableGPayIframeForAllBrowsers = "PXEnableGPayIframeForAllBrowsers";

            // Use CDN endpoint for static resource service
            public const string PXUseCDNForStaticResourceService = "PXUseCDNForStaticResourceService";

            // Includes the Culture And Language Transformation for PI linked profile pidl and standalone profile
            public const string PXIncludeCultureAndLanguageTransformation = "PXIncludeCultureAndLanguageTransformation";

            // Overrides the Culture And Language Transformation for PI linked profile pidl with origin country data
            public const string PXOverrideCultureAndLanguageTransformation = "PXOverrideCultureAndLanguageTransformation";

            // Uses the defaultAddress country if available to return profile update partial pidl for completererequisite
            public const string PXSetDefaultAddressCountryForProfileUpdatePartial = "PXSetDefaultAddressCountryForProfileUpdatePartial";

            // Enable attestation call to TransactionDataService for SafetyNet scenarios
            public const string PXSafetyNetTransactionDataServiceAttestation = "PXSafetyNetTransactionDataServiceAttestation";

            // Used to Enable PIMS cache for get payment methods:
            public const string PXEnablePIMSGetPaymentMethodsCache = "PXEnablePIMSGetPaymentMethodsCache";

            // Used to this flighting to set taxid values for Italy
            public const string PXSetItalyTaxIdValuesByFunction = "PXSetItalyTaxIdValuesByFunction";

            // use regional prod base url
            public const string PXUsePifdBaseUrlInsteadOfForwardedHostHeader = "PXUsePifdBaseUrlInsteadOfForwardedHostHeader";

            // Enable xboxCardUpsell
            public const string PXEnableXboxCardUpsell = "PXEnableXboxCardUpsell";

            // Send sms validation call directly from PX
            public const string PXEnableSMSChallengeValidation = "PXEnableSMSChallengeValidation";

            // Uses the style hints for xboxnative
            public const string PXEnableXboxNativeStyleHints = "PXEnableXboxNativeStyleHints";

            // enables redeem rewards flows for xboxnative
            public const string PXEnableXboxNativeRewards = "PXEnableXboxNativeRewards";

            // shows rewards error page when a challenge is mandated by ms rewards service or a failure occurs during redemption
            public const string PXShowRewardsErrorPage = "PXShowRewardsErrorPage";

            // Uses the style hints for Apply PI flow on xboxnative
            public const string PXEnableApplyPIXboxNativeStyleHints = "PXEnableApplyPIXboxNativeStyleHints";

            // Uses the flgiht to enabled the feature PXUsePSSToEnableValidatePIOnAttachChallenge
            public const string PXUsePSSToEnableValidatePIOnAttachChallenge = "PXUsePSSToEnableValidatePIOnAttachChallenge";

            // PX flighting to support card holder name regex update
            public const string PXCCNameRegexUpdate = "PXCCNameRegexUpdate";

            // Used to allow the local and PR DiffTest to use the PSS Partner mock from emulator
            public const string PXUsePSSPartnerMockForDiffTest = "PXUsePSSPartnerMockForDiffTest";

            // Used to get exposed flight features from stored session instead of this.ExposedFlioghtFeatures
            public const string PXGetAttestationFlightFeaturesFromStoredSession = "PXGetAttestationFlightFeaturesFromStoredSession";

            public const string PXEnableXboxCardUpsellPaymentOptions = "PXEnableXboxCardUpsellPaymentOptions";

            // Used to enable the china union pay payment method for the CN market for international partners
            public const string PXEnableCUPInternational = "PXEnableCUPInternational";

            // PX flighting to disable InvalidPaymentInstrumentType ServerErrorCode
            public const string PXDisableInvalidPaymentInstrumentType = "PXDisableInvalidPaymentInstrumentType";

            // Force PX to use the PSS to validate partner name
            public const string PXEnforcePSSPartnerValidation = "PXEnforcePSSPartnerValidation";

            // Skip syncToLegacy for Jarvis create address call
            public const string PXSkipJarvisAddressSyncToLegacy = "PXSkipJarvisAddressSyncToLegacy";

            public const string PXReturnFailedSessionState = "PXReturnFailedSessionState";

            public const string PXXboxCardApplyEnableFeedbackButton = "PXXboxCardApplyEnableFeedbackButton";

            public const string PXXboxCardApplyDisableStoreButtonNavigation = "PXXboxCardApplyDisableStoreButtonNavigation";

            // Used to call validation API to Wallet service
            public const string PXEnableValidateAPIForGPAP = "PXEnableValidateAPIForGPAP";

            // Used to enable getting storeSession from PaymentSessionHandler
            public const string PXEnableGettingStoredSessionForChallengeDescriptionsController = "PXEnableGettingStoredSessionForChallengeDescriptionsController";

            // Used to bypass GetPM if CC is the only option
            public const string PXEnableSkipGetPMIfCreditCardIsTheOnlyOption = "PXEnableSkipGetPMIfCreditCardIsTheOnlyOption";

            // Used to route PX service to PIMS PPE environment
            public const string PXEnablePIMSPPEEnvironment = "PXEnablePIMSPPEEnvironment";

            // Used to enable the paypay payment method for the jp market
            public const string PXEnablePayPay = "PXEnablePayPay";

            // Used to enable the alipayhk payment method for the hk market
            public const string PXEnableAlipayHK = "PXEnableAlipayHK";

            // Used to enable the gcash payment method for the ph market
            public const string PXEnableGCash = "PXEnableGCash";

            // Used to enable the truemoney payment method for the th market
            public const string PXEnableTrueMoney = "PXEnableTrueMoney";

            // Used to enable the touchngo payment method for the my market
            public const string PXEnableTouchNGo = "PXEnableTouchNGo";

            // Used to enable the alipaycn payment method for the cn market
            public const string PXEnableAlipayCN = "PXEnableAlipayCN";

            // Used to enable the alipaycn payment method limit text for the cn market
            public const string PXEnableAlipayCNLimitText = "PXEnableAlipayCNLimitText";

            // Used to enable Risk Eligibility check
            public const string PxEnableRiskEligibilityCheck = "PxEnableRiskEligibilityCheck";

            // Used to enable the listpi to include employee and organization PI's
            public const string PXEnableEmpOrgListPI = "PXEnableEmpOrgListPI";

            // Used to enable customize action context - remove action context instance
            public const string PXEnableReplaceContextInstanceWithPaymentInstrumentId = "PXEnableReplaceContextInstanceWithPaymentInstrumentId";

            // Used to enable CustomizeActionForm - set cancel button display content as back
            public const string PXEnableSetCancelButtonDisplayContentAsBack = "PXEnableSetCancelButtonDisplayContentAsBack";

            // Used to enable the sub-feature addAllFieldsRequiredText under the CustomizeDisplayContent feature
            public const string PXEnableAddAllFieldsRequiredText = "PXEnableAddAllFieldsRequiredText";

            // Used to enable the sub-feature addAsteriskToAllMandatoryFields under the CustomizeDisplayContent feature
            public const string PXEnableAddAsteriskToAllMandatoryFields = "PXEnableAddAsteriskToAllMandatoryFields";

            // The flight PXDisableRaisePIAddedOnOfferEvent is used to disable the telemetry for PI added on offer event.
            public const string PXDisableRaisePIAddedOnOfferEvent = "PXDisableRaisePIAddedOnOfferEvent ";

            // Used to flight the "vnext" header to PIMS when sending a request with the "GetPaymentMethods" action.
            public const string PXEnableVNextToPIMS = "PXEnableVNextToPIMS";

            // This flight is not used to sending to the PIMS if PXEnableVNextToPIMS enabled.
            public const string Vnext = "vnext";

            // Used to disable Redeem CSV flow
            public const string PXDisableRedeemCSVFlow = "PXDisableRedeemCSVFlow";

            // Flight to move AVS traffic from Address Enrichment to Jarvis Accounts
            public const string PXUseJarvisAccountsForAddressEnrichment = "PXUseJarvisAccountsForAddressEnrichment";

            // Flight to enable the use of the new Jarvis Accounts service for Address Enrichment
            public const string PXMakeAccountsAddressEnrichmentCall = "PXMakeAccountsAddressEnrichmentCall";

            // Used to enable the feature that checks if the Select Payment Method step was skipped
            public const string PXEnableIsSelectPMSkippedValue = "PXEnableIsSelectPMSkippedValue";

            // Used by Authentication Status in PaymentSessionsController
            public const string PXAuthenticateStatusForceVerifiedTrue = "PXAuthenticateStatusForceVerifiedTrue";

            // Used by Authentication Status in PaymentSessionsController
            public const string PXAuthenticateStatusOverrideVerification = "PXAuthenticateStatusOverrideVerification";

            // Used by Xbox Partner and Authentication Status in PaymentSessionsController to override verified status
            public const string PXAuthenticateStatusOverrideVerificationForXbox = "PXAuthenticateStatusOverrideVerificationForXbox";

            // Used to flight the check for credit card types with IsCreditCard in PaymentInstrumentExController
            public const string PXCheckCreditCardTypes = "PXCheckCreditCardTypes";

            // Used to flight the enable caching on TokenizationEncryption
            public const string PXEnableCachingTokenizationEncryption = "PXEnableCachingTokenizationEncryption";

            // Used to flight the update pan cvv data protections for Add and update credit card operations (PAN/CVV)
            public const string PXEnableTokenizationEncryptionAddUpdateCC = "PXEnableTokenizationEncryptionAddUpdateCC";

            // Used to flight the update pan cvv data protections for search transaction, challenge CVV, and India 3DS operation
            public const string PXEnableTokenizationEncryptionOtherOperation = "PXEnableTokenizationEncryptionOtherOperation";

            // Used to flight the update pan cvv data protections fetch config for Add and update credit card operations
            public const string PXEnableTokenizationEncryptionFetchConfigAddUpdateCC = "PXEnableTokenizationEncryptionFetchConfigAddUpdateCC";

            // Used to flight the update pan cvv data protections fetch config with encrypt script for Add and update credit card operations
            public const string PXEnableTokenizationEncryptionFetchConfigWithScript = "PXEnableTokenizationEncryptionFetchConfigWithScript";

            // Used to flight the update pan cvv data protections fetch config for Add and update credit card operations
            public const string PXEnableTokenizationEncryptionFetchConfigOtherOperation = "PXEnableTokenizationEncryptionFetchConfigOtherOperation";

            // Used to flight the update piauthkey data protections fetch config for Add credit card operations (PIAuthKey)
            public const string PXEnableTokenizationEncFetchConfigAddCCPiAuthKey = "PXEnableTokenizationEncFetchConfigAddCCPiAuthKey";

            // Used to flight the update piauthkey data protections fetch config with out encrypted payload for Add credit card operations (PIAuthKey)
            public const string PXDisableTokenizationEncPiAuthKeyFetchConfigtEncPayload = "PXDisableTokenizationEncPiAuthKeyFetchConfigtEncPayload";

            // Flight to use encrypted tokenization only for PAN/CVV/PiAuthKey
            public const string PXEncryptedTokenizationOnlyForPanCvvPiAuthKey = "PXEncryptedTokenizationOnlyForPanCvvPiAuthKey";

            // Flight to use remove use field from public key fallback in subtle.importKey error handling
            public const string PXRemoveUseFallbackForSubtleImportKey = "PXRemoveUseFallbackForSubtleImportKey";

            // Flight to use remove use field from public key fallback in subtle.importKey error handling when running encrypted tokenization only for PAN/CVV/PiAuthKey
            public const string PXRemoveUseFallbackWhenEncryptedTokenizationOnlyForPanCvvPiAuthKey = "PXRemoveUseFallbackWhenEncryptedTokenizationOnlyForPanCvvPiAuthKey";

            // Used to flight the set isSubmitGroup to be false in TradeAVS V1 flow in the address suggestion page
            public const string PXSetIsSubmitGroupFalseForTradeAVSV1 = "PXSetIsSubmitGroupFalseForTradeAVSV1";

            // level 1 flight for DeviceSupportStatus:  Used to add DeviceSupportStatus to the config, once DeviceSupportStatus is present in the config, the pidlsdk drop client side user agent check and use the value from server side config
            public const string PXWalletConfigAddDeviceSupportStatus = "PXWalletConfigAddDeviceSupportStatus";

            // level 2 flight for DeviceSupportStatus: Used to disable Gpay, set DeviceSupportStatus in GPay config to false. It is a flight based on os and browser from user agent.
            public const string PXWalletConfigDisableGooglePay = "PXWalletConfigDisableGooglePay";

            // level 2 flight for DeviceSupportStatus: Used to disable Apay, set deviceSupportStatus in Apay config to false. It is a flight based on os and browser from user agent.
            public const string PXWalletConfigDisableApplePay = "PXWalletConfigDisableApplePay";

            // level 1 flight for IframeFallbackSupported: Used to add IframeFallbackSupported to the config, once IframeFallbackSupported is present in the config, the pidlsdk drop client side browser check for iframe fallback and use the value from server side config
            public const string PXWalletConfigAddIframeFallbackSupported = "PXWalletConfigAddIframeFallbackSupported";

            // level 2 flight for IframeFallbackSupported: Used to set gpay IframeFallbackSupported to true. It is a flight based on os and browser from user agent
            public const string PXWalletEnableGooglePayIframeFallback = "PXWalletEnableGooglePayIframeFallback";

            // Used to flight the QR code flow for Xbox partners
            public const string PxEnableAddCcQrCode = "PxEnableAddCcQrCode";

            // Flight to use PaymentSessionsHandlerV2
            public const string PXUsePaymentSessionsHandlerV2 = "PXUsePaymentSessionsHandlerV2";

            // Flight to use GetVersionBasedPaymentSessionsHandler
            public const string PXUseGetVersionBasedPaymentSessionsHandler = "PXUseGetVersionBasedPaymentSessionsHandler";

            // Flight to add default payment method in Select flow
            public const string PXEnableDefaultPaymentMethod = "PXEnableDefaultPaymentMethod";

            // Flight to add when partners are not part of LuhnValidationEnabledPartners
            public const string PXLuhnValidationEnabledPartners = "PXLuhnValidationEnabledPartners";

            // Used to skip PaymentMethodGrouping feature
            public const string PXDisablePMGrouping = "PXDisablePMGrouping";

            // Flight to submit Sequential calls for Egypt customers
            public const string PXSubmitEGTaxIdsInSequence = "PXSubmitEGTaxIdsInSequence";

            // Flight to Enable Tax Number and UIN as mandatory fields for Egypt customers
            public const string PXEnableEGTaxIdsRequired = "PXEnableEGTaxIdsRequired";

            // Flight to enable psd2 flow for guest checkout instead of returning session
            public const string PXEnablePSD2ForGuestCheckoutFlow = "PXEnablePSD2ForGuestCheckoutFlow";

            // Used for mocking the PostAddresValidate with account emulator to decide if the intended call from
            // account accessor or AVS accessor to return respective response
            public const string AccountEmulatorValidateAddressWithAVS = "AccountEmulatorValidateAddressWithAVS";

            // Flight to enable expiryCVVGrouping to display expiry and CVV inline
            public const string PXEnableExpiryCVVGrouping = "enableExpiryCVVGrouping";

            // Flight to enable enableSavePaymentDetails to populate checkbox to save PI details.
            public const string PXEnableSavePaymentDetails = "enableSavePaymentDetails";

            // Flight to enable PSD2 for google pay.
            public const string PXEnablePSD2ForGooglePay = "PXEnablePSD2ForGooglePay";

            // Flight to enable PSD2 for JCB cards.
            public const string PXDisplayJCBChallenge = "PXDisplayJCBChallenge";

            // Flight to use mock wallet config instead from provider service for ExpressCheckout/QuickPayment
            public const string PXUseMockWalletConfig = "PXUseMockWalletConfig";

            // Flight to enable Gpay and Apay only in US
            public const string PXEnableGooglePayApplePayOnlyInUS = "PXEnableGooglePayApplePayOnlyInUS";

            // Flight to enable instance PI of GPay and Apay
            public const string GPayApayInstancePI = "GPayApayInstancePI";

            // Flight to enable instance PI of GPay and Apay
            public const string PXWalletConfigEnableBillingAddress = "PXWalletConfigEnableBillingAddress";

            // Flight to enable instance PI of GPay and Apay
            public const string PXWalletConfigEnableEmail = "PXWalletConfigEnableEmail";
            
            // Flight to enable modern UI in iDeal payments page
            public const string PXEnableModernIdealPayment = "PXEnableModernIdealPayment";

            // Flight to enable extra validation for paymentSession object
            public const string ValidatePaymentSessionProperties = "ValidatePaymentSessionProperties";

            // Flight to enable extra validation for paymentSession object
            public const string ValidatePaymentSessionPropertiesLogging = "ValidatePaymentSessionPropertiesLogging";

            // Flight to force skip PSD2 fingerprint, means that when we call authenticate we will likely get a response to challenge
            public const string PXPSD2SkipFingerprint = "PXPSD2SkipFingerprint";

            // Flight to enable skipping PSD2 fingerprint by URL domain.  Bin will be defined in a separate flight in format "PXPSD2SkipUrl_<domain>
            public const string PXPSD2SkipFingerprintByUrl = "PXPSD2SkipFingerprintByUrl";

            // Flight to enable timeout template via src js file instead of via onload body listener
            public const string PXPSD2TimeoutOnPostViaSrc = "PXPSD2TimeoutOnPostViaSrc";

            // Flight to enable Fraud Detection Service Integration
            public const string PXIntegrateFraudDetectionService = "PXIntegrateFraudDetectionService";

            // Flight to enable challenge cvv with error min and max length validation.
            public const string PXEnableChallengeCvvValidation = "PXEnableChallengeCvvValidation";

            // Flight to enable validation for SSRF
            public const string PXEnableSSRFPolicy = "PXEnableSSRFPolicy";

            public const string PXEnableHandleTransactionNotAllowed = "PXEnableHandleTransactionNotAllowed";

            // Enable flight based setting of device channel to Browser
            public const string PXEnableBrowserBasedDeviceChannel = "PXEnableBrowserBasedDeviceChannel";

            // Flight to enable Get all component descriptions from initialization controller.
            public const string PXEnableAllComponentDescriptions = "PXEnableAllComponentDescriptions";

            // Flight to enable Get all component descriptions from initialization controller.
            public const string PXEnableUsePOCapabilities = "PXEnableUsePOCapabilities";

            // Flight to enable challenges for MOTO transactions
            public const string PXEnableChallengesForMOTO = "PXEnableChallengesForMOTO";

            // Flight to enable switch from checkout request to payment request in PX.
            public const string UsePaymentRequestApi = "UsePaymentRequestApi";

            // Flight to enable CachedPrefetcherData to support Add and list PI PIDL's with the select PM PIDL description.
            public const string PXEnableCachedPrefetcherData = "PXEnableCachedPrefetcherData";

            // Flight to update the maxlength of addresslin1 to 255
            public const string UpdateAddressline1MaxLength = "UpdateAddressline1MaxLength";

            // Flight to remove the Jarvis headers from the submit call headers that are not pointing to Jarvis.
            public const string PXRemoveJarvisHeadersFromSubmitUrl = "PXRemoveJarvisHeadersFromSubmitUrl";

            // Flight to not add the PIFD address post URL for profile submit.
            public const string PXSkipPifdAddressPostForNonAddressesType = "PXSkipPifdAddressPostForNonAddressesType";

            // Enables express checkout HTML with inline JS
            public const string PXUseInlineExpressCheckoutHtml = "PXUseInlineExpressCheckoutHtml";

            // Enables use of express checkout static resources from int
            public const string PXExpressCheckoutUseIntStaticResources = "PXExpressCheckoutUseIntStaticResources";

            // Enables use of express checkout static resources from prod
            public const string PXExpressCheckoutUseProdStaticResources = "PXExpressCheckoutUseProdStaticResources";

            // Enables priority of getting session with sessionId 
            public const string PXEnableGetSessionWithSessionId = "PXEnableGetSessionWithSessionId";

            // Flight to enable feature, which is to change style structure of expiry month and year from dropdown to one text box for month and year.
            public const string PXChangeExpiryMonthYearToExpiryDateTextBox = "PXChangeExpiryMonthYearToExpiryDateTextBox";

            // Flight to enable feature, which is to change style structure of expiry month and year from dropdown to one text box for month and year.
            public const string PXCombineExpiryMonthYearToDateTextBox = "PXCombineExpiryMonthYearToDateTextBox";

            // Ebables to use deep copy of payment experience partner settings instead of shallow copy
            public const string PXEnablePartnerSettingsDeepCopy = "PXEnablePartnerSettingsDeepCopy";

            // Skip additional validation for Midterm bypass fix for zero amount
            public const string PXSkipAdditionalValidationForZeroAmount = "PXSkipAdditionalValidationForZeroAmount";

            // To use NTS INT Url
            public const string PXUseNTSIntUrl = "PXUseNTSIntUrl";

            // Enables enforcement for 4004 (/CompleteChallenge) errors from bank
            public const string PXPSD2BankErrorEnforcementCompleteChallenge = "PXPSD2BankErrorEnforcementCompleteChallenge";

            // Enables enforcement for 4002 (/Authenticate) from bank
            public const string PXPSD2BankErrorEnforcementAuthenticate = "PXPSD2BankErrorEnforcementAuthenticate";

            // Enable Safety net validation in Authenticate flow
            public const string PXPSD2SafetyNetAuthenticate = "PXPSD2SafetyNetAuthenticate";

            // To disable the cache for GetWalletConfig
            public const string PXDisableGetWalletConfigCache = "PXDisableGetWalletConfigCache";

            // Fail PSD2NTSR19 by only for specific card types
            public const string PSD2NTSR19FailCardType = "PSD2_N_TSR19_FailCardType_";

            // To disable the cache for GetWalletConfig
            public const string PXPaasAddCCDfpIframeForCommerceRisk = "PXPaasAddCCDfpIframeForCommerceRisk";

            // Enable use of short URL for PX
            public const string PXUseShortURLController = "PXUseShortURLController";
            
            // Flight to enable switch from PO client action to PIMS eligible payment methods.
            public const string IncludePIDLDescriptionsV2 = "IncludePIDLDescriptionsV2";

            // Uber flight to skip challenge for PSD2 flow
            public const string PXPSD2SkipChallenge = "PXPSD2SkipChallenge";

            // Uber flight to skip challenge for PSD2 flow by URL domain.  Bin will be defined in a separate flight in format "PXPSD2SkipChallengeByUrl_<domain>
            public const string PXPSD2SkipChallengeByUrl = "PXPSD2SkipChallengeByUrl";
        }

        public static class ContextKeys
        {
            // Account id of the user
            public const string AccountId = "accountId";

            // V4 IP Address of the end user device use to determine if the user is in CorpNet
            public const string IpAddress = "ipAddress";

            public const string BaseMsCV = "baseMsCV";

            public const string Country = "country";

            public const string Partner = "partner";

            public const string Language = "language";

            public const string Family = "family";

            public const string Operation = "operation";

            public const string Millisecond = "millisecond";

            public const string IsCorpNet = "isCorpNet";

            public const string EnvName = "environmentName";

            public const string EnvType = "environmentType";

            public const string PidlSdkVersion = "pidlsdkVersion";

            public const string OperatingSystem = "operatingsystem";

            public const string OperatingSystemVer = "operatingsystemver";

            public const string Browser = "browser";

            public const string BrowserVer = "browserver";

            public const string PidlSdkVer = "pidlsdkver";

            public const string ReferrerDomain = "referrerDomain";
        }

        public class FeatureConfig
        {
            private string assignmentContext;
            private List<string> enabledFeatures;

            public FeatureConfig() : this(string.Empty, new List<string>())
            {
            }

            public FeatureConfig(string assignmentContext, List<string> enabledFeatures)
            {
                this.assignmentContext = assignmentContext ?? string.Empty;
                this.enabledFeatures = enabledFeatures ?? new List<string>();
            }

            /// <summary>
            /// Gets assignment context set by AzureExP for logging purpose.
            /// Sample assignment context looks like  "bff76dbb-d8fb:27651;"
            /// </summary>
            public string AssignmentContext
            {
                get
                {
                    return this.assignmentContext;
                }
            }

            /// <summary>
            /// Gets list of boolean FeatureVariables that has true value assigned for the request
            /// </summary>
            public List<string> EnabledFeatures
            {
                get
                {
                    return this.enabledFeatures;
                }
            }
        }
    }
}
