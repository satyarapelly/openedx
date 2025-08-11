// <copyright file="Async.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;

    public static class Async
    {
        public static async Task Using(IAsyncDisposable asyncDisposable, Func<Task> function)
        {
            ExceptionDispatchInfo capturedException = null;
            try
            {
                await function();
            }
            catch (Exception e)
            {
                capturedException = ExceptionDispatchInfo.Capture(e);
            }

            await asyncDisposable.DisposeAsync();

            if (capturedException != null)
            {   
                capturedException.Throw();
            }
        }
    }
}
