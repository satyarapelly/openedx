// <copyright file="TokenAssetValue.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel
{
    using Newtonsoft.Json;

    public class TokenAssetValue
    {
        /// <summary>
        /// Gets or sets the value for assets that have a value, such as CSV.
        /// This represents the actual value associated with the token and has meaning only when
        /// combined with the value measurement (unit of measure).
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the unit of measurement for assets that have a value, such as CSV.
        /// This represents the unit of measurement, such as s currency code.
        /// </summary>
        [JsonProperty(PropertyName = "valueMeasurement")]
        public string ValueMeasurement { get; set; }
    }
}