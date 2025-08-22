// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Commerce.Payments.PXService;

    /// <summary>
    /// Maintains a collection of flight names that can be applied to inbound requests.
    /// </summary>
    public class PXServiceFlightHandler : IMiddleware
    {
        private const string FlightContextKey = GlobalConstants.RequestPropertyKeys.ExposedFlightFeatures;

        /// <summary>
        /// Gets the list of flights that should be exposed on each request.
        /// </summary>
        public List<string> EnabledFlights { get; }

        public PXServiceFlightHandler()
        {
            EnabledFlights = new List<string>();
        }

        /// <summary>
        /// Clears all configured flights.
        /// </summary>
        public void ResetToDefault()
        {
            EnabledFlights.Clear();
        }

        /// <summary>
        /// Adds the comma separated flight names to the list of enabled flights.
        /// </summary>
        public void AddToEnabledFlights(string flightsToAdd)
        {
            EnabledFlights.AddRange(
                flightsToAdd.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(f => f.Trim()));
        }

        /// <summary>
        /// Middleware entry point used by ASP.NET Core to stamp enabled flights
        /// onto the current <see cref="HttpContext"/>.
        /// </summary>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (EnabledFlights.Count > 0)
            {
                if (context.Items.TryGetValue(FlightContextKey, out var existing) &&
                    existing is List<string> exposableFeatures)
                {
                    if (exposableFeatures.Contains("PXEnableIndia3DS1Challenge"))
                    {
                        EnabledFlights.Add("PXEnableIndia3DS1Challenge");
                    }

                    if (exposableFeatures.Contains("India3dsEnableForBilldesk"))
                    {
                        EnabledFlights.Add("India3dsEnableForBilldesk");
                    }
                }

                context.Items[FlightContextKey] = EnabledFlights;
            }

            if (next != null)
            {
                await next(context);
            }
        }
    }
}

