// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PXService.Mocks
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
                request.Options.Set(
                    new HttpRequestOptionsKey<List<string>>("PX.ExposedFlightFeatures"),
                    EnabledFlights);
            }

            return next != null
                ? next(request)
                : Task.FromResult<HttpResponseMessage>(null);
        }
    }
}