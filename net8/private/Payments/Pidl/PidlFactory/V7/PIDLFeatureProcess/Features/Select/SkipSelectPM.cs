// <copyright file="SkipSelectPM.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using static Microsoft.Commerce.Payments.PidlFactory.V7.Constants;

    /// <summary>
    /// Class representing the SkipSelectPM, which skips select PM if CreditCard is the only option and goes straight to CreditCard AddResource
    /// </summary>
    internal class SkipSelectPM : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                SkipSelectPMIfOnlyCC
            };
        }

        private static void SkipSelectPMIfOnlyCC(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (string.Equals(featureContext.OperationType, PidlOperationTypes.Select, StringComparison.OrdinalIgnoreCase))
            {
                if (PaymentSelectionHelper.IsAllCreditCardOrStoredValue(featureContext.PaymentMethods) && PaymentSelectionHelper.ContainsCreditCard(featureContext.PaymentMethods))
                {
                    string paymentMethodFamily = PaymentMethodFamilyNames.CreditCard;
                    string paymentMethodType = PaymentSelectionHelper.GetCommaSeparatedTypes(paymentMethodFamily, featureContext.PaymentMethods);
                    string id = string.Format("{0}.{1}", paymentMethodFamily, paymentMethodType);

                    inputResources.RemoveAll(p => true);
                    inputResources.Add(GetAddCreditCardClientActionResponse(featureContext.OriginalPartner, featureContext.Country, featureContext.Language, id, paymentMethodFamily, paymentMethodType, featureContext.FeatureConfigs));
                }
            }
        }

        private static PIDLResource GetAddCreditCardClientActionResponse(
            string partnerName,
            string country,
            string language,
            string id,
            string paymentMethodFamily,
            string paymentMethodType,
            Dictionary<string, FeatureConfig> featureConfigs)
        {
            ActionContext actionContext = new ActionContext()
            {
                Action = PaymentInstrumentActions.ToString(PIActionType.AddResource),
                Id = id,
                PaymentMethodFamily = paymentMethodFamily,
                PaymentMethodType = paymentMethodType
            };

            FeatureConfig featureConfig;
            featureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.SkipSelectPM, out featureConfig);

            if (featureConfig?.DisplayCustomizationDetail?.Count > 0)
            {
                foreach (PartnerSettingsModel.DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayHintCustomizationDetail.EnableIsSelectPMskippedValue == true)
                    {
                        actionContext.IsSelectPMSkipped = true;
                        break;
                    }
                }
            }

            actionContext.ResourceActionContext = new ResourceActionContext()
            {
                Action = actionContext.Action,
                PidlDocInfo = new PidlDocInfo(DescriptionTypes.PaymentInstrumentDescription, language, country, partnerName, null, PaymentMethodFamilyNames.CreditCard)
            };

            return new PIDLResource()
            {
                ClientAction = new ClientAction(ClientActionType.ReturnContext, actionContext)
            };
        }
    }
}