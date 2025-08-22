// <copyright file="PXTracingHttpClient.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Net.Http;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Tracing;

    /// <summary>
    /// This client class wraps a typical HttpClient into a tracing handler
    /// </summary>
    public class PXTracingHttpClient : HttpClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PXTracingHttpClient" /> class.
        /// </summary>
        /// <param name="serviceName">The name of the service using this Tracing Client. This name is used for identification of the trace logs.</param>
        /// <param name="logError">A tracing action that will be executed if there is an error when sending or receiving a request.</param>
        /// <param name="logRequest">A tracing action that will be executed to log the request that's being sent. Default is no action. Do not use if the request could contain sensitive data.</param>
        /// <param name="logResponse">A tracing action that will be executed to log the response that's received. Default is no action. Do not use if the response could contain sensitive data.</param>
        public PXTracingHttpClient(
            string serviceName,
            Action<string, EventTraceActivity> logError = null,
            Action<string, string, EventTraceActivity> logRequest = null,
            Action<string, EventTraceActivity> logResponse = null)
            : base(new PXTraceCorrelationHandler(
                serviceName: serviceName, 
                innerHandler: new PXTracingHandler(
                    serviceName: serviceName, 
                    httpMessageHandler: new HttpClientHandler(), 
                    logError: logError, 
                    logRequest: logRequest, 
                    logResponse: logResponse), 
                isDependentServiceRequest: true))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PXTracingHttpClient" /> class.
        /// </summary>
        /// <param name="serviceName">The name of the service using this Tracing Client. This name is used for identification of the trace logs.</param>
        /// <param name="httpMessageHandler">An inner handler that is attached to this HttpClient object.</param>
        /// <param name="logError">An tracing action that will be executed if there is an error when sending or receiving a request.</param>
        /// <param name="logRequest">A tracing action that will be executed to log the request that's being sent. Default is no action. Do not use if the request could contain sensitive data.</param>
        /// <param name="logResponse">A tracing action that will be executed to log the response that's received. Default is no action. Do not use if the response could contain sensitive data.</param>
        public PXTracingHttpClient(
            string serviceName,
            HttpMessageHandler httpMessageHandler,
            Action<string, EventTraceActivity> logError = null,
            Action<string, string, EventTraceActivity> logRequest = null,
            Action<string, EventTraceActivity> logResponse = null) :
            base(new PXTraceCorrelationHandler(
                serviceName: serviceName, 
                innerHandler: new PXTracingHandler(
                    serviceName: serviceName, 
                    httpMessageHandler: httpMessageHandler, 
                    logError: logError, 
                    logRequest: logRequest, 
                    logResponse: logResponse), 
                isDependentServiceRequest: true))
        {
        }

        public PXTracingHttpClient(
            string serviceName,
            HttpMessageHandler httpMessageHandler,
            Action<string, string, HttpRequestMessage, HttpResponseMessage, string, string, string, string> logOutgoingRequestToApplicationInsight) :
            base(new PXTraceCorrelationHandler(
                serviceName: serviceName,
                innerHandler: httpMessageHandler,
                isDependentServiceRequest: true,
                logOutgoingToAppInsight: logOutgoingRequestToApplicationInsight))
        {
        }

        public PXTracingHttpClient(
            string serviceName,
            Action<string, string, HttpRequestMessage, HttpResponseMessage, string, string, string, string> logOutgoingRequestToApplicationInsight) :
            base(new PXTraceCorrelationHandler(
                serviceName: serviceName,
                innerHandler: new HttpClientHandler(),
                isDependentServiceRequest: true,
                logOutgoingToAppInsight: logOutgoingRequestToApplicationInsight))
        {
        }
    }
}
