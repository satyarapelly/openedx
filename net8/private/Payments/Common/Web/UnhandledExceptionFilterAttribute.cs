// <copyright file="UnhandledExceptionFilterAttribute.cs" company="Microsoft">
// Copyright (c) Microsoft 2024. All rights reserved.</copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Commerce.Payments.Common.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;
using System;
using System.Net;

namespace Microsoft.Commerce.Payments.Common.Web
{
    /// <summary>
    /// ASP.NET Core exception filter to handle and log unhandled exceptions globally.
    /// </summary>
    public sealed class UnhandledExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var correlationId = context.HttpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var values)
                ? values.ToString()
                : Guid.NewGuid().ToString();

            if (context.Exception is FailedOperationException failedOp)
            {
                context.Result = new ObjectResult(new ErrorResponse
                {
                    ErrorCode = PaymentConstants.ErrorTypes.FailedOperation,
                    Message = failedOp.Message
                })
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
            else
            {
                context.Result = new ObjectResult(new ErrorResponse
                {
                    ErrorCode = PaymentConstants.ErrorTypes.UnknownFailure,
                    Message = context.Exception.Message
                })
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
            context.ExceptionHandled = true;
        }
    }
}
