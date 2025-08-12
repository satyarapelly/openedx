// <copyright file="IMerchantCapabilitiesAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.MerchantCapabilitiesService.V7
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PimsModel.V4;

    public interface IMerchantCapabilitiesAccessor
    {
        Task<IList<PaymentMethod>> FilterPaymentMethods(string merchantId, string currencyCode, IList<PaymentMethod> paymentMethods, EventTraceActivity traceActivityId);

        Task<MerchantCapabilities> GetMerchantCapabilities(string merchantId, string currencyCode, EventTraceActivity traceActivityId);
    }
}