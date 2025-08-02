// <copyright file="EmulatorsRouteExtensions.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators
{
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;

    public static class EmulatorsRouteExtensions
    {
        internal static void MapPartnerSettingsRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: Constants.PartnerSettingsApiName.GetPartnerSettings,
                routeTemplate: "partnersettings/{partnerName}",
                defaults: new { controller = "PartnerSettings", action = Constants.PartnerSettingsApiName.GetPartnerSettings });
        }

        internal static void MapPIMSRoutes(this HttpRouteCollection routes)
        {
            // PIMS
            routes.MapHttpRoute(
                name: Constants.PIMSApiName.GetPI,
                routeTemplate: "{version}/{accountId}/paymentInstruments/{piid}",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.GetPI });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.GetChallengeContext,
                routeTemplate: "{version}/{accountId}/paymentInstruments/{piid}/GetChallengeContext",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.GetChallengeContext });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.AddPI,
                routeTemplate: "{version}/{accountId}/paymentInstruments",
                defaults: new { controller = "PimsPaymentInstruments" });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.AddPIWithoutJarvisAccount,
                routeTemplate: "{version}/paymentInstruments",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.AddPI });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.ResumeAddPI,
                routeTemplate: "{version}/{accountId}/paymentInstruments/{piid}/resume",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.ResumeAddPI });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.UpdatePI,
                routeTemplate: "{version}/{accountId}/paymentInstruments/{piid}/update",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.UpdatePI });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.ReplacePI,
                routeTemplate: "{version}/{accountId}/paymentInstruments/{piid}/replace",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.ReplacePI });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.ValidateCvv,
                routeTemplate: "{version}/{accountId}/paymentInstruments/{piid}/validateCvv",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.ValidateCvv });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.GetPM,
                routeTemplate: "{version}/paymentMethods",
                defaults: new { controller = "PimsPaymentMethods", action = Constants.PIMSApiName.GetPM });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.GetSeCardPersos,
                routeTemplate: "{version}/{accountId}/paymentInstruments/{piid}/seCardPersos",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.GetSeCardPersos });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.GetPIExtendedView,
                routeTemplate: "{version}/paymentInstruments/{piid}/extendedView",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.GetPIExtendedView });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.Validate,
                routeTemplate: "{version}/{accountId}/paymentInstruments/{piid}/Validate",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.Validate });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.LinkTransaction,
                routeTemplate: "{version}/{accountId}/paymentInstruments/{piid}/LinkTransaction",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.LinkTransaction });

            routes.MapHttpRoute(
               name: Constants.PIMSApiName.SearchByAccountNumber,
               routeTemplate: "{version}/paymentInstruments/searchByAccountNumber",
               defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.SearchByAccountNumber });

            routes.MapHttpRoute(
               name: Constants.PIMSApiName.ListEmpOrgPI,
               routeTemplate: "{version}/emporg/paymentInstruments",
               defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.ListEmpOrgPI });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.GetSessionDetails,
                routeTemplate: "{version}/{accountId}/sessions/{sessionId}/",
                defaults: new { controller = "PimsPaymentInstruments", action = Constants.PIMSApiName.GetSessionDetails });

            routes.MapHttpRoute(
                name: Constants.PIMSApiName.GetEligiblePaymentMethods,
                routeTemplate: "{version}/thirdPartyPayments/eligiblePaymentMethods",
                defaults: new { controller = "PimsPaymentMethods", action = Constants.PIMSApiName.GetPM });
        }

        internal static void MapMSRewardsRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: Constants.MSRewardsApiName.GetUserInfo,
                routeTemplate: "api/users({userId})",
                defaults: new { controller = "MSRewardsGetUserInfo", action = Constants.MSRewardsApiName.GetUserInfo });

            routes.MapHttpRoute(
                name: Constants.MSRewardsApiName.GetUserInfoUserIdEmpty,
                routeTemplate: "api/users()",
                defaults: new { controller = "MSRewardsGetUserInfo", action = Constants.MSRewardsApiName.GetUserInfoUserIdEmpty });

            routes.MapHttpRoute(
                name: Constants.MSRewardsApiName.RedeemRewards,
                routeTemplate: "api/users({userId})/orders",
                defaults: new { controller = "RedeemRewards", action = Constants.MSRewardsApiName.RedeemRewards });

            routes.MapHttpRoute(
                name: Constants.MSRewardsApiName.RedeemRewardsUserIdEmpty,
                routeTemplate: "api/users()/orders",
                defaults: new { controller = "RedeemRewards", action = Constants.MSRewardsApiName.RedeemRewards });
        }
        
        internal static void MapCatalogRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: Constants.CatalogApiName.GetProducts,
                routeTemplate: "V8.0/products",
                defaults: new { controller = "Catalog", action = Constants.CatalogApiName.GetProducts });
        }

        internal static void MapAccountRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: Constants.AccountApiName.GetProfiles,
                routeTemplate: "{accountId}/profiles",
                defaults: new { controller = "AccountProfiles", action = Constants.AccountApiName.GetProfiles });

            routes.MapHttpRoute(
                name: Constants.AccountApiName.PutProfile,
                routeTemplate: "{accountId}/profiles/{profileId}",
                defaults: new { controller = "AccountProfiles", action = Constants.AccountApiName.PutProfile },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Put) });

            routes.MapHttpRoute(
                name: Constants.AccountApiName.PatchProfile,
                routeTemplate: "{accountId}/profiles/{profileId}",
                defaults: new { controller = "AccountProfiles", action = Constants.AccountApiName.PatchProfile },
                constraints: new { httpMethod = new HttpMethodConstraint(new HttpMethod("PATCH")) });

            routes.MapHttpRoute(
                name: Constants.AccountApiName.GetCustomers,
                routeTemplate: "customers/{accountId}",
                defaults: new { controller = "AccountCustomers", action = Constants.AccountApiName.GetCustomers });

            routes.MapHttpRoute(
                name: Constants.AccountApiName.PostAddress,
                routeTemplate: "{accountId}/addresses",
                defaults: new { controller = "AccountAddresses", action = Constants.AccountApiName.PostAddress },
                constraints: new { httpMethod = new HttpMethodConstraint(new HttpMethod("POST")) });

            routes.MapHttpRoute(
                name: Constants.AccountApiName.GetAddresses,
                routeTemplate: "{accountId}/addresses",
                defaults: new { controller = "AccountAddresses", action = Constants.AccountApiName.GetAddresses },
                constraints: new { httpMethod = new HttpMethodConstraint(new HttpMethod("Get")) });

            routes.MapHttpRoute(
               name: Constants.AccountApiName.GetAddress,
               routeTemplate: "{accountId}/addresses/{addressId}",
               defaults: new { controller = "AccountAddresses" });

            routes.MapHttpRoute(
                name: Constants.AccountApiName.PostAddressValidate,
                routeTemplate: "addresses/validate",
                defaults: new { controller = "AccountAddresses", action = Constants.AccountApiName.PostAddressValidate });

            routes.MapHttpRoute(
                name: Constants.AccountApiName.LegacyValidateAddress,
                routeTemplate: "addresses",
                defaults: new { controller = "AccountAddresses", action = Constants.AccountApiName.LegacyValidateAddress });
        }

        internal static void MapIssuerServiceRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: Constants.IssuerServiceApiName.Initialize,
                routeTemplate: "applications/session",
                defaults: new { controller = "IssuerService", action = Constants.IssuerServiceApiName.Initialize });

            routes.MapHttpRoute(
                name: Constants.IssuerServiceApiName.Apply,
                routeTemplate: "applications/{customerPuid}",
                defaults: new { controller = "IssuerService", action = Constants.IssuerServiceApiName.Apply });

            routes.MapHttpRoute(
                name: Constants.IssuerServiceApiName.Eligibility,
                routeTemplate: "applications/{customerPuid}/eligibility",
                defaults: new { controller = "IssuerService", action = Constants.IssuerServiceApiName.Eligibility });
        }
        
        internal static void MapChallengeManagementRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: Constants.ChallengeManagementApiName.CreateChallenge,
                routeTemplate: "challenge/create",
                defaults: new { controller = "Challenge", action = Constants.ChallengeManagementApiName.CreateChallenge });

            routes.MapHttpRoute(
                name: Constants.ChallengeManagementApiName.GetChallengeStatus,
                routeTemplate: "challenge/status/{sessionId}",
                defaults: new { controller = "Challenge", action = Constants.ChallengeManagementApiName.GetChallengeStatus });

            routes.MapHttpRoute(
                name: Constants.ChallengeManagementApiName.CreateChallengeSession,
                routeTemplate: "challengesession/create",
                defaults: new { controller = "ChallengeManagementSession", action = Constants.ChallengeManagementApiName.CreateChallengeSession });

            routes.MapHttpRoute(
                name: Constants.ChallengeManagementApiName.GetChallengeSessionData,
                routeTemplate: "challengesession/get/{sessionId}",
                defaults: new { controller = "ChallengeManagementSession", action = Constants.ChallengeManagementApiName.GetChallengeSessionData });

            routes.MapHttpRoute(
                name: Constants.ChallengeManagementApiName.UpdateChallengeSession,
                routeTemplate: "challengesession/update",
                defaults: new { controller = "ChallengeManagementSession", action = Constants.ChallengeManagementApiName.UpdateChallengeSession });
        }
        
        internal static void MapPaymentThirdPartyRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: Constants.PaymentThirdPartyApiName.GetCheckout,
                routeTemplate: "payment-providers/{paymentProviderId}/api/checkouts/{checkoutId}",
                defaults: new { controller = "PaymentThirdPartyPaymentAndCheckouts", action = Constants.PaymentThirdPartyApiName.GetCheckout });

            routes.MapHttpRoute(
                name: Constants.PaymentThirdPartyApiName.Charge,
                routeTemplate: "payment-providers/{paymentProviderId}/api/checkouts/{checkoutId}/charge",
                defaults: new { controller = "PaymentThirdPartyCheckoutCharge", action = Constants.PaymentThirdPartyApiName.Charge });

            routes.MapHttpRoute(
                name: Constants.PaymentThirdPartyApiName.GetPaymentRequest,
                routeTemplate: "payment-providers/{paymentProviderId}/api/payment-requests/{paymentRequestId}",
                defaults: new { controller = "PaymentThirdPartyPaymentAndCheckouts", action = Constants.PaymentThirdPartyApiName.GetPaymentRequest });
        }

        internal static void MapPurchaseRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: Constants.PurchaseApiName.ListOrder,
                routeTemplate: "v7.0/users/{userId}/orders",
                defaults: new { controller = "Purchase", action = Constants.PurchaseApiName.ListOrder });

            routes.MapHttpRoute(
                name: Constants.PurchaseApiName.GetOrder,
                routeTemplate: "v7.0/users/{userId}/orders/{orderId}",
                defaults: new { controller = "Purchase", action = Constants.PurchaseApiName.GetOrder });

            routes.MapHttpRoute(
                name: Constants.PurchaseApiName.GetSub,
                routeTemplate: "v8.0/users/{userId}/recurrences/{recurrenceId}",
                defaults: new { controller = "Purchase", action = Constants.PurchaseApiName.GetSub });

            routes.MapHttpRoute(
                name: Constants.PurchaseApiName.ListSub,
                routeTemplate: "v8.0/users/{userId}/recurrences",
                defaults: new { controller = "Purchase", action = Constants.PurchaseApiName.ListSub });

            routes.MapHttpRoute(
                name: Constants.PurchaseApiName.CheckPi,
                routeTemplate: "v7.0/users/{userId}/paymentinstruments/{paymentinstrumentid}/check",
                defaults: new { controller = "Purchase", action = Constants.PurchaseApiName.CheckPi });
        }

        internal static void MapRiskRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: Constants.RiskApiName.RiskEvaluation,
                routeTemplate: "risk/risk-evaluation",
                defaults: new { controller = "Risk", action = Constants.RiskApiName.RiskEvaluation });
        }

        internal static void MapSellerMarketPlaceRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: Constants.SellerMarketPlaceApiName.GetSeller,
                routeTemplate: "v1/payment-providers/{paymentProviderId}/sellers/{sellerId}",
                defaults: new { controller = "Sellers", action = Constants.SellerMarketPlaceApiName.GetSeller });
        }

        internal static void MapTokenPolicyRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: Constants.TokenPolicyApiName.GetTokenPolicyDescription,
                routeTemplate: "{version}/users/{userid}/tokenDescriptionRequests",
                defaults: new { controller = "TokenPolicyDescription", action = Constants.TokenPolicyApiName.GetTokenPolicyDescription });
        }

        internal static void MapStoredValueRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: Constants.StoredValueApiName.GetGiftCatalog,
                routeTemplate: "gift-catalog",
                defaults: new { controller = "StoredValueGiftCatalog", action = Constants.StoredValueApiName.GetGiftCatalog });

            routes.MapHttpRoute(
                name: Constants.StoredValueApiName.PostFunding,
                routeTemplate: "{legacyAccountId}/funds",
                defaults: new { controller = "StoredValueRedeem", action = Constants.StoredValueApiName.PostFunding });

            routes.MapHttpRoute(
               name: Constants.StoredValueApiName.GetFundingStatus,
               routeTemplate: "{legacyAccountId}/funds/{referenceId}",
               defaults: new { controller = "StoredValueRedeem", action = Constants.StoredValueApiName.GetFundingStatus });
        }

        internal static void MapTransactionServiceRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: Constants.TransactionServiceApiName.Payments,
                routeTemplate: "{accountId}/payments",
                defaults: new { controller = "TransactionService", action = Constants.TransactionServiceApiName.Payments });

            routes.MapHttpRoute(
                name: Constants.TransactionServiceApiName.TransactionValidate,
                routeTemplate: "{accountId}/payments/{paymentId}/validate",
                defaults: new { controller = "TransactionService", action = Constants.TransactionServiceApiName.TransactionValidate });
        }

        internal static void MapPaymentOchestratorRoutes(this HttpRouteCollection routes)
        {
            // Checkout requests routes mapping
            routes.MapHttpRoute(
                name: Constants.PaymentOchestratorApiName.AttachAddress,
                routeTemplate: "checkoutRequests/{checkoutId}/attachaddress",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.AttachAddress });

            routes.MapHttpRoute(
                name: Constants.PaymentOchestratorApiName.AttachProfile,
                routeTemplate: "checkoutRequests/{checkoutId}/attachprofile",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.AttachProfile });

            routes.MapHttpRoute(
                name: Constants.PaymentOchestratorApiName.AttachPaymentInstruments,
                routeTemplate: "checkoutRequests/{checkoutId}/attachpaymentinstruments",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.AttachPaymentInstruments });

            routes.MapHttpRoute(
                name: Constants.PaymentOchestratorApiName.Confirm,
                routeTemplate: "checkoutRequests/{checkoutId}/confirm",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.Confirm });

            routes.MapHttpRoute(
                name: Constants.PaymentOchestratorApiName.GetClientAction,
                routeTemplate: "checkoutRequests/{checkoutId}/clientaction",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.GetClientAction });

            // Payment requests routes mapping
            routes.MapHttpRoute(
                name: Constants.PaymentOchestratorApiName.GetClientActions,
                routeTemplate: "paymentRequests/{paymentRequestId}/clientactions",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.GetClientActions });

            routes.MapHttpRoute(
                name: Constants.PaymentOchestratorApiName.PRAttachAddress,
                routeTemplate: "paymentRequests/{paymentRequestId}/attachaddress",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.PRAttachAddress });

            routes.MapHttpRoute(
                name: Constants.PaymentOchestratorApiName.PRAttachProfile,
                routeTemplate: "paymentRequests/{paymentRequestId}/attachprofile",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.PRAttachProfile });

            routes.MapHttpRoute(
                name: Constants.PaymentOchestratorApiName.PRAttachPaymentInstruments,
                routeTemplate: "paymentRequests/{paymentRequestId}/attachpaymentinstruments",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.PRAttachPaymentInstruments });

            routes.MapHttpRoute(
                name: Constants.PaymentOchestratorApiName.PRConfirm,
                routeTemplate: "paymentRequests/{paymentRequestId}/confirm",
                defaults: new { controller = "PaymentOchestratorCheckoutRequests", action = Constants.PaymentOchestratorApiName.PRConfirm });
        }

        internal static void MapPayerAuthRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
               name: Constants.PayerAuthApiName.CreatePaymentSessionId,
               routeTemplate: "CreatePaymentSessionId",
               defaults: new { controller = "PayerAuth", action = Constants.PayerAuthApiName.CreatePaymentSessionId });

            routes.MapHttpRoute(
               name: Constants.PayerAuthApiName.Get3DSMethodUrl,
               routeTemplate: "getThreeDSMethodURL",
               defaults: new { controller = "PayerAuth", action = Constants.PayerAuthApiName.Get3DSMethodUrl });

            routes.MapHttpRoute(
               name: Constants.PayerAuthApiName.Authenticate,
               routeTemplate: "authenticate",
               defaults: new { controller = "PayerAuth", action = Constants.PayerAuthApiName.Authenticate });

            routes.MapHttpRoute(
               name: Constants.PayerAuthApiName.Result,
               routeTemplate: "result",
               defaults: new { controller = "PayerAuth", action = Constants.PayerAuthApiName.Result });

            routes.MapHttpRoute(
               name: Constants.PayerAuthApiName.CompleteChallenge,
               routeTemplate: "CompleteChallenge",
               defaults: new { controller = "PayerAuth", action = Constants.PayerAuthApiName.CompleteChallenge });
        }
        
        internal static void MapFraudDetectionRoutes(this HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
               name: Constants.FraudDetectionApiName.BotCheck,
               routeTemplate: "api/v1/botcheck",
               defaults: new { controller = "FraudDetection", action = Constants.FraudDetectionApiName.BotCheck });
        }
    }
}