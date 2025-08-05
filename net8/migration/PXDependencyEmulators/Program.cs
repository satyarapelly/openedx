// <copyright file="Program.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(); // Use Newtonsoft.Json for compatibility

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo {
        Title = "PXDependencyEmulators",
        Version = "v1",
        Description = "Payment X Dependency Emulators (.NET 8.0)",
    });
    c.OperationFilter<AddHeaderParameterOperationFilter>();
});

// Configure Web API services before building the app
WebApiConfig.ConfigureServices(builder.Services);

var app = builder.Build();

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

// Configure Web API app
WebApiConfig.ConfigureApp(app);

app.MapControllers();

Console.WriteLine("PXDependencyEmulators is starting...");
Console.WriteLine($"Application running on .NET {Environment.Version}");
Console.WriteLine("Navigate to https://localhost:44304 or http://localhost:58974 to view Swagger UI");

app.Run();
