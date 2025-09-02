// <copyright file="SetDefaultPaymentMethod.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;

    internal class SetDefaultPaymentMethod : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>> 
            {
                SetPaymentMethodAsDefault
            };
        }

        internal static void SetPaymentMethodAsDefault(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (inputResources == null || string.IsNullOrEmpty(featureContext?.DefaultPaymentMethod) || featureContext?.ResourceType != Constants.ResourceTypes.PaymentMethod || FeatureConfiguration.IsEnabledUsingPartnerSettings(FeatureConfiguration.FeatureNames.PaymentMethodGrouping, featureContext))
            {
                return;
            }

            foreach (PIDLResource resource in inputResources)
            {
                if (resource?.DisplayPages == null)
                {
                    continue;
                }

                HeadingDisplayHint paymentMethodSelectHeading = resource.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodSelectHeading) as HeadingDisplayHint;
                PropertyDisplayHint paymentMethodProperty = resource.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodSelect) as PropertyDisplayHint;
                string defaultPaymentMethod = featureContext.DefaultPaymentMethod;

                if (paymentMethodSelectHeading == null || paymentMethodProperty == null || paymentMethodProperty.SelectType != Constants.PaymentMethodSelectType.ButtonList)
                {
                    continue;
                }

                PaymentMethod parsedDefaultPaymentMethod = new PaymentMethod();
                HashSet<PaymentMethod> paymentMethods = featureContext?.PaymentMethods ?? new HashSet<PaymentMethod>();
                parsedDefaultPaymentMethod.PaymentMethodFamily = defaultPaymentMethod?.Split('.')?.FirstOrDefault() ?? string.Empty;
                parsedDefaultPaymentMethod.PaymentMethodType = defaultPaymentMethod?.Split('.')?.Skip(1)?.FirstOrDefault() ?? string.Empty;
                string defaultPossibleValue = null;

                if (!string.IsNullOrEmpty(parsedDefaultPaymentMethod.PaymentMethodFamily) && !string.IsNullOrEmpty(parsedDefaultPaymentMethod.PaymentMethodType))
                {
                    defaultPossibleValue = PaymentSelectionHelper.GetPaymentMethodFamilyTypeDisplayId(parsedDefaultPaymentMethod);
                }
                else if (!string.IsNullOrEmpty(parsedDefaultPaymentMethod.PaymentMethodFamily) && PaymentSelectionHelper.IsCollapsedPaymentMethodOption(parsedDefaultPaymentMethod))
                {
                    defaultPossibleValue = PaymentSelectionHelper.GetPaymentMethodFamilyTypeDisplayId(
                        string.Format("{0}_{1}", parsedDefaultPaymentMethod.PaymentMethodFamily, PaymentSelectionHelper.GetCommaSeparatedTypes(parsedDefaultPaymentMethod.PaymentMethodFamily, featureContext?.PaymentMethods)));
                }

                if (!string.IsNullOrEmpty(defaultPossibleValue))
                {
                    PaymentSelectionHelper.UpdateSelectDisplay(resource, paymentMethodProperty.HintId, Constants.DataDescriptionIds.PaymentInstrumentId, paymentMethodProperty.PossibleValues, paymentMethodProperty.PossibleOptions, defaultPossibleValue, defaultPossibleValue != null);

                    // UpdateSelectDisplay method will set the IsSelectFirstItem property to true when defaultPaymentMethod is selected as first option in possibleOptions.
                    // This property doesn't have any effect when displayHint selectType is ButtonList. Overriding the property value to null.
                    paymentMethodProperty.IsSelectFirstItem = null;

                    // After modifying the possibleOptions, defaultPaymentMethod option will be in the first position. And the previous first option will be in the second position.
                    // Update the accessibilityTags of first and second options in possibleOptions to get the correct narrator announcements.
                    if (paymentMethodProperty.PossibleOptions != null && paymentMethodProperty.PossibleOptions.Count > 1 && paymentMethodSelectHeading != null)
                    {
                        var firstOption = paymentMethodProperty.PossibleOptions.FirstOrDefault().Value;
                        var secondOption = paymentMethodProperty.PossibleOptions.Skip(1).FirstOrDefault().Value;

                        if (firstOption?.DisplayText != null)
                        {
                            firstOption.AccessibilityTag = PidlModelHelper.GetLocalizedString($"{Constants.AccessibilityLabels.PickAPaymentMethod}. {firstOption.DisplayText}");
                        }

                        if (secondOption?.DisplayText != null)
                        {
                            secondOption.AccessibilityTag = PidlModelHelper.GetLocalizedString($"{secondOption.DisplayText}");
                        }
                    }
                }
            }
        }
    }
}
