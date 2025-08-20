// <copyright file="WebApiConfig.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Web.Http;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Settings;
    using Microsoft.CommonSchema.Services.Listeners;
    using Microsoft.Diagnostics.Tracing;
    using Newtonsoft.Json;

    /// <summary>
    /// Web Api Configuration
    /// </summary>
    public static class WebApiConfig
    {
        public static readonly Type PXSettingsType = typeof(PXServiceSettings);

        private static VersionedControllerSelector selector;

        public static void Register(HttpConfiguration config, PXServiceSettings settings)
        {
            // Ignore all null fields in response as they are meanlingless for json
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            AddUrlVersionedRoutes(config);
            
            // Add message handlers
            if (!WebHostingUtility.IsApplicationSelfHosted())
            {
                ////Preventing this reduces in-memory difftest run from 00:14:02 to 00:03:38
                config.MessageHandlers.Add(new PXTraceCorrelationHandler(
                    serviceName: Constants.ServiceNames.PXService,
                    logIncomingRequestToAppInsight: ApplicationInsightsProvider.LogIncomingOperation));
            }
            
            InitVersionSelector();
            selector.Initialize(config, false);
            config.MessageHandlers.Add(new PXServiceApiVersionHandler(selector.SupportedVersions, new string[] { GlobalConstants.ControllerNames.ProbeController }, settings));

            if (settings.ValidateCors)
            {
                config.MessageHandlers.Add(new PXServiceCorsHandler(settings));
            }

            config.MessageHandlers.Add(new PXServiceInputValidationHandler());

            if (settings.PIDLDocumentValidationEnabled)
            {
                config.MessageHandlers.Add(new PXServicePIDLValidationHandler());
            }

            config.Properties[PXSettingsType] = settings;
            config.Filters.Add(new PXServiceExceptionFilter());

            if (settings.AuthorizationFilter != null)
            {
                config.Filters.Add(settings.AuthorizationFilter);
            }

            EnsureSllInitialized();

            ServicePointManager.CheckCertificateRevocationList = true;

            ApplicationInsightsProvider.SetupAppInsightsConfiguration(settings.ApplicationInsightInstrumentKey, settings.ApplicationInsightMode);
        }

        /// <summary>
        /// Add routes for the controllers in which Version is provided in the Uri
        /// </summary>
        /// <param name="config">The http config to register into</param>
        private static void AddUrlVersionedRoutes(HttpConfiguration config)
        {
            // V7 Routes
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.Probe,
                routeTemplate: GlobalConstants.EndPointNames.V7Probe,
                defaults: new { controller = GlobalConstants.ControllerNames.ProbeController, action = "Get" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetPaymentInstrumentEx,
                routeTemplate: GlobalConstants.EndPointNames.V7GetPaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "GetModernPI" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.ListPaymentInstrumentEx,
                routeTemplate: GlobalConstants.EndPointNames.V7ListPaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetChallengeContextPaymentInstrumentEx,
                routeTemplate: GlobalConstants.EndPointNames.V7GetChallengeContextPaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "GetChallengeContext" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.ReplacePaymentInstrumentEx,
                routeTemplate: GlobalConstants.EndPointNames.V7ReplacePaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "ReplaceModernPI" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.RedeemPaymentInstrumentEx,
                routeTemplate: GlobalConstants.EndPointNames.V7RedeemPaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "RedeemModernPI" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.RemovePaymentInstrumentEx,
                routeTemplate: GlobalConstants.EndPointNames.V7RemovePaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "RemoveModernPI" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.UpdatePaymentInstrumentEx,
                routeTemplate: GlobalConstants.EndPointNames.V7UpdatePaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "UpdateModernPI" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.ResumePendingOperationEx,
                routeTemplate: GlobalConstants.EndPointNames.V7ResumePendingOperationEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "ResumePendingOperation" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AnonymousResumePendingOperationEx,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousResumePendingOperationEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "AnonymousResumePendingOperation" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetCardProfileEx,
                routeTemplate: GlobalConstants.EndPointNames.V7GetCardProfileEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "GetCardProfile" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetSeCardPersos,
                routeTemplate: GlobalConstants.EndPointNames.V7GetSeCardPersosEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "GetSeCardPersos" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.PostPaymentInstrumentEx,
                routeTemplate: GlobalConstants.EndPointNames.V7PostReplenishTransactionCredentialsEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "PostReplenishTransactionCredentials" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AcquireLUKsEx,
                routeTemplate: GlobalConstants.EndPointNames.V7AcquireLuksEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "AcquireLUKs" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.ConfirmLUKsEx,
                routeTemplate: GlobalConstants.EndPointNames.V7ConfirmLuksEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "ConfirmLUKs" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.ValidateCvvEx,
                routeTemplate: GlobalConstants.EndPointNames.V7ValidateCvvEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "ValidateCvv" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetSettings,
                routeTemplate: GlobalConstants.EndPointNames.V7GetSettings,
                defaults: new { controller = GlobalConstants.ControllerNames.SettingsController });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetSettingsInPost,
                routeTemplate: GlobalConstants.EndPointNames.V7GetSettingsInPost,
                defaults: new { controller = GlobalConstants.ControllerNames.SettingsController, action = "GetSettingsInPost" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionApi,
                routeTemplate: GlobalConstants.EndPointNames.V7PaymentSessions,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "PostPaymentSession" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionGetApi,
                routeTemplate: GlobalConstants.EndPointNames.V7PaymentSessionsGet,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "GetPaymentSession" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.QRCodePaymentSessionGetApi,
                routeTemplate: GlobalConstants.EndPointNames.V7QrCodePaymentSessionsGet,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "qrCodeStatus" });
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.PaymentSessionCreateAndAuthenticateApi,
                routeTemplate: GlobalConstants.EndPointNames.V7PaymentSessionsCreateAndAuthenticate,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "CreateAndAuthenticate" });
            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticateApi,
               routeTemplate: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticate,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "Authenticate" });
            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.PaymentSessionNotifyThreeDSChallengeCompletedApi,
               routeTemplate: GlobalConstants.EndPointNames.V7PaymentSessionsNotifyThreeDSChallengeCompleted,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "NotifyThreeDSChallengeCompleted" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.BrowserFlowPaymentSessionsAuthenticateApi,
               routeTemplate: GlobalConstants.EndPointNames.V7BrowserFlowAuthenticate,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserAuthenticate" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.BrowserFlowPaymentSessionsNotifyThreeDSChallengeCompletedApi,
                routeTemplate: GlobalConstants.EndPointNames.V7BrowserFlowPaymentSessionsNotifyChallengeCompleted,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserNotifyThreeDSChallengeCompleted" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.PaymentSessionBrowserAuthenticateThreeDSOneApi,
               routeTemplate: GlobalConstants.EndPointNames.V7PaymentSessionsBrowserAuthenticateThreeDSOne,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserAuthenticateThreeDSOne" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.PaymentSessionBrowserAuthenticateRedirectionThreeDSOneApi,
               routeTemplate: GlobalConstants.EndPointNames.V7PaymentSessionsBrowserAuthenticateRedirectionThreeDSOne,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserAuthenticateRedirectionThreeDSOne" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.PaymentSessionBrowserNotifyThreeDSOneChallengeCompletedOneApi,
               routeTemplate: GlobalConstants.EndPointNames.V7PaymentSessionsBrowserNotifyThreeDSOneChallengeCompleted,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserNotifyThreeDSOneChallengeCompleted" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticateIndiaThreeDSApi,
               routeTemplate: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticateIndiaThreeDS,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "AuthenticateIndiaThreeDS" });

            config.Routes.MapHttpRoute(
                      name: GlobalConstants.V7RouteNames.PaymentTransactionApi,
                      routeTemplate: GlobalConstants.EndPointNames.V7PaymentTransactions,
                      defaults: new { controller = GlobalConstants.ControllerNames.PaymentTransactionsController });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.MSRewardsApi,
               routeTemplate: GlobalConstants.EndPointNames.V7MSRewards,
               defaults: new { controller = GlobalConstants.ControllerNames.MSRewardsController });

            config.Routes.MapHttpRoute(
              name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticationStatusApi,
              routeTemplate: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticationStatus,
              defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "AuthenticationStatus" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.AttachAddressCheckoutRequestExApi,
               routeTemplate: GlobalConstants.EndPointNames.V7AttachAddressCheckoutRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.CheckoutRequestsExController, action = "AttachAddress" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.AttachProfileCheckoutRequestExApi,
               routeTemplate: GlobalConstants.EndPointNames.V7AttachProfileCheckoutRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.CheckoutRequestsExController, action = "AttachProfile" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.ConfirmCheckoutRequestExApi,
               routeTemplate: GlobalConstants.EndPointNames.V7ConfirmCheckoutRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.CheckoutRequestsExController, action = "Confirm" });
            
            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.ExpressCheckoutConfirmApi,
               routeTemplate: GlobalConstants.EndPointNames.V7ExpressCheckoutConfirm,
               defaults: new { controller = GlobalConstants.ControllerNames.ExpressCheckoutController, action = "Confirm" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.TokensExApi,
               routeTemplate: GlobalConstants.EndPointNames.V7TokensEx,
               defaults: new { controller = GlobalConstants.ControllerNames.TokensExController, action = "Tokens" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.TokensExChallengeApi,
               routeTemplate: GlobalConstants.EndPointNames.V7TokensExChallenge,
               defaults: new { controller = GlobalConstants.ControllerNames.TokensExController, action = "PostChallenge" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.TokensExValidateChallengeApi,
               routeTemplate: GlobalConstants.EndPointNames.V7TokensExValidateChallenge,
               defaults: new { controller = GlobalConstants.ControllerNames.TokensExController, action = "ValidateChallenge" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.TokensExMandatesApi,
               routeTemplate: GlobalConstants.EndPointNames.V7TokensExMandate,
               defaults: new { controller = GlobalConstants.ControllerNames.TokensExController, action = "Mandates" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.GetAgenticTokenDescriptionsApi,
               routeTemplate: GlobalConstants.EndPointNames.V7AgenticTokenDescriptions,
               defaults: new { controller = GlobalConstants.ControllerNames.AgenticTokenDescriptionsController });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.AttachPaymentInstrumentExApi,
               routeTemplate: GlobalConstants.EndPointNames.V7AttachPaymentInstrumentCheckoutRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.CheckoutRequestsExController, action = "AttachPaymentInstrument" });

            // V7 PIDL Routes
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetPaymentMethodDescriptionsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7PaymentMethodDescriptions + "{id}",
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentMethodDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetPaymentMethodDescriptionsApiNoId,
                routeTemplate: GlobalConstants.EndPointNames.V7PaymentMethodDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentMethodDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetAddressDescriptionsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AddressDescriptions + "{id}",
                defaults: new { controller = GlobalConstants.ControllerNames.AddressDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetAddressDescriptionsApiNoId,
                routeTemplate: GlobalConstants.EndPointNames.V7AddressDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.AddressDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AddressesExApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AddressesEx,
                defaults: new { controller = GlobalConstants.ControllerNames.AddressesExController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AddressesExApiWithId,
                routeTemplate: GlobalConstants.EndPointNames.V7AddressesExWithId,
                defaults: new { controller = GlobalConstants.ControllerNames.AddressesExController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetChallengeDescriptionsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7ChallengeDescriptions + "{id}",
                defaults: new { controller = GlobalConstants.ControllerNames.ChallengeDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetChallengeDescriptionsApiNoId,
                routeTemplate: GlobalConstants.EndPointNames.V7ChallengeDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.ChallengeDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetProfileDescriptionsApiNoId,
                routeTemplate: GlobalConstants.EndPointNames.V7ProfileDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.ProfileDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetBillingGroupDescriptionsApiNoId,
                routeTemplate: GlobalConstants.EndPointNames.V7BillingGroupDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.BillingGroupDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetTaxIdDescriptionsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7TaxIdDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.TaxIdDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetTenantDescriptionsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7TenantDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.TenantDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetPaymentSessionDescriptionsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7GetPaymentSessionDescription,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionDescriptionsController });

            // Anonymous or authenticated
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.PostCardsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7Cards,
                defaults: new { controller = GlobalConstants.ControllerNames.CardsController });

            // Anonymous
            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AnonymousLegacyAddressValidationApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousLegacyAddressValidation,
                defaults: new { controller = GlobalConstants.ControllerNames.AddressesController, action = "LegacyValidate" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AnonymousModernAddressValidationApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousModernAddressValidation,
                defaults: new { controller = GlobalConstants.ControllerNames.AddressesController, action = "ModernValidate" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AnonymousRDSSessionQueryApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousRDSSessionQuery,
                defaults: new { controller = GlobalConstants.ControllerNames.RDSSessionController, action = "Query" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.TransformationApi,
                routeTemplate: GlobalConstants.EndPointNames.V7Transformation,
                defaults: new { controller = GlobalConstants.ControllerNames.PidlTransformationController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.ValidationApi,
                routeTemplate: GlobalConstants.EndPointNames.V7Validation,
                defaults: new { controller = GlobalConstants.ControllerNames.PidlValidationController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AnonymousGetAddressDescriptionsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousAddressDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.AddressDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AnonymousGetTaxIdDescriptionsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousTaxIdDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.TaxIdDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AnonymousGetPaymentMethodDescriptionsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousPaymentMethodDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentMethodDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AnonymousGetPaymentMethodDescriptionsSessionIdApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousPaymentMethodDescriptionsSessionId,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentMethodDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.SessionsByIdApi,
                routeTemplate: GlobalConstants.EndPointNames.V7SessionsById,
                defaults: new { controller = GlobalConstants.ControllerNames.SessionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.SessionsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7Sessions,
                defaults: new { controller = GlobalConstants.ControllerNames.SessionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AnonymousGetCheckoutDescriptionsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousCheckoutDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.CheckoutDescriptionsController });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AnonymousCheckoutsExApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousCheckoutsExCharge,
                defaults: new { controller = GlobalConstants.ControllerNames.CheckoutsExController, action = "Charge" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AnonymousCheckoutsExStatusApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousCheckoutsExStatus,
                defaults: new { controller = GlobalConstants.ControllerNames.CheckoutsExController, action = "Status" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.AnonymousCheckoutsExCompletedApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousCheckoutsExCompleted,
                defaults: new { controller = GlobalConstants.ControllerNames.CheckoutsExController, action = "Completed" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.ApplyPaymentInstrumentEx,
                routeTemplate: GlobalConstants.EndPointNames.V7ApplyPaymentInstrumentEx,
                defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "apply" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.CreatePaymentInstrumentEx,
               routeTemplate: GlobalConstants.EndPointNames.V7CreatePaymentInstrumentEx,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "CreateModernPI" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.WalletsGetConfigApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousGetWalletConfig,
                defaults: new { controller = GlobalConstants.ControllerNames.WalletsController, action = "GetWalletConfig" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.WalletsSetupProviderSessionApi,
                routeTemplate: GlobalConstants.EndPointNames.V7AnonymousWalletSetupProviderSession,
                defaults: new { controller = GlobalConstants.ControllerNames.WalletsController, action = "SetupWalletProviderSession" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.WalletsProvisionWalletTokenApi,
                routeTemplate: GlobalConstants.EndPointNames.V7ProvisionWalletToken,
                defaults: new { controller = GlobalConstants.ControllerNames.WalletsController, action = "ProvisionWalletToken" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetRewardsDescriptionsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7RewardsDescriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.RewardsDescriptionsController });

            // Payment Client
            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.PaymentClientInitializationApi,
               routeTemplate: GlobalConstants.EndPointNames.V7PaymentClientInitialization,
               defaults: new { controller = GlobalConstants.ControllerNames.InitializationController, action = "Initialize" });

            config.Routes.MapHttpRoute(
                name: GlobalConstants.V7RouteNames.GetDescriptionsApi,
                routeTemplate: GlobalConstants.EndPointNames.V7Descriptions,
                defaults: new { controller = GlobalConstants.ControllerNames.DescriptionsController });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.ConfirmPaymentRequestExApi,
               routeTemplate: GlobalConstants.EndPointNames.V7ConfirmPaymentRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentRequestsExController, action = "Confirm" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.AttachChallengeDataPaymentRequestExApi,
               routeTemplate: GlobalConstants.EndPointNames.V7AttachChallengeDataPaymentRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentRequestsExController, action = "AttachChallengeData" });

            config.Routes.MapHttpRoute(
               name: GlobalConstants.V7RouteNames.RemoveEligiblePaymentmethodsPaymentRequestExApi,
               routeTemplate: GlobalConstants.EndPointNames.V7RemoveEligiblePaymentmethodsPaymentRequestsEx,
               defaults: new { controller = GlobalConstants.ControllerNames.PaymentRequestsExController, action = "RemoveEligiblePaymentmethods" });
        }

        private static void InitVersionSelector()
        {
            selector = new VersionedControllerSelector();
            AddV7Controllers();
        }

        private static void AddV7Controllers()
        {
            selector.AddVersionless(GlobalConstants.ControllerNames.ProbeController, typeof(ProbeController));
            selector.Add(
                new ApiVersion(V7.Constants.Versions.ApiVersion, new Version(7, 0)),
                new Dictionary<string, Type>
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
                });
        }

        private static void EnsureSllInitialized()
        {
            SllLogger.TraceMessage("Initialize SllLogger and Sll static dependencies.", EventLevel.Informational);
            AuditLogger.Instantiate();
        }
    }
}
