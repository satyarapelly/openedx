using System;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Commerce.Payments.PXService;
using Microsoft.Commerce.Payments.PXService.Settings;
using Microsoft.Commerce.Payments.PXCommon;
using Microsoft.Commerce.Payments.Common.Tracing;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Commerce.Payments.Common.Web;
using Microsoft.Commerce.Payments.Common.Environments;
using System.Diagnostics.Tracing;
using Microsoft.Commerce.Payments.PXService.Handlers;
using Microsoft.CommonSchema.Services.Listeners;
using Environment = Microsoft.Commerce.Payments.Common.Environments.Environment;

// If your custom handlers were ported to middleware, import their namespaces too:
// using Microsoft.Commerce.Payments.PXService.Middleware;  // e.g., PXTraceCorrelationMiddleware, etc.

var builder = WebApplication.CreateBuilder(args);

// Mirror Global.asax security protocol initialization
ServicePointManager.SecurityProtocol &= ~(SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11);
if (ServicePointManager.SecurityProtocol == 0)
{
    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
}

// Create and register PX settings (mirrors Global.asax Application_Start)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile("local.settings.json", optional: true)
    .AddEnvironmentVariables();

var pxSettings = PXServiceSettings.CreateInstance(Environment.Current.EnvironmentType, Environment.Current.EnvironmentName);

builder.Services.AddSingleton(pxSettings);

// WebApiConfig settings
ServicePointManager.CheckCertificateRevocationList = true;
ApplicationInsightsProvider.SetupAppInsightsConfiguration(pxSettings.ApplicationInsightInstrumentKey, pxSettings.ApplicationInsightMode);
EnsureSllInitialized();

// Define supported API versions and controllers allowed without an explicit version
var supportedVersions = VersionCatalog.Supported.ToDictionary(
    kvp => kvp.Key,
    kvp => new ApiVersion(kvp.Key, new Version(kvp.Value)));
string[] versionlessControllers = { GlobalConstants.ControllerNames.ProbeController };

// Controllers + Newtonsoft.Json (Nulls ignored like WebApiConfig)
builder.Services.AddScoped<VersionGateFilter>();

