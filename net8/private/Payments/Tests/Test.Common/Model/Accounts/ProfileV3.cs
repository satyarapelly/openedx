// <copyright file="ProfileV3.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.Accounts
{
    using Newtonsoft.Json;

    public abstract class ProfileV3
    {
        [JsonProperty(PropertyName = "id", Required = Required.Default)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "etag", Required = Required.Default)]
        public string Etag { get; set; }

        [JsonProperty(PropertyName = "customer_id", Required = Required.Default)]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "default_address_id", Required = Required.Default)]
        public string DefaultAddressId { get; set; }

        [JsonProperty(PropertyName = "country", Required = Required.Default)]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "snapshot_id", Required = Required.Default)]
        public string SnapshotId { get; set; }

        [JsonProperty(PropertyName = "links", Required = Required.Default)]
        public object Links { get; set; }

        [JsonProperty(PropertyName = "object_type", Required = Required.Default)]
        public string ObjectType { get; set; }

        [JsonProperty(PropertyName = "resource_status", Required = Required.Default)]
        public string ResourceStatus { get; set; }

        // Type must be in the schema and match the type of the profile
        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        public string ProfileType { get; set; }

        // These are the custom properties
        [JsonProperty(PropertyName = "first_name", Required = Required.Default)]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name", Required = Required.Default)]
        public string LastName { get; set; }
    }
}