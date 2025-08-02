// <copyright file="LogTracer.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Helpers
{
    using System;

    public class LogTracer : ITracer
    {
        public void Exception(Exception e)
        {
            // throw new NotImplementedException();
        }

        public void Exception(Exception e, string message, params object[] args)
        {
            // throw new NotImplementedException();
        }

        public void Warning(string message, params object[] args)
        {
            // throw new NotImplementedException();
        }

        public void Verbose(string message, params object[] args)
        {
            // throw new NotImplementedException();
        }

        public void CriticalException(Exception e, int errorCode)
        {
            throw new NotImplementedException();
        }
    }
}
