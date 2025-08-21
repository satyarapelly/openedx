// <copyright file="PasskeyMandateRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a request to mandate a passkey.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class PasskeyMandateRequest
    {
        /// <summary>
        /// Gets or sets the app instance.
        /// </summary>
        public object AppInstance { get; set; }

        /// <summary>
        /// Gets or sets the assurance data.
        /// </summary>
        public AssuranceData AssuranceData { get; set; }

        /// <summary>
        /// Gets or sets the list of mandates.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<Mandate> Mandates { get; set; } = new List<Mandate>();

        /// <summary>
        /// Gets or sets the DFP session identifier.
        /// </summary>
        public string DfpSessionId { get; set; }
    }
}
