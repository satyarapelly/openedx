// <copyright file="GetEncryptionKeyResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.TokenizationService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class GetEncryptionKeyResponse
    {
        [JsonProperty(PropertyName = "keys")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<EncryptionKey> Keys { get; set; }
    }
}