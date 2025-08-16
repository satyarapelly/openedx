// <copyright file="PXServiceInputValidationHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using CommonSchema.Services.Logging;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.Commerce.Payments.Common.Helper;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using PXCommon;

    /// <summary>
    /// Middleware that validates query string parameters for PXService requests.
    /// </summary>
    public class PXServiceInputValidationHandler
    {
        private readonly RequestDelegate next;

        private static readonly Dictionary<string, List<IParameterValidator>> parameterValidators = new(StringComparer.OrdinalIgnoreCase)
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

        public PXServiceInputValidationHandler(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            try
            {
                bool throwOnInvalid = IsFeatureEnabled(request, Flighting.Features.PXEnableThrowInvalidUrlParameterException);
                foreach (var queryParam in request.Query)
                {
                    if (parameterValidators.TryGetValue(queryParam.Key, out var validators))
                    {
                        foreach (var validator in validators)
                        {
                            if (!validator.Validate(queryParam.Value))
                            {
                                string message = string.Format("The parameter {0} value {1} is invalid in the request URL: {2}.", queryParam.Key, queryParam.Value, request.GetDisplayUrl());

                                if (throwOnInvalid)
                                {
                                    var traceActivityId = request.GetRequestCorrelationId();
                                    var innerError = new ServiceErrorResponse(ErrorCode.InvalidParameter.ToString(), message);
                                    var error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    await context.Response.WriteAsJsonAsync(error);
                                    return;
                                }
                                else
                                {
                                    context.Items["InputValidationFailed"] = true;
                                    TraceIntegrationError(request, message);
                                }
                            }
                        }
                    }
                    else
                    {
                        context.Items["InputValidationFailed"] = true;
                        string message = string.Format("The parameter {0} or value {1} is null or invalid in the request URL: {2}.", queryParam.Key, queryParam.Value, request.GetDisplayUrl());
                        TraceIntegrationError(request, message);
                    }
                }
            }
            catch
            {
                string message = string.Format("An error has occurred, exception was thrown, with this URL: {0}", request.GetDisplayUrl());
                TraceIntegrationError(request, message);
            }

            await this.next(context);
        }

        public static void TraceIntegrationError(HttpRequest request, string message)
        {
            string serviceName = request.GetServiceName();
            IntegrationErrorCode integrationErrorCode = IntegrationErrorCode.InvalidRequestParameterFormat;
            EventTraceActivity requestTraceId = request.GetRequestCorrelationId();
            CorrelationVector correlationVector = request.GetCorrelationVector();
            EventTraceActivity serverTraceId = new EventTraceActivity { CorrelationVectorV4 = correlationVector };
            SllWebLogger.TracePXServiceIntegrationError(serviceName, integrationErrorCode, message, requestTraceId.ActivityId.ToString(), serverTraceId.ActivityId.ToString(), correlationVector.ToString());
        }

        private static bool IsFeatureEnabled(HttpRequest request, string featureName)
        {
            if (request.HttpContext.Items.TryGetValue(GlobalConstants.RequestPropertyKeys.ExposedFlightFeatures, out var exposedFeatureFlight) && exposedFeatureFlight is List<string> features)
            {
                return features.Contains(featureName);
            }

            return false;
        }
    }
}

