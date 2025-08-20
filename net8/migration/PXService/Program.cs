using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Commerce.Payments.PXService;
using Microsoft.Commerce.Payments.PXService.Settings;
using Microsoft.Commerce.Payments.Common.Environments;
using Environment = Microsoft.Commerce.Payments.Common.Environments.Environment;

var builder = WebApplication.CreateBuilder(args);

// Load configuration files and environment variables
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile("local.settings.json", optional: true)
    .AddEnvironmentVariables();

// Create PX service settings and register required services
var pxSettings = PXServiceSettings.CreateInstance(
    Environment.Current.EnvironmentType,
    Environment.Current.EnvironmentName);
WebApiConfig.Register(builder, pxSettings);

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    WebApiConfig.AddUrlVersionedRoutes(endpoints);
    endpoints.MapControllers();
});

app.Run();
