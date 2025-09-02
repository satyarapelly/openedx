// <copyright file="AccountConsumerProfileV3.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class AccountConsumerProfileV3 : AccountProfileV3
    {
        [JsonProperty(PropertyName = Fields.Culture, Required = Required.Default)]
        public string Culture { get; set; }

        [JsonProperty(PropertyName = Fields.EmailAddress, Required = Required.Default)]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = Fields.LocaleId, Required = Required.Default)]
        public int LocaleId { get; set; }

        public override string GetEmailAddressPropertyValue()
        {
            return this.EmailAddress;
        }

        public override Dictionary<string, string> GetPropertyDictionary()
        {
            Dictionary<string, string> propertyDictionary = base.GetPropertyDictionary();
            propertyDictionary[AccountConsumerProfileV3.Fields.EmailAddress] = this.EmailAddress;
            return propertyDictionary;
        }

        private static class Fields
        {
            internal const string EmailAddress = "email_address";
            internal const string LocaleId = "locale_id";
            internal const string Culture = "culture";
        }
    }
}