// <copyright file="KeyVaultAccessor.cs" company="Microsoft">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService;

    public class KeyVaultAccessor : IKeyVaultAccessor
    {
        public Task<string> GetSecretAsync(string name)
        {
            return Task.Run(() => "DummySecret");
        }
    }
}