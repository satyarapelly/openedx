// <copyright file="TokenPolicyEvaluationResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class TokenPolicyEvaluationResult
    {
        /// <summary>
        /// Gets or sets a collection of additional attributes, if any, that may be used to help
        /// describe the result.
        /// </summary>
        [JsonProperty(PropertyName = "additionalAttributes")]
        public IEnumerable<TokenPolicyEvaluationResultProperty> AdditionalAttributes { get; set; }

        /// <summary>
        /// Gets or sets a code that defines the evaluation result for the policy.
        /// </summary>
        /// <remarks>This acts like a result code or error code.  This is safe for clients to parse
        /// and use to drive logic or reporting.  We may add codes in the future.  Clients must gracefully
        /// handle or provide default behavior for new codes without breaking, such as by handling them as an
        /// unknown policy failure.
        /// Existing codes will not be changed without a version change because that would be
        /// considered a breaking change.</remarks>
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets a description string that provides a brief, human-readable description of the result.
        /// </summary>
        /// <remarks>This string is subject to change at any time and is not reliable for clients to parse or use
        /// to drive logic or reporting.  This is an aid for integration and debugging purposes.</remarks>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the policy that was evaluated.
        /// </summary>
        /// <remarks>Existing policy names will not be changed over time.  However, new policy names
        /// may be added at any time without versioning and will be considered a non-breaking change.
        /// Clients must gracefully handle or ignore any new policy names without breaking.</remarks>
        [JsonProperty(PropertyName = "name")]
        public TokenDescriptionPolicyType PolicyName { get; set; }
    }
}