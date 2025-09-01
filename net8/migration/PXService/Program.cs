using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Commerce.Payments.Common.Environments;
using Microsoft.Commerce.Payments.Common.Tracing;
using Microsoft.Commerce.Payments.Common.Web;
using Microsoft.Commerce.Payments.PXService;
using Microsoft.Commerce.Payments.PXService.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

// Register PX service dependencies using legacy WebApiConfig
WebApiConfig.Register(builder, pxSettings);

// optional – if you rely on your own AI setup it’s fine to leave this
builder.Services.AddApplicationInsightsTelemetry();

using var app = builder.Build();

// Rewrite URLs to maintain legacy behavior.
var rewrite = new RewriteOptions()
    // Remove a trailing slash so routes match whether or not it is provided.
    .AddRewrite(@"^(.*[^/])/$", "$1", skipRemainingRules: false)
    // Replicate <rewrite> rule: /px/(.*) -> /$1  (internal rewrite)
    .AddRewrite(@"^px/(.*)$", "$1", skipRemainingRules: true);
app.UseRewriter(rewrite);

// Ensure the routing matcher runs before custom middleware so HttpContext.GetEndpoint()
// is populated when those middlewares execute.
app.UseRouting();
WebApiConfig.Configure(app, pxSettings);
ApplicationInsightsProvider.SetupAppInsightsConfiguration(pxSettings.ApplicationInsightInstrumentKey, pxSettings.ApplicationInsightMode);
EnsureSllInitialized();

// Conditionally redirect HTTP to HTTPS only when not self-hosted
if (!WebHostingUtility.IsApplicationSelfHosted())
{
    app.UseHttpsRedirection();
}

// API version handler
app.UseMiddleware<PXServiceApiVersionHandler>();

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

// Conventional maps that mimic your old WebApiConfig
WebApiConfig.AddUrlVersionedRoutes(app);

app.MapControllers();

// graceful shutdown (replaces Global.asax Application_End)
app.Lifetime.ApplicationStopping.Register(() => pxSettings?.AzureExPAccessor?.StopPolling());

await app.RunAsync();

// -------- helpers --------
static void EnsureSllInitialized()
{
    SllLogger.TraceMessage("Initialize SllLogger and Sll static dependencies.", EventLevel.Informational);
    AuditLogger.Instantiate();
}
