// <copyright file="PXServiceInputValidationHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using CommonSchema.Services.Logging;
    using Microsoft.Commerce.Payments.Common.Helper;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using PXCommon;

    public class PXServiceInputValidationHandler : DelegatingHandler
    {
        private static Dictionary<string, List<IParameterValidator>> parameterValidators = new Dictionary<string, List<IParameterValidator>>(StringComparer.OrdinalIgnoreCase)
        {
            { "allowedPaymentMethods",  new List<IParameterValidator>() { new ParameterRegexValidator("^.{1,1500}$"), new ParameterJsonValidator() } },
            { "billableAccountId", new List<IParameterValidator>() { new ParameterRegexValidator("^.{1,100}$") } },
            { "classicProduct", new List<IParameterValidator>() { new ParameterRegexValidator("^[a-z]{1,30}$") } },
            { "complete", new List<IParameterValidator>() { new ParameterRegexValidator("^(true|false)$") } },
            { "completePrerequisites", new List<IParameterValidator>() { new ParameterRegexValidator("^(true|false)$") } },
            { "country", new List<IParameterValidator>() { new ParameterRegexValidator("^[a-z]{2}$") } },
            { "deviceId", new List<IParameterValidator>() { new ParameterRegexValidator(@"^[\d]{16}$") } },
            { "deviceIdFilter", new List<IParameterValidator>() { new ParameterRegexValidator("^(true|false)$") } },
            { "family", new List<IParameterValidator>() { new ParameterRegexValidator(@"^[\w]{1,25}$") } },
            { "filters", new List<IParameterValidator>() { new ParameterRegexValidator("^.{1,500}$"), new ParameterJsonValidator() } },
            { "ignoreMissingTaxId", new List<IParameterValidator>() { new ParameterRegexValidator("^(true|false)$") } },
            { "language", new List<IParameterValidator>() { new ParameterRegexValidator(@"^[-\w,]{1,200}$") } },
            { "operation", new List<IParameterValidator>() { new ParameterRegexValidator("^[a-z]{1,25}$") } },
            { "orderId", new List<IParameterValidator>() { new ParameterRegexValidator(@"^[-\da-z]{1,50}$") } },
            { "partner", new List<IParameterValidator>() { new ParameterRegexValidator(@"^[\w]{1,25}$") } },
            { "paymentSessionData", new List<IParameterValidator>() { new ParameterRegexValidator("^.{1,2048}$"), new ParameterJsonValidator() } },
            { "paymentSessionOrData", new List<IParameterValidator>() { new ParameterRegexValidator("^.{1,2048}$"), new ParameterJsonValidator() } },
            { "piid", new List<IParameterValidator>() { new ParameterRegexValidator(@"^[-+\da-z]{1,50}$") } },
            { "revertChallengeOption", new List<IParameterValidator>() { new ParameterRegexValidator("^(true|false)$") } },
            { "scenario", new List<IParameterValidator>() { new ParameterRegexValidator(@"^[\w]{1,35}$") } },
            { "sessionId", new List<IParameterValidator>() { new ParameterRegexValidator(@"^[-\da-z]{1,60}$") } },
            { "status", new List<IParameterValidator>() { new ParameterRegexValidator("^[a-z]{1,15}$") } },
            { "timezoneOffset", new List<IParameterValidator>() { new ParameterRegexValidator(@"^[-\d]{1,6}$") } },
            { "type", new List<IParameterValidator>() { new ParameterRegexValidator(@"^[-\w,\s]{1,100}$") } }
        };
        
        public static void TraceIntegrationError(HttpRequestMessage request, string message)
        {
            string serviceName = request.GetServiceName();
            IntegrationErrorCode integrationErrorCode = IntegrationErrorCode.InvalidRequestParameterFormat;
            EventTraceActivity requestTraceId = PXTraceCorrelationHandler.GetOrCreateCorrelationIdFromHeader(request);
            CorrelationVector correlationVector = SllCorrelationVectorManager.SetCorrelationVectorAtRequestEntry(request);
            EventTraceActivity serverTraceId = new EventTraceActivity { CorrelationVectorV4 = correlationVector };
            SllWebLogger.TracePXServiceIntegrationError(serviceName, integrationErrorCode, message, requestTraceId.ActivityId.ToString(), serverTraceId.ActivityId.ToString(), correlationVector.ToString());
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                bool isPXEnableThrowInvalidUrlParameterExceptionEanbled = IsFeatureEnabled(request, Flighting.Features.PXEnableThrowInvalidUrlParameterException);
                var queryParams = request.GetQueryNameValuePairs();
                if (queryParams != null)
                {
                    foreach (var queryParam in queryParams)
                    {
                        if (parameterValidators.ContainsKey(queryParam.Key))
                        {
                            var paramValidators = parameterValidators[queryParam.Key];
                            foreach (var paramValidator in paramValidators)
                            {
                                if (!paramValidator.Validate(queryParam.Value))
                                {
                                    string message = string.Format("The parameter {0} value {1} is invalid in the request URL: {2}.", queryParam.Key, queryParam.Value, request.RequestUri.AbsoluteUri);

                                    if (isPXEnableThrowInvalidUrlParameterExceptionEanbled)
                                    {
                                        var traceActivityId = request.GetRequestCorrelationId();
                                        var innerError = new ServiceErrorResponse(ErrorCode.InvalidParameter.ToString(), message);
                                        var error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                                        var response = await request.CreateJsonResponseAsync(HttpStatusCode.BadRequest, error);

                                        // Return the response with 400 Bad Request
                                        return response;
                                    }
                                    else
                                    {
                                        request.SetProperty("InputValidationFailed", true);
                                        TraceIntegrationError(request, message);
                                    }
                                }
                            }
                        }
                        else
                        {
                            request.SetProperty("InputValidationFailed", true);
                            string message = string.Format("The parameter {0} or value {1} is null or invalid in the request URL: {2}.", queryParam.Key, queryParam.Value, request.RequestUri.AbsoluteUri);
                            TraceIntegrationError(request, message);
                        }
                    }
                }
            }
            catch
            {
                string message = string.Format("An error has occurred, exception was thrown, with this URL: {0}", request.RequestUri.AbsoluteUri);
                TraceIntegrationError(request, message);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private static bool IsFeatureEnabled(HttpRequestMessage request, string featureName)
        {
            List<string>? exposedFeatureFlight = new List<string>();
            return request.TryGetProperty(GlobalConstants.RequestPropertyKeys.ExposedFlightFeatures, out exposedFeatureFlight)
                && exposedFeatureFlight != null && exposedFeatureFlight.Contains(featureName);
        }
    }
}