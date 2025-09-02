// <copyright file="PublicKeyInfo.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pims
{
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public enum KeyType
    {
        Tpm,
        Soft,
    }

    public class PublicKeyInfo
    {
        /// <summary>
        /// Gets or sets the key value. ASN.1 SubjectPublicKeyInfo as defined in RFC 5280 and RFC 3280. The SubjectPublicKeyInfo blob is encoded in UrlSafeBase64.
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "keyType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public KeyType KeyType { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Required by json serialization")]
        [JsonProperty(PropertyName = "attestationBlob")]
        public byte[] AttestationBlob { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Required by json serialization")]
        [JsonProperty(PropertyName = "attestationKeyChain")]
        public byte[] AttestationKeyChain { get; set; }
    }
}
