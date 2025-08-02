using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Yarp.ReverseProxy;
using System.Web.Adapters;

var builder = WebApplication.CreateBuilder(args);

// Ensure required configuration exists
string? proxyTarget = builder.Configuration["ProxyTo"];
if (string.IsNullOrEmpty(proxyTarget))
{
    throw new InvalidOperationException("Missing required configuration: 'ProxyTo'");
}

// Add SystemWebAdapters support
builder.Services.AddSystemWebAdapters();

// Add YARP (reverse proxy)
builder.Services.AddReverseProxy();
builder.Services.AddHttpForwarder();

// Add MVC support
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Use HSTS only in non-dev environments
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// SystemWebAdapters middleware (optional - only needed for Web Forms migration)
app.UseSystemWebAdapters();

// Default MVC route
app.MapDefaultControllerRoute();

// Reverse proxy route
app.MapForwarder("/{**catch-all}", proxyTarget)
   .Add(builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);

app.Run();