builder.Services
    .AddControllers(options =>
    {
        // Global filters (was: config.Filters.Add(...))
        options.Filters.Add(new PXServiceExceptionFilter());
        if (pxSettings.AuthorizationFilter is not null)
        {
            options.Filters.Add(pxSettings.AuthorizationFilter);
        }
        options.Filters.Add<VersionGateFilter>();
    })
    .AddNewtonsoftJson(o =>
    {
        o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

// If you used HttpClient or Application Insights, register those here as needed.
builder.Services.AddHttpClient();
builder.Services.AddApplicationInsightsTelemetry(); // optional if you rely on your own AI setup

// Register the resolver and populate it with your mappings (V7, probe, etc.)
builder.Services.AddSingleton<VersionedControllerResolver>(sp =>
{
    var resolverLogger = sp.GetRequiredService<ILogger<VersionedControllerResolver>>();
    var resolver = new VersionedControllerResolver(resolverLogger);

    // Add versioned and versionless controllers
    var catalogLogger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("VersionCatalog");
    VersionCatalog.Register(resolver, catalogLogger);

    return resolver;
});

var app = builder.Build();

app.MapGet("/routes", (EndpointDataSource ds) =>
    Results.Text(string.Join(System.Environment.NewLine,
        ds.Endpoints.OfType<RouteEndpoint>().Select(e => e.RoutePattern.RawText))));

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

// Input validation handler
app.UseMiddleware<PXServiceInputValidationHandler>();

app.UseRouting();

// Gate requests using the resolver: if no controller is registered for (version, controller) -> 404
app.Use(async (ctx, next) =>
{
    var routeData = ctx.GetRouteData();
    if (routeData?.Values.TryGetValue("controller", out var _) == true)
    {
        var resolver = ctx.RequestServices.GetRequiredService<VersionedControllerResolver>();
        var resolvedType = resolver.ResolveAllowedController(ctx);
        if (resolvedType is null)
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            await ctx.Response.WriteAsync($"No controller mapped for version '{ctx.Request.Headers["api-version"]}'.");
            return;
        }
    }

    await next();
});

if (pxSettings.PIDLDocumentValidationEnabled)
{
    app.UseMiddleware<PXServicePIDLValidationHandler>();
}

app.UseEndpoints(endpoints =>
{
    MapV7Routes(endpoints);
    endpoints.MapControllers();
});



// Graceful shutdown hook (replaces Global.asax Application_End)
app.Lifetime.ApplicationStopping.Register(() =>
{
    pxSettings?.AzureExPAccessor?.StopPolling();
});

app.Run();

// ----------------- helpers -----------------
static void EnsureSllInitialized()
{
    SllLogger.TraceMessage("Initialize SllLogger and Sll static dependencies.", EventLevel.Informational);
    AuditLogger.Instantiate();
}

static void MapV7Routes(IEndpointRouteBuilder endpoints)
{
    // Probe
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.Probe,
        pattern: GlobalConstants.EndPointNames.V7Probe,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.ProbeController,
            action = "Get"
        });

    // PaymentInstrumentsEx
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetPaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7GetPaymentInstrumentEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "GetModernPI"
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ListPaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7ListPaymentInstrumentEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetChallengeContextPaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7GetChallengeContextPaymentInstrumentEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "GetChallengeContext"
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ReplacePaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7ReplacePaymentInstrumentEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "ReplaceModernPI"
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.RedeemPaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7RedeemPaymentInstrumentEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "RedeemModernPI"
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.RemovePaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7RemovePaymentInstrumentEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "RemoveModernPI"
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.UpdatePaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7UpdatePaymentInstrumentEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "UpdateModernPI"
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ResumePendingOperationEx,
        pattern: GlobalConstants.EndPointNames.V7ResumePendingOperationEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "ResumePendingOperation"
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AnonymousResumePendingOperationEx,
        pattern: GlobalConstants.EndPointNames.V7AnonymousResumePendingOperationEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "AnonymousResumePendingOperation"
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetCardProfileEx,
        pattern: GlobalConstants.EndPointNames.V7GetCardProfileEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "GetCardProfile"
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetSeCardPersos,
        pattern: GlobalConstants.EndPointNames.V7GetSeCardPersosEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "GetSeCardPersos"
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PostPaymentInstrumentEx,
        pattern: GlobalConstants.EndPointNames.V7PostReplenishTransactionCredentialsEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "PostReplenishTransactionCredentials"
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.AcquireLUKsEx,
        pattern: GlobalConstants.EndPointNames.V7AcquireLuksEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "AcquireLUKs"
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ConfirmLUKsEx,
        pattern: GlobalConstants.EndPointNames.V7ConfirmLuksEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "ConfirmLUKs"
        });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.ValidateCvvEx,
        pattern: GlobalConstants.EndPointNames.V7ValidateCvvEx,
        defaults: new
        {
            controller = GlobalConstants.ControllerNames.PaymentInstrumentsExController,
            action = "ValidateCvv"
        });

    // Settings
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetSettings,
        pattern: GlobalConstants.EndPointNames.V7GetSettings,
        defaults: new { controller = GlobalConstants.ControllerNames.SettingsController });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetSettingsInPost,
        pattern: GlobalConstants.EndPointNames.V7GetSettingsInPost,
        defaults: new { controller = GlobalConstants.ControllerNames.SettingsController, action = "GetSettingsInPost" });

    // Payment Sessions (+ Browser flow / India / 3DS, etc.)
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
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "qrCodeStatus" });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentSessionCreateAndAuthenticateApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessionsCreateAndAuthenticate,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "CreateAndAuthenticate" });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticateApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticate,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "Authenticate" });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentSessionNotifyThreeDSChallengeCompletedApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessionsNotifyThreeDSChallengeCompleted,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "NotifyThreeDSChallengeCompleted" });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.BrowserFlowPaymentSessionsAuthenticateApi,
        pattern: GlobalConstants.EndPointNames.V7BrowserFlowAuthenticate,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserAuthenticate" });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.BrowserFlowPaymentSessionsNotifyThreeDSChallengeCompletedApi,
        pattern: GlobalConstants.EndPointNames.V7BrowserFlowPaymentSessionsNotifyChallengeCompleted,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "BrowserNotifyThreeDSChallengeCompleted" });
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
        name: GlobalConstants.V7RouteNames.PaymentSessionAuthenticationStatusApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentSessionsAuthenticationStatus,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentSessionsController, action = "AuthenticationStatus" });

    // Transactions
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.PaymentTransactionApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentTransactions,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentTransactionsController });

    // Rewards
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.MSRewardsApi,
        pattern: GlobalConstants.EndPointNames.V7MSRewards,
        defaults: new { controller = GlobalConstants.ControllerNames.MSRewardsController });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetRewardsDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7RewardsDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.RewardsDescriptionsController });

    // PIDL/Descriptions (addresses, payment methods, challenges, profiles, billing groups, tax, tenant)
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetPaymentMethodDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7PaymentMethodDescriptions + "{id}",
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentMethodDescriptionsController });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetPaymentMethodDescriptionsApiNoId,
        pattern: GlobalConstants.EndPointNames.V7PaymentMethodDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.PaymentMethodDescriptionsController });

    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetAddressDescriptionsApi,
        pattern: GlobalConstants.EndPointNames.V7AddressDescriptions + "{id}",
        defaults: new { controller = GlobalConstants.ControllerNames.AddressDescriptionsController });
    endpoints.MapControllerRoute(
        name: GlobalConstants.V7RouteNames.GetAddressDescriptionsApiNoId,
        pattern: GlobalConstants.EndPointNames.V7AddressDescriptions,
        defaults: new { controller = GlobalConstants.ControllerNames.AddressDescriptionsController });
}
