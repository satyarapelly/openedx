// <copyright file="PXServiceCorsHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Settings;

    /// <summary>
    /// Delegating handler which validates that any CORS request is coming from an allowed origin
    /// </summary>
    public class PXServiceCorsHandler : DelegatingHandler 
    {
        private static List<string> corsAllowedOrigins;

        public PXServiceCorsHandler(PXServiceSettings settings)
        {
            corsAllowedOrigins = settings.CorsAllowedOrigins ?? new List<string>();
        }

        /// <summary>
        /// Handles Cors Origin validation for multiple Origins
        /// </summary>
        /// <param name="request">The inbound request.</param>
        /// <param name="cancellationToken">A token which may be used to listen for cancellation.</param>
        /// <returns>The outbound response.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string origin = request.GetRequestHeader("Origin");
            bool isCorsRequest = !string.IsNullOrEmpty(origin);

            if (!isCorsRequest)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            string allowedOrigin = corsAllowedOrigins.Find(x => x.ToLower() == origin.ToLower());

            if (string.IsNullOrEmpty(allowedOrigin))
            {
                SllWebLogger.TracePXServiceException($"CORS request from domain not in allowed list: {origin}", request.GetRequestCorrelationId());

                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }

            // The controllers don't handle OPTIONS requests, and once the origin has been verified there is no need to do anything else but send a response
            // There is the possibliity of a simpler GET request coming through, and that will run the normal route with base.SendAsync()
            HttpResponseMessage response = request.Method == HttpMethod.Options
               ? new HttpResponseMessage(HttpStatusCode.OK)
               : await base.SendAsync(request, cancellationToken);

            response.Headers.Add("Access-Control-Allow-Origin", origin);

            return response;
        }
    }
}