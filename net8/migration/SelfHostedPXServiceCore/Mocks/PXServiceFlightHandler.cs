// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
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
                // Flights in request.Properties["PX.ExposedFlightFeatures"] will be overwritten by EnabledFlights if EnabledFlights is not empty.
                // If PXEnableIndia3DS1Challenge exists in request.Properties["PX.ExposedFlightFeatures"], we manually add it in EnabledFlights to avoid losing it.
                // Ideally we should add all other flights from in EnabledFlights in the future.
                object exposableFeaturesObject = null;
                request.Properties.TryGetValue("PX.ExposedFlightFeatures", out exposableFeaturesObject);
                List<string> exposableFeatures = exposableFeaturesObject as List<string>;
                if (exposableFeatures.Contains("PXEnableIndia3DS1Challenge"))
                {
                    EnabledFlights.Add("PXEnableIndia3DS1Challenge");
                }

                if (exposableFeatures.Contains("India3dsEnableForBilldesk"))
                {
                    EnabledFlights.Add("India3dsEnableForBilldesk");
                }

                request.Properties["PX.ExposedFlightFeatures"] = EnabledFlights;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
