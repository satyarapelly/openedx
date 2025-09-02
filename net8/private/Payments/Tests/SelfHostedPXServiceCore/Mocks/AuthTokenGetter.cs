// <copyright file="AuthTokenGetter.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Azure.Core;
    using Microsoft.Commerce.Payments.Authentication.AzureActiveDirectoryCaller;

    public class AuthTokenGetter : IAzureActiveDirectoryTokenLoader
    {
        public Task<AccessToken> GetTokenAsync(string traceId, CancellationToken cancellationToken)
        {
            return Task.Run(() => new AccessToken("DummyToken", DateTime.UtcNow));
        }

        public Task<string> GetTokenStringAsync(string traceId, CancellationToken cancellationToken)
        {
            return Task.Run(() => "DummyToken");
        }

        public Task<bool> RefreshTokenAsync(string traceId, CancellationToken cancellationToken)
        {
            return Task.Run(() => true);
        }
    }
}
