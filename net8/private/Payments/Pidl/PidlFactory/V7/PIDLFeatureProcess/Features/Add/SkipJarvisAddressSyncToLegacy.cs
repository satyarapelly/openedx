// <copyright file="SkipJarvisAddressSyncToLegacy.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Security.AccessControl;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using static Microsoft.Commerce.Payments.PidlFactory.V7.Constants;

    /// <summary>
    /// Adds syncToLegacy=0 query param to jarvis post address api. This skips CTP account creation from jarvis. 
    /// </summary>
    internal class SkipJarvisAddressSyncToLegacy : IFeature
    {
        private const string JarvisFdAddressCreateUrlTemplate = "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses";

        // Ref Constants.SyncToLegacyCodes for different values
        private const string JarvisFdAddressCreateUrlTemplateWithSkipCTP = "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses?syncToLegacy=0";

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                SkipSyncToLegacy
            };
        }

        internal static void SkipSyncToLegacy(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (featureContext.ResourceType.Equals(ResourceTypes.Address) && featureContext.OperationType.Equals(PidlOperationTypes.Add))
            {
                foreach (PIDLResource addressPidl in inputResources)
                {
                    var saveButton = addressPidl.GetDisplayHintById(Constants.ButtonDisplayHintIds.SaveButton);
                    var submitLink = saveButton?.Action?.Context as PXCommon.RestLink;
                    if (submitLink?.Href?.Equals(JarvisFdAddressCreateUrlTemplate) ?? false)
                    {
                        // Replace the submit link with new url with syncToLegacy=0 query param
                        submitLink.Href = JarvisFdAddressCreateUrlTemplateWithSkipCTP;
                    }
                }
            }
        }
    }
}