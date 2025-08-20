using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Commerce.Payments.Common.Web;
using Microsoft.Commerce.Payments.PXService.Settings;
using Newtonsoft.Json;

namespace Microsoft.Commerce.Payments.PXService
{
    /// <summary>
    /// Simplified Web API configuration for ASP.NET Core.
    /// </summary>
    public static class WebApiConfig
    {
        public static readonly Type PXSettingsType = typeof(PXServiceSettings);

        private static VersionedControllerSelector selector;

        /// <summary>
        /// Registers services required for the PX service.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        /// <param name="settings">PX service settings instance.</param>
        public static void Register(WebApplicationBuilder builder, PXServiceSettings settings)
        {
            builder.Services.AddSingleton(settings);
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            InitVersionSelector();
            builder.Services.AddSingleton(selector);
        }

        /// <summary>
        /// Adds conventional routes that include the API version in the URL.
        /// </summary>
        /// <param name="routes">Endpoint route builder for the application.</param>
        public static void AddUrlVersionedRoutes(IEndpointRouteBuilder routes)
        {
            // V7 Routes
            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.Probe,
                pattern: GlobalConstants.EndPointNames.V7Probe,
                defaults: new { controller = GlobalConstants.ControllerNames.ProbeController, action = "Get" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetPaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7GetPaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "GetModernPI" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ListPaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7ListPaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetChallengeContextPaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7GetChallengeContextPaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "GetChallengeContext" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ReplacePaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7ReplacePaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "ReplaceModernPI" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.RedeemPaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7RedeemPaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "RedeemModernPI" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.RemovePaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7RemovePaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "RemoveModernPI" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.UpdatePaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7UpdatePaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "UpdateModernPI" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ResumePendingOperationEx,
                pattern: GlobalConstants.EndPointNames.V7ResumePendingOperationEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "ResumePendingOperation" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousResumePendingOperationEx,
                pattern: GlobalConstants.EndPointNames.V7AnonymousResumePendingOperationEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "AnonymousResumePendingOperation" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetCardProfileEx,
                pattern: GlobalConstants.EndPointNames.V7GetCardProfileEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "GetCardProfile" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetSeCardPersos,
                pattern: GlobalConstants.EndPointNames.V7GetSeCardPersosEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "GetSeCardPersos" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PostPaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7PostReplenishTransactionCredentialsEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "PostReplenishTransactionCredentials" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AcquireLUKsEx,
                pattern: GlobalConstants.EndPointNames.V7AcquireLuksEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "AcquireLUKs" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ConfirmLUKsEx,
                pattern: GlobalConstants.EndPointNames.V7ConfirmLuksEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "ConfirmLUKs" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ValidateCvvEx,
                pattern: GlobalConstants.EndPointNames.V7ValidateCvvEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "ValidateCvv" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetSettings,
                pattern: GlobalConstants.EndPointNames.V7GetSettings,
                defaults: new { controller = GlobalConstants.ControllerNames.SettingsController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetSettingsInPost,
                pattern: GlobalConstants.EndPointNames.V7GetSettingsInPost,
                defaults: new { controller = GlobalConstants.ControllerNames.SettingsController, action = "GetSettingsInPost" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentSessions,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "PostPaymentSession" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionGetApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentSessionsGet,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "GetPaymentSession" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.QRCodePaymentSessionGetApi,
                pattern: GlobalConstants.EndPointNames.V7QrCodePaymentSessionsGet,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "qrCodeStatus" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionCreateAndAuthenticateApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentSessionsCreateAndAuthenticate,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "CreateAndAuthenticate" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticateApi,
               pattern: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticate,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "Authenticate" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.PaymentSessionNotifyThreeDSChallengeCompletedApi,
               pattern: GlobalConstants.EndPointNames.V7PaymentSessionsNotifyThreeDSChallengeCompleted,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "NotifyThreeDSChallengeCompleted" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.BrowserFlowPaymentSessionsAuthenticateApi,
               pattern: GlobalConstants.EndPointNames.V7BrowserFlowAuthenticate,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserAuthenticate" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.BrowserFlowPaymentSessionsNotifyThreeDSChallengeCompletedApi,
                pattern: GlobalConstants.EndPointNames.V7BrowserFlowPaymentSessionsNotifyChallengeCompleted,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserNotifyThreeDSChallengeCompleted" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.PaymentSessionBrowserAuthenticateThreeDSOneApi,
               pattern: GlobalConstants.EndPointNames.V7PaymentSessionsBrowserAuthenticateThreeDSOne,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserAuthenticateThreeDSOne" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.PaymentSessionBrowserAuthenticateRedirectionThreeDSOneApi,
               pattern: GlobalConstants.EndPointNames.V7PaymentSessionsBrowserAuthenticateRedirectionThreeDSOne,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserAuthenticateRedirectionThreeDSOne" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.PaymentSessionBrowserNotifyThreeDSOneChallengeCompletedOneApi,
               pattern: GlobalConstants.EndPointNames.V7PaymentSessionsBrowserNotifyThreeDSOneChallengeCompleted,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserNotifyThreeDSOneChallengeCompleted" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticateIndiaThreeDSApi,
               pattern: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticateIndiaThreeDS,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "AuthenticateIndiaThreeDS" });

            routes.MapControllerRoute(
                      name: GlobalConstants.V7RouteNames.PaymentTransactionApi,
                      pattern: GlobalConstants.EndPointNames.V7PaymentTransactions,
                      defaults: new { controller = GlobalConstants.ControllerNames.PaymentTransactionsController });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.MSRewardsApi,
               pattern: GlobalConstants.EndPointNames.V7MSRewards,
               defaults: new { controller = GlobalConstants.ControllerNames.MSRewardsController });

            routes.MapControllerRoute(
              name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticationStatusApi,
              pattern: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticationStatus,
              defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "AuthenticationStatus" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.AttachAddressCheckoutRequestExApi,
               pattern: GlobalConstants.EndPointNames.V7AttachAddressCheckoutRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.CheckoutRequestsExController, action = "AttachAddress" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.AttachProfileCheckoutRequestExApi,
               pattern: GlobalConstants.EndPointNames.V7AttachProfileCheckoutRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.CheckoutRequestsExController, action = "AttachProfile" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.ConfirmCheckoutRequestExApi,
               pattern: GlobalConstants.EndPointNames.V7ConfirmCheckoutRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.CheckoutRequestsExController, action = "Confirm" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.ExpressCheckoutConfirmApi,
               pattern: GlobalConstants.EndPointNames.V7ExpressCheckoutConfirm,
               defaults: new { controller = GlobalConstants.ControllerNames.ExpressCheckoutController, action = "Confirm" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.TokensExApi,
               pattern: GlobalConstants.EndPointNames.V7TokensEx,
               defaults: new { controller = GlobalConstants.ControllerNames.TokensExController, action = "Tokens" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.TokensExChallengeApi,
               pattern: GlobalConstants.EndPointNames.V7TokensExChallenge,
               defaults: new { controller = GlobalConstants.ControllerNames.TokensExController, action = "PostChallenge" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.TokensExValidateChallengeApi,
               pattern: GlobalConstants.EndPointNames.V7TokensExValidateChallenge,
               defaults: new { controller = GlobalConstants.ControllerNames.TokensExController, action = "ValidateChallenge" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.TokensExMandatesApi,
               pattern: GlobalConstants.EndPointNames.V7TokensExMandate,
               defaults: new { controller = GlobalConstants.ControllerNames.TokensExController, action = "Mandates" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.GetAgenticTokenDescriptionsApi,
               pattern: GlobalConstants.EndPointNames.V7AgenticTokenDescriptions,
               defaults: new { controller = GlobalConstants.ControllerNames.AgenticTokenDescriptionsController });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.AttachPaymentInstrumentExApi,
               pattern: GlobalConstants.EndPointNames.V7AttachPaymentInstrumentCheckoutRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.CheckoutRequestsExController, action = "AttachPaymentInstrument" });

            // V7 PIDL Routes
            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetPaymentMethodDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentMethodDescriptions + "{id}",
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentMethodDescriptionsController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetPaymentMethodDescriptionsApiNoId,
                pattern: GlobalConstants.EndPointNames.V7PaymentMethodDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentMethodDescriptionsController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetAddressDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7AddressDescriptions + "{id}",
                defaults: new { controller = GlobalConstants.ControllerNames.AddressDescriptionsController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetAddressDescriptionsApiNoId,
                pattern: GlobalConstants.EndPointNames.V7AddressDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.AddressDescriptionsController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AddressesExApi,
                pattern: GlobalConstants.EndPointNames.V7AddressesEx,
                defaults: new { controller = GlobalConstants.ControllerNames.AddressesExController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AddressesExApiWithId,
                pattern: GlobalConstants.EndPointNames.V7AddressesExWithId,
                defaults: new { controller = GlobalConstants.ControllerNames.AddressesExController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetChallengeDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7ChallengeDescriptions + "{id}",
                defaults: new { controller = GlobalConstants.ControllerNames.ChallengeDescriptionsController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetChallengeDescriptionsApiNoId,
                pattern: GlobalConstants.EndPointNames.V7ChallengeDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.ChallengeDescriptionsController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetProfileDescriptionsApiNoId,
                pattern: GlobalConstants.EndPointNames.V7ProfileDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.ProfileDescriptionsController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetBillingGroupDescriptionsApiNoId,
                pattern: GlobalConstants.EndPointNames.V7BillingGroupDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.BillingGroupDescriptionsController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetTenantDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7TenantDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.TenantDescriptionsController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.TransformationApi,
                pattern: GlobalConstants.EndPointNames.V7Validation,
                defaults: new { controller = GlobalConstants.ControllerNames.PidlTransformationController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ValidationApi,
                pattern: GlobalConstants.EndPointNames.V7Validation,
                defaults: new { controller = GlobalConstants.ControllerNames.PidlValidationController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.SessionsApi,
                pattern: GlobalConstants.EndPointNames.V7Sessions,
                defaults: new { controller = GlobalConstants.ControllerNames.SessionsController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousModernAddressValidationApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousModernAddressValidation,
            defaults: new { controller = GlobalConstants.ControllerNames.AddressesController, action = "ModernAddressValidation" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetPaymentSessionDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7GetPaymentSessionDescription,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionDescriptionsController });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousRDSSessionQueryApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousRDSSessionQuery,
                defaults: new { controller = GlobalConstants.ControllerNames.RDSSessionController, action = "Get" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousGetCheckoutDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousCheckoutDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.CheckoutDescriptionsController, action = "AnonymousList" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousCheckoutsExApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousCheckoutsExCharge,
                defaults: new { controller = GlobalConstants.ControllerNames.CheckoutsExController, action = "AnonymousCharge" });


            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousCheckoutsExStatusApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousCheckoutsExStatus,
                defaults: new { controller = GlobalConstants.ControllerNames.CheckoutsExController, action = "AnonymousStatus" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousCheckoutsExCompletedApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousCheckoutsExCompleted,
                defaults: new { controller = GlobalConstants.ControllerNames.CheckoutsExController, action = "AnonymousCompleted" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.WalletsGetConfigApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousGetWalletConfig,
                defaults: new { controller = GlobalConstants.ControllerNames.WalletsController, action = "GetConfig" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.WalletsSetupProviderSessionApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousWalletSetupProviderSession,
                defaults: new { controller = GlobalConstants.ControllerNames.WalletsController, action = "SetupProviderSession" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.WalletsProvisionWalletTokenApi,
                pattern: GlobalConstants.EndPointNames.V7ProvisionWalletToken,
                defaults: new { controller = GlobalConstants.ControllerNames.WalletsController, action = "ProvisionWalletToken" });

            routes.MapControllerRoute(
                 name: GlobalConstants.V7RouteNames.GetRewardsDescriptionsApi,
                 pattern: GlobalConstants.EndPointNames.V7RewardsDescriptions,
                 defaults: new { controller = GlobalConstants.ControllerNames.RewardsDescriptionsController, action = "List" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentClientInitializationApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentClientInitialization,
                defaults: new { controller = GlobalConstants.ControllerNames.InitializationController, action = "Post" });

            routes.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7Descriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.DescriptionsController, action = "List" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.ConfirmPaymentRequestExApi,
               pattern: GlobalConstants.EndPointNames.V7ConfirmPaymentRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentRequestsExController, action = "Confirm" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.AttachChallengeDataPaymentRequestExApi,
               pattern: GlobalConstants.EndPointNames.V7AttachChallengeDataPaymentRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentRequestsExController, action = "AttachChallengeData" });

            routes.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.RemoveEligiblePaymentmethodsPaymentRequestExApi,
               pattern: GlobalConstants.EndPointNames.V7RemoveEligiblePaymentmethodsPaymentRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentRequestsExController, action = "RemoveEligiblePaymentmethods" });
        }

        private static void InitVersionSelector()
        {
            selector = new VersionedControllerSelector(NullLogger<VersionedControllerSelector>.Instance);
            AddV7Controllers();
        }

        private static void AddV7Controllers()
        {
            selector.AddVersionless(GlobalConstants.ControllerNames.ProbeController, typeof(ProbeController));

            var v7Controllers = new Dictionary<string, Type>
            {
                { GlobalConstants.ControllerNames.PaymentInstrumentsExController, typeof(V7.PaymentInstrumentsExController) },
                { GlobalConstants.ControllerNames.SettingsController, typeof(V7.SettingsController) },
                { GlobalConstants.ControllerNames.PaymentMethodDescriptionsController, typeof(V7.PaymentMethodDescriptionsController) },
                { GlobalConstants.ControllerNames.AddressDescriptionsController, typeof(V7.AddressDescriptionsController) },
                { GlobalConstants.ControllerNames.ChallengeDescriptionsController, typeof(V7.ChallengeDescriptionsController) },
                { GlobalConstants.ControllerNames.ProfileDescriptionsController, typeof(V7.ProfileDescriptionsController) },
                { GlobalConstants.ControllerNames.BillingGroupDescriptionsController, typeof(V7.BillingGroupDescriptionsController) },
                { GlobalConstants.ControllerNames.TenantDescriptionsController, typeof(V7.TenantDescriptionsController) },
                { GlobalConstants.ControllerNames.TaxIdDescriptionsController, typeof(V7.TaxIdDescriptionsController) },
                { GlobalConstants.ControllerNames.PidlTransformationController, typeof(V7.PidlTransformationController) },
                { GlobalConstants.ControllerNames.PidlValidationController, typeof(V7.PidlValidationController) },
                { GlobalConstants.ControllerNames.SessionsController, typeof(V7.SessionsController) },
                { GlobalConstants.ControllerNames.AddressesController, typeof(V7.AddressesController) },
                { GlobalConstants.ControllerNames.AddressesExController, typeof(V7.AddressesExController) },
                { GlobalConstants.ControllerNames.PaymentSessionDescriptionsController, typeof(V7.PaymentSessionDescriptionsController) },
                { GlobalConstants.ControllerNames.PaymentSessionsController, typeof(V7.PaymentChallenge.PaymentSessionsController) },
                { GlobalConstants.ControllerNames.PaymentTransactionsController, typeof(V7.PaymentTransaction.PaymentTransactionsController) },
                { GlobalConstants.ControllerNames.RDSSessionController, typeof(V7.RDSSessionController) },
                { GlobalConstants.ControllerNames.CheckoutDescriptionsController, typeof(V7.CheckoutDescriptionsController) },
                { GlobalConstants.ControllerNames.CheckoutsExController, typeof(V7.Checkouts.CheckoutsExController) },
                { GlobalConstants.ControllerNames.WalletsController, typeof(V7.WalletsController) },
                { GlobalConstants.ControllerNames.RewardsDescriptionsController, typeof(V7.RewardsDescriptionsController) },
                { GlobalConstants.ControllerNames.MSRewardsController, typeof(V7.MSRewardsController) },
                { GlobalConstants.ControllerNames.InitializationController, typeof(V7.InitializationController) },
                { GlobalConstants.ControllerNames.DescriptionsController, typeof(V7.DescriptionsController) },
                { GlobalConstants.ControllerNames.CheckoutRequestsExController, typeof(V7.PaymentClient.CheckoutRequestsExController) },
                { GlobalConstants.ControllerNames.ExpressCheckoutController, typeof(V7.ExpressCheckoutController) },
                { GlobalConstants.ControllerNames.PaymentRequestsExController, typeof(V7.PaymentClient.PaymentRequestsExController) },
                { GlobalConstants.ControllerNames.AgenticTokenDescriptionsController, typeof(V7.AgenticTokenDesctipionsController) },
                { GlobalConstants.ControllerNames.TokensExController, typeof(V7.TokensExController) }
            };

            var v7Version = V7.Constants.Versions.ApiVersion;
            selector.AddVersion(v7Version, v7Controllers);
            if (v7Version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                selector.AddVersion(v7Version[1..], v7Controllers);
            }
        }
    }
}