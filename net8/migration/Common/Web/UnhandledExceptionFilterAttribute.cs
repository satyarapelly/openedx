// <copyright file="UnhandledExceptionFilterAttribute.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Filters;
    using Microsoft.Commerce.Payments.Common.Tracing;
    
    public sealed class UnhandledExceptionFilterAttribute : ExceptionFilterAttribute 
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception is FailedOperationException)
            {
                FailedOperationException failedOperationException = (FailedOperationException)actionExecutedContext.Exception;

                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                                                  HttpStatusCode.InternalServerError,
                                                    new ErrorResponse
                                                    {
                                                        ErrorCode = PaymentConstants.ErrorTypes.FailedOperation,
                                                        Message = failedOperationException.Message
                                                    });
            }
            else
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                                                  HttpStatusCode.InternalServerError,
                                                    new ErrorResponse
                                                    {
                                                        ErrorCode = PaymentConstants.ErrorTypes.UnknownFailure,
                                                        Message = actionExecutedContext.Exception.Message
                                                    });
            }
        }
    }
}
