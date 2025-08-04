using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Commerce.Payments.PXCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Microsoft.Commerce.Payments.PXService;
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);

var validateCors = builder.Configuration.GetValue<bool>("PXServiceSettings:ValidateCors");
var corsOrigins = builder.Configuration.GetSection("PXServiceSettings:CorsAllowedOrigins").Get<string[]>();

if (validateCors)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("PXServiceCors", policy =>
        {
            policy.WithOrigins(corsOrigins ?? Array.Empty<string>())
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

if (validateCors)
{
    app.UseCors("PXServiceCors");
}

app.UseAuthorization();

MapApiRoutes(app);

app.Run();

static void MapApiRoutes(IEndpointRouteBuilder endpoints)
{
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.Probe,
        pattern: GlobalConstants.EndPointNames.V7Probe,
        defaults: new { controller = GlobalConstants.ControllerNames.ProbeController, action = "Get" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetPaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7GetPaymentInstrumentEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "GetModernPI" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ListPaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7ListPaymentInstrumentEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetChallengeContextPaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7GetChallengeContextPaymentInstrumentEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "GetChallengeContext" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ReplacePaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7ReplacePaymentInstrumentEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "ReplaceModernPI" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.RedeemPaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7RedeemPaymentInstrumentEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "RedeemModernPI" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.RemovePaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7RemovePaymentInstrumentEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "RemoveModernPI" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.UpdatePaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7UpdatePaymentInstrumentEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "UpdateModernPI" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ResumePendingOperationEx,
        pattern: GlobalConstants.EndPointNames.V7ResumePendingOperationEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "ResumePendingOperation" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AnonymousResumePendingOperationEx,
        pattern: GlobalConstants.EndPointNames.V7AnonymousResumePendingOperationEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "AnonymousResumePendingOperation" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetCardProfileEx,
        pattern: GlobalConstants.EndPointNames.V7GetCardProfileEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "GetCardProfile" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetSeCardPersos,
        pattern: GlobalConstants.EndPointNames.V7GetSeCardPersosEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "GetSeCardPersos" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PostPaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7PostReplenishTransactionCredentialsEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "PostReplenishTransactionCredentials" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AcquireLUKsEx,
        pattern: GlobalConstants.EndPointNames.V7AcquireLuksEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "AcquireLUKs" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ConfirmLUKsEx,
        pattern: GlobalConstants.EndPointNames.V7ConfirmLuksEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "ConfirmLUKs" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ValidateCvvEx,
        pattern: GlobalConstants.EndPointNames.V7ValidateCvvEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "ValidateCvv" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetSettings,
        pattern: GlobalConstants.EndPointNames.V7GetSettings,
        defaults: new { controller = GlobalConstants.ControllerNames.SettingsController });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetSettingsInPost,
        pattern: GlobalConstants.EndPointNames.V7GetSettingsInPost,
        defaults: new { controller = GlobalConstants.ControllerNames.SettingsController, action = "GetSettingsInPost" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentSessionApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessions,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "PostPaymentSession" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentSessionGetApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessionsGet,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "GetPaymentSession" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.QRCodePaymentSessionGetApi,
        pattern: GlobalConstants.EndPointNames.V7QrCodePaymentSessionsGet,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "GetQrCodePaymentSession" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentSessionCreateAndAuthenticateApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessionsCreateAndAuthenticate,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "CreateAndAuthenticatePaymentSession" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticateApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticate,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "AuthenticatePaymentSession" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentSessionNotifyThreeDSChallengeCompletedApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessionsNotifyThreeDSChallengeCompleted,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "NotifyThreeDSChallengeCompleted" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.BrowserFlowPaymentSessionsAuthenticateApi,
        pattern: GlobalConstants.EndPointNames.V7BrowserFlowAuthenticate,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserFlowAuthenticate" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.BrowserFlowPaymentSessionsNotifyThreeDSChallengeCompletedApi,
        pattern: GlobalConstants.EndPointNames.V7BrowserFlowPaymentSessionsNotifyChallengeCompleted,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserFlowNotifyChallengeCompleted" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentSessionBrowserAuthenticateThreeDSOneApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessionsBrowserAuthenticateThreeDSOne,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserAuthenticateThreeDSOne" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentSessionBrowserAuthenticateRedirectionThreeDSOneApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessionsBrowserAuthenticateRedirectionThreeDSOne,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserAuthenticateRedirectionThreeDSOne" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentSessionBrowserNotifyThreeDSOneChallengeCompletedOneApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessionsBrowserNotifyThreeDSOneChallengeCompleted,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserNotifyThreeDSOneChallengeCompleted" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticateIndiaThreeDSApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticateIndiaThreeDS,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "AuthenticateIndiaThreeDS" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentTransactionApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentTransactions,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentTransactionsController });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.MSRewardsApi,
        pattern: GlobalConstants.EndPointNames.V7MSRewards,
        defaults: new { controller = GlobalConstants.ControllerNames.MSRewardsController });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticationStatusApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticationStatus,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "PaymentSessionAuthenticationStatus" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AttachAddressCheckoutRequestExApi,
        pattern: GlobalConstants.EndPointNames.V7AttachAddressCheckoutRequestsEx,
        defaults: new { controller = GlobalConstants.ControllerNames.CheckoutRequestsExController, action = "AttachAddress" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AttachProfileCheckoutRequestExApi,
        pattern: GlobalConstants.EndPointNames.V7AttachProfileCheckoutRequestsEx,
        defaults: new { controller = GlobalConstants.ControllerNames.CheckoutRequestsExController, action = "AttachProfile" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ConfirmCheckoutRequestExApi,
        pattern: GlobalConstants.EndPointNames.V7ConfirmCheckoutRequestsEx,
        defaults: new { controller = GlobalConstants.ControllerNames.CheckoutRequestsExController, action = "Confirm" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ExpressCheckoutConfirmApi,
        pattern: GlobalConstants.EndPointNames.V7ExpressCheckoutConfirm,
        defaults: new { controller = GlobalConstants.ControllerNames.ExpressCheckoutController, action = "Confirm" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.TokensExApi,
        pattern: GlobalConstants.EndPointNames.V7TokensEx,
        defaults: new { controller = GlobalConstants.ControllerNames.TokensExController, action = "CreateToken" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.TokensExChallengeApi,
        pattern: GlobalConstants.EndPointNames.V7TokensExChallenge,
        defaults: new { controller = GlobalConstants.ControllerNames.TokensExController, action = "Challenge" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.TokensExValidateChallengeApi,
        pattern: GlobalConstants.EndPointNames.V7TokensExValidateChallenge,
        defaults: new { controller = GlobalConstants.ControllerNames.TokensExController, action = "ValidateChallenge" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.TokensExMandatesApi,
        pattern: GlobalConstants.EndPointNames.V7TokensExMandate,
        defaults: new { controller = GlobalConstants.ControllerNames.TokensExController, action = "Mandate" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetAgenticTokenDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7AgenticTokenDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.AgenticTokenDescriptionsController });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AttachPaymentInstrumentExApi,
        pattern: GlobalConstants.EndPointNames.V7AttachPaymentInstrumentCheckoutRequestsEx,
        defaults: new { controller = GlobalConstants.ControllerNames.CheckoutRequestsExController, action = "AttachPaymentInstrument" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetPaymentMethodDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentMethodDescriptions + "{id}",
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentMethodDescriptionsController, action = "Get" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetPaymentMethodDescriptionsApiNoId,
        pattern: GlobalConstants.EndPointNames.V7PaymentMethodDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentMethodDescriptionsController, action = "List" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetAddressDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7AddressDescriptions + "{id}",
        defaults: new { controller = GlobalConstants.ControllerNames.AddressDescriptionsController, action = "Get" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetAddressDescriptionsApiNoId,
        pattern: GlobalConstants.EndPointNames.V7AddressDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.AddressDescriptionsController, action = "List" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AddressesExApi,
        pattern: GlobalConstants.EndPointNames.V7AddressesEx,
        defaults: new { controller = GlobalConstants.ControllerNames.AddressesExController, action = "Create" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AddressesExApiWithId,
        pattern: GlobalConstants.EndPointNames.V7AddressesExWithId,
        defaults: new { controller = GlobalConstants.ControllerNames.AddressesExController, action = "Get" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetChallengeDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7ChallengeDescriptions + "{id}",
        defaults: new { controller = GlobalConstants.ControllerNames.ChallengeDescriptionsController, action = "Get" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetChallengeDescriptionsApiNoId,
        pattern: GlobalConstants.EndPointNames.V7ChallengeDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.ChallengeDescriptionsController, action = "List" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetProfileDescriptionsApiNoId,
        pattern: GlobalConstants.EndPointNames.V7ProfileDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.ProfileDescriptionsController, action = "List" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetBillingGroupDescriptionsApiNoId,
        pattern: GlobalConstants.EndPointNames.V7BillingGroupDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.BillingGroupDescriptionsController, action = "List" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetTaxIdDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7TaxIdDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.TaxIdDescriptionsController, action = "List" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetTenantDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7TenantDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.TenantDescriptionsController, action = "List" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetPaymentSessionDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7GetPaymentSessionDescription,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionDescriptionsController, action = "Get" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PostCardsApi,
        pattern: GlobalConstants.EndPointNames.V7Cards,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "PostCard" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AnonymousLegacyAddressValidationApi,
        pattern: GlobalConstants.EndPointNames.V7AnonymousLegacyAddressValidation,
        defaults: new { controller = GlobalConstants.ControllerNames.AddressesController, action = "LegacyAddressValidation" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AnonymousModernAddressValidationApi,
        pattern: GlobalConstants.EndPointNames.V7AnonymousModernAddressValidation,
        defaults: new { controller = GlobalConstants.ControllerNames.AddressesController, action = "ModernAddressValidation" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AnonymousRDSSessionQueryApi,
        pattern: GlobalConstants.EndPointNames.V7AnonymousRDSSessionQuery,
        defaults: new { controller = GlobalConstants.ControllerNames.RDSSessionController, action = "Get" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.TransformationApi,
        pattern: GlobalConstants.EndPointNames.V7Transformation,
        defaults: new { controller = GlobalConstants.ControllerNames.PidlTransformationController, action = "Post" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ValidationApi,
        pattern: GlobalConstants.EndPointNames.V7Validation,
        defaults: new { controller = GlobalConstants.ControllerNames.PidlValidationController, action = "Post" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AnonymousGetAddressDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7AnonymousAddressDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.AddressDescriptionsController, action = "AnonymousList" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AnonymousGetTaxIdDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7AnonymousTaxIdDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.TaxIdDescriptionsController, action = "AnonymousList" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AnonymousGetPaymentMethodDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7AnonymousPaymentMethodDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentMethodDescriptionsController, action = "AnonymousList" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AnonymousGetPaymentMethodDescriptionsSessionIdApi,
        pattern: GlobalConstants.EndPointNames.V7AnonymousPaymentMethodDescriptionsSessionId,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentMethodDescriptionsController, action = "AnonymousListBySession" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.SessionsByIdApi,
        pattern: GlobalConstants.EndPointNames.V7SessionsById,
        defaults: new { controller = GlobalConstants.ControllerNames.SessionsController, action = "GetSession" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.SessionsApi,
        pattern: GlobalConstants.EndPointNames.V7Sessions,
        defaults: new { controller = GlobalConstants.ControllerNames.SessionsController, action = "PostSession" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AnonymousGetCheckoutDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7AnonymousCheckoutDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.CheckoutDescriptionsController, action = "AnonymousList" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AnonymousCheckoutsExApi,
        pattern: GlobalConstants.EndPointNames.V7AnonymousCheckoutsExCharge,
        defaults: new { controller = GlobalConstants.ControllerNames.CheckoutsExController, action = "AnonymousCharge" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AnonymousCheckoutsExStatusApi,
        pattern: GlobalConstants.EndPointNames.V7AnonymousCheckoutsExStatus,
        defaults: new { controller = GlobalConstants.ControllerNames.CheckoutsExController, action = "AnonymousStatus" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AnonymousCheckoutsExCompletedApi,
        pattern: GlobalConstants.EndPointNames.V7AnonymousCheckoutsExCompleted,
        defaults: new { controller = GlobalConstants.ControllerNames.CheckoutsExController, action = "AnonymousCompleted" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ApplyPaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7ApplyPaymentInstrumentEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "Apply" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.CreatePaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7CreatePaymentInstrumentEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController, action = "CreateModernPI" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.WalletsGetConfigApi,
        pattern: GlobalConstants.EndPointNames.V7AnonymousGetWalletConfig,
        defaults: new { controller = GlobalConstants.ControllerNames.WalletsController, action = "GetConfig" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.WalletsSetupProviderSessionApi,
        pattern: GlobalConstants.EndPointNames.V7AnonymousWalletSetupProviderSession,
        defaults: new { controller = GlobalConstants.ControllerNames.WalletsController, action = "SetupProviderSession" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.WalletsProvisionWalletTokenApi,
        pattern: GlobalConstants.EndPointNames.V7ProvisionWalletToken,
        defaults: new { controller = GlobalConstants.ControllerNames.WalletsController, action = "ProvisionWalletToken" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetRewardsDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7RewardsDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.RewardsDescriptionsController, action = "List" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentClientInitializationApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentClientInitialization,
        defaults: new { controller = GlobalConstants.ControllerNames.InitializationController, action = "Post" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7Descriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.DescriptionsController, action = "List" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ConfirmPaymentRequestExApi,
        pattern: GlobalConstants.EndPointNames.V7ConfirmPaymentRequestsEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentRequestsExController, action = "Confirm" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AttachChallengeDataPaymentRequestExApi,
        pattern: GlobalConstants.EndPointNames.V7AttachChallengeDataPaymentRequestsEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentRequestsExController, action = "AttachChallengeData" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.RemoveEligiblePaymentmethodsPaymentRequestExApi,
        pattern: GlobalConstants.EndPointNames.V7RemoveEligiblePaymentmethodsPaymentRequestsEx,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentRequestsExController, action = "RemoveEligiblePaymentMethods" });
}