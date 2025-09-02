// <copyright file="PidlDigitizationResourceFactory.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Web;
    using Microsoft.Commerce.Payments.PidlModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json.Linq;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using System.Threading.Tasks;

    /// <summary>
    /// This class is responsible for returning the PIDL pages used for Digitization purposes.
    /// </summary>
    public sealed class PidlDigitizationResourceFactory
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="PidlDigitizationResourceFactory"/> class from being created.
        /// </summary>
        private PidlDigitizationResourceFactory()
        {
        }

        public static void AddClientActionForPI(PaymentInstrument paymentInstrument, string accountId, string language, EventTraceActivity traceActivityId, string partnerName = Constants.PidlConfig.DefaultPartnerName, string pidlBaseUrl = null)
        {
            ClientAction clientAction = null;
            ChallengePidlArgs challengeArgs = new ChallengePidlArgs()
            {
                AccountId = accountId,
                PaymentInstrument = paymentInstrument,
                RevertChallengeOption = false,
                Language = language,
                EventTraceActivity = traceActivityId,
                PartnerName = partnerName
            };

            challengeArgs.PifdBaseUrl = pidlBaseUrl;
            clientAction = BuildDigitizationClientAction(challengeArgs);
            paymentInstrument.ClientAction = clientAction;
        }

        public static List<PIDLResource> GetChallengePidl(ChallengePidlArgs challengeArgs)
        {
            PaymentInstrument digitizedCard = challengeArgs.PaymentInstrument;
            string accountId = challengeArgs.AccountId;
            string language = challengeArgs.Language;
            string partnerName = challengeArgs.PartnerName;
            string baseUrl = challengeArgs.PifdBaseUrl;

            bool revertChallengeOption = challengeArgs.RevertChallengeOption;

            if (digitizedCard == null)
            {
                throw new PIDLArgumentException("challengeArgs.DigitizedCard is null", Constants.ErrorCodes.PIDLArgumentChallengeHasNullDigitizedCard);
            }

            if (string.IsNullOrEmpty(accountId))
            {
                throw new PIDLArgumentException("challengeArgs.AccountId", Constants.ErrorCodes.PIDLArgumentChallengeHasNullAccountId);
            }

            string piid = digitizedCard.PaymentInstrumentId;

            language = Helper.TryToLower(language);

            // Set context
            Context.Culture = Helper.GetCultureInfo(language);

            List<PIDLResource> pidlResource = new List<PIDLResource>();

            if (digitizedCard.Status == PaymentInstrumentStatus.Pending)
            {
                // Need to override self and submit_url link in the PIDL resource
                var digitizationLinks = GetDigitizationLinks(accountId, piid, partnerName, baseUrl);

                if (string.Equals(digitizedCard.PaymentInstrumentDetails.PendingOn, Constants.PendingOperation.TermsAndConditions, StringComparison.OrdinalIgnoreCase))
                {
                    pidlResource = GetTermsAndConditions(digitizedCard, partnerName, challengeArgs.EventTraceActivity);
                }
                else if (string.Equals(digitizedCard.PaymentInstrumentDetails.PendingOn, Constants.PendingOperation.Cvv, StringComparison.OrdinalIgnoreCase))
                {
                    pidlResource = GetCvv(partnerName);
                }
                else if (string.Equals(digitizedCard.PaymentInstrumentDetails.PendingOn, Constants.PendingOperation.UserIdentificationAndVerification, StringComparison.OrdinalIgnoreCase))
                {
                    var selectedActivationMethod = GetSelectedActivationMethod(digitizedCard);

                    if (selectedActivationMethod == null)
                    {
                        pidlResource = GetChallengeSelection(digitizedCard, partnerName, true);
                    }
                    else
                    {
                        if (revertChallengeOption)
                        {
                            pidlResource = GetChallengeSelection(digitizedCard, partnerName);
                        }
                        else
                        {
                            pidlResource = GetChallengeResolution(digitizedCard, partnerName);
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException(string.Format("Digitization state not supported", digitizedCard.PaymentInstrumentDetails.PendingOn));
                }

                AddDigitizationLinks(digitizationLinks, pidlResource);
            }

            return pidlResource;
        }

        private static ClientAction BuildDigitizationClientAction(ChallengePidlArgs challengeArgs)
        {
            PaymentInstrument digitizedCard = challengeArgs.PaymentInstrument;
            string accountId = challengeArgs.AccountId;

            if (digitizedCard == null)
            {
                throw new PIDLArgumentException("challengeArgs.DigitizedCard is null", Constants.ErrorCodes.PIDLArgumentChallengeHasNullDigitizedCard);
            }

            if (string.IsNullOrEmpty(accountId))
            {
                throw new PIDLArgumentException("challengeArgs.AccountId", Constants.ErrorCodes.PIDLArgumentChallengeHasNullAccountId);
            }

            ClientAction clientAction = null;

            if (string.Equals(challengeArgs.PaymentInstrument.PaymentInstrumentDetails.PendingOn, Constants.PendingOperation.Notification, StringComparison.OrdinalIgnoreCase))
            {
                clientAction = new ClientAction(ClientActionType.Wait);
                clientAction.Context = null;
            }
            else if (string.Equals(challengeArgs.PaymentInstrument.PaymentInstrumentDetails.PendingOn, Constants.PendingOperation.ProvisionConfirmation, StringComparison.OrdinalIgnoreCase))
            {
                clientAction = new ClientAction(ClientActionType.ExecuteScriptAndResume);
                clientAction.Context = null;
            }
            else
            {
                clientAction = new ClientAction(ClientActionType.Pidl);
                clientAction.Context = GetChallengePidl(challengeArgs);
            }

            return clientAction;
        }

        /// <summary>
        /// Gets the Pidl page for Terms and conditions for digitization
        /// </summary>
        /// <param name="digitizedCard">Object of Digitized card</param>
        /// <param name="partnerName">The name of the partner for which the Terms and conditions page is requested</param>
        /// <param name="traceActivityId">The trace activity id for this requext</param>
        /// <returns>List of <see cref="PIDLResource"/></returns>
        private static List<PIDLResource> GetTermsAndConditions(PaymentInstrument digitizedCard, string partnerName, EventTraceActivity traceActivityId = null)
        {
            var overrideLinks = new Dictionary<string, RestLink>();
            overrideLinks[Constants.LinkNames.Self] = GetSelfRestLink();

            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.ChallengeDescription },
                { Constants.DescriptionIdentityFields.Type, Constants.ChallengeDescriptionTypes.TermsAndConditions }
            });

            PIDLResourceFactory.Instance.GetPIDLResourceRecursive(
                partnerName, 
                Constants.DescriptionTypes.ChallengeDescription, 
                Constants.ChallengeDescriptionTypes.TermsAndConditions, 
                GlobalConstants.Defaults.CountryKey, 
                GlobalConstants.Defaults.OperationKey, 
                retVal, 
                overrideLinks);

            // Need to override the SourceUrl property of terms and conditions url
            JObject pendingDetails = (JObject)digitizedCard.PaymentInstrumentDetails.PendingDetails;

            if (pendingDetails != null)
            {
                string termsAndConditionsUrl = string.Empty;
                string mimeType = string.Empty;

                JToken termsAndConditionsJobject = pendingDetails[Constants.PendingOperation.TermsAndConditionsContext];
                if (termsAndConditionsJobject != null && termsAndConditionsJobject[Constants.PendingOperation.Url] != null)
                {
                    termsAndConditionsUrl = termsAndConditionsJobject[Constants.PendingOperation.Url].ToString();
                    if (termsAndConditionsJobject[Constants.PendingOperation.MimeType] != null)
                    {
                        mimeType = termsAndConditionsJobject[Constants.PendingOperation.MimeType].ToString().ToLower();
                    }
                }

                if (string.IsNullOrEmpty(termsAndConditionsUrl))
                {
                    throw new PIDLException(string.Format("The terms and conditions url on the Payment Instrument with Id: {0} is null", digitizedCard.PaymentInstrumentId), Constants.ErrorCodes.PIDLInvalidUrl);
                }

                // Populate t&c pidl TermsAndConditionsWebView/TermsAndConditionsText
                if (string.IsNullOrEmpty(mimeType) || mimeType == "text/plain")
                {
                    // Remove TermsAndConditionsWebView pidl from list
                    PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<WebViewDisplayHint>(Constants.DigitizationDisplayHintIds.TermsAndConditionsWebView, retVal.DisplayPages);

                    // Update TextDisplayHint 
                    TextDisplayHint displayHintTermsAndConditions = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<TextDisplayHint>(Constants.DigitizationDisplayHintIds.TermsAndConditionsText, retVal.DisplayPages);
                    if (displayHintTermsAndConditions != null)
                    {
                        displayHintTermsAndConditions.DisplayContent = GetTermsAndConditionsTextAsync(traceActivityId, termsAndConditionsUrl).Result;
                    }
                    else
                    {
                        throw new PIDLException("Pidl termsAndConditionsText is missing in configuration", Constants.ErrorCodes.PIDLConfigPIDLResourceForIdIsMissing);
                    }
                }
                else
                {
                    // Remove TextDisplayHint pidl from list
                    PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<TextDisplayHint>(Constants.DigitizationDisplayHintIds.TermsAndConditionsText, retVal.DisplayPages);

                    // Modify WebViewDisplayhint
                    WebViewDisplayHint displayHintTermsAndConditions = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<WebViewDisplayHint>(Constants.DigitizationDisplayHintIds.TermsAndConditionsWebView, retVal.DisplayPages);
                    if (displayHintTermsAndConditions != null)
                    {
                        displayHintTermsAndConditions.SourceUrl = termsAndConditionsUrl;
                    }
                    else
                    {
                        throw new PIDLException("Pidl termsAndConditionsWebView is missing in configuration", Constants.ErrorCodes.PIDLConfigPIDLResourceForIdIsMissing);
                    }
                }
            }

            List<PIDLResource> retList = new List<PIDLResource> { retVal };
            return retList;
        }

        /// <summary>
        /// Gets the Pidl page for Cvv for digitization
        /// </summary>
        /// <param name="partnerName">The name of the partner</param>
        /// <returns>List of <see cref="PIDLResource"/></returns>
        private static List<PIDLResource> GetCvv(string partnerName)
        {
            var overrideLinks = new Dictionary<string, RestLink>();
            overrideLinks[Constants.LinkNames.Self] = GetSelfRestLink();

            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.ChallengeDescription },
                { Constants.DescriptionIdentityFields.Type, Constants.ChallengeDescriptionTypes.Cvv }
            });

            PIDLResourceFactory.Instance.GetPIDLResourceRecursive(
                partnerName, 
                Constants.DescriptionTypes.ChallengeDescription, 
                Constants.ChallengeDescriptionTypes.Cvv, 
                GlobalConstants.Defaults.CountryKey, 
                GlobalConstants.Defaults.OperationKey, 
                retVal, 
                overrideLinks);

            List<PIDLResource> retList = new List<PIDLResource> { retVal };

            return retList;
        }

        /// <summary>
        /// Gets the Pidl for Challenge Selection for digitization
        /// </summary>
        /// <param name="digitizedCard">An object of Digitized card</param>
        /// <param name="partnerName">The name of the partner</param>
        /// <param name="suppressCodeLink">A boolean indicating whether the Code link needs to be suppresed</param>
        /// <returns>List of <see cref="PIDLResource"/></returns>
        private static List<PIDLResource> GetChallengeSelection(PaymentInstrument digitizedCard, string partnerName, bool suppressCodeLink = false)
        {
            var overrideLinks = new Dictionary<string, RestLink>();
            overrideLinks[Constants.LinkNames.Self] = GetSelfRestLink();

            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.ChallengeDescription },
                { Constants.DescriptionIdentityFields.Type, Constants.ChallengeDescriptionTypes.ChallengeSelection }
            });

            PIDLResourceFactory.Instance.GetPIDLResourceRecursive(
                partnerName, 
                Constants.DescriptionTypes.ChallengeDescription, 
                Constants.ChallengeDescriptionTypes.ChallengeSelection, 
                GlobalConstants.Defaults.CountryKey, 
                GlobalConstants.Defaults.OperationKey, 
                retVal, 
                overrideLinks);

            // Need to override the possible values in "activation_method_id" with the activation methods provided by PIMS
            PropertyDescription activationMethodIdProperty = retVal.DataDescription[Constants.PendingOperations.ActivationMethodIdProperty] as PropertyDescription;
            Dictionary<string, string> localizedActivationMethodsFormats = new Dictionary<string, string>(activationMethodIdProperty.PossibleValues);

            PropertyDisplayHint activationMethodPossibleValueDisplayHints = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<PropertyDisplayHint>(Constants.DigitizationDisplayHintIds.ChallengeSelectionOption, retVal.DisplayPages);

            Dictionary<string, Tuple<string, string>> availableActivationMethods = GetAvailableActivationMethods(digitizedCard);

            // localizedActivationMethodsFormats would contain the below (localized) activation method formats as key value pairs:
            //   textactivationcode,           Text {0}
            //   emailactivationcode,          Email {0}
            //   issuercallcackactivationcode, Call {0}

            // Use the (localized) formats from localizedActivationMethodsFormats to construct availalbe activation methods, as possible values
            // to "activation_method_id" property description.
            activationMethodIdProperty.PossibleValues.Clear();

            if (activationMethodPossibleValueDisplayHints != null)
            {
                activationMethodPossibleValueDisplayHints.PossibleValues.Clear();
            }

            foreach (var providedActivationMethodId in availableActivationMethods.Keys)
            {
                string activationMethodType = availableActivationMethods[providedActivationMethodId].Item1.ToLower();
                string activationMethodValue = availableActivationMethods[providedActivationMethodId].Item2;

                // If the activation method is not in the list of supported PIMS activation method types then continue
                // Note: When we move to AP we would have a telemetry logged in this scenario
                if (!IsValidActivationMethod(activationMethodType))
                {
                    continue;
                }

                // If the activation method is in the list of supported PIMS activation method types
                if (!IsSupportedActivationMethod(activationMethodType))
                {
                    continue;
                }

                // If we do not have a mapping of the activationMethodType on PX side then throw a config exception
                // but not understood by PX then silently ignore the activation
                if (!localizedActivationMethodsFormats.ContainsKey(activationMethodType))
                {
                    throw new PIDLConfigException(
                        "The ActivationMethodType : {0} is not supported in the Config",
                        Constants.ErrorCodes.PIDLConfigActivationMethodTypeIsMissing);
                }

                activationMethodIdProperty.PossibleValues[providedActivationMethodId] =
                    string.Format(
                    localizedActivationMethodsFormats[activationMethodType],
                    activationMethodValue);

                if (activationMethodPossibleValueDisplayHints != null)
                {
                    activationMethodPossibleValueDisplayHints.PossibleValues[providedActivationMethodId] =
                                        string.Format(
                                        localizedActivationMethodsFormats[activationMethodType],
                                        activationMethodValue);

                    var textHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<TextDisplayHint>(
                        activationMethodType,
                        retVal.DisplayPages);

                    if (textHint != null)
                    {
                        textHint.DependentPropertyValueRegex = providedActivationMethodId;
                    }
                }
            }

            List<PIDLResource> retList = new List<PIDLResource> { retVal };

            if (suppressCodeLink)
            {
                PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<HyperlinkDisplayHint>(
                    Constants.DigitizationDisplayLinkIds.HaveCode, 
                    retVal.DisplayPages);
            }

            return retList;
        }

        /// <summary>
        /// Gets the Pidl for Challenge Resolution for digitization
        /// </summary>
        /// <param name="digitizedCard">An object of Digitized card</param>
        /// <param name="partnerName">The name of the partner</param>
        /// <returns>List of <see cref="PIDLResource"/></returns>
        private static List<PIDLResource> GetChallengeResolution(PaymentInstrument digitizedCard, string partnerName)
        {
            var overrideLinks = new Dictionary<string, RestLink>();
            overrideLinks[Constants.LinkNames.Self] = GetSelfRestLink();

            var selectedActivationMethod = GetSelectedActivationMethod(digitizedCard);
            if (string.Equals(selectedActivationMethod.Item1, Constants.ActivationType.CallCustomerService, StringComparison.InvariantCultureIgnoreCase))
            {
                return GetChallengeResolutionOfflinePhone(selectedActivationMethod, partnerName);
            }

            var availableActivationMethods = GetAvailableActivationMethods(digitizedCard);

            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.ChallengeDescription },
                { Constants.DescriptionIdentityFields.Type, Constants.ChallengeDescriptionTypes.ChallengeResolution }
            });

            PIDLResourceFactory.Instance.GetPIDLResourceRecursive(
                partnerName, 
                Constants.DescriptionTypes.ChallengeDescription, 
                Constants.ChallengeDescriptionTypes.ChallengeResolution, 
                GlobalConstants.Defaults.CountryKey, 
                GlobalConstants.Defaults.OperationKey, 
                retVal, 
                overrideLinks);

            if (availableActivationMethods.Count == 1)
            {
                PIDLResourceDisplayHintFactory.Instance.RemoveDisplayHintById<HyperlinkDisplayHint>(Constants.DigitizationDisplayLinkIds.RevertChallenge, retVal.DisplayPages);
            }

            List<PIDLResource> retList = new List<PIDLResource> { retVal };
            AddDigitizationActivationValue(selectedActivationMethod.Item2, retList);
            return retList;
        }

        private static List<PIDLResource> GetChallengeResolutionOfflinePhone(Tuple<string, string> selectedActivationMethod, string partnerName)
        {
            var overrideLinks = new Dictionary<string, RestLink>();
            overrideLinks[Constants.LinkNames.Self] = GetSelfRestLink();
            
            PIDLResource retVal = new PIDLResource(new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, Constants.DescriptionTypes.ChallengeDescription },
                { Constants.DescriptionIdentityFields.Type, Constants.ChallengeDescriptionTypes.ChallengeResolutionPhoneOffline }
            });

            PIDLResourceFactory.Instance.GetPIDLResourceRecursive(
                partnerName, 
                Constants.DescriptionTypes.ChallengeDescription, 
                Constants.ChallengeDescriptionTypes.ChallengeResolutionPhoneOffline, 
                GlobalConstants.Defaults.CountryKey, 
                GlobalConstants.Defaults.OperationKey,
                retVal, 
                overrideLinks);

            List<PIDLResource> retList = new List<PIDLResource> { retVal };
            AddDigitizationActivationValue(selectedActivationMethod.Item2, retList);
            return retList;
        }

        private static RestLink GetSelfRestLink(string? requestUrl = null)
        {
            // Fallback URL when not provided
            var fallbackUrl = "http://localhost";

            // Use caller-provided URL (from HttpContext.Request) when available
            Uri currentUrl = new Uri(requestUrl?.ToLowerInvariant() ?? fallbackUrl);

            RestLink selfLink = new RestLink
            {
                Href = currentUrl.ToString(),
                Method = Constants.HTTPVerbs.GET
            };

            return selfLink;
        }

        private static Dictionary<string, RestLink> GetDigitizationLinks(string accountId, string piid, string partnerName, string baseUrl)
        {
            var retLinks = new Dictionary<string, RestLink>();
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            // Bug 1608253:[PX AP] Refactor how PIDL generates submit Urls from config instead of hardcoding it
            accountId = "users/me";

            // Need to add a submit url to PX service where the terms and conditions acceptance has to be posted
            var submitUrl = baseUrl + accountId + "/paymentInstrumentsEx/" + piid + string.Format(Constants.PendingOperations.ResumeSubPath, partnerName);
            RestLink submitUrlLink = new RestLink() { Href = submitUrl, Method = Constants.HTTPVerbs.POST };
            retLinks[Constants.DigitizationDisplayLinkIds.Next] = submitUrlLink;
            retLinks[Constants.DigitizationDisplayLinkIds.Submit] = submitUrlLink;

            var cancelUrl = baseUrl + accountId + "/paymentInstruments/" + piid + Constants.PendingOperations.CancelSubPath;
            RestLink cancelUrlLink = new RestLink() { Href = cancelUrl, Method = Constants.HTTPVerbs.POST };
            retLinks[Constants.DigitizationDisplayLinkIds.Cancel] = cancelUrlLink;

            var revertChallengeOptionUrl = baseUrl + accountId + "/challengeDescriptions?piid=" + piid + string.Format(Constants.PendingOperations.RevertChallengeSubPath, partnerName);
            RestLink revertChallengeOptionUrlLink = new RestLink() { Href = revertChallengeOptionUrl, Method = Constants.HTTPVerbs.GET };
            retLinks[Constants.DigitizationDisplayLinkIds.RevertChallenge] = revertChallengeOptionUrlLink;

            var haveCodeUrl = baseUrl + accountId + "/challengeDescriptions?piid=" + piid + string.Format(Constants.PendingOperations.HaveCodeSubPath, partnerName);
            RestLink haveCodeLink = new RestLink() { Href = haveCodeUrl, Method = Constants.HTTPVerbs.GET };

            retLinks[Constants.DigitizationDisplayLinkIds.HaveCode] = haveCodeLink;

            return retLinks;
        }

        private static void AddDigitizationLinks(Dictionary<string, RestLink> digitizationLinks, List<PIDLResource> pidlResources)
        {
            foreach (var pidlResource in pidlResources)
            {
                foreach (var linkid in digitizationLinks.Keys)
                {
                    var displayHintButton = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(linkid, pidlResource.DisplayPages);
                    if (displayHintButton != null)
                    {
                        if (displayHintButton.Action != null)
                        {
                            displayHintButton.Action.Context = digitizationLinks[linkid];
                        }
                    }
                    else
                    {
                        var displayHintText = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<HyperlinkDisplayHint>(linkid, pidlResource.DisplayPages);
                        if (displayHintText != null)
                        {
                            if (displayHintText.Action != null)
                            {
                                displayHintText.Action.Context = digitizationLinks[linkid];
                            }
                        }
                    }
                }
            }
        }

        private static void AddDigitizationActivationValue(string activationValue, List<PIDLResource> pidlResources)
        {
            var digitizationHintIds = new List<string>() 
            { 
                Constants.DigitizationDisplayHintIds.ChallengeResolutionText, 
                Constants.DigitizationDisplayHintIds.ChallengeResolutionPhoneOfflineText,
                Constants.DigitizationDisplayHintIds.DigitizationCallBank
            };

            foreach (var pidlResource in pidlResources)
            {
                foreach (var hintId in digitizationHintIds)
                {
                    var displayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<TextDisplayHint>(hintId, pidlResource.DisplayPages);
                    if (displayHint != null)
                    {
                        displayHint.DisplayContent = string.Format(displayHint.DisplayContent, activationValue);
                    }
                    else
                    {
                        var buttonDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(hintId, pidlResource.DisplayPages);
                        if (buttonDisplayHint != null)
                        {
                            if (buttonDisplayHint.Action != null)
                            {
                                buttonDisplayHint.Action.Context = activationValue;
                            }
                        }
                    }
                }
            }
        }

        private static Dictionary<string, Tuple<string, string>> GetAvailableActivationMethods(PaymentInstrument digitizedCard)
        {
            // Sample activation methods array from PIMS:
            //    "activationMethods": [{
            //    "id": "44776",
            //    "activationType": "EmailActivationCode",
            //    "value": "mdes*@mastercard.com"
            //    },
            //    {
            //    "id": "44777",
            //    "activationType": "TextActivationCode",
            //    "value": "xxx-xxx-1234"
            //    }]
            //
            // GetAvailableActivationMethods should transform and return the above (JSON) array as the below key value pairs:
            // "44776", <"EmailActivationCode","mdes*@mastercard.com">
            // "447776", <"TextActivationCode","xxx-xxx-1234">
            JObject pendingDetails = (JObject)digitizedCard.PaymentInstrumentDetails.PendingDetails;

            var activationMethodArray = (JArray)pendingDetails[Constants.PendingOperation.ActivationMethods];

            var availableActivationMethods = new Dictionary<string, Tuple<string, string>>();

            foreach (var activatonMethod in activationMethodArray)
            {
                var id = activatonMethod[Constants.PendingOperation.Id].ToString();
                var type = activatonMethod[Constants.PendingOperation.ActivationType].ToString();
                var value = activatonMethod[Constants.PendingOperation.Value].ToString();
                availableActivationMethods[id] =
                    new Tuple<string, string>(
                        type,
                        value);
            }

            return availableActivationMethods;
        }

        private static bool IsValidActivationMethod(string activationMethodType)
        {
            if (string.IsNullOrWhiteSpace(activationMethodType))
            {
                throw new PIDLArgumentException(
                    "activationMethodType is null or blank",
                    Constants.ErrorCodes.PIDLActivationMethodTypeIsNotValid);
            }

            if (string.Equals(activationMethodType, Constants.ActivationType.EmailActivation, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(activationMethodType, Constants.ActivationType.TextActivation, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(activationMethodType, Constants.ActivationType.CallCustomerService, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(activationMethodType, Constants.ActivationType.AppRedirection, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(activationMethodType, Constants.ActivationType.WebRedirect, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(activationMethodType, Constants.ActivationType.IssuerCallBackActivationCode, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool IsSupportedActivationMethod(string activationMethodType)
        {
            if (string.IsNullOrWhiteSpace(activationMethodType))
            {
                throw new PIDLArgumentException(
                    "activationMethodType is null or blank",
                    Constants.ErrorCodes.PIDLActivationMethodTypeIsNotValid);
            }

            if (string.Equals(activationMethodType, Constants.ActivationType.EmailActivation, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(activationMethodType, Constants.ActivationType.TextActivation, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(activationMethodType, Constants.ActivationType.CallCustomerService, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static Tuple<string, string> GetSelectedActivationMethod(PaymentInstrument digitizedCard)
        {
            IDictionary<string, JToken> pendingDetails = (JObject)digitizedCard.PaymentInstrumentDetails.PendingDetails;
            
            if (!pendingDetails.ContainsKey(Constants.PendingOperation.SelectedActivationMethod))
            {
                return null;
            }

            if (string.IsNullOrEmpty(pendingDetails[Constants.PendingOperation.SelectedActivationMethod].ToString()))
            {
                return null;
            }
            
            var selectedActivationMethod = (JObject)pendingDetails[Constants.PendingOperation.SelectedActivationMethod];

            return new Tuple<string, string>(
                selectedActivationMethod[Constants.PendingOperation.ActivationType].ToString(),
                selectedActivationMethod[Constants.PendingOperation.Value].ToString());
        }

        private static async Task<string> GetTermsAndConditionsTextAsync(EventTraceActivity traceActivityId, string termsAndConditionsUrl)
        {
            string termsText = string.Empty;

            var headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(HttpRequestHeader.Accept.ToString(), GlobalConstants.HeaderValues.TextContent)
            };

            await GenericHttpClient.SendAndReceiveAsync(
                GlobalConstants.HttpMethods.Get,
                termsAndConditionsUrl,
                traceActivityId,
                headers,
                null,
                payload => termsText = payload);

            return termsText;
        }
    }
}
