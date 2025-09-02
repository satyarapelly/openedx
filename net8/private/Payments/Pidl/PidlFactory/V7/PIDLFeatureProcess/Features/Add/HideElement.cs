// <copyright file="HideElement.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the HideElement, which is to hide diplayhints in PIDL form.
    /// </summary>
    internal class HideElement : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                HideDisplayHints
            };
        }

        internal static void HideDisplayHints(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.HideElement, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    foreach (PIDLResource paymentMethodPidl in inputResources)
                    {
                        if (displayHintCustomizationDetail.HideAddCreditDebitCardHeading)
                        {
                            HideDisplayHint(paymentMethodPidl, Constants.HeadingDisplayHintIds.AddCreditDebitHeading);
                        }

                        if (displayHintCustomizationDetail.HideFirstAndLastNameForCompletePrerequisites
                            && paymentMethodPidl?.LinkedPidls != null)
                        {
                            foreach (var linkedPidl in paymentMethodPidl?.LinkedPidls)
                            {
                                HideDisplayHint(linkedPidl, Constants.DisplayHintIds.AddressFirstName);
                                HideDisplayHint(linkedPidl, Constants.DisplayHintIds.AddressMiddleName);
                                HideDisplayHint(linkedPidl, Constants.DisplayHintIds.AddressLastName);
                            }
                        }

                        if (displayHintCustomizationDetail.HidePaymentSummaryText)
                        {
                            HideDisplayHint(paymentMethodPidl, Constants.DisplayHintIds.PaymentSummaryText);
                        }

                        if (displayHintCustomizationDetail.HidepaymentOptionSaveText)
                        {
                            HideDisplayHint(paymentMethodPidl, Constants.DisplayHintIds.PaymentOptionSaveText);
                        }

                        if (displayHintCustomizationDetail.HidePaymentMethodHeading.HasValue && displayHintCustomizationDetail.HidePaymentMethodHeading.Value)
                        {
                            HideDisplayHint(paymentMethodPidl, Constants.DisplayHintIds.PaymentMethodHeading);
                        }

                        if (displayHintCustomizationDetail.HideChangeSettingText.HasValue && displayHintCustomizationDetail.HideChangeSettingText.Value)
                        {
                            HideDisplayHint(paymentMethodPidl, Constants.DisplayHintIds.PaymentChangeSettingsTextGroup);
                        }

                        if (displayHintCustomizationDetail.HideCountryDropdown == true)
                        {
                            HideDisplayHint(paymentMethodPidl, Constants.DisplayHintIds.AddressCountry);
                        }

                        if (displayHintCustomizationDetail.HideAcceptCardMessage == true)
                        {
                            HideDisplayHint(paymentMethodPidl, Constants.DisplayHintIds.AcceptCardMessage);
                        }

                        if (displayHintCustomizationDetail?.HideCardLogos == true)
                        {
                            HideCreditCardLogos(paymentMethodPidl);
                        }

                        if (displayHintCustomizationDetail?.HideAddress == true)
                        {
                            HideAddress(paymentMethodPidl, featureContext);
                        }
                    }
                }
            }
        }

        private static void HideCreditCardLogos(PIDLResource paymentMethodPidl)
        {
            var cardLogoDisplayHints = new List<string>
                            {
                                Constants.DisplayHintIds.AcceptedVisaCardGroup,
                                Constants.DisplayHintIds.AcceptedAmexCardGroup,
                                Constants.DisplayHintIds.AcceptedMCCardGroup,
                                Constants.DisplayHintIds.AcceptedDiscoverCardGroup,
                                Constants.DisplayHintIds.AcceptedJCBCardGroup,
                                Constants.DisplayHintIds.AcceptedEloCardGroup,
                                Constants.DisplayHintIds.AcceptedVerveCardGroup,
                                Constants.DisplayHintIds.AcceptedHipercardCardGroup,
                                Constants.DisplayHintIds.AcceptedCupInternationalCardGroup,
                                Constants.DisplayHintIds.CreditCardRupayLogo,
                                Constants.DisplayHintIds.CreditCardCupLogo
                            };

            paymentMethodPidl.HideDisplayHintsById(cardLogoDisplayHints);
        }

        private static void HideDisplayHint(PIDLResource resource, string hintId)
        {
            DisplayHint displayHint = resource.GetDisplayHintById(hintId);
            if (displayHint != null)
            {
                displayHint.IsHidden = true;
            }
        }

        private static void HideAddress(PIDLResource pidlResource, FeatureContext featureContext)
        {
            var addressDisplayHints = new List<string>
                            {
                                Constants.DisplayHintIds.AddressLine1,
                                Constants.DisplayHintIds.AddressLine2,
                                Constants.DisplayHintIds.AddressLine3,
                                Constants.DisplayHintIds.AddressCity,
                                Constants.DisplayHintIds.AddressState,
                                Constants.DisplayHintIds.AddressPostalCode,
                                Constants.DisplayHintIds.AddressCountry,
                                Constants.DisplayHintIds.NoProfileAddressText,                                
                                Constants.DisplayHintIds.AddressProvince,
                                Constants.DisplayHintIds.AddressStatePostalCodeGroup,
                                Constants.DisplayHintIds.AddressPostalCodeStateGroup,
                                Constants.DisplayHintIds.AddressProvincePostalCodeGroup,
                                Constants.DisplayHintIds.AddressPostalCodeProvinceGroup
                            };

            pidlResource.HideDisplayHintsById(addressDisplayHints);
        }
    }
}