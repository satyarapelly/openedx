// <copyright file="AgenticPaymentHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    public class AgenticPaymentHelper
    {
        /// <summary>
        /// Updates default values for PropertyDescriptions in challengeMethodTypesPidl based on matching keys in payload
        /// </summary>
        /// <param name="challengeMethodTypesPidl">List of PIDLResource containing PropertyDescriptions to update</param>
        /// <param name="payload">PIDLData containing key-value pairs to match against PropertyDescriptionId</param>
        public static void UpdateDefaultValuesFromPayload(List<PIDLResource> challengeMethodTypesPidl, PIDLData payload)
        {
            if (challengeMethodTypesPidl == null || payload == null)
            {
                return;
            }

            foreach (PIDLResource pidlResource in challengeMethodTypesPidl)
            {
                if (pidlResource.DataDescription == null)
                {
                    continue;
                }

                UpdateDataDescriptionDefaultValues(pidlResource.DataDescription, payload);
            }
        }

        /// <summary>
        /// Recursively updates default values in data description dictionary
        /// </summary>
        /// <param name="dataDescription">Dictionary containing PropertyDescriptions and nested structures</param>
        /// <param name="payload">PIDLData containing key-value pairs to match against PropertyDescriptionId</param>
        public static void UpdateDataDescriptionDefaultValues(Dictionary<string, object> dataDescription, PIDLData payload)
        {
            if (dataDescription == null || payload == null)
            {
                return;
            }

            foreach (var kvp in dataDescription)
            {
                if (kvp.Value is PropertyDescription)
                {
                    PropertyDescription propertyDescription = (PropertyDescription)kvp.Value;

                    // Check if the PropertyDescriptionId exists as a key in the payload
                    if (!string.IsNullOrEmpty(propertyDescription.PropertyDescriptionId) &&
                        payload.ContainsKey(propertyDescription.PropertyDescriptionId))
                    {
                        // Update the DefaultValue with the value from payload
                        string payloadValue = payload.TryGetPropertyValueFromPIDLData(propertyDescription.PropertyDescriptionId);
                        if (payloadValue != null)
                        {
                            propertyDescription.DefaultValue = payloadValue;
                        }
                    }
                }
                else if (kvp.Value is List<PIDLResource>)
                {
                    var subPidlResources = kvp.Value as List<PIDLResource>;

                    // Handle nested PIDLResource collections
                    foreach (PIDLResource subPidlResource in subPidlResources)
                    {
                        if (subPidlResource.DataDescription != null)
                        {
                            UpdateDataDescriptionDefaultValues(subPidlResource.DataDescription, payload);
                        }
                    }
                }
            }
        }
    }
}