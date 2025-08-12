// <copyright file="RequestDeviceBindingResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a response of the operation to bind the client device to a network token.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class RequestDeviceBindingResponse
    {
        /// <summary>
        /// Gets or sets the challenge status.
        /// </summary>
        public ChallengeStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the challenge.
        /// This Id is created during RequestDeviceBinding and passed to VTS.RequestDeviceBinding as ClientReferenceId (the id of the binding).
        /// </summary>
        public string ChallengeId { get; set; }

        /// <summary>
        /// Gets or sets a list of challenge methods if Status == Challenge.
        /// This is null if Status == Approved.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<ChallengeMethod> ChallengeMethods { get; set; }
    }
}
