// <copyright file="ServiceInstrumentationScopeHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public class ServiceInstrumentationScopeHandler : DelegatingHandler
    {
        private ServiceInstrumentationCounters counters;

        public ServiceInstrumentationScopeHandler(ServiceInstrumentationCounters counters)
        {
            this.counters = counters;
        }

        public ServiceInstrumentationScopeHandler(ServiceInstrumentationCounters counters, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.counters = counters;
        }

        /// <summary>
        /// Get the request api version from the request header.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The api version in string.</returns>
        protected virtual string GetRequestApiVersion(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(PaymentConstants.Web.Properties.Version))
            {
                return request.GetApiVersion().ExternalVersion;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Determines the appropriate perf counter to use for this operation and manages the instrumentation scope.
        /// </summary>
        /// <param name="request">The inbound request.</param>
        /// <param name="cancellationToken">A token which may be used to listen for cancellation.</param>
        /// <returns>The outbound response.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string operationName = request.GetOperationName();
            string apiVersion = this.GetRequestApiVersion(request);
            string instanceName = string.IsNullOrEmpty(apiVersion) ? operationName : operationName + "_" + apiVersion;

            using (ServiceInstrumentationScope scope = new ServiceInstrumentationScope(
                this.counters, 
                request.GetRequestCorrelationId(), 
                null, 
                null,
                instanceName))
            {
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

                if (scope != null)
                {
                    if (response.StatusCode.IsSuccessStatus() 
                        || response.DoesReponseIndicateIdempotentTransaction()
                        || response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
                    {
                        scope.Success();
                    }

                    if (response.StatusCode.IsClientError())
                    {
                        scope.UserError();
                    }
                }

                return response;
            }
        }
    }
}
