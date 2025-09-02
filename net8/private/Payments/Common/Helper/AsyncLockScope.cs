// <copyright file="AsyncLockScope.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    using System;
    using System.Threading.Tasks;

    public sealed class AsyncLockScope : IDisposable
    {
        private AsyncLock asyncLock;

        internal AsyncLockScope(AsyncLock asyncLock)
        {
            this.asyncLock = asyncLock;
        }
                
        public void Dispose()
        {
            this.asyncLock.Release();
        }

        internal Task WaitLock()
        {
            return this.asyncLock.WaitLock();
        }
    }
}
