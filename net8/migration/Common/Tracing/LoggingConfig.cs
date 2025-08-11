// <copyright file="LoggingConfig.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public enum LoggingMode
    {
        Sll,
        OpenTelemetry,
        Default
    }

    public static class LoggingConfig
    {
        public static LoggingMode Mode { get; set; }
    }
}
