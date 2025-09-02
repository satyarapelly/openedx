// <copyright file="PaymentSelectionHelper.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using static Microsoft.Commerce.Payments.PidlFactory.V7.Constants;

    /// <summary>
    /// This class contains helper functions for populating payment select PIDLs
    /// </summary>
    public static class PaymentSelectionHelper
    {
        private static readonly Dictionary<string, int> MaxAllowedLogosPerSelectOptionForPartner = new Dictionary<string, int>
        {
            { Constants.PartnerNames.Storify, Constants.MaxAllowedPaymentMethodLogos.Six },
            { Constants.PartnerNames.XboxSettings, Constants.MaxAllowedPaymentMethodLogos.Six },
            { Constants.PartnerNames.XboxSubs, Constants.MaxAllowedPaymentMethodLogos.Six },
            { Constants.PartnerNames.Saturn, Constants.MaxAllowedPaymentMethodLogos.Six },
            { Constants.PartnerNames.AmcWeb, Constants.MaxAllowedPaymentMethodLogos.Six },
        };

        private static readonly Dictionary<string, string> AlternateImageLinkLookupTable = new Dictionary<string, string>()
        {
            { Constants.PaymentMethodTypeNonSimMobileNames.TdcDenmark, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.TdcDenmark },
            { Constants.PaymentMethodTypeNonSimMobileNames.KpnNetherlands, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.KpnNetherlands },
            { Constants.PaymentMethodTypeNonSimMobileNames.NetNorway, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.NetNorway },
            { Constants.PaymentMethodTypeNonSimMobileNames.AtOneAustria, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.AtOneAustria },
            { Constants.PaymentMethodTypeNonSimMobileNames.DigiMalaysia, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.DigiMalaysia },
            { Constants.PaymentMethodTypeNonSimMobileNames.M1Singapore, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.M1Singapore },
            { Constants.PaymentMethodTypeNonSimMobileNames.OrangeSpain, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.OrangeSpain },
            { Constants.PaymentMethodTypeNonSimMobileNames.SunriseSwitzerland, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.SunriseSwitzerland },
            { Constants.PaymentMethodTypeNonSimMobileNames.StarHubSingapore, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.StarHubSingapore },
            { Constants.PaymentMethodTypeNonSimMobileNames.TmobileTeleringAustria, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.TmobileTeleringAustria },
            { Constants.PaymentMethodTypeNonSimMobileNames.TmobileCzechia, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.TmobileCzechia },
            { Constants.PaymentMethodTypeNonSimMobileNames.TmobileUnitedKingdom, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.TmobileUnitedKingdom },
            { Constants.PaymentMethodTypeNonSimMobileNames.TmobileGermany, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.TmobileGermany },
            { Constants.PaymentMethodTypeNonSimMobileNames.TmobileNetherlands, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.TmobileNetherlands },
            { Constants.PaymentMethodTypeNonSimMobileNames.TmobileSlovakia, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.TmobileSlovakia },
            { Constants.PaymentMethodTypeNonSimMobileNames.ZainSouthAfrica, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.ZainSouthAfrica },
            { Constants.PaymentMethodTypeNonSimMobileNames.MtcRussia, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.MtcRussia },
            { Constants.PaymentMethodTypeNonSimMobileNames.ProBelgium, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.ProBelgium },
            { Constants.PaymentMethodTypeNonSimMobileNames.TliDenmark, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.TliDenmark },
            { Constants.PaymentMethodTypeNonSimMobileNames.VivoBrazil, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.VivoBrazil },
            { Constants.PaymentMethodTypeNonSimMobileNames.EraPoland, Constants.PaymentMethodTypeNonSimAlternateLogoSvg.EraPoland },
            { Constants.PaymentMethodTypeNames.MasterCard, Constants.StaticResourceNames.MasterCardLogoLeftAligned },
            { Constants.PaymentMethodTypeNames.Paysafecard, Constants.PaymentMethodTypePaysafeAlternateLogoSvg.PaysafeCard },
            { Constants.PaymentMethodTypeNames.Visa, Constants.StaticResourceNames.VisaSvg },
            { Constants.PaymentMethodTypeNames.Verve, Constants.StaticResourceNames.VervePng },
            { Constants.PaymentMethodTypeNames.Venmo, Constants.StaticResourceNames.VenmoSvg }
        };

        private static readonly Dictionary<string, string> DisplayHintIdIconLookUpTable = new Dictionary<string, string>()
        {
            { Constants.DisplayHintIds.RedeemGiftCardLink, Constants.FontIcons.GiftCard },
            { Constants.DisplayHintIds.NewPaymentMethodLink, Constants.FontIcons.PlusSign },
        };

        public static ImageDisplayHint BuildLogoElementForSelectionInstance(PaymentInstrument selectionInstance)
        {
            ImageDisplayHint logo = new ImageDisplayHint
            {
                SourceUrl = GetPaymentInstrumentLogoUrl(selectionInstance),
                HintId = V7.Constants.DisplayHintIdPrefixes.PaymentOptionLogo + selectionInstance.PaymentInstrumentId,
                StyleHints = new List<string>() { "image-small" }
            };
            string logoName = selectionInstance.PaymentMethod.Display.Name;
            logo.AddDisplayTag(V7.Constants.DiplayHintProperties.AccessibilityName, logoName);

            return logo;
        }

        // This method is used to get the card holder name and last four digits of the card number with expiry details for the list of PIs.
        public static TextGroupDisplayHint BuildTextGroupElementForSelectionInstance(PaymentInstrument selectionInstance, FeatureContext postProcessingParams)
        {
            TextGroupDisplayHint textGroup = new TextGroupDisplayHint { HintId = Constants.DisplayHintIdPrefixes.PaymentOptionTextGroup + selectionInstance.PaymentInstrumentId, LayoutOrientation = Constants.PartnerHintsValues.InlinePlacement, StyleHints = new List<string> { "gap-small", "align-vertical-center" } };
            TextDisplayHint displayText = new TextDisplayHint
            {
                HintId = Constants.DisplayHintIdPrefixes.PaymentOptionText + selectionInstance.PaymentInstrumentId,
                DisplayContent = BuildPiDefaultDisplayName(selectionInstance, postProcessingParams.Partner, postProcessingParams.Country, showExpiryMonthAndYear: true)
            };

            textGroup.Members.Add(displayText);
            if (ExpiryActionNeeded(selectionInstance) && IsCreditCardNotCup(selectionInstance.PaymentMethod) && !string.Equals(postProcessingParams.Country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase))
            {
                TextDisplayHint displayTextExpiry = new TextDisplayHint { HintId = Constants.DisplayHintIdPrefixes.PaymentOptionText + selectionInstance.PaymentInstrumentId, DisplayContent = PidlModelHelper.GetLocalizedString("Expired") };
                displayTextExpiry.StyleHints = new List<string>() { "text-alert" };
                textGroup.Members.Add(displayTextExpiry);
            }

            return textGroup;
        }

        public static GroupDisplayHint BuildGroupElementForSelectInstanceDeletePI(PaymentInstrument selectionInstance, FeatureContext postProcessingParams, string displayText)
        {
            TextDisplayHint displayTextHint = new TextDisplayHint
            {
                HintId = DisplayHintIdPrefixes.PaymentOptionText + selectionInstance.PaymentInstrumentId,
                DisplayContent = displayText,
                DisplayTags = new Dictionary<string, string>
                {
                    { DisplayTag.LabelText, DisplayTag.LabelText }
                },
            };

            TextGroupDisplayHint textGroup = new TextGroupDisplayHint
            {
                HintId = DisplayHintIdPrefixes.PaymentOptionTextGroup + selectionInstance.PaymentInstrumentId,
            };
            textGroup.Members.Add(displayTextHint);

            ButtonDisplayHint deleteButton = new ButtonDisplayHint
            {
                HintId = DisplayHintIdPrefixes.PaymentOptionUpdate + selectionInstance.PaymentInstrumentId,
                DisplayContent = PidlModelHelper.GetLocalizedString(UnlocalizedDisplayText.DeleteButtonDisplayText),
                DisplayHintType = HintType.Button.ToString().ToLower(),
                Action = new DisplayHintAction
                {
                    ActionType = ActionType.Success,
                    IsDefault = false,
                    Context = new ActionContext
                    {
                        Id = selectionInstance.PaymentInstrumentId,
                        Action = ActionType.DeletePaymentInstrument,
                        ResourceActionContext = new ResourceActionContext
                        {
                            Action = ActionType.DeletePaymentInstrument,
                            ResourceInfo = new ResourceInfo
                            {
                                Id = selectionInstance.PaymentInstrumentId
                            }
                        },
                        PartnerHints = new PartnerHints
                        {
                            TriggeredBy = PartnerHintsValues.TriggeredByUpdateButton
                        }
                    }
                },
                DisplayTags = new Dictionary<string, string>
                {
                    { DisplayTag.AccessibilityName, string.Format(PidlModelHelper.GetLocalizedString(UnlocalizedDisplayText.DeleteButtonAccessibilityName), displayText) },
                    { DisplayTag.ActionTrigger, DisplayTag.ActionTrigger },
                    { DisplayTag.AutoHeight, DisplayTag.AutoHeight }
                },
            };

            var displayContent = new GroupDisplayHint
            {
                HintId = DisplayHintIdPrefixes.PaymentOptionContainer + selectionInstance.PaymentInstrumentId,
                LayoutOrientation = PartnerHintsValues.InlinePlacement,
                DisplayTags = new Dictionary<string, string>()
                {
                    { DisplayTag.SpaceBetween, DisplayTag.SpaceBetween }
                },
            };
            displayContent.Members.Add(textGroup);
            displayContent.Members.Add(deleteButton);

            return displayContent;
        }

        /// <summary>
        /// Splits the card holder's first and last name from the last four digits and expiry information of a payment card
        /// into two separate lines.
        /// </summary>
        /// <param name="option">The SelectOptionDescription containing possible payment instruments.</param>
        /// <param name="postProcessingParams">The FeatureContext containing feature information.</param>
        /// <returns>A GroupDisplayHint with the split card holder name and related information for the list of PIs.</returns>
        public static GroupDisplayHint SplitInfoInTwoLines(SelectOptionDescription option, FeatureContext postProcessingParams)
        {
            ActionContext context = option.PidlAction.Context as ActionContext;
            PaymentInstrument selectionInstance = context.Instance as PaymentInstrument;

            TextGroupDisplayHint textGroup = new TextGroupDisplayHint
            {
                HintId = Constants.DisplayHintIdPrefixes.PaymentOptionTextGroup + selectionInstance.PaymentInstrumentId,
                LayoutOrientation = Constants.PartnerHintsValues.InlinePlacement,
                StyleHints = new List<string> { "gap-small", "align-vertical-center" }
            };

            var optionDisplayHint = option?.DisplayContent.Members[1] as TextGroupDisplayHint;
            var paymentInstrumentTextDisplayHint = optionDisplayHint.Members[0] as TextDisplayHint;
            var paymentInstrumentText = paymentInstrumentTextDisplayHint?.DisplayContent;

            if (!string.IsNullOrEmpty(paymentInstrumentText) && paymentInstrumentText.Contains("••"))
            {
                GroupDisplayHint newGroupWithInfoSplitIntTwo = new GroupDisplayHint
                {
                    HintId = Constants.DisplayHintIdPrefixes.PaymentOptionText + selectionInstance.PaymentInstrumentId,
                };

                string[] splitDisplayContentInTwo = paymentInstrumentText.Split(new string[] { "••" }, StringSplitOptions.None);

                // Create a hint for the card holder's name
                TextDisplayHint cardHolderName = new TextDisplayHint { DisplayContent = splitDisplayContentInTwo[0] };

                // Create a hint for the last four digits and expiry information
                TextDisplayHint lastFourDigitsWithExpiry = new TextDisplayHint { DisplayContent = " ••" + splitDisplayContentInTwo[1] };

                newGroupWithInfoSplitIntTwo.Members.Add(cardHolderName);

                // Check if the "showPIExpirationInformation" feature is enabled and if there are
                // Expired memebrs are present in the display content
                if (postProcessingParams.FeatureConfigs.ContainsKey("showPIExpirationInformation") &&
                    (optionDisplayHint.Members.Count >= 2))
                {
                    GroupDisplayHint expiryInfoGroupHint = new GroupDisplayHint
                    {
                        HintId = Constants.DisplayHintIdPrefixes.PaymentOptionText + selectionInstance.PaymentInstrumentId,
                        LayoutOrientation = Constants.PartnerHintsValues.InlinePlacement,
                        StyleHints = new List<string> { "gap-small", "align-vertical-center" }
                    };

                    TextDisplayHint expiryInfo = optionDisplayHint.Members[1] as TextDisplayHint;
                    expiryInfoGroupHint.Members.Add(lastFourDigitsWithExpiry);
                    expiryInfoGroupHint.Members.Add(expiryInfo);
                    newGroupWithInfoSplitIntTwo.Members.Add(expiryInfoGroupHint);
                }
                else
                {
                    newGroupWithInfoSplitIntTwo.Members.Add(lastFourDigitsWithExpiry);
                }

                textGroup.Members.Add(newGroupWithInfoSplitIntTwo);
                option.DisplayContent.Members.RemoveAt(1); // Remove the original members[1]
                option.DisplayContent.Members.Add(textGroup);
            }
            else
            {
                option.DisplayContent.Members.Add(textGroup);
            }

            return option.DisplayContent;
        }

        public static bool IsCreditCardNotCup(PaymentMethod method)
        {
            return method.PaymentMethodFamily.Equals(Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase)
                && !(method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.CupCreditCard, StringComparison.OrdinalIgnoreCase)
                || method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.CupDebitCard, StringComparison.OrdinalIgnoreCase));
        }

        public static bool ContainsCreditCard(IEnumerable<PaymentMethod> paymentMethods)
        {
            return paymentMethods.Aggregate<PaymentMethod, bool>(false, (containsCC, pm) => containsCC || IsCreditCardNotCup(pm));
        }

        public static bool IsAllCreditCardOrStoredValue(IEnumerable<PaymentMethod> paymentMethods)
        {
            return paymentMethods.Aggregate<PaymentMethod, bool>(true, (allCC, pm) => allCC && (IsCreditCardNotCup(pm) || IsStoredValue(pm)));
        }

        public static Dictionary<string, int> GetMaxAllowedLogosPerSelectOptionForPartner()
        {
            return MaxAllowedLogosPerSelectOptionForPartner;
        }

        public static DisplayHintAction CreateSuccessPidlAction(ActionContext context, bool isDefault)
        {
            ActionContext actionContext = new ActionContext(context);
            return new DisplayHintAction(Constants.ActionType.Success, isDefault, actionContext, null);
        }

        // Returns a string of all the types of the given payment method family that appear in the given paymentMethods
        // Optionally adds the string to a dictionary cache so the value does not have to be created repeatedly
        public static string GetCommaSeparatedTypes(string family, HashSet<PaymentMethod> paymentMethods, Dictionary<string, string> cachedTypes = null)
        {
            if (cachedTypes != null && cachedTypes.ContainsKey(family))
            {
                return cachedTypes[family];
            }

            string types = paymentMethods.First(pm => pm.PaymentMethodFamily.Equals(family)).PaymentMethodType;
            foreach (PaymentMethod methodOfFamily in paymentMethods.Where(
                pm => pm.PaymentMethodFamily.Equals(family) && !pm.PaymentMethodType.Equals(types) && IsCollapsedPaymentMethodOption(pm)))
            {
                types = string.Format("{0},{1}", types, methodOfFamily.PaymentMethodType);
            }

            if (cachedTypes != null)
            {
                cachedTypes[family] = types;
            }

            return types;
        }

        public static bool IsCollapsedPaymentMethodOption(PaymentMethod method)
        {
            return IsCreditCardNotCup(method)
                || method.PaymentMethodFamily.Equals(Constants.PaymentMethodFamilyNames.MobileBillingNonSim, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCup(PaymentMethod method)
        {
            return method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.CupCreditCard, StringComparison.OrdinalIgnoreCase)
                || method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.CupDebitCard, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetPaymentMethodFamilyTypeDisplayId(PaymentMethod method)
        {
            return string.Format("{0}_{1}", method.PaymentMethodFamily, method.PaymentMethodType);
        }

        public static string GetPaymentMethodDisplayText(PaymentMethod method, string country = null, bool localized = true, List<string> exposedFlightFeatures = null)
        {
            string paymentMethodDisplayText = null;

            if (IsCreditCardNotCup(method))
            {
                if (string.Equals(country, Constants.CountryCodes.Brazil, StringComparison.OrdinalIgnoreCase))
                {
                    paymentMethodDisplayText = Constants.PaymentMethodOptionStrings.WebblendsCreditCardBr;
                }
                else if (string.Equals(country, Constants.CountryCodes.Sweden, StringComparison.OrdinalIgnoreCase))
                {
                    paymentMethodDisplayText = Constants.PaymentMethodOptionStrings.WebblendsCreditCardSe;
                }
                else
                {
                    if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.EnablePaymentMethodGrouping, StringComparer.OrdinalIgnoreCase))
                    {
                        paymentMethodDisplayText = Constants.PaymentMethodOptionStrings.PMGroupingCreditCard;
                    }
                    else
                    {
                        paymentMethodDisplayText = Constants.PaymentMethodOptionStrings.WebblendsCreditCard;
                    }
                }
            }
            else if (method.PaymentMethodFamily.Equals(Constants.PaymentMethodFamilyNames.MobileBillingNonSim, StringComparison.OrdinalIgnoreCase))
            {
                paymentMethodDisplayText = Constants.PaymentMethodOptionStrings.WebblendsNonSim;
            }
            else if (IsStoredValue(method))
            {
                paymentMethodDisplayText = Constants.PaymentMethodOptionStrings.StoredValue;
            }

            if (paymentMethodDisplayText != null)
            {
                if (localized)
                {
                    return PidlModelHelper.GetLocalizedString(paymentMethodDisplayText);
                }
                else
                {
                    return paymentMethodDisplayText;
                }
            }
            else
            {
                return method.Display.Name;
            }
        }

        public static string GetPaymentMethodFamilyTypeDisplayId(string id)
        {
            return id.Replace('.', '_').Replace(',', '_');
        }

        public static string GetPaymentMethodFamilyTypeId(PaymentMethod method)
        {
            return string.Format("{0}.{1}", method.PaymentMethodFamily, method.PaymentMethodType);
        }

        public static string BuildPiDefaultDisplayName(PaymentInstrument pi, string partnerName, string country = null, List<string> flightNames = null, FeatureConfig featureConfig = null, bool showExpiryMonthAndYear = false, PaymentExperienceSetting setting = null)
        {
            PaymentInstrumentDetails details = pi.PaymentInstrumentDetails;
            PaymentMethod method = pi.PaymentMethod;

            if (IsAmcWebPartner(partnerName) || PXCommon.Constants.PartnerGroups.IsWindowsNativePartner(partnerName) || TemplateHelper.IsListPiRadioButtonTemplate(setting))
            {
                if (IsCreditCardNotCup(method))
                {
                    if (IsWindowsSettingsPartner(partnerName) || TemplateHelper.IsListPiRadioButtonTemplate(setting))
                    {
                        return string.Format(
                        PidlModelHelper.GetLocalizedString(Constants.PaymentMethodFormatStrings.DotFormat),
                        pi.PaymentInstrumentDetails.CardHolderName,
                        details.LastFourDigits);
                    }

                    return string.Format(
                    PidlModelHelper.GetLocalizedString(Constants.PaymentMethodFormatStrings.StarFormat),
                    pi.PaymentMethod.Display.Name,
                    details.LastFourDigits);
                }
                else if (IsEmail(method))
                {
                    return details.Email;
                }
                else if (IsUpi(method))
                {
                    return pi.PaymentInstrumentDetails.Vpa;
                }
                else if (method.PaymentMethodFamily.Equals(Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase) && method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.Venmo, StringComparison.OrdinalIgnoreCase))
                {
                    return pi.PaymentInstrumentDetails.UserName;
                }
                else
                {
                    return pi.PaymentMethod.Display.Name;
                }
            }
            else if (IsSetupOfficeSdxPartner(partnerName))
            {
                if (IsCreditCardNotCup(method))
                {
                    return string.Format(
                        PidlModelHelper.GetLocalizedString(Constants.PaymentMethodFormatStrings.MixerFormat),
                        method.PaymentMethodType.ToUpper(),
                        details.LastFourDigits);
                }
                else if (method.PaymentMethodFamily.Equals(Constants.PaymentMethodFamilyNames.DirectDebit, StringComparison.OrdinalIgnoreCase) || IsCup(method))
                {
                    return string.Format("{0} \u2022\u2022 {1}", details.CardHolderName, details.LastFourDigits);
                }
                else if (IsEmail(method))
                {
                    return details.Email;
                }
                else if (IsUpi(method))
                {
                    return pi.PaymentInstrumentDetails.Vpa;
                }
                else
                {
                    return pi.PaymentMethod.Display.Name;
                }
            }
            else
            {
                if (IsCreditCardNotCup(method))
                {
                    string retVal = string.Format("{0} \u2022\u2022{1}", details.CardHolderName, details.LastFourDigits);

                    if (flightNames == null || !flightNames.Any())
                    {
                        retVal = BuildPiDefaultDisplayNameAddExpiryDetails(pi, retVal, flightNames, partnerName, showExpiryMonthAndYear || TemplateHelper.IsListPiTemplate(setting), country);
                    }
                    else
                    {
                        // When country=india, flight=IndiaExpiryGroupDelete is on and partner=xboxnative or partner=storify, expiry year and expiry month info should be removed from card display
                        if (!(string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase)
                            && flightNames.Contains(Constants.PartnerFlightValues.IndiaExpiryGroupDelete)
                            && (IsXboxNativePartner(partnerName) || PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))))
                        {
                            retVal = BuildPiDefaultDisplayNameAddExpiryDetails(pi, retVal, flightNames, partnerName, showExpiryMonthAndYear || TemplateHelper.IsListPiTemplate(setting), country);
                        }
                        else if (string.Equals(pi?.PaymentInstrumentDetails?.Address?.Country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase) && flightNames.Contains(Constants.PartnerFlightValues.IndiaExpiryGroupDelete) && PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                        {
                            pi.PaymentInstrumentDetails.IsIndiaExpiryGroupDeleteFlighted = true;
                        }
                    }

                    return retVal;
                }
                else if (method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.IdealBillingAgreement, StringComparison.OrdinalIgnoreCase) && method.PaymentMethodFamily.Equals(Constants.PaymentMethodFamilyNames.DirectDebit, StringComparison.OrdinalIgnoreCase))
                {
                    return string.Format("{0} \u2022\u2022{1}", details.Issuer, details.BankAccountLastFourDigits);
                }
                else if (method.PaymentMethodFamily.Equals(Constants.PaymentMethodFamilyNames.DirectDebit, StringComparison.OrdinalIgnoreCase) || IsCup(method))
                {
                    return string.Format("{0} \u2022\u2022{1}", details.CardHolderName, details.LastFourDigits);
                }
                else if (IsEmail(method))
                {
                    return details.Email;
                }
                else if (method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.Venmo, StringComparison.OrdinalIgnoreCase))
                {
                    return pi.PaymentInstrumentDetails.UserName;
                }
                else if (IsUpi(method) || IsUpiCommercial(method))
                {
                    return pi.PaymentInstrumentDetails.Vpa;
                }
                else if (IsUserLoginId(method))
                {
                    return pi.PaymentInstrumentDetails.UserLoginId;
                }
                else
                {
                    return pi.PaymentMethod.Display.Name;
                }
            }
        }

        public static void PopulatePaymentMethods(
            PIDLResource pidlResource,
            HashSet<PaymentMethod> paymentMethods,
            string defaultPaymentMethod,
            string language,
            string country,
            string partnerName,
            List<string> exposedFlightFeatures = null,
            string scenario = null,
            PaymentExperienceSetting setting = null)
        {
            if (pidlResource == null)
            {
                throw new ArgumentNullException("pidlResource");
            }

            bool isEligiblePI = string.Equals(scenario, Constants.ScenarioNames.EligiblePI, StringComparison.OrdinalIgnoreCase);
            var displayHint = isEligiblePI
                ? pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodSelectRadio) as PropertyDisplayHint
                : pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodSelect) as PropertyDisplayHint;

            if (displayHint == null)
            {
                // PaymentMethodTppSelect is used for third party payment flow for template parnter.
                displayHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodTppSelect) as PropertyDisplayHint;
            }

            // TODO: We need to remove this code once the implementation is done after partner's migration and started to use the SelectPMDropDown template partner
            // checks for webblends override select payment method to dropdown scenario
            if ((IsWebBlendsPartner(partnerName) || IsOxoWebDirectPartner(partnerName) || IsOxoDime(partnerName))
                && string.Equals(scenario, Constants.ScenarioNames.PaymentMethodAsDropdown, StringComparison.OrdinalIgnoreCase))
            {
                displayHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodSelectDropdown) as PropertyDisplayHint;
                displayHint.SelectType = Constants.PaymentMethodSelectType.DropDown;
            }

            if (displayHint != null)
            {
                if (exposedFlightFeatures?.Contains(Flighting.Features.PXSelectPMDropdownButtonListMerge, StringComparer.OrdinalIgnoreCase) ?? false)
                {
                    PopulatePaymentMethodButtonOptions(pidlResource, paymentMethods, language, country, partnerName, displayHint.HintId);

                    // the following changes will be made to the csv when the flight is complete:
                    // displayHint for buttonList partners already has propertyName = id,
                    // for dropdown elements it is equal to the dummy 'displayId', this needs to be changed to id
                    displayHint.PropertyName = Constants.DataDescriptionIds.PaymentInstrumentId;

                    // set dataType of id to returnObject instead of string to return the context object when the
                    // display type is dropdown (buttonList returns the full context no matter the dataType)
                    PropertyDescription id = pidlResource.DataDescription[Constants.DataDescriptionIds.PaymentInstrumentId] as PropertyDescription;
                    id.PropertyDescriptionType = "returnObject";
                }
                else if (displayHint.SelectType.Equals(Constants.PaymentMethodSelectType.ButtonList) || displayHint.SelectType.Equals(Constants.PaymentMethodSelectType.Radio))
                {
                    if (!string.IsNullOrEmpty(MaxAllowedLogosPerSelectOptionForPartner.SingleOrDefault(x => x.Key == partnerName).Key)
                        || IsSelectPmRadioButtonListPartner(partnerName, setting: setting)
                        || string.Equals(scenario, Constants.ScenarioNames.SelectPMWithLogo, StringComparison.OrdinalIgnoreCase))
                    {
                        PopulatePaymentMethodButtonOptionsGroupByFamily(pidlResource, paymentMethods, language, country, partnerName, displayHint.HintId, scenario, exposedFlightFeatures, setting: setting);
                    }
                    else
                    {
                        PopulatePaymentMethodButtonOptions(pidlResource, paymentMethods, language, country, partnerName, displayHint.HintId, scenario);
                    }
                }
                else
                {
                    PopulatePaymentMethodSelectOptions(pidlResource, paymentMethods, defaultPaymentMethod, country, displayHintId: displayHint.HintId, setting: setting, language, partnerName, scenario);
                }
            }
        }

        public static List<PIDLResource> GetPaymentMethodSelectPidls(
            string partnerName,
            string country,
            string language,
            HashSet<PaymentMethod> filteredPaymentMethods,
            string defaultPaymentMethod = null,
            List<string> exposedFlightFeatures = null,
            string scenario = null,
            PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retList = new List<PIDLResource>();

            // Used to bypass GetPM if CC is the only option and go straight to CC AddResource
            if (IsAllCreditCardOrStoredValue(filteredPaymentMethods) && ContainsCreditCard(filteredPaymentMethods))
            {
                string paymentMethodFamily = Constants.PaymentMethodFamilyNames.CreditCard;
                string paymentMethodType = GetCommaSeparatedTypes(paymentMethodFamily, filteredPaymentMethods);
                string id = string.Format("{0}.{1}", paymentMethodFamily, paymentMethodType);

                // for template based flow, we could enable "SkipSelectPM" feature in PSS setting to skip select PM and go straight to add resource.
                // for non-template based flow, we could enable "PXEnableSkipGetPMIfCreditCardIsTheOnlyOption" flighting to skip select PM and go straight to add resource.
                if (string.Equals(partnerName, Constants.PartnerNames.Commercialstores, StringComparison.OrdinalIgnoreCase)
                    || (exposedFlightFeatures?.Contains(Flighting.Features.PXEnableSkipGetPMIfCreditCardIsTheOnlyOption) ?? false))
                {
                    retList.Add(GetAddCreditCardClientActionResponse(partnerName, country, language, id, paymentMethodFamily, paymentMethodType, exposedFlightFeatures));
                    return retList;
                }
                else if (IsPXSkipGetPMCCOnly(country, partnerName))
                {
                    // TODO: Task 55128179: This if block is to get the client action response from add credit card, as it is covered under the SkipSelectPM feature for select operation.
                    // The IsPXSkipGetPMCCOnly can be removed once the partners under the IsPXSkipGetPMCCOnly migrated to use the PSS feature.
                    retList.Add(GetAddResourceClientActionResponse(partnerName, country, language, exposedFlightFeatures, id, paymentMethodFamily, paymentMethodType));
                    return retList;
                }
            }

            PIDLResource retVal = GetPidlResource(partnerName, country, Constants.PidlOperationTypes.Select, Constants.PidlResourceIdentities.PaymentMethodSelectPidl, null, Constants.DescriptionTypes.PaymentMethodDescription, exposedFlightFeatures, scenario: scenario, setting: setting);
            PopulatePaymentMethods(retVal, filteredPaymentMethods, defaultPaymentMethod, language, country, partnerName, exposedFlightFeatures, scenario, setting: setting);
            retList.Add(retVal);
            return retList;
        }

        public static List<PIDLResource> GetSinglePiDisplayPidl(
            string partnerName,
            string country,
            string language,
            List<PaymentInstrument> paymentInstruments,
            List<PaymentInstrument> disabledPaymentInstruments,
            string filters,
            PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retList = new List<PIDLResource>();
            PaymentMethodFilters filtersObject = GetPaymentMethodFilters(filters);
            PaymentInstrument targetPi = null;

            if (paymentInstruments.Count > 0)
            {
                targetPi = paymentInstruments.Find(pi => pi.PaymentInstrumentId.Equals(filtersObject.Id));
            }

            if (targetPi == null && disabledPaymentInstruments.Count > 0)
            {
                targetPi = disabledPaymentInstruments.Find(pi => pi.PaymentInstrumentId.Equals(filtersObject.Id));
            }

            if (targetPi != null)
            {
                PIDLResource pidlResource = GetPaymentInstrumentPidlResource(
                    partnerName,
                    country,
                    Constants.PidlOperationTypes.SelectSingleInstance,
                    Constants.PidlResourceIdentities.DisplaySinglePaymentInstrumentPidl,
                    targetPi.PaymentInstrumentId,
                    setting: setting);

                TextDisplayHint textHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentDisplay) as TextDisplayHint;
                PaymentInstrumentDetails piDetails = targetPi.PaymentInstrumentDetails;
                string displayName = BuildPiDefaultDisplayName(targetPi, partnerName);
                string displayTag = "single-pi-display-pidl-text";
                bool isExpired = false;

                if (piDetails != null && !string.IsNullOrEmpty(piDetails.ExpiryMonth) && !string.IsNullOrEmpty(piDetails.ExpiryYear))
                {
                    targetPi.PaymentInstrumentDetails.DaysUntilExpired = GetDaysUntilExpired(targetPi);
                    isExpired = targetPi.PaymentInstrumentDetails.DaysUntilExpired <= 0;
                }

                if (isExpired)
                {
                    displayName = LocalizationRepository.Instance.GetLocalizedString(Constants.SinglePiDisplayLabels.ExpiredPI, language).Replace("{0}", displayName);
                    displayTag += "-expired";
                }

                textHint.AddDisplayTag("SinglePiDisplayPidlText", displayTag);
                textHint.DisplayContent = displayName;

                LogoDisplayHint logoHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentSelectionImage) as LogoDisplayHint;
                logoHint.SourceUrl = GetPaymentInstrumentLogoUrl(targetPi);
                logoHint.AddDisplayTag("SinglePiDisplayPidlLogo", "single-pi-display-pidl-logo");

                string localizedString = PidlModelHelper.GetLocalizedString("{0} logo,");
                localizedString = localizedString.Replace("{0}", targetPi.PaymentMethod.Display.Name);
                logoHint.AddDisplayTag("accessibilityName", localizedString);

                // Needed for Get Selected Resource
                ButtonDisplayHint hiddenButton = pidlResource.GetDisplayHintById(Constants.ButtonDisplayHintIds.SubmitButtonHidden) as ButtonDisplayHint;

                if (hiddenButton != null)
                {
                    hiddenButton.Action = CreateSuccessPidlAction(CreatePaymentInstrumentActionContext(targetPi, null, null), true);
                }

                retList.Add(pidlResource);
            }
            else
            {
                // TODO: This is a temporary error fallback.  Update when error case contract is finalized.
                throw new InvalidOperationException();
            }

            return retList;
        }

        public static List<PIDLResource> GetPaymentInstrumentSelectPidls(
            string partnerName,
            string country,
            string language,
            List<PaymentInstrument> paymentInstruments,
            List<PaymentInstrument> disabledPaymentInstruments,
            HashSet<PaymentMethod> filteredPaymentMethods,
            string filters,
            List<string> exposedFlightFeatures = null,
            string scenario = null,
            PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retList = new List<PIDLResource>();
            bool noPaymentInstruments = paymentInstruments.Count == 0 && disabledPaymentInstruments.Count == 0;
            PaymentMethodFilters filtersObject = GetPaymentMethodFilters(filters);

            // TODO: As part of PR-11875566, this if block is converted into SkipSelectInstanceNoPI feature with cutomizations
            // Feature Customizations: EnableBackupPICheckForSkipSelectInstanceNoPI, AddTriggeredByForSkipSelectInstanceNoPI, ReturnAddCCOnlyForSkipSelectInstanceNoPI
            // these partner conditions can be removed once migrated to use the pss feature
            if (noPaymentInstruments && (PartnerSkipsToAddResource(partnerName) || (IsAmcWebPartner(partnerName) && filtersObject.IsBackupPiOptional != true)))
            {
                retList.Add(GetAddResourceClientActionResponse(partnerName, country, language));
                return retList;
            }

            PaymentInstrument storedValue = paymentInstruments.FirstOrDefault(pi => IsStoredValue(pi.PaymentMethod));
            string pidlId = (noPaymentInstruments && !PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                ? Constants.PidlResourceIdentities.PaymentInstrumentSelectNoPiPidl
                : Constants.PidlResourceIdentities.PaymentInstrumentSelectPidl;

            if (filtersObject.SplitPaymentSupported == true
                && (IsAmcWebPartner(partnerName) || IsReturnBackupPidlFeatureEnabled(country, setting)))
            {
                pidlId = Constants.PidlResourceIdentities.PaymentInstrumentBackupPidl;
            }

            PIDLResource primarySelect = GetPaymentInstrumentPidlResource(
                partnerName,
                country,
                Constants.PidlOperationTypes.SelectInstance,
                pidlId,
                scenario,
                flightNames: exposedFlightFeatures,
                setting: setting);

            if (string.Equals(partnerName, PXCommon.Constants.PartnerNames.XboxSettings, StringComparison.OrdinalIgnoreCase))
            {
                AddActionToGetPMButton(primarySelect, null, Constants.DisplayHintIds.NewPaymentMethodLink, country, language, partnerName);
            }
            else
            {
                PopulateSelectInstancePidl(
                    primarySelect,
                    partnerName,
                    country,
                    language,
                    paymentInstruments,
                    disabledPaymentInstruments,
                    filteredPaymentMethods,
                    null,
                    filtersObject.Id,
                    exposedFlightFeatures,
                    filtersObject,
                    scenario,
                    setting: setting);
            }

            if (filtersObject.SplitPaymentSupported == true && filtersObject.IsBackupPiOptional == true
                && (IsAmcWebPartner(partnerName) || IsReturnBackupPidlFeatureEnabled(country, setting)))
            {
                AddNoBackPIOption(primarySelect, Constants.DisplayHintIds.NoBackupPISelected, LocalizationRepository.Instance.GetLocalizedString(Constants.ListPaymentInstrumentStaticElements.NotWantBackupPI, language));
            }

            if (PartnerSupportsSingleInstancePidls(partnerName) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableSingleInstancePidls, country, setting))
            {
                List<PaymentInstrument> singlePaymentOptions = new List<PaymentInstrument>(paymentInstruments);

                if (storedValue != null)
                {
                    decimal price = GetPriceToken(filtersObject);
                    List<PaymentInstrument> backupOptions = GetBackupPiOptions(paymentInstruments, storedValue, price);

                    if (backupOptions != null && backupOptions.Count > 0)
                    {
                        AddSelectValidation(primarySelect, storedValue.PaymentInstrumentId, true);
                        UpdateStoredValueActionToSelectBackupPI(primarySelect, storedValue.PaymentInstrumentId);

                        singlePaymentOptions.Remove(storedValue);
                        List<PaymentInstrument> backupDisabledPIs = GetBackupPiOptions(disabledPaymentInstruments, storedValue, price);

                        retList.Add(GetBackupSelectPidl(partnerName, country, language, storedValue, paymentInstruments, disabledPaymentInstruments, backupOptions, backupDisabledPIs, price, filteredPaymentMethods, setting: setting));
                        AddSingleInstancePidls(retList, partnerName, country, language, Constants.PidlResourceIdentities.SingleBackupPidl, false, backupOptions, filtersObject.Id, filtersObject.BackupId, storedValue, price, setting: setting);

                        // Add special storedValue pidl with selectResourceType action for adding new backup PIs
                        AddSingleInstancePidls(retList, partnerName, country, language, Constants.PidlResourceIdentities.SingleCsvPidl, true, new List<PaymentInstrument> { storedValue }, filtersObject.Id, filtersObject.BackupId, null, price, setting: setting);
                    }
                    else
                    {
                        DisableStoredValueOption(primarySelect, storedValue.PaymentInstrumentId);
                    }
                }

                if (singlePaymentOptions.Count == 0 && storedValue == null)
                {
                    // If there are no singlePaymentOptions (and special storedValue pidl was not added), user should be shown special selectSingleInstance pidl
                    if (disabledPaymentInstruments.Count > 0)
                    {
                        PIDLResource noActivePiPidl = GetPaymentInstrumentPidlResource(partnerName, country, Constants.PidlOperationTypes.SelectSingleInstance, Constants.PidlResourceIdentities.SinglePaymentInstrumentNoActivePiPidl, setting: setting);
                        AddActionToSelectPMButton(noActivePiPidl, Constants.DisplayHintIds.SelectPaymentMethodLink, country, language, partnerName);
                        retList.Add(noActivePiPidl);
                    }
                    else
                    {
                        PIDLResource noSinglePiPidl = GetPaymentInstrumentPidlResource(partnerName, country, Constants.PidlOperationTypes.SelectSingleInstance, Constants.PidlResourceIdentities.SinglePaymentInstrumentNoPiPidl, setting: setting);
                        AddActionToGetPMButton(noSinglePiPidl, null, Constants.DisplayHintIds.NewPaymentMethodLink, country, language, partnerName);
                        retList.Add(noSinglePiPidl);
                    }
                }

                AddSingleInstancePidls(retList, partnerName, country, language, Constants.PidlResourceIdentities.SinglePaymentInstrumentPidl, false, singlePaymentOptions, filtersObject.Id, filtersObject.BackupId, setting: setting);
            }

            retList.Insert(0, primarySelect);
            return retList;
        }

        public static List<PaymentInstrument> GetFilteredPaymentInstruments(
            List<PaymentInstrument> paymentInstruments,
            List<PaymentInstrument> disabledPaymentInstruments,
            HashSet<PaymentMethod> filteredPaymentMethods,
            string allowedPaymentMethods,
            string filters,
            string partnerName,
            string country,
            PaymentExperienceSetting setting = null)
        {
            List<PaymentInstrument> retList = new List<PaymentInstrument>();

            Dictionary<string, int> inclusionList;
            try
            {
                if (string.Equals(partnerName, PXCommon.Constants.PartnerNames.XboxSettings, StringComparison.OrdinalIgnoreCase))
                {
                    List<string> allowed_pm = JsonConvert.DeserializeObject<List<string>>(allowedPaymentMethods);
                    inclusionList = allowed_pm.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
                }
                else
                {
                    // This line is throwing an error for most of the partners and needs to be addressed
                    inclusionList = JObject.Parse(allowedPaymentMethods).ToObject<Dictionary<string, int>>();
                }
            }
            catch
            {
                inclusionList = null;
            }

            PaymentMethodFilters filtersObject = GetPaymentMethodFilters(filters);

            // TODO: Task 55128179: The cart partner check from SelectInstanceShowDisabledPIs can be removed once the migration is complete and the PSS UseDisabledPIsForSelectInstance inline feature is in use.
            bool useDisabledPaymentInstruments = disabledPaymentInstruments != null && (SelectInstanceShowDisabledPIs(partnerName, filtersObject) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseDisabledPIsForSelectInstance, country, setting));
            decimal price = GetPriceToken(filtersObject);

            // Remove payment instruments based on filtersObject flags
            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.RemoveZeroBalanceCsv, country, setting))
            {
                RemoveZeroBalanceCSV(paymentInstruments);
            }

            RemoveExpiredPaymentInstruments(paymentInstruments, filtersObject);
            RemovePrepaidPaymentInstruments(paymentInstruments, filtersObject);

            if (disabledPaymentInstruments != null)
            {
                // Remove disabled PIs if they are not in allowedPaymentMethods or active/pending on picv/pending on upgrading from MIB to CIB (Paypal only)
                disabledPaymentInstruments.RemoveAll(pi => !PaymentMethodsContainsActivePi(filteredPaymentMethods, pi));
                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.RemoveZeroBalanceCsv, country, setting))
                {
                    RemoveZeroBalanceCSV(disabledPaymentInstruments);
                }

                RemoveExpiredPaymentInstruments(disabledPaymentInstruments, filtersObject);
                RemovePrepaidPaymentInstruments(disabledPaymentInstruments, filtersObject);
            }

            foreach (PaymentInstrument pi in paymentInstruments)
            {
                if (string.Equals(partnerName, PXCommon.Constants.PartnerNames.XboxSettings, StringComparison.OrdinalIgnoreCase))
                {
                    if (inclusionList == null || inclusionList.ContainsKey(GetInclusionListId(inclusionList, pi)))
                    {
                        string alternateImageLinkForPartner = CheckForReactNativeAlternatePaymentInstrumentLogoUrl(pi, partnerName);
                        if (!string.IsNullOrEmpty(alternateImageLinkForPartner))
                        {
                            var index = pi.PaymentMethod.Display?.Logos?.FindIndex(logo => logo.MimeType.Equals("image/svg+xml", StringComparison.OrdinalIgnoreCase));
                            if (index.HasValue && index.Value > -1)
                            {
                                pi.PaymentMethod.Display.Logos[index.Value].Url = alternateImageLinkForPartner;
                            }
                        }
                        //// use v2 png logo for verve and venmo
                        if (pi.PaymentMethod.PaymentMethodType == Constants.PaymentMethodTypeNames.Verve)
                        {
                            var index = pi.PaymentMethod.Display?.Logos?.FindIndex(logo => logo.MimeType.Equals("image/png", StringComparison.OrdinalIgnoreCase));
                            if (index.HasValue && index.Value > -1)
                            {
                                pi.PaymentMethod.Display.Logos[index.Value].Url = alternateImageLinkForPartner;
                            }
                            else
                            {
                                Logo newPilogo = new Logo
                                {
                                    MimeType = "image/png",
                                    Url = alternateImageLinkForPartner,
                                };
                                pi.PaymentMethod.Display.Logos.Insert(0, newPilogo);
                            }
                        }

                        if (pi.Status.Equals(PaymentInstrumentStatus.Active) && (pi.PaymentMethod.Properties.UserManaged || pi.PaymentMethod.PaymentMethodType.Equals("stored_value")))
                        {
                            retList.Add(pi);
                        }
                    }
                }
                else if (PaymentMethodsContainsActivePi(filteredPaymentMethods, pi))
                {
                    //// Only include payment instruments if they are active or pending on picv or pending on upgrading from MIB to CIB (Paypal only)
                    if (inclusionList == null || inclusionList[GetInclusionListId(inclusionList, pi)] == 1)
                    {
                        if (useDisabledPaymentInstruments && ExpiryActionNeeded(pi) && !PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                        {
                            // Partners who have disabled PIs in list will show expired cards as disabled.
                            // Other partners will show expired cards as active unless 'filterExpiredPayment' is specified.
                            disabledPaymentInstruments.Add(pi);
                        }
                        else if (HasMatchingExclusionTags(filtersObject, pi.PaymentMethod) || PriceNotMeetsChargeThreshold(pi.PaymentMethod, price, country))
                        {
                            if (useDisabledPaymentInstruments)
                            {
                                disabledPaymentInstruments.Add(pi);
                            }
                        }
                        else if (useDisabledPaymentInstruments && ShouldAddNonSplitPaymentSupportedPI(filtersObject, pi, partnerName))
                        {
                            disabledPaymentInstruments.Add(pi);
                        }
                        else
                        {
                            retList.Add(pi);
                        }
                    }
                    else if (useDisabledPaymentInstruments)
                    {
                        disabledPaymentInstruments.Add(pi);
                    }
                }
            }

            return retList.OrderBy(pi => pi.Status.ToString()).ToList();
        }

        public static List<PaymentInstrument> GetFilteredPaymentInstrumentsForChallenge(List<PaymentInstrument> paymentInstruments, HashSet<PaymentMethod> filteredPaymentMethods, string allowedPaymentMethods, string filters, string country, string partnerName, PaymentExperienceSetting setting = null)
        {
            // filter payment instruments by inclusion list and filters first
            List<PaymentInstrument> retList = GetFilteredPaymentInstruments(paymentInstruments, null, filteredPaymentMethods, allowedPaymentMethods, filters, partnerName, country, setting: setting);

            // only include payment instruments that support validation challenge
            retList.RemoveAll(pi => PIDLResourceFactory.Instance.GetSupportedValidationChallenge(country, pi.PaymentMethod.PaymentMethodFamily, pi.PaymentMethod.PaymentMethodType) == null);
            return retList;
        }

        public static HashSet<PaymentMethod> GetFilteredPaymentMethods(HashSet<PaymentMethod> paymentMethods, string allowedPaymentMethods, string filters, string operation, string partnerName, string country, PaymentExperienceSetting setting = null)
        {
            HashSet<PaymentMethod> returnSet = GetAllowedPaymentMethods(paymentMethods, allowedPaymentMethods);
            PaymentMethodFilters filtersObject = GetPaymentMethodFilters(filters);

            // Xbox Native partners will show disabled payment methods in the list when split payment supported
            if (ShouldRemoveNonSplitPaymentSupportedPMs(filtersObject, partnerName, operation))
            {
                returnSet.RemoveWhere(pm => !pm.Properties.SplitPaymentSupported);
            }

            if (filtersObject.FilterPurchaseRedirectPayment.HasValue && filtersObject.FilterPurchaseRedirectPayment.Value)
            {
                returnSet.RemoveWhere(pm => pm.Properties.RedirectRequired != null && pm.Properties.RedirectRequired.Contains("Purchase", StringComparer.OrdinalIgnoreCase));
            }

            if (operation.Equals(Constants.PidlOperationTypes.Select))
            {
                returnSet.RemoveWhere(pm => HasMatchingExclusionTags(filtersObject, pm));

                decimal price = GetPriceToken(filtersObject);
                returnSet.RemoveWhere(pm => PriceNotMeetsChargeThreshold(pm, price, country));
            }

            return returnSet;
        }

        public static List<PIDLResource> GetValidateInstancePidls(List<PaymentInstrument> paymentInstruments, string filters, string partnerName, string country, string language)
        {
            List<PIDLResource> retList = new List<PIDLResource>();
            if (paymentInstruments.Count <= 0)
            {
                retList.Add(GetAddResourceClientActionResponse(partnerName, country, language));
                return retList;
            }

            foreach (PaymentInstrument pi in paymentInstruments)
            {
                string pidlIdentity = PIDLResourceFactory.Instance.GetSupportedValidationChallenge(country, pi.PaymentMethod.PaymentMethodFamily, pi.PaymentMethod.PaymentMethodType);
                PIDLResource challengePidl = GetPaymentInstrumentPidlResource(partnerName, country, Constants.PidlOperationTypes.ValidateInstance, pidlIdentity, pi.PaymentInstrumentId);

                PopulateSelectInstancePidl(challengePidl, partnerName, country, language, paymentInstruments, null, null, null, pi.PaymentInstrumentId);
                AddSelectValidation(challengePidl, pi.PaymentInstrumentId, false);
                AddChallengeSubmitLink(challengePidl, pi.PaymentInstrumentId, language);

                // if id was specified, return that PIDL first
                PaymentMethodFilters filtersObject = GetPaymentMethodFilters(filters);
                if (string.Equals(filtersObject.Id, pi.PaymentInstrumentId))
                {
                    retList.Insert(0, challengePidl);
                }
                else
                {
                    retList.Add(challengePidl);
                }
            }

            return retList;
        }

        public static List<PIDLResource> GetAddressGroupSelectPidls(string partnerName, string country, string language, CMResources<AddressInfo> addressGroup)
        {
            List<PIDLResource> retList = new List<PIDLResource>();

            PIDLResource primarySelect = GetAddressGroupPidlResource(
                partnerName,
                country,
                Constants.PidlOperationTypes.SelectInstance,
                Constants.PidlResourceIdentities.AddressGroupSelectPidl);

            PopulateAddressGroupPidl(
                primarySelect,
                partnerName,
                country,
                language,
                addressGroup);

            retList.Add(primarySelect);
            return retList;
        }

        public static List<PIDLResource> GetPaymentInstrumentListPidls(string partnerName, string country, string scenario, string language, string classicProduct = null, string billableAccountId = null, List<string> exposedFlightFeatures = null, PaymentExperienceSetting setting = null)
        {
            string pidlId = string.Equals(scenario, Constants.ScenarioNames.MonetaryCommit, StringComparison.OrdinalIgnoreCase) ? Constants.PidlResourceIdentities.ListMCPI
                : string.Equals(scenario, Constants.ScenarioNames.MonetaryCommitModernAccounts, StringComparison.OrdinalIgnoreCase) ? Constants.PidlResourceIdentities.ListMCPIModern
                : string.Equals(scenario, Constants.ScenarioNames.EligiblePI, StringComparison.OrdinalIgnoreCase) ? Constants.PidlResourceIdentities.ListAdditionalPI : Constants.PidlResourceIdentities.List;

            // special case pidlid for NorthStar partner
            if (string.Equals(partnerName, Constants.PartnerNames.NorthStarWeb, StringComparison.InvariantCultureIgnoreCase))
            {
                pidlId = Constants.PidlResourceIdentities.ListPI;
            }

            // WindowsStore partner uses the paymentInstrumentEx/ListModernPI endpoint for list PIs
            if (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseListModernResource, country, setting))
            {
                pidlId = Constants.PidlResourceIdentities.ListModern;
            }

            List<PIDLResource> retList = new List<PIDLResource>();
            PIDLResource listPidl = GetPidlResource(
                partnerName,
                country,
                Constants.PidlOperationTypes.SelectInstance,
                pidlId,
                null,
                Constants.DescriptionTypes.PaymentInstrumentDescription,
                exposedFlightFeatures,
                classicProduct,
                billableAccountId,
                (string.Equals(scenario, Constants.ScenarioNames.DepartmentalPurchase, StringComparison.OrdinalIgnoreCase) || string.Equals(scenario, Constants.ScenarioNames.EligiblePI, StringComparison.OrdinalIgnoreCase)) ? scenario : null,
                setting: setting);

            retList.Add(listPidl);

            return retList;
        }

        /// <summary>
        /// Get the PIDL resource for search transaction
        /// Populate possiblevalues and possibleoptions of dropdown in PIDL with list PIs
        /// Add search transactions submit link
        /// </summary>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="paymentInstruments">Payment Instrument list from PIMS</param>
        /// <param name="flightNames">Flightnames need to be passed to handle flighting of the DisplaySequences</param>
        /// <param name="setting">setting from partner setting service to generate PIDL and enable feture</param>
        /// <returns> Returns list of PIDL resource for search transactions</returns>
        public static List<PIDLResource> GetPaymentInstrumentSearchTransactionsPidls(string partnerName, string country, string language, List<PaymentInstrument> paymentInstruments, List<string> flightNames = null, PaymentExperienceSetting setting = null)
        {
            string pidlId = Constants.PidlResourceIdentities.SearchTransactions;

            List<PIDLResource> retList = new List<PIDLResource>();

            PIDLResource pidlResource = GetPaymentInstrumentPidlResource(
              partnerName,
              country,
              Constants.PidlOperationTypes.SearchTransactions,
              pidlId,
              null,
              flightNames: flightNames,
              setting: setting);

            PopulateSearchInstancePidl(pidlResource, partnerName, paymentInstruments, language);

            AddSearchTransactionSubmitLink(pidlResource, language, country, partnerName);

            retList.Add(pidlResource);
            return retList;
        }

        public static List<PIDLResource> GetBillingGroupListPidls(string type, string partnerName, string country, string language, List<string> flightNames = null, PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retList = new List<PIDLResource>();

            // Currently lightWeightBG type is identified as 'List' in csv files. Once that is deprecated, we can remove the following conversion.
            string pidlId = string.Equals(type, Constants.BillingGroupTypeNames.LightWeight, StringComparison.OrdinalIgnoreCase) ? Constants.PidlResourceIdentities.List : type;

            PIDLResource listPidl = GetBillingGroupPidlResource(
                partnerName,
                country,
                Constants.PidlOperationTypes.SelectInstance,
                pidlId,
                flightNames: flightNames,
                setting: setting);

            PropertyDescription billingGroupPropertyDescription = listPidl.GetPropertyDescriptionByPropertyName(Constants.DisplayHintIds.BillingGroupListSIBillingGroupId);

            if (billingGroupPropertyDescription != null)
            {
                billingGroupPropertyDescription.UpdatePossibleValues(new Dictionary<string, string>());
            }

            retList.Add(listPidl);
            return retList;
        }

        public static List<PIDLResource> GetPaymentMethodShowOrSearchPidls(string family, string type, string partnerName, string operation, string country, string language, PaymentExperienceSetting setting = null)
        {
            string pidlId = string.IsNullOrEmpty(type) ? family : family + "." + type;

            List<PIDLResource> retList = new List<PIDLResource>();
            PIDLResource listPidl = GetPidlResource(
                partnerName,
                country,
                operation,
                pidlId,
                null,
                Constants.DescriptionTypes.PaymentMethodDescription,
                setting: setting);

            retList.Add(listPidl);

            return retList;
        }

        public static List<PIDLResource> GetPaymentMethodFundStoredValuePidls(string family, string type, string country, string language, string partnerName, string operation, bool redeem = false, PaymentExperienceSetting setting = null)
        {
            string pidlId = string.IsNullOrEmpty(type) ? family : family + "." + type;
            pidlId = redeem ? pidlId + ".redeem" : pidlId;

            List<PIDLResource> retList = new List<PIDLResource>();
            PIDLResource fundStoredValuePidl = GetPidlResource(
                partnerName,
                country,
                operation,
                pidlId,
                null,
                Constants.DescriptionTypes.PaymentMethodDescription,
                setting: setting);

            retList.Add(fundStoredValuePidl);

            return retList;
        }

        public static string GetPaymentInstrumentLogoUrl(PaymentInstrument pi)
        {
            return GetPaymentMethodLogoUrl(pi.PaymentMethod);
        }

        public static PaymentInstrument GetStoredValuePI(List<PaymentInstrument> paymentInstruments)
        {
            return paymentInstruments.FirstOrDefault(pi => PaymentSelectionHelper.IsStoredValue(pi.PaymentMethod));
        }

        public static bool ExpiryActionNeeded(PaymentInstrument pi)
        {
            PaymentInstrumentDetails details = pi.PaymentInstrumentDetails;
            if (details != null && !string.IsNullOrEmpty(details.ExpiryMonth) && !string.IsNullOrEmpty(details.ExpiryYear))
            {
                DateTime date = DateTime.Now.Date;
                return date.Year > Convert.ToInt32(details.ExpiryYear) || (date.Year == Convert.ToInt32(details.ExpiryYear) && date.Month > Convert.ToInt32(details.ExpiryMonth));
            }

            return false;
        }

        public static string CheckForReactNativeAlternatePaymentMethodLogoUrl(PaymentMethod pm, string partner, List<string> exposedFlightFeatures = null)
        {
            if (partner == null)
            {
                return null;
            }

            if (!IsAlternateSVGEnabledPartner(partner, exposedFlightFeatures) || !AlternateImageLinkLookupTable.ContainsKey(pm.PaymentMethodType))
            {
                return null;
            }

            if (pm.PaymentMethodType == Constants.PaymentMethodTypeNames.Visa)
            {
                return null;
            }

            return $"{Constants.PidlUrlConstants.StaticResourceServiceImagesV4}/{AlternateImageLinkLookupTable[pm.PaymentMethodType]}";
        }

        public static string GetPaymentMethodLogoUrl(PaymentMethod pm)
        {
            if (pm.Display.Logos != null && pm.Display.Logos.Count > 0)
            {
                Logo svg = pm.Display.Logos.Find(logo => logo.MimeType.Equals("image/svg+xml", StringComparison.OrdinalIgnoreCase));
                if (svg != null && !string.IsNullOrEmpty(svg.Url))
                {
                    return svg.Url;
                }
            }

            return pm.Display.Logo;
        }

        public static int GetDaysUntilExpired(PaymentInstrument pi, DateTime? currentDate = null)
        {
            PaymentInstrumentDetails details = pi.PaymentInstrumentDetails;

            if (details != null && !string.IsNullOrEmpty(details.ExpiryMonth) && !string.IsNullOrEmpty(details.ExpiryYear))
            {
                // Adding one month to expiration date. If a card expires on 9/21, it will still
                // be valid throughout September.  On October 1st, it will be officially expired.
                DateTime expirationDate = new DateTime(Convert.ToInt32(details.ExpiryYear), Convert.ToInt32(details.ExpiryMonth), 1).AddMonths(1);

                if (currentDate == null)
                {
                    currentDate = DateTime.Now.Date;
                }

                TimeSpan interval = expirationDate - (DateTime)currentDate;
                return interval.Days;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static void SetMoveNextAction(PIDLResource resource)
        {
            var oobeConfirmButton = resource.GetDisplayHintById(Constants.DisplayHintIds.OOBEPhoneConfirmButton);

            if (oobeConfirmButton != null)
            {
                oobeConfirmButton.Action.NextAction = new DisplayHintAction("moveNext");
            }
        }

        public static bool IsBingTravelPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.BingTravel, StringComparison.InvariantCultureIgnoreCase);
        }

        public static void UpdateStoredValuePIDisplayHintForCart(PaymentInstrument storedValuePI, TextGroupDisplayHint textGroup, string country, string language)
        {
            if (storedValuePI?.PaymentInstrumentDetails != null)
            {
                decimal currencyBalance = storedValuePI.PaymentInstrumentDetails.Balance;
                string currencyCode = storedValuePI.PaymentInstrumentDetails.Currency;

                if (currencyCode != null)
                {
                    string formattedCurrency = CurrencyHelper.FormatCurrency(country, language, currencyBalance, currencyCode);

                    TextDisplayHint currencyValueText = new TextDisplayHint()
                    {
                        DisplayContent = string.Format(Constants.PaymentMethodFormatStrings.PointsCurrencyFormat, formattedCurrency),
                        HintId = Constants.TextDisplayHintIds.CurrencyValueText
                    };
                    textGroup.AddDisplayHint(currencyValueText);
                }
            }
        }

        public static void AddXboxNativeSelectOptionAccessibilityTag(PropertyDisplayHint pm)
        {
            foreach (var paymentMethod in pm?.PossibleOptions)
            {
                var logoArray = new List<string>();
                var paymentMethodLogoGroup = paymentMethod.Value?.DisplayContent?.Members?.FirstOrDefault() as GroupDisplayHint;
                foreach (var logo in paymentMethodLogoGroup.Members)
                {
                    if (logo.DisplayHintType == Constants.DisplayHintTypes.Image)
                    {
                        var standardLogo = logo as ImageDisplayHint;
                        logoArray.Add(standardLogo.AccessibilityName);
                    }

                    // if PMOptionLogo has altLogo
                    if (logo.DisplayHintType == Constants.DisplayHintTypes.Group)
                    {
                        var logoTest = logo as GroupDisplayHint;
                        var altLogo = logoTest.Members.FirstOrDefault() as ImageDisplayHint;
                        logoArray.Add(altLogo.AccessibilityName);
                    }
                }

                string logoString = string.Join(" ", logoArray);
                paymentMethod.Value.AccessibilityTag = string.Format($"{logoString}; {paymentMethod.Value.AccessibilityTag}");
            }
        }

        public static void AddSelectOptionAcessibilityTag(PropertyDisplayHint pm)
        {
            foreach (var paymentMethod in pm?.PossibleOptions)
            {
                var logoArray = new List<string>();
                var paymentMethodLogoGroup = paymentMethod.Value?.DisplayContent?.Members?.FirstOrDefault() as GroupDisplayHint;
                foreach (var logo in paymentMethodLogoGroup.Members)
                {
                    if (logo.DisplayHintType == Constants.DisplayHintTypes.Image)
                    {
                        var standardLogo = logo as ImageDisplayHint;
                        logoArray.Add(PidlModelHelper.GetLocalizedString(standardLogo.AccessibilityName));
                    }

                    // if PMOptionLogo has altLogo
                    if (logo.DisplayHintType == Constants.DisplayHintTypes.Group)
                    {
                        var logoTest = logo as GroupDisplayHint;
                        var altLogo = logoTest.Members.FirstOrDefault() as ImageDisplayHint;
                        logoArray.Add(PidlModelHelper.GetLocalizedString(altLogo.AccessibilityName));
                    }
                }

                string logoString = string.Join(" ", logoArray);
                paymentMethod.Value.AccessibilityTag = string.Format($"{logoString}; {PidlModelHelper.GetLocalizedString(paymentMethod.Value.AccessibilityTag)}");
            }
        }

        public static void UpdateSelectDisplay(
            PIDLResource pidlResource,
            string displayHintId,
            string dataHintId,
            Dictionary<string, string> possibleValues,
            Dictionary<string, SelectOptionDescription> possibleOptions = null,
            string defaultInstanceId = null,
            bool setDisplayHintDefault = false,
            List<string> exposedFlightFeatures = null)
        {
            PropertyDisplayHint displayHint = pidlResource.GetDisplayHintById(displayHintId) as PropertyDisplayHint;
            if (displayHint != null)
            {
                if (setDisplayHintDefault && !string.IsNullOrEmpty(defaultInstanceId))
                {
                    bool containsDefault = false;

                    if (possibleValues != null && possibleValues.ContainsKey(defaultInstanceId))
                    {
                        containsDefault = true;
                        Dictionary<string, string> temp = new Dictionary<string, string>();

                        temp[defaultInstanceId] = possibleValues[defaultInstanceId];

                        foreach (string key in possibleValues.Keys.Where(key => !key.Equals(defaultInstanceId, StringComparison.OrdinalIgnoreCase)))
                        {
                            temp[key] = possibleValues[key];
                        }

                        possibleValues = temp;
                    }

                    if (possibleOptions != null && possibleOptions.ContainsKey(defaultInstanceId))
                    {
                        containsDefault = true;
                        Dictionary<string, SelectOptionDescription> temp = new Dictionary<string, SelectOptionDescription>();

                        temp[defaultInstanceId] = possibleOptions[defaultInstanceId];

                        foreach (string key in possibleOptions.Keys.Where(key => !key.Equals(defaultInstanceId, StringComparison.OrdinalIgnoreCase)))
                        {
                            temp[key] = possibleOptions[key];
                        }

                        possibleOptions = temp;
                    }

                    if (containsDefault)
                    {
                        displayHint.IsSelectFirstItem = true;
                    }
                }

                if (possibleOptions != null)
                {
                    displayHint.SetPossibleOptions(possibleOptions);
                }
                else
                {
                    displayHint.SetPossibleOptions(possibleValues);
                }
            }

            if (pidlResource.DataDescription.ContainsKey(dataHintId))
            {
                var propertyHint = pidlResource.DataDescription[dataHintId] as PropertyDescription;
                propertyHint.UpdatePossibleValues(possibleValues);

                if (displayHint != null
                && (string.Equals(displayHint.SelectType, Constants.PaymentMethodSelectType.DropDown, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(displayHint.SelectType, Constants.PaymentMethodSelectType.Radio, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (propertyHint.PossibleValues != null && propertyHint.PossibleValues.Count != 0)
                    {
                        propertyHint.DefaultValue = (!string.IsNullOrEmpty(defaultInstanceId) && propertyHint.PossibleValues.ContainsKey(defaultInstanceId)) ?
                            defaultInstanceId : propertyHint.PossibleValues.First().Key;
                    }
                }
            }
        }

        public static void PopulateSelectInstancePidl(
            PIDLResource pidlResource,
            string partnerName,
            string country,
            string language,
            List<PaymentInstrument> paymentInstruments,
            List<PaymentInstrument> disabledPaymentInstruments = null,
            HashSet<PaymentMethod> filteredPaymentMethods = null,
            PaymentInstrument primaryInstance = null,
            string defaultInstanceId = null,
            List<string> exposedFlightFeatures = null,
            PaymentMethodFilters paymentMethodFilters = null,
            string scenario = null,
            PaymentExperienceSetting setting = null)
        {
            bool isBackupPidl = primaryInstance != null;

            // TODO: Task 55128179: The cart partner check from SelectInstanceShowDisabledPIs can be removed once the migration is complete and the PSS UseDisabledPIsForSelectInstance inline feature is in use.
            bool useDisabledPaymentInstruments = disabledPaymentInstruments != null && (SelectInstanceShowDisabledPIs(partnerName, paymentMethodFilters) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseDisabledPIsForSelectInstance, country, setting));
            string dataHintId = isBackupPidl ? Constants.DataDescriptionIds.BackupId : Constants.DataDescriptionIds.PaymentInstrumentId;
            string displayHintId = isBackupPidl ? Constants.DisplayHintIds.BackupPaymentInstrumentSelect : Constants.DisplayHintIds.PaymentInstrumentSelect;

            PropertyDisplayHint displayHint = pidlResource.GetDisplayHintById(displayHintId) as PropertyDisplayHint;

            Dictionary<string, SelectOptionDescription> possibleOptions = new Dictionary<string, SelectOptionDescription>();
            Dictionary<string, string> possibleValues = new Dictionary<string, string>();

            foreach (PaymentInstrument pi in paymentInstruments)
            {
                PIActionType? optionAction = null;
                PidlDocInfo pidlDocInfo = null;
                ResourceInfo resourceInfo = null;
                PartnerHints partnerHints = null;
                PidlIdentity targetPidlIdentity = null;

                if (PartnerSupportsSingleInstancePidls(partnerName)
                    || PXCommon.Constants.PartnerGroups.IsAzureBasedPartner(partnerName)
                    || IsMsegPartner(partnerName)
                    || TemplateHelper.IsListPiButtonListTemplate(setting)
                    || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableSingleInstancePidls, country, setting))
                {
                    optionAction = GetActionType(pi, PIActionType.SelectSingleResource);
                    pidlDocInfo = GetPidlDocInfoByPartner(pi, language, country, partnerName, optionAction.Value);
                    resourceInfo = GetResourceInfoForAction(pi, optionAction, language, country, partnerName);
                    partnerHints = GetPartnerHintsForAction(pi);
                    targetPidlIdentity = new PidlIdentity(
                        Constants.DescriptionTypes.PaymentInstrumentDescription,
                        Constants.PidlOperationTypes.SelectSingleInstance,
                        country,
                        isBackupPidl ? string.Format("{0}.{1}", primaryInstance.PaymentInstrumentId, pi.PaymentInstrumentId) : pi.PaymentInstrumentId);
                }
                else if (IsAmcWebPartner(partnerName) || IsWindowsSettingsPartner(partnerName) || IsListPiRadioButtonTemplate(setting))
                {
                    if (ExpiryActionNeeded(pi))
                    {
                        optionAction = PIActionType.EditPaymentInstrument;
                        pidlDocInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentInstrumentDescription, language, country, partnerName, pi.PaymentMethod.PaymentMethodType, pi.PaymentMethod.PaymentMethodFamily);
                        resourceInfo = new ResourceInfo(Constants.DescriptionTypes.PaymentInstrumentDescription, pi.PaymentInstrumentId, language, country, partnerName);
                    }
                }

                SelectOptionDescription selectOption = new SelectOptionDescription();

                AddDisplayDetailsToPaymentOption(selectOption, country, language, partnerName, optionAction, pi, primaryInstance, targetPidlIdentity, null, pidlDocInfo, resourceInfo, displayHint != null ? displayHint.SelectType : null, partnerHints, exposedFlightFeatures, setting);

                possibleValues.Add(pi.PaymentInstrumentId, selectOption.DisplayText);
                possibleOptions.Add(pi.PaymentInstrumentId, selectOption);
            }

            if (useDisabledPaymentInstruments)
            {
                foreach (PaymentInstrument disabledPi in disabledPaymentInstruments)
                {
                    PIActionType? optionAction = null;
                    PidlDocInfo pidlDocInfo = null;
                    ResourceInfo resourceInfo = null;
                    PartnerHints partnerHints = null;

                    if (PartnerSupportsSingleInstancePidls(partnerName) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableSingleInstancePidls, country, setting))
                    {
                        optionAction = GetActionType(disabledPi, PIActionType.SelectSingleResource);
                        pidlDocInfo = GetPidlDocInfoByPartner(disabledPi, language, country, partnerName, optionAction.Value);
                        resourceInfo = GetResourceInfoForAction(disabledPi, optionAction, language, country, partnerName);
                        partnerHints = GetPartnerHintsForAction(disabledPi);
                    }

                    SelectOptionDescription selectOption = new SelectOptionDescription
                    {
                        IsDisabled = true
                    };

                    AddDisplayDetailsToPaymentOption(selectOption, country, language, partnerName, optionAction, disabledPi, primaryInstance, null, null, pidlDocInfo, resourceInfo, displayHint != null ? displayHint.SelectType : null, partnerHints);

                    possibleValues.Add(disabledPi.PaymentInstrumentId, selectOption.DisplayText);
                    possibleOptions.Add(disabledPi.PaymentInstrumentId, selectOption);
                }
            }

            AddActionToGetPMButton(pidlResource, primaryInstance, Constants.DisplayHintIds.NewPaymentMethodLink, country, language, partnerName);
            AddActionToNewCCButton(pidlResource, Constants.DisplayHintIds.AddNewCCButton, country, language, partnerName);
            AddPaymentMethodLogos(pidlResource, filteredPaymentMethods);

            if (displayHint != null
                && (PXCommon.Constants.PartnerGroups.IsAzureBasedPartner(partnerName)
                || IsMsegPartner(partnerName)
                || IsSetupOfficeSdxPartner(partnerName)
                || IsOxoWebDirectPartner(partnerName)
                || IsOxoDime(partnerName)
                || IsOxoOobe(partnerName)
                || (IsSmbOobePartner(partnerName) && string.Equals(scenario, ScenarioNames.Roobe))
                || IsListPiDropDownTemplate(setting)))
            {
                ButtonDisplayHint newPaymentMethodButton = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.NewPaymentMethodLink) as ButtonDisplayHint;
                SelectOptionDescription selectOption = new SelectOptionDescription { DisplayText = newPaymentMethodButton.DisplayContent, DisplayType = newPaymentMethodButton.DisplayHintType, PidlAction = newPaymentMethodButton.Action };

                // This will avoid new payment method button link inside dropdonw if the partner is Azure based and the PXEnableEmpOrgListPI flight is not enabled. Will add the feaurte to refactore the code
                if ((PXCommon.Constants.PartnerGroups.IsAzureBasedPartner(partnerName) && (exposedFlightFeatures == null || !exposedFlightFeatures.Contains(Flighting.Features.PXEnableEmpOrgListPI, StringComparer.OrdinalIgnoreCase)))
                    || !PXCommon.Constants.PartnerGroups.IsAzureBasedPartner(partnerName))
                {
                    possibleOptions.Add(newPaymentMethodButton.HintId, selectOption);
                    PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<ButtonDisplayHint>(Constants.DisplayHintIds.NewPaymentMethodLink, pidlResource.DisplayPages);
                }
            }
            else if (displayHint != null && PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
            {
                var giftCardSelectOption = CreateSelectOption(
                    Constants.DisplayHintIds.RedeemGiftCardLink,
                    Constants.PaymentMethodOptionStrings.RedeemGiftCard,
                    Constants.PidlUrlConstants.StaticResourceServiceImagesV4 + "/" + Constants.StaticResourceNames.GiftCardSvg,
                    exposedFlightFeatures);

                possibleOptions.Add(Constants.DisplayHintIds.RedeemGiftCardLink, giftCardSelectOption);
                possibleValues.Add(Constants.DisplayHintIds.RedeemGiftCardLink, Constants.DisplayHintIds.RedeemGiftCardLink);

                var newPiSelectOption = CreateSelectOption(
                    Constants.DisplayHintIds.NewPaymentMethodLink,
                    Constants.PaymentMethodOptionStrings.AddAWayToPay,
                    Constants.PidlUrlConstants.StaticResourceServiceImagesV4 + "/" + Constants.StaticResourceNames.AddBoldSvg,
                    exposedFlightFeatures);

                possibleOptions.Add(Constants.DisplayHintIds.NewPaymentMethodLink, newPiSelectOption);
                possibleValues.Add(Constants.DisplayHintIds.NewPaymentMethodLink, Constants.DisplayHintIds.NewPaymentMethodLink);
            }

            // TODO: As part of T-55474531, setDisplayHintDefault is converted into feature to move the selectedPI to first option making it default instance
            // And below IsAmcWebPartner check can removed once mirgated to use the pss feature
            bool setDisplayHintDefault = IsAmcWebPartner(partnerName);

            UpdateSelectDisplay(
                pidlResource,
                displayHintId,
                dataHintId,
                possibleValues,
                possibleOptions,
                defaultInstanceId,
                setDisplayHintDefault,
                exposedFlightFeatures);

            if (paymentInstruments.Count == 0)
            {
                PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<ButtonDisplayHint>(Constants.ButtonDisplayHintIds.UseButton, pidlResource.DisplayPages);
            }
        }

        public static string GetExpiryMonthForListPiTemplate(string expiryMonth)
        {
            if (int.TryParse(expiryMonth, out int month))
            {
                return month.ToString("00");
            }

            return expiryMonth; // Return as is if parsing fails
        }

        internal static bool TryGetPaymentMethodFilters(string filters, out PaymentMethodFilters paymentMethodFilters)
        {
            try
            {
                paymentMethodFilters = GetPaymentMethodFilters(filters);

                return true;
            }
            catch
            {
                paymentMethodFilters = null;

                return false;
            }
        }

        private static bool IsAlternateSVGEnabledPartner(string partner, List<string> exposedFlightFeatures = null)
        {
            // Legacy wise, this was originally a partner based check, but migrating to a feature based check
            return PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) || (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXUseAlternateSVG));
        }

        private static bool HasMatchingExclusionTags(PaymentMethodFilters filtersObject, PaymentMethod pm)
        {
            if (filtersObject.ExclusionTags != null && filtersObject.ExclusionTags.Count() > 0)
            {
                return pm.ExclusionTags != null && filtersObject.ExclusionTags.Intersect(pm.ExclusionTags, StringComparer.OrdinalIgnoreCase).Count() > 0;
            }

            return false;
        }

        private static bool PriceNotMeetsChargeThreshold(PaymentMethod pm, decimal price, string country)
        {
            if (pm.Properties.ChargeThreshold != null)
            {
                var chargeThreshold = pm.Properties.ChargeThreshold.Where(ct => ct.Country.Equals(country, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (chargeThreshold != null)
                {
                    if (price > chargeThreshold.MaxPrice || price < chargeThreshold.MinPrice)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool PaymentMethodsContainsActivePi(HashSet<PaymentMethod> paymentMethods, PaymentInstrument pi)
        {
            return paymentMethods.Any(m => m.EqualByFamilyAndType(pi.PaymentMethod)) && (pi.Status.Equals(PaymentInstrumentStatus.Active) || VerifyActionNeeded(pi) || PaypalBillingAgreementUpdateActionNeeded(pi));
        }

        private static string GetInclusionListId(Dictionary<string, int> inclusionList, PaymentInstrument pi)
        {
            return inclusionList.ContainsKey(GetPaymentMethodFamilyTypeId(pi.PaymentMethod)) ? GetPaymentMethodFamilyTypeId(pi.PaymentMethod) : pi.PaymentMethod.PaymentMethodFamily;
        }

        private static PaymentMethodFilters GetPaymentMethodFilters(string filters)
        {
            if (!string.IsNullOrEmpty(filters))
            {
                try
                {
                    return JObject.Parse(filters).ToObject<PaymentMethodFilters>();
                }
                catch
                {
                    throw new PIDLException("Error deserializing filters query param", Constants.ErrorCodes.PIDLInvalidFilters);
                }
            }
            else
            {
                return new PaymentMethodFilters();
            }
        }

        private static HashSet<PaymentMethod> GetAllowedPaymentMethods(HashSet<PaymentMethod> paymentMethods, string allowedPaymentMethods)
        {
            if (!string.IsNullOrEmpty(allowedPaymentMethods))
            {
                // Allowed payment methods can either be a dictionary or a string array. Try deserializing to dictionary first.
                HashSet<string> includeSet;
                try
                {
                    includeSet = new HashSet<string>(JObject.Parse(allowedPaymentMethods).ToObject<Dictionary<string, int>>().Keys);
                }
                catch
                {
                    try
                    {
                        includeSet = new HashSet<string>(JsonConvert.DeserializeObject<string[]>(allowedPaymentMethods.ToLower()));
                    }
                    catch
                    {
                        throw new PIDLException("Error deserializing allowedPaymentMethods query param", Constants.ErrorCodes.PIDLInvalidAllowedPaymentMethods);
                    }
                }

                if (includeSet != null)
                {
                    return new HashSet<PaymentMethod>(paymentMethods.Where(m => includeSet.Contains(m.PaymentMethodFamily) || includeSet.Contains(GetPaymentMethodFamilyTypeId(m))));
                }
            }

            return new HashSet<PaymentMethod>(paymentMethods);
        }

        private static void PopulatePaymentMethodButtonOptions(PIDLResource pidlResource, HashSet<PaymentMethod> paymentMethods, string language, string country, string partnerName, string displayHintId, string scenario = null)
        {
            Dictionary<string, string> cachedTypes = new Dictionary<string, string>();
            Dictionary<string, SelectOptionDescription> possibleOptions = new Dictionary<string, SelectOptionDescription>();
            Dictionary<string, string> possibleValues = new Dictionary<string, string>();
            PaymentMethod storedValue = null;
            string unlocalizedFirstKeyDisplayText = null;

            foreach (PaymentMethod method in paymentMethods)
            {
                if (method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.StoredValue))
                {
                    // Stored value must be added at the end of the possible values list
                    storedValue = method;
                }
                else
                {
                    if (unlocalizedFirstKeyDisplayText == null)
                    {
                        unlocalizedFirstKeyDisplayText = GetPaymentMethodDisplayText(method, country, false);
                    }

                    string displayText = GetPaymentMethodDisplayText(method, country);
                    if (IsCollapsedPaymentMethodOption(method))
                    {
                        string types = GetCommaSeparatedTypes(method.PaymentMethodFamily, paymentMethods, cachedTypes);
                        string id = string.Format("{0}.{1}", method.PaymentMethodFamily, types);
                        string displayId = GetPaymentMethodFamilyTypeDisplayId(id);

                        if (!possibleValues.ContainsKey(displayId))
                        {
                            possibleValues.Add(displayId, displayText);
                            possibleOptions.Add(displayId, GetPaymentMethodSelectOption(method, id, displayText, types, language, country, partnerName, scenario));
                        }
                    }
                    else
                    {
                        string displayId = GetPaymentMethodFamilyTypeDisplayId(method);

                        possibleValues.Add(displayId, displayText);
                        possibleOptions.Add(displayId, GetPaymentMethodSelectOption(method, GetPaymentMethodFamilyTypeId(method), displayText, method.PaymentMethodType, language, country, partnerName, scenario));
                    }
                }
            }

            if (storedValue != null)
            {
                string displayId = GetPaymentMethodFamilyTypeDisplayId(storedValue);
                string displayText = GetPaymentMethodDisplayText(storedValue, country);

                possibleValues.Add(displayId, displayText);
                possibleOptions.Add(displayId, GetPaymentMethodSelectOption(storedValue, GetPaymentMethodFamilyTypeId(storedValue), displayText, storedValue.PaymentMethodType, language, country, partnerName));

                if (unlocalizedFirstKeyDisplayText == null)
                {
                    unlocalizedFirstKeyDisplayText = GetPaymentMethodDisplayText(storedValue, country, false);
                }
            }

            if (string.Equals(partnerName, Constants.PartnerNames.Xbox, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerNames.Webblends, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerNames.OXODIME, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerNames.OXOOobe, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerNames.OXOWebDirect, StringComparison.OrdinalIgnoreCase))
            {
                string expectedHeading = "Pick a payment method";
                HeadingDisplayHint headingDisplayHint = pidlResource.GetDisplayHintById("paymentMethodSelectHeading") as HeadingDisplayHint;

                if (possibleOptions != null && possibleOptions.Any() && headingDisplayHint != null)
                {
                    if (unlocalizedFirstKeyDisplayText != null)
                    {
                        possibleOptions[possibleOptions.First().Key].AccessibilityTag = PidlModelHelper.GetLocalizedString($"{expectedHeading}. {unlocalizedFirstKeyDisplayText}");
                    }
                    else
                    {
                        possibleOptions[possibleOptions.First().Key].AccessibilityTag = PidlModelHelper.GetLocalizedString($"{expectedHeading}. {possibleOptions[possibleOptions.First().Key].DisplayText}");
                    }
                }
            }

            UpdateSelectDisplay(pidlResource, displayHintId, Constants.DataDescriptionIds.PaymentInstrumentId, possibleValues, possibleOptions);
        }

        private static void PopulatePaymentMethodButtonOptionsGroupByFamily(PIDLResource pidlResource, HashSet<PaymentMethod> paymentMethods, string language, string country, string partnerName, string displayHintId, string scenario = null, List<string> exposedFlightFeatures = null, PaymentExperienceSetting setting = null)
        {
            Dictionary<string, string> cachedTypes = new Dictionary<string, string>();
            Dictionary<string, SelectOptionDescription> possibleOptions = new Dictionary<string, SelectOptionDescription>();
            Dictionary<string, string> possibleValues = new Dictionary<string, string>();

            HashSet<PaymentMethod> storedValue = null;

            Dictionary<string, HashSet<PaymentMethod>> paymentMethodFamilyGroups = new Dictionary<string, HashSet<PaymentMethod>>();
            foreach (PaymentMethod method in paymentMethods)
            {
                string family = method.PaymentMethodFamily;
                string type = method.PaymentMethodType;

                if (string.Equals(family, Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase) && string.Equals(method.PaymentMethodType, Constants.PaymentMethodTypeNames.StoredValue))
                {
                    storedValue = new HashSet<PaymentMethod> { method };
                    continue;
                }

                bool familyIsGrouped = string.Equals(family, Constants.PaymentMethodFamilyNames.CreditCard) || string.Equals(family, Constants.PaymentMethodFamilyNames.MobileBillingNonSim);

                // only credit card and nonsim get grouped, with exception of unionpay (cup) which gets its own group
                bool hasOwnGroup = IsCup(method) || !familyIsGrouped;
                bool createNewGroup = hasOwnGroup || (familyIsGrouped && !paymentMethodFamilyGroups.ContainsKey(family));

                string key = (familyIsGrouped && !IsCup(method)) ? family : $"{family}_{type}";

                if (createNewGroup)
                {
                    paymentMethodFamilyGroups.Add(key, new HashSet<PaymentMethod> { method });
                }
                else
                {
                    paymentMethodFamilyGroups[key].Add(method);
                }
            }

            foreach (KeyValuePair<string, HashSet<PaymentMethod>> entry in paymentMethodFamilyGroups)
            {
                PaymentMethod firstPaymentMethod = entry.Value.First();
                string type = firstPaymentMethod.PaymentMethodType;

                // Unionpay will not have multiple types for comma separation.
                string types = entry.Value.Count > 1
                    ? GetCommaSeparatedTypes(firstPaymentMethod.PaymentMethodFamily, entry.Value, cachedTypes)
                    : firstPaymentMethod.PaymentMethodType;

                string id = string.Format("{0}.{1}", firstPaymentMethod.PaymentMethodFamily, types);

                AddDisplayContentToSelectOptionPossibleValuesAndOptions(entry.Value, possibleValues, possibleOptions, language, country, partnerName, scenario, id, types, cachedTypes, setting: setting);
            }

            // Stored value must be added at the end of the possible values list
            if (storedValue != null && storedValue.Count > 0)
            {
                PaymentMethod firstPaymentMethod = storedValue.First();

                string types = Constants.PaymentMethodTypeNames.StoredValue;
                string id = string.Format("{0}.{1}", firstPaymentMethod.PaymentMethodFamily, types);

                AddDisplayContentToSelectOptionPossibleValuesAndOptions(storedValue, possibleValues, possibleOptions, language, country, partnerName, scenario, id, types, cachedTypes, setting: setting);
            }

            if (string.Equals(partnerName, Constants.PartnerNames.XboxSettings, StringComparison.OrdinalIgnoreCase) &&
                (exposedFlightFeatures?.Contains(Flighting.Features.PXEnableXboxCardUpsellPaymentOptions) ?? false))
            {
                // TODO (48097277): Should not be made available to the public until completed
                possibleOptions.Add("xboxCardApp", GetPaymentMethodSelectXboxCardUpsell(partnerName, scenario));
            }

            UpdateSelectDisplay(pidlResource, displayHintId, Constants.DataDescriptionIds.PaymentInstrumentId, possibleValues, possibleOptions);

            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
            {
                var paymentOptionsGroup = pidlResource.GetDisplayHintById("paymentOptionsGroup") as ContainerDisplayHint;
                var pm = paymentOptionsGroup?.Members?.First() as PropertyDisplayHint;
                AddXboxNativeSelectOptionAccessibilityTag(pm);
            }
        }

        private static void AddDisplayContentToSelectOptionPossibleValuesAndOptions(HashSet<PaymentMethod> paymentMethods, Dictionary<string, string> possibleValues, Dictionary<string, SelectOptionDescription> possibleOptions, string language, string country, string partnerName, string scenario, string id, string types, Dictionary<string, string> cachedTypes = null, PaymentExperienceSetting setting = null)
        {
            PaymentMethod firstPaymentMethod = paymentMethods.First();

            string displayText = GetPaymentMethodDisplayText(firstPaymentMethod, country);
            string displayId = GetPaymentMethodFamilyTypeDisplayId(id);

            if (!possibleValues.ContainsKey(displayId))
            {
                possibleValues.Add(displayId, displayText);
            }

            possibleOptions.Add(displayId, GetSelectOptionForMultiplePaymentMethods(paymentMethods, id, displayText, types, language, country, partnerName, scenario, setting: setting));
        }

        private static SelectOptionDescription GetPaymentMethodSelectXboxCardUpsell(string partnerName, string scenario = null, List<string> exposedFlightFeatures = null)
        {
            // TODO (48097277): This code block represents a placeholder for the future xbox card upsell in Select PM
            // Will be updated with proper content in the future.
            TextDisplayHint paymentOptionText = new TextDisplayHint() { HintId = Constants.TextDisplayHintIds.PaymentOptionText };
            paymentOptionText.DisplayContent = "Upsell Placeholder";

            GroupDisplayHint paymentOptionTextGroup = new GroupDisplayHint { HintId = Constants.GroupDisplayHintIds.PaymentOptionTextGroup };
            paymentOptionTextGroup.AddDisplayHint(paymentOptionText);

            GroupDisplayHint paymentMethodOption = new GroupDisplayHint { HintId = string.Format($"{Constants.GroupDisplayHintIds.PaymentMethodOption}_{0}", "xboxMasterCard") };

            paymentMethodOption.AddDisplayHint(paymentOptionTextGroup);

            SelectOptionDescription selectOption = new SelectOptionDescription { DisplayContent = paymentMethodOption };

            ActionContext optionContext = new ActionContext()
            {
                Action = PaymentInstrumentActions.ToString(PIActionType.ApplyPaymentInstrument),
                Id = "upsellPlaceholder",
                PaymentMethodFamily = "credit_card",
                PaymentMethodType = "mc"
            };

            ResourceActionContext resourceActionContext = new ResourceActionContext()
            {
                Action = PaymentInstrumentActions.ToString(PIActionType.ApplyPaymentInstrument)
            };
            optionContext.ResourceActionContext = resourceActionContext;

            selectOption.PidlAction = CreateSuccessPidlAction(optionContext, false);
            selectOption.AccessibilityTag = "Upsell Placeholder";

            return selectOption;
        }

        private static SelectOptionDescription GetPaymentMethodSelectOptionWithDisplayContent(HashSet<PaymentMethod> paymentMethods, string partnerName, int maxAllowedLogosPerOption, string scenario = null, List<string> exposedFlightFeatures = null, PaymentExperienceSetting setting = null)
        {
            PaymentMethod firstPaymentMethod = paymentMethods.First();

            TextDisplayHint paymentOptionText = new TextDisplayHint() { HintId = Constants.TextDisplayHintIds.PaymentOptionText };
            paymentOptionText.DisplayContent = GetPaymentMethodDisplayText(firstPaymentMethod);

            GroupDisplayHint paymentOptionTextGroup = new GroupDisplayHint { HintId = Constants.GroupDisplayHintIds.PaymentOptionTextGroup };
            paymentOptionTextGroup.AddDisplayHint(paymentOptionText);

            GroupDisplayHint paymentOptionLogosGroup = new GroupDisplayHint { HintId = Constants.GroupDisplayHintIds.MultiplePaymentMethodLogosRowOneGroup, LayoutOrientation = Constants.PartnerHintsValues.InlinePlacement };

            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
            {
                paymentOptionText.AddDisplayTag(Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Polite);
            }

            foreach (PaymentMethod paymentMethod in paymentMethods.Take(maxAllowedLogosPerOption))
            {
                string alternateSvgForPartner = CheckForReactNativeAlternatePaymentMethodLogoUrl(paymentMethod, partnerName, exposedFlightFeatures);
                string hintId = $"{GetPaymentMethodFamilyTypeDisplayId(paymentMethod)}_logo";

                // If multiple logos are possible, we need to give a special id when only a single logo is present for react-native styling purposes.
                if (paymentMethods.Count == 1)
                {
                    bool canHaveMultipleLogos = string.Equals(paymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(paymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.MobileBillingNonSim, StringComparison.OrdinalIgnoreCase);

                    if (canHaveMultipleLogos)
                    {
                        hintId = $"single-{hintId}";
                    }
                }

                ImageDisplayHint paymentOptionLogo = new ImageDisplayHint
                {
                    HintId = hintId,
                    SourceUrl = alternateSvgForPartner ?? GetPaymentMethodLogoUrl(paymentMethod),
                    AccessibilityName = PidlModelHelper.GetLocalizedString($"{paymentMethod.Display.Name} logo,")
                };

                if (alternateSvgForPartner != null)
                {
                    // alternate svg don't scale well so we need a group wrapper to allow a background colored rectangle matching the size of the standard logos
                    GroupDisplayHint alternativeSvgWrapper = new GroupDisplayHint { HintId = $"{Constants.GroupDisplayHintIds.AlternativeSvgLogoWrapper}{hintId}" };

                    alternativeSvgWrapper.AddDisplayHint(paymentOptionLogo);
                    paymentOptionLogosGroup.AddDisplayHint(alternativeSvgWrapper);
                }
                else
                {
                    paymentOptionLogosGroup.AddDisplayHint(paymentOptionLogo);
                }
            }

            if (paymentMethods.Count > maxAllowedLogosPerOption)
            {
                TextDisplayHint plusMore = new TextDisplayHint { HintId = Constants.TextDisplayHintIds.PlusMore, DisplayContent = Constants.TextDisplayHintContents.PlusMore };

                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                {
                    plusMore.AddDisplayTag(Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Polite);
                }

                GroupDisplayHint multiLogosRowTwoGroup = new GroupDisplayHint { HintId = Constants.GroupDisplayHintIds.MultiplePaymentMethodLogosRowOneGroup, LayoutOrientation = Constants.PartnerHintsValues.InlinePlacement };
                multiLogosRowTwoGroup.AddDisplayHint(plusMore);

                paymentOptionLogosGroup.AddDisplayHint(multiLogosRowTwoGroup);
            }

            GroupDisplayHint paymentMethodOption = new GroupDisplayHint { HintId = string.Format($"{Constants.GroupDisplayHintIds.PaymentMethodOption}_{0}", firstPaymentMethod.PaymentMethodType) };

            // for SelectPMWithLogo scenario or if its SelectPMRadioButtonList template partner , paymentOptionTextGroup is not needed because it is redundant with displayText
            List<DisplayHint> paymentMethodOptionMembers = (string.Equals(scenario, Constants.ScenarioNames.SelectPMWithLogo, StringComparison.OrdinalIgnoreCase) || IsSelectPmRadioButtonListPartner(partnerName, setting: setting))
                ? new List<DisplayHint> { paymentOptionLogosGroup }
                : new List<DisplayHint> { paymentOptionLogosGroup, paymentOptionTextGroup };
            paymentMethodOption.AddDisplayHints(paymentMethodOptionMembers);

            SelectOptionDescription selectOption = new SelectOptionDescription { DisplayContent = paymentMethodOption };

            return selectOption;
        }

        private static SelectOptionDescription GetSelectOptionForMultiplePaymentMethods(HashSet<PaymentMethod> paymentMethods, string id, string displayText, string type, string language, string country, string partnerName, string scenario, PaymentExperienceSetting setting = null)
        {
            int maxAllowedLogos = MaxAllowedLogosPerSelectOptionForPartner.ContainsKey(partnerName) ? MaxAllowedLogosPerSelectOptionForPartner[partnerName] : Constants.MaxAllowedPaymentMethodLogos.Six;

            SelectOptionDescription selectOption = GetPaymentMethodSelectOptionWithDisplayContent(paymentMethods, partnerName, maxAllowedLogos, scenario, setting: setting);

            string paymentMethodFamily = paymentMethods.First().PaymentMethodFamily;

            ActionContext optionContext = new ActionContext()
            {
                Action = PaymentInstrumentActions.ToString(PIActionType.AddResource),
                Id = id,
                PaymentMethodFamily = paymentMethodFamily,
                PaymentMethodType = type
            };

            ResourceActionContext resourceActionContext = new ResourceActionContext()
            {
                Action = PaymentInstrumentActions.ToString(PIActionType.AddResource),
                PidlDocInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentInstrumentDescription, language, country, partnerName, type, paymentMethodFamily),
            };
            optionContext.ResourceActionContext = resourceActionContext;

            selectOption.PidlAction = CreateSuccessPidlAction(optionContext, false);
            selectOption.DisplayText = displayText;
            selectOption.AccessibilityTag = displayText;

            return selectOption;
        }

        private static SelectOptionDescription GetPaymentMethodSelectOption(PaymentMethod method, string id, string displayText, string type, string language, string country, string partnerName, string scenario = null)
        {
            SelectOptionDescription selectOption = new SelectOptionDescription { DisplayText = displayText };

            ActionContext optionContext = new ActionContext()
            {
                Action = PaymentInstrumentActions.ToString(PIActionType.AddResource),
                Id = id,
                PaymentMethodFamily = method.PaymentMethodFamily,
                PaymentMethodType = type
            };

            ResourceActionContext resourceActionContext = new ResourceActionContext()
            {
                Action = PaymentInstrumentActions.ToString(PIActionType.AddResource),
                PidlDocInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentInstrumentDescription, language, country, partnerName, type, method.PaymentMethodFamily)
            };

            // For scenario eligiblePI, return partnerAction to partner, then partner will call invokeResourceAction to load the add PI Pidl
            // For others, return success to partner and PIDL SDK handles the entire flow
            optionContext.ResourceActionContext = resourceActionContext;
            selectOption.PidlAction = string.Equals(scenario, Constants.ScenarioNames.EligiblePI, StringComparison.OrdinalIgnoreCase) ? CreatePartnerActionPidlAction(optionContext, false) : CreateSuccessPidlAction(optionContext, false);
            if (string.Equals(partnerName, Constants.PartnerNames.Webblends, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerNames.Xbox, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerNames.OXODIME, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerNames.OXOOobe, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerNames.OXOWebDirect, StringComparison.OrdinalIgnoreCase))
            {
                selectOption.AccessibilityTag = displayText;
            }

            return selectOption;
        }

        private static void PopulatePaymentMethodSelectOptions(PIDLResource pidlResource, HashSet<PaymentMethod> paymentMethods, string defaultPaymentMethod, string country, string displayHintId = Constants.DisplayHintIds.PaymentMethodSelect, PaymentExperienceSetting setting = null, string language = null, string partnerName = null, string scenario = null)
        {
            Dictionary<string, string> cachedTypes = new Dictionary<string, string>();
            Dictionary<string, SelectOptionDescription> possibleOptions = new Dictionary<string, SelectOptionDescription>();
            PaymentMethod storedValue = null;

            var idProperty = pidlResource.DataDescription[Constants.DataDescriptionIds.PaymentInstrumentId] as PropertyDescription;
            var familyProperty = pidlResource.DataDescription[Constants.DataDescriptionIds.PaymentInstrumentFamily] as PropertyDescription;
            var typeProperty = pidlResource.DataDescription[Constants.DataDescriptionIds.PaymentInstrumentType] as PropertyDescription;
            var actionProperty = pidlResource.DataDescription[Constants.DataDescriptionIds.PaymentInstrumentAction] as PropertyDescription;
            var displayIdProperty = pidlResource.DataDescription[Constants.DataDescriptionIds.PaymentInstrumentDisplayId] as PropertyDescription;

            string selectAction = PaymentInstrumentActions.ToString(PIActionType.AddResource);

            // Added for the webblends scenario which requires use of two different selectpms, one a dropdown, simultaneously
            if (!displayHintId.Equals(Constants.DisplayHintIds.PaymentMethodSelectDropdown, StringComparison.OrdinalIgnoreCase))
            {
                displayHintId = Constants.DisplayHintIds.PaymentMethodSelect;
            }

            // TODO: Add indexedOn to csv so this is not needed
            idProperty.IndexedOn = "displayId";
            familyProperty.IndexedOn = "displayId";
            typeProperty.IndexedOn = "displayId";
            actionProperty.IndexedOn = "displayId";

            familyProperty.SetSkipPossibleValuesLocalization(true);
            typeProperty.SetSkipPossibleValuesLocalization(true);

            PaymentMethod parsedDefaultPaymentMethod = new PaymentMethod();
            parsedDefaultPaymentMethod.PaymentMethodFamily = defaultPaymentMethod?.Split('.').FirstOrDefault() ?? string.Empty;
            parsedDefaultPaymentMethod.PaymentMethodType = defaultPaymentMethod?.Split('.').Skip(1).FirstOrDefault() ?? string.Empty;
            string defaultPossibleValue = null;
            GroupDisplayHint logoInGroupForTemplate = new GroupDisplayHint();
            string displayIdForTemplate = string.Empty;

            foreach (PaymentMethod method in paymentMethods)
            {
                if (method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.StoredValue))
                {
                    // Stored value must be added at the end of the possible values list
                    storedValue = method;
                }
                else
                {
                    string displayText = GetPaymentMethodDisplayText(method, country);
                    string displayId = GetPaymentMethodFamilyTypeDisplayId(method);
                    string id = GetPaymentMethodFamilyTypeId(method);

                    if (IsCollapsedPaymentMethodOption(method))
                    {
                        string types = GetCommaSeparatedTypes(method.PaymentMethodFamily, paymentMethods, cachedTypes);

                        id = string.Format("{0}.{1}", method.PaymentMethodFamily, types);
                        displayId = GetPaymentMethodFamilyTypeDisplayId(id);

                        if (displayIdProperty.PossibleValues != null && displayIdProperty.PossibleValues.ContainsKey(displayId))
                        {
                            if (TemplateHelper.IsSelectPMDropDownTemplate(setting))
                            {
                                ImageDisplayHint logoInGroup = new ImageDisplayHint
                                {
                                    HintId = V7.Constants.DisplayHintIdPrefixes.PaymentOptionLogo + method.PaymentMethodType,
                                    SourceUrl = GetPaymentMethodLogoUrl(method),
                                    AccessibilityName = method.Display.Name,
                                    StyleHints = new List<string>() { Constants.StyleHints.ImageHeightSmall, Constants.StyleHints.AlignHorizontalCenter }
                                };

                                displayIdForTemplate = displayId;
                                logoInGroupForTemplate.StyleHints = new List<string> { Constants.StyleHints.GapSmall, Constants.StyleHints.DirectionHorizontal };
                                logoInGroupForTemplate.Members.Add(logoInGroup);
                            }

                            continue;
                        }

                        typeProperty.AddPossibleValue(displayId, types);
                    }
                    else
                    {
                        typeProperty.AddPossibleValue(displayId, method.PaymentMethodType);
                    }

                    familyProperty.AddPossibleValue(displayId, method.PaymentMethodFamily);

                    displayIdProperty.AddPossibleValue(displayId, displayText);
                    actionProperty.AddPossibleValue(displayId, selectAction);
                    idProperty.AddPossibleValue(displayId, id);

                    if (TemplateHelper.IsSelectPMDropDownTemplate(setting))
                    {
                        GroupDisplayHint displayContent = CreateLogoAndTextGroupDisplayHint(displayId, displayText, GetPaymentMethodLogoUrl(method), method.Display.Name);

                        if (logoInGroupForTemplate.Members.Count > 1)
                        {
                            var logoImage = possibleOptions[displayIdForTemplate].DisplayContent.Members.FirstOrDefault(logoHintId => logoHintId.DisplayHintType == Constants.DisplayHintTypes.Image) as ImageDisplayHint;

                            if (logoImage != null)
                            {
                                possibleOptions[displayIdForTemplate].DisplayContent.Members.Remove(logoImage);
                                logoInGroupForTemplate.Members.Add(logoImage);
                                possibleOptions[displayIdForTemplate].DisplayContent.Members.Add(logoInGroupForTemplate);
                            }
                        }

                        logoInGroupForTemplate = new GroupDisplayHint();
                        possibleOptions.Add(displayId, new SelectOptionDescription { DisplayText = displayText, DisplayContent = displayContent });
                    }
                    else
                    {
                        possibleOptions.Add(displayId, new SelectOptionDescription { DisplayText = displayText });
                    }
                }
            }

            if (storedValue != null)
            {
                string displayId = GetPaymentMethodFamilyTypeDisplayId(storedValue);
                string displayText = GetPaymentMethodDisplayText(storedValue, country);
                GroupDisplayHint displayContent = null;

                if (TemplateHelper.IsSelectPMDropDownTemplate(setting))
                {
                    displayContent = CreateLogoAndTextGroupDisplayHint(displayId, displayText, GetPaymentMethodLogoUrl(storedValue), storedValue.Display.Name);
                }
                else
                {
                    familyProperty.AddPossibleValue(displayId, storedValue.PaymentMethodFamily);
                    typeProperty.AddPossibleValue(displayId, storedValue.PaymentMethodType);

                    displayIdProperty.AddPossibleValue(displayId, displayText);
                    actionProperty.AddPossibleValue(displayId, selectAction);
                    idProperty.AddPossibleValue(displayId, GetPaymentMethodFamilyTypeId(storedValue));
                }

                possibleOptions.Add(displayId, new SelectOptionDescription { DisplayText = displayText, DisplayContent = displayContent });
            }

            if (!string.IsNullOrEmpty(parsedDefaultPaymentMethod.PaymentMethodFamily) && !string.IsNullOrEmpty(parsedDefaultPaymentMethod.PaymentMethodType))
            {
                defaultPossibleValue = GetPaymentMethodFamilyTypeDisplayId(parsedDefaultPaymentMethod);
            }
            else if (!string.IsNullOrEmpty(parsedDefaultPaymentMethod.PaymentMethodFamily) && IsCollapsedPaymentMethodOption(parsedDefaultPaymentMethod))
            {
                defaultPossibleValue = GetPaymentMethodFamilyTypeDisplayId(
                    string.Format("{0}_{1}", parsedDefaultPaymentMethod.PaymentMethodFamily, GetCommaSeparatedTypes(parsedDefaultPaymentMethod.PaymentMethodFamily, paymentMethods, cachedTypes)));
            }

            UpdateSelectDisplay(pidlResource, displayHintId, Constants.DataDescriptionIds.PaymentInstrumentDisplayId, displayIdProperty.PossibleValues, possibleOptions, defaultPossibleValue, defaultPossibleValue != null);
        }

        private static decimal GetPriceToken(PaymentMethodFilters filtersObject)
        {
            return filtersObject.ChargeThreshold.HasValue ?
                filtersObject.ChargeThreshold.Value :
                filtersObject.ChargeThresholds != null ? filtersObject.ChargeThresholds.Sum() : 0.0M;
        }

        /// <summary>
        /// Creates a GroupDisplayHint that includes a logo and text group display hint.
        /// </summary>
        /// <param name="displayId">The display ID for the hint.</param>
        /// <param name="displayText">The text to be displayed.</param>
        /// <param name="sourceUrl">The URL of the logo image.</param>
        /// <param name="accessibilityName">The accessibility name for the display name.</param>
        /// <returns>A GroupDisplayHint containing the logo and text group display hints.</returns>
        private static GroupDisplayHint CreateLogoAndTextGroupDisplayHint(string displayId, string displayText, string sourceUrl, string accessibilityName)
        {
            // Create the ImageDisplayHint for the logo
            ImageDisplayHint logo = new ImageDisplayHint
            {
                HintId = V7.Constants.DisplayHintIdPrefixes.PaymentOptionLogo + displayId,
                SourceUrl = sourceUrl,
                AccessibilityName = accessibilityName,
                StyleHints = new List<string>() { Constants.StyleHints.ImageHeightSmall, Constants.StyleHints.AlignHorizontalCenter }
            };

            // Create the TextGroupDisplayHint for the display text
            TextGroupDisplayHint textGroup = new TextGroupDisplayHint
            {
                HintId = V7.Constants.DisplayHintIdPrefixes.PaymentOptionTextGroup + displayId,
                StyleHints = new List<string>() { Constants.StyleHints.GapSmall, Constants.StyleHints.WidthFill },
                Members = new List<DisplayHint>
                {
                    new TextDisplayHint
                    {
                        DisplayContent = displayText,
                        HintId = V7.Constants.DisplayHintIdPrefixes.PaymentOptionText + displayId
                    }
                }
            };

            // Create the GroupDisplayHint for the display content
            GroupDisplayHint displayContent = new GroupDisplayHint
            {
                HintId = V7.Constants.DisplayHintIdPrefixes.PaymentOptionContainer + displayId,
                StyleHints = new List<string>() { Constants.StyleHints.WidthFill, Constants.StyleHints.AlignverticalCenter },
                LayoutOrientation = Constants.PartnerHintsValues.InlinePlacement,
                Members = new List<DisplayHint>
                {
                    textGroup, // Add the text group
                    logo // Add the logo
                }
            };

            return displayContent;
        }

        private static List<string> GetPaymentInstrumentDisplayTextSeparateNameAndNumber(PaymentInstrument pi, string partnerName, string language, List<string> flightNames = null)
        {
            PaymentInstrumentDetails details = pi.PaymentInstrumentDetails;
            PaymentMethod method = pi.PaymentMethod;

            string line1 = string.Empty;
            string line2 = string.Empty;
            string line3 = string.Empty;

            if (IsCreditCardNotCup(method))
            {
                line1 = details.CardHolderName;
                line2 = string.Format(PidlModelHelper.GetLocalizedString("Ending in", language) + " \u2022\u2022{0}", details.LastFourDigits);
                line3 = string.Format(PidlModelHelper.GetLocalizedString("Exp", language) + " {1}/{2}", details.LastFourDigits, details.ExpiryMonth, details.ExpiryYear.Substring(2));

                if (ExpiryActionNeeded(pi))
                {
                    line3 = string.Format(PidlModelHelper.GetLocalizedString("Expired", language) + " {1}/{2}", details.LastFourDigits, details.ExpiryMonth, details.ExpiryYear.Substring(2));
                }

                if (flightNames != null && flightNames.Contains(Constants.PartnerFlightValues.IndiaExpiryGroupDelete) && PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                {
                    line3 = string.Empty;
                }
            }
            else if (string.Equals(method.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.DirectDebit, StringComparison.OrdinalIgnoreCase))
            {
                if (method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.IdealBillingAgreement, StringComparison.OrdinalIgnoreCase))
                {
                    line1 = details.Issuer;
                    line2 = string.Format(PidlModelHelper.GetLocalizedString("Ending in", language) + " \u2022\u2022{0}", details.BankAccountLastFourDigits);
                }
                else
                {
                    line1 = details.CardHolderName;
                    line2 = string.Format(PidlModelHelper.GetLocalizedString("Ending in", language) + " \u2022\u2022{0}", details.LastFourDigits);
                }
            }
            else if (IsEmail(method) || string.Equals(method.PaymentMethodType, Constants.PaymentMethodTypeNames.Klarna, StringComparison.OrdinalIgnoreCase))
            {
                line1 = details.Email;
            }
            else if (string.Equals(method.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.MobileBillingNonSim, StringComparison.OrdinalIgnoreCase))
            {
                line1 = pi.PaymentMethod.Display.Name;
                line2 = details.Msisdn;
            }
            else if (string.Equals(method.PaymentMethodType, Constants.PaymentMethodTypeNames.Venmo, StringComparison.OrdinalIgnoreCase))
            {
                line1 = pi.PaymentInstrumentDetails.UserName;
            }
            else if (!string.Equals(method.PaymentMethodType, Constants.PaymentMethodTypeNames.Paysafecard, StringComparison.OrdinalIgnoreCase))
            {
                line1 = pi.PaymentMethod.Display.Name;
            }

            line1 = TruncateLongerDisplayText(line1);

            return new List<string>()
            {
                line1,
                line2,
                line3,
            };
        }

        private static string TruncateLongerDisplayText(string text)
        {
            if (text == null || text.Length <= 20)
            {
                return text;
            }

            return $"{text.Substring(0, 20)}...";
        }

        private static PidlDocInfo GetPidlDocInfoByPartner(PaymentInstrument pi, string language, string country, string partnerName, PIActionType action)
        {
            PidlDocInfo pidlDocInfo = null;

            if (action == PIActionType.UpdateResource)
            {
                pidlDocInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentInstrumentDescription, language, country, partnerName, pi.PaymentMethod.PaymentMethodType, pi.PaymentMethod.PaymentMethodFamily);
            }
            else
            {
                pidlDocInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentInstrumentDescription, language, country, partnerName);
            }

            return pidlDocInfo;
        }

        private static PartnerHints GetPartnerHintsForAction(PaymentInstrument pi)
        {
            PartnerHints partnerHints = null;

            if (PaypalBillingAgreementUpdateActionNeeded(pi))
            {
                partnerHints = new PartnerHints()
                {
                    Placement = Constants.PartnerHintsValues.PopupPlacement
                };
            }

            return partnerHints;
        }

        private static bool IsReturnBackupPidlFeatureEnabled(string country, PaymentExperienceSetting setting)
        {
            if (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ReturnBackupPidlForSplitPaymentSupported, country, setting))
            {
                // Only enable the feature for listpiradiobutton template
                return string.Equals(setting.Template, Constants.TemplateName.ListPiRadioButton, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static ResourceInfo GetResourceInfoForAction(PaymentInstrument pi, PIActionType? action, string language, string country, string partnerName)
        {
            ResourceInfo resourceInfo = null;

            if (action == PIActionType.HandleChallenge || action == PIActionType.UpdateResource)
            {
                resourceInfo = new ResourceInfo(Constants.DescriptionTypes.PaymentInstrumentDescription, pi.PaymentInstrumentId, language, country, partnerName);
            }

            return resourceInfo;
        }

        private static PIDLResource GetPaymentInstrumentPidlResource(string partnerName, string country, string operation, string pidlId, string scenario = null, string resourceId = null, List<string> flightNames = null, PaymentExperienceSetting setting = null)
        {
            return GetPidlResource(partnerName, country, operation, pidlId, resourceId, Constants.DescriptionTypes.PaymentInstrumentDescription, scenario: scenario, flightNames: flightNames, setting: setting);
        }

        private static PIDLResource GetBillingGroupPidlResource(string partnerName, string country, string operation, string pidlId, string resourceId = null, List<string> flightNames = null, PaymentExperienceSetting setting = null)
        {
            return GetPidlResource(partnerName, country, operation, pidlId, resourceId, Constants.DescriptionTypes.BillingGroupDescription, flightNames, setting: setting);
        }

        private static PIDLResource GetAddressGroupPidlResource(string partnerName, string country, string operation, string pidlId, string resourceId = null)
        {
            return GetPidlResource(partnerName, country, operation, pidlId, resourceId, Constants.DescriptionTypes.AddressGroupDescription);
        }

        private static PIDLResource GetPidlResource(
            string partnerName,
            string country,
            string operation,
            string pidlId,
            string resourceId = null,
            string descriptionType = Constants.DescriptionTypes.PaymentInstrumentDescription,
            List<string> flightNames = null,
            string classicProduct = null,
            string billableAccountId = null,
            string scenario = null,
            PaymentExperienceSetting setting = null)
        {
            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>
                {
                        { Constants.DescriptionIdentityFields.DescriptionType, descriptionType },
                        { Constants.DescriptionIdentityFields.Operation, operation },
                        { Constants.DescriptionIdentityFields.Country, country }
                });

            if (descriptionType.Equals(Constants.DescriptionTypes.PaymentInstrumentDescription, StringComparison.Ordinal))
            {
                retVal.Identity.Add(Constants.DescriptionIdentityFields.ResourceIdentity, resourceId != null ? resourceId : pidlId);
            }

            PIDLResourceFactory.Instance.GetPIDLResourceRecursive(
                partnerName,
                descriptionType,
                pidlId,
                country,
                operation,
                retVal,
                flightNames: flightNames,
                classicProduct: classicProduct,
                billableAccountId: billableAccountId,
                scenario: scenario,
                setting: setting);

            return retVal;
        }

        private static PIActionType? GetActionType(PaymentInstrument pi, PIActionType? defaultAction)
        {
            if (ExpiryActionNeeded(pi))
            {
                return PIActionType.UpdateResource;
            }
            else if (VerifyActionNeeded(pi) || PaypalBillingAgreementUpdateActionNeeded(pi))
            {
                return PIActionType.HandleChallenge;
            }

            return defaultAction;
        }

        private static void RemoveZeroBalanceCSV(List<PaymentInstrument> paymentInstruments)
        {
            paymentInstruments.RemoveAll(pi => string.Equals(pi.PaymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase)
                && string.Equals(pi.PaymentMethod.PaymentMethodType, Constants.PaymentMethodTypeNames.StoredValue, StringComparison.OrdinalIgnoreCase)
                && pi.PaymentInstrumentDetails.Balance == decimal.Zero);
        }

        private static void RemoveExpiredPaymentInstruments(List<PaymentInstrument> paymentInstruments, PaymentMethodFilters filtersObject)
        {
            if (filtersObject.FilterExpiredPayment.HasValue && filtersObject.FilterExpiredPayment.Value)
            {
                paymentInstruments.RemoveAll(pi => ExpiryActionNeeded(pi));
            }
        }

        private static void RemovePrepaidPaymentInstruments(List<PaymentInstrument> paymentInstruments, PaymentMethodFilters filtersObject)
        {
            if (filtersObject.FilterPrepaidCards.HasValue && filtersObject.FilterPrepaidCards.Value)
            {
                paymentInstruments.RemoveAll(pi => pi.PaymentInstrumentDetails.CardType != null && pi.PaymentInstrumentDetails.CardType.Equals("prepaid", StringComparison.InvariantCultureIgnoreCase));
            }
        }

        private static bool VerifyActionNeeded(PaymentInstrument pi)
        {
            return pi.Status.Equals(PaymentInstrumentStatus.Pending) &&
                pi.PaymentInstrumentDetails != null &&
                !string.IsNullOrEmpty(pi.PaymentInstrumentDetails.PendingOn) &&
                string.Equals(pi.PaymentInstrumentDetails.PendingOn, Constants.PendingOnOperations.Picv, StringComparison.OrdinalIgnoreCase);
        }

        private static bool PaypalBillingAgreementUpdateActionNeeded(PaymentInstrument pi)
        {
            return pi.Status.Equals(PaymentInstrumentStatus.Pending) &&
                pi.PaymentMethod != null &&
                string.Equals(pi.PaymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(pi.PaymentMethod.PaymentMethodType, Constants.PaymentMethodTypeNames.Paypal, StringComparison.OrdinalIgnoreCase) &&
                pi.PaymentInstrumentDetails != null &&
                string.Equals(pi.PaymentInstrumentDetails.BillingAgreementType, Constants.PayPalBillingAgreementTypes.MerchantInitiatedBilling, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(pi.PaymentInstrumentDetails.PendingOn, Constants.PendingOnOperations.AgreementUpdate, StringComparison.OrdinalIgnoreCase);
        }

        private static List<PaymentInstrument> GetBackupPiOptions(List<PaymentInstrument> paymentInstruments, PaymentInstrument storedValue, decimal value)
        {
            if (storedValue.PaymentInstrumentDetails.Balance < value)
            {
                List<PaymentInstrument> retList = new List<PaymentInstrument>(paymentInstruments);
                retList.RemoveAll(pi => !pi.PaymentMethod.Properties.SplitPaymentSupported || IsStoredValue(pi.PaymentMethod));

                return retList;
            }

            return null;
        }

        // Returns a success client action with return context to the partner to skip the current action
        // and go straight to addResource or selectResourceType
        private static PIDLResource GetAddResourceClientActionResponse(
            string partnerName,
            string country,
            string language,
            List<string> exposedFlightFeatures = null,
            string id = null,
            string paymentMethodFamily = null,
            string paymentMethodType = null)
        {
            // TODO: As part of PR-11875566, this entire if-elseif-else block is converted into SkipSelectPM, & SkipSelectInstanceNoPI feature with cutomizations for each if flow
            // Feature Customizations: EnableBackupPICheckForSkipSelectInstanceNoPI, AddTriggeredByForSkipSelectInstanceNoPI, ReturnAddCCOnlyForSkipSelectInstanceNoPI
            // these partner conditions can be removed once migrated to use the pss feature
            if (AddResourceCreditCardOnly(partnerName) || IsPXSkipGetPMCCOnly(country, partnerName))
            {
                return GetAddCreditCardClientActionResponse(partnerName, country, language, id, paymentMethodFamily, paymentMethodType);
            }
            else if (IsAmcWebPartner(partnerName))
            {
                PidlDocInfo pidlDocInfo = new PidlDocInfo(
                Constants.DescriptionTypes.PaymentInstrumentDescription,
                language,
                country,
                partnerName);
                var partnerHints = new PartnerHints()
                {
                    TriggeredBy = PartnerHintsValues.TriggeredByEmptyResourceList
                };

                ActionContext actionContext = CreatePaymentInstrumentActionContext(null, PIActionType.AddPaymentInstrument, pidlDocInfo, null, partnerHints);
                return new PIDLResource()
                {
                    ClientAction = new ClientAction(ClientActionType.ReturnContext, actionContext)
                };
            }
            else
            {
                Dictionary<string, object> clientContext = new Dictionary<string, object>();
                ActionContext actionContext = new ActionContext();
                actionContext.Action = PaymentInstrumentActions.ToString(PIActionType.SelectResourceType);
                actionContext.ResourceActionContext = new ResourceActionContext()
                {
                    Action = actionContext.Action,
                    PidlDocInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentInstrumentDescription, language, country, partnerName)
                };

                return new PIDLResource()
                {
                    ClientAction = new ClientAction(ClientActionType.ReturnContext, actionContext)
                };
            }
        }

        private static PIDLResource GetAddCreditCardClientActionResponse(
            string partnerName,
            string country,
            string language,
            string id,
            string paymentMethodFamily,
            string paymentMethodType,
            List<string> exposedFlightFeatures = null)
        {
            Dictionary<string, object> clientContext = new Dictionary<string, object>();
            ActionContext actionContext = new ActionContext();

            // resourceActionContext to be used by new partners
            actionContext.Action = PaymentInstrumentActions.ToString(PIActionType.AddResource);
            actionContext.ResourceActionContext = new ResourceActionContext()
            {
                Action = actionContext.Action,
                PidlDocInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentInstrumentDescription, language, country, partnerName, null, Constants.PaymentMethodFamilyNames.CreditCard)
            };

            // id, paymentMethodFamily and paymentMethodType to be used by webblends
            actionContext.Id = id;
            actionContext.PaymentMethodFamily = paymentMethodFamily;
            actionContext.PaymentMethodType = paymentMethodType;

            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableIsSelectPMSkippedValue, StringComparer.OrdinalIgnoreCase))
            {
                actionContext.IsSelectPMSkipped = true;
            }

            return new PIDLResource()
            {
                ClientAction = new ClientAction(ClientActionType.ReturnContext, actionContext)
            };
        }

        /// <summary>
        /// Populate possiblevalues and possibleoptions of pidlResource dropdown with payment instruments list
        /// </summary>
        /// <param name="pidlResource"> PIDL resource of searchTransactions </param>
        /// <param name="partnerName"> The name of the partner </param>
        /// <param name="paymentInstruments"> PI or payment instrument list </param>
        /// <param name="language"> language code </param>
        private static void PopulateSearchInstancePidl(PIDLResource pidlResource, string partnerName, List<PaymentInstrument> paymentInstruments, string language)
        {
            string dataHintId = Constants.DataDescriptionIds.PaymentInstrumentId;
            string displayHintId = Constants.DisplayHintIds.PaymentInstrumentSearchId;

            Dictionary<string, SelectOptionDescription> possibleOptions = new Dictionary<string, SelectOptionDescription>();
            Dictionary<string, string> possibleValues = new Dictionary<string, string>();

            if (paymentInstruments.Count == 0)
            {
                var searchTransactionsSubHeading = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentInstrumentSearchTransactionsSubHeading);
                if (searchTransactionsSubHeading != null)
                {
                    (searchTransactionsSubHeading as TextDisplayHint).IsHidden = true;
                }

                var searchTransactionsCvv = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentInstrumentSearchTransactionsCvv);
                if (searchTransactionsCvv != null)
                {
                    (searchTransactionsCvv as PropertyDisplayHint).IsHidden = true;
                }

                var searchSubmitButtonGroup = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.SearchSubmitButtonGroup);
                if (searchSubmitButtonGroup != null)
                {
                    (searchSubmitButtonGroup as GroupDisplayHint).IsHidden = true;
                }

                BuildDisableOptions(pidlResource, displayHintId, possibleOptions, LocalizationRepository.Instance.GetLocalizedString("Use an existing payment method", language), true);
            }

            foreach (PaymentInstrument pi in paymentInstruments)
            {
                SelectOptionDescription selectOption = new SelectOptionDescription();

                selectOption.DisplayText = BuildPiDefaultDisplayName(pi, partnerName);
                selectOption.DisplayImageUrl = GetPaymentInstrumentLogoUrl(pi);

                possibleValues.Add(pi.PaymentInstrumentId, selectOption.DisplayText);
                possibleOptions.Add(pi.PaymentInstrumentId, selectOption);
            }

            UpdateSelectDisplay(pidlResource, displayHintId, dataHintId, possibleValues, possibleOptions);
        }

        private static void BuildDisableOptions(PIDLResource pidlResource, string displayHintId, Dictionary<string, SelectOptionDescription> possibleOptions, string defaultMessage, bool selectFirstItem)
        {
            SelectOptionDescription selectOption = new SelectOptionDescription();
            selectOption.IsDisabled = true;
            possibleOptions.Add(defaultMessage, selectOption);

            PropertyDisplayHint displayHint = pidlResource.GetDisplayHintById(displayHintId) as PropertyDisplayHint;
            if (displayHint != null)
            {
                displayHint.IsSelectFirstItem = selectFirstItem;
                displayHint.IsDisabled = true;
            }
        }

        private static void PopulateAddressGroupPidl(PIDLResource pidlResource, string partnerName, string country, string language, CMResources<AddressInfo> addressGroups)
        {
            string dataHintId = Constants.DataDescriptionIds.AddressGroupId;
            string displayHintId = Constants.DisplayHintIds.AddressGroup;

            PropertyDisplayHint displayHint = pidlResource.GetDisplayHintById(displayHintId) as PropertyDisplayHint;

            Dictionary<string, SelectOptionDescription> possibleOptions = new Dictionary<string, SelectOptionDescription>();
            Dictionary<string, string> possibleValues = new Dictionary<string, string>();

            foreach (AddressInfo addressGroup in addressGroups.Items)
            {
                string displayText = addressGroup.AddressLine1 + " " + addressGroup.AddressLine2 + " " + addressGroup.City + "," + addressGroup.State + " " + addressGroup.Zip;
                possibleValues.Add(addressGroup.Id, displayText);
                possibleOptions.Add(addressGroup.Id, new SelectOptionDescription { DisplayText = displayText });
            }

            UpdateSelectDisplay(
                pidlResource,
                displayHintId,
                dataHintId,
                possibleValues,
                possibleOptions);
        }

        private static void AddSingleInstancePidls(List<PIDLResource> retList, string partnerName, string country, string language, string resourceId, bool selectResourceType, List<PaymentInstrument> paymentInstruments, string id, string backupId, PaymentInstrument primaryInstance = null, decimal? price = null, PaymentExperienceSetting setting = null)
        {
            string displayType = null;
            foreach (PaymentInstrument pi in paymentInstruments)
            {
                PIDLResource pidlResource = GetPaymentInstrumentPidlResource(
                    partnerName,
                    country,
                    Constants.PidlOperationTypes.SelectSingleInstance,
                    resourceId,
                    primaryInstance == null ? pi.PaymentInstrumentId : string.Format("{0}.{1}", primaryInstance.PaymentInstrumentId, pi.PaymentInstrumentId),
                    setting: setting);

                displayType = string.IsNullOrEmpty(displayType) ? pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentSelection).DisplayHintType : displayType;

                if (displayType.Equals("text"))
                {
                    PopulateStaticSingleInstancePidl(pidlResource, pi, primaryInstance, country, language, partnerName, selectResourceType, price);
                }
                else
                {
                    PopulateButtonListSingleInstancePidl(pidlResource, pi, primaryInstance, country, language, partnerName);
                }

                if (!string.IsNullOrEmpty(id) &&
                    ((primaryInstance == null && string.IsNullOrEmpty(backupId) && id.Equals(pi.PaymentInstrumentId)) || (primaryInstance != null && !string.IsNullOrEmpty(backupId) && backupId.Equals(pi.PaymentInstrumentId) && id.Equals(primaryInstance.PaymentInstrumentId))))
                {
                    // Return single instance pidl matching given ids first
                    retList.Insert(0, pidlResource);
                }
                else
                {
                    retList.Add(pidlResource);
                }
            }
        }

        private static ActionContext CreatePaymentInstrumentActionContext(
            PaymentInstrument pi,
            PIActionType? action,
            PidlDocInfo pidlDocInfo,
            ResourceInfo resourceInfo,
            PartnerHints partnerHints)
        {
            if (action == null)
            {
                return null;
            }

            return new ActionContext()
            {
                Id = pi?.PaymentInstrumentId,
                Action = PaymentInstrumentActions.ToString(action.Value),
                PartnerHints = partnerHints,
                ResourceActionContext = new ResourceActionContext()
                {
                    Action = PaymentInstrumentActions.ToString(action.Value),
                    PidlDocInfo = pidlDocInfo,
                    ResourceInfo = resourceInfo,
                    Resource = pi
                }
            };
        }

        private static ActionContext CreatePaymentInstrumentActionContext(PaymentInstrument pi, PaymentInstrument primaryInstance, PIActionType? action, PidlDocInfo pidlDocInfo = null, Dictionary<string, string> prefillData = null, PidlIdentity targetIdentity = null, ResourceInfo resourceInfo = null, PartnerHints partnerHints = null)
        {
            ActionContext actionContext = new ActionContext();

            if (pi != null && primaryInstance != null)
            {
                actionContext.Id = primaryInstance.PaymentInstrumentId;
                actionContext.Instance = primaryInstance;
                actionContext.BackupId = pi.PaymentInstrumentId;
                actionContext.BackupInstance = pi;
            }
            else if (pi != null && primaryInstance == null)
            {
                actionContext.Id = pi.PaymentInstrumentId;
                actionContext.Instance = pi;
            }

            if (action.HasValue)
            {
                string actionString = PaymentInstrumentActions.ToString(action.Value);
                actionContext.Action = actionString;
                actionContext.SetPrefillData(prefillData);
                actionContext.TargetIdentity = targetIdentity;
                actionContext.ResourceActionContext = new ResourceActionContext(PaymentInstrumentActions.ToString(action.Value), pidlDocInfo, targetIdentity, prefillData, resourceInfo);
                actionContext.PartnerHints = partnerHints;
            }

            return actionContext;
        }

        private static void PopulateStaticSingleInstancePidl(PIDLResource pidlResource, PaymentInstrument pi, PaymentInstrument primaryInstance, string country, string language, string partnerName, bool selectResourceType, decimal? price = null)
        {
            if (primaryInstance == null)
            {
                AddStaticPaymentSelectionDisplay(pidlResource, country, language, partnerName, pi, null, price);
            }
            else
            {
                AddStaticPaymentSelectionDisplay(pidlResource, country, language, partnerName, primaryInstance, pi, price);
            }

            AddActionToSelectPMButton(pidlResource, Constants.DisplayHintIds.ChangeInstanceButton, country, language, partnerName, primaryInstance == null ? pi.PaymentInstrumentId : primaryInstance.PaymentInstrumentId, primaryInstance == null ? null : pi.PaymentInstrumentId);

            if (selectResourceType)
            {
                AddActionToGetPMButton(pidlResource, null, Constants.ButtonDisplayHintIds.SuccessButtonHidden, country, language, partnerName);
            }
            else
            {
                ButtonDisplayHint submitButton = pidlResource.GetDisplayHintById(Constants.ButtonDisplayHintIds.SuccessButtonHidden) as ButtonDisplayHint;
                submitButton.Action = CreateSuccessPidlAction(CreatePaymentInstrumentActionContext(pi, primaryInstance, GetActionType(pi, null)), true);
            }
        }

        private static void PopulateButtonListSingleInstancePidl(PIDLResource pidlResource, PaymentInstrument pi, PaymentInstrument primaryInstance, string country, string language, string partnerName)
        {
            PidlIdentity selectPiIdentity = new PidlIdentity(Constants.DescriptionTypes.PaymentInstrumentDescription, Constants.PidlOperationTypes.SelectInstance, country, Constants.PidlResourceIdentities.PaymentInstrumentSelectPidl);

            if (primaryInstance == null)
            {
                Dictionary<string, string> selectPiPrefillData = CreatePrefillData(pi.PaymentInstrumentId, null);
                PopulateSinglePaymentOption(pidlResource, pi, selectPiIdentity, selectPiPrefillData, Constants.DisplayHintIds.PaymentSelection, Constants.DataDescriptionIds.PaymentInstrumentId, country, language, partnerName);
            }
            else
            {
                PidlIdentity selectBackupIdentity = new PidlIdentity(Constants.DescriptionTypes.PaymentInstrumentDescription, Constants.PidlOperationTypes.SelectInstance, country, Constants.PidlResourceIdentities.PaymentInstrumentBackupPidl);
                Dictionary<string, string> selectPiPrefillDataWithBackup = CreatePrefillData(primaryInstance.PaymentInstrumentId, pi.PaymentInstrumentId);

                PopulateSinglePaymentOption(pidlResource, primaryInstance, selectPiIdentity, selectPiPrefillDataWithBackup, Constants.DisplayHintIds.PaymentSelection, Constants.DataDescriptionIds.PaymentInstrumentId, country, language, partnerName);
                PopulateSinglePaymentOption(pidlResource, pi, selectBackupIdentity, selectPiPrefillDataWithBackup, Constants.DisplayHintIds.BackupPaymentSelection, Constants.DataDescriptionIds.BackupId, country, language, partnerName);
            }

            ButtonDisplayHint buyButton = pidlResource.GetDisplayHintById(Constants.ButtonDisplayHintIds.BuyButton) as ButtonDisplayHint;
            if (buyButton == null)
            {
                buyButton = pidlResource.GetDisplayHintById(Constants.ButtonDisplayHintIds.HiddenBuyButton) as ButtonDisplayHint;
            }

            if (buyButton != null)
            {
                buyButton.Action = CreateSuccessPidlAction(CreatePaymentInstrumentActionContext(pi, primaryInstance, null), true);
            }
        }

        private static PIDLResource GetBackupSelectPidl(string partnerName, string country, string language, PaymentInstrument storedValue, List<PaymentInstrument> paymentInstruments, List<PaymentInstrument> disabledPaymentInstruments, List<PaymentInstrument> backupOptions, List<PaymentInstrument> backupDisabledPIs, decimal price, HashSet<PaymentMethod> filteredPaymentMethods, PaymentExperienceSetting setting = null)
        {
            PIDLResource backupPidl = GetPaymentInstrumentPidlResource(partnerName, country, Constants.PidlOperationTypes.SelectInstance, Constants.PidlResourceIdentities.PaymentInstrumentBackupPidl, setting: setting);
            PopulateSelectInstancePidl(backupPidl, partnerName, country, language, backupOptions, backupDisabledPIs, filteredPaymentMethods, storedValue);
            if (backupPidl.DisplayPages.Count() > 1)
            {
                PIDLResource firstPagePidl = GetPaymentInstrumentPidlResource(partnerName, country, Constants.PidlOperationTypes.SelectInstance, Constants.PidlResourceIdentities.PaymentInstrumentBackupPidl, setting: setting);
                PopulateSelectInstancePidl(firstPagePidl, partnerName, country, language, paymentInstruments, disabledPaymentInstruments, filteredPaymentMethods);

                backupPidl.DataDescription[Constants.DataDescriptionIds.PaymentInstrumentId] = firstPagePidl.DataDescription[Constants.DataDescriptionIds.PaymentInstrumentId];
                backupPidl.DisplayPages[0] = firstPagePidl.DisplayPages[0];

                AddSelectValidation(backupPidl, storedValue.PaymentInstrumentId, false);

                if (backupOptions.Count == 0 && backupDisabledPIs.Count == 0)
                {
                    // User has no PIs except CSV, "Use this payment method button" on the first page with CSV selected should redirect to single CSV pidl w/ selectResourceType action
                    AddActionToGetPMButton(backupPidl, storedValue, Constants.ButtonDisplayHintIds.UseButtonNext, country, language, partnerName);
                    backupPidl.DisplayPages.Remove(backupPidl.DisplayPages.Last());
                }
                else
                {
                    AddStaticPaymentSelectionDisplay(backupPidl, country, language, partnerName, storedValue);
                    PopulateRemainingBalanceText(backupPidl, country, language, price, storedValue);
                }
            }

            return backupPidl;
        }

        private static void PopulateRemainingBalanceText(PIDLResource pidlResource, string country, string language, decimal price, PaymentInstrument storedValue)
        {
            TextDisplayHint balanceText = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.RemainingBalance) as TextDisplayHint;
            if (balanceText != null)
            {
                balanceText.DisplayContent = string.Format(
                    "{0} {1}",
                    balanceText.DisplayContent,
                    CurrencyHelper.FormatCurrency(country, language, price - storedValue.PaymentInstrumentDetails.Balance, storedValue.PaymentInstrumentDetails.Currency));
            }
        }

        private static void AddStaticPaymentSelectionDisplay(PIDLResource pidlResource, string country, string language, string partnerName, PaymentInstrument selection, PaymentInstrument backupSelection = null, decimal? cost = null)
        {
            TextDisplayHint textHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentSelection) as TextDisplayHint;
            textHint.DisplayContent = BuildPiDefaultDisplayName(selection, partnerName);

            ImageDisplayHint imageHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentSelectionImage) as ImageDisplayHint;
            imageHint.SourceUrl = GetPaymentInstrumentLogoUrl(selection);

            if (IsStoredValue(selection.PaymentMethod))
            {
                TextDisplayHint balanceHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.BalanceText) as TextDisplayHint;
                balanceHint.DisplayContent = CurrencyHelper.FormatCurrency(country, language, selection.PaymentInstrumentDetails.Balance, selection.PaymentInstrumentDetails.Currency);

                if (cost.HasValue)
                {
                    PopulateRemainingBalanceText(pidlResource, country, language, cost.Value, selection);
                }
            }
            else
            {
                PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<TextDisplayHint>(Constants.DisplayHintIds.BalanceText, pidlResource.DisplayPages);
            }

            if (backupSelection != null)
            {
                TextDisplayHint backupTextHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.BackupPaymentSelection) as TextDisplayHint;
                backupTextHint.DisplayContent = BuildPiDefaultDisplayName(backupSelection, partnerName);

                TextDisplayHint backupBalanceHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.BackupBalanceText) as TextDisplayHint;
                backupBalanceHint.DisplayContent = CurrencyHelper.FormatCurrency(country, language, cost.Value - selection.PaymentInstrumentDetails.Balance, selection.PaymentInstrumentDetails.Currency);

                ImageDisplayHint backupImageHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.BackupSelectionImage) as ImageDisplayHint;
                backupImageHint.SourceUrl = GetPaymentInstrumentLogoUrl(backupSelection);
            }
        }

        private static void AddSelectValidation(PIDLResource pidlResource, string id, bool isPrimary)
        {
            if (pidlResource.DataDescription.ContainsKey(Constants.DataDescriptionIds.PaymentInstrumentId))
            {
                PropertyDescription idHint = pidlResource.DataDescription[Constants.DataDescriptionIds.PaymentInstrumentId] as PropertyDescription;
                idHint.IsKey = true;

                // Escape special regex characters that may be in the id with \
                string escapeId = Regex.Replace(id, @"[*+?|{[()^$.#]", m => string.Format(@"\{0}", m.Value));
                string regex = isPrimary ? string.Format("^(?!{0}$).*", escapeId) : string.Format("^{0}$", escapeId);

                idHint.AddAdditionalValidation(new PropertyValidation(regex));
            }
        }

        private static void UpdateStoredValueActionToSelectBackupPI(PIDLResource pidlResource, string storedValueId)
        {
            PropertyDisplayHint displayHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentInstrumentSelect) as PropertyDisplayHint;
            if (displayHint != null && displayHint.PossibleOptions != null)
            {
                SelectOptionDescription storedValueOption = null;
                displayHint.PossibleOptions.TryGetValue(storedValueId, out storedValueOption);
                if (storedValueOption != null)
                {
                    ActionContext storedValueOptionAction = storedValueOption.PidlAction.Context as ActionContext;
                    if (storedValueOptionAction != null)
                    {
                        storedValueOptionAction.Action = PaymentInstrumentActions.ToString(PIActionType.SelectResource);
                        if (storedValueOptionAction.ResourceActionContext != null)
                        {
                            storedValueOptionAction.ResourceActionContext.Action = PaymentInstrumentActions.ToString(PIActionType.SelectResource);
                            if (storedValueOptionAction.ResourceActionContext.PidlIdentity != null)
                            {
                                storedValueOptionAction.ResourceActionContext.PidlIdentity.Operation = Constants.PidlOperationTypes.SelectInstance;
                                storedValueOptionAction.ResourceActionContext.PidlIdentity.ResourceId = Constants.PidlResourceIdentities.PaymentInstrumentBackupPidl;
                            }
                        }

                        if (storedValueOptionAction.TargetIdentity != null)
                        {
                            storedValueOptionAction.TargetIdentity.Operation = Constants.PidlOperationTypes.SelectInstance;
                            storedValueOptionAction.TargetIdentity.ResourceId = Constants.PidlResourceIdentities.PaymentInstrumentBackupPidl;
                        }
                    }
                }
            }
        }

        private static void DisableStoredValueOption(PIDLResource pidlResource, string storedValueId)
        {
            PropertyDisplayHint displayHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentInstrumentSelect) as PropertyDisplayHint;
            if (displayHint != null && displayHint.PossibleOptions != null)
            {
                SelectOptionDescription storedValueOption = null;
                displayHint.PossibleOptions.TryGetValue(storedValueId, out storedValueOption);
                if (storedValueOption != null)
                {
                    storedValueOption.IsDisabled = true;
                }
            }
        }

        private static Dictionary<string, string> CreatePrefillData(string paymentInstrumentId, string backupPaymentInstrumentId)
        {
            Dictionary<string, string> prefillData = new Dictionary<string, string> { { Constants.DescriptionIdentityFields.PaymentInstrumentId, paymentInstrumentId } };

            if (!string.IsNullOrEmpty(backupPaymentInstrumentId))
            {
                prefillData[Constants.DescriptionIdentityFields.BackupPaymentInstrumentId] = backupPaymentInstrumentId;
            }

            return prefillData;
        }

        private static void PopulateSinglePaymentOption(PIDLResource pidlResource, PaymentInstrument pi, PidlIdentity selectPiIdentity, Dictionary<string, string> selectPiPrefillData, string displayHintId, string dataHintId, string country, string language, string partnerName)
        {
            Dictionary<string, SelectOptionDescription> possibleOptions = new Dictionary<string, SelectOptionDescription>();
            Dictionary<string, string> possibleValues = new Dictionary<string, string>();

            SelectOptionDescription selectOption = new SelectOptionDescription();
            AddDisplayDetailsToPaymentOption(selectOption, country, language, partnerName, PIActionType.SelectResource, pi, null, selectPiIdentity, selectPiPrefillData);

            possibleValues.Add(pi.PaymentInstrumentId, selectOption.DisplayText);
            possibleOptions.Add(pi.PaymentInstrumentId, selectOption);

            UpdateSelectDisplay(pidlResource, displayHintId, dataHintId, possibleValues, possibleOptions);
        }

        private static void AddDisplayCustomDetailsToButtonListOption(
            SelectOptionDescription selectOption,
            string country,
            string language,
            string partnerName,
            PIActionType? optionAction,
            PaymentInstrument pi,
            PidlDocInfo pidlDocInfo = null,
            ResourceInfo resourceInfo = null)
        {
            selectOption.DisplayText = string.Empty;

            // Text Group 1: Display logo Visa **** 9999   Name   Expiry 01/01 button
            GroupDisplayHint group = new GroupDisplayHint { HintId = Constants.DisplayHintIdPrefixes.PaymentOptionTextGroup + pi.PaymentInstrumentId };
            group.AddDisplayTag(Constants.DisplayTag.PiContainer, Constants.DisplayTag.PiContainer);
            PaymentInstrumentDetails details = pi.PaymentInstrumentDetails;
            PaymentMethod method = pi.PaymentMethod;

            ImageDisplayHint logo = new ImageDisplayHint
            {
                SourceUrl = GetPaymentInstrumentLogoUrl(pi),
                HintId = Constants.DisplayHintIdPrefixes.PaymentOptionLogo + pi.PaymentInstrumentId
            };

            logo.AddDisplayTag("image-icon", "image-icon");
            group.Members.Add(logo);

            // Build Part 1 CC VISA **** 8136
            var partNum = 1;
            var displayNameHint = new TextDisplayHint
            {
                DisplayContent = string.Format(
                    "{0} **** {1}",
                    pi.PaymentMethod.Display.Name,
                    details.LastFourDigits),
                HintId = string.Format(Constants.DisplayHintIdPrefixes.PaymentOptionTextTemplate, partNum++, pi.PaymentInstrumentId)
            };

            displayNameHint.AddDisplayTag("label-info", "label-info");
            group.Members.Add(displayNameHint);

            var displayUserHint = new TextDisplayHint
            {
                DisplayContent = details.CardHolderName,
                HintId = string.Format(Constants.DisplayHintIdPrefixes.PaymentOptionTextTemplate, partNum++, pi.PaymentInstrumentId)
            };

            displayUserHint.AddDisplayTag("label-text", "label-text");
            group.Members.Add(displayUserHint);

            // Build Part 2 for CC
            // CC Expiry 6/2033
            if (IsCreditCardNotCup(method))
            {
                var expirationDisplayHint = new TextDisplayHint
                {
                    DisplayContent = string.Format(
                            PidlModelHelper.GetLocalizedString("Expiry", language) + " {0}/{1}",
                            int.Parse(details.ExpiryMonth).ToString("00"),
                            details.ExpiryYear),
                    HintId = string.Format(Constants.DisplayHintIdPrefixes.PaymentOptionTextTemplate, partNum, pi.PaymentInstrumentId)
                };
                expirationDisplayHint.AddDisplayTag("expiry-text", "expiry-text");

                if (ExpiryActionNeeded(pi))
                {
                    expirationDisplayHint.AddDisplayTag("warning-icon", "warning-icon");
                }

                group.Members.Add(expirationDisplayHint);
            }

            var selectButton = new TextDisplayHint
            {
                DisplayContent = PidlModelHelper.GetLocalizedString("Select"),
                HintId = string.Format(Constants.DisplayHintIdPrefixes.PaymentOptionTextTemplate, partNum++, pi.PaymentInstrumentId)
            };

            selectButton.AddDisplayTag("label-button", "label-button");
            group.Members.Add(selectButton);
            selectOption.DisplayContent = group;
        }

        private static void AddDisplayDetailsToRadioButtonOption(
            SelectOptionDescription selectOption,
            string country,
            string language,
            string partnerName,
            PIActionType? optionAction,
            PaymentInstrument pi,
            PidlDocInfo pidlDocInfo = null,
            ResourceInfo resourceInfo = null,
            List<string> flightNames = null,
            PaymentExperienceSetting setting = null)
        {
            selectOption.DisplayText = string.Empty;
            if (optionAction != null && optionAction.Value == PIActionType.EditPaymentInstrument)
            {
                selectOption.PidlAction = CreateUpdatePaymentInstrumentPidlAction(
                            pi,
                            optionAction,
                            pidlDocInfo,
                            resourceInfo,
                            Constants.PartnerHintsValues.TriggeredBySubmitGroup);
            }
            else
            {
                ActionContext optionContext = CreatePaymentInstrumentActionContext(pi, null, optionAction, pidlDocInfo, null, null, resourceInfo, null);
                selectOption.PidlAction = CreateSuccessPidlAction(optionContext, false);
            }

            // |image ||last four     |       Action|||
            //        ||Exp month year|
            selectOption.DisplayContent = new GroupDisplayHint
            {
                HintId = Constants.DisplayHintIdPrefixes.PaymentOptionContainer + pi.PaymentInstrumentId,
                LayoutOrientation = "inline"
            };

            selectOption.DisplayContent.AddDisplayTag(Constants.DisplayTag.PiContainer, Constants.DisplayTag.PiContainer);

            // Create Image Icon
            ImageDisplayHint logo = new ImageDisplayHint
            {
                SourceUrl = GetPaymentInstrumentLogoUrl(pi),
                HintId = Constants.DisplayHintIdPrefixes.PaymentOptionLogo + pi.PaymentInstrumentId
            };

            logo.AddDisplayTag("image-icon", "image-icon");
            selectOption.DisplayContent.Members.Add(logo);

            // Create Text Group (Display Text + Action Button)
            GroupDisplayHint textGroupWithAction = new GroupDisplayHint { HintId = Constants.DisplayHintIdPrefixes.PaymentOptionDisplayGroup + pi.PaymentInstrumentId };
            textGroupWithAction.LayoutOrientation = "inline";
            textGroupWithAction.AddDisplayTag("space-between", "space-between");

            // Text Group 1: Display Text Visa ** 4444
            //                            Exp 07/2023
            GroupDisplayHint textGroup = GetPaymentInstrumentDisplayTextGroup(pi, partnerName, language, country, flightNames, setting);
            textGroupWithAction.Members.Add(textGroup);

            // Text Group 2: Action Button (eg. update)
            if (optionAction.HasValue && PaymentInstrumentActions.GetPaymentInstrumentActions().ContainsKey(optionAction.Value))
            {
                if (optionAction.Equals(PIActionType.EditPaymentInstrument))
                {
                    var actionButton = new ButtonDisplayHint
                    {
                        HintId = Constants.DisplayHintIdPrefixes.PaymentOptionUpdate + pi.PaymentInstrumentId,
                        DisplayContent = TemplateHelper.IsListPiTemplate(setting) ? PidlModelHelper.GetLocalizedString(Constants.PaymentMethodOptionStrings.Edit) : PidlModelHelper.GetLocalizedString(Constants.PaymentMethodOptionStrings.Update),
                        Action = CreateUpdatePaymentInstrumentPidlAction(
                            pi,
                            optionAction,
                            pidlDocInfo,
                            resourceInfo,
                            Constants.PartnerHintsValues.TriggeredByUpdateButton),
                    };

                    // Narator will announce the details of card on update button
                    PaymentInstrumentDetails details = pi.PaymentInstrumentDetails;

                    string updateCardString = string.Format(
                    Constants.PaymentMethodFormatStrings.UpdateYourCard, pi.PaymentMethod.Display.Name, details.LastFourDigits);

                    string updateYourCard = string.Format(
                    PidlModelHelper.GetLocalizedString(updateCardString, language));

                    actionButton.AddDisplayTag("accessibilityName", updateYourCard);
                    actionButton.AddDisplayTag("action-trigger", "action-trigger");
                    actionButton.AddDisplayTag("auto-height", "auto-height");
                    actionButton.AddDisplayTag("hightlight", "hightlight");
                    textGroupWithAction.Members.Add(actionButton);
                }
            }

            selectOption.DisplayContent.Members.Add(textGroupWithAction);
        }

        private static void AddDisplayDetailsToWindowsSettingsRadioButtonOption(
            SelectOptionDescription selectOption,
            string country,
            string language,
            string partnerName,
            PIActionType? optionAction,
            PaymentInstrument pi,
            PidlDocInfo pidlDocInfo = null,
            ResourceInfo resourceInfo = null,
            List<string> flightNames = null)
        {
            selectOption.DisplayText = string.Empty;
            if (optionAction != null && optionAction.Value == PIActionType.EditPaymentInstrument)
            {
                selectOption.PidlAction = CreateUpdatePaymentInstrumentPidlAction(
                            pi,
                            optionAction,
                            pidlDocInfo,
                            resourceInfo,
                            Constants.PartnerHintsValues.TriggeredBySubmitGroup);
            }
            else
            {
                ActionContext optionContext = CreatePaymentInstrumentActionContext(pi, null, optionAction, pidlDocInfo, null, null, resourceInfo, null);
                selectOption.PidlAction = CreateSuccessPidlAction(optionContext, false);
            }

            // |image ||cardholder's name ••last four| expiry month/year| Expired (if expired)||       Action|
            selectOption.DisplayContent = new GroupDisplayHint
            {
                HintId = Constants.DisplayHintIdPrefixes.PaymentOptionContainer + pi.PaymentInstrumentId,
                LayoutOrientation = "inline"
            };

            selectOption.DisplayContent.AddDisplayTag(Constants.DisplayTag.PiContainer, Constants.DisplayTag.PiContainer);

            // Create Image Icon
            string alternateSvgForPartner = CheckForReactNativeAlternatePaymentInstrumentLogoUrl(pi, partnerName, flightNames);

            string accessibilityName = pi.PaymentMethod.Display.Name + " ";

            ImageDisplayHint logo = new ImageDisplayHint
            {
                SourceUrl = alternateSvgForPartner ?? GetPaymentInstrumentLogoUrl(pi),
                HintId = Constants.DisplayHintIdPrefixes.PaymentOptionLogo + pi.PaymentInstrumentId
            };

            logo.AddDisplayTag("image-icon", "image-icon");
            selectOption.DisplayContent.Members.Add(logo);

            // Create Text Group (Display Text + Action Button)
            GroupDisplayHint textGroupWithAction = new GroupDisplayHint { HintId = Constants.DisplayHintIdPrefixes.PaymentOptionDisplayGroup + pi.PaymentInstrumentId };
            textGroupWithAction.LayoutOrientation = "inline";
            textGroupWithAction.AddDisplayTag("space-between", "space-between");

            // Text Group 1: John Doe ••4321 01/33
            // if expired: John Doe ••4321 01/21 Expired
            GroupDisplayHint textGroup = GetPaymentInstrumentDisplayTextGroup(pi, partnerName, language, country, flightNames, null, true);
            accessibilityName += textGroup.DisplayName;
            textGroupWithAction.Members.Add(textGroup);

            // Text Group 2: Action Button (eg. Edit)
            if (optionAction.HasValue && PaymentInstrumentActions.GetPaymentInstrumentActions().ContainsKey(optionAction.Value))
            {
                if (optionAction.Equals(PIActionType.EditPaymentInstrument))
                {
                    var actionButton = new ButtonDisplayHint
                    {
                        HintId = Constants.DisplayHintIdPrefixes.PaymentOptionEdit + pi.PaymentInstrumentId,
                        DisplayContent = PidlModelHelper.GetLocalizedString(Constants.PaymentMethodOptionStrings.Edit),
                        Action = CreateUpdatePaymentInstrumentPidlAction(
                            pi,
                            optionAction,
                            pidlDocInfo,
                            resourceInfo,
                            Constants.PartnerHintsValues.TriggeredByUpdateButton),
                    };

                    // Narator will announce the details of card on Edit button
                    PaymentInstrumentDetails details = pi.PaymentInstrumentDetails;

                    string updateCardString = string.Format(Constants.PaymentMethodFormatStrings.EditYourCard, pi.PaymentMethod.Display.Name, details.CardHolderName, details.LastFourDigits);
                    string updateYourCard = string.Format(PidlModelHelper.GetLocalizedString(updateCardString, language));

                    actionButton.AddDisplayTag("accessibilityName", updateYourCard);
                    actionButton.AddDisplayTag("action-trigger", "action-trigger");
                    textGroupWithAction.Members.Add(actionButton);
                }
            }

            selectOption.DisplayContent.AddDisplayTag(Constants.DisplayTag.AccessibilityName, accessibilityName);
            selectOption.DisplayContent.Members.Add(textGroupWithAction);
        }

        private static DisplayHintAction CreateUpdatePaymentInstrumentPidlAction(
            PaymentInstrument pi,
            PIActionType? optionAction,
            PidlDocInfo pidlDocInfo,
            ResourceInfo resourceInfo,
            string triggeredBy)
        {
            var partnerHints = new PartnerHints()
            {
                TriggeredBy = triggeredBy
            };

            ActionContext actionContext = CreatePaymentInstrumentActionContext(pi, optionAction, pidlDocInfo, resourceInfo, partnerHints);
            return CreateSuccessPidlAction(actionContext, false);
        }

        private static string BuildPiDefaultDisplayNameAddExpiryDetails(
            PaymentInstrument pi,
            string retVal,
            List<string> flightNames,
            string parter,
            bool showExpiryYearAndMonth = false,
            string country = null)
        {
            if (string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase))
            {
                // When country=IN, token expiry year and token expiry month info should be displayed on card display
                retVal = BuildPiDefaultDisplayNameAddExpiryDetailsINWithFlight(pi, retVal, flightNames, parter, showExpiryYearAndMonth);
                return retVal;
            }
            else
            {
                retVal = AppendExpiryInfo(showExpiryYearAndMonth, pi, retVal);
            }

            return retVal;
        }

        private static string BuildPiDefaultDisplayNameAddExpiryDetailsINWithFlight(
            PaymentInstrument pi,
            string retVal,
            List<string> flightNames,
            string partner,
            bool showExpiryYearAndMonth = false)
        {
            PaymentInstrumentDetails details = pi.PaymentInstrumentDetails;

            if (IsXboxNativePartner(partner) && (flightNames?.Contains(Constants.PartnerFlightValues.EnableIndiaTokenExpiryDetails) == true))
            {
                if (details.ProviderToken?.BillDeskToken?.TokenStatus != null)
                {
                    if (string.Equals(details.ProviderToken.BillDeskToken.TokenStatus.ToString(), Constants.TokenExpiryStatus.Expired, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(details.ProviderToken.BillDeskToken.TokenStatus.ToString(), Constants.TokenExpiryStatus.Deleted, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(details.ProviderToken.BillDeskToken.TokenStatus.ToString(), Constants.TokenExpiryStatus.Suspended, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(details.ProviderToken.BillDeskToken.TokenExpiryMonth) && !string.IsNullOrWhiteSpace(details.ProviderToken.BillDeskToken.TokenExpiryYear))
                        {
                            // TokenizedStatus == Expired/Suspended/Deleted
                            retVal = string.Format("{0} {1}/{2} {3}", retVal, showExpiryYearAndMonth ? GetExpiryMonthForListPiTemplate(details.ProviderToken.BillDeskToken.TokenExpiryMonth) : details.ProviderToken.BillDeskToken.TokenExpiryMonth, details.ProviderToken.BillDeskToken.TokenExpiryYear.Substring(2), Constants.TokenExpiryStatus.Expired);
                        }
                        else
                        {
                            // TokenizedStatus == Expired/Suspended/Deleted and (atleast one of TokenizedExpiryMonth or TokenizedExpiryYear is null)
                            retVal = string.Format("{0} {1}", retVal, Constants.TokenExpiryStatus.Expired);
                        }

                        return retVal;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(details.ProviderToken.BillDeskToken.TokenExpiryMonth) && !string.IsNullOrWhiteSpace(details.ProviderToken.BillDeskToken.TokenExpiryYear))
                        {
                            // TokenizedStatus == Active/Pending
                            retVal = string.Format("{0} {1}/{2}", retVal, showExpiryYearAndMonth ? GetExpiryMonthForListPiTemplate(details.ProviderToken.BillDeskToken.TokenExpiryMonth) : details.ProviderToken.BillDeskToken.TokenExpiryMonth, details.ProviderToken.BillDeskToken.TokenExpiryYear.Substring(2));
                            return retVal;
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(details.ProviderToken?.BillDeskToken?.TokenExpiryMonth) && !string.IsNullOrWhiteSpace(details.ProviderToken?.BillDeskToken?.TokenExpiryYear))
                    {
                        // (TokenizedStatus == null) and (both TokenizedExpiryMonth and TokenizedExpiryYear are not null)
                        retVal = string.Format("{0} {1}/{2}", retVal, showExpiryYearAndMonth ? GetExpiryMonthForListPiTemplate(details.ProviderToken.BillDeskToken.TokenExpiryMonth) : details.ProviderToken.BillDeskToken.TokenExpiryMonth, details.ProviderToken.BillDeskToken.TokenExpiryYear.Substring(2));
                        return retVal;
                    }
                }
            }

            if (showExpiryYearAndMonth)
            {
                return retVal;
            }

            retVal = AppendExpiryInfo(showExpiryYearAndMonth, pi, retVal);

            return retVal;
        }

        private static void GetPaymentInstrumentDisplayTextGroupAddExpiryDateInformation(
            string language,
            int partNum,
            GroupDisplayHint group,
            PaymentInstrument pi,
            string partnerName,
            List<string> flightNames,
            PaymentExperienceSetting setting = null,
            string country = null,
            bool fillDisplayName = false)
        {
            PaymentInstrumentDetails details = pi.PaymentInstrumentDetails;

            // When country=IN, token expiry year and token expiry month info should be displayed on card display
            if (string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase))
            {
                var returnFromINFlow = GetPaymentInstrumentDisplayTextGroupAddExpiryDateInformationINFlightedFlow(language, details, partNum, group, pi, partnerName, flightNames);

                if (returnFromINFlow || TemplateHelper.IsListPiTemplate(setting))
                {
                    return;
                }
            }

            string displayContent;

            if (IsWindowsSettingsPartner(partnerName) || TemplateHelper.IsListPiRadioButtonTemplate(setting))
            {
                displayContent = string.Format(
                   " {0}/{1}",
                   int.Parse(details.ExpiryMonth).ToString("00"),
                   int.Parse(details.ExpiryYear).ToString("00").Substring(2));
            }
            else
            {
                displayContent = string.Format(
                   PidlModelHelper.GetLocalizedString("Exp", language) + " {0}/{1}",
                   int.Parse(details.ExpiryMonth).ToString("00"),
                   details.ExpiryYear);
            }

            var expirationDisplayHint = new TextDisplayHint
            {
                DisplayContent = displayContent,
                HintId = string.Format(Constants.DisplayHintIdPrefixes.PaymentOptionTextTemplate, partNum, pi.PaymentInstrumentId)
            };

            if (ExpiryActionNeeded(pi) && !(IsWindowsSettingsPartner(partnerName) || TemplateHelper.IsListPiRadioButtonTemplate(setting)))
            {
                expirationDisplayHint.AddDisplayTag("warning-icon", "warning-icon");
            }

            if (fillDisplayName)
            {
                group.DisplayName += displayContent;
            }

            group.Members.Add(expirationDisplayHint);

            if (ExpiryActionNeeded(pi) && (IsWindowsSettingsPartner(partnerName) || TemplateHelper.IsListPiRadioButtonTemplate(setting)))
            {
                var expiredDisplayHint = new TextDisplayHint
                {
                    DisplayContent = PidlModelHelper.GetLocalizedString("Expired", language),
                    HintId = string.Format(Constants.DisplayHintIdPrefixes.PaymentOptionTextTemplate, ++partNum, pi.PaymentInstrumentId)
                };

                if (!TemplateHelper.IsListPiRadioButtonTemplate(setting))
                {
                    expiredDisplayHint.AddDisplayTag("warning-icon", "warning-icon");
                }

                if (fillDisplayName)
                {
                    group.DisplayName += " " + expiredDisplayHint.DisplayContent;
                }

                group.Members.Add(expiredDisplayHint);
            }
        }

        private static bool GetPaymentInstrumentDisplayTextGroupAddExpiryDateInformationINFlightedFlow(
            string language,
            PaymentInstrumentDetails details,
            int partNum,
            GroupDisplayHint group,
            PaymentInstrument pi,
            string partnerName,
            List<string> flightNames)
        {
            var returnFromINFlow = false;

            if (IsAmcWebPartner(partnerName) && (flightNames?.Contains(Constants.PartnerFlightValues.EnableIndiaTokenExpiryDetails) == true))
            {
                if (details.ProviderToken?.BillDeskToken?.TokenStatus != null)
                {
                    if (string.Equals(details.ProviderToken.BillDeskToken.TokenStatus.ToString(), Constants.TokenExpiryStatus.Expired, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(details.ProviderToken.BillDeskToken.TokenStatus.ToString(), Constants.TokenExpiryStatus.Deleted, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(details.ProviderToken.BillDeskToken.TokenStatus.ToString(), Constants.TokenExpiryStatus.Suspended, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(details.ProviderToken?.BillDeskToken?.TokenExpiryMonth) && !string.IsNullOrWhiteSpace(details.ProviderToken?.BillDeskToken?.TokenExpiryYear))
                        {
                            var expirationDisplayHint = AddTokenExpiryDetailsOnGroupDisplayHint(language, partNum, pi, details.ProviderToken?.BillDeskToken?.TokenExpiryMonth, details.ProviderToken?.BillDeskToken?.TokenExpiryYear, Constants.TokenExpiryStatus.Expired);

                            expirationDisplayHint.AddDisplayTag("warning-icon", "warning-icon");

                            group.Members.Add(expirationDisplayHint);
                        }
                        else
                        {
                            var expirationDisplayHint = AddTokenExpiryDetailsOnGroupDisplayHint(language, partNum, pi, null, null, Constants.TokenExpiryStatus.Expired);

                            expirationDisplayHint.AddDisplayTag("warning-icon", "warning-icon");

                            group.Members.Add(expirationDisplayHint);
                        }

                        returnFromINFlow = true;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(details.ProviderToken?.BillDeskToken?.TokenExpiryMonth) && !string.IsNullOrWhiteSpace(details.ProviderToken?.BillDeskToken?.TokenExpiryYear))
                        {
                            var expirationDisplayHint = AddTokenExpiryDetailsOnGroupDisplayHint(language, partNum, pi, details.ProviderToken?.BillDeskToken?.TokenExpiryMonth, details.ProviderToken?.BillDeskToken?.TokenExpiryYear, null);

                            group.Members.Add(expirationDisplayHint);
                            returnFromINFlow = true;
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(details.ProviderToken?.BillDeskToken?.TokenExpiryMonth) && !string.IsNullOrWhiteSpace(details.ProviderToken?.BillDeskToken?.TokenExpiryYear))
                    {
                        var expirationDisplayHint = AddTokenExpiryDetailsOnGroupDisplayHint(language, partNum, pi, details.ProviderToken?.BillDeskToken?.TokenExpiryMonth, details.ProviderToken?.BillDeskToken?.TokenExpiryYear, null);

                        group.Members.Add(expirationDisplayHint);
                        returnFromINFlow = true;
                    }
                }
            }

            return returnFromINFlow;
        }

        private static GroupDisplayHint GetPaymentInstrumentDisplayTextGroup(PaymentInstrument pi, string partner, string language, string country, List<string> flightNames = null, PaymentExperienceSetting setting = null, bool fillDisplayName = false)
        {
            GroupDisplayHint group = new GroupDisplayHint { HintId = Constants.DisplayHintIdPrefixes.PaymentOptionTextGroup + pi.PaymentInstrumentId };
            PaymentInstrumentDetails details = pi.PaymentInstrumentDetails;
            PaymentMethod method = pi.PaymentMethod;

            if (IsWindowsSettingsPartner(partner) || TemplateHelper.IsListPiRadioButtonTemplate(setting))
            {
                group.LayoutOrientation = "inline";
            }

            // Build Part 1 CC VISA ** 8136 (or for Windows Settings: John Doe ••4321)
            var partNum = 1;
            var displayNameHint = new TextDisplayHint
            {
                DisplayContent = BuildPiDefaultDisplayName(pi, partner, setting: setting),
                HintId = string.Format(Constants.DisplayHintIdPrefixes.PaymentOptionTextTemplate, partNum++, pi.PaymentInstrumentId)
            };

            displayNameHint.AddDisplayTag("label-text", "label-text");
            if (fillDisplayName)
            {
                group.ShowDisplayName = "false";
                group.DisplayName = displayNameHint.DisplayContent + " ";
            }

            group.Members.Add(displayNameHint);

            // Build Part 2 for CC and Store Value
            // CC Exp 6/2033
            // Store Value Bal
            // Or for CC for Windows Settings: 06/33
            // if expired: 03/20 Expired
            if (IsCreditCardNotCup(method))
            {
                if (flightNames == null || !flightNames.Any())
                {
                    GetPaymentInstrumentDisplayTextGroupAddExpiryDateInformation(language, partNum, group, pi, partner, flightNames, setting: setting, country: country, fillDisplayName);
                }
                else
                {
                    // When country=india, flight=IndiaExpiryGroupDelete is on and partner=amcweb, expiry year and expiry month info should be removed from card display
                    if (!(string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase) && flightNames.Contains(Constants.PartnerFlightValues.IndiaExpiryGroupDelete) && (IsAmcWebPartner(partner) || IsListPiRadioButtonTemplate(setting))))
                    {
                        GetPaymentInstrumentDisplayTextGroupAddExpiryDateInformation(language, partNum, group, pi, partner, flightNames, setting: setting, country: country, fillDisplayName);
                    }
                }
            }
            else if (IsStoredValue(method))
            {
                string storedValueDisplayContent = CurrencyHelper.FormatCurrency(country, language, details.Balance, details.Currency);
                if (fillDisplayName)
                {
                    group.DisplayName += storedValueDisplayContent;
                }

                group.Members.Add(
                    new TextDisplayHint()
                    {
                        DisplayContent = storedValueDisplayContent,
                        HintId = string.Format(Constants.DisplayHintIdPrefixes.PaymentOptionTextTemplate, partNum, pi.PaymentInstrumentId)
                    });
            }

            return group;
        }

        private static void AddDisplayDetailsToPaymentOption(
            SelectOptionDescription selectOption,
            string country,
            string language,
            string partnerName,
            PIActionType? optionAction,
            PaymentInstrument selectionInstance,
            PaymentInstrument primaryInstance = null,
            PidlIdentity targetPidlIdentity = null,
            Dictionary<string, string> contextPrefillData = null,
            PidlDocInfo pidlDocInfo = null,
            ResourceInfo resourceInfo = null,
            string selectType = null,
            PartnerHints partnerHints = null,
            List<string> exposedFlightFeatures = null,
            PaymentExperienceSetting setting = null)
        {
            if (IsAmcWebPartner(partnerName) || IsListPiRadioButtonTemplate(setting))
            {
                AddDisplayDetailsToRadioButtonOption(
                    selectOption,
                    country,
                    language,
                    partnerName,
                    optionAction,
                    selectionInstance,
                    pidlDocInfo,
                    resourceInfo,
                    flightNames: exposedFlightFeatures,
                    setting: setting);
                return;
            }
            else if (IsWindowsSettingsPartner(partnerName))
            {
                AddDisplayDetailsToWindowsSettingsRadioButtonOption(
                    selectOption,
                    country,
                    language,
                    partnerName,
                    optionAction,
                    selectionInstance,
                    pidlDocInfo,
                    resourceInfo,
                    flightNames: exposedFlightFeatures);
                return;
            }

            ActionContext optionContext = CreatePaymentInstrumentActionContext(selectionInstance, primaryInstance, optionAction, pidlDocInfo, contextPrefillData, targetPidlIdentity, resourceInfo, partnerHints);
            selectOption.PidlAction = CreateSuccessPidlAction(optionContext, false);

            if (IsBingTravelPartner(partnerName) ||
                (IsListPiButtonListTemplate(setting) && PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(FeatureConfiguration.FeatureNames.AddDisplayCustomDetailsToButtonListOption, country, setting)))
            {
                AddDisplayCustomDetailsToButtonListOption(
                    selectOption,
                    country,
                    language,
                    partnerName,
                    optionAction,
                    selectionInstance,
                    pidlDocInfo,
                    resourceInfo);
                return;
            }

            if (!string.IsNullOrEmpty(selectType) && string.Equals(selectType, Constants.PaymentMethodSelectType.DropDown, StringComparison.InvariantCultureIgnoreCase))
            {
                selectOption.DisplayText = BuildPiDefaultDisplayName(selectionInstance, partnerName, country, setting: setting);
                selectOption.DisplayImageUrl = GetPaymentInstrumentLogoUrl(selectionInstance);
            }
            else
            {
                selectOption.DisplayContent = new GroupDisplayHint
                {
                    HintId = Constants.DisplayHintIdPrefixes.PaymentOptionContainer + selectionInstance.PaymentInstrumentId,
                    LayoutOrientation = "inline"
                };

                selectOption.DisplayContent.AddDisplayTag(Constants.DisplayTag.PiContainer, Constants.DisplayTag.PiContainer);

                string alternateSvgForPartner = CheckForReactNativeAlternatePaymentInstrumentLogoUrl(selectionInstance, partnerName, exposedFlightFeatures);

                ImageDisplayHint logo = new ImageDisplayHint
                {
                    SourceUrl = alternateSvgForPartner ?? GetPaymentInstrumentLogoUrl(selectionInstance),
                    HintId = alternateSvgForPartner != null
                         ? string.Format("{0}-{1}_{2}", Constants.DisplayHintIdPrefixes.PaymentOptionLogoType, selectionInstance.PaymentMethod.PaymentMethodType, selectionInstance.PaymentInstrumentId)
                         : Constants.DisplayHintIdPrefixes.PaymentOptionLogo + selectionInstance.PaymentInstrumentId
                };

                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                {
                    string logoName = selectionInstance.PaymentMethod.Display.Name;
                    logo.AddDisplayTag("accessibilityName", logoName);
                }

                selectOption.DisplayContent.Members.Add(logo);

                GroupDisplayHint verticalTextGroup = new GroupDisplayHint { HintId = Constants.DisplayHintIdPrefixes.PaymentOptionDisplayGroup + selectionInstance.PaymentInstrumentId };
                TextGroupDisplayHint textGroup = new TextGroupDisplayHint { HintId = Constants.DisplayHintIdPrefixes.PaymentOptionTextGroup + selectionInstance.PaymentInstrumentId };

                string hintId = Constants.DisplayHintIdPrefixes.PaymentOptionText + selectionInstance.PaymentInstrumentId;

                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                {
                    List<string> displayTextList = GetPaymentInstrumentDisplayTextSeparateNameAndNumber(selectionInstance, partnerName, language, exposedFlightFeatures);

                    textGroup.Members.Add(CreateRadioTextDisplayHint(displayTextList[0], hintId + "-line1", Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Polite));
                    textGroup.Members.Add(CreateRadioTextDisplayHint(displayTextList[1], hintId + "-line2", Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Polite));

                    if (ExpiryActionNeeded(selectionInstance) && !(exposedFlightFeatures.Contains("IndiaExpiryGroupDelete") && string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase)))
                    {
                        GroupDisplayHint expiredInlineGroup = new GroupDisplayHint
                        {
                            HintId = Constants.DisplayHintIdPrefixes.PaymentOptionExpiredInlineGroup + selectionInstance.PaymentInstrumentId,
                            LayoutOrientation = "inline",
                        };

                        GroupDisplayHint warningIcon = new GroupDisplayHint() { HintId = Constants.GroupDisplayHintIds.WarningIcon + "-group" };
                        warningIcon.Members.Add(CreateRadioTextDisplayHint("!", Constants.GroupDisplayHintIds.WarningIcon + "-text", Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Polite));

                        expiredInlineGroup.Members.Add(warningIcon);
                        expiredInlineGroup.Members.Add(CreateRadioTextDisplayHint(displayTextList[2], Constants.TextDisplayHintIds.Expired + hintId + "-line3", Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Polite));
                        textGroup.Members.Add(expiredInlineGroup);
                    }
                    else
                    {
                        textGroup.Members.Add(CreateRadioTextDisplayHint(displayTextList[2], hintId + "-line3", Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Polite));
                    }
                }
                else
                {
                    textGroup.Members.Add(CreateRadioTextDisplayHint(BuildPiDefaultDisplayName(selectionInstance, partnerName, country: country, flightNames: exposedFlightFeatures, showExpiryMonthAndYear: TemplateHelper.IsListPiTemplate(setting)), hintId));
                }

                if (optionAction.HasValue && PaymentInstrumentActions.GetPaymentInstrumentActions().ContainsKey(optionAction.Value))
                {
                    TextDisplayHint actionHint = CreateRadioTextDisplayHint(" " + PidlModelHelper.GetLocalizedString(PaymentInstrumentActions.GetPaymentInstrumentActions()[optionAction.Value].DisplayText), Constants.DisplayHintIdPrefixes.PaymentOptionAction + selectionInstance.PaymentInstrumentId);
                    textGroup.Members.Add(actionHint);

                    if (optionAction.Equals(PIActionType.UpdateResource))
                    {
                        actionHint.AddDisplayTag("text-alert", "text-alert");
                        textGroup.Members.Add(new HyperlinkDisplayHint
                        {
                            HintId = Constants.DisplayHintIdPrefixes.PaymentOptionUpdate + selectionInstance.PaymentInstrumentId,
                            DisplayContent = TemplateHelper.IsListPiTemplate(setting) ? PidlModelHelper.GetLocalizedString(Constants.PaymentMethodOptionStrings.Edit) : PidlModelHelper.GetLocalizedString(Constants.PaymentMethodOptionStrings.Update),
                            Action = selectOption.PidlAction
                        });
                    }
                }

                verticalTextGroup.Members.Add(textGroup);

                if (exposedFlightFeatures?.Contains(Flighting.Features.PXEnableXboxNativeListPIRewardsPointsDisplay) == true &&
                    string.Equals(country, Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase) &&
                    selectionInstance.PaymentInstrumentDetails.IsXboxCoBrandedCard == true &&
                    selectionInstance.PaymentInstrumentDetails.PointsBalanceDetails?.RewardsEnabled == true)
                {
                    UpdatePIDisplayHintForXboxCard(selectionInstance, verticalTextGroup, country, language);
                }

                if (IsStoredValue(selectionInstance.PaymentMethod))
                {
                    TextDisplayHint storedValueElement = null;
                    if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                    {
                        storedValueElement = CreateRadioTextDisplayHint(
                        CurrencyHelper.FormatCurrency(country, language, selectionInstance.PaymentInstrumentDetails.Balance, selectionInstance.PaymentInstrumentDetails.Currency), Constants.DisplayHintIdPrefixes.PaymentOptionBalance + selectionInstance.PaymentInstrumentId, Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Polite);
                    }
                    else
                    {
                        storedValueElement = CreateRadioTextDisplayHint(
                        CurrencyHelper.FormatCurrency(country, language, selectionInstance.PaymentInstrumentDetails.Balance, selectionInstance.PaymentInstrumentDetails.Currency), Constants.DisplayHintIdPrefixes.PaymentOptionBalance + selectionInstance.PaymentInstrumentId);
                    }

                    verticalTextGroup.Members.Add(storedValueElement);
                }

                if (selectOption.IsDisabled && !optionAction.Equals(PIActionType.UpdateResource))
                {
                    TextDisplayHint disabledText = CreateRadioTextDisplayHint(
                        PidlModelHelper.GetLocalizedString(Constants.PaymentMethodOptionStrings.Disabled),
                        Constants.DisplayHintIdPrefixes.PaymentOptionDisabled + selectionInstance.PaymentInstrumentId);

                    disabledText.AddDisplayTag("text-alert", "text-alert");
                    verticalTextGroup.Members.Add(disabledText);
                }

                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName))
                {
                    GroupDisplayHint selectedTextGroup = new GroupDisplayHint
                    {
                        HintId = Constants.DisplayHintIdPrefixes.PaymentOptionSelectedTextGroup + selectionInstance.PaymentInstrumentId,
                        LayoutOrientation = "vertical",
                    };

                    TextDisplayHint focusTextHint = GetFocusedTextString(selectionInstance, hintId);

                    focusTextHint.AddDisplayTag("disableLive", "disable-live");
                    selectedTextGroup.Members.Add(focusTextHint);
                    verticalTextGroup.Members.Add(selectedTextGroup);
                }

                selectOption.DisplayContent.Members.Add(verticalTextGroup);
            }
        }

        private static void UpdatePIDisplayHintForXboxCard(PaymentInstrument selectionInstance, GroupDisplayHint textGroup, string country, string language)
        {
            int pointsBalance = selectionInstance.PaymentInstrumentDetails.PointsBalanceDetails.RewardsSummary.PointsBalance;
            double currencyBalance = selectionInstance.PaymentInstrumentDetails.PointsBalanceDetails.RewardsSummary.CurrencyBalance;
            string currencyCode = selectionInstance.PaymentInstrumentDetails.PointsBalanceDetails.RewardsSummary.CurrencyCode;
            string formattedCurrency = CurrencyHelper.FormatCurrency(country, language, Convert.ToDecimal(currencyBalance), currencyCode);

            GroupDisplayHint cardImageWrapperGroup = new GroupDisplayHint()
            {
                HintId = Constants.GroupDisplayHintIds.CardImageWrapperGroup
            };
            ImageDisplayHint cardImage = new ImageDisplayHint()
            {
                SourceUrl = Constants.PidlUrlConstants.XboxCardImage,
                HintId = Constants.ImageDisplayHintIds.CardImage
            };
            cardImageWrapperGroup.AddDisplayHint(cardImage);

            GroupDisplayHint cardPointsTextWrapperGroup = new GroupDisplayHint()
            {
                HintId = Constants.GroupDisplayHintIds.CardPointsTextWrapperGroup
            };
            TextDisplayHint cardPointsText = new TextDisplayHint()
            {
                DisplayContent = string.Format(PidlModelHelper.GetLocalizedString(Constants.PaymentMethodFormatStrings.PointsFormat), pointsBalance),
                HintId = Constants.TextDisplayHintIds.CardPointsText
            };
            TextDisplayHint currencyValueText = new TextDisplayHint()
            {
                DisplayContent = string.Format(Constants.PaymentMethodFormatStrings.PointsCurrencyFormat, formattedCurrency),
                HintId = Constants.TextDisplayHintIds.CurrencyValueText
            };
            cardPointsTextWrapperGroup.AddDisplayHint(cardPointsText);
            cardPointsTextWrapperGroup.AddDisplayHint(currencyValueText);

            GroupDisplayHint cardPointsGroup = new GroupDisplayHint()
            {
                HintId = Constants.TextDisplayHintIds.CardPointsGroup
            };
            cardPointsGroup.AddDisplayHint(cardImageWrapperGroup);
            cardPointsGroup.AddDisplayHint(cardPointsTextWrapperGroup);

            textGroup.Members.Add(cardPointsGroup);
        }

        private static TextDisplayHint GetFocusedTextString(PaymentInstrument pi, string hintId)
        {
            TextDisplayHint focusedText = CreateRadioTextDisplayHint(Constants.PaymentMethodOptionStrings.UseThisPaymentMethod, Constants.TextDisplayHintIds.UseThisPaymentMethod + hintId);

            if (ExpiryActionNeeded(pi))
            {
                focusedText = CreateRadioTextDisplayHint(Constants.PaymentMethodOptionStrings.FixThisWayToPay, Constants.TextDisplayHintIds.FixThisWayToPay + hintId);
            }
            else if (string.Equals(pi.PaymentMethod.PaymentMethodType, Constants.PaymentMethodTypeNames.Paysafecard, StringComparison.OrdinalIgnoreCase))
            {
                focusedText = CreateRadioTextDisplayHint(Constants.PaymentMethodOptionStrings.UsePaysafecard, Constants.TextDisplayHintIds.UseThisPaymentMethod + hintId);
            }

            return focusedText;
        }

        private static TextDisplayHint CreateRadioTextDisplayHint(string displayContent, string hintId, string tagKey = null, string tagVal = null)
        {
            TextDisplayHint textElement = new TextDisplayHint
            {
                DisplayContent = displayContent,
                HintId = hintId
            };

            if (!string.IsNullOrEmpty(tagKey) && !string.IsNullOrEmpty(tagVal))
            {
                textElement.AddDisplayTag(tagKey, tagVal);
            }

            return textElement;
        }

        private static DisplayHintAction CreatePartnerActionPidlAction(ActionContext context, bool isDefault)
        {
            ActionContext actionContext = new ActionContext(context);
            return new DisplayHintAction("partnerAction", isDefault, actionContext, null);
        }

        private static string CheckForReactNativeAlternatePaymentInstrumentLogoUrl(PaymentInstrument pi, string partner, List<string> exposedFlightFeatures = null)
        {
            return CheckForReactNativeAlternatePaymentMethodLogoUrl(pi.PaymentMethod, partner, exposedFlightFeatures);
        }

        // TODO: Task 55128179: This if block is to get the client action response from add credit card, as it is covered under the SkipSelectPM feature for select operation.
        // The IsPXSkipGetPMCCOnly can be removed once the partners under the IsPXSkipGetPMCCOnly migrated to use the PSS feature.
        private static bool IsPXSkipGetPMCCOnly(string country, string partner)
        {
            if (Constants.CountryToPartnerCombinationsForPXSkipGetPMCCOnly.ContainsKey(country.ToLower()))
            {
                List<string> partners = Constants.CountryToPartnerCombinationsForPXSkipGetPMCCOnly[country.ToLower()];
                return partners.Contains(partner, StringComparer.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool IsUpi(PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.RealTimePayments, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodTypeNames.Upi, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUpiCommercial(PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.RealTimePayments, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodTypeNames.UpiCommercial, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEmail(PaymentMethod method)
        {
            return method.PaymentMethodFamily.Equals(Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase) && method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.Paypal, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUserLoginId(PaymentMethod method)
        {
            return method.PaymentMethodFamily.Equals(Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase) &&
                (method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.PayPay, StringComparison.OrdinalIgnoreCase)
                || method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.AlipayHK, StringComparison.OrdinalIgnoreCase)
                || method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.GCash, StringComparison.OrdinalIgnoreCase)
                || method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.TrueMoney, StringComparison.OrdinalIgnoreCase)
                || method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.TouchNGo, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsStoredValue(PaymentMethod method)
        {
            return method.EqualByFamilyAndType(Constants.PaymentMethodFamilyNames.Ewallet, Constants.PaymentMethodTypeNames.StoredValue);
        }

        // TODO: Move partner specific logic to config
        private static bool IsAmcWebPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.AmcWeb, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsListPiRadioButtonTemplate(PaymentExperienceSetting setting)
        {
            return string.Equals(setting?.Template, Constants.TemplateName.ListPiRadioButton, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsListPiButtonListTemplate(PaymentExperienceSetting setting)
        {
            return string.Equals(setting?.Template, Constants.TemplateName.ListPiButtonList, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsXboxNativePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.XboxNative, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsWindowsSettingsPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.WindowsSettings, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsMsegPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.Mseg, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsSetupOfficeSdxPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.SetupOfficeSdx, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsWebBlendsPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.Webblends, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsOxoWebDirectPartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.OXOWebDirect, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsOxoDime(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.OXODIME, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsOxoOobe(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.OXOOobe, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsListPiDropDownTemplate(PaymentExperienceSetting setting)
        {
            return string.Equals(setting?.Template, Constants.TemplateName.ListPiDropDown, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsSmbOobePartner(string partnerName)
        {
            return string.Equals(partnerName, Constants.PartnerNames.SmbOobe, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool PartnerSupportsSingleInstancePidls(string partnerName)
        {
            return partnerName.Equals(Constants.PartnerNames.Cart)
                || partnerName.Equals(Constants.PartnerNames.Webblends)
                || string.Equals(partnerName, Constants.PartnerNames.OXODIME, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerNames.OXOWebDirect, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partnerName, Constants.PartnerNames.SetupOfficeSdx, StringComparison.OrdinalIgnoreCase);
        }

        private static bool PartnerSkipsToAddResource(string partnerName)
        {
            return partnerName.Equals(Constants.PartnerNames.OfficeOobe)
                || partnerName.Equals(Constants.PartnerNames.OfficeOobeInApp)
                || partnerName.Equals(Constants.PartnerNames.SmbOobe);
        }

        private static bool AddResourceCreditCardOnly(string partnerName)
        {
            return partnerName.Equals(Constants.PartnerNames.OfficeOobe)
                || partnerName.Equals(Constants.PartnerNames.OfficeOobeInApp)
                || partnerName.Equals(Constants.PartnerNames.SmbOobe);
        }

        // TODO:Task 55128179: The cart partner check from SelectInstanceShowDisabledPIs can be removed once the migration is complete and the PSS UseDisabledPIsForSelectInstance inline feature is in use.
        private static bool SelectInstanceShowDisabledPIs(string partnerName, PaymentMethodFilters paymentMethodFilters)
        {
            // show xbox native disabled PIs only when SplitPaymentSupported = true
            return partnerName.Equals(Constants.PartnerNames.Cart) || (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) && IsSplitPaymentSupported(paymentMethodFilters));
        }

        private static bool IsSplitPaymentSupported(PaymentMethodFilters paymentMethodFilters)
        {
            return paymentMethodFilters != null && paymentMethodFilters.SplitPaymentSupported.HasValue && paymentMethodFilters.SplitPaymentSupported.Value;
        }

        private static bool IsSelectPmRadioButtonListPartner(string partnerName, PaymentExperienceSetting setting = null)
        {
            var templateInPartenrName = TemplateHelper.GetSettingTemplate(partnerName, setting, Constants.DescriptionTypes.PaymentMethodDescription, Constants.PidlResourceIdentities.PaymentMethodSelectPidl);
            return templateInPartenrName != null && string.Equals(templateInPartenrName.ToLower(), Constants.TemplateName.SelectPMRadioButtonList, StringComparison.OrdinalIgnoreCase);
        }

        private static string AppendExpiryInfo(bool showExpiryYearAndMonth, PaymentInstrument pi, string retVal)
        {
            PaymentInstrumentDetails details = pi?.PaymentInstrumentDetails;

            if (!ExpiryActionNeeded(pi) || showExpiryYearAndMonth)
            {
                retVal = string.Format("{0} {1}/{2}", retVal, showExpiryYearAndMonth ? GetExpiryMonthForListPiTemplate(details.ExpiryMonth) : details.ExpiryMonth, details.ExpiryYear.Substring(2));
            }

            return retVal;
        }

        private static void AddActionToGetPMButton(PIDLResource pidlResource, PaymentInstrument primaryInstance, string displayHint, string country, string language, string partner)
        {
            List<DisplayHint> buttonHints = pidlResource.GetAllDisplayHintsOfId(displayHint);
            PidlDocInfo pidlDocInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentInstrumentDescription, language, country, partner);

            if (IsAmcWebPartner(partner))
            {
                foreach (ButtonDisplayHint buttonHint in buttonHints)
                {
                    buttonHint.Action.ActionType = DisplayHintActionType.success.ToString();
                    buttonHint.Action.Context = CreatePaymentInstrumentActionContext(null, PIActionType.AddPaymentInstrument, pidlDocInfo, null, null);
                    buttonHint.AddDisplayTag("add-icon", "add-icon");
                }

                return;
            }

            foreach (ButtonDisplayHint buttonHint in buttonHints)
            {
                buttonHint.Action.ActionType = DisplayHintActionType.success.ToString();
                buttonHint.Action.Context = CreatePaymentInstrumentActionContext(null, null, PIActionType.SelectResourceType, pidlDocInfo);
            }

            if (primaryInstance != null)
            {
                ButtonDisplayHint backupPageHint = buttonHints.Last() as ButtonDisplayHint;
                if (backupPageHint != null)
                {
                    PidlIdentity csvSingleInstanceIdentity = new PidlIdentity(
                        Constants.DescriptionTypes.PaymentInstrumentDescription,
                        Constants.PidlOperationTypes.SelectSingleInstance,
                        country,
                        primaryInstance.PaymentInstrumentId);

                    backupPageHint.Action.Context = CreatePaymentInstrumentActionContext(primaryInstance, null, PIActionType.SelectResourceType, pidlDocInfo, null, csvSingleInstanceIdentity);
                }
            }
        }

        private static void AddActionToSelectPMButton(PIDLResource pidlResource, string displayHint, string country, string language, string partner, string primaryPiId = null, string backupPiId = null)
        {
            ButtonDisplayHint selectPMButton = pidlResource.GetDisplayHintById(displayHint) as ButtonDisplayHint;
            if (selectPMButton != null)
            {
                Dictionary<string, string> prefillData = string.IsNullOrEmpty(primaryPiId) ? null : CreatePrefillData(primaryPiId, backupPiId);
                PidlIdentity targetIdentity = new PidlIdentity(
                        Constants.DescriptionTypes.PaymentInstrumentDescription,
                        Constants.PidlOperationTypes.SelectInstance,
                        country,
                        backupPiId == null ? Constants.PidlResourceIdentities.PaymentInstrumentSelectPidl : Constants.PidlResourceIdentities.PaymentInstrumentBackupPidl);

                PidlDocInfo pidlDocInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentInstrumentDescription, language, country, partner);

                ActionContext selectContext = CreatePaymentInstrumentActionContext(null, null, PIActionType.SelectResource, pidlDocInfo, prefillData, targetIdentity);
                selectPMButton.Action = CreateSuccessPidlAction(selectContext, false);
            }
        }

        private static TextDisplayHint AddTokenExpiryDetailsOnGroupDisplayHint(string language, int partNum, PaymentInstrument pi, string tokenExpiryMonth, string tokenExpiryYear, string tokenStatus)
        {
            var expirationDisplayHint = new TextDisplayHint();

            if (!string.IsNullOrWhiteSpace(tokenExpiryMonth) && !string.IsNullOrWhiteSpace(tokenExpiryYear) && !string.IsNullOrWhiteSpace(tokenStatus))
            {
                expirationDisplayHint = new TextDisplayHint
                {
                    DisplayContent = string.Format(
                        PidlModelHelper.GetLocalizedString("Exp", language) + " {0}/{1}" + " {2}",
                        int.Parse(tokenExpiryMonth).ToString("00"),
                        tokenExpiryYear,
                        Constants.TokenExpiryStatus.Expired),
                    HintId = string.Format(Constants.DisplayHintIdPrefixes.PaymentOptionTextTemplate, partNum, pi.PaymentInstrumentId)
                };
            }
            else if (!string.IsNullOrWhiteSpace(tokenExpiryMonth) && !string.IsNullOrWhiteSpace(tokenExpiryYear))
            {
                expirationDisplayHint = new TextDisplayHint
                {
                    DisplayContent = string.Format(
                                    PidlModelHelper.GetLocalizedString("Exp", language) + " {0}/{1}",
                                    int.Parse(tokenExpiryMonth).ToString("00"),
                                    tokenExpiryYear),
                    HintId = string.Format(Constants.DisplayHintIdPrefixes.PaymentOptionTextTemplate, partNum, pi.PaymentInstrumentId)
                };
            }
            else if (!string.IsNullOrWhiteSpace(tokenStatus))
            {
                expirationDisplayHint = new TextDisplayHint
                {
                    DisplayContent = string.Format(
                                    PidlModelHelper.GetLocalizedString("Exp", language) + " {0}",
                                    Constants.TokenExpiryStatus.Expired),
                    HintId = string.Format(Constants.DisplayHintIdPrefixes.PaymentOptionTextTemplate, partNum, pi.PaymentInstrumentId)
                };
            }

            return expirationDisplayHint;
        }

        private static void AddActionToNewCCButton(PIDLResource pidlResource, string displayHint, string country, string language, string partner)
        {
            List<DisplayHint> buttonHints = pidlResource.GetAllDisplayHintsOfId(displayHint);
            PidlDocInfo pidlDocInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentInstrumentDescription, language, country, partner, null, Constants.PaymentMethodFamilyNames.CreditCard);

            foreach (ButtonDisplayHint buttonHint in buttonHints)
            {
                buttonHint.Action.ActionType = DisplayHintActionType.success.ToString();
                buttonHint.Action.Context = CreatePaymentInstrumentActionContext(null, null, PIActionType.AddResource, pidlDocInfo);
            }
        }

        private static void AddPaymentMethodLogos(PIDLResource pidlResource, HashSet<PaymentMethod> filteredPaymentMethods)
        {
            if (filteredPaymentMethods != null)
            {
                GroupDisplayHint logoGroup = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentInstrumentLogoBlock) as GroupDisplayHint;
                if (logoGroup != null)
                {
                    logoGroup.ClearDisplayHints();
                    foreach (PaymentMethod pm in filteredPaymentMethods)
                    {
                        logoGroup.AddDisplayHint(new ImageDisplayHint()
                        {
                            HintId = string.Format("{0}_{1}", Constants.DisplayHintIds.PaymentInstrumentLogoBlock, pm.PaymentMethodType),
                            SourceUrl = GetPaymentMethodLogoUrl(pm)
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Add submiturl and http method to searchTransactions submit button PIDL
        /// </summary>
        /// <param name="pidl">PIDL resource of the search transactions</param>
        /// <param name="language">Code specifying the language to localize the PIDL</param>
        /// <param name="country">Specifies the country of the user as defined by the store</param>
        /// <param name="partner">The name of the partner</param>
        private static void AddSearchTransactionSubmitLink(PIDLResource pidl, string language, string country, string partner)
        {
            ButtonDisplayHint submitButton = pidl.GetDisplayHintById(Constants.ButtonDisplayHintIds.SearchSubmitButton) as ButtonDisplayHint;
            if (submitButton != null)
            {
                string submitUrl = @"https://{pifd-endpoint}/users/{userId}/" + Constants.RestResourceNames.PaymentTransactions + "?country=" + country + "&language=" + language + "&partner=" + partner;

                submitButton.Action.Context = new RestLink()
                {
                    Href = submitUrl,
                    Method = Constants.HTTPVerbs.POST
                };
            }
        }

        private static void AddChallengeSubmitLink(PIDLResource challengePidl, string piid, string language)
        {
            ButtonDisplayHint submitButton = challengePidl.GetDisplayHintById(Constants.ButtonDisplayHintIds.VerifyCodeButton) as ButtonDisplayHint;
            if (submitButton != null)
            {
                string submitUrl = @"https://{pifd-endpoint}/users/{userId}/" + Constants.RestResourceNames.PaymentInstrumentsEx + "/" + piid + "/validateCvv";
                if (!string.IsNullOrEmpty(language))
                {
                    submitUrl += "?language=" + language;
                }

                submitButton.Action.Context = new RestLink()
                {
                    Href = submitUrl,
                    Method = Constants.HTTPVerbs.POST
                };
            }
        }

        private static SelectOptionDescription CreateSelectOption(string displayHintId, string paymentMethodOption, string logoSourceUrl, List<string> exposedFlightFeatures)
        {
            SelectOptionDescription selectOption = new SelectOptionDescription { IsDisabled = false };
            ActionContext actionContext = new ActionContext();
            selectOption.PidlAction = CreateSuccessPidlAction(actionContext, false);

            selectOption.DisplayContent = new GroupDisplayHint
            {
                HintId = Constants.DisplayHintIdPrefixes.PaymentOptionContainer + displayHintId,
                LayoutOrientation = "inline"
            };

            selectOption.DisplayContent.AddDisplayTag(Constants.DisplayTag.PiContainer, Constants.DisplayTag.PiContainer);

            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXUseFontIcons) && DisplayHintIdIconLookUpTable.ContainsKey(displayHintId))
            {
                selectOption.DisplayContent.Members.Add(new TextDisplayHint
                {
                    DisplayContent = DisplayHintIdIconLookUpTable[displayHintId],
                    HintId = Constants.DisplayHintIdPrefixes.PaymentOptionFontIcon + displayHintId
                });
            }
            else
            {
                selectOption.DisplayContent.Members.Add(new ImageDisplayHint
                {
                    SourceUrl = logoSourceUrl,
                    HintId = Constants.DisplayHintIdPrefixes.PaymentOptionLogo + displayHintId
                });
            }

            GroupDisplayHint verticalTextGroup = new GroupDisplayHint { HintId = Constants.DisplayHintIdPrefixes.PaymentOptionDisplayGroup + displayHintId };
            TextGroupDisplayHint textGroup = new TextGroupDisplayHint { HintId = Constants.DisplayHintIdPrefixes.PaymentOptionTextGroup + displayHintId };

            textGroup.Members.Add(CreateRadioTextDisplayHint(PidlModelHelper.GetLocalizedString(paymentMethodOption), Constants.DisplayHintIdPrefixes.PaymentOptionText + displayHintId, Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Polite));
            verticalTextGroup.Members.Add(textGroup);
            selectOption.DisplayContent.Members.Add(verticalTextGroup);

            selectOption.PidlAction.Context = new
            {
                id = displayHintId,
                instance = displayHintId,
            };

            return selectOption;
        }

        private static SelectOptionDescription AddNoBackPIOption(PIDLResource pidlResource, string id, string content)
        {
            SelectOptionDescription selectOption = new SelectOptionDescription();
            selectOption.DisplayText = string.Empty;
            selectOption.DisplayContent = new GroupDisplayHint
            {
                HintId = Constants.DisplayHintIdPrefixes.PaymentOptionContainer + id,
                LayoutOrientation = Constants.PartnerHintsValues.InlinePlacement
            };
            selectOption.DisplayContent.Members.Add(new TextDisplayHint { DisplayContent = content, HintId = Constants.DisplayHintIdPrefixes.PaymentOptionText + id });
            selectOption.DisplayContent.AddDisplayTag(Constants.DisplayTag.PiContainer, Constants.DisplayTag.PiContainer);
            var idDataDescription = pidlResource.DataDescription[Constants.DataDescriptionIds.Id] as PropertyDescription;
            idDataDescription.AddPossibleValue(id, string.Empty);
            PropertyDisplayHint displayHint = pidlResource.GetDisplayHintById(Constants.DescriptionTypes.PaymentInstrumentDescription) as PropertyDisplayHint;
            displayHint.AddPossibleOption(id, selectOption);

            return selectOption;
        }

        private static bool ShouldRemoveNonSplitPaymentSupportedPMs(PaymentMethodFilters filtersObject, string partnerName, string operation)
        {
            // Xbox native keeps non-split payment supported PIs in the list (and shows them as disabled)
            return IsSplitPaymentSupported(filtersObject) &&
                (!PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) ||
                (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) && string.Equals(operation, Constants.PidlOperationTypes.Select)));
        }

        private static bool ShouldAddNonSplitPaymentSupportedPI(PaymentMethodFilters filtersObject, PaymentInstrument pi, string partnerName)
        {
            return PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partnerName) && IsSplitPaymentSupported(filtersObject) && !pi.PaymentMethod.Properties.SplitPaymentSupported;
        }
    }
}