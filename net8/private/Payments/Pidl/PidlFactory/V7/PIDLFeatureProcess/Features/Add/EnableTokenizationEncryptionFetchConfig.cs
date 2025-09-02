// <copyright file="EnableTokenizationEncryptionFetchConfig.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;

    internal class EnableTokenizationEncryptionFetchConfig : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                AddTokenizationEncryptionFetchConfig
            };
        }

        internal static void AddTokenizationEncryptionFetchConfig(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.EnableTokenizationEncryptionFetchConfig, out featureConfig);

            if (featureConfig != null)
            {
                foreach (PIDLResource paymentPidlResource in inputResources)
                {
                    // Skip fetrch config if PIDL resource is enabeld with secure property                    
                    if (IsSecureFieldEnabled(featureContext))
                    {
                        continue;
                    }

                    var dataDescription = paymentPidlResource.GetTargetDataDescription(V7.Constants.DataDescriptionVariableNames.Details);

                    if (dataDescription == null)
                    {
                        continue;
                    }

                    // Check if the encrypt based on the script file feature is enabled
                    bool isEncryptScriptEnabled = featureContext.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncryptionFetchConfigWithScript);

                    // Enable fetch config - PAN and CVV for Add/Update/CVV/India 3DS/Serach Transaction operations
                    if (featureContext.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncryptionFetchConfigAddUpdateCC)
                        || featureContext.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncryptionFetchConfigOtherOperation))
                    {
                        // Pan for Add Credit card operation
                        if (dataDescription.ContainsKey(V7.Constants.DataDescriptionVariableNames.AccountToken))
                        {
                            var propertyDescription = (PropertyDescription)dataDescription[V7.Constants.DataDescriptionVariableNames.AccountToken];
                            propertyDescription.DataProtection = GetDataProtections(featureContext, null, V7.Constants.PropertyDataProtectionType.TokenizeMSREncrypt, isEncryptScriptEnabled);
                            propertyDescription.DataProtection.FetchConfig = GetDataProtectionsFetchConfig(featureContext, propertyDescription.TokenSet);
                        }

                        // CVV for Add/Update Credit card and also CVV for India 3DS and Search Transaction operations
                        if (dataDescription.ContainsKey(V7.Constants.DataDescriptionVariableNames.CvvToken))
                        {
                            var propertyDescription = (PropertyDescription)dataDescription[V7.Constants.DataDescriptionVariableNames.CvvToken];
                            propertyDescription.DataProtection = GetDataProtections(featureContext, null, V7.Constants.PropertyDataProtectionType.TokenizeMSREncrypt, isEncryptScriptEnabled);
                            propertyDescription.DataProtection.FetchConfig = GetDataProtectionsFetchConfig(featureContext, propertyDescription.TokenSet);
                        }
                    }

                    // Enable fetch config - PiAuthKey for Add crdit card operation
                    if (featureContext.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncFetchConfigAddCCPiAuthKey))
                    {
                        if (dataDescription.ContainsKey(V7.Constants.DataDescriptionVariableNames.Permission))
                        {
                            var hmacDataDescription = paymentPidlResource.GetTargetDataDescription(V7.Constants.DataDescriptionVariableNames.Details + "." + V7.Constants.DataDescriptionVariableNames.Permission);
                            if (hmacDataDescription == null || !hmacDataDescription.ContainsKey(V7.Constants.DataDescriptionVariableNames.Hmac))
                            {
                                continue;
                            }

                            var hmacPropertyDescription = (PropertyDescription)hmacDataDescription[V7.Constants.DataDescriptionVariableNames.Hmac];

                            // Check if the disable encrypted payload feature is enabled
                            bool isEncryptedPayloadDisabled = featureContext.ExposedFlightFeatures.Contains(Flighting.Features.PXDisableTokenizationEncPiAuthKeyFetchConfigtEncPayload);

                            hmacPropertyDescription.DataProtection = GetDataProtections(featureContext, hmacPropertyDescription.DataProtection, V7.Constants.PropertyDataProtectionType.HMACSignatureMSREncrypt, isEncryptScriptEnabled);
                            hmacPropertyDescription.DataProtection.FetchConfig = GetDataProtectionsFetchConfig(featureContext, V7.Constants.FetchConfig.HmacTokenSet, isEncryptedPayloadDisabled);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the secure field feature is enabled.
        /// </summary>
        /// <param name="featureContext">The feature context to check for the secure field feature.</param>
        /// <returns>True if the secure field feature is enabled; otherwise, false.</returns>
        private static bool IsSecureFieldEnabled(FeatureContext featureContext)
        {
            return FeatureConfiguration.IsEnabled(FeatureConfiguration.FeatureNames.EnableSecureField, featureContext) ||
                   FeatureConfiguration.IsEnabledUsingPartnerSettings(FeatureConfiguration.FeatureNames.EnableSecureField, featureContext);
        }

        /// <summary>
        /// Get data protection for token set (pan/cvv/piauthkey)
        /// </summary>
        /// <param name="featureContext">Feature context to get public key and endpoints</param>
        /// <param name="dataProtection">Current data protection from PIDL</param>
        /// <param name="protectionType">Token Set (pan/cvv/piauthkey)</param>
        /// <param name="isEncryptScriptEnabled">Flag to set encryption script file or function name</param>
        /// <returns>New or updated property data protection</returns>
        private static PropertyDataProtection GetDataProtections(FeatureContext featureContext, PropertyDataProtection dataProtection, string protectionType, bool isEncryptScriptEnabled)
        {
            // Set encryption function name by default
            string encryptionScriptkey = V7.Constants.PropertyDataProtectionParamName.EncryptionFunction;
            string encryptionScriptValue = V7.Constants.PropertyDataProtectionParamValue.EncryptionFunction;

            // Set encryption script file name if the feature is enabled
            if (isEncryptScriptEnabled)
            {
                encryptionScriptkey = V7.Constants.PropertyDataProtectionParamName.EncryptionScript;
                encryptionScriptValue = V7.Constants.PropertyDataProtectionParamValue.EncryptionScript;
            }

            // Update the data protection if it is already present(PiAuthKey)
            if (dataProtection != null)
            {
                dataProtection.ProtectionType = protectionType;
                dataProtection.Parameters = new Dictionary<string, string>
                    {
                        { V7.Constants.PropertyDataProtectionParamName.EncryptionLibrary, V7.Constants.PropertyDataProtectionParamValue.EncryptionLibrary },
                        { encryptionScriptkey, encryptionScriptValue },
                        { V7.Constants.PropertyDataProtectionParamName.PublicKey, featureContext.TokenizationPublicKey }
                    };
            }
            else
            {
                // Create new data protection if it is not present (Pan/CVV)
                dataProtection = new PropertyDataProtection()
                {
                    ProtectionType = protectionType,
                    Parameters = new Dictionary<string, string>
                    {
                        { V7.Constants.PropertyDataProtectionParamName.EncryptionLibrary, V7.Constants.PropertyDataProtectionParamValue.EncryptionLibrary },
                        { encryptionScriptkey, encryptionScriptValue },
                        { V7.Constants.PropertyDataProtectionParamName.PublicKey, featureContext.TokenizationPublicKey }
                    }
                };
            }

            // Need to be able to turn this on for any browser to update current tokenization where encrypted tokenization is used for backup
            // and for when all tokenization is encrypted if PXRemoveUseFallbackWhenEncryptedTokenizationOnlyForPanCvvPiAuthKey is also on
            if (featureContext.ExposedFlightFeatures.Contains(Flighting.Features.PXRemoveUseFallbackForSubtleImportKey) ||
                (featureContext.ExposedFlightFeatures.Contains(Flighting.Features.PXEncryptedTokenizationOnlyForPanCvvPiAuthKey) &&
                featureContext.ExposedFlightFeatures.Contains(Flighting.Features.PXRemoveUseFallbackWhenEncryptedTokenizationOnlyForPanCvvPiAuthKey)))
            {
                dataProtection.Parameters.Add(V7.Constants.PropertyDataProtectionParamName.RemoveUseFallback, "true");
            }

            return dataProtection;
        }

        /// <summary>
        /// Get fetch config for token set (pan/cvv/piauthkey)
        /// </summary>
        /// <param name="featureContext">Feature context to get public key and endpoints</param>
        /// <param name="token">Token Set (pan/cvv/piauthkey)</param>
        /// <param name="isEncryptedPayloadDisabled">Flag is to set encrypted payload(use secondary payload) is true/false</param>
        /// <returns>Fetch config for (pan/cvv/piauthkey)</returns>
        private static FetchConfig GetDataProtectionsFetchConfig(FeatureContext featureContext, string token, bool isEncryptedPayloadDisabled = false)
        {
            return new FetchConfig()
            {
                InitialRetryTimeout = V7.Constants.FetchConfig.InitialRetryTimeout,
                RetryTimeoutMultiplier = V7.Constants.FetchConfig.RetryTimeoutMultiplier,
                RetryableErrorCodes = V7.Constants.FetchConfig.RetryableErrorCodes,
                MaxServerErrorRetryCount = V7.Constants.FetchConfig.MaxServerErrorRetryCount,

                FetchOrder = GetFetchOrder(featureContext, token, isEncryptedPayloadDisabled)
            };
        }

        /// <summary>
        /// Get fetch order for token set (pan/cvv/piauthkey)
        /// </summary>
        /// <param name="featureContext">Feature context to get public key and endpoints</param>
        /// <param name="token">Token Set (pan/cvv/piauthkey)</param>
        /// <param name="isEncryptedPayloadDisabled">Flag is to set encrypted payload(use secondary payload) is true/false</param>
        /// <returns>Fetch order with tokenization/tokenizationfd endpoint</returns>
        private static List<FetchOrder> GetFetchOrder(FeatureContext featureContext, string token, bool isEncryptedPayloadDisabled)
        {
            if (featureContext.ExposedFlightFeatures.Contains(Flighting.Features.PXEncryptedTokenizationOnlyForPanCvvPiAuthKey))
            {
                return new List<FetchOrder>()
                {
                    new FetchOrder()
                    {
                        Retry = V7.Constants.FetchConfig.Retry2,
                        Endpoint = GetEndpoint(featureContext, token, false, false, true),
                        UseSecondaryPayload = true,
                    },
                    new FetchOrder()
                    {
                        Retry = V7.Constants.FetchConfig.Retry1,
                        Endpoint = GetEndpoint(featureContext, token, false, false, true),
                        UseSecondaryPayload = true,
                        XHRConfig = GetXHRConfig()
                    },
                    new FetchOrder()
                    {
                        Retry = V7.Constants.FetchConfig.Retry2,
                        Endpoint = GetEndpoint(featureContext, token, true, false),
                        UseSecondaryPayload = true,
                    },
                    new FetchOrder()
                    {
                        Retry = V7.Constants.FetchConfig.Retry1,
                        Endpoint = GetEndpoint(featureContext, token, true, false),
                        UseSecondaryPayload = true,
                        XHRConfig = GetXHRConfig()
                    }
                };
            }

            return new List<FetchOrder>()
            {
                new FetchOrder()
                {
                    Retry = V7.Constants.FetchConfig.Retry2,
                    Endpoint = GetEndpoint(featureContext, token),
                },
                new FetchOrder()
                {
                    Retry = V7.Constants.FetchConfig.Retry1,
                    Endpoint = GetEndpoint(featureContext, token),
                    XHRConfig = GetXHRConfig()
                },
                new FetchOrder()
                {
                    Retry = V7.Constants.FetchConfig.Retry2,
                    Endpoint = GetEndpoint(featureContext, token, true, isEncryptedPayloadDisabled),
                    UseSecondaryPayload = !isEncryptedPayloadDisabled,
                },
                new FetchOrder()
                {
                    Retry = V7.Constants.FetchConfig.Retry1,
                    Endpoint = GetEndpoint(featureContext, token, true, isEncryptedPayloadDisabled),
                    UseSecondaryPayload = !isEncryptedPayloadDisabled,
                    XHRConfig = GetXHRConfig()
                }
            };
        }

        /// <summary>
        /// Get endpoint for token set (pan/cvv/piauthkey) with tokenization/tokenizationfd endpoint
        /// </summary>
        /// <param name="featureContext">Feature context to get public key and endpoints</param>
        /// <param name="token">Token Set (pan/cvv/piauthkey)</param>
        /// <param name="isSecondaryEndpoint">Flag is set tokenzation/tokenizationfd endpoint</param>
        /// <param name="isEncryptedPayloadDisabled">Flag is to set path /getToken or /gettokenfromencryptedvalue for afd endpoint</param>
        /// <param name="isPrimaryPayloadEncrypted">Flag is to set path /getToken or /gettokenfromencryptedvalue for default endpoint</param>
        /// <returns>Return endpoint with tokenization/tokenizationfd</returns>
        private static string GetEndpoint(FeatureContext featureContext, string token, bool isSecondaryEndpoint = false, bool isEncryptedPayloadDisabled = false, bool isPrimaryPayloadEncrypted = false)
        {
            // TokenizationFD endpoint for token set (pan/cvv/piauthkey), if isEncryptedPayloadDisabled then use TokenizationFD/getToken else TokenizationFD/gettokenfromencryptedvalue
            if (isSecondaryEndpoint)
            {
                return isEncryptedPayloadDisabled ?
                        string.Format(V7.Constants.FetchConfig.GetTokenEndpoint, featureContext.TokenizationServiceUrls[V7.Constants.FetchConfig.GetTokenFromEncryptedValueURL], token)
                        : string.Format(V7.Constants.FetchConfig.GetTokenFromEncryptedValueEndpoint, featureContext.TokenizationServiceUrls[V7.Constants.FetchConfig.GetTokenFromEncryptedValueURL], token);
            }

            // Default - tokenization/getToken endpoint for (pan/cvv/piauthkey)
            return isPrimaryPayloadEncrypted ? 
                    string.Format(V7.Constants.FetchConfig.GetTokenFromEncryptedValueEndpoint, featureContext.TokenizationServiceUrls[V7.Constants.FetchConfig.GetTokenURL], token)
                    : string.Format(V7.Constants.FetchConfig.GetTokenEndpoint, featureContext.TokenizationServiceUrls[V7.Constants.FetchConfig.GetTokenURL], token);
        }

        /// <summary>
        /// Get XHR config for token set (pan/cvv/piauthkey) with timeout
        /// </summary>
        /// <returns>XHR config with request timeout</returns>
        private static XHRConfig GetXHRConfig()
        {
            return new XHRConfig()
            {
                GetRequestTimeout = V7.Constants.FetchConfig.GetRequestTimeout,
                PostRequestTimeout = V7.Constants.FetchConfig.PostRequestTimeout,
            };
        }
    }    
}