// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.AddressEnrichmentService.V7
{
    using System;
    using System.Collections.Generic;
    using static Microsoft.Commerce.Payments.PXService.GlobalConstants;

    public static class Constants
    {
        internal static IEnumerable<string> CountriesRequiredRegionIsoEnabledFlag
        {
            get
            {
                return new List<string>
                {
                    CountryCodes.AE,
                    CountryCodes.CN,
                    CountryCodes.TR,
                    CountryCodes.VE
                };
            }
        }

        /// <summary>
        /// Gets Region mapping between AVS and PIDL with both unique Key and Value pairs
        /// </summary>
        internal static Dictionary<string, string> RegionMappingFromPIDLToAVS
        {
            get
            {
                return new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
                {
                    { "National Capital Territory of Delhi", "Delhi" }
                };
            }
        }

        internal static class ExtendedHttpHeaders
        {
            internal const string RegionIsoEnabled = "regionIsoEnabled";
        }
    }
}