// <copyright file="ITracer.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Helpers
{
    using System;

    public interface ITracer
    {
        void Warning(string message, params object[] args);

        void Verbose(string message, params object[] args);

        void Exception(Exception e, string message, params object[] args);

        void Exception(Exception e);

        void CriticalException(Exception e, int errorCode);
    }
}
