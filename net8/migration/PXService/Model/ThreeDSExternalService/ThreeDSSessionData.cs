// <copyright file="ThreeDSSessionData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService
{
    using Newtonsoft.Json;

    public class ThreeDSSessionData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the transaction 
        /// </summary>
        [JsonProperty(PropertyName = "threeDSServerTransID")]
        public string ThreeDSServerTransID { get; set; }

        /// <summary>
        /// Gets or sets universally Unique transaction identifier assigned by the ACS to identify a single transaction.  
        /// </summary>
        [JsonProperty(PropertyName = "acsTransID")]
        public string AcsTransID { get; set; }
    }
}