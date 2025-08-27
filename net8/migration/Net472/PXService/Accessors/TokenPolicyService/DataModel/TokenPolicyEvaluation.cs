// <copyright file="TokenPolicyEvaluation.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class TokenPolicyEvaluation
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not the token is redeemable according to the
        /// policies evaluated.  In general, the presence of any failed policies will indicate the token is
        /// not redeemable.
        /// </summary>
        [JsonProperty(PropertyName = "isRedeemable")]
        public bool IsRedeemable { get; set; }

        /// <summary>
        /// Gets or sets the collection of policy results, if any, after evaluation of policies.
        /// </summary>
        [JsonProperty(PropertyName = "policyResults")]
        public IEnumerable<TokenPolicyEvaluationResult> PolicyResults { get; set; }
    }
}