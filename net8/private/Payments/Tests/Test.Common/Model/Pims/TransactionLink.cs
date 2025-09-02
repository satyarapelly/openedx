// <copyright file="PayinCap.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pims
{
    using Newtonsoft.Json;

    public class TransactionLink
    {
        [JsonProperty(PropertyName = "linkedPaymentSessionId")]
        public string LinkedPaymentSessionId { get; set; }
    }
}