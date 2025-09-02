// <copyright file="ChallengeDisplayHelper.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class contains helper functions for populating PI specific fields of challenge pidls
    /// </summary>
    public static class ChallengeDisplayHelper
    {
        public static void PopulateChallengePidl(PIDLResource pidlResource, PaymentInstrument pi, string challengeType, string partnerName, string language, string sessionId, object session, string expiryPrefix = null, string scenario = null, List<string> exposedFlightFeatures = null, PaymentExperienceSetting setting = null)
        {
            string country = string.Empty;

            if (session != null)
            {
                var sessionJObject = JObject.FromObject(session);
                if (sessionJObject != null)
                {
                    country = sessionJObject?.GetValue("country")?.ToString();
                }
            }

            PaymentInstrumentDetails details = pi.PaymentInstrumentDetails;
            if (string.Equals(challengeType, Constants.ChallengeDescriptionTypes.Sms))
            {
                TextDisplayHint smsChallengeText = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.SmsChallengeText) as TextDisplayHint;
                if (smsChallengeText != null)
                {
                    smsChallengeText.DisplayContent = string.Format(smsChallengeText.DisplayContent, GetPhoneNumberFromPi(pi));
                }

                RestLink sendNewCodeUrlLink = new RestLink()
                {
                    Href = @"https://{pifd-endpoint}/users/{userId}/challenge/sms",
                    Method = Constants.HTTPVerbs.POST,
                    Payload = new SmsChallengeData(pi.PaymentInstrumentId, sessionId)
                };

                DisplayHint newCodeLink = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.SmsNewCodeLink);
                if (newCodeLink != null)
                {
                    newCodeLink.Action.Context = sendNewCodeUrlLink;
                }

                ButtonDisplayHint nextButton = pidlResource.GetDisplayHintById(Constants.ButtonDisplayHintIds.OkActionButton) as ButtonDisplayHint;
                if (nextButton != null)
                {
                    nextButton.Action.Context = sendNewCodeUrlLink;
                    nextButton.Action.NextAction = new DisplayHintAction(DisplayHintActionType.moveNext.ToString());
                }

                // Add accessibility labels to xbox native partners
                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                {
                    SetButtonAccessibilityNameWithOrder(pidlResource, Constants.ButtonDisplayHintIds.CancelButton, Constants.XboxNativeChallengeAccessibilityLabels.Cancel, language, 1, 2, pidlResource.DisplayPages[0]);
                    SetButtonAccessibilityNameWithOrder(pidlResource, Constants.ButtonDisplayHintIds.OkActionButton, Constants.XboxNativeChallengeAccessibilityLabels.OK, language, 2, 2);
                    SetButtonAccessibilityNameWithOrder(pidlResource, Constants.ButtonDisplayHintIds.OkButton, Constants.XboxNativeChallengeAccessibilityLabels.OK, language, 1, 3);
                    SetButtonAccessibilityNameWithOrder(pidlResource, Constants.ButtonDisplayHintIds.CancelButton, Constants.XboxNativeChallengeAccessibilityLabels.Cancel, language, 2, 3, pidlResource.DisplayPages[1]);
                    SetButtonAccessibilityNameWithOrder(pidlResource, Constants.ButtonDisplayHintIds.ViewTermsButton, Constants.XboxNativeChallengeAccessibilityLabels.ViewTermsButton, language, 3, 3);
                }
            }
            else if (string.Equals(challengeType, Constants.ChallengeDescriptionTypes.ThreeDS))
            {
                ButtonDisplayHint nextButton = pidlResource.GetDisplayHintById(Constants.ButtonDisplayHintIds.ContinueRedirectButton) as ButtonDisplayHint;
                if (nextButton != null)
                {
                    // TODO Task 16155749: Return correct 3DS redirect URL for get challenge description by piid and type
                    nextButton.Action.Context = string.Format(@"https://bing.com?sessionId={0}", sessionId);
                    nextButton.Action.NextAction = new DisplayHintAction(DisplayHintActionType.moveNext.ToString());
                }
            }
            else if (string.Equals(challengeType, Constants.ChallengeDescriptionTypes.Cvv))
            {
                ImageDisplayHint cardLogo = pidlResource.GetDisplayHintById(Constants.ChallengeDisplayHintIds.CardLogo) as ImageDisplayHint;
                if (cardLogo != null)
                {
                    if (cardLogo.DisplayTags == null || !cardLogo.DisplayTags.ContainsKey("accessibilityName"))
                    {
                        cardLogo.AddDisplayTag("accessibilityName", pi?.PaymentMethod?.Display.Name);
                    }

                    // For react-native partners, some svgs provided in the PM or PI object are not compatible and will not render and need to be replaced
                    if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) && string.Equals(scenario, Constants.ScenarioNames.IndiaThreeDS))
                    {
                        string alternateSVG = PaymentSelectionHelper.CheckForReactNativeAlternatePaymentMethodLogoUrl(pi.PaymentMethod, partnerName, exposedFlightFeatures);
                        cardLogo.SourceUrl = alternateSVG ?? PaymentSelectionHelper.GetPaymentMethodLogoUrl(pi.PaymentMethod);
                    }
                    else
                    {
                        cardLogo.SourceUrl = PaymentSelectionHelper.GetPaymentMethodLogoUrl(pi.PaymentMethod);
                    }
                }

                TextDisplayHint cardName = pidlResource.GetDisplayHintById(Constants.ChallengeDisplayHintIds.CardName) as TextDisplayHint;
                if (cardName != null)
                {
                    cardName.DisplayContent = details.CardHolderName;
                }

                TextDisplayHint cardNumber = pidlResource.GetDisplayHintById(Constants.ChallengeDisplayHintIds.CardNumber) as TextDisplayHint;
                if (cardNumber != null)
                {
                    if (PaymentSelectionHelper.IsBingTravelPartner(partnerName))
                    {
                        cardNumber.DisplayContent = string.Format("{0} \u2022\u2022\u2022\u2022 {1}", pi.PaymentMethod.Display.Name, details.LastFourDigits);
                    }
                    else
                    {
                        cardNumber.DisplayContent = string.Format("\u2022\u2022{0}", details.LastFourDigits);
                    }
                }

                TextDisplayHint cardExpiry = pidlResource.GetDisplayHintById(Constants.ChallengeDisplayHintIds.CardExpiry) as TextDisplayHint;
                if (cardExpiry != null)
                {
                    if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) && !string.IsNullOrEmpty(expiryPrefix))
                    {
                        cardExpiry.DisplayContent = string.Format("{0} {1}/{2}", expiryPrefix, details.ExpiryMonth, details.ExpiryYear);
                    }
                    else if (PaymentSelectionHelper.IsBingTravelPartner(partnerName))
                    {
                        cardExpiry.DisplayContent = string.Format("{0} {1}/{2}", Constants.ExpiryPrefixes.Expiry, int.Parse(details.ExpiryMonth).ToString("00"), details.ExpiryYear);
                    }
                    else if (!string.IsNullOrEmpty(country) && string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase) 
                        && exposedFlightFeatures.Contains(Flighting.Features.IndiaExpiryGroupDelete, StringComparer.OrdinalIgnoreCase) && exposedFlightFeatures.Contains(Flighting.Features.IndiaCvvChallengeExpiryGroupDelete, StringComparer.OrdinalIgnoreCase))
                    {
                        cardExpiry.DisplayContent = PidlModelHelper.GetLocalizedString(PXCommon.Constants.ExpiryDate.IndiaCvvChallengeExpiryDateMasked, language);
                    }
                    else
                    {
                        cardExpiry.DisplayContent = string.Format("{0}/{1}", details.ExpiryMonth, details.ExpiryYear);
                    }
                }

                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                {
                    HeadingDisplayHint cvvChallengeHeading = pidlResource.GetDisplayHintById(Constants.ChallengeDisplayHintIds.CvvChallengeHeading) as HeadingDisplayHint;

                    if (cvvChallengeHeading != null)
                    {
                        string type = pi.PaymentMethod.PaymentMethodType;
                        string content = cvvChallengeHeading.DisplayContent;

                        string capitalizedType;
                        if (string.Equals(type, Constants.PaymentMethodTypeNames.JapanCreditBureau))
                        {
                            capitalizedType = type.ToUpper();
                        }
                        else
                        {
                            capitalizedType = string.Format("{0}{1}", char.ToUpper(type[0]), type.Substring(1));
                        }

                        content = content.Replace("{type placeholder}", capitalizedType);
                        content = content.Replace("{lastFour placeholder}", $"**{details.LastFourDigits}");

                        cvvChallengeHeading.DisplayContent = content;
                    }

                    SetButtonAccessibilityNameWithOrder(pidlResource, Constants.ButtonDisplayHintIds.SubmitButton, Constants.XboxNativeChallengeAccessibilityLabels.Submit, language, 1, 2);
                    SetButtonAccessibilityNameWithOrder(pidlResource, Constants.ButtonDisplayHintIds.CancelBackButton, Constants.XboxNativeChallengeAccessibilityLabels.Cancel, language, 2, 2);
                }
                else if (PXCommon.Constants.PartnerGroups.IsWindowsNativePartner(partnerName) && setting?.Template == null)
                {
                    pidlResource.UpdateIsKeyProperty(Constants.ChallengeDisplayHintIds.CvvToken, true);

                    HeadingDisplayHint cvvChallengeText = pidlResource.GetDisplayHintById(Constants.ChallengeDisplayHintIds.CvvChallengeText) as HeadingDisplayHint;

                    if (cvvChallengeText != null)
                    {
                        string type = pi.PaymentMethod.PaymentMethodType;
                        string content = cvvChallengeText.DisplayContent;

                        string capitalizedType;
                        if (string.Equals(type, Constants.PaymentMethodTypeNames.JapanCreditBureau))
                        {
                            capitalizedType = type.ToUpper();
                        }
                        else
                        {
                            capitalizedType = string.Format("{0}{1}", char.ToUpper(type[0]), type.Substring(1));
                        }

                        content = content.Replace("{type placeholder}", capitalizedType);
                        content = content.Replace("{lastFour placeholder}", $"**{details.LastFourDigits}");

                        cvvChallengeText.DisplayContent = content;
                    }

                    PropertyDisplayHint challengeCvv = pidlResource.GetDisplayHintById(Constants.ChallengeDisplayHintIds.ChallengeCvv) as PropertyDisplayHint;

                    if (challengeCvv != null)
                    {
                        challengeCvv.DisplayDescription = PidlModelHelper.GetLocalizedString("Code", language);
                    }

                    SetButtonAccessibilityNameWithOrder(pidlResource, Constants.ButtonDisplayHintIds.SubmitButton, Constants.XboxNativeChallengeAccessibilityLabels.Submit, language, 1, 2);
                    SetButtonAccessibilityNameWithOrder(pidlResource, Constants.ButtonDisplayHintIds.CancelBackButton, Constants.XboxNativeChallengeAccessibilityLabels.Cancel, language, 2, 2);
                }

                if ((PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) || string.Equals(partnerName, Constants.PartnerNames.Xbox, StringComparison.OrdinalIgnoreCase)) && session != null)
                {
                    // For 3DS1 challenge, attach the payment session to the back button so we can return it in the callback to
                    // the partner
                    ButtonDisplayHint cancelButton = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.SuccessBackButton) as ButtonDisplayHint;
                    if (cancelButton != null)
                    {
                        cancelButton.Action.Context = session;
                    }
                }

                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) && string.Equals(scenario, Constants.ScenarioNames.IndiaThreeDS))
                {
                    Set3DS1XboxNativeCVVAccessibilityLabels(pidlResource, language, pi);
                }
            }
        }

        public static string GetPhoneNumberFromPi(PaymentInstrument pi)
        {
            string phoneNumber = string.Empty;
            if (!string.IsNullOrEmpty(pi.PaymentInstrumentDetails.Phone))
            {
                phoneNumber = pi.PaymentInstrumentDetails.Phone;
            }
            else if (!string.IsNullOrEmpty(pi.PaymentInstrumentDetails.Msisdn))
            {
                phoneNumber = pi.PaymentInstrumentDetails.Msisdn;
            }

            return phoneNumber;
        }

        public static void Populate3DS1XboxNativeQrCodeAccessibilityLabels(PIDLResource pidlResource, string language)
        {
            // Instruction Page
            SetButtonAccessibilityNameWithOrder(pidlResource, Constants.DisplayHintIds.MoveNext2Button, Constants.XboxNative3DS1AccessibilityLabels.Next, language, 1, 3);
            SetButtonAccessibilityNameWithOrder(pidlResource, Constants.DisplayHintIds.SuccessBackButton, Constants.XboxNative3DS1AccessibilityLabels.Back, language, 2, 3);
            SetButtonAccessibilityNameWithOrder(pidlResource, Constants.DisplayHintIds.CC3DSPurchaseViewTermsButton, Constants.XboxNative3DS1AccessibilityLabels.PrivacyStatement, language, 3, 3);

            // QR code page
            SetButtonAccessibilityNameWithOrder(pidlResource, Constants.DisplayHintIds.MoveBack2Button, Constants.XboxNative3DS1AccessibilityLabels.Back, language, 1, 2);
            SetButtonAccessibilityNameWithOrder(pidlResource, Constants.DisplayHintIds.GoToBankButton, Constants.XboxNative3DS1AccessibilityLabels.GoToBank, language, 2, 2);
        }

        private static void Set3DS1XboxNativeCVVAccessibilityLabels(PIDLResource pidlResource, string language, PaymentInstrument pi)
        {
            // 3DS1 purchase for xbox native partners received accessibility feedback
            // https://dev.azure.com/microsoft/OSGS/_workitems/edit/38178914
            PropertyDisplayHint cvv = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.CVV) as PropertyDisplayHint;
            if (cvv != null)
            {
                string localizedOrderText = PidlModelHelper.GetLocalizedString(Constants.XboxNative3DS1AccessibilityLabels.TextBoxAccessibilityOrder, language);
                string formattedOrderText = string.Format(localizedOrderText, 1, 1);
                string localizedCVVAccessibilityLabel = PidlModelHelper.GetLocalizedString(Constants.XboxNative3DS1AccessibilityLabels.CVVTextBoxAccessibilityLabel, language);
                string formattedCVVAccessibilityLabel = string.Format(localizedCVVAccessibilityLabel, pi.PaymentMethod.PaymentMethodType, pi.PaymentInstrumentDetails.LastFourDigits);

                cvv.AddOrUpdateDisplayTag("accessibilityName", $"{formattedCVVAccessibilityLabel} {formattedOrderText}");
            }

            SetButtonAccessibilityNameWithOrder(pidlResource, Constants.DisplayHintIds.CVV3DSSubmitButton, Constants.XboxNative3DS1AccessibilityLabels.SubmitButtonAccessibilityLabel, language, 1, 2);

            SetButtonAccessibilityNameWithOrder(pidlResource, Constants.DisplayHintIds.SuccessBackButton, Constants.XboxNative3DS1AccessibilityLabels.Back, language, 2, 2);
        }

        private static void SetButtonAccessibilityNameWithOrder(PIDLResource pidlResource, string displayHintId, string accessibilityLabel, string language, int index, int total, ContainerDisplayHint displayPage = null)
        {
            ButtonDisplayHint button = new ButtonDisplayHint(); 
            if (displayPage != null)
            {
                // for flows with two of the same buttons in one pidl. ie. 2 pages, each page has a cancel button
                button = pidlResource.GetDisplayHintFromContainer(displayPage, displayHintId) as ButtonDisplayHint;
            }
            else
            {
                button = pidlResource.GetDisplayHintById(displayHintId) as ButtonDisplayHint;
            }

            if (button != null)
            {
                string localizedOrderText = PidlModelHelper.GetLocalizedString(Constants.XboxNative3DS1AccessibilityLabels.ButtonAccessibilityOrder, language);
                string formattedOrderText = string.Format(localizedOrderText, index, total);
                string localizedButtonAccessibilityLabel = PidlModelHelper.GetLocalizedString(accessibilityLabel, language);

                button.AddOrUpdateDisplayTag("accessibilityName", $"{localizedButtonAccessibilityLabel} {formattedOrderText}");
            }
        }
    }
}
