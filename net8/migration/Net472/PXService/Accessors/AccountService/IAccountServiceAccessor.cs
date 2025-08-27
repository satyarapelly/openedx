// <copyright file="IAccountServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.Accessors.AccountService.DataModel;
    using Microsoft.Commerce.Tracing;
    using Model.AccountService.AddressValidation;
    using Newtonsoft.Json.Linq;

    public interface IAccountServiceAccessor
    {
        Task<T> PostAddress<T>(string accountId, T address, string apiVersion, EventTraceActivity traceActivityId);

        Task<T> PostAddress<T>(string accountId, T address, string apiVersion, int? syncToLegacyCode, EventTraceActivity traceActivityId);

        Task<T> GetAddress<T>(string accountId, string addressId, string apiVersion, EventTraceActivity traceActivityId);

        Task<T> GetAddressesByCountry<T>(string accountId, string country, string apiVersion, EventTraceActivity traceActivityId);

        Task<AccountProfile> GetProfile(string accountId, string type, EventTraceActivity traceActivityId);

        Task<AccountProfileV3> GetProfileV3(string accountId, string type, EventTraceActivity traceActivityId);

        Task UpdateProfile(string accountId, AccountProfile profile, EventTraceActivity traceActivityId);

        Task UpdateProfileV3(string accountId, AccountProfileV3 profile, string type, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, bool syncLegacyAddress = true, bool useJarvisPatchForConsumerProfile = false);

        Task<AccountLegalProfileV3> GetLegalEntityProfile(string tenantId, EventTraceActivity traceActivityId);

        Task<object> LegacyValidateAddress(object address, EventTraceActivity traceActivityId);

        Task<string> GetEmployeeProfileAccountId(string tenantId, string organizationId, EventTraceActivity traceActivityId);

        Task<T> ModernValidateAddress<T>(object address, EventTraceActivity traceActivityId, bool regionIsoEnabled = false);

        Task<AddressInfoV3> PatchAddress(string accountId, string addressId, AddressInfoV3 address, string eTag, EventTraceActivity traceActivityId);

        Task<LegacyBillableAccount> GetOrCreateLegacyBillableAccount(string accountId, string country, EventTraceActivity traceActivityId);

        Task<CustomerInfo> GetCustomers(string customerId, EventTraceActivity traceActivityId);
    }
}