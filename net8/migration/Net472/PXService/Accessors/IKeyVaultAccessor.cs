// <copyright file="IKeyVaultAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Threading.Tasks;

    public interface IKeyVaultAccessor
    {
        Task<string> GetSecretAsync(string name);
    }
}