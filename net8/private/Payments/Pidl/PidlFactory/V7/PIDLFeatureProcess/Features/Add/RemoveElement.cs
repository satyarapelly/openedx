// <copyright file="RemoveElement.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using static Microsoft.Commerce.Payments.PidlFactory.V7.Constants;

    /// <summary>
    /// Class representing the RemoveElement.
    /// </summary>
    internal class RemoveElement : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                RemoveDisplayHints
            };
        }

        internal static void RemoveDisplayHints(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig = null;
            featureContext?.FeatureConfigs?.TryGetValue(FeatureConfiguration.FeatureNames.RemoveElement, out featureConfig);

            if (featureConfig?.DisplayCustomizationDetail?.Count > 0)
            {
                foreach (PartnerSettingsModel.DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    foreach (PIDLResource paymentMethodPidl in inputResources)
                    {
                        if (displayHintCustomizationDetail != null && displayHintCustomizationDetail.RemoveCancelButton)
                        {
                            paymentMethodPidl.RemoveDisplayHintById(Constants.ButtonDisplayHintIds.CancelBackButton);
                        }

                        if (displayHintCustomizationDetail?.RemoveSelectPiEditButton != null && bool.Parse(displayHintCustomizationDetail?.RemoveSelectPiEditButton.ToString()))
                        {
                            PropertyDisplayHint paymentInstrumentList = paymentMethodPidl.GetDisplayHintById(Constants.DisplayHintIds.PaymentInstrumentSelect) as PropertyDisplayHint;

                            if (paymentInstrumentList != null)
                            {
                                foreach (var possibleOption in paymentInstrumentList.PossibleOptions)
                                {
                                    ActionContext context = possibleOption.Value.PidlAction.Context as ActionContext;
                                    if (context.ResourceActionContext != null)
                                    {
                                        GroupDisplayHint optionDisplayGroup = possibleOption.Value.DisplayContent.Members.Last() as GroupDisplayHint;
                                        optionDisplayGroup.Members.Remove(optionDisplayGroup.Members.Last()); // remove edit hyperlink, should be temp change
                                    }
                                }
                            }
                        }

                        if (displayHintCustomizationDetail?.RemoveSelectPiNewPaymentMethodLink == true)
                        {
                            if (string.Equals(featureContext.OperationType, V7.Constants.PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (PIDLResource pidlResource in inputResources)
                                {
                                    PropertyDisplayHint paymentInstrument = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentInstrumentSelect) as PropertyDisplayHint;

                                    if (paymentInstrument != null && paymentInstrument.PossibleOptions != null && paymentInstrument.PossibleOptions.ContainsKey(Constants.DisplayHintIds.NewPaymentMethodLink))
                                    {
                                        paymentInstrument.PossibleOptions.Remove(Constants.DisplayHintIds.NewPaymentMethodLink);
                                    }

                                    paymentMethodPidl.RemoveDisplayHintById(Constants.DisplayHintIds.NewPaymentMethodLink);
                                }
                            }
                        }

                        if (displayHintCustomizationDetail?.RemoveEwalletYesButtons != null && bool.Parse(displayHintCustomizationDetail?.RemoveEwalletYesButtons.ToString()))
                        {
                            string type = featureContext.PaymentMethodType ?? string.Empty;

                            switch (type)
                            {
                                case Constants.PaymentMethodTypeNames.Paypal:
                                    inputResources[0].RemoveDisplayHintById("paypalYesButton");
                                    break;
                                case Constants.PaymentMethodTypeNames.Venmo:
                                    inputResources[0].RemoveDisplayHintById("venmoYesButton");
                                    break;
                            }
                        }

                        if (displayHintCustomizationDetail?.RemoveSpaceInPrivacyTextGroup != null && bool.Parse(displayHintCustomizationDetail.RemoveSpaceInPrivacyTextGroup.ToString()))
                        {
                            paymentMethodPidl.RemoveDisplayHintById(Constants.DisplayHintIds.Space);
                            string type = featureContext.PaymentMethodType ?? string.Empty;

                            // if the payment method is Paysafecard, remove the space element from the client action context
                            if (string.Equals(type, Constants.PaymentMethodTypeNames.Paysafecard, StringComparison.OrdinalIgnoreCase))
                            {
                                var clientActionContext = inputResources[0]?.ClientAction?.Context as List<PIDLResource>;
                                if (clientActionContext != null)
                                {
                                    clientActionContext[0]?.RemoveDisplayHintById(Constants.DisplayHintIds.Space);
                                }
                            }
                        }

                        if (displayHintCustomizationDetail?.RemoveAddCreditDebitCardHeading == true)
                        {
                            paymentMethodPidl.RemoveDisplayHintById(Constants.HeadingDisplayHintIds.AddCreditDebitHeading);
                        }

                        if (displayHintCustomizationDetail?.RemoveStarRequiredTextGroup == true)
                        {
                            paymentMethodPidl.RemoveDisplayHintById(Constants.DisplayHintIds.StarRequiredTextGroup);
                        }

                        if (displayHintCustomizationDetail?.RemoveEwalletBackButtons != null && bool.Parse(displayHintCustomizationDetail.RemoveEwalletBackButtons.ToString()))
                        {
                            string type = featureContext.PaymentMethodType ?? string.Empty;

                            switch (type)
                            {
                                case Constants.PaymentMethodTypeNames.Venmo:
                                    inputResources[0].RemoveDisplayHintById("venmoQrCodeChallengePageBackButton");
                                    inputResources[0].RemoveDisplayHintById("backMoveFirstButton");
                                    break;

                                case Constants.PaymentMethodTypeNames.Paypal:
                                    inputResources[0].RemoveDisplayHintById("paypalQrCodeChallengePageBackButton");
                                    inputResources[0].RemoveDisplayHintById("backButton");
                                    break;

                                case Constants.PaymentMethodTypeNames.Paysafecard:
                                    var clientActionContext = inputResources[0]?.ClientAction?.Context as List<PIDLResource>;
                                    if (clientActionContext != null)
                                    {
                                        clientActionContext[0]?.RemoveDisplayHintById("globalPIQrCodeMovePrevCancelButton");
                                    }

                                    break;
                            }
                        }

                        if (displayHintCustomizationDetail?.RemoveAddressFormHeading == true)
                        {
                            paymentMethodPidl.RemoveDisplayHintById(Constants.HeadingDisplayHintIds.BillingAddressPageHeading);
                        }

                        if (displayHintCustomizationDetail?.RemoveAcceptCardMessage == true)
                        {
                            paymentMethodPidl.RemoveDisplayHintById(Constants.DisplayHintIds.AcceptCardMessage);
                        }

                        if (displayHintCustomizationDetail?.RemoveMicrosoftPrivacyTextGroup == true)
                        {
                            paymentMethodPidl.RemoveDisplayHintById(Constants.DisplayHintIds.MicrosoftPrivacyTextGroup);
                        }
                    }
                }
            }
        }
    }
}