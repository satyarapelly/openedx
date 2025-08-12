// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.AccountService.V7
{
    public static class Constants
    {
        public static class ServiceNames
        {
            public const string AccountService = "AccountService";
        }

        public static class UriTemplate
        {
            public const string GetProfilesByAccountId = "/{0}/profiles?type={1}";
            public const string UpdateProfilesById = "/{0}/profiles/{1}";
            public const string GetAddressByAddressId = "/{0}/addresses/{1}";
            public const string GetAddressesByCountry = "/{0}/addresses?country={1}";
        }

        public static class AccountV3ExtendedHttpHeaders
        {
            public const string Etag = "etag";
            public const string IfMatch = "If-Match";
        }
    }
}