// <copyright file="TraceCore.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public class TraceCore
    {
        public static TException TraceException<TException>(EventTraceActivity traceActivityId, TException exception) where TException : Exception
        {
           return exception;
        }

        public static TException TraceException<TException>(TException exception) where TException : Exception
        {
            return TraceException<TException>(EventTraceActivity.Current, exception);
        }

        public static string XmlSerializeForTracing(object content, EventTraceActivity traceActivityId)
        {
            return XmlMessageTraceHelper.XmlSerializeForTracing(content, traceActivityId);
        }
    }
}
