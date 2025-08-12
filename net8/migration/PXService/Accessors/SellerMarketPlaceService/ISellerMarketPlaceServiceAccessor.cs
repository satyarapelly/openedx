// <copyright file="ISellerMarketPlaceServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft 2022 All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.SellerMarketPlaceService
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PXService.Model.SellerMarketPlaceService;

    public interface ISellerMarketPlaceServiceAccessor
    {
        Task<Seller> GetSeller(string partner, string paymentProviderId, string sellerId, EventTraceActivity traceActivityId);
    }
}