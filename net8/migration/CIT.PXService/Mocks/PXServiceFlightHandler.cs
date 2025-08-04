// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PXService.Mocks
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// This is a delegating handler that allows specific flights to be added to PX.ExposedFlightFeatures property of the request.
    /// </summary>
    public class PXServiceFlightHandler : DelegatingHandler
    {
        public List<string> EnabledFlights { get; set; }

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
            EnabledFlights.AddRange(flightsToAdd.Split(new char[] { ',' }).Select(f => f.Trim()));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (EnabledFlights != null && EnabledFlights.Count > 0)
            {
                request.Properties["PX.ExposedFlightFeatures"] = EnabledFlights;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
