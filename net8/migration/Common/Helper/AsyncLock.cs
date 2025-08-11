// <copyright file="AsyncLock.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    using System.Threading;
    using System.Threading.Tasks;

    public class AsyncLock
    {
        private readonly SemaphoreSlim semaphore;

        public AsyncLock()
        {
            this.semaphore = new SemaphoreSlim(1);
        }

        public async Task<AsyncLockScope> CreateLockScope()
        {
            AsyncLockScope lockScope = new AsyncLockScope(this);
            await lockScope.WaitLock();
            return lockScope;
        }

        internal Task WaitLock()
        {
            return this.semaphore.WaitAsync();
        }

        internal void Release()
        {
            this.semaphore.Release();
        }
    }
}
