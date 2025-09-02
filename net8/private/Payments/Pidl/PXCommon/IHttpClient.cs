// <copyright file="IHttpClient.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public interface IHttpClient
    {
        HttpWebResponse SendAndReceive<TReq, TResSuccess, TResError>(
            string httpMethod,
            string relativeUrl,
            EventTraceActivity correlationId,
            ref TReq inputObject,
            ref TResSuccess resSuccess,
            ref TResError resError)
            where TReq : class
            where TResError : class
            where TResSuccess : class;

        HttpWebResponse SendAndReceive<TReq, TResSuccess, TResError>(
            string httpMethod,
            string relativeUrl,
            EventTraceActivity correlationId,
            IList<KeyValuePair<string, string>> optionalHeaders,
            ref TReq inputObject,
            ref TResSuccess resSuccess,
            ref TResError resError)
            where TReq : class
            where TResError : class
            where TResSuccess : class;
    }
}
