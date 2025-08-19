// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators;

/// <summary>
/// ASP.NET Core Web API configuration for the dependency emulators.
/// </summary>
public static class WebApiConfig
{
    /// <summary>
    /// Register services required for the dependency emulators.
    /// </summary>
    /// <param name="builder">Application builder.</param>
    /// <param name="useArrangedResponses">Flag to enable arranged responses.</param>
    public static void Register(WebApplicationBuilder builder, bool useArrangedResponses = false)
    {
        // Controllers from the legacy WebApi project are brought in using the
        // compatibility shim so that existing ApiController types continue to work.
        builder.Services.AddControllers()
            .AddNewtonsoftJson()
            .AddWebApiConventions();

        // TODO: Re-implement message handlers and scenario managers if needed
        //       for full feature parity with the original HttpConfiguration based setup.
    }

    /// <summary>
    /// Configure HTTP routes for the emulators.
    /// </summary>
    /// <param name="routes">Route builder.</param>
    public static void ConfigureRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapAccountRoutes();
        routes.MapPartnerSettingsRoutes();
        routes.MapPIMSRoutes();
        routes.MapMSRewardsRoutes();
        routes.MapCatalogRoutes();
        routes.MapIssuerServiceRoutes();
        routes.MapChallengeManagementRoutes();
        routes.MapPaymentThirdPartyRoutes();
        routes.MapPurchaseRoutes();
        routes.MapRiskRoutes();
        routes.MapSellerMarketPlaceRoutes();
        routes.MapTokenPolicyRoutes();
        routes.MapStoredValueRoutes();
        routes.MapTransactionServiceRoutes();
        routes.MapPaymentOchestratorRoutes();
        routes.MapPayerAuthRoutes();
        routes.MapFraudDetectionRoutes();
    }
}

