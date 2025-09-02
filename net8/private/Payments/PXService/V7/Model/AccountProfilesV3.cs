// <copyright file="AccountProfilesV3.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class AccountProfilesV3<T>
    {
        [JsonProperty(PropertyName = "items")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Needed for serialization purpose.")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        public List<T> UserProfiles { get; set; }
    }
}