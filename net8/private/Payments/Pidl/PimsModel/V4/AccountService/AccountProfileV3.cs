// <copyright file="AccountProfileV3.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public abstract class AccountProfileV3
    {
        [JsonProperty(PropertyName = Fields.ProfileId, Required = Required.Default)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = Fields.Etag, Required = Required.Default)]
        public string Etag { get; set; }

        [JsonProperty(PropertyName = Fields.CustomerId, Required = Required.Default)]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = Fields.DefaultAddressId, Required = Required.Default)]
        public string DefaultAddressId { get; set; }

        [JsonProperty(PropertyName = Fields.DefaultShippingAddressId, Required = Required.Default)]
        public string DefaultShippingAddressId { get; set; }

        [JsonProperty(PropertyName = Fields.Country, Required = Required.Default)]
        public string Country { get; set; }

        [JsonProperty(PropertyName = Fields.SnapshotId, Required = Required.Default)]
        public string SnapshotId { get; set; }

        [JsonProperty(PropertyName = Fields.Links, Required = Required.Default)]
        public object Links { get; set; }

        [JsonProperty(PropertyName = Fields.ObjectType, Required = Required.Default)]
        public string ObjectType { get; set; }

        [JsonProperty(PropertyName = Fields.ResourceStatus, Required = Required.Default)]
        public string ResourceStatus { get; set; }

        // Type must be in the schema and match the type of the profile
        [JsonProperty(PropertyName = Fields.ProfileType, Required = Required.Always)]
        public string ProfileType { get; set; }

        // These are the custom properties
        [JsonProperty(PropertyName = Fields.FirstName, Required = Required.Default)]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = Fields.LastName, Required = Required.Default)]
        public string LastName { get; set; }

        /// <summary>
        /// This function works around the lack of an email address,
        /// which isn't defined as a property at the abstract parent level
        /// because it differs in implementation between subtypes
        /// </summary>
        /// <returns>Returns the subclass specific email address property value</returns>
        public abstract string GetEmailAddressPropertyValue();

        public virtual Dictionary<string, string> GetPropertyDictionary()
        {
            Dictionary<string, string> propertyDictionary = new Dictionary<string, string>();
            propertyDictionary[AccountProfileV3.Fields.ProfileId] = this.Id;
            propertyDictionary[AccountProfileV3.Fields.Etag] = this.Etag;
            propertyDictionary[AccountProfileV3.Fields.CustomerId] = this.CustomerId;
            propertyDictionary[AccountProfileV3.Fields.DefaultAddressId] = this.DefaultAddressId;
            propertyDictionary[AccountProfileV3.Fields.DefaultShippingAddressId] = this.DefaultShippingAddressId;
            propertyDictionary[AccountProfileV3.Fields.Country] = this.Country;
            propertyDictionary[AccountProfileV3.Fields.SnapshotId] = this.SnapshotId;
            propertyDictionary[AccountProfileV3.Fields.Links] = this.Links.ToString();
            propertyDictionary[AccountProfileV3.Fields.ObjectType] = this.ObjectType;
            propertyDictionary[AccountProfileV3.Fields.ResourceStatus] = this.ResourceStatus;
            propertyDictionary[AccountProfileV3.Fields.ProfileType] = this.ProfileType;
            propertyDictionary[AccountProfileV3.Fields.FirstName] = this.FirstName;
            propertyDictionary[AccountProfileV3.Fields.LastName] = this.LastName;
            return propertyDictionary;
        }

        private static class Fields
        {
            internal const string ProfileId = "id";
            internal const string Etag = "etag";
            internal const string CustomerId = "customer_id";
            internal const string DefaultAddressId = "default_address_id";
            internal const string DefaultShippingAddressId = "default_shipping_address_id";
            internal const string Country = "country";
            internal const string SnapshotId = "snapshot_id";
            internal const string Links = "links";
            internal const string ObjectType = "object_type";
            internal const string ResourceStatus = "resource_status";
            internal const string ProfileType = "type";
            internal const string FirstName = "first_name";
            internal const string LastName = "last_name";
        }
    }
}