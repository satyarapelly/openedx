// <copyright file="EnableSecureField.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    internal class EnableSecureField : IFeature
    {
        private static readonly Dictionary<string, List<string>> availableKeyPropertyResolutionMappings = new Dictionary<string, List<string>>()
        {
            { "amex", new List<string>() { "^3[47][0-9]{13}$", "371111111111114" } },
            { "visa", new List<string>() { "^4([0-9]{12}|[0-9]{15})$", "4444444444444444" } },            
            { "mc", new List<string>() { "(?!^((?!506199)(506099|5061(\\d\\d))(\\d{10}|\\d{12}|\\d{13}))|(507(86[5-9]|8[7-9][0-9]|9[0-5][0-9]|96[0-4]))(\\d{10}|\\d{13})|(6500(0[2-9]|1[0-9]|2[0-7]))(\\d{10})$)(^(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}$|^(?:5[0678]\\d\\d|6304|6390|67\\d\\d)\\d{8,15}$)", "5444444444444444" } },
            { "discover", new List<string>() { "^6(?:011|4[4-9][0-9]|5[0-9]{2})[0-9]{12}|62[4-6][0-9]{13}|628[2-8][0-9]{12}|622(1(2[6-9]|[3-9][0-9])|[2-8][0-9]{2}|9([01][0-9]|2[0-5]))[0-9]{10}$", "6444444444444444" } },
            { "elo", new List<string>() { "^(4011(78|79)|43(1274|8935)|45(1416|7393|763(1|2))|50(4175|6699|67[0-7][0-9]|9000)|50(9[0-9][0-9][0-9])|627780|63(6297|6368)|650(03([^4])|04([0-9])|05(0|1)|4([0-3][0-9]|8[5-9]|9[0-9])|5([0-9][0-9]|3[0-8])|9([0-6][0-9]|7[0-8])|7([0-2][0-9])|541|700|720|727|901)|65165([2-9])|6516([6-7][0-9])|65500([0-9])|6550([0-5][0-9])|655021|65505([6-7])|650077|650078|651688|650062|650072|650073|650074|650075|650079|650080|650081|651696|651697|651700|651702|651703|651704|650070|650076|651701|650071|650069|650059|650060|650061|650065|650066|650067|650063|650064|650058|650068)\\d{10}$", "5066991111111118" } },
            { "verve", new List<string>() { "^((?!506199)(506099|5061(\\d\\d))(\\d{10}|\\d{12}|\\d{13}))|(507(86[5-9]|8[7-9][0-9]|9[0-5][0-9]|96[0-4]))(\\d{10}|\\d{13})|(6500(0[2-9]|1[0-9]|2[0-7]))(\\d{10})$", "506103000000000000" } },
            { "hipercard", new List<string>() { "^(3841[046]0|606282|637(5(68|99)|095|6(09|12)))(\\d{7}|\\d{10}|\\d{13})$", "6062821234567890" } },
            { "jcb", new List<string>() { "^(?:3[0-9]{15}|(2131|1800)[0-9]{11})$", "3111111111111111" } },
            { "rupay", new List<string>() { "^(60|65|81|82|508|353|356|999|502|504|505)\\d{13,14}$", "6012345678901234" } },
            { "unionpay_international", new List<string>() { "^(?!3(?:6|8|9[0-9])[0-9]{12})(?!3[47][0-9]{13}$)(?!4([0-9]{12}|[0-9]{15})$)(?!(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}$)(?!(?:5[0678]\\d\\d|6304|6390|67\\d\\d)\\d{8,15}$)(?!6(?:011|4[4-9][0-9]|5[0-9]{2})[0-9]{12}$)(?!62[4-6][0-9]{13}$)(?!628[2-8][0-9]{12}$)(?!622(1(2[6-9]|[3-9][0-9])|[2-8][0-9]{2}|9([01][0-9]|2[0-5]))[0-9]{10}$)(?!(?:3[0-9]{15}|(2131|1800)[0-9]{11})$)(?:(?![0-2]|[4-5]|[7])[0-9][0-9]{13,18}$|5[0-9]{17}$)", "6291200000000001" } }
        };

        private static HashSet<string> secureDisplayHints = new HashSet<string>()
        {
            Constants.DisplayHintIds.NumberDisplayHintId,
            Constants.DisplayHintIds.AmexNumberDisplayHintId,
            Constants.DisplayHintIds.CupInternationalNumberDisplayHintId,
            Constants.DisplayHintIds.CVV,
            Constants.DisplayHintIds.CVV3,
            Constants.DisplayHintIds.CVV4,
            Constants.DisplayHintIds.PaymentInstrumentSearchTransactionsCvv,
            Constants.DisplayHintIds.ChallengeCvvSecurityCode,
            Constants.DisplayHintIds.ChallengeCvvSecurityCodeWithValidation
        };
        
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                AddSecureFields
            };
        }

        internal static List<List<string>> GetkeyPropertyResolutionMappings(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            const string Type = "type";

            var keyPropertyResolutionMappings = new List<List<string>>();

            foreach (var resource in inputResources)
            {
                if (resource.Identity != null && resource.Identity.ContainsKey(Type))
                {
                    var cardType = resource.Identity[Type].ToLower();
                    if (availableKeyPropertyResolutionMappings.ContainsKey(cardType))
                    {
                        var keyPropertyResolutionMapping = availableKeyPropertyResolutionMappings[cardType];

                        // Check if the update regexes for cards flight/feature is enabled
                        CheckForUpdate(keyPropertyResolutionMapping, cardType, featureContext);
                        keyPropertyResolutionMappings.Add(keyPropertyResolutionMapping);
                    }
                }
            }

            return keyPropertyResolutionMappings;
        }

        internal static void AddSecureFields(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (inputResources == null || inputResources.Count == 0)
            {
                return;
            }

            if (featureContext == null || featureContext.FeatureConfigs == null)
            {
                return;
            }

            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.EnableSecureField, out featureConfig);

            if (featureConfig?.DisplayCustomizationDetail?.Count <= 0)
            {
                return;
            }

            var enableSecureField = featureConfig.DisplayCustomizationDetail.Any(x => x.EnableSecureFieldAddCC);

            if (!enableSecureField)
            {
                return;
            }

            var keyPropertyResolutionMappings = GetkeyPropertyResolutionMappings(inputResources, featureContext);

            foreach (PIDLResource paymentPidlResource in inputResources)
            {                
                var displayHints = paymentPidlResource.GetDisplayHints().Where((displayHint) => secureDisplayHints.Contains(displayHint.HintId)).ToList();

                if (displayHints == null || displayHints.Count == 0)
                {
                    continue;
                }

                paymentPidlResource.AddClientSetting(Constants.ClientSettingNames.PidlSdkWaitTimeForSecureFieldsInit, Constants.ClientSettings.PidlSdkWaitTimeForSecureFieldsInit);

                foreach (var displayHint in displayHints)
                {
                    PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;
                    if (propertyDisplayHint == null)
                    {
                        continue;
                    }

                    var secureDisplayHint = SecurePropertyDisplayHint.CreateInstance(propertyDisplayHint);
                    PidlFactoryHelper.RepalceDisplayHint(paymentPidlResource, displayHint, secureDisplayHint);

                    if (string.Equals(displayHint.PropertyName, Constants.DataDescriptionVariableNames.AccountToken, StringComparison.OrdinalIgnoreCase))
                    {
                        var dataDescription = paymentPidlResource.GetTargetDataDescription(Constants.DataDescriptionVariableNames.Details);
                        if (dataDescription != null && dataDescription.ContainsKey(Constants.DataDescriptionVariableNames.AccountToken))
                        {                            
                            var propertyDescription = (PropertyDescription)dataDescription[Constants.DataDescriptionVariableNames.AccountToken];
                            propertyDescription?.UpdateKeyPropertyResolutionMappings(keyPropertyResolutionMappings);
                        }
                    }
                }
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> GetRegexesForCards(FeatureContext featureContext)
        {
            var regexes = new List<KeyValuePair<string, string>>();
            if (featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.EnableUpdateCreditCardRegex, out var featureConfig))
            {
                foreach (var displayCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayCustomizationDetail.UpdateRegexesForCards != null)
                    {
                        regexes.AddRange(displayCustomizationDetail.UpdateRegexesForCards);
                    }
                }
            }

            return regexes;
        }

        private static bool IsUpdateCreditCardRegexEnabled(FeatureContext featureContext)
        {
            return FeatureConfiguration.IsEnabled(FeatureConfiguration.FeatureNames.EnableUpdateCreditCardRegex, featureContext) ||
                   FeatureConfiguration.IsEnabledUsingPartnerSettings(FeatureConfiguration.FeatureNames.EnableUpdateCreditCardRegex, featureContext);
        }

        private static void CheckForUpdate(List<string> keyPropertyResolutionMapping, string cardType, FeatureContext featureContext)
        {
            if (IsUpdateCreditCardRegexEnabled(featureContext))
            {
                var regexes = GetRegexesForCards(featureContext);
                foreach (var regex in regexes)
                {
                    if (string.Equals(cardType, regex.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        keyPropertyResolutionMapping[0] = regex.Value;
                    }
                }
            }
        }
    }
}