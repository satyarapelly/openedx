using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators;

var builder = WebApplication.CreateBuilder(args);
WebApiConfig.Register(builder);

var app = builder.Build();
WebApiConfig.ConfigureRoutes(app);

app.Run();

