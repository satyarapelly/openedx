// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Maintains a collection of flight names that can be applied to outbound requests.
    /// </summary>
    public class PXServiceFlightHandler
    {
        public List<string> EnabledFlights { get; }

        public PXServiceFlightHandler()
        {
            EnabledFlights = new List<string>();
        }

        public void ResetToDefault()
        {
            EnabledFlights.Clear();
        }

        public void AddToEnabledFlights(string flightsToAdd)
        {
            EnabledFlights.AddRange(
                flightsToAdd.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(f => f.Trim()));
        }

        public Task<HttpResponseMessage> InvokeAsync(
            HttpRequestMessage request,
            Func<HttpRequestMessage, Task<HttpResponseMessage>> next)
        {
            if (EnabledFlights.Count > 0)
            {
                var optionsKey = new HttpRequestOptionsKey<List<string>>("PX.ExposedFlightFeatures");
                if (request.Options.TryGetValue(optionsKey, out var exposableFeatures))
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

                request.Options.Set(optionsKey, EnabledFlights);
            }

            return next != null
                ? next(request)
                : Task.FromResult<HttpResponseMessage>(null);
        }
    }
}
