// <copyright file="EmulatorsRouteExtensions.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

public static class EmulatorsRouteExtensions
{
    internal static void MapPartnerSettingsRoutes(this IEndpointRouteBuilder routes)
    {
        routes.MapControllerRoute(
            name: Constants.PartnerSettingsApiName.GetPartnerSettings,
            pattern: "partnersettings/{partnerName}",
            defaults: new { controller = "PartnerSettings", action = Constants.PartnerSettingsApiName.GetPartnerSettings });
    }

    internal static void MapPIMSRoutes(this IEndpointRouteBuilder routes)
    {
        // PIMS
        routes.MapControllerRoute(
                name: Constants.PIMSApiName.GetPI,
                pattern: "{version}/{accountId}/paymentInstruments/{piid}",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.GetPI });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.GetChallengeContext,
                pattern: "{version}/{accountId}/paymentInstruments/{piid}/GetChallengeContext",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.GetChallengeContext });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.AddPI,
                pattern: "{version}/{accountId}/paymentInstruments",
                defaults: new { controller = "PimsPaymentInstruments" });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.AddPIWithoutJarvisAccount,
                pattern: "{version}/paymentInstruments",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.AddPI });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.ResumeAddPI,
                pattern: "{version}/{accountId}/paymentInstruments/{piid}/resume",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.ResumeAddPI });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.UpdatePI,
                pattern: "{version}/{accountId}/paymentInstruments/{piid}/update",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.UpdatePI });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.ReplacePI,
                pattern: "{version}/{accountId}/paymentInstruments/{piid}/replace",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.ReplacePI });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.ValidateCvv,
                pattern: "{version}/{accountId}/paymentInstruments/{piid}/validateCvv",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.ValidateCvv });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.GetPM,
                pattern: "{version}/paymentMethods",
                defaults: new { controller = "PimsPaymentMethods", action = Constants.PIMSApiName.GetPM });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.GetSeCardPersos,
                pattern: "{version}/{accountId}/paymentInstruments/{piid}/seCardPersos",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.GetSeCardPersos });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.GetPIExtendedView,
                pattern: "{version}/paymentInstruments/{piid}/extendedView",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.GetPIExtendedView });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.Validate,
                pattern: "{version}/{accountId}/paymentInstruments/{piid}/Validate",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.Validate });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.LinkTransaction,
                pattern: "{version}/{accountId}/paymentInstruments/{piid}/LinkTransaction",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.LinkTransaction });

            routes.MapControllerRoute(
               name: Constants.PIMSApiName.SearchByAccountNumber,
               pattern: "{version}/paymentInstruments/searchByAccountNumber",
               defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.SearchByAccountNumber });

            routes.MapControllerRoute(
               name: Constants.PIMSApiName.ListEmpOrgPI,
               pattern: "{version}/emporg/paymentInstruments",
               defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.ListEmpOrgPI });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.GetSessionDetails,
                pattern: "{version}/{accountId}/sessions/{sessionId}/",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.GetSessionDetails });

            routes.MapControllerRoute(
                name: Constants.PIMSApiName.GetEligiblePaymentMethods,
                pattern: "{version}/thirdPartyPayments/eligiblePaymentMethods",
                defaults: new { controller = "PimsPaymentMethods", action = Constants.PIMSApiName.GetPM });
        }

        internal static void MapMSRewardsRoutes(this IEndpointRouteBuilder routes)
        {
            routes.MapControllerRoute(
                name: Constants.MSRewardsApiName.GetUserInfo,
                pattern: "api/users({userId})",
                defaults: new { controller = "MSRewardsGetUserInfo", action = Constants.MSRewardsApiName.GetUserInfo });

            routes.MapControllerRoute(
                name: Constants.MSRewardsApiName.GetUserInfoUserIdEmpty,
                pattern: "api/users()",
                defaults: new { controller = "MSRewardsGetUserInfo", action = Constants.MSRewardsApiName.GetUserInfoUserIdEmpty });

            routes.MapControllerRoute(
                name: Constants.MSRewardsApiName.RedeemRewards,
                pattern: "api/users({userId})/orders",
                defaults: new { controller = "RedeemRewards", action = Constants.MSRewardsApiName.RedeemRewards });

            routes.MapControllerRoute(
                name: Constants.MSRewardsApiName.RedeemRewardsUserIdEmpty,
                pattern: "api/users()/orders",
                defaults: new { controller = "RedeemRewards", action = Constants.MSRewardsApiName.RedeemRewards });
        }
        
        internal static void MapCatalogRoutes(this IEndpointRouteBuilder routes)
        {
            routes.MapControllerRoute(
                name: Constants.CatalogApiName.GetProducts,
                pattern: "V8.0/products",
                defaults: new { controller = "Catalog", action = Constants.CatalogApiName.GetProducts });
        }

        internal static void MapAccountRoutes(this IEndpointRouteBuilder routes)
        {
            routes.MapControllerRoute(
                name: Constants.AccountApiName.GetProfiles,
                pattern: "{accountId}/profiles",
                defaults: new { controller = "AccountProfiles", action = Constants.AccountApiName.GetProfiles });

            routes.MapControllerRoute(
                name: Constants.AccountApiName.PutProfile,
                pattern: "{accountId}/profiles/{profileId}",
                defaults: new { controller = "AccountProfiles", action = Constants.AccountApiName.PutProfile };

            routes.MapControllerRoute(
                name: Constants.AccountApiName.PatchProfile,
                pattern: "{accountId}/profiles/{profileId}",
                defaults: new { controller = "AccountProfiles", action = Constants.AccountApiName.PatchProfile };

            routes.MapControllerRoute(
                name: Constants.AccountApiName.GetCustomers,
                pattern: "customers/{accountId}",
                defaults: new { controller = "AccountCustomers", action = Constants.AccountApiName.GetCustomers });

            routes.MapControllerRoute(
                name: Constants.AccountApiName.PostAddress,
                pattern: "{accountId}/addresses",
                defaults: new { controller = "AccountAddresses", action = Constants.AccountApiName.PostAddress };

            routes.MapControllerRoute(
                name: Constants.AccountApiName.GetAddresses,
                pattern: "{accountId}/addresses",
                defaults: new { controller = "AccountAddresses", action = Constants.AccountApiName.GetAddresses };

            routes.MapControllerRoute(
               name: Constants.AccountApiName.GetAddress,
               pattern: "{accountId}/addresses/{addressId}",
               defaults: new { controller = "AccountAddresses" });

            routes.MapControllerRoute(
                name: Constants.AccountApiName.PostAddressValidate,
                pattern: "addresses/validate",
                defaults: new { controller = "AccountAddresses", action = Constants.AccountApiName.PostAddressValidate });

            routes.MapControllerRoute(
                name: Constants.AccountApiName.LegacyValidateAddress,
                pattern: "addresses",
                defaults: new { controller = "AccountAddresses", action = Constants.AccountApiName.LegacyValidateAddress });
        }

        internal static void MapIssuerServiceRoutes(this IEndpointRouteBuilder routes)
        {
            routes.MapControllerRoute(
                name: Constants.IssuerServiceApiName.Initialize,
                pattern: "applications/session",
                defaults: new { controller = "IssuerService", action = Constants.IssuerServiceApiName.Initialize });

            routes.MapControllerRoute(
                name: Constants.IssuerServiceApiName.Apply,
                pattern: "applications/{customerPuid}",
                defaults: new { controller = "IssuerService", action = Constants.IssuerServiceApiName.Apply });

            routes.MapControllerRoute(
                name: Constants.IssuerServiceApiName.Eligibility,
                pattern: "applications/{customerPuid}/eligibility",
                defaults: new { controller = "IssuerService", action = Constants.IssuerServiceApiName.Eligibility });
        }
        
        internal static void MapChallengeManagementRoutes(this IEndpointRouteBuilder routes)
        {
            routes.MapControllerRoute(
                name: Constants.ChallengeManagementApiName.CreateChallenge,
                pattern: "challenge/create",
                defaults: new { controller = "Challenge", action = Constants.ChallengeManagementApiName.CreateChallenge });

            routes.MapControllerRoute(
                name: Constants.ChallengeManagementApiName.GetChallengeStatus,
                pattern: "challenge/status/{sessionId}",
                defaults: new { controller = "Challenge", action = Constants.ChallengeManagementApiName.GetChallengeStatus });

            routes.MapControllerRoute(
                name: Constants.ChallengeManagementApiName.CreateChallengeSession,
                pattern: "challengesession/create",
                defaults: new { controller = "ChallengeManagementSession", action = Constants.ChallengeManagementApiName.CreateChallengeSession });

            routes.MapControllerRoute(
                name: Constants.ChallengeManagementApiName.GetChallengeSessionData,
                pattern: "challengesession/get/{sessionId}",
                defaults: new { controller = "ChallengeManagementSession", action = Constants.ChallengeManagementApiName.GetChallengeSessionData });

            routes.MapControllerRoute(
                name: Constants.ChallengeManagementApiName.UpdateChallengeSession,
                pattern: "challengesession/update",
                defaults: new { controller = "ChallengeManagementSession", action = Constants.ChallengeManagementApiName.UpdateChallengeSession });
        }
        
        internal static void MapPaymentThirdPartyRoutes(this IEndpointRouteBuilder routes)
        {
            routes.MapControllerRoute(
                name: Constants.PaymentThirdPartyApiName.GetCheckout,
                pattern: "payment-providers/{paymentProviderId}/api/checkouts/{checkoutId}",
                defaults: new { controller = "PaymentThirdPartyPaymentAndCheckouts", action = Constants.PaymentThirdPartyApiName.GetCheckout });

            routes.MapControllerRoute(
                name: Constants.PaymentThirdPartyApiName.Charge,
                pattern: "payment-providers/{paymentProviderId}/api/checkouts/{checkoutId}/charge",
                defaults: new { controller = "PaymentThirdPartyCheckoutCharge", action = Constants.PaymentThirdPartyApiName.Charge });

            routes.MapControllerRoute(
                name: Constants.PaymentThirdPartyApiName.GetPaymentRequest,
                pattern: "payment-providers/{paymentProviderId}/api/payment-requests/{paymentRequestId}",
                defaults: new { controller = "PaymentThirdPartyPaymentAndCheckouts", action = Constants.PaymentThirdPartyApiName.GetPaymentRequest });
        }

        internal static void MapPurchaseRoutes(this IEndpointRouteBuilder routes)
        {
            routes.MapControllerRoute(
                name: Constants.PurchaseApiName.ListOrder,
                pattern: "v7.0/users/{userId}/orders",
                defaults: new { controller = "Purchase", action = Constants.PurchaseApiName.ListOrder });

            routes.MapControllerRoute(
                name: Constants.PurchaseApiName.GetOrder,
                pattern: "v7.0/users/{userId}/orders/{orderId}",
                defaults: new { controller = "Purchase", action = Constants.PurchaseApiName.GetOrder });

            routes.MapControllerRoute(
                name: Constants.PurchaseApiName.GetSub,
                pattern: "v8.0/users/{userId}/recurrences/{recurrenceId}",
                defaults: new { controller = "Purchase", action = Constants.PurchaseApiName.GetSub });

            routes.MapControllerRoute(
                name: Constants.PurchaseApiName.ListSub,
                pattern: "v8.0/users/{userId}/recurrences",
                defaults: new { controller = "Purchase", action = Constants.PurchaseApiName.ListSub });

            routes.MapControllerRoute(
                name: Constants.PurchaseApiName.CheckPi,
                pattern: "v7.0/users/{userId}/paymentinstruments/{paymentinstrumentid}/check",
                defaults: new { controller = "Purchase", action = Constants.PurchaseApiName.CheckPi });
        }

        internal static void MapRiskRoutes(this IEndpointRouteBuilder routes)
        {
            routes.MapControllerRoute(
                name: Constants.RiskApiName.RiskEvaluation,
                pattern: "risk/risk-evaluation",
                defaults: new { controller = "Risk", action = Constants.RiskApiName.RiskEvaluation });
        }

        internal static void MapSellerMarketPlaceRoutes(this IEndpointRouteBuilder routes)
        {
            routes.MapControllerRoute(
                name: Constants.SellerMarketPlaceApiName.GetSeller,
                pattern: "v1/payment-providers/{paymentProviderId}/sellers/{sellerId}",
                defaults: new { controller = "Sellers", action = Constants.SellerMarketPlaceApiName.GetSeller });
        }

        internal static void MapTokenPolicyRoutes(this IEndpointRouteBuilder routes)
        {
            routes.MapControllerRoute(
                name: Constants.TokenPolicyApiName.GetTokenPolicyDescription,
                pattern: "{version}/users/{userid}/tokenDescriptionRequests",
                defaults: new { controller = "TokenPolicyDescription", action = Constants.TokenPolicyApiName.GetTokenPolicyDescription });
        }

        internal static void MapStoredValueRoutes(this IEndpointRouteBuilder routes)
        {
            routes.MapControllerRoute(
                name: Constants.StoredValueApiName.GetGiftCatalog,
                pattern: "gift-catalog",
                defaults: new { controller = "StoredValueGiftCatalog", action = Constants.StoredValueApiName.GetGiftCatalog });

            routes.MapControllerRoute(
                name: Constants.StoredValueApiName.PostFunding,
                pattern: "{legacyAccountId}/funds",
                defaults: new { controller = "StoredValueRedeem", action = Constants.StoredValueApiName.PostFunding });

            routes.MapControllerRoute(
               name: Constants.StoredValueApiName.GetFundingStatus,
               pattern: "{legacyAccountId}/funds/{referenceId}",
               defaults: new { controller = "StoredValueRedeem", action = Constants.StoredValueApiName.GetFundingStatus });
        }

        internal static void MapTransactionServiceRoutes(this IEndpointRouteBuilder routes)
        {
            routes.MapControllerRoute(
                name: Constants.TransactionServiceApiName.Payments,
                pattern: "{accountId}/payments",
                defaults: new { controller = "TransactionService", action = Constants.TransactionServiceApiName.Payments });

            routes.MapControllerRoute(
                name: Constants.TransactionServiceApiName.TransactionValidate,
                pattern: "{accountId}/payments/{paymentId}/validate",
                defaults: new { controller = "TransactionService", action = Constants.TransactionServiceApiName.TransactionValidate });
        }

        internal static void MapPaymentOchestratorRoutes(this IEndpointRouteBuilder routes)
        {
            // Checkout requests routes mapping
            routes.MapControllerRoute(
                name: Constants.PaymentOchestratorApiName.AttachAddress,
                pattern: "checkoutRequests/{checkoutId}/attachaddress",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.AttachAddress });

            routes.MapControllerRoute(
                name: Constants.PaymentOchestratorApiName.AttachProfile,
                pattern: "checkoutRequests/{checkoutId}/attachprofile",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.AttachProfile });

            routes.MapControllerRoute(
                name: Constants.PaymentOchestratorApiName.AttachPaymentInstruments,
                pattern: "checkoutRequests/{checkoutId}/attachpaymentinstruments",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.AttachPaymentInstruments });

            routes.MapControllerRoute(
                name: Constants.PaymentOchestratorApiName.Confirm,
                pattern: "checkoutRequests/{checkoutId}/confirm",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.Confirm });

            routes.MapControllerRoute(
                name: Constants.PaymentOchestratorApiName.GetClientAction,
                pattern: "checkoutRequests/{checkoutId}/clientaction",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.GetClientAction });

            // Payment requests routes mapping
            routes.MapControllerRoute(
                name: Constants.PaymentOchestratorApiName.GetClientActions,
                pattern: "paymentRequests/{paymentRequestId}/clientactions",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.GetClientActions });

            routes.MapControllerRoute(
                name: Constants.PaymentOchestratorApiName.PRAttachAddress,
                pattern: "paymentRequests/{paymentRequestId}/attachaddress",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.PRAttachAddress });

            routes.MapControllerRoute(
                name: Constants.PaymentOchestratorApiName.PRAttachProfile,
                pattern: "paymentRequests/{paymentRequestId}/attachprofile",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.PRAttachProfile });

            routes.MapControllerRoute(
                name: Constants.PaymentOchestratorApiName.PRAttachPaymentInstruments,
                pattern: "paymentRequests/{paymentRequestId}/attachpaymentinstruments",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.PRAttachPaymentInstruments });

            routes.MapControllerRoute(
                name: Constants.PaymentOchestratorApiName.PRConfirm,
                pattern: "paymentRequests/{paymentRequestId}/confirm",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.PRConfirm });
        }

        internal static void MapPayerAuthRoutes(this IEndpointRouteBuilder routes)
        {
            routes.MapControllerRoute(
               name: Constants.PayerAuthApiName.CreatePaymentSessionId,
               pattern: "CreatePaymentSessionId",
               defaults: new { controller = "PayerAuth", action = Constants.PayerAuthApiName.CreatePaymentSessionId });

            routes.MapControllerRoute(
               name: Constants.PayerAuthApiName.Get3DSMethodUrl,
               pattern: "getThreeDSMethodURL",
               defaults: new { controller = "PayerAuth", action = Constants.PayerAuthApiName.Get3DSMethodUrl });

            routes.MapControllerRoute(
               name: Constants.PayerAuthApiName.Authenticate,
               pattern: "authenticate",
               defaults: new { controller = "PayerAuth", action = Constants.PayerAuthApiName.Authenticate });

            routes.MapControllerRoute(
               name: Constants.PayerAuthApiName.Result,
               pattern: "result",
               defaults: new { controller = "PayerAuth", action = Constants.PayerAuthApiName.Result });

            routes.MapControllerRoute(
               name: Constants.PayerAuthApiName.CompleteChallenge,
               pattern: "CompleteChallenge",
               defaults: new { controller = "PayerAuth", action = Constants.PayerAuthApiName.CompleteChallenge });
        }
        
    internal static void MapFraudDetectionRoutes(this IEndpointRouteBuilder routes)
    {
        routes.MapControllerRoute(
           name: Constants.FraudDetectionApiName.BotCheck,
           pattern: "api/v1/botcheck",
           defaults: new { controller = "FraudDetection", action = Constants.FraudDetectionApiName.BotCheck });
    }
}
