// <copyright file="BitConverterExtensions.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public static class BitConverterExtensions
    {
        /// <summary>
        /// Long is 64 bits and Guid is 128bits, convert from Guid to long could loss the data
        /// however this will only been used when the Guid only contain the 64bits data and other bits are zero.
        /// the upper stream will use below function to do the reverse convertion
        /// private static Guid LongToGuid(long value)
        /// {
        ///     byte[] guidData = new byte[16];
        ///     Array.Copy(BitConverter.GetBytes(value), guidData, 8);
        ///     return new Guid(guidData);
        /// }
        /// </summary>
        /// <param name="value">the guid type of value</param>
        /// <param name="traceActivityId">trace activity id</param>
        /// <returns>the long value</returns>
        [SuppressMessage("Microsoft.Naming", "CA1720", Justification = "provide a meanful method name and suppress the rule of Identifiers should not contain type names")]
        public static long ToLong(this Guid value, EventTraceActivity traceActivityId)
        {
            if (BitConverter.ToInt64(value.ToByteArray(), 8) != 0)
            {
                throw TraceCore.TraceException<OverflowException>(traceActivityId, new OverflowException("Value was either too large or too small for an Int64."));
            }

            return BitConverter.ToInt64(value.ToByteArray(), 0);
        }
    }
}
