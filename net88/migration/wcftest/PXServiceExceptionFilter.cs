// <copyright file="PXServiceExceptionFilter.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using HttpRequest = System.Net.Http.HttpRequestMessage;
    using HttpResponse = System.Net.Http.HttpResponseMessage;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Filters;
    using MerchantCapabilitiesService.V7;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PidlFactory;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;
    using static Microsoft.Commerce.Payments.Common.PaymentConstants.Web;

    public class PXServiceExceptionFilter : IExceptionFilter
    {
        public bool AllowMultiple
        {
            get
            {
                return false;
            }
        }

        public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            HttpRequest request = actionExecutedContext.Request;
            Exception exception = actionExecutedContext.Exception;

            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            ServiceErrorResponse errorResponse;
            ValidationException validationException = null;
            IntegrationException integrationException = null;
            MerchantCapabilitiesErrorException merchantCapabilitiesErrorException = null;
            PimsSessionException pimsSessionException = null;
            ExpressCheckoutException expressCheckoutSessionException = null;
            PidlFactory.IntegrationException pidlIntegrationException = null;

            if ((validationException = exception as ValidationException) != null)
            {
                errorResponse = new ServiceErrorResponse(validationException.ErrorCode.ToString(), validationException.Message);
                errorResponse.Target = validationException.Target;
            }
            else if (exception is JsonReaderException)
            {
                errorResponse = new ServiceErrorResponse(ErrorConstants.ErrorCodes.InvalidRequestData, exception.Message);
            }
            else if (exception is InvalidOperationException || exception is NotSupportedException)
            {
                errorResponse = new ServiceErrorResponse(exception.GetType().Name, exception.Message);
            }
            else if (exception is PIDLArgumentException)
            {
                errorResponse = new ServiceErrorResponse(exception.Data[GlobalConstants.ExceptionDataKeys.PIDLErrorCode].ToString(), exception.Message);
            }
            else if (exception is SqlCommandFailedException)
            {
                statusCode = HttpStatusCode.InternalServerError;
                errorResponse = new ServiceErrorResponse(exception.HResult.ToString(), exception.ToString());
            }
            else if (exception.InnerException is PIDLArgumentException)
            {
                errorResponse = new ServiceErrorResponse(exception.InnerException.Data[GlobalConstants.ExceptionDataKeys.PIDLErrorCode].ToString(), exception.InnerException.Message);
            }
            else if (exception.InnerException is PIDLException)
            {
                errorResponse = new ServiceErrorResponse(exception.InnerException.Data[GlobalConstants.ExceptionDataKeys.PIDLErrorCode].ToString(), exception.InnerException.Message);
            }
            else if (exception is PIDLException && exception.Data.Values.OfType<string>().Contains(GlobalConstants.ErrorCodes.PIDLInvalidFilters, StringComparer.Ordinal))
            {
                // This error occurs due to invalid query parameters. Therefore, we want to return a BadRequest status and throw the exception instead of an InternalServerError.
                errorResponse = new ServiceErrorResponse(exception.Data[GlobalConstants.ExceptionDataKeys.PIDLErrorCode].ToString(), exception.Message);
            }
            else if (exception is PIDLConfigException)
            {
                statusCode = HttpStatusCode.InternalServerError;
                errorResponse = new ServiceErrorResponse(exception.Data[GlobalConstants.ExceptionDataKeys.PIDLErrorCode].ToString(), exception.Message);
            }
            else if (exception.InnerException is PIDLConfigException)
            {
                statusCode = HttpStatusCode.InternalServerError;
                errorResponse = new ServiceErrorResponse(exception.InnerException.Data[GlobalConstants.ExceptionDataKeys.PIDLErrorCode].ToString(), exception.InnerException.Message);
            }
            else if ((integrationException = exception as PXService.IntegrationException) != null)
            {
                statusCode = HttpStatusCode.InternalServerError;
                errorResponse = new ServiceErrorResponse(integrationException.ErrorCode, integrationException.ToString());
            }
            else if ((pidlIntegrationException = exception as PidlFactory.IntegrationException) != null)
            {
                statusCode = HttpStatusCode.InternalServerError;
                errorResponse = new ServiceErrorResponse(pidlIntegrationException.Data[GlobalConstants.ExceptionDataKeys.PIDLErrorCode].ToString(), pidlIntegrationException.ToString());
            }
            else if ((merchantCapabilitiesErrorException = exception as MerchantCapabilitiesErrorException) != null)
            {
                statusCode = merchantCapabilitiesErrorException.Response != null ? merchantCapabilitiesErrorException.Response.StatusCode : merchantCapabilitiesErrorException.Error.HttpStatusCode;
                errorResponse = merchantCapabilitiesErrorException.Error;
            }
            else if ((pimsSessionException = exception as PimsSessionException) != null)
            {
                errorResponse = new ServiceErrorResponse(pimsSessionException.ErrorCode, pimsSessionException.Message);
            }
            else if ((expressCheckoutSessionException = exception as ExpressCheckoutException) != null)
            {
                errorResponse = new ServiceErrorResponse(expressCheckoutSessionException.ErrorCode, expressCheckoutSessionException.Message);
                statusCode = HttpStatusCode.InternalServerError;
            }
            else if (exception is TypeInitializationException)
            {
                //// PX throwed TypeInitializationException, when Azure app service has monthly storage upgrade.
                //// During upgrade, app service restarted our apps, restart fails to initialize PidlFactory due to not loading part of .csv files during
                //// [Incident 244674370](https://portal.microsofticm.com/imp/v3/incidents/details/244674370/home) 
                //// more details can be found in autoheal.md
                statusCode = HttpStatusCode.ServiceUnavailable;
                errorResponse = new ServiceErrorResponse(ErrorConstants.ErrorCodes.ServiceUnavailable, exception.Message);
            }
            else
            {
                ServiceErrorResponseException serviceErrorException = exception as ServiceErrorResponseException;
                if (serviceErrorException != null
                    && serviceErrorException.HandlingType == ExceptionHandlingPolicy.ByPass)
                {
                    //// PX returns 503 ONLY when PX service itself is unavailable and a restart is needed
                    //// If dependency  service is service unavailable, 
                    //// We return BadGateway to client
                    //// A 502 error means that a website server that is serving as a reverse proxy for the website origin server 
                    //// did not receive a valid response from the origin server.
                    statusCode = serviceErrorException.Response.StatusCode == HttpStatusCode.ServiceUnavailable
                            ? HttpStatusCode.BadGateway
                            : serviceErrorException.Response.StatusCode;
                    errorResponse = serviceErrorException.Error;
                }
                else
                {
                    statusCode = HttpStatusCode.InternalServerError;
                    errorResponse = new ServiceErrorResponse(ErrorConstants.ErrorCodes.InternalError, exception.Message);
                }
            }

            StringBuilder certInfoStringBuilder = new StringBuilder();
            foreach (DependenciesCertInfo dependencyNameUsingCert in Enum.GetValues(typeof(DependenciesCertInfo)))
            {
                certInfoStringBuilder.Append(HttpRequestHelper.GetRequestContextItem(dependencyNameUsingCert.ToString()));
            }

            string exceptionMessage = exception.ToString();
            if (!string.IsNullOrEmpty(certInfoStringBuilder.ToString()))
            {
                exceptionMessage = string.Format("CertInfo: {0}, exception message: {1}", certInfoStringBuilder.ToString(), exceptionMessage);
            }

            SllWebLogger.TracePXServiceException(exceptionMessage, request.GetRequestCorrelationId());
            errorResponse.CorrelationId = request.GetRequestCorrelationId().ActivityId.ToString();
            actionExecutedContext.Response = request.CreateResponse(statusCode, errorResponse);
            request.AddErrorCodeProperty(errorResponse.ErrorCode);
            request.AddErrorMessageProperty(errorResponse.Message);

            return Task.FromResult(0);
        }
    }
}