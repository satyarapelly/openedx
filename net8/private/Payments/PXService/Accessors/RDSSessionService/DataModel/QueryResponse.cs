// <copyright file="QueryResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.RDSSessionService.DataModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// RDS Session Query Response.
    /// </summary>
    public class QueryResponse
    {
        /// <summary>
        /// Gets or sets session state.
        /// </summary>
        [JsonProperty(PropertyName = "session_state")]
        public string SessionState { get; set; }
    }
}