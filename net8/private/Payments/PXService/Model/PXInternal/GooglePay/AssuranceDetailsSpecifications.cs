// <copyright file="AssuranceDetailsSpecifications.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to extract google pay payment data
    /// </summary>
    public class AssuranceDetailsSpecifications
    {
        public AssuranceDetailsSpecifications()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether - If true, indicates that Cardholder possession validation has been performed on returned payment credential.
        /// </summary>
        [JsonProperty(PropertyName = "accountVerified")]
        public bool AccountVerified { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether - If true, indicates that identification and verifications (ID and V) was performed on the returned payment credential.
        /// If false, the same risk-based authentication can be performed as you would for card transactions. This risk-based authentication can include, but not limited to, step-up with 3D Secure protocol if applicable.
        /// </summary>
        [JsonProperty(PropertyName = "cardHolderAuthenticated")]
        public bool CardHolderAuthenticated { get; set; }
    }
}