// <copyright file="AccountProfile.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System;
    using Newtonsoft.Json;
    
    public class AccountProfile
    {
        /// <summary>
        /// Gets or sets the Id of this profile resource
        /// </summary>
        [JsonProperty(PropertyName = "id", Required = Required.Default)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the accountId of the user
        /// </summary>
        [JsonProperty(PropertyName = "account_id", Required = Required.Always)]
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the company name
        /// </summary>
        [JsonProperty(PropertyName = "company_name", Required = Required.Default)]
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the default locale for the profile
        /// </summary>
        [JsonProperty(PropertyName = "culture", Required = Required.Default)]
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets the date of birth
        /// </summary>
        [JsonProperty(PropertyName = "birth_date", Required = Required.Default)]
        public string DateOfBirth { get; set; }

        /// <summary>
        /// Gets or sets the default address id
        /// </summary>
        [JsonProperty(PropertyName = "default_address_id", Required = Required.Default)]
        public string DefaultAddressId { get; set; }

        /// <summary>
        /// Gets or sets the Email address
        /// </summary>
        [JsonProperty(PropertyName = "email_address", Required = Required.Default)]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        [JsonProperty(PropertyName = "first_name", Required = Required.Default)]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        [JsonProperty(PropertyName = "last_name", Required = Required.Default)]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the type of the profile
        /// </summary>
        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        public string ProfileType { get; set; }

        /// <summary>
        /// Gets or sets the nationality of the profile
        /// </summary>
        [JsonProperty(PropertyName = "nationality", Required = Required.Default)]
        public string Nationality { get; set; }
    }
}