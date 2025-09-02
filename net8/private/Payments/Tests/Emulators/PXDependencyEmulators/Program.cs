using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators;

var builder = WebApplication.CreateBuilder(args);
WebApiConfig.Register(builder);


var app = builder.Build();
WebApiConfig.ConfigureRoutes(app);

// Configure the HTTP request pipeline.
// For testing purposes, always enable Swagger in this environment
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PXDependencyEmulators v1");
        c.RoutePrefix = string.Empty; // Make Swagger UI the default page
    });
}

app.UseRouting();
app.MapControllers();

Console.WriteLine("PXDependencyEmulators is starting...");
Console.WriteLine($"Application running on .NET {Environment.Version}");
Console.WriteLine("Navigate to https://localhost:44304 or http://localhost:58974 to view Swagger UI");

app.Run();

