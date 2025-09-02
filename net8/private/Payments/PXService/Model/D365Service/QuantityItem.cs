// <copyright file="QuantityItem.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using System;
    using Newtonsoft.Json;

    public class QuantityItem
    {
        [JsonProperty("estimatedDeliveryDate", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? EstimatedDeliveryDate { get; set; }

        [JsonProperty("isCancellable", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool IsCancellable { get; set; }

        [JsonProperty("warrantySalesLineId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string WarrantySalesLineId { get; set; }

        [JsonProperty("returnedDate", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ReturnedDate { get; set; }

        [JsonProperty("returnedReason", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string ReturnedReason { get; set; }

        [JsonProperty("canceledDate", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CanceledDate { get; set; }

        [JsonProperty("canceledReason", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string CanceledReason { get; set; }

        [JsonProperty("tokenIdentifier", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string TokenIdentifier { get; set; }

        [JsonProperty("rmaId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string RmaId { get; set; }

        [JsonProperty("fulfillmentState", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string FulfillmentState { get; set; }

        [JsonProperty("billingState", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string BillingState { get; set; }

        [JsonProperty("mcapiFulfillmentId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string McapiFulfillmentId { get; set; }

        [JsonProperty("omniChannelFulfillmentLineId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string OmniChannelFulfillmentLineId { get; set; }

        [JsonProperty("quantityItemState", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string QuantityItemState { get; set; }

        [JsonProperty("quantityItemId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string QuantityItemId { get; set; }

        [JsonProperty("renderEDD", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool RenderEDD { get; set; }

        [JsonProperty("renderEDDOverride", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool RenderEDDOverride { get; set; }
    }
}