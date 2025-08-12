// <copyright file="AssuranceDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.WalletService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    // Metadata for Google Pay Wallet token
    public class AssuranceDetails
    {
        [JsonProperty("cardHolderAuthenticated")]
        public string CardHolderAuthenticated { get; set; }

        [JsonProperty("accountVerified")]
        public string AccountVerified { get; set; }
    }
}