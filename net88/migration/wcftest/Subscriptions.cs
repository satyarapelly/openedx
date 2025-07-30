// <copyright file="Subscriptions.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PurchaseService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class Subscriptions
    {
        [JsonProperty("items")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<Subscription> Items { get; set; }
    }
}