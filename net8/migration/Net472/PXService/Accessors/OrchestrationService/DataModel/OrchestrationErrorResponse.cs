// <copyright file="OrchestrationErrorResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService.DataModel
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// Orchestration Service Error contract.
    /// </summary>
    public class OrchestrationErrorResponse
    {
        /// <summary>
        /// Gets or sets Error code specify what error this is.
        /// </summary>
        [JsonProperty(Order = 1, PropertyName = "errorCode")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets User friendly messsage sigifying what went wrong.
        /// </summary>
        [JsonProperty(Order = 2, PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets Targets.
        /// </summary>
        [JsonProperty(Order = 3, PropertyName = "targets")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Allowed to be set by derived classes")]
        public IList<string> Targets { get; set; }
    }
}