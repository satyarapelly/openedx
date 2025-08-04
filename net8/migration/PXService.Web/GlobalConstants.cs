// <copyright file="GlobalConstants.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PXService.Model.AccountService.AddressValidation;

    /// <summary>
    /// Constants container, each set of constants will be grouped into a nested class
    /// </summary>
    public static class GlobalConstants
    {
        internal const string ServiceName = "PXService";

        private static List<string> threeDSTestAccountIds = new List<string>()
        {
            "4089c4a0-6cb6-4bad-8ca1-a30f47b28365",
            "1b11f28d-2a22-4c04-aa0d-ac005cb16926"
        };

        // The left hand side of this mapping is an identifier of the Pidl property.  The right hand side is an array
        // that indicates possible sources from which to prefill the Pidl property from.  The array is in the order of
        // most preferred to least preferred source.
        private static Dictionary<string, string[]> prefillMapping = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            // Prefill Profile Pidl
            { "profile.first_name", new string[] { ClientContextKeys.CMProfile.FirstName, ClientContextKeys.CMProfileV3.FirstName, ClientContextKeys.MsaProfile.FirstName } },
            { "profile.last_name", new string[] { ClientContextKeys.CMProfile.LastName, ClientContextKeys.CMProfileV3.LastName, ClientContextKeys.MsaProfile.LastName } },
            { "profile.email_address", new string[] { ClientContextKeys.CMLegalEntityProfile.EmailAddress, ClientContextKeys.CMProfile.EmailAddress, ClientContextKeys.CMProfileV3.EmailAddress, ClientContextKeys.MsaProfile.EmailAddress } },
            { "profile.email", new string[] { ClientContextKeys.CMLegalEntityProfile.EmailAddress, ClientContextKeys.CMProfileV3.Email, ClientContextKeys.MsaProfile.EmailAddress } },
            { "profile.id", new string[] { ClientContextKeys.CMProfile.ProfileId, ClientContextKeys.CMProfileV3.ProfileId } },
            { "profile.type", new string[] { ClientContextKeys.CMProfile.ProfileType, ClientContextKeys.CMProfileV3.ProfileType } },
            { "profile.birth_date", new string[] { ClientContextKeys.CMProfile.BirthDate } },
            { "profile.company_name", new string[] { ClientContextKeys.CMLegalEntityProfile.CompanyName, ClientContextKeys.CMProfile.CompanyName, ClientContextKeys.CMProfileV3.CompanyName } },
            { "profile.culture", new string[] { ClientContextKeys.CMLegalEntityProfile.Culture, ClientContextKeys.CMProfile.Culture, ClientContextKeys.CMProfileV3.Culture } },
            { "profile.nationality", new string[] { ClientContextKeys.CMProfile.Nationality } },
            { "profile.default_address_id", new string[] { ClientContextKeys.CMProfile.DefaultAddressId, ClientContextKeys.CMProfileV3.DefaultAddressId } },
            { "profile.etag", new string[] { ClientContextKeys.CMProfileV3.Etag } },
            { "profile.customer_id", new string[] { ClientContextKeys.CMProfileV3.CustomerId } },
            { "profile.country", new string[] { ClientContextKeys.CMProfileV3.Country } },
            { "profile.language", new string[] { ClientContextKeys.CMLegalEntityProfile.Language, ClientContextKeys.CMProfileV3.Language } },
            { "profile.love_code", new string[] { ClientContextKeys.CMProfileV3.LoveCode } },
            { "profile.mobile_barcode", new string[] { ClientContextKeys.CMProfileV3.MobileBarcode } },
            { "profile.snapshot_id", new string[] { ClientContextKeys.CMProfileV3.SnapshotId } },
            { "profile.links", new string[] { ClientContextKeys.CMProfileV3.Links } },
            { "profile.object_type", new string[] { ClientContextKeys.CMProfileV3.ObjectType } },
            { "profile.resource_status", new string[] { ClientContextKeys.CMProfileV3.ResourceStatus } },
            { "profile.name", new string[] { ClientContextKeys.CMProfileV3.Name } },

            // Prefill Address Pidl
            { "address.customer_id", new string[] { ClientContextKeys.CMProfileAddressV3.CustomerId } },
            { "address.address_line1", new string[] { ClientContextKeys.CMLegalEntityProfile.AddressLine1, ClientContextKeys.CMProfileAddress.AddressLine1, ClientContextKeys.CMProfileAddressV3.AddressLine1, ClientContextKeys.LegacyBillableAccountAddress.AddressLine1 } },
            { "address.address_line2", new string[] { ClientContextKeys.CMLegalEntityProfile.AddressLine2, ClientContextKeys.CMProfileAddress.AddressLine2, ClientContextKeys.CMProfileAddressV3.AddressLine2, ClientContextKeys.LegacyBillableAccountAddress.AddressLine2 } },
            { "address.address_line3", new string[] { ClientContextKeys.CMLegalEntityProfile.AddressLine3, ClientContextKeys.CMProfileAddress.AddressLine3, ClientContextKeys.CMProfileAddressV3.AddressLine3, ClientContextKeys.LegacyBillableAccountAddress.AddressLine3 } },
            { "address.city", new string[] { ClientContextKeys.CMLegalEntityProfile.City, ClientContextKeys.CMProfileAddress.City, ClientContextKeys.CMProfileAddressV3.City, ClientContextKeys.LegacyBillableAccountAddress.City } },
            { "address.region", new string[] { ClientContextKeys.CMLegalEntityProfile.Region, ClientContextKeys.CMProfileAddress.Region, ClientContextKeys.CMProfileAddressV3.Region, ClientContextKeys.LegacyBillableAccountAddress.Region } },
            { "address.district", new string[] { ClientContextKeys.CMLegalEntityProfile.District, ClientContextKeys.CMProfileAddressV3.District, ClientContextKeys.LegacyBillableAccountAddress.District } },
            { "address.postal_code", new string[] { ClientContextKeys.CMLegalEntityProfile.PostalCode, ClientContextKeys.CMProfileAddress.PostalCode, ClientContextKeys.CMProfileAddressV3.PostalCode, ClientContextKeys.LegacyBillableAccountAddress.PostalCode } },
            { "address.country", new string[] { ClientContextKeys.CMProfileAddress.Country, ClientContextKeys.CMProfileAddressV3.Country } },
            { "address.first_name", new string[] { ClientContextKeys.CMLegalEntityProfile.FirstName, ClientContextKeys.CMProfileAddressV3.FirstName } },
            { "address.first_name_pronunciation", new string[] { ClientContextKeys.CMProfileAddressV3.FirstNamePronunciation } },
            { "address.last_name", new string[] { ClientContextKeys.CMLegalEntityProfile.LastName, ClientContextKeys.CMProfileAddressV3.LastName } },
            { "address.last_name_pronunciation", new string[] { ClientContextKeys.CMProfileAddressV3.LastNamePronunciation } },
            { "address.middle_name", new string[] { ClientContextKeys.CMLegalEntityProfile.MiddleName, ClientContextKeys.CMProfileAddressV3.MiddleName } },
            { "address.correspondence_name", new string[] { ClientContextKeys.CMProfileAddressV3.CorrespondenceName } },
            { "address.phone_number", new string[] { ClientContextKeys.CMLegalEntityProfile.PhoneNumber, ClientContextKeys.CMProfileAddressV3.PhoneNumber } },
            { "address.mobile", new string[] { ClientContextKeys.CMProfileAddressV3.Mobile } },
            { "address.fax", new string[] { ClientContextKeys.CMProfileAddressV3.Fax } },
            { "address.telex", new string[] { ClientContextKeys.CMProfileAddressV3.Telex } },
            { "address.email_address", new string[] { ClientContextKeys.CMLegalEntityProfile.EmailAddress, ClientContextKeys.CMProfileAddressV3.EmailAddress } },
            { "address.web_site_url", new string[] { ClientContextKeys.CMProfileAddressV3.WebSiteUrl } },
            { "address.street_supplement", new string[] { ClientContextKeys.CMProfileAddressV3.StreetSupplement } },
            { "address.is_within_city_limits", new string[] { ClientContextKeys.CMProfileAddressV3.IsWithinCityLimits } },
            { "address.form_of_address", new string[] { ClientContextKeys.CMProfileAddressV3.FormOfAddress } },
            { "address.address_notes", new string[] { ClientContextKeys.CMProfileAddressV3.AddressNotes } },
            { "address.time_zone", new string[] { ClientContextKeys.CMProfileAddressV3.TimeZone } },
            { "address.latitude", new string[] { ClientContextKeys.CMProfileAddressV3.Latitude } },
            { "address.longitude", new string[] { ClientContextKeys.CMProfileAddressV3.Longitude } },
            { "address.is_avs_validated", new string[] { ClientContextKeys.CMProfileAddressV3.IsAvsValidated } },
            { "address.validate", new string[] { ClientContextKeys.CMProfileAddressV3.Validate } },
            { "address.validation_stamp", new string[] { ClientContextKeys.CMProfileAddressV3.ValidationStamp } },
            { "address.links", new string[] { ClientContextKeys.CMProfileAddressV3.Links } },
            { "address.object_type", new string[] { ClientContextKeys.CMProfileAddressV3.ObjectType } },
            { "address.contract_version", new string[] { ClientContextKeys.CMProfileAddressV3.ContractVersion } },
            { "address.resource_status", new string[] { ClientContextKeys.CMProfileAddressV3.ResourceStatus } },

            // Prefill Tax Pidl
            { "hapiTaxId.taxId", new string[] { ClientContextKeys.TaxData.TaxId } },
            { "hapiTaxIdStandalone.taxId", new string[] { ClientContextKeys.TaxData.TaxId } },
            { "taxId.value", new string[] { ClientContextKeys.TaxData.TaxId } },
        };

        private static Dictionary<string, AddressAVSValidationStatus> avsErrorMessages = new Dictionary<string, AddressAVSValidationStatus>()
        {
            { "Street", AddressAVSValidationStatus.InvalidStreet },
            { "City", AddressAVSValidationStatus.InvalidCityRegionPostalCode },
            { "Province", AddressAVSValidationStatus.InvalidCityRegionPostalCode },
            { "Region", AddressAVSValidationStatus.InvalidCityRegionPostalCode },
            { "PostalCode", AddressAVSValidationStatus.InvalidCityRegionPostalCode }
        };

        internal static Dictionary<string, string[]> PrefillMapping
        {
            get
            {
                return GlobalConstants.prefillMapping;
            }
        }

        internal static List<string> ThreeDSTestAccountIds
        {
            get
            {
                return GlobalConstants.threeDSTestAccountIds;
            }
        }

        internal static Dictionary<string, AddressAVSValidationStatus> AvsErrorMessages
        {
            get
            {
                return GlobalConstants.avsErrorMessages;
            }
        }

        internal static class RouteNames
        {
            public const string DefaultWebApi = "DefaultWebApi";
            public const string ProbeApi = "ProbeApi";
            public const string GetAddressDescriptionsApi = "GetAddressDescriptionsAPI";
            public const string GetAddressDescriptionsApiNoId = "GetAddressDescriptionsAPINoId";
            public const string GetChallengeDescriptionsApi = "GetChallengeDescriptionsAPI";
            public const string GetChallengeDescriptionsApiNoId = "GetChallengeDescriptionsAPINoId";
            public const string GetPaymentMethodsApi = "GetPaymentMethodsAPI";
            public const string GetPaymentMethodsApiNoId = "GetPaymentMethodAPINoId";
            public const string GetPaymentMethodDescriptionsApi = "GetPaymentMethodDescriptionsAPI";
            public const string GetPaymentMethodDescriptionsApiNoId = "GetPaymentMethodDescriptionsAPINoId";
            public const string GetProfileDescriptionsApiNoId = "GetProfileDescriptionsAPINoId";
        }

        internal static class V6RouteNames
        {
            public const string Probe = "V6Probe";
            public const string GetPaymentInstrumentEx = "V6GetPaymentInstrumentExAPI";
            public const string ListPaymentInstrumentEx = "V6ListPaymentInstrumentExAPI";
            public const string PostPaymentInstrumentEx = "V6PostPaymentInstrumentEx";
            public const string RemovePaymentInstrumentEx = "V6RemovePaymentInstrumentEx";
            public const string ResumePendingOperationEx = "V6ResumePendingOperationEx";
            public const string GetCardProfileEx = "V6GetCardProfileEx";
            public const string PostReplenishTransactionCredentialsEx = "V6PostReplenishTransactionCredentialsEx";
            public const string GetSettings = "V6GetSettings";
        }

        internal static class V7RouteNames
        {
            public const string Probe = "V7Probe";
            public const string GetPaymentInstrumentEx = "V7GetPaymentInstrumentExAPI";
            public const string ListPaymentInstrumentEx = "V7ListPaymentInstrumentExAPI";
            public const string GetChallengeContextPaymentInstrumentEx = "V7GetChallengeContextPaymentInstrumentEx";
            public const string PostPaymentInstrumentEx = "V7PostPaymentInstrumentEx";
            public const string RemovePaymentInstrumentEx = "V7RemovePaymentInstrumentEx";
            public const string UpdatePaymentInstrumentEx = "V7UpdatePaymentInstrumentEx";
            public const string ReplacePaymentInstrumentEx = "V7ReplacePaymentInstrumentEx";
            public const string RedeemPaymentInstrumentEx = "V7RedeemPaymentInstrumentEx";
            public const string ResumePendingOperationEx = "V7ResumePendingOperationEx";
            public const string AnonymousResumePendingOperationEx = "V7AnonymousResumePendingOperationEx";
            public const string CreatePaymentInstrumentEx = "V7CreatePaymentInstrumentEx";
            public const string GetCardProfileEx = "V7GetCardProfileEx";
            public const string GetSeCardPersos = "V7GetSeCardPersosEx";
            public const string PostReplenishTransactionCredentialsEx = "V7PostReplenishTransactionCredentialsEx";
            public const string AcquireLUKsEx = "AcquireLUKsEx";
            public const string ConfirmLUKsEx = "ConfirmLUKsEx";
            public const string ValidateCvvEx = "ValidateCvvEx";
            public const string GetSettings = "V7GetSettings";
            public const string GetSettingsInPost = "V7GetSettingsInPost";
            public const string GetAddressDescriptionsApi = "V7GetAddressDescriptionsAPI";
            public const string GetAgenticTokenDescriptionsApi = "V7GetAgenticTokenDescriptionsAPI";
            public const string GetAddressDescriptionsApiNoId = "V7GetAddressDescriptionsAPINoId";
            public const string GetChallengeDescriptionsApi = "V7GetChallengeDescriptionsAPI";
            public const string GetChallengeDescriptionsApiNoId = "V7GetChallengeDescriptionsAPINoId";
            public const string GetPaymentMethodDescriptionsApi = "V7GetPaymentMethodDescriptionsAPI";
            public const string GetPaymentMethodDescriptionsApiNoId = "V7GetPaymentMethodDescriptionsAPINoId";
            public const string GetProfileDescriptionsApiNoId = "V7GetProfileDescriptionsAPINoId";
            public const string GetBillingGroupDescriptionsApiNoId = "V7GetBillingGroupDescriptionsAPINoId";
            public const string GetDigitizationDescriptionsApi = "V7GetDigitizationDescriptionsAPI";
            public const string GetTaxIdDescriptionsApi = "V7GetTaxIdDescriptionsAPI";
            public const string GetRewardsDescriptionsApi = "V7GetRewardsDescriptionsAPI";
            public const string GetTenantDescriptionsApi = "V7GetTenantDescriptionsAPI";
            public const string TransformationApi = "V7TransformationApi";
            public const string ValidationApi = "V7ValidationApi";
            public const string AnonymousGetAddressDescriptionsApi = "V7AnonymousGetAddressDescriptionsAPI";
            public const string AnonymousGetTaxIdDescriptionsApi = "V7AnonymousGetTaxIdDescriptionsApi";
            public const string AnonymousGetPaymentMethodDescriptionsApi = "V7AnonymousGetPaymentMethodDescriptionsAPI";
            public const string AnonymousGetPaymentMethodDescriptionsSessionIdApi = "V7AnonymousGetPaymentMethodDescriptionsSessionIdAPI";
            public const string SessionsByIdApi = "V7SessionsByIdApi";
            public const string SessionsApi = "V7SessionsApi";
            public const string PostCardsApi = "V7PostCardsApi";
            public const string AnonymousLegacyAddressValidationApi = "V7AnonymousLegacyAddressValidationApi";
            public const string GetPaymentSessionDescriptionsApi = "V7GetPaymentSessionDescriptionsApi";
            public const string AnonymousCheckoutsExApi = "V7AnonymousCheckoutsExApi";
            public const string AnonymousCheckoutsExStatusApi = "V7AnonymousCheckoutsExStatusApi";
            public const string AnonymousCheckoutsExCompletedApi = "V7AnonymousCheckoutsExCompletedApi";
            public const string AnonymousGetCheckoutDescriptionsApi = "V7AnonymousGetCheckoutDescriptionsAPI";
            public const string PaymentSessionApi = "V7PaymentSessionApi";
            public const string PaymentSessionGetApi = "V7PaymentSessionGetApi";
            public const string QRCodePaymentSessionGetApi = "V7QrCodePaymentSessionGetApi";
            public const string PaymentSessionCreateAndAuthenticateApi = "V7PaymentSessionCreateAndAuthenticateApi";
            public const string PaymentSessionAuthenticateApi = "V7PaymentSessionAuthenticateApi";
            public const string PaymentSessionNotifyThreeDSChallengeCompletedApi = "V7PaymentSessionNotifyThreeDSChallengeCompletedApi";
            public const string PaymentSessionBrowserAuthenticateThreeDSOneApi = "V7PaymentSessionBrowserAuthenticateThreeDSOneApi";
            public const string PaymentSessionBrowserAuthenticateRedirectionThreeDSOneApi = "V7PaymentSessionBrowserAuthenticateRedirectionThreeDSOneApi";
            public const string PaymentSessionBrowserNotifyThreeDSOneChallengeCompletedOneApi = "V7PaymentSessionBrowserNotifyThreeDSOneChallengeCompletedApi";
            public const string PaymentSessionAuthenticateIndiaThreeDSApi = "V7PaymentSessionAuthenticateIndiaThreeDSApi";
            public const string BrowserFlowPaymentSessionsAuthenticateApi = "V7AnonymousPaymentSessionsAuthenticateApi";
            public const string BrowserFlowPaymentSessionsNotifyThreeDSChallengeCompletedApi = "V7AnonymousPaymentSessionsNotifyThreeDSChallengeCompletedApi";
            public const string AnonymousModernAddressValidationApi = "V7AnonymousModernAddressValidationApi";
            public const string AnonymousRDSSessionQueryApi = "V7AnonymousRDSSessionQueryApi";
            public const string PaymentTransactionApi = "V7PaymentTransactionApi";
            public const string AddressesExApi = "V7AddressesExApi";
            public const string AddressesExApiWithId = "V7AddressesExApiWithId";
            public const string PatchAddressesExApi = "V7PatchAddressesExApi";
            public const string PostAssets = "V7Assets";
            public const string ApplyPaymentInstrumentEx = "V7ApplyPaymentInstrumentEx";
            public const string WalletsGetConfigApi = "V7WalletsGetConfigApi";
            public const string WalletsSetupProviderSessionApi = "V7WalletsSetupProviderSessionApi";
            public const string WalletsProvisionWalletTokenApi = "V7WalletsProvisionWalletTokenApi";
            public const string MSRewardsApi = "V7MSRewardsApi";
            public const string AttachAddressCheckoutRequestExApi = "V7AttachAdddressCheckoutRequestsExApi";
            public const string AttachProfileCheckoutRequestExApi = "V7AttachProfileCheckoutRequestsExApi";
            public const string ConfirmCheckoutRequestExApi = "V7ConfirmCheckoutRequestsExApi";
            public const string AttachPaymentInstrumentExApi = "V7AttachPaymentIntrumentCheckoutRequestsExApi";
            public const string PaymentSessionAuthenticationStatusApi = "V7PaymentSessionAuthenticationStatusApi";
            public const string ExpressCheckoutConfirmApi = "V7ExpressCheckoutConfirmApi";
            public const string TokensExApi = "V7TokensExApi";
            public const string TokensExChallengeApi = "V7TokensExChallengeApi";
            public const string TokensExValidateChallengeApi = "V7TokensExValidateChallengeApi";
            public const string TokensExMandatesApi = "V7TokensExMandates";

            // Payment Client
            public const string PaymentClientInitializationApi = "V7PaymentClientInitializationAPI";
            public const string GetDescriptionsApi = "V7GetDescriptionsAPI";
            public const string ConfirmPaymentRequestExApi = "V7ConfirmPaymentRequestExApi";
            public const string AttachChallengeDataPaymentRequestExApi = "V7AttachChallengeDataPaymentRequestExApi";
            public const string RemoveEligiblePaymentmethodsPaymentRequestExApi = "V7RemoveEligiblePaymentmethodsPaymentRequestExApi";
        }

        internal static class APINames
        {
            public const string ApplyPaymentInstrumentEx = "ApplyPaymentInstrumentEx";
            public const string GetPaymentInstrumentEx = "GetPaymentInstrumentEx";
            public const string ListPaymentInstrumentEx = "ListPaymentInstrumentEx";
            public const string GetChallengeContextEx = "GetChallengeContextEx";
            public const string PostPaymentInstrumentEx = "PostPaymentInstrumentEx";
            public const string RemovePaymentInstrumentEx = "RemovePaymentInstrumentEx";
            public const string UpdatePaymentInstrumentEx = "UpdatePaymentInstrumentEx";
            public const string ReplacePaymentInstrumentEx = "ReplacePaymentInstrumentEx";
            public const string RedeemPaymentInstrumentEx = "RedeemPaymentInstrumentEx";
            public const string ResumePendingOperationEx = "ResumePendingOperationEx";
            public const string GetCardProfileEx = "GetCardProfileEx";
            public const string GetSeCardPersosEx = "GetSeCardPersosEx";
            public const string PostReplenishTransactionCredentialsEx = "PostReplenishTransactionCredentialsEx";
            public const string GetSettings = "GetSettings";
            public const string GetLifestyleMerchants = "GetLifestyleMerchants";
            public const string AcquireLUKs = "AcquireLUKs";
            public const string ConfirmLUKs = "ConfirmLUKs";
            public const string ValidateCvv = "ValidateCvv";
            public const string GetSession = "GetSession";
            public const string PostSessionId = "PostSessionId";
            public const string NotifyThreeDSMethodCompleted = "NotifyThreeDSMethodCompleted";
            public const string NotifyThreeDSChallengeCompleted = "NotifyThreeDSChallengeCompleted";
            public const string BrowserNotifyThreeDSOneChallengeCompleted = "BrowserNotifyThreeDSOneChallengeCompleted";
            public const string PostPaymentSession = "PostPaymentSession";
            public const string GetPaymentSession = "GetPaymentSession";
            public const string PostSession = "PostSession";
            public const string PostCard = "PostCard";
            public const string ListCards = "ListCards";
            public const string GenerateSessionId = "GenerateSessionId";
            public const string GetDirectoryServerInfo = "GetDirectoryServerInfo";
            public const string Authenticate = "Authenticate";
            public const string AuthenticateIndiaThreeDS = "AuthenticateIndiaThreeDS";
            public const string CreateAndAuthenticate = "CreateAndAuthenticate";
            public const string AuthenticateNoSessionId = "AuthenticateNoSessionId";
            public const string LegacyValidateAddress = "LegacyValidateAddress";
            public const string ModernValidateAddress = "ModernValidateAddress";
            public const string RDSSessionQuery = "RDSSessionQuery";
            public const string SuggestAddress = "SuggestAddress";
            public const string PostAddress = "PostAddress";
            public const string TryUpdateAddressWith9digitZipCode = "TryUpdateAddressWith9digitZipCode";
            public const string GetAddressEx = "GetAddressEx";
            public const string BrowserAuthenticateThreeDSOne = "BrowserAuthenticateThreeDSOne";
            public const string BrowserAuthenticateRedirectionThreeDSOne = "BrowserAuthenticateRedirectionThreeDSOne";
            public const string RedeemMSRewards = "RedeemMSRewards";
            public const string AuthenticationStatus = "AuthenticationStatus";

            // Pidl APIs
            public const string GetChallengeDescriptions = "GetChallengeDescriptionsAPI";
            public const string GetAddressDescriptions = "GetAddressDescriptions";
            public const string GetPaymentMethodDescriptions = "GetPaymentMethodDescriptions";
            public const string GetPaymentMethods = "GetPaymentMethods";
            public const string GetProfileDescriptions = "GetProfileDescriptions";
            public const string GetDigitizationDescriptions = "GetDigitizationDescriptions";
            public const string GetTaxIdDescriptions = "GetTaxIdDescriptions";
            public const string GetTenantDescriptions = "GetTenantDescriptions";
            public const string TransformPidlProperty = "TransformPidlProperty";
            public const string ValidatePidlProperty = "ValidatePidlProperty";
            public const string GetResourceDescriptions = "GetResourceDescriptions";
            public const string GetCheckoutDescriptions = "GetCheckoutDescriptions";
            public const string GetRewardsDescriptions = "GetRewardsDescriptions";
            public const string IssuerServiceApply = "IssuerService";

            public const string PostAssets = "PostAssets";
            public const string ModernAddressValidateForTrade = "ModernAddressValidateForTrade";

            // Payment Client
            public const string Initialization = "Initialize";
            public const string GetDescriptions = "GetDescriptions";
        }

        internal static class ControllerNames
        {
            public const string ProbeController = "ProbeController";

            public const string PaymentInstrumentsExController = "PaymentInstrumentsExController";
            public const string SettingsController = "SettingsController";
            public const string SessionsController = "SessionsController";
            public const string PaymentSessionsController = "PaymentSessionsController";
            public const string AddressesController = "AddressesController";
            public const string RDSSessionController = "RDSSessionController";
            public const string AddressesExController = "AddressesExController";
            public const string PaymentSessionDescriptionsController = "PaymentSessionDescriptionsController";
            public const string PaymentTransactionsController = "PaymentTransactionsController";
            public const string CheckoutsExController = "CheckoutsExController";
            public const string CheckoutDescriptionsController = "CheckoutDescriptionsController";
            public const string ExpressCheckoutController = "ExpressCheckoutController";
            public const string TokensExController = "TokensExController";

            // Anonymous or Authenticated controllers
            public const string CardsController = "CardsController";

            // Pidl Controllers
            public const string AgenticTokenDescriptionsController = "AgenticTokenDescriptionsController";
            public const string AddressDescriptionsController = "AddressDescriptionsController";
            public const string ChallengeDescriptionsController = "ChallengeDescriptionsController";
            public const string PaymentMethodDescriptionsController = "PaymentMethodDescriptionsController";
            public const string PaymentMethodsController = "PaymentMethodsController";
            public const string ProfileDescriptionsController = "ProfileDescriptionsController";
            public const string DigitizationDescriptionsController = "DigitizationDescriptionsController";
            public const string TaxIdDescriptionsController = "TaxIdDescriptionsController";
            public const string TenantDescriptionsController = "TenantDescriptionsController";
            public const string PidlTransformationController = "PidlTransformationController";
            public const string PidlValidationController = "PidlValidationController";
            public const string BillingGroupDescriptionsController = "BillingGroupDescriptionsController";
            public const string RewardsDescriptionsController = "RewardsDescriptionsController";

            // Wallet Controller
            public const string WalletsController = "WalletsController";

            // MSRewards Controller
            public const string MSRewardsController = "MSRewardsController";

            // Payment Client
            public const string InitializationController = "InitializationController";
            public const string DescriptionsController = "DescriptionsController";

            // CheckoutRequests Controller
            public const string CheckoutRequestsExController = "CheckoutRequestsExController";

            // PaymentRequests Controller
            public const string PaymentRequestsExController = "PaymentRequestsExController";
        }

        internal static class ExceptionDataKeys
        {
            public const string PIDLErrorCode = "Microsoft.Commerce.Payments.PidlFactory.PidlErrorCodes";
        }

        internal static class Defaults
        {
            public const string Locale = "en-us";
            public const string Language = "en";
            public const string All = "all";
            public const string Active = "active";
            public const string DeviceId = "";
            public const string Context = "";
            public const string Country = "us";
        }

        internal static class ClientNames
        {
            public const string Risk = "Risk";
            public const string Oms = "Oms";
            public const string TransactionService = "TransactionService";
            public const string Est = "Est";
            public const string Skype = "Skype";
            public const string CmatOld = "CmatOld";
            public const string CmatNew = "CmatNew";
            public const string MemberServices = "MemberServices";
            public const string Billing = "Billing";
            public const string Seller = "Seller";
            public const string Ccmt = "Ccmt";
            public const string Subpoena = "Subpoena";
            public const string BAM = "BAM";
            public const string Tax = "Tax";
            public const string PaymentExperience = "PaymentExperience";
            public const string AnonymousCaller = "AnonymousCaller";
            public const string Unknown = "UnknownCaller";
        }

        internal static class EndPointNames
        {
            public const string V7Probe = "probe";
            public const string V7GetPaymentInstrumentEx = "{version}/{accountId}/paymentInstrumentsEx/{piid}";
            public const string V7ListPaymentInstrumentEx = "{version}/{accountId}/paymentInstrumentsEx";
            public const string V7GetChallengeContextPaymentInstrumentEx = "{version}/{accountId}/paymentInstrumentsEx/{piid}/getChallengeContext";
            public const string V7ResumePendingOperationEx = "{version}/{accountId}/paymentInstrumentsEx/{piid}/resume";
            public const string V7AnonymousResumePendingOperationEx = "{version}/paymentInstrumentsEx/{piid}/resume";
            public const string V7CreatePaymentInstrumentEx = "{version}/paymentInstrumentsEx/create";
            public const string V7RemovePaymentInstrumentEx = "{version}/{accountId}/paymentInstrumentsEx/{piid}/remove";
            public const string V7UpdatePaymentInstrumentEx = "{version}/{accountId}/paymentInstrumentsEx/{piid}/update";
            public const string V7ReplacePaymentInstrumentEx = "{version}/{accountId}/paymentInstrumentsEx/{piid}/replace";
            public const string V7RedeemPaymentInstrumentEx = "{version}/{accountId}/paymentInstrumentsEx/{piid}/redeem";
            public const string V7GetCardProfileEx = "{version}/{accountId}/paymentInstrumentsEx/{piid}/cardProfile";
            public const string V7GetSeCardPersosEx = "{version}/{accountId}/paymentInstrumentsEx/{piid}/seCardPersos";
            public const string V7AcquireLuksEx = "{version}/{accountId}/paymentInstrumentsEx/{piid}/acquireLuk";
            public const string V7ConfirmLuksEx = "{version}/{accountId}/paymentInstrumentsEx/{piid}/confirmNotification";
            public const string V7PostReplenishTransactionCredentialsEx = "{version}/{accountId}/paymentInstrumentsEx/{piid}/replenishTransactionCredentials";
            public const string V7ValidateCvvEx = "{version}/{accountId}/paymentInstrumentsEx/{piid}/validateCvv";
            public const string V7GetSettings = "{version}/settings/{appName}/{appVersion}";
            public const string V7GetSettingsInPost = "{version}/{accountId}/settings";
            public const string V7PaymentSessions = "{version}/{accountId}/paymentSessions/";
            public const string V7PaymentSessionsGet = "{version}/{accountId}/paymentSessions/{sessionId}/status";
            public const string V7QrCodePaymentSessionsGet = "{version}/{accountId}/secondScreenSessions/{sessionId}/qrCodeStatus";
            public const string V7PaymentSessionsCreateAndAuthenticate = "{version}/{accountId}/paymentSessions/createAndAuthenticate";
            public const string V7PaymentSessionsAuthenticate = "{version}/{accountId}/paymentSessions/{sessionId}/authenticate";
            public const string V7PaymentSessionsBrowserAuthenticateThreeDSOne = "{version}/{accountId}/paymentSessions/{sessionId}/browserAuthenticateThreeDSOne";
            public const string V7PaymentSessionsAuthenticateIndiaThreeDS = "{version}/{accountId}/paymentSessions/{sessionId}/authenticateIndiaThreeDS";
            public const string V7PaymentSessionsNotifyThreeDSChallengeCompleted = "{version}/{accountId}/paymentSessions/{sessionId}/notifyThreeDSChallengeCompleted";
            public const string V7PaymentTransactions = "{version}/{accountId}/paymentTransactions";
            public const string V7AddressesEx = "{version}/{accountId}/addressesEx/";
            public const string V7AddressesExWithId = "{version}/{accountId}/addressesEx/{addressId}";
            public const string V7ApplyPaymentInstrumentEx = "{version}/paymentInstrumentsEx";
            public const string V7MSRewards = "{version}/{accountId}/msRewards/";
            public const string V7PaymentSessionsAuthenticationStatus = "{version}/{accountId}/paymentSessions/{sessionId}/{piId}/authenticationStatus";
            public const string V7AttachAddressCheckoutRequestsEx = "{version}/paymentClient/checkoutRequestsEx/{checkoutRequestId}/attachAddress";
            public const string V7AttachProfileCheckoutRequestsEx = "{version}/paymentClient/checkoutRequestsEx/{checkoutRequestId}/attachProfile";
            public const string V7ConfirmCheckoutRequestsEx = "{version}/paymentClient/checkoutRequestsEx/{checkoutRequestId}/confirm";
            public const string V7AttachPaymentInstrumentCheckoutRequestsEx = "{version}/paymentClient/checkoutRequestsEx/{checkoutRequestId}/attachPaymentInstrument";
            public const string V7ExpressCheckoutConfirm = "{version}/{accountId}/expressCheckout/confirm";
            public const string V7TokensEx = "{version}/{accountId}/tokensEx";
            public const string V7TokensExChallenge = "{version}/{accountId}/tokensEx/{ntid}/challenges/{challengeid}";
            public const string V7TokensExValidateChallenge = "{version}/{accountId}/tokensEx/{ntid}/challenges/{challengeid}/validate";
            public const string V7TokensExMandate = "{version}/{accountId}/tokensEx/{ntid}/mandates";

            // Pidl Endpoints
            public const string V7AgenticTokenDescriptions = "{version}/{accountId}/agenticTokenDescriptions/";
            public const string V7PaymentMethodDescriptions = "{version}/{accountId}/paymentMethodDescriptions/";
            public const string V7ChallengeDescriptions = "{version}/{accountId}/challengeDescriptions/";
            public const string V7AddressDescriptions = "{version}/{accountId}/addressDescriptions/";
            public const string V7ProfileDescriptions = "{version}/{accountId}/profileDescriptions/";
            public const string V7BillingGroupDescriptions = "{version}/{accountId}/billingGroupDescriptions/";
            public const string V7TaxIdDescriptions = "{version}/{accountId}/taxIdDescriptions/";
            public const string V7TenantDescriptions = "{version}/{accountId}/tenantDescriptions/";
            public const string V7GetPaymentSessionDescription = "{version}/{accountId}/paymentSessionDescriptions/";
            public const string V7ProvisionWalletToken = "{version}/{accountId}/provisionWalletToken";
            public const string V7RewardsDescriptions = "{version}/{accountId}/rewardsDescriptions/";
            public const string V7ConfirmPaymentRequestsEx = "{version}/paymentClient/paymentRequestsEx/{paymentRequestId}/confirm";
            public const string V7AttachChallengeDataPaymentRequestsEx = "{version}/paymentClient/paymentRequestsEx/{paymentRequestId}/attachChallengeData";
            public const string V7RemoveEligiblePaymentmethodsPaymentRequestsEx = "{version}/paymentClient/paymentRequestsEx/{paymentRequestId}/removeEligiblePaymentmethods";

            // Anonymous Endpoints
            public const string V7Transformation = "{version}/transformation/";
            public const string V7Validation = "{version}/validation/";
            public const string V7AnonymousAddressDescriptions = "{version}/addressDescriptions/";
            public const string V7AnonymousTaxIdDescriptions = "{version}/taxIdDescriptions/";
            public const string V7AnonymousPaymentMethodDescriptions = "{version}/paymentMethodDescriptions/";
            public const string V7AnonymousPaymentMethodDescriptionsSessionId = "{version}/paymentMethodDescriptions/{sessionId}";
            public const string V7SessionsById = "{version}/sessions/{sessionId}";
            public const string V7Sessions = "{version}/sessions/";
            public const string V7AnonymousLegacyAddressValidation = "{version}/addresses/legacyValidate";
            public const string V7AnonymousModernAddressValidation = "{version}/addresses/modernValidate";
            public const string V7AnonymousRDSSessionQuery = "{version}/rdssession/query";

            public const string V7BrowserFlowAuthenticate = "{version}/paymentSessions/{sessionId}/authenticate";
            public const string V7PaymentSessionsBrowserAuthenticateRedirectionThreeDSOne = "{version}/paymentSessions/{sessionId}/browserAuthenticateRedirectionThreeDSOne";
            public const string V7BrowserFlowPaymentSessionsNotifyChallengeCompleted = "{version}/paymentSessions/{sessionId}/notifyThreeDSChallengeCompleted";
            public const string V7PaymentSessionsBrowserNotifyThreeDSOneChallengeCompleted = "{version}/paymentSessions/{sessionId}/browserNotifyThreeDSOneChallengeCompleted";

            public const string V7AnonymousCheckoutsExCharge = "{version}/CheckoutsEx/{checkoutId}/charge";
            public const string V7AnonymousCheckoutDescriptions = "{version}/checkoutDescriptions";
            public const string V7AnonymousCheckoutsExStatus = "{version}/CheckoutsEx/{checkoutId}/status";
            public const string V7AnonymousCheckoutsExCompleted = "{version}/CheckoutsEx/{checkoutId}/completed";
            public const string V7AnonymousGetWalletConfig = "{version}/getWalletConfig";
            public const string V7AnonymousWalletSetupProviderSession = "{version}/setupWalletProviderSession";

            // Anonymous or authenticated endpoints
            public const string V7Cards = "{version}/cards/";
            public const string V7Assets = "{version}/assets/";

            // Payment Client
            public const string V7PaymentClientInitialization = "{version}/PaymentClient/Initialize";
            public const string V7Descriptions = "{version}/paymentClient/descriptions";
        }

        internal static class HTTPVerbs
        {
            public const string GET = "GET";
            public const string POST = "POST";
            public const string GETPOST = "GET,POST";
            public const string PATCH = "PATCH";
            public const string PUT = "PUT";
        }

        internal static class ClientRoles
        {
            public const string Admin = "Admin";
            public const string Test = "User";
        }

        internal static class HttpMethods
        {
            public const string Get = "GET";
            public const string Post = "POST";
            public const string Delete = "DELETE";
        }

        internal static class HeaderValues
        {
            public const string JsonContent = "application/json";
            public const string TextContent = "text/plain";
            public const string PidlSdkVersion = "x-ms-pidlsdk-version";
            public const string CorrelationId = "x-ms-correlation-id";
            public const string ExtendedFlightName = "x-ms-flight";
            public const string MsaProfileHeader = "x-ms-msaprofile";
            public const string ClientContextEncoding = "x-ms-clientcontext-encoding";
            public const string DeviceInfoHeader = "x-ms-deviceinfo";
            public const string RiskInfoHeader = "x-ms-riskinfo";
            public const string AuthInfoHeader = "x-ms-authinfo";
            public const string AadInfoHeader = "x-ms-aadinfo";
            public const string ForwardedHostHeader = "x-forwarded-host";
            public const string OfferId = "x-ms-offerid";
            public const string RetailServerInfoHeader = "x-ms-retailserverinfo";
            public const string IsMotoHeader = "x-ms-ismoto";
            public const string CustomerHeader = "x-ms-customer";
            public const string IfMatch = "If-Match";
            public const string XboxProfile = "x-ms-xboxprofile";
            public const string Version = "x-ms-version";
            public const string RequestContext = "request-context";
            public const string XMsRequestContext = "x-ms-request-context";
            public const string CorrelationContext = "Correlation-Context";
        }

        internal static class ClientContextGroups
        {
            internal const string MsaProfile = "MsaProfile";
            internal const string CMProfile = "CMProfile";
            internal const string CMProfileV3 = "CMProfileV3";
            internal const string CMProfileAddress = "CMProfileAddress";
            internal const string CMProfileAddressV3 = "CMProfileAddressV3";
            internal const string LegacyBillableAccountAddress = "LegacyBillableAccountAddress";
            internal const string TaxData = "TaxData";
            internal const string DeviceInfo = "DeviceInfo";
            internal const string AuthInfo = "AuthInfo";
            internal const string AadInfo = "AadInfo";
            internal const string RetailServerInfo = "RetailServerInfo";
            internal const string CMLegalEntityProfile = "CMLegalEntityProfile";
            internal const string XboxProfile = "XboxProfile";

            private static Dictionary<string, string> headerMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { HeaderValues.MsaProfileHeader, MsaProfile },
                { HeaderValues.DeviceInfoHeader, DeviceInfo },
                { HeaderValues.AuthInfoHeader, AuthInfo },
                { HeaderValues.AadInfoHeader, AadInfo },
                { HeaderValues.RetailServerInfoHeader, RetailServerInfo },
                { HeaderValues.XboxProfile, XboxProfile },
            };

            public static Dictionary<string, string> FromHeader
            {
                get { return headerMapping; }
            }
        }

        internal static class ClientActionContract
        {
            internal const string NoMessage = "[]";
        }

        internal static class CMProfileFields
        {
            internal const string FirstName = "firstName";
            internal const string LastName = "lastName";
            internal const string EmailAddress = "emailAddress";
            internal const string ProfileId = "id";
            internal const string CompanyName = "company_name";
            internal const string Culture = "culture";
            internal const string DefaultAddressId = "default_address_id";
            internal const string ProfileType = "type";
            internal const string Nationality = "nationality";
            internal const string BirthDate = "birth_date";
        }

        internal static class CMProfileV3Fields
        {
            internal const string ProfileId = "id";
            internal const string Etag = "etag";
            internal const string CustomerId = "customer_id";
            internal const string DefaultAddressId = "default_address_id";
            internal const string Country = "country";
            internal const string Culture = "culture";
            internal const string SnapshotId = "snapshot_id";
            internal const string Links = "links";
            internal const string ObjectType = "object_type";
            internal const string ResourceStatus = "resource_status";
            internal const string ProfileType = "type";
            internal const string FirstName = "first_name";
            internal const string LastName = "last_name";
            internal const string CompanyName = "company_name";
            internal const string Language = "language";
            internal const string LoveCode = "love_code";
            internal const string MobileBarcode = "mobile_barcode";
            internal const string Name = "name";
            internal const string Email = "email";
            internal const string EmailAddress = "email_address";
            internal const string LocaleId = "locale_id";
            internal const string BirthDate = "birth_date";
            internal const string Nationality = "nationality";
            internal const string Hcti = "hcti";
        }

        internal static class LegalEntityProfileFields
        {
            internal const string ProfileId = "id";
            internal const string Etag = "etag";
            internal const string CustomerId = "customer_id";
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string AddressLine3 = "address_line3";
            internal const string City = "city";
            internal const string Region = "region";
            internal const string PostalCode = "postal_code";
            internal const string District = "district";
            internal const string Country = "country";
            internal const string PhoneNumber = "phone_number";
            internal const string DefaultAddressId = "default_address_id";
            internal const string Culture = "culture";
            internal const string SnapshotId = "snapshot_id";
            internal const string Links = "links";
            internal const string ObjectType = "object_type";
            internal const string ResourceStatus = "resource_status";
            internal const string ProfileType = "type";
            internal const string FirstName = "first_name";
            internal const string LastName = "last_name";
            internal const string MiddleName = "middle_name";
            internal const string CompanyName = "company_name";
            internal const string Language = "language";
            internal const string EmailAddress = "email_address";
        }

        internal static class CMAddressFields
        {
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string AddressLine3 = "address_line3";
            internal const string City = "city";
            internal const string Region = "region";
            internal const string PostalCode = "postal_code";
            internal const string Country = "country";
        }

        internal static class CMAddressV3Fields
        {
            internal const string CustomerId = "customer_id";
            internal const string Country = "country";
            internal const string Region = "region";
            internal const string District = "district";
            internal const string City = "city";
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string AddressLine3 = "address_line3";
            internal const string PostalCode = "postal_code";
            internal const string FirstName = "first_name";
            internal const string FirstNamePronunciation = "first_name_pronunciation";
            internal const string LastName = "last_name";
            internal const string LastNamePronunciation = "last_name_pronunciation";
            internal const string MiddleName = "middle_name";
            internal const string CorrespondenceName = "correspondence_name";
            internal const string PhoneNumber = "phone_number";
            internal const string Mobile = "mobile";
            internal const string Fax = "fax";
            internal const string Telex = "telex";
            internal const string EmailAddress = "email_address";
            internal const string WebSiteUrl = "web_site_url";
            internal const string StreetSupplement = "street_supplement";
            internal const string IsWithinCityLimits = "is_within_city_limits";
            internal const string FormOfAddress = "form_of_address";
            internal const string AddressNotes = "address_notes";
            internal const string TimeZone = "time_zone";
            internal const string Latitude = "latitude";
            internal const string Longitude = "longitude";
            internal const string IsAvsValidated = "is_avs_validated";
            internal const string Validate = "validate";
            internal const string ValidationStamp = "validation_stamp";
            internal const string Links = "links";
            internal const string ObjectType = "object_type";
            internal const string ContractVersion = "contract_version";
            internal const string ResourceStatus = "resource_status";
            internal const string IsUserConsented = "is_customer_consented";
        }

        internal static class LegacyBillableAccountAddressFields
        {
            internal const string Region = "region";
            internal const string District = "district";
            internal const string City = "city";
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string AddressLine3 = "address_line3";
            internal const string PostalCode = "postal_code";
        }

        internal static class TaxDataFields
        {
            internal const string Value = "value";
        }

        internal static class CommercialZipPlusFourPropertyNames
        {
            internal const string IsUserConsented = "is_customer_consented";
            internal const string IsAvsFullValidationSucceeded = "is_avs_full_validation_succeeded";
        }

        // TODO Bug 1682888:[PX AP] Make address prefilling selective by country
        internal static class ClientContextKeys
        {
            internal const string Format = "{0}.{1}";

            internal static class MsaProfile
            {
                internal static readonly string FirstName = string.Format(Format, ClientContextGroups.MsaProfile, "firstName");
                internal static readonly string LastName = string.Format(Format, ClientContextGroups.MsaProfile, "lastName");
                internal static readonly string EmailAddress = string.Format(Format, ClientContextGroups.MsaProfile, "emailAddress");
                internal static readonly string Puid = string.Format(Format, ClientContextGroups.MsaProfile, "PUID");
            }

            internal static class RetailServerInfo
            {
                internal static readonly string MerchantId = string.Format(Format, ClientContextGroups.RetailServerInfo, RetailServerInfoNames.MarketId);
            }

            internal static class CMProfile
            {
                internal static readonly string FirstName = string.Format(Format, ClientContextGroups.CMProfile, CMProfileFields.FirstName);
                internal static readonly string LastName = string.Format(Format, ClientContextGroups.CMProfile, CMProfileFields.LastName);
                internal static readonly string EmailAddress = string.Format(Format, ClientContextGroups.CMProfile, CMProfileFields.EmailAddress);
                internal static readonly string DefaultAddressId = string.Format(Format, ClientContextGroups.CMProfile, CMProfileFields.DefaultAddressId);
                internal static readonly string Culture = string.Format(Format, ClientContextGroups.CMProfile, CMProfileFields.Culture);
                internal static readonly string CompanyName = string.Format(Format, ClientContextGroups.CMProfile, CMProfileFields.CompanyName);
                internal static readonly string Nationality = string.Format(Format, ClientContextGroups.CMProfile, CMProfileFields.Nationality);
                internal static readonly string ProfileId = string.Format(Format, ClientContextGroups.CMProfile, CMProfileFields.ProfileId);
                internal static readonly string ProfileType = string.Format(Format, ClientContextGroups.CMProfile, CMProfileFields.ProfileType);
                internal static readonly string BirthDate = string.Format(Format, ClientContextGroups.CMProfile, CMProfileFields.BirthDate);
            }

            internal static class CMProfileV3
            {
                internal static readonly string ProfileId = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.ProfileId);
                internal static readonly string Etag = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.Etag);
                internal static readonly string CustomerId = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.CustomerId);
                internal static readonly string DefaultAddressId = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.DefaultAddressId);
                internal static readonly string Country = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.Country);
                internal static readonly string Culture = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.Culture);
                internal static readonly string SnapshotId = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.SnapshotId);
                internal static readonly string Links = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.Links);
                internal static readonly string ObjectType = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.ObjectType);
                internal static readonly string ResourceStatus = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.ResourceStatus);
                internal static readonly string ProfileType = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.ProfileType);
                internal static readonly string FirstName = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.FirstName);
                internal static readonly string LastName = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.LastName);
                internal static readonly string CompanyName = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.CompanyName);
                internal static readonly string Language = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.Language);
                internal static readonly string LoveCode = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.LoveCode);
                internal static readonly string MobileBarcode = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.MobileBarcode);
                internal static readonly string Name = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.Name);
                internal static readonly string Email = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.Email);
                internal static readonly string EmailAddress = string.Format(Format, ClientContextGroups.CMProfileV3, CMProfileV3Fields.EmailAddress);
            }

            internal static class CMProfileAddress
            {
                internal static readonly string AddressLine1 = string.Format(Format, ClientContextGroups.CMProfileAddress, CMAddressFields.AddressLine1);
                internal static readonly string AddressLine2 = string.Format(Format, ClientContextGroups.CMProfileAddress, CMAddressFields.AddressLine2);
                internal static readonly string AddressLine3 = string.Format(Format, ClientContextGroups.CMProfileAddress, CMAddressFields.AddressLine3);
                internal static readonly string City = string.Format(Format, ClientContextGroups.CMProfileAddress, CMAddressFields.City);
                internal static readonly string Region = string.Format(Format, ClientContextGroups.CMProfileAddress, CMAddressFields.Region);
                internal static readonly string PostalCode = string.Format(Format, ClientContextGroups.CMProfileAddress, CMAddressFields.PostalCode);
                internal static readonly string Country = string.Format(Format, ClientContextGroups.CMProfileAddress, CMAddressFields.Country);
            }

            internal static class CMProfileAddressV3
            {
                internal static readonly string CustomerId = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.CustomerId);
                internal static readonly string Country = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.Country);
                internal static readonly string Region = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.Region);
                internal static readonly string District = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.District);
                internal static readonly string City = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.City);
                internal static readonly string AddressLine1 = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.AddressLine1);
                internal static readonly string AddressLine2 = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.AddressLine2);
                internal static readonly string AddressLine3 = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.AddressLine3);
                internal static readonly string PostalCode = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.PostalCode);
                internal static readonly string FirstName = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.FirstName);
                internal static readonly string FirstNamePronunciation = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.FirstNamePronunciation);
                internal static readonly string LastName = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.LastName);
                internal static readonly string LastNamePronunciation = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.LastNamePronunciation);
                internal static readonly string MiddleName = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.MiddleName);
                internal static readonly string CorrespondenceName = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.CorrespondenceName);
                internal static readonly string PhoneNumber = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.PhoneNumber);
                internal static readonly string Mobile = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.Mobile);
                internal static readonly string Fax = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.Fax);
                internal static readonly string Telex = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.Telex);
                internal static readonly string EmailAddress = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.EmailAddress);
                internal static readonly string WebSiteUrl = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.WebSiteUrl);
                internal static readonly string StreetSupplement = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.StreetSupplement);
                internal static readonly string IsWithinCityLimits = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.IsWithinCityLimits);
                internal static readonly string FormOfAddress = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.FormOfAddress);
                internal static readonly string AddressNotes = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.AddressNotes);
                internal static readonly string TimeZone = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.TimeZone);
                internal static readonly string Latitude = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.Latitude);
                internal static readonly string Longitude = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.Longitude);
                internal static readonly string IsAvsValidated = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.IsAvsValidated);
                internal static readonly string Validate = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.Validate);
                internal static readonly string ValidationStamp = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.ValidationStamp);
                internal static readonly string Links = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.Links);
                internal static readonly string ObjectType = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.ObjectType);
                internal static readonly string ContractVersion = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.ContractVersion);
                internal static readonly string ResourceStatus = string.Format(Format, ClientContextGroups.CMProfileAddressV3, CMAddressV3Fields.ResourceStatus);
            }

            internal static class LegacyBillableAccountAddress
            {
                internal static readonly string Region = string.Format(Format, ClientContextGroups.LegacyBillableAccountAddress, LegacyBillableAccountAddressFields.Region);
                internal static readonly string District = string.Format(Format, ClientContextGroups.LegacyBillableAccountAddress, LegacyBillableAccountAddressFields.District);
                internal static readonly string City = string.Format(Format, ClientContextGroups.LegacyBillableAccountAddress, LegacyBillableAccountAddressFields.City);
                internal static readonly string AddressLine1 = string.Format(Format, ClientContextGroups.LegacyBillableAccountAddress, LegacyBillableAccountAddressFields.AddressLine1);
                internal static readonly string AddressLine2 = string.Format(Format, ClientContextGroups.LegacyBillableAccountAddress, LegacyBillableAccountAddressFields.AddressLine2);
                internal static readonly string AddressLine3 = string.Format(Format, ClientContextGroups.LegacyBillableAccountAddress, LegacyBillableAccountAddressFields.AddressLine3);
                internal static readonly string PostalCode = string.Format(Format, ClientContextGroups.LegacyBillableAccountAddress, LegacyBillableAccountAddressFields.PostalCode);
            }

            internal static class TaxData
            {
                internal static readonly string TaxId = string.Format(Format, ClientContextGroups.TaxData, TaxDataFields.Value);
            }

            internal static class DeviceInfo
            {
                internal static readonly string XboxLiveDeviceId = string.Format(Format, ClientContextGroups.DeviceInfo, "xboxLiveDeviceId");
                internal static readonly string IPAddress = string.Format(Format, ClientContextGroups.DeviceInfo, "ipAddress");
                internal static readonly string UserAgent = string.Format(Format, ClientContextGroups.DeviceInfo, "userAgent");
                internal static readonly string DeviceId = string.Format(Format, ClientContextGroups.DeviceInfo, "deviceId");
            }            

            internal static class CMLegalEntityProfile
            {
                internal static readonly string FirstName = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.FirstName);
                internal static readonly string MiddleName = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.MiddleName);
                internal static readonly string LastName = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.LastName);
                internal static readonly string AddressLine1 = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.AddressLine1);
                internal static readonly string AddressLine2 = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.AddressLine2);
                internal static readonly string AddressLine3 = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.AddressLine3);
                internal static readonly string City = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.City);
                internal static readonly string Region = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.Region);
                internal static readonly string District = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.District);
                internal static readonly string PostalCode = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.PostalCode);
                internal static readonly string EmailAddress = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.EmailAddress);
                internal static readonly string PhoneNumber = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.PhoneNumber);
                internal static readonly string CompanyName = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.CompanyName);
                internal static readonly string Culture = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.Culture);
                internal static readonly string Language = string.Format(Format, ClientContextGroups.CMLegalEntityProfile, LegalEntityProfileFields.Language);
            }

            internal static class AadInfo
            {
                internal static readonly string AltSecId = string.Format(Format, ClientContextGroups.AadInfo, "altSecId");
                internal static readonly string OrgPuid = string.Format(Format, ClientContextGroups.AadInfo, "orgPuid");
                internal static readonly string Tid = string.Format(Format, ClientContextGroups.AadInfo, "tid");
                internal static readonly string Oid = string.Format(Format, ClientContextGroups.AadInfo, "oid");
            }

            internal static class XboxProfile
            {
                internal static readonly string Gamertag = string.Format(Format, ClientContextGroups.XboxProfile, "gamertag");
            }
        }

        internal static class DeviceClass
        {
            internal const string Web = "Web";
            internal const string Mobile = "MobileApp";
            internal const string Console = "GameConsole";
        }

        internal static class OperatingSystem
        {
            internal const string Windows = "windows";
            internal const string Android = "android";
            internal const string IOS = "ios";
        }

        internal static class DeviceIdNames
        {
            public const string DeviceId = "deviceId";
            public const string XboxLiveDeviceId = "xboxLiveDeviceId";
        }

        internal static class AuthInfoNames
        {
            public const string Type = "type";
            public const string Context = "context";
        }

        internal static class RetailServerInfoNames
        {
            public const string MarketId = "merchantId";
            public const string ShopperId = "shopperId";
        }

        internal static class AccountServiceApiVersion
        {
            public const string V2 = "2014-09-01";
            public const string V3 = "2015-03-31";
        }

        internal static class PartnerGuids
        {
            public const string Azure = "fbf178a5-144e-46d1-aa81-612c2d3f97f4";
        }

        internal static class Namespaces
        {
            public const string Risk = "RISK";
        }

        internal static class ProfileTypes
        {
            public const string Consumer = "consumer";
            public const string ConsumerV3 = "consumerV3";
            public const string Organization = "organization";
            public const string Employee = "employee";
            public const string Isv = "isv";
            public const string Legal = "legalentity";
        }

        internal static class AccountTypes
        {
            public const string Consumer = "msa";
            public const string Organization = "org";
            public const string AAD = "aad";
            public const string Bing = "bing";
        }

        internal static class ErrorCodes
        {
            public const string JsonDeserializationError = "JsonDeserializationError";
            public const string NetworkError = "NetworkError";
            public const string SqlError = "SqlError";
            public const string RegistrationNotFound = "RegistrationNotFound";
            public const string PIDLInvalidFilters = "FiltersIsInvalid";
        }

        internal static class PXServiceErrorCodes
        {
            internal const string LegacyBillableAccountUpdateFailed = "LegacyBillableAccountUpdateFailed";
            internal const string LegacyAccountServiceFailed = "LegacyAccountServiceFailed";
            internal const string CTPCommerceServiceFailed = "CTPCommerceServiceFailed";
        }

        internal static class LegacyAccountErrorCodes
        {
            internal const string InvalidAddress = "InvalidAddress";
            internal const string InvalidCity = "InvalidCity";
            internal const string InvalidState = "InvalidState";
            internal const string InvalidZipCode = "InvalidZipCode";
        }

        internal static class PostAddressErrorCodes
        {
            internal const string InvalidStreet = "InvalidStreet";
            internal const string InvalidCity = "InvalidCity";
            internal const string InvalidPostalCode = "InvalidPostalCode";
            internal const string InvalidRegion = "InvalidRegion";
            internal const string InvalidAddressFieldsCombination = "InvalidAddressFieldsCombination";
            internal const string InvalidParameter = "InvalidParameter";
            internal const string JarvisAddressFieldCombination = "invalid_address_fields_combination";
        }

        internal static class LegacyAccountErrorMessages
        {
            internal const string InvalidAddress = "Check your address. There appears to be an error in it.";
            internal const string InvalidAddressRequiredFieldMissing = "Check your address. A required field is missing.";
            internal const string InvalidCity = "Check the city in your address. There appears to be an error in it.";
            internal const string InvalidCountry = "Choose your country or region again. There appears to be an error in it.";
            internal const string InvalidState = "Check the state in your address. There appears to be an error in it.";
            internal const string InvalidZipCode = "Check the Zip or Postal code in your address. There appears to be an error in it.";
        }

        internal static class AddressErrorTargets
        {
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string AddressLine3 = "address_line3";
            internal const string City = "city";
            internal const string State = "region";
            internal const string PostalCode = "postal_code";
        }

        internal static class StoredProcParameters
        {
            public static class InsertRegistration
            {
                public const string AppId = "@AppId";
                public const string DeviceId = "@DeviceId";
                public const string NotificationUri = "@NotificationUri";
                public const string CommerceAccountId = "@CommerceAccountId";
                public const string Status = "@Status";
                public const string Id = "@Id";
            }
        }

        internal static class OAuthData
        {
            public const string ContentFormat = "grant_type=client_credentials&client_id={0}&client_secret={1}&scope=notify.windows.com";
            public const string ContentTypeHeaderValue = "application/x-www-form-urlencoded";
            public const string AuthorizationHeaderPrefix = "Bearer ";
            public const string DefaultScopeName = ".default";
        }

        internal static class RequestPropertyKeys
        {
            public const string FlightContext = "PX.FlightContext";
            public const string ExposedFlightFeatures = "PX.ExposedFlightFeatures";
            public const string FlightFeatureConfig = "PX.FlightFeatureConfig";
            public const string FlightAssignmentContext = "PX.FlightAssignmentContext";
            public const string PartnerSettings = "PX.PartnerSettings";
        }

        internal static class ScenarioNames
        {
            public const string Commercialhardware = "commercialhardware";
            public const string GuestCheckoutPrepaidMeeting = "prepaidmeeting";
        }

        internal static class PaymentRequestContext
        {
            // Refer teams contract with Teams. 
            // https://microsoft.sharepoint.com/:w:/t/ExtensibilityandFundamentals/EWbi75-vg_RNoZXU9JsquvwB93qW096N79_LBOqFfTdBow?e=GhaxNd&CID=80398A44-1BA7-489B-BA5E-CA1A7C550856&wdLOR=c089CEA99-883A-4BD8-B990-3C80089A33FC
            public const string PrePaidMeeting = "prepaidmeeting";
        }

        internal static class QueryParamNames
        {
            public const string BillableAccountId = "billableAccountId";
            public const string ClassicProduct = "classicProduct";
            public const string Partner = "partner";
            public const string Country = "country";
        }

        internal static class PayerAuthApiVersions
        {
            public const string V2 = "2018-10-03";
            public const string V3 = "2019-04-16";
        }

        internal static class PurchaseApiVersions
        {
            public const string V6 = "v6.0";
            public const string V7 = "v7.0";
            public const string V8 = "v8.0";
        }

        internal static class D365ServiceApiVersions
        {
            public const string V1 = "v1.0";
        }

        internal static class CatalogApiVersions
        {
            public const string V8 = "v8.0";
        }

        internal static class IssuerServiceApiVersions
        {
            public const string V1 = "v1.0";
        }

        internal static class TokenPolicyServiceApiVersions
        {
            public const string V1 = "v1.0";
        }

        internal static class CountryCodes
        {
            public const string AT = "AT";
            public const string CN = "CN";
            public const string DE = "DE";
            public const string IN = "IN";
            public const string NL = "NL";
            public const string TR = "TR";
            public const string AE = "AE";
            public const string VE = "VE";
            public const string US = "US";
        }

        internal static class PaymentErrorMessages
        {
            internal const string CountryNotSupported = "The country is not supported.";
        }

        internal static class AuthResult
        {
            public const string ByPass = "ByPass";
            public const string Succeed = "Succeed";
            public const string Failed = "Failed";
        }

        internal static class AbnormalDetection
        {
            public const string LogMsgWhenCaughtByPX = "Caught and rejected by PXService";
        }

        internal static class Value
        {
            public const string True = "true";
        }

        internal static class PSD2Constants
        {
            public const string DefaultBrowserRequestMessageVersion = "2.2.0";
            public const string FallbackMessageVersion = "2.1.0";
        }
    }
}