// <copyright file="ParameterAssert.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common
{
    using System;
    using Microsoft.Commerce.Payments.Common.Tracing;

    /// <summary>
    /// Helper class for paramerer validation
    /// </summary>
    public class ParameterAssert
    {
        public static void Valid(bool expression, string parameterName, string message, EventTraceActivity traceActivityId)
        {
            if (!expression)
            {
                throw TraceCore.TraceException(traceActivityId ?? new EventTraceActivity(), new ArgumentException(message ?? "Invalid parameter", parameterName));
            }
        }
    }
}
