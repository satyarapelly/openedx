using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Commerce.Payments.Common.Environments;
using Microsoft.Commerce.Payments.Common.Tracing;
using Microsoft.Commerce.Payments.Common.Web; // VersionedControllerResolver (Core helper)
using Microsoft.Commerce.Payments.PXCommon;
using Microsoft.Commerce.Payments.PXService;
using Microsoft.Commerce.Payments.PXService.Settings;
using Newtonsoft.Json;
using System.Diagnostics.Tracing;
using Environment = Microsoft.Commerce.Payments.Common.Environments.Environment;

var builder = WebApplication.CreateBuilder(args);

// config + settings
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile("local.settings.json", optional: true)
    .AddEnvironmentVariables();

var pxSettings = PXServiceSettings.CreateInstance(Environment.Current.EnvironmentType, Environment.Current.EnvironmentName);
builder.Services.AddSingleton(pxSettings);

// Define supported API versions and controllers allowed without an explicit version
var supportedVersions = new Dictionary<string, ApiVersion>(StringComparer.OrdinalIgnoreCase)
{
    { "v7.0", new ApiVersion("v7.0", new Version(7, 0)) }
};
string[] versionlessControllers = { C(GlobalConstants.ControllerNames.ProbeController) };

// controllers + Newtonsoft.Json (ignore nulls)
builder.Services.AddControllers(o =>
{
    o.Filters.Add(new PXServiceExceptionFilter());
})
.AddNewtonsoftJson(o => o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);

// optional – if you rely on your own AI setup it’s fine to leave this
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

ApplicationInsightsProvider.SetupAppInsightsConfiguration(pxSettings.ApplicationInsightInstrumentKey, pxSettings.ApplicationInsightMode);
EnsureSllInitialized();

app.UseHttpsRedirection();

// Optional endpoint to debug routes
app.MapGet("/routes", (EndpointDataSource ds) =>
    Results.Text(string.Join(System.Environment.NewLine, ds.Endpoints.OfType<RouteEndpoint>().Select(e => e.RoutePattern.RawText))));
// Trace correlation (mirrors WebApiConfig)
if (!WebHostingUtility.IsApplicationSelfHosted())
{
    app.UseMiddleware<PXTraceCorrelationHandler>(Constants.ServiceNames.PXService, ApplicationInsightsProvider.LogIncomingOperation);
}

// API version handler
app.UseMiddleware<PXServiceApiVersionHandler>(supportedVersions, versionlessControllers, pxSettings);

// CORS
if (pxSettings.ValidateCors)
{
    app.UseMiddleware<PXServiceCorsHandler>(pxSettings);
}

// Input and PIDL validation handlers
app.UseMiddleware<PXServiceInputValidationHandler>();
if (pxSettings.PIDLDocumentValidationEnabled)
{
    app.UseMiddleware<PXServicePIDLValidationHandler>();
}
app.UseRouting();

// Version gate: after routing, before controllers
app.Use(async (ctx, next) =>
{
    var endpoint = ctx.GetEndpoint();
    var cad = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
    var controllerName = cad?.ControllerName; // short name (no "Controller")
    if (!string.IsNullOrEmpty(controllerName))
    {
        var resolver = ctx.RequestServices.GetRequiredService<VersionedControllerSelector>();
        var allowedType = resolver.ResolveAllowedController(ctx); // returns Type? or null per your Core helper
        if (allowedType is null)
        {
            var version = ctx.Request.Headers["api-version"].ToString();
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            await ctx.Response.WriteAsync($"No controller mapped for version '{(string.IsNullOrWhiteSpace(version) ? "(none)" : version)}'.");
            return;
        }

        // ok, let it flow
        await next();
        return;
    }

    // No endpoint matched (RouteData is null/empty)  try to parse and fail fast with a clearer 404
    // Expected path like: /v7.0/Account001/AddressDescriptions
    var segments = ctx.Request.Path.Value?.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
    if (segments.Length >= 3 && segments[0].StartsWith("v", StringComparison.OrdinalIgnoreCase))
    {
        var parsedVersion = segments[0][1..];         // "7.0"
        var parsedController = segments[2];           // "AddressDescriptions"
        var resolver = ctx.RequestServices.GetRequiredService<VersionedControllerSelector>();

        // Fake the route values just for checking
        ctx.Request.RouteValues["controller"] = parsedController;
        if (resolver.ResolveAllowedController(ctx) is null)
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            await ctx.Response.WriteAsync($"No controller mapped for version '{parsedVersion}'.");
            return;
        }
    }
    await next();
});

// Conventional maps that mimic your old WebApiConfig
AddUrlVersionedRoutes(app);

app.MapControllers();

// graceful shutdown (replaces Global.asax Application_End)
app.Lifetime.ApplicationStopping.Register(() => pxSettings?.AzureExPAccessor?.StopPolling());

app.Run();

// -------- helpers --------
static void EnsureSllInitialized()
{
    SllLogger.TraceMessage("Initialize SllLogger and Sll static dependencies.", EventLevel.Informational);
    AuditLogger.Instantiate();
}

static string C(string name) =>
    name.EndsWith("Controller", StringComparison.Ordinal) ? name[..^"Controller".Length] : name;

static void AddUrlVersionedRoutes(IEndpointRouteBuilder endpoints)
{
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.Probe,
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
        defaults: new { controller = C(GlobalConstants.ControllerNames.ChallengeDescriptionsController), action = "Get" });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetChallengeDescriptionsApiNoId,
        pattern: GlobalConstants.EndPointNames.V7ChallengeDescriptions,
        defaults: new { controller = C(GlobalConstants.ControllerNames.ChallengeDescriptionsController), action = "List" });

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
        defaults: new { controller = C(GlobalConstants.ControllerNames.TenantDescriptionsController), action = "List" });

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
        defaults: new { controller = C(GlobalConstants.ControllerNames.SessionsController), action = "GetSession" });

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
}
