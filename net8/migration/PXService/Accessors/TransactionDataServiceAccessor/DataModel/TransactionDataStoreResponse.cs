// <copyright file="TransactionDataStoreResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService.DataModel
{
    using Newtonsoft.Json;

    public class TransactionDataStoreResponse
    {
        [JsonProperty(PropertyName = "data_reference_id")]
        public string DataReferenceId { get; set; }
    }
}