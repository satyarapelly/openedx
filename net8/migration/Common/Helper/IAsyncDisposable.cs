// <copyright file="IAsyncDisposable.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    using System.Threading.Tasks;

    public interface IAsyncDisposable
    {
        Task DisposeAsync();
    }
}
