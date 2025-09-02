// <copyright file="SkipSelectInstanceNoPI.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using static Microsoft.Commerce.Payments.PidlFactory.V7.Constants;

    /// <summary>
    /// Class representing the SkipSelectInstanceNoPI, which skips selectInstance (listPI) if no PIs available and returns AddResource action.
    /// </summary>
    internal class SkipSelectInstanceNoPI : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                SkipSelectInstanceIfNoPIFound
            };
        }

        private static void SkipSelectInstanceIfNoPIFound(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.SkipSelectInstanceNoPI, out FeatureConfig featureConfig);

            if (string.Equals(featureContext.OperationType, PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase))
            {
                bool noPaymentInstruments = featureContext.PaymentInstruments.Count == 0 && featureContext.DisabledPaymentInstruments.Count == 0;
                PaymentSelectionHelper.TryGetPaymentMethodFilters(featureContext.Filters, out PaymentMethodFilters filters);

                // Check if IsBackupPiOptional validation is required or not
                bool isBackupPICheckEnabled = featureConfig?.DisplayCustomizationDetail?.Select(d => d.EnableBackupPICheckForSkipSelectInstanceNoPI ?? false).FirstOrDefault() ?? false;

                if (noPaymentInstruments && (!isBackupPICheckEnabled || filters?.IsBackupPiOptional != true))
                {
                    bool addTriggeredBy = featureConfig?.DisplayCustomizationDetail?.Select(d => d.AddTriggeredByForSkipSelectInstanceNoPI ?? false).FirstOrDefault() ?? false;
                    bool returnAddCCOnly = featureConfig?.DisplayCustomizationDetail?.Select(d => d.ReturnAddCCOnlyForSkipSelectInstanceNoPI ?? false).FirstOrDefault() ?? false;

                    inputResources.RemoveAll(pidl => true);
                    inputResources.Add(GetAddResourceClientActionResponse(featureContext.OriginalPartner, featureContext.Country, featureContext.Language, returnAddCCOnly, addTriggeredBy));
                }
            }
        }

        /// <summary>
        /// Returns Add Resource Client Action Response with additional returnAddCCOnly and addTriggeredBy customizations.
        /// </summary>
        /// <param name="partnerName">Partner name</param>
        /// <param name="country">Country parameter</param>
        /// <param name="language">Language parameter</param>
        /// <param name="returnAddCCOnly">ReturnAddCConly customization for feature</param>
        /// <param name="addTriggeredBy">AddTriggeredBy customization for feature</param>
        /// <returns>Returns Add Resource Client Action Response </returns>
        private static PIDLResource GetAddResourceClientActionResponse(
            string partnerName,
            string country,
            string language,
            bool returnAddCCOnly,
            bool addTriggeredBy)
        {
            PIDLResource retVal = new PIDLResource();
            PidlDocInfo pidlDocInfo = new PidlDocInfo(DescriptionTypes.PaymentInstrumentDescription, language, country, partnerName);
            ActionContext actionContext = new ActionContext()
            {
                Action = PaymentInstrumentActions.ToString(PIActionType.AddPaymentInstrument),
                ResourceActionContext = new ResourceActionContext()
                {
                    Action = PaymentInstrumentActions.ToString(PIActionType.AddPaymentInstrument),
                    PidlDocInfo = pidlDocInfo,
                }
            };

            retVal.ClientAction = new ClientAction(ClientActionType.ReturnContext, actionContext);

            // If returnAddCCOnly feature property enabled then add the family to param and action to AddResource
            if (returnAddCCOnly)
            {
                pidlDocInfo.Parameters.Add(Constants.QueryParamFields.Family, PaymentMethodFamilyNames.CreditCard);
                actionContext.Action = PaymentInstrumentActions.ToString(PIActionType.AddResource);
                actionContext.ResourceActionContext.Action = PaymentInstrumentActions.ToString(PIActionType.AddResource);

                return retVal;
            }

            // If addTriggeredBy feature property enabled then add the triggeredBy parnterHints or else return SelectResourceType action
            if (addTriggeredBy)
            {
                actionContext.PartnerHints = new PartnerHints() { TriggeredBy = PartnerHintsValues.TriggeredByEmptyResourceList };
            }
            else
            {
                actionContext.Action = PaymentInstrumentActions.ToString(PIActionType.SelectResourceType);
                actionContext.ResourceActionContext.Action = actionContext.Action;
            }

            return retVal;
        }
    }
}