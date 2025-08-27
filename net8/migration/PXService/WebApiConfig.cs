using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Commerce.Payments.Common.Web;
using Microsoft.Commerce.Payments.PXService.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocol.Handlers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using PXServiceSettings = Microsoft.Commerce.Payments.PXService.Settings.PXServiceSettings;

namespace Microsoft.Commerce.Payments.PXService
{
    /// <summary>
    /// Simplified Web API configuration for ASP.NET Core.
    /// </summary>
    public static class WebApiConfig
    {
        public static readonly Type PXSettingsType = typeof(PXServiceSettings);

        /// <summary>
        /// Registers services required for the PX service.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        /// <param name="settings">PX service settings instance.</param>
        public static void Register(WebApplicationBuilder builder, PXServiceSettings settings)
        {
            builder.Services.AddSingleton(settings);
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add(new PXServiceExceptionFilter());
                //if (settings.AuthorizationFilter != null)
                //{
                //    options.Filters.Add(settings.AuthorizationFilter);
                //}
            })
            .AddApplicationPart(typeof(WebApiConfig).Assembly)
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            builder.Services.AddSingleton<VersionedControllerSelector>(sp =>
            {
                var selectorLogger = sp.GetRequiredService<ILogger<VersionedControllerSelector>>();
                var selector = new VersionedControllerSelector(selectorLogger);
                InitVersionSelector(selector);
                return selector;
            });

            builder.Services.AddSingleton<PXServiceApiVersionHandler>(); // state used by CORS middleware

            string[] versionlessControllers = { GlobalConstants.ControllerNames.ProbeController };
            builder.Services.AddSingleton(versionlessControllers);
            builder.Services.AddSingleton<IDictionary<string, ApiVersion>>(sp =>
            {
                var selectorInstance = sp.GetRequiredService<VersionedControllerSelector>();
                return selectorInstance.SupportedVersions;
            });

            //if (settings.ValidateCors)
            //{
            //    builder.Services.AddSingleton<PXServiceCorsHandler>(new PXServiceCorsHandler(settings));
            //}

            //builder.Services.AddSingleton<PXServiceHandler>();     // your migrated state (used by PXServiceHandler middleware)
            //builder.Services.AddSingleton<PXServiceFlightHandler>(); // state used by flighting middleware
            //builder.Services.AddSingleton<PXServicePIDLValidationHandler>(); // state used by pidl validation middleware
        }

        /// <summary>
        /// Adds conventional routes that include the API version in the URL.
        /// </summary>
        /// <param name="routes">Endpoint route builder for the application.</param>
        public static void AddUrlVersionedRoutes(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.Probe,
                pattern: GlobalConstants.EndPointNames.V7ProbeVersioned,
                defaults: new { controller = C(GlobalConstants.ControllerNames.ProbeController), action = "Get" });

            endpoints.MapControllerRoute(
               name: GlobalConstants.V7RouteNames.Probe + "NoVersion",
               pattern: GlobalConstants.EndPointNames.V7Probe,
               defaults: new { controller = C(GlobalConstants.ControllerNames.ProbeController), action = "Get" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetPaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7GetPaymentInstrumentEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "GetModernPI" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ListPaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7ListPaymentInstrumentEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController) });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetChallengeContextPaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7GetChallengeContextPaymentInstrumentEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "GetChallengeContext" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ReplacePaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7ReplacePaymentInstrumentEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "ReplaceModernPI" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.RedeemPaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7RedeemPaymentInstrumentEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "RedeemModernPI" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.RemovePaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7RemovePaymentInstrumentEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "RemoveModernPI" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.UpdatePaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7UpdatePaymentInstrumentEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "UpdateModernPI" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ResumePendingOperationEx,
                pattern: GlobalConstants.EndPointNames.V7ResumePendingOperationEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "ResumePendingOperation" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousResumePendingOperationEx,
                pattern: GlobalConstants.EndPointNames.V7AnonymousResumePendingOperationEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "AnonymousResumePendingOperation" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetCardProfileEx,
                pattern: GlobalConstants.EndPointNames.V7GetCardProfileEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "GetCardProfile" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetSeCardPersos,
                pattern: GlobalConstants.EndPointNames.V7GetSeCardPersosEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "GetSeCardPersos" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PostPaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7PostReplenishTransactionCredentialsEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "PostReplenishTransactionCredentials" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AcquireLUKsEx,
                pattern: GlobalConstants.EndPointNames.V7AcquireLuksEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "AcquireLUKs" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ConfirmLUKsEx,
                pattern: GlobalConstants.EndPointNames.V7ConfirmLuksEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "ConfirmLUKs" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ValidateCvvEx,
                pattern: GlobalConstants.EndPointNames.V7ValidateCvvEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "ValidateCvv" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetSettings,
                pattern: GlobalConstants.EndPointNames.V7GetSettings,
                defaults: new
                {
                    controller = C(GlobalConstants.ControllerNames.SettingsController.Replace("Controller", string.Empty)),
                    action = "GetSettings"
                });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetSettingsInPost,
                pattern: GlobalConstants.EndPointNames.V7GetSettingsInPost,
                defaults: new
                {
                    controller = C(GlobalConstants.ControllerNames.SettingsController.Replace("Controller", string.Empty)),
                    action = "GetSettingsInPost"
                });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentSessions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionsController), action = "PostPaymentSession" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionGetApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentSessionsGet,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionsController), action = "GetPaymentSession" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.QRCodePaymentSessionGetApi,
                pattern: GlobalConstants.EndPointNames.V7QrCodePaymentSessionsGet,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionsController), action = "GetQrCodePaymentSession" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionCreateAndAuthenticateApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentSessionsCreateAndAuthenticate,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionsController), action = "CreateAndAuthenticatePaymentSession" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticateApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticate,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionsController), action = "AuthenticatePaymentSession" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionNotifyThreeDSChallengeCompletedApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentSessionsNotifyThreeDSChallengeCompleted,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionsController), action = "NotifyThreeDSChallengeCompleted" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.BrowserFlowPaymentSessionsAuthenticateApi,
                pattern: GlobalConstants.EndPointNames.V7BrowserFlowAuthenticate,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionsController), action = "BrowserFlowAuthenticate" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.BrowserFlowPaymentSessionsNotifyThreeDSChallengeCompletedApi,
                pattern: GlobalConstants.EndPointNames.V7BrowserFlowPaymentSessionsNotifyChallengeCompleted,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionsController), action = "BrowserFlowNotifyChallengeCompleted" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionBrowserAuthenticateThreeDSOneApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentSessionsBrowserAuthenticateThreeDSOne,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionsController), action = "BrowserAuthenticateThreeDSOne" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionBrowserAuthenticateRedirectionThreeDSOneApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentSessionsBrowserAuthenticateRedirectionThreeDSOne,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionsController), action = "BrowserAuthenticateRedirectionThreeDSOne" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionBrowserNotifyThreeDSOneChallengeCompletedOneApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentSessionsBrowserNotifyThreeDSOneChallengeCompleted,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionsController), action = "BrowserNotifyThreeDSOneChallengeCompleted" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticateIndiaThreeDSApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticateIndiaThreeDS,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionsController), action = "AuthenticateIndiaThreeDS" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentTransactionApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentTransactions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentTransactionsController) });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.MSRewardsApi,
                pattern: GlobalConstants.EndPointNames.V7MSRewards,
                defaults: new { controller = C(GlobalConstants.ControllerNames.MSRewardsController) });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticationStatusApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticationStatus,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionsController), action = "PaymentSessionAuthenticationStatus" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AttachAddressCheckoutRequestExApi,
                pattern: GlobalConstants.EndPointNames.V7AttachAddressCheckoutRequestsEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.CheckoutRequestsExController), action = "AttachAddress" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AttachProfileCheckoutRequestExApi,
                pattern: GlobalConstants.EndPointNames.V7AttachProfileCheckoutRequestsEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.CheckoutRequestsExController), action = "AttachProfile" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ConfirmCheckoutRequestExApi,
                pattern: GlobalConstants.EndPointNames.V7ConfirmCheckoutRequestsEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.CheckoutRequestsExController), action = "Confirm" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ExpressCheckoutConfirmApi,
                pattern: GlobalConstants.EndPointNames.V7ExpressCheckoutConfirm,
                defaults: new { controller = C(GlobalConstants.ControllerNames.ExpressCheckoutController), action = "Confirm" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.TokensExApi,
                pattern: GlobalConstants.EndPointNames.V7TokensEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.TokensExController), action = "CreateToken" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.TokensExChallengeApi,
                pattern: GlobalConstants.EndPointNames.V7TokensExChallenge,
                defaults: new { controller = C(GlobalConstants.ControllerNames.TokensExController), action = "Challenge" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.TokensExValidateChallengeApi,
                pattern: GlobalConstants.EndPointNames.V7TokensExValidateChallenge,
                defaults: new { controller = C(GlobalConstants.ControllerNames.TokensExController), action = "ValidateChallenge" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.TokensExMandatesApi,
                pattern: GlobalConstants.EndPointNames.V7TokensExMandate,
                defaults: new { controller = C(GlobalConstants.ControllerNames.TokensExController), action = "Mandate" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetAgenticTokenDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7AgenticTokenDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.AgenticTokenDescriptionsController) });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AttachPaymentInstrumentExApi,
                pattern: GlobalConstants.EndPointNames.V7AttachPaymentInstrumentCheckoutRequestsEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.CheckoutRequestsExController), action = "AttachPaymentInstrument" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetPaymentMethodDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentMethodDescriptions + "{id}",
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentMethodDescriptionsController), action = "GetByFamilyAndType" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetPaymentMethodDescriptionsApiNoId,
                pattern: GlobalConstants.EndPointNames.V7PaymentMethodDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentMethodDescriptionsController), action = "List" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetPaymentMethodDescriptionsApiNoId,
                pattern: GlobalConstants.EndPointNames.V7PaymentMethodDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentMethodDescriptionsController), action = "SelectPaymentResource" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetAddressDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7AddressDescriptions + "{id}",
                defaults: new { controller = C(GlobalConstants.ControllerNames.AddressDescriptionsController), action = "GetById" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetAddressDescriptionsApiNoId,
                pattern: GlobalConstants.EndPointNames.V7AddressDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.AddressDescriptionsController), action = "GetAddressGroupsById" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AddressesExApi,
                pattern: GlobalConstants.EndPointNames.V7AddressesEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.AddressesExController), action = "Create" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AddressesExApiWithId,
                pattern: GlobalConstants.EndPointNames.V7AddressesExWithId,
                defaults: new { controller = C(GlobalConstants.ControllerNames.AddressesExController), action = "Get" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetChallengeDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7ChallengeDescriptions + "{id}",
                defaults: new { controller = C(GlobalConstants.ControllerNames.ChallengeDescriptionsController), action = "GetById" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetChallengeDescriptionsApiNoId,
                pattern: GlobalConstants.EndPointNames.V7ChallengeDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.ChallengeDescriptionsController), action = "GetPaymentChallenge" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetProfileDescriptionsApiNoId,
                pattern: GlobalConstants.EndPointNames.V7ProfileDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.ProfileDescriptionsController), action = "List" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetBillingGroupDescriptionsApiNoId,
                pattern: GlobalConstants.EndPointNames.V7BillingGroupDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.BillingGroupDescriptionsController), action = "List" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetTaxIdDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7TaxIdDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.TaxIdDescriptionsController), action = "List" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetTenantDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7TenantDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.TenantDescriptionsController), action = "Get" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetPaymentSessionDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7GetPaymentSessionDescription,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentSessionDescriptionsController), action = "Get" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PostCardsApi,
                pattern: GlobalConstants.EndPointNames.V7Cards,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "PostCard" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousLegacyAddressValidationApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousLegacyAddressValidation,
                defaults: new { controller = C(GlobalConstants.ControllerNames.AddressesController), action = "LegacyAddressValidation" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousModernAddressValidationApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousModernAddressValidation,
                defaults: new { controller = C(GlobalConstants.ControllerNames.AddressesController), action = "ModernAddressValidation" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousRDSSessionQueryApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousRDSSessionQuery,
                defaults: new { controller = C(GlobalConstants.ControllerNames.RDSSessionController), action = "Get" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.TransformationApi,
                pattern: GlobalConstants.EndPointNames.V7Transformation,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PidlTransformationController), action = "Post" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ValidationApi,
                pattern: GlobalConstants.EndPointNames.V7Validation,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PidlValidationController), action = "Post" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousGetAddressDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousAddressDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.AddressDescriptionsController), action = "AnonymousList" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousGetTaxIdDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousTaxIdDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.TaxIdDescriptionsController), action = "AnonymousList" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousGetPaymentMethodDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousPaymentMethodDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentMethodDescriptionsController), action = "GetAnonymousPidl" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousGetPaymentMethodDescriptionsSessionIdApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousPaymentMethodDescriptionsSessionId,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentMethodDescriptionsController), action = "AnonymousListBySession" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.SessionsByIdApi,
                pattern: GlobalConstants.EndPointNames.V7SessionsById,
                defaults: new { controller = C(GlobalConstants.ControllerNames.SessionsController), action = "GetBySessionId" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.SessionsApi,
                pattern: GlobalConstants.EndPointNames.V7Sessions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.SessionsController), action = "PostSession" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousGetCheckoutDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousCheckoutDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.CheckoutDescriptionsController), action = "AnonymousList" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousCheckoutsExApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousCheckoutsExCharge,
                defaults: new { controller = C(GlobalConstants.ControllerNames.CheckoutsExController), action = "AnonymousCharge" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousCheckoutsExStatusApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousCheckoutsExStatus,
                defaults: new { controller = C(GlobalConstants.ControllerNames.CheckoutsExController), action = "AnonymousStatus" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AnonymousCheckoutsExCompletedApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousCheckoutsExCompleted,
                defaults: new { controller = C(GlobalConstants.ControllerNames.CheckoutsExController), action = "AnonymousCompleted" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ApplyPaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7ApplyPaymentInstrumentEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "Apply" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.CreatePaymentInstrumentEx,
                pattern: GlobalConstants.EndPointNames.V7CreatePaymentInstrumentEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentInstrumentsExController), action = "CreateModernPI" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.WalletsGetConfigApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousGetWalletConfig,
                defaults: new { controller = C(GlobalConstants.ControllerNames.WalletsController), action = "GetWalletConfig" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.WalletsSetupProviderSessionApi,
                pattern: GlobalConstants.EndPointNames.V7AnonymousWalletSetupProviderSession,
                defaults: new { controller = C(GlobalConstants.ControllerNames.WalletsController), action = "SetupProviderSession" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.WalletsProvisionWalletTokenApi,
                pattern: GlobalConstants.EndPointNames.V7ProvisionWalletToken,
                defaults: new { controller = C(GlobalConstants.ControllerNames.WalletsController), action = "ProvisionWalletToken" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetRewardsDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7RewardsDescriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.RewardsDescriptionsController), action = "List" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.PaymentClientInitializationApi,
                pattern: GlobalConstants.EndPointNames.V7PaymentClientInitialization,
                defaults: new { controller = C(GlobalConstants.ControllerNames.InitializationController), action = "Post" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.GetDescriptionsApi,
                pattern: GlobalConstants.EndPointNames.V7Descriptions,
                defaults: new { controller = C(GlobalConstants.ControllerNames.DescriptionsController), action = "List" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.ConfirmPaymentRequestExApi,
                pattern: GlobalConstants.EndPointNames.V7ConfirmPaymentRequestsEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentRequestsExController), action = "Confirm" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.AttachChallengeDataPaymentRequestExApi,
                pattern: GlobalConstants.EndPointNames.V7AttachChallengeDataPaymentRequestsEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentRequestsExController), action = "AttachChallengeData" });

            endpoints.MapControllerRoute(
                name: GlobalConstants.V7RouteNames.RemoveEligiblePaymentmethodsPaymentRequestExApi,
                pattern: GlobalConstants.EndPointNames.V7RemoveEligiblePaymentmethodsPaymentRequestsEx,
                defaults: new { controller = C(GlobalConstants.ControllerNames.PaymentRequestsExController), action = "RemoveEligiblePaymentMethods" });

            // Also expose any attribute-routed controllers
            endpoints.MapControllers();
        }

        private static void InitVersionSelector(VersionedControllerSelector selector)
        {
            AddV7Controllers(selector);
        }

        private static string Key(string name) =>
               name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                   ? name[..^"Controller".Length]
                   : name;

        private static void AddV7Controllers(VersionedControllerSelector selector)
        {
            // Versionless (probe)
            selector.AddVersionless(
                Key(GlobalConstants.ControllerNames.ProbeController),
                typeof(ProbeController));

            selector.AddVersionless(
                Key(GlobalConstants.ControllerNames.HealthController),
                typeof(HealthController));

            // --- V7 controllers (ported 1:1 from your WebApiConfig.AddV7Controllers) ---
            var v7 = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { Key(GlobalConstants.ControllerNames.PaymentInstrumentsExController),   typeof(V7.PaymentInstrumentsExController) },
                { Key(GlobalConstants.ControllerNames.SettingsController),                typeof(V7.SettingsController) },
                { Key(GlobalConstants.ControllerNames.PaymentMethodDescriptionsController), typeof(V7.PaymentMethodDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.AddressDescriptionsController),     typeof(V7.AddressDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.ChallengeDescriptionsController),   typeof(V7.ChallengeDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.ProfileDescriptionsController),     typeof(V7.ProfileDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.BillingGroupDescriptionsController),typeof(V7.BillingGroupDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.TenantDescriptionsController),      typeof(V7.TenantDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.TaxIdDescriptionsController),       typeof(V7.TaxIdDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.PidlTransformationController),      typeof(V7.PidlTransformationController) },
                { Key(GlobalConstants.ControllerNames.PidlValidationController),          typeof(V7.PidlValidationController) },
                { Key(GlobalConstants.ControllerNames.SessionsController),                typeof(V7.SessionsController) },
                { Key(GlobalConstants.ControllerNames.AddressesController),               typeof(V7.AddressesController) },
                { Key(GlobalConstants.ControllerNames.AddressesExController),             typeof(V7.AddressesExController) },
                { Key(GlobalConstants.ControllerNames.PaymentSessionDescriptionsController), typeof(V7.PaymentSessionDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.PaymentSessionsController),         typeof(V7.PaymentChallenge.PaymentSessionsController) },
                { Key(GlobalConstants.ControllerNames.PaymentTransactionsController),     typeof(V7.PaymentTransaction.PaymentTransactionsController) },
                { Key(GlobalConstants.ControllerNames.RDSSessionController),              typeof(V7.RDSSessionController) },
                { Key(GlobalConstants.ControllerNames.CheckoutDescriptionsController),    typeof(V7.CheckoutDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.CheckoutsExController),             typeof(V7.Checkouts.CheckoutsExController) },
                { Key(GlobalConstants.ControllerNames.WalletsController),                 typeof(V7.WalletsController) },
                { Key(GlobalConstants.ControllerNames.RewardsDescriptionsController),     typeof(V7.RewardsDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.MSRewardsController),               typeof(V7.MSRewardsController) },
                { Key(GlobalConstants.ControllerNames.InitializationController),          typeof(V7.InitializationController) },
                { Key(GlobalConstants.ControllerNames.DescriptionsController),            typeof(V7.DescriptionsController) },
                { Key(GlobalConstants.ControllerNames.CheckoutRequestsExController),      typeof(V7.PaymentClient.CheckoutRequestsExController) },
                { Key(GlobalConstants.ControllerNames.ExpressCheckoutController),         typeof(V7.ExpressCheckoutController) },
                { Key(GlobalConstants.ControllerNames.PaymentRequestsExController),       typeof(V7.PaymentClient.PaymentRequestsExController) },
                { Key(GlobalConstants.ControllerNames.AgenticTokenDescriptionsController),typeof(V7.AgenticTokenDesctipionsController) },
                { Key(GlobalConstants.ControllerNames.TokensExController),                typeof(V7.TokensExController) },
            };

            selector.AddVersion("v7.0", v7);
        }

        static string C(string name) =>
            name.EndsWith("Controller", StringComparison.Ordinal) ? name[..^"Controller".Length] : name;

    }
}