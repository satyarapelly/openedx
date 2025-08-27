// <copyright file="IAddressEnrichmentServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.AddressEnrichmentService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accessors.AddressEnrichmentService.DataModel;
    using Microsoft.Commerce.Tracing;

    public interface IAddressEnrichmentServiceAccessor
    {
        Task<List<Tuple<string, string>>> GetCityStateMapping(string country, string zipcode, EventTraceActivity traceActivityId);

        Task<AddressValidateResponse> ValidateAddress(Address address, EventTraceActivity traceActivityId);
    }
}
