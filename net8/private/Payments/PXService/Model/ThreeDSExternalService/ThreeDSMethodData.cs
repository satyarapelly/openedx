// <copyright file="ThreeDSMethodData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService
{
    using Newtonsoft.Json;

    public class ThreeDSMethodData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the transaction 
        /// </summary>
        [JsonProperty(PropertyName = "threeDSServerTransID")]
        public string ThreeDSServerTransID { get; set; }

        /// <summary>
        /// Gets or sets the URL that will receive the notification of 3DS Method completion from the ACS. 
        /// This is sent in the initial request to the ACS from the 3DS Requestor executing 
        /// the 3DS Method. 
        /// </summary>
        [JsonProperty(PropertyName = "threeDSMethodNotificationURL")]
        public string ThreeDSMethodNotificationURL { get; set; }
    }
}