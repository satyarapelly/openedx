// <copyright file="OfflineHandler.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Helper;

    public class OfflineHandler : DelegatingHandler
    {
        private IOfflineStatusChecker offlineChecker;

        public OfflineHandler(IOfflineStatusChecker offlineChecker)
        {
            this.offlineChecker = offlineChecker;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            if (this.offlineChecker != null && this.offlineChecker.GetOfflineState())
            {
                response.Headers.ConnectionClose = true;
            }

            return response;
        }
    }
}