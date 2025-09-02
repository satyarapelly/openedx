// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.TaxIdService.V7
{
    public static class Constants
    {
        internal static class ServiceNames
        {
            internal const string TaxIdService = "TaxIdService";
        }

        internal static class UriTemplate
        {
            internal const string GetTaxIds = "/{0}/tax-ids";
            internal const string GetTaxIdsWithTypeCountryAndStatus = "/{0}/tax-ids?profileType={1}&status=valid&country={2}";
        }
    }
}