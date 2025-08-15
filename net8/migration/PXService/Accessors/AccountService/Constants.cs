// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.AccountService.V7
{
    public static class Constants
    {
        internal static class ServiceNames
        {
            internal const string AccountService = "AccountService";
        }

        internal static class UriTemplate
        {
            internal const string GetProfilesByAccountId = "/{0}/profiles?type={1}";
            internal const string UpdateProfilesById = "/{0}/profiles/{1}";
            internal const string GetAddressByAddressId = "/{0}/addresses/{1}";
            internal const string GetAddressesByCountry = "/{0}/addresses?country={1}";
        }

        internal static class AccountV3ExtendedHttpHeaders
        {
            public const string Etag = "etag";
            public const string IfMatch = "If-Match";
        }
    }
}