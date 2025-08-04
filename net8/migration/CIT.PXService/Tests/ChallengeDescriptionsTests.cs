// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using global::Tests.Common.Model;
    using global::Tests.Common.Model.Pidl;
    using global::Tests.Common.Model.PX;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using PXConstants = Microsoft.Commerce.Payments.PXCommon.Constants;

    [TestClass]
    public class ChallengeDescriptionsTests : TestBase
    {
        [DataRow("webblends_inline", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456")]
        [DataRow("oxowebdirect", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456")]
        [DataRow("webblends_inline", "Account001", "cvv", "Account001-Pi001-Visa", "123456")]
        [DataRow("oxowebdirect", "Account001", "cvv", "Account001-Pi001-Visa", "123456")]
        [DataRow("storify", "Account001", "cvv", "Account001-Pi001-Visa", "123456")]
        [TestMethod]
        public async Task GetChallengeDescription_GetByTypePiidAndSessionId(string partner, string accountId, string type, string piid, string sessionId)
        {
            // Arrange
            string url = $"/v7.0/{accountId}/challengeDescriptions?language=en-us&country=us&partner={partner}&type={type}&piid={piid}&sessionId={sessionId}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.AreEqual(1, pidls.Count, "The count of PIDLs is not as expected");
            Assert.AreEqual(type, pidls[0].Identity[Constants.DescriptionIdentityFields.Type], "The type of the PIDL is not as expected");
            Assert.AreEqual("challenge", pidls[0].Identity[Constants.DescriptionIdentityFields.DescriptionType], "The description type of the PIDL is not as expected");
        }

        /// <summary>
        /// Incase of 1PP guest checkout, cvvChallengeText should me modified.
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="isGuestCheckout"></param>
        /// <param name="cvvChallengeText"></param>
        /// <returns></returns>
        [DataRow("cart", false, "To protect your credit card from unauthorized charges, we require you to enter the security code. We will NOT save the code in your account.")]
        [DataRow("cart", true, "To protect your credit card from unauthorized charges, we require you to re-enter the security code.")]
        [TestMethod]
        public async Task Test_GetChallengeDescription_GetByTypePiidAndSessionId_cvvChallengeText(string partner, bool isGuestCheckout, string cvvChallengeText)
        {
            string type = "cvv";
            string piid = "Account001-Pi001-Visa";
            string sessionId = "123456";

            // Arrange
            string url = $"/v7.0/Account001/challengeDescriptions?language=en-us&country=us&partner={partner}&type={type}&piid={piid}&sessionId={sessionId}";
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-customer", isGuestCheckout ? CustomerHeaderTests.CustomerHeaderTestToken : string.Empty
                }
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.AreEqual(1, pidls.Count, "The count of PIDLs is not as expected");
            Assert.AreEqual(cvvChallengeText, pidls[0].GetDisplayHintById("cvvChallengeText").DisplayText());
        }

        /// <summary>
        /// This CIT is used to validate the changes in HandlePurchaseRiskChallenge for the partner Bing and PSS.
        /// It also validates the submit button text for the feature.
        /// </summary>
        /// <param name="partner">The partner name.</param>
        /// <param name="type">The type of challenge.</param>
        /// <param name="piid">The payment instrument ID.</param>
        /// <param name="country">The country code.</param>
        /// <param name="setButtonHintIdNameForFeature">Stores the HintId value for the submit button which is required to change for the feature.</param>
        /// <param name="setButtonRequireUpdateNameForFeature">Stores the new display text name for the feature which needs to be updated for the setButtonHintIdNameForFeature hintId.</param>
        /// <param name="showCardInformationInChallengeFeatureStatus">It will define the status of the feature `showCardInformationInChallenge`. When this feature is enabled, it sets the scenario `india3ds` for the template partner, allowing it to use the `authorizeCvvSequence`.</param>
        /// <param name="isFeatureAddAllFieldsRequiredTextStatus">Checks if the addAllFieldsRequiredText feature is enabled for the pssBased partner.</param>
        /// <param name="isFeaturesetActionContextEmptyEnabled">Checks if the SetActionContextEmpty feature is enabled for the pssBased partner.</param>
        /// <returns></returns>
        [DataRow("bing", "cvv", "Account001-Pi001-Visa")]
        [DataRow("officesmb", "cvv", "Account001-Pi001-Visa")]
        [DataRow("officesmb", "cvv", "Account001-Pi001-Visa", null, null)]
        [DataRow("officesmb", "cvv", "Account001-Pi001-Visa", Constants.DisplayHintIds.Cvv3DSSubmitButton, Constants.DisplayContent.NextDisplayContentText)]
        [DataRow("officesmb", "cvv", "Account001-Pi001-Visa", Constants.DisplayHintIds.Cvv3DSSubmitButton, Constants.DisplayContent.NextDisplayContentText, true)]
        [DataRow("officesmb", "cvv", "Account001-Pi001-Visa", Constants.DisplayHintIds.Cvv3DSSubmitButton, Constants.DisplayContent.NextDisplayContentText, true, true)]
        [DataRow("officesmb", "cvv", "Account001-Pi001-Visa", null, null, false, false, true)]
        [DataRow("officesmb", "cvv", "Account001-Pi001-Visa", Constants.DisplayHintIds.Cvv3DSSubmitButton, Constants.DisplayContent.NextDisplayContentText, false, false, true)]
        [DataRow("officesmb", "cvv", "Account001-Pi001-Visa", Constants.DisplayHintIds.Cvv3DSSubmitButton, Constants.DisplayContent.NextDisplayContentText, true, false, true)]
        [DataRow("officesmb", "cvv", "Account001-Pi001-Visa", Constants.DisplayHintIds.Cvv3DSSubmitButton, Constants.DisplayContent.NextDisplayContentText, true, true, true)]
        [TestMethod]
        public async Task GetChallengeDescription_GetByTypeAndPiid(string partner, string type, string piid, string setButtonHintIdNameForFeature = null, string setButtonRequireUpdateNameForFeature = null, bool showCardInformationInChallengeFeatureStatus = false, bool isFeatureAddAllFieldsRequiredTextStatus = false, bool isFeaturesetActionContextEmptyEnabled = false)
        {
            // Arrange
            foreach (string country in Constants.Countries)
            {
                string url = $"/v7.0/Account001/challengeDescriptions?partner={partner}&type={type}&piid={piid}&country={country}&language=en-us";
                var requestHeaders = new Dictionary<string, string>()
                {
                    { "x-ms-flight", "PXDisablePSSCache" }
                };

                if (string.Equals(partner, Constants.VirtualPartnerNames.OfficeSmb, StringComparison.OrdinalIgnoreCase))
                {
                    string showCardInformationInChallenge = showCardInformationInChallengeFeatureStatus ? "\"showCardInformationInChallenge\":{\"applicableMarkets\":[]}" : null;
                    string partnerSettingResponse = "{\"handlePurchaseRiskChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"features\":{" +
                        (showCardInformationInChallenge != null ? showCardInformationInChallenge + "," : string.Empty) +
                        "\"customizeDisplayContent\":{\"displayCustomizationDetail\":[{\"addAllFieldsRequiredText\":" + isFeatureAddAllFieldsRequiredTextStatus.ToString().ToLower() + ",\"setButtonDisplayContent\":{\"" + setButtonHintIdNameForFeature + "\":\"" + setButtonRequireUpdateNameForFeature + "\"}}]}," +
                        "\"customizeActionContext\":{\"displayCustomizationDetail\":[{\"setActionContextEmpty\":" + isFeaturesetActionContextEmptyEnabled.ToString().ToLower() + "}]}}}}";

                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                }

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: requestHeaders);

                // Assert
                Assert.IsNotNull(pidls);
                Assert.AreEqual(1, pidls.Count, "The count of PIDLs is not as expected");
                Assert.AreEqual(type, pidls[0].Identity[Constants.DescriptionIdentityFields.Type], "The type of the PIDL is not as expected");
                Assert.AreEqual("challenge", pidls[0].Identity[Constants.DescriptionIdentityFields.DescriptionType], "The description type of the PIDL is not as expected");

                DisplayHint cvs3DSSubmitButtonHintId = pidls[0].GetDisplayHintById(Constants.DisplayHintIds.Cvv3DSSubmitButton);
                DisplayHint submitButtonHintId = pidls[0].GetDisplayHintById(Constants.DisplayHintIds.SubmitButton);
                DisplayHint okSubmitButtonHintId = pidls[0].GetDisplayHintById("okButton");

                if (string.Equals(partner, Constants.PartnerNames.Bing, StringComparison.OrdinalIgnoreCase))
                {
                    Assert.IsNotNull(submitButtonHintId);
                }
                else
                {
                    Assert.IsNotNull(cvs3DSSubmitButtonHintId);
                    
                    // Validate the submit button text for the feature
                    // If 'setButtonRequireUpdateNameForFeature' equals 'Next', the expected value is 'Next'.
                    // If 'setButtonRequireUpdateNameForFeature' equals 'Ok', the expected value is 'Ok'.
                    // Otherwise, the expected value is 'Submit'.
                    Assert.AreEqual(string.Equals(setButtonRequireUpdateNameForFeature, Constants.DisplayContent.NextDisplayContentText, StringComparison.OrdinalIgnoreCase) ? Constants.DisplayContent.NextDisplayContentText : Constants.DisplayContent.SubmitDisplayContentText, cvs3DSSubmitButtonHintId.DisplayText());

                    // When the `showCardInformationInChallenge` inline feature is enabled, it will set the scenario to `india3ds`, allowing the template to call the `authorizeCvvPage`, which has the same UI as the Bing partner.
                    if (showCardInformationInChallengeFeatureStatus)
                    {
                        List<string> displayHintId = new List<string>
                        {
                            "challengeCardLogo",
                            "challengeCardNumber",
                            "cardLogoAndDigitsInlineGroup",
                            "cardNameLabel",
                            "cardExpiryLabel",
                            "challengeCardExpiry",
                            "challengeCvv",
                            "authorizeCvvEnterGroup",
                            "authorizeCvvNextBackInlineGroup"
                        };

                        foreach (string hintId in displayHintId)
                        {
                            Assert.IsNotNull(pidls[0].GetDisplayHintById(hintId), $"The pidl are expecting the {hintId} to be present");
                        }
                    }

                    if (isFeatureAddAllFieldsRequiredTextStatus)
                    {
                        var mandatoryFieldsMessageHint = pidls[0].GetDisplayHintById("mandatory_fields_message") as TextDisplayHint;
                        Assert.IsNotNull(mandatoryFieldsMessageHint, "mandatory_fields_message should not be null");
                        Assert.AreEqual("text", mandatoryFieldsMessageHint.DisplayHintType, "addAllFieldsRequiredText DisplayHintType");
                        Assert.AreEqual(mandatoryFieldsMessageHint.DisplayContent, "All fields are mandatory/required.", "pidl should contain 'All fields are mandatory/required.'");
                    }
                }

                DisplayHint buttonHintId = string.Equals(partner, Constants.PartnerNames.Bing, StringComparison.OrdinalIgnoreCase) ? submitButtonHintId : cvs3DSSubmitButtonHintId;
                Assert.AreEqual("submit", buttonHintId.Action.ActionType, "The action type of the button is not as expected");

                if (string.Equals(partner, Constants.PartnerNames.Bing, StringComparison.OrdinalIgnoreCase) || isFeaturesetActionContextEmptyEnabled)
                {
                    Assert.AreEqual(string.Empty, isFeaturesetActionContextEmptyEnabled ? cvs3DSSubmitButtonHintId.Action.Context.ToString() : submitButtonHintId.Action.Context.ToString());
                }
                else
                {
                    Assert.AreNotEqual(string.Empty, cvs3DSSubmitButtonHintId.Action.Context.ToString());
                }

                PXSettings.PartnerSettingsService.ResetToDefaults();
            }
        }

        [DataRow("windowsstore", "cvv", "Account001-Pi001-Visa", "us")]
        [TestMethod]
        public async Task GetChallengeDescription_GetByTypeAndPiid_Windows(string partner, string type, string piid, string country)
        {
            string expectedPSSResponse = "{\"handlePurchaseRiskChallenge\":{\"template\":\"defaulttemplate\",\"features\":{\"showRedirectURLInIframe\":{\"applicableMarkets\":[]},\"useIFrameForPiLogOn\":{\"applicableMarkets\":[]},\"cvvChallengeForWindows\":{\"applicableMarkets\":[]}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            var requestHeaders = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXUsePartnerSettingsService" }
            };

            // Arrange
            string url = $"/v7.0/Account001/challengeDescriptions?partner={partner}&type={type}&piid={piid}&country={country}&language=en-us";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, null, requestHeaders);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected not to be null");
            Assert.AreEqual(1, pidls.Count, "The count of PIDLs is not as expected");
            Assert.AreEqual(type, pidls[0].Identity[Constants.DescriptionIdentityFields.Type], "The type of the PIDL is not as expected");
            Assert.AreEqual("challenge", pidls[0].Identity[Constants.DescriptionIdentityFields.DescriptionType], "The description type of the PIDL is not as expected");
            var cancelCvv3DSSubmitGroup = pidls[0].DisplayPages[0].Members.Last() as GroupDisplayHint;
            List<string> cancelGroupStyleHints = new List<string>() { "anchor-bottom", "gap-small" };
            Assert.IsTrue(cancelGroupStyleHints.SequenceEqual(cancelCvv3DSSubmitGroup.StyleHints));

            ImageDisplayHint challengeCardLogo = pidls[0].GetDisplayHintById("challengeCardLogo") as ImageDisplayHint;
            ButtonDisplayHint cancelBackButton = pidls[0].GetDisplayHintById("cancelBackButton") as ButtonDisplayHint;
            Assert.IsTrue(challengeCardLogo.DisplayTags["noPidlddc"] == "pidlddc-assertive-live");
            Assert.IsTrue(cancelBackButton.DisplayTags["accessibilityName"] == "Back");
        }

        /// <summary>
        /// This test verifies that when the feature is enabled, the style hints are changed for the SubmitGroupDisplayHint.
        /// </summary>
        /// <param name="partner">Partner Name</param>
        /// <param name="type">Type</param>
        /// <param name="piid">PIID</param>
        /// <param name="enableFormButtonGapAdjustmentFlightFeatureEnable">When flight feature is enabled the different hintId will call with updated styling.</param>
        /// <returns></returns>
        [DataRow("macmanage", "cvv", "Account001-Pi002-MC", true)]
        [DataRow("macmanage", "cvv", "Account001-Pi002-MC", false)]
        [DataRow("macmanage", "cvv", "Account001-Pi002-MC", null)]
        [TestMethod]
        public async Task GetChallengeDescription_CVVForEnableFormButtonGapAdjustmentFeatureFlight(string partner, string type, string piid, bool enableFormButtonGapAdjustmentFlightFeatureEnable)
        {
            // Arrange
            foreach (string country in Constants.Countries)
            {
                string url = $"/v7.0/Account001/challengeDescriptions?partner={partner}&type={type}&piid={piid}&country={country}&operation=RenderPidlPage&language=en-us";

                string expectedPSSResponse = "{\"handlePurchaseRiskChallenge\":{\"template\":\"defaulttemplate\",\"features\":{\"enableFormButtonGapAdjustment\":{\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":" + enableFormButtonGapAdjustmentFlightFeatureEnable.ToString().ToLower() + " }]}}}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK);

                // Assert
                foreach (PIDLResource pidlResource in pidls)
                {
                    Assert.IsNotNull(pidlResource, "The PIDL resource is expected not to be null");
                    Assert.IsNotNull(pidlResource.DisplayPages, "The DisplayPages is expected not to be null");

                    string submitButtonHintId = enableFormButtonGapAdjustmentFlightFeatureEnable ? Constants.DisplayHintIds.CancelCvv3DSSubmitWithAdjustedGapGroup : Constants.DisplayHintIds.CancelCvv3DSSubmitGroup;
                    var updatedGroupDisplayHint = pidlResource.GetDisplayHintById(submitButtonHintId) as GroupDisplayHint;

                    Assert.IsNotNull(updatedGroupDisplayHint, $"The pidlResource is expceting the {submitButtonHintId}");

                    if (enableFormButtonGapAdjustmentFlightFeatureEnable)
                    {
                        CollectionAssert.AreEqual(Constants.StyleHints.CancelCvv3dsSubmitButtonGroupAtBottomOfFormStyleHints, updatedGroupDisplayHint.StyleHints, $"The {updatedGroupDisplayHint.HintId} is expecting the styling {updatedGroupDisplayHint.StyleHints}");
                    }
                    else
                    {
                        CollectionAssert.AreEqual(Constants.StyleHints.CancelCvv3dsSubmitButtonGroupAtBottomOfPageStyleHints, updatedGroupDisplayHint.StyleHints, $"The {updatedGroupDisplayHint.HintId} is expecting the styling {updatedGroupDisplayHint.StyleHints}");
                    }
                }

                // Reset the partner settings service
                PXSettings.PartnerSettingsService.ResetToDefaults();
            }
        }

        /// <summary>
        /// This CIT is used to verify the pidls for challenge type threeds.
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="type"></param>
        /// <param name="piid"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        [DataRow("webblends", "threeds", "Account001-Pi002-MC", "in")]
        [DataRow("oxowebdirect", "threeds", "Account001-Pi002-MC", "in")]
        [DataRow("webblends_inline", "threeds", "Account001-Pi002-MC", "in")]
        [DataRow("officesmb", "threeds", "Account001-Pi002-MC", "in")]
        [TestMethod]
        public async Task GetChallengeDescription_GetByTypeThreedsPiidAndSessionId(string partner, string type, string piid, string country)
        {
            // Arrange
            if (partner.Contains("officesmb"))
            {
                var partnerSettingResponse = PartnerSettingsServiceMockResponseProvider.GetPSSMockResponseById("Account001-PI001-fullPageRedirectionDefaultTemplate");
                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
            }

            var requestHeaders = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXDisablePSSCache" }
            };

            string url = $"/v7.0/Account001/challengeDescriptions?partner={partner}&type={type}&piid={piid}&country={country}&language=en-us&sessionId=1234-1234-1234-1234";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, null, requestHeaders);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected not to be null");
            ButtonDisplayHint nextButton = pidls[0].GetDisplayHintById(Constants.DisplayHintIds.ContinueRedirectButton) as ButtonDisplayHint;

            Assert.IsNotNull(nextButton, "Next button was not found");
            Assert.IsNotNull(nextButton.Action.Context, "Redirect url for next button was not set");
            Assert.IsNotNull(nextButton.Action.NextAction, "MoveNext action for next button was not set");

            ButtonDisplayHint continueSubmitButton = pidls[0].GetDisplayHintById(Constants.DisplayHintIds.ContinueSubmitButton) as ButtonDisplayHint;
            Assert.IsNotNull(continueSubmitButton, "Submit button was not found");
            Assert.AreEqual(continueSubmitButton.Action.Context.ToString(), string.Empty, "Submit url for continue button should be null");
            Assert.IsTrue(continueSubmitButton.Action.ActionType == "submit", "Action type for continue button was not set to submit");
        }

        /// <summary>
        /// This test is to verify that the PIDLs are returned for the Paysafecard partner,
        /// and also to verify that the PIDLs are returned for the PaysafeCard polling scenario.
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="isExpectedPSSPartnerToEnable"></param>
        /// <returns></returns>
        [DataRow("officesmb", "us")]
        [DataRow("officesmb", "at")]
        [DataRow("officesmb", "gb")]
        [DataRow("officesmb", "nl")]
        [TestMethod]
        public async Task GetChallengeDescription_GetByPiidAndSessionIdAndPollingForPSSPartner(string partner, string country)
        {
            // Arrange
            string url = $"/v7.0/Account001/challengeDescriptions?language=en-US&operation=RenderPidlPage&country={country}&partner={partner}&piid=Account001-Paysafecard&sessionId=3da30633-b4bc-426f-a235-85e2a010f859&orderId=ceffcf3f-0f87-4d88-bd9z-74c325b00746";
            string expectedPSSResponse = "{\"handlePurchaseRiskChallenge\":{\"template\":\"defaulttemplate\",\"features\":null}}";

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXDisablePSSCache"
                }
            };

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, additionaHeaders: headers);
            ClientAction clientAction = pidls[0].ClientAction;
            List<PIDLResource> resourceList = clientAction.Context as List<PIDLResource>;
            PIDLResource contextPidl = resourceList[0];
            DisplayHintAction action = contextPidl.DisplayPages[0].Action;

            // Assert
            Assert.IsNotNull(pidls);
            Assert.AreEqual(1, pidls.Count, "Count of PIDLs is not as expected");
            Assert.AreEqual("Pidl", pidls[0].ClientAction.ActionType.ToString(), "ActionType is not as expected");
            Assert.IsNotNull(pidls[0].ClientAction.Context, "Action context is expected to be non null");
            Assert.IsNotNull(clientAction);
            Assert.AreEqual("poll", action.ActionType);
            Assert.IsNotNull(action.Context);

            // List of display hint IDs to check for
            List<string> displayHintIds = new List<string>
            {
                "globalPIQrCodeHeading",
                "globalPIQrCodeScanText",
                "globalPIQrCodeImage",
                "globalPIQrCodeHintText",
                "globalPIQrCodeCancelButtonGroup",
                "paysafecardQrCodeLogo",
                "globalPIQrCodeHeading",
                "globalPIQrCodeCancelButton",
                "microsoftPrivacyTextGroup",
                "globalPIQrCodeSecondImage",
            };
            PIDLResource pidlResource = (pidls[0].ClientAction.Context as List<PIDLResource>)[0];

            // For each display hint Id
            foreach (string displayHintId in displayHintIds)
            {
                Assert.IsNotNull(pidlResource.GetDisplayHintById(displayHintId), $"DisplayHint with id {displayHintId} is missing.");
            }
        }

        /// <summary>
        /// Verifies the logon link's action is converted to a updatepollandMoveLast instead of the default navigate
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Test_GetChallengeDescription_GetByTypePiidAndSessionId_useIFrameForLPiLogOn()
        {
            string accountId = "Account001";
            string piid = "Account001-Paysafecard";
            string country = "us";
            string language = "en-US";
            string partner = "windowsstore";

            // Arrange
            string url = $"/v7.0/{accountId}/challengeDescriptions?language={language}&operation=RenderPidlPage&country={country}&partner={partner}&piid={piid}&sessionId=3da30633-b4bc-426f-a235-85e2a010f859&orderId=ceffcf3f-0f87-4d88-bd9z-74c325b00746";
            string expectedPSSResponse = "{\"handlePurchaseRiskChallenge\":{\"template\":\"defaulttemplate\",\"features\":{\"useIFrameForPiLogOn\":{}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache"
                }
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.IsNotNull(pidls, "Pidls is expected to be not null");
            var pidlContexts = pidls.FirstOrDefault()?.ClientAction?.Context as List<PIDLResource>;
            Assert.IsNotNull(pidlContexts, "pidlActionContext is expected to be not null");
            var pidl = pidlContexts.FirstOrDefault();
            Assert.IsNotNull(pidl, "Pidl is expected to be not null");

            var logOnLinkProperty = pidl.GetDisplayHintById("globalPIQrCodeRedirectButton");
            Assert.IsNotNull(logOnLinkProperty, "logOnLinkProperty");
            Assert.AreEqual("updatePollAndMoveLast", logOnLinkProperty.Action.ActionType, "logOnLinkProperty ActionType");
            Assert.AreEqual("globalPIQrCodeChallengePage", logOnLinkProperty.Action.Context.ToString(), "logOnLinkProperty Context");
            Assert.AreEqual("globalPIQrCodeChallengePage3", logOnLinkProperty.Action.DestinationId, "logOnLinkProperty DestinationId");
            Assert.AreEqual("pidlReact.noUrl", logOnLinkProperty.DisplayTags?.FirstOrDefault(tag => tag.Key == "pidlReact.noUrl").Value, "logOnLinkProperty Tag");

            var logOnLinkProperty2 = pidl.GetDisplayHintById("globalPIQrCodeRedirectButtonPage2");
            Assert.IsNotNull(logOnLinkProperty2, "logOnLinkProperty2");
            Assert.AreEqual("moveNext", logOnLinkProperty2.Action.ActionType, "logOnLinkProperty2 ActionType");
            Assert.AreEqual(string.Empty, logOnLinkProperty2.Action.Context.ToString(), "logOnLinkProperty2 Context");
            Assert.IsNull(logOnLinkProperty2.Action.DestinationId, "logOnLinkProperty2 DestinationId");
            Assert.IsFalse(logOnLinkProperty2.DisplayTags?.Any(tag => tag.Key == "pidlReact.noUrl"), "logOnLinkProperty2 Tag");

            var privacyGroup = pidl.GetDisplayHintById("microsoftPrivacyTextGroup");
            Assert.IsNotNull(privacyGroup, "privacyGroup");
            Assert.AreEqual("group", privacyGroup.DisplayHintType, "privacyGroup DisplayHintType");
            Assert.AreEqual(1, privacyGroup.StyleHints.Count, "privacyGroup StyleHints");

            var privacyText = pidl.GetDisplayHintById("microsoft_privacy_static_text");
            Assert.IsNotNull(privacyText, "privacyText");
            Assert.IsNotNull(privacyText.StyleHints, "privacyText StyleHints");
            Assert.IsTrue(privacyText.StyleHints.Any(styleHint => styleHint == "margin-end-2x-small"), "privacyText margin");
        }

        /// <summary>
        /// Verifies the logon link's action is the default navigate when useIFrameForPiLogOn feature is off.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Test_GetChallengeDescription_GetByTypePiidAndSessionId_useIFrameForLPiLogOnDisabled()
        {
            string accountId = "Account001";
            string piid = "Account001-Paysafecard";
            string country = "us";
            string language = "en-US";
            string partner = "windowsstore";

            // Arrange
            string url = $"/v7.0/{accountId}/challengeDescriptions?language={language}&operation=RenderPidlPage&country={country}&partner={partner}&piid={piid}&sessionId=3da30633-b4bc-426f-a235-85e2a010f859&orderId=ceffcf3f-0f87-4d88-bd9z-74c325b00746";
            string expectedPSSResponse = "{\"handlePurchaseRiskChallenge\":{\"template\":\"defaulttemplate\",\"features\":{}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache"
                }
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.IsNotNull(pidls, "Pidls is expected to be not null");
            var pidlContexts = pidls.FirstOrDefault()?.ClientAction?.Context as List<PIDLResource>;
            Assert.IsNotNull(pidlContexts, "pidlActionContext is expected to be not null");
            var pidl = pidlContexts.FirstOrDefault();
            Assert.IsNotNull(pidl, "Pidl is expected to be not null");
            var logOnLinkProperty = pidl.GetDisplayHintById("globalPIQrCodeRedirectButton");

            Assert.IsNotNull(logOnLinkProperty, "logOnLinkProperty");
            Assert.AreEqual("navigate", logOnLinkProperty.Action.ActionType, "logOnLinkProperty");
        }

        /// <summary>
        /// Verifies the back buttons are removed when using the removeElement PSS feature.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Test_GetChallengeDescription_GetByTypePiidAndSessionId_RemoveElements()
        {
            string accountId = "Account001";
            string piid = "Account001-Paysafecard";
            string country = "us";
            string language = "en-US";
            string partner = "windowsstore";

            // Arrange
            string url = $"/v7.0/{accountId}/challengeDescriptions?language={language}&operation=RenderPidlPage&country={country}&partner={partner}&piid={piid}&sessionId=3da30633-b4bc-426f-a235-85e2a010f859&orderId=ceffcf3f-0f87-4d88-bd9z-74c325b00746";
            string expectedPSSResponse = "{\"handlePurchaseRiskChallenge\":{\"template\":\"defaulttemplate\",\"features\":{\"removeElement\":{\"displayCustomizationDetail\":[{\"removeEwalletBackButtons\": true,\"removeSpaceInPrivacyTextGroup\": true}]}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache"
                }
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.IsNotNull(pidls, "Pidls is expected to be not null");
            var pidlContexts = pidls.FirstOrDefault()?.ClientAction?.Context as List<PIDLResource>;
            Assert.IsNotNull(pidlContexts, "pidlActionContext is expected to be not null");
            var pidl = pidlContexts.FirstOrDefault();
            Assert.IsNotNull(pidl, "Pidl is expected to be not null");
            var backButton = pidl.GetDisplayHintById("globalPIQrCodeMovePrevCancelButton");
            Assert.IsNull(backButton, "backButton");

            // removeSpaceInPrivacyTextGroup customizationDetail should remove the space element from the privacy text group
            var spaceTextElement = pidl.GetDisplayHintById("space");
            Assert.IsNull(spaceTextElement, "spaceTextElement");
        }

        /// <summary>
        /// Verifies the back buttons are not removed when the removeElement PSS feature is off or misconfigured.
        /// </summary>
        /// <returns></returns>
        [DataRow("\"removeElement\":{\"displayCustomizationDetail\":[{\"removeEwalletBackButtons\": false}]}")]
        [DataRow("\"removeElement\":{\"displayCustomizationDetail\":[{}]}")]
        [DataRow("\"removeElement\":{\"displayCustomizationDetail\":[]}")]
        [DataRow("\"removeElement\":{\"displayCustomizationDetail\":[null]}")]
        [DataRow("\"removeElement\":{\"displayCustomizationDetail\":null")]
        [DataRow("\"removeElement\":{}")]
        [DataRow("\"removeElement\":null")]
        [DataRow("")]
        [DataRow(null)]
        [TestMethod]
        public async Task Test_GetChallengeDescription_GetByTypePiidAndSessionId_RemoveElementsDisabled(string features)
        {
            string accountId = "Account001";
            string piid = "Account001-Paysafecard";
            string country = "us";
            string language = "en-US";
            string partner = "windowsstore";

            // Arrange
            string url = $"/v7.0/{accountId}/challengeDescriptions?language={language}&operation=RenderPidlPage&country={country}&partner={partner}&piid={piid}&sessionId=3da30633-b4bc-426f-a235-85e2a010f859&orderId=ceffcf3f-0f87-4d88-bd9z-74c325b00746";
            string expectedPSSResponse = "{\"handlePurchaseRiskChallenge\":{\"template\":\"defaulttemplate\",\"features\":{" + features + "}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache"
                }
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.IsNotNull(pidls, "Pidls is expected to be not null");
            var pidlContexts = pidls.FirstOrDefault()?.ClientAction?.Context as List<PIDLResource>;
            Assert.IsNotNull(pidlContexts, "pidlActionContext is expected to be not null");
            var pidl = pidlContexts.FirstOrDefault();
            Assert.IsNotNull(pidl, "Pidl is expected to be not null");
            var backButton = pidl.GetDisplayHintById("globalPIQrCodeMovePrevCancelButton");
            Assert.IsNotNull(backButton, "backButton");
        }

        [DataRow("storify", "", "2.7.0")] // Without stylehints
        [DataRow("xboxsettings", "", "2.7.2")] // Without stylehints
        [DataRow("storify", "PXEnableXboxNativeStyleHints", "2.6.0")] // Without stylehints
        [DataRow("xboxsettings", "PXEnableXboxNativeStyleHints", "2.6.2")] // Without stylehints
        [DataRow("storify", "PXEnableXboxNativeStyleHints", "2.7.0")] // With Stylehints
        [DataRow("storify", "PXEnableXboxNativeStyleHints", "2.7.2")] // With Stylehints
        [DataRow("xboxsettings", "PXEnableXboxNativeStyleHints", "2.7.0")] // With Stylehints
        [TestMethod]
        public async Task Test_GetChallengeDescription_GetPaySafeByTypePiidAndSessionId_XboxNative(string partner, string flights, string pidlSdkVersion)
        {
            string accountId = "Account001";
            string piid = "Account001-Paysafecard";
            string country = "us";
            string language = "en-US";

            // Arrange
            string url = $"/v7.0/{accountId}/challengeDescriptions?language={language}&operation=RenderPidlPage&country={country}&partner={partner}&piid={piid}&sessionId=3da30633-b4bc-426f-a235-85e2a010f859&orderId=ceffcf3f-0f87-4d88-bd9z-74c325b00746";
            var headers = new Dictionary<string, string>()
            {
                { "x-ms-flight", flights },
                { "x-ms-pidlsdk-version", pidlSdkVersion }
            };
            Version fullPidlSdkVersion = new Version(pidlSdkVersion + ".0");
            Version lowestCompatibleVersion = new Version("2.7.0.0");

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.IsNotNull(pidls, "Pidls is expected to be not null");
            var pidlContexts = pidls.FirstOrDefault()?.ClientAction?.Context as List<PIDLResource>;
            Assert.IsNotNull(pidlContexts, "pidlActionContext is expected to be not null");
            var pidl = pidlContexts.FirstOrDefault();
            Assert.IsNotNull(pidl, "Pidl is expected to be not null");

            foreach (PIDLResource resource in pidlContexts)
            {
                foreach (DisplayHint displayHint in resource.GetAllDisplayHints())
                {
                    if (fullPidlSdkVersion < lowestCompatibleVersion || flights?.Contains("PXEnableXboxNativeStyleHints") == false)
                    {
                        Assert.IsTrue(displayHint.StyleHints == null);
                    }
                    else
                    {
                        Assert.IsTrue(displayHint.StyleHints != null && displayHint.StyleHints.Count > 0);
                    }
                }
            }
        }

        [DataRow("storify", "", "2.7.0")]
        [DataRow("xboxsettings", "", "2.7.2")]
        [DataRow("storify", "ApplyAccentBorderWithGutterOnFocus", "2.6.0")]
        [DataRow("storify", "PXEnableXboxNativeStyleHints,ApplyAccentBorderWithGutterOnFocus", "2.7.2")]
        [DataRow("xboxsubs", "ApplyAccentBorderWithGutterOnFocus", "2.5.8")]
        [TestMethod]
        public async Task Test_BorderAroundButtonPaySafe(string partner, string flights, string pidlSdkVersion)
        {
            string accountId = "Account001";
            string piid = "Account001-Paysafecard";
            string country = "us";
            string language = "en-US";

            // Arrange
            string url = $"/v7.0/{accountId}/challengeDescriptions?language={language}&operation=RenderPidlPage&country={country}&partner={partner}&piid={piid}&sessionId=3da30633-b4bc-426f-a235-85e2a010f859&orderId=ceffcf3f-0f87-4d88-bd9z-74c325b00746";
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "x-ms-flight", flights },
                { "x-ms-pidlsdk-version", pidlSdkVersion }
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);
            bool shouldApplyBorder = flights?.Contains("ApplyAccentBorderWithGutterOnFocus") == true;
            bool isBorderApplied;
            
            // Assert
            Assert.IsNotNull(pidls);
            Assert.IsNotNull(pidls, "Pidls is expected to be not null");
            var pidlContexts = pidls.FirstOrDefault()?.ClientAction?.Context as List<PIDLResource>;
            Assert.IsNotNull(pidlContexts, "pidlActionContext is expected to be not null");

            foreach (PIDLResource resource in pidlContexts)
            {
                foreach (DisplayHint displayHint in resource.GetAllDisplayHints())
                {
                    ButtonDisplayHint buttonDisplayHint = displayHint as ButtonDisplayHint;
                    PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                    if (buttonDisplayHint != null)
                    {
                        isBorderApplied = buttonDisplayHint.DisplayTags?.ContainsKey(Constants.DisplayTagKeys.DisplayTagStyleHints) == true && buttonDisplayHint.DisplayTags[Constants.DisplayTagKeys.DisplayTagStyleHints] == Constants.DisplayTagValues.SelectionBorderGutterAccent;
                        Assert.AreEqual(isBorderApplied, shouldApplyBorder);
                    }
                    else if (propertyDisplayHint != null)
                    {
                        string elementType = resource.GetElementTypeByPropertyDisplayHint(propertyDisplayHint);
                        if (elementType == Constants.ElementTypes.Dropdown || elementType == Constants.ElementTypes.Textbox)
                        {
                            isBorderApplied = buttonDisplayHint.DisplayTags?.ContainsKey(Constants.DisplayTagKeys.DisplayTagStyleHints) == true && propertyDisplayHint.DisplayTags[Constants.DisplayTagKeys.DisplayTagStyleHints] == Constants.DisplayTagValues.SelectionBorderGutterAccent;
                            Assert.AreEqual(isBorderApplied, shouldApplyBorder);
                        }
                        else if (elementType == Constants.ElementTypes.ButtonList)
                        {
                            foreach (var item in propertyDisplayHint.PossibleOptions)
                            {
                                isBorderApplied = buttonDisplayHint.DisplayTags?.ContainsKey(Constants.DisplayTagKeys.DisplayTagStyleHints) == true && item.Value.DisplayTags[Constants.DisplayTagKeys.DisplayTagStyleHints] == Constants.DisplayTagValues.SelectionBorderGutterAccent;
                                Assert.AreEqual(isBorderApplied, shouldApplyBorder);
                            }
                        }
                        else
                        {
                            Assert.IsTrue(propertyDisplayHint.DisplayTags == null || !propertyDisplayHint.DisplayTags.ContainsKey(Constants.DisplayTagKeys.DisplayTagStyleHints));
                        }
                    }
                    else
                    {
                        Assert.IsTrue(displayHint.DisplayTags == null || !displayHint.DisplayTags.ContainsKey(Constants.DisplayTagKeys.DisplayTagStyleHints));
                    }
                }
            }
        }

        /// <summary>
        /// Verifies no PSS settings response still returns PIDL.
        /// </summary>
        /// <returns></returns>
        [DataRow(null)]
        [DataRow("")]
        [TestMethod]
        public async Task Test_GetChallengeDescription_GetByTypePiidAndSessionId_NoPartnerSettings(string pssResponse)
        {
            string accountId = "Account001";
            string piid = "Account001-Paysafecard";
            string country = "us";
            string language = "en-US";
            string partner = "windowsstore";

            // Arrange
            string url = $"/v7.0/{accountId}/challengeDescriptions?language={language}&operation=RenderPidlPage&country={country}&partner={partner}&piid={piid}&sessionId=3da30633-b4bc-426f-a235-85e2a010f859&orderId=ceffcf3f-0f87-4d88-bd9z-74c325b00746";
            PXSettings.PartnerSettingsService.ArrangeResponse(pssResponse);

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache"
                }
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.IsNotNull(pidls, "Pidls is expected to be not null");
            var pidlContexts = pidls.FirstOrDefault()?.ClientAction?.Context as List<PIDLResource>;
            Assert.IsNotNull(pidlContexts, "pidlActionContext is expected to be not null");
            var pidl = pidlContexts.FirstOrDefault();
            Assert.IsNotNull(pidl, "Pidl is expected to be not null");
        }

        [DataRow("officesmb", "Account004", "Account004-Pi001-IndiaUPI", "12345678-1234-1234-1234-123456789012", null)] // Upi
        [DataRow("officesmb", "Account008", "Account008-Pi001-IndiaUPI", "12345678-1234-1234-1234-123456789012", null)] // Upi_commercial
        [DataRow("officesmb", "Account008", "Account008-Pi002-IndiaUPI", "12345678-1234-1234-1234-123456789012", null)] // Upi_qr_commercial
        [DataRow("officesmb", "Account004", "Account004-Pi003-IndiaUPI", "12345678-1234-1234-1234-123456789012", null)] // Upi_qr
        [TestMethod]
        public async Task GetChallengeDescription_GetByPiidAndSessionIdForUPIFullPageRedirection_PSS(string partner, string accountId, string piid, string sessionId, string flightOverrides)
        {
            // Arrange
            string url = $"/v7.0/{accountId}/challengeDescriptions?language=en-US&operation=RenderPidlPage&partner={partner}&piid={piid}&sessionId={sessionId}";

            string expectedPSSResponse = "{\"confirmPayment\":{\"template\":\"defaulttemplate\",\"resources\":{\"challenge\":{\"real_time_payments.upi\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"},\"real_time_payments.upi_qr\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"},\"real_time_payments.upi_commercial\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"},\"real_time_payments.upi_qr_commercial\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}}}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, flightOverrides);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.AreEqual(1, pidls.Count, "Count of PIDLs is not as expected");
            Assert.AreEqual("Redirect", pidls[0].ClientAction.ActionType.ToString(), "ActionType is not as expected");
            Assert.IsNotNull(pidls[0].ClientAction.Context, "Action context is expected to be non null");
        }

        [DataRow("officesmb", "Account004", "Account004-Pi001-IndiaUPI", "12345678-1234-1234-1234-123456789012", null)] // Upi
        [DataRow("officesmb", "Account008", "Account008-Pi001-IndiaUPI", "12345678-1234-1234-1234-123456789012", null)] // Upi_commercial
        [DataRow("officesmb", "Account008", "Account008-Pi002-IndiaUPI", "12345678-1234-1234-1234-123456789012", null)] // Upi_qr_commercial
        [DataRow("officesmb", "Account004", "Account004-Pi003-IndiaUPI", "12345678-1234-1234-1234-123456789012", null)] // Upi_qr
        [TestMethod]
        public async Task GetChallengeDescription_GetByPiidAndSessionIdForUPI(string partner, string accountId, string piid, string sessionId, string flightOverrides)
        {
            // Arrange
            string url = $"/v7.0/{accountId}/challengeDescriptions?language=en-US&operation=RenderPidlPage&partner={partner}&piid={piid}&sessionId={sessionId}";

            string expectedPSSResponse = "{\"confirmPayment\":{\"template\":\"defaulttemplate\",\"resources\":{\"challenge\":{\"real_time_payments.upi\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\"},\"real_time_payments.upi_qr\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\"},\"real_time_payments.upi_commercial\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\"},\"real_time_payments.upi_qr_commercial\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\"}}}}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, flightOverrides);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.AreEqual(1, pidls.Count, "Count of PIDLs is not as expected");
            Assert.AreEqual("Pidl", pidls[0].ClientAction.ActionType.ToString(), "ActionType is not as expected");
            Assert.IsNotNull(pidls[0].ClientAction.Context, "Action context is expected to be non null");
        }

        [DataRow(PXService.GlobalConstants.Partners.Cart, true, "")]
        [DataRow(PXService.GlobalConstants.Partners.WebblendsInline, true, "")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, true, "PXEnableIndia3DS1Challenge")]
        [DataRow(PXService.GlobalConstants.Partners.Storify, true, "PXEnableIndia3DS1Challenge")]
        [DataRow(PXService.GlobalConstants.Partners.Xbet, true, "PXEnableIndia3DS1Challenge")]
        [DataRow(PXService.GlobalConstants.Partners.Saturn, true, "PXEnableIndia3DS1Challenge")]
        [DataRow(PXService.GlobalConstants.Partners.Azure, true, "")]
        [DataRow(PXService.GlobalConstants.Partners.CommercialStores, true, "")]
        [DataRow(PXService.GlobalConstants.Partners.AmcWeb, true, "PXEnableIndia3DS1Challenge")]
        [DataRow(PXService.GlobalConstants.TemplatePartners.DefaultTemplate, true, "")]
        [DataRow(PXService.GlobalConstants.TemplatePartners.DefaultTemplate, true, "PXEnableIndia3DS1Challenge")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, true, "")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, true, "PXEnableIndia3DS1Challenge")]
        [TestMethod]
        public async Task GetChallengeDescription_India3DSChallenge(string partnerName, bool challengeExpected, string flightOverrides)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "INR";
            string country = "in";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            var testHeader = new Dictionary<string, string>()
            {
                { "x-ms-test", "{\"scenarios\": \"px-service-3ds1-test-emulator\"}" }
            };

            if (string.Equals(partnerName, PXService.GlobalConstants.Partners.OfficeSMB))
            {
                string expectedPSSResponse = "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
                testHeader.Add("x-ms-flight", "PXDisablePSSCache");
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, flightOverrides, testHeader);

            foreach (PIDLResource resource in pidls)
            {
                if (challengeExpected)
                {
                    // Assert
                    if (string.Equals(partnerName, PXService.GlobalConstants.Partners.Azure, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partnerName, PXService.GlobalConstants.Partners.CommercialStores, StringComparison.OrdinalIgnoreCase))
                    {
                        var textDisplayHint = resource.GetDisplayHintById("cvvChallengeText") as TextDisplayHint;
                        Assert.IsNotNull(textDisplayHint);
                    }
                    else if (string.Equals(partnerName, PXService.GlobalConstants.Partners.Storify, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partnerName, PXService.GlobalConstants.Partners.Saturn, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partnerName, PXService.GlobalConstants.Partners.Webblends, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partnerName, PXService.GlobalConstants.Partners.WebblendsInline, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partnerName, PXService.GlobalConstants.Partners.Cart, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partnerName, PXService.GlobalConstants.Partners.AmcWeb, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partnerName, PXService.GlobalConstants.Partners.Xbet, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partnerName, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partnerName, PXService.GlobalConstants.TemplatePartners.DefaultTemplate, StringComparison.OrdinalIgnoreCase))
                    {
                        var expiry = resource.GetDisplayHintById("challengeCardExpiry") as TextDisplayHint;
                        Assert.IsNotNull(expiry);
                        var name = resource.GetDisplayHintById("challengeCardName") as TextDisplayHint;
                        Assert.IsNotNull(name);
                        if (string.Equals(partnerName, PXService.GlobalConstants.Partners.Storify, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partnerName, PXService.GlobalConstants.Partners.Saturn, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partnerName, PXService.GlobalConstants.Partners.Xbet, StringComparison.OrdinalIgnoreCase))
                        {
                            var cvv = resource.GetDisplayHintById("cvv") as PropertyDisplayHint;
                            Assert.IsNotNull(cvv);
                            Assert.IsNotNull(cvv.DisplayTags["accessibilityName"]);
                            Assert.AreEqual(cvv.DisplayTags["accessibilityName"], "Enter your cvv for visa ending in 5678 text box 1 of 1");

                            var nextButton = resource.GetDisplayHintById("cvv3DSSubmitButton") as ButtonDisplayHint;
                            Assert.IsNotNull(nextButton);
                            Assert.IsNotNull(nextButton.DisplayTags["accessibilityName"]);
                            Assert.AreEqual(nextButton.DisplayTags["accessibilityName"], "You'll be redirected to your bank's website for card verification. We'll collect your information but won't use it without your permission. Next button 1 of 2");
                        }
                    }
                    else
                    {
                        var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(resource.ClientAction.Context.ToString());
                        Assert.AreEqual(piid, paymentSession.PaymentInstrumentId);
                        Assert.AreEqual(true, paymentSession.IsChallengeRequired);
                        Assert.AreEqual("ValidatePIOnAttachChallenge", paymentSession.ChallengeType);
                        Assert.AreEqual(PaymentChallengeStatus.Succeeded, paymentSession.ChallengeStatus);
                    }
                }
                else
                {
                    var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(resource.ClientAction.Context.ToString());
                    Assert.AreEqual(piid, paymentSession.PaymentInstrumentId);
                    Assert.AreEqual(false, paymentSession.IsChallengeRequired);
                    Assert.IsNull(paymentSession.ChallengeType);
                    Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
                }
            }
        }

        [DataRow(PXService.GlobalConstants.Partners.AmcWeb, false, "PXEnablePSD2ForGuestCheckoutFlow")]
        [DataRow(PXService.GlobalConstants.Partners.AmcWeb, true, "")]
        [DataRow(PXService.GlobalConstants.Partners.Azure, false, "PXEnablePSD2ForGuestCheckoutFlow")]
        [DataRow(PXService.GlobalConstants.Partners.Azure, true, "")]
        [DataRow(PXService.GlobalConstants.Partners.CommercialStores, false, "PXEnablePSD2ForGuestCheckoutFlow")]
        [DataRow(PXService.GlobalConstants.Partners.CommercialStores, true, "")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, "PXEnablePSD2ForGuestCheckoutFlow")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, true, "")]
        [TestMethod]
        public async Task GetChallengeDescription_HandlePaymentChallenge_GuestCheckout(string partnerName, bool isSessionResponseExpected, string flights)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "USD";
            string country = "us";
            string piid = "Account001-Pi001-Visa-linkedSessionId";
            string language = "en-us";

            var expectedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piid);

            PXSettings.PayerAuthService.ArrangeResponse(
               method: HttpMethod.Post,
               urlPattern: ".*/GetThreeDSMethodURL.*",
               statusCode: HttpStatusCode.OK,
               content: "{\"three_ds_server_trans_id\":\"7b41a540-cbf8-4ada-85f6-24d4705f983b\",\"three_ds_method_url\":\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/acs/fingerprint\",\"tracking_id\":\"00000000-0000-0000-0000-000000000000\"}");

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?operation=RenderPidlPage&language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            var testHeader = new Dictionary<string, string>()
            {
                { "x-ms-test", "{\"scenarios\": \"px-service-psd2-e2e-emulator\"}" },
                { "x-ms-customer", CustomerHeaderTests.CustomerHeaderTestToken }
            };

            flights = string.IsNullOrEmpty(flights) ? "PXUseGetVersionBasedPaymentSessionsHandler,PXUsePaymentSessionsHandlerV2,PXPSD2EnableCSPProxyFrame" : $"PXUseGetVersionBasedPaymentSessionsHandler,PXUsePaymentSessionsHandlerV2,PXPSD2EnableCSPProxyFrame,{flights}";
            PXFlightHandler.AddToEnabledFlights(flights);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, additionaHeaders: testHeader);

            // Assert
            Assert.IsNotNull(pidls);
            
            if (isSessionResponseExpected)
            {
                Assert.IsNotNull(pidls[0].ClientAction.Context, "Action context is expected to be non null");
                Assert.AreEqual(ClientActionType.ReturnContext, pidls[0].ClientAction.ActionType, "ActionType is not as expected");
            }
            else
            {
                Assert.AreEqual("fingerprintIFrame", pidls[0].Identity["description_type"]);

                IFrameDisplayHint threedsIfame = pidls[0].GetDisplayHintById("ThreeDSFingerprintIFrame") as IFrameDisplayHint;
                Assert.IsNotNull(threedsIfame, "Pidl expected to have the ThreeDSFingerprintIFrame");
                
                Assert.IsNotNull(expectedPI?.PaymentInstrumentDetails?.TransactionLink?.LinkedPaymentSessionId, "PI is expected to have the LinkedPaymentSessionId");

                // Expected to use the sessionId created from PIMS
                // PIMS sessionId is found in PIdetails as TransactionLink.LinkedPaymentSessionId
                Assert.IsTrue(threedsIfame.ExpectedClientActionId.Contains(expectedPI?.PaymentInstrumentDetails?.TransactionLink?.LinkedPaymentSessionId), "Expected to use the LinkedPaymentSessionId instead of new one.");
            }

            PXSettings.PayerAuthService.ResetToDefaults();
            PXFlightHandler.ResetToDefault();
        }

        [DataRow(PXService.GlobalConstants.Partners.Webblends, false)]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, true)]
        [DataRow(PXService.GlobalConstants.Partners.Azure, false)]
        [DataRow(PXService.GlobalConstants.Partners.Azure, true)]
        [DataRow(PXService.GlobalConstants.Partners.CommercialStores, false)]
        [DataRow(PXService.GlobalConstants.Partners.CommercialStores, true)]
        [DataRow(PXService.GlobalConstants.Partners.AmcWeb, false)]
        [DataRow(PXService.GlobalConstants.Partners.AmcWeb, true)]
        [DataRow(PXService.GlobalConstants.Partners.OXOOobe, false)]
        [DataRow(PXService.GlobalConstants.Partners.OXOOobe, true)]
        [TestMethod]
        public async Task GetChallengeDescription_HandlePaymentChallenge_GuestCheckout_SessionError(string partnerName, bool sessionExists)
        {
            // Arrange
            string accountId = "Account001";
            string piid = "Account001-Pi001-Visa-linkedSessionId";
            var expectedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piid);
            string pimsSessionId = expectedPI.PaymentInstrumentDetails.TransactionLink.LinkedPaymentSessionId;
            bool preProcessAssertCalled = false;
            bool createSessionCalled = false;
            string sessionResponse;

            PXSettings.PayerAuthService.ArrangeResponse(
               method: HttpMethod.Post,
               urlPattern: ".*/GetThreeDSMethodURL.*",
               statusCode: HttpStatusCode.OK,
               content: "{\"three_ds_server_trans_id\":\"7b41a540-cbf8-4ada-85f6-24d4705f983b\",\"three_ds_method_url\":\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/acs/fingerprint\",\"tracking_id\":\"00000000-0000-0000-0000-000000000000\"}");

            if (sessionExists)
            {
                sessionResponse = "{\"id\":\"" + pimsSessionId + "\",\"session_type\":\"any\",\"data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXPSD2Auth-_-_-Succeeded\\\",\\\"PXPSD2Auth-C-_-Unknown\\\",\\\"PXPSD2Auth-N-TSR10-Failed\\\",\\\"PXPSD2Auth-R-_-Failed\\\",\\\"PXPSD2Comp-_-_-_-Succeeded\\\",\\\"PXPSD2Comp-_-_-01-Cancelled\\\",\\\"PXPSD2Comp-N-_-04-TimedOut\\\",\\\"PXPSD2Comp-N-TSR10-_-Failed\\\",\\\"PXPSD2Comp-R-_-_-Failed\\\",\\\"PXPSD2SettingVersionV19\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"BrowserInfo\\\":{\\\"browser_accept_header\\\":\\\"*/*\\\",\\\"browser_ip\\\":\\\"::1\\\",\\\"browser_java_enabled\\\":true,\\\"browser_language\\\":\\\"en-US\\\",\\\"browser_color_depth\\\":\\\"8\\\",\\\"browser_screen_height\\\":\\\"480\\\",\\\"browser_screen_width\\\":\\\"640\\\",\\\"browser_tz\\\":\\\"480\\\",\\\"browser_user_agent\\\":\\\"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36\\\",\\\"challenge_window_size\\\":\\\"05\\\"},\\\"MethodData\\\":{\\\"three_ds_server_trans_id\\\":\\\"123\\\",\\\"three_ds_method_url\\\":\\\"https://testurl.com\\\"},\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"73f512ef-48fb-4322-b87d-ce6a3622ef1b\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiNzNmNTEyZWYtNDhmYi00MzIyLWI4N2QtY2U2YTM2MjJlZjFiIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":null,\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-US\\\",\\\"SuccessUrl\\\":null,\\\"FailureUrl\\\":null,\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"payment_session_id\\\":\\\"Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec\\\",\\\"account_id\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"9MEvEAAAAAAEAACA\\\",\\\"partner\\\":\\\"" + partnerName + "\\\",\\\"amount\\\":100.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"IN\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"visa\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null,\\\"cvv_token \\\":null}\",\"encrypt_data\":false,\"state\":\"INCOMPLETE\",\"test_context\":{\"scenarios\":\"px-service-billdesk-provider,px-service-psd2-e2e-emulator,px-service-3ds1-show-iframe\",\"contact\":\"kowshikp\",\"context_props\":{}}}";
                PXSettings.SessionService.ResponseProvider.SessionStore.Add(pimsSessionId, sessionResponse);
            }
            else
            {
                sessionResponse = "{\"error_code\":\"InvalidOrExpiredSessionId\",\"message\":\"Microsoft.Commerce.Payments.Storage.Session.NotFoundException: Session with key \"" + pimsSessionId + "\" is missing or expired\"}";

                PXSettings.SessionService.ArrangeResponse(
                   method: HttpMethod.Get,
                   urlPattern: ".*/sessions/.*",
                   statusCode: HttpStatusCode.NotFound,
                   content: sessionResponse);
            }

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?operation=RenderPidlPage&language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = "USD",
                                    Partner = partnerName,
                                    Country = "us",
                                    PaymentInstrumentId = piid,
                                    Language = "en-us",
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            var testHeader = new Dictionary<string, string>()
            {
                { "x-ms-test", "{\"scenarios\": \"px-service-psd2-e2e-emulator\"}" },
                { "x-ms-customer", CustomerHeaderTests.CustomerHeaderTestToken }
            };

            string flights = "PXUseGetVersionBasedPaymentSessionsHandler,PXUsePaymentSessionsHandlerV2,PXPSD2EnableCSPProxyFrame,PXEnablePSD2ForGuestCheckoutFlow";
            PXFlightHandler.AddToEnabledFlights(flights);

            PXSettings.SessionService.PreProcess = (request) =>
            {
                if (request != null && (request.Method == HttpMethod.Get || request.Method == HttpMethod.Post))
                {
                    Assert.IsTrue(request.RequestUri.AbsoluteUri.Contains($"/sessions/{pimsSessionId}"));
                    preProcessAssertCalled = true;

                    if (request.Method == HttpMethod.Post)
                    {
                        createSessionCalled = true;
                    }
                }
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, additionaHeaders: testHeader);

            // Assert
            Assert.IsTrue(preProcessAssertCalled, "SessionService was not called as expected");
            if (sessionExists)
            {
                Assert.IsFalse(createSessionCalled, "SessionService was called to create session when it should not have been");
            }
            else
            {
                Assert.IsTrue(createSessionCalled, "SessionService was not called to create session as expected");
            }

            Assert.IsNotNull(pidls);
            Assert.AreEqual("fingerprintIFrame", pidls[0].Identity["description_type"]);
            IFrameDisplayHint threedsIfame = pidls[0].GetDisplayHintById("ThreeDSFingerprintIFrame") as IFrameDisplayHint;
            
            Assert.IsNotNull(threedsIfame, "Pidl expected to have the ThreeDSFingerprintIFrame");
            Assert.IsNotNull(expectedPI?.PaymentInstrumentDetails?.TransactionLink?.LinkedPaymentSessionId, "PI is expected to have the LinkedPaymentSessionId");
            Assert.IsTrue(threedsIfame.ExpectedClientActionId.Contains(pimsSessionId), "Expected to use the LinkedPaymentSessionId instead of new one.");

            PXSettings.PayerAuthService.ResetToDefaults();
            PXSettings.SessionService.ResetToDefaults();
            PXFlightHandler.ResetToDefault();
        }

        /// <summary>
        /// Payment session with xbox rewards points through HandlePaymentChallenge pidl component
        /// </summary>
        [TestMethod]
        public async Task GetChallengeDescription_XboxRedeemRewards_HandlePaymentChallenge()
        {
            // Arrange
            string accountId = "Account001";
            bool redeemRewards = true;
            int rewardsPoints = 50;

            PXSettings.PayerAuthService.ArrangeResponse(
                 method: HttpMethod.Post,
                urlPattern: ".*/CreatePaymentSessionId.*",
                statusCode: HttpStatusCode.OK,
                content: "{ \"payment_session_id\" : \"1234\" }");

            string payAuthResp = "{\"enrollment_status\":\"bypassed\"}";
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            string url = string.Format(
                "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                accountId,
                HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                        new PaymentSessionData()
                        {
                            Amount = 10.0m,
                            Currency = "USD",
                            Partner = "cart",
                            Country = "us",
                            PaymentInstrumentId = "Account001-Pi002-MC",
                            Language = "en",
                            RewardsPoints = rewardsPoints,
                            RedeemRewards = redeemRewards
                        })));

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK);

            // Assert
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.ReturnContext, pidls[0].ClientAction.ActionType);
            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidls[0].ClientAction.Context.ToString());
            Assert.IsTrue(paymentSession.RedeemRewards);
            Assert.AreEqual(paymentSession.RewardsPoints, rewardsPoints);

            // Clean upSessionService.ArrangeResponse
            PXSettings.PayerAuthService.Dispose();
        }

        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, "Account001-Pi001-Visa")]
        [DataRow(PXService.GlobalConstants.Partners.Storify, true, "Account001-Pi001-Visa")]
        [DataRow(PXService.GlobalConstants.Partners.Xbet, true, "Account001-Pi001-Visa")]
        [DataRow(PXService.GlobalConstants.Partners.Saturn, true, "Account001-Pi001-Visa")]
        [DataRow(PXService.GlobalConstants.Partners.AmcWeb, false, "Account001-Pi001-Visa")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, "Account001-Pi002-MC")]
        [DataRow(PXService.GlobalConstants.Partners.Storify, true, "Account001-Pi002-MC")]
        [DataRow(PXService.GlobalConstants.Partners.Xbet, true, "Account001-Pi002-MC")]
        [DataRow(PXService.GlobalConstants.Partners.Saturn, true, "Account001-Pi002-MC")]
        [DataRow(PXService.GlobalConstants.Partners.AmcWeb, false, "Account001-Pi002-MC")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, "Account001-Pi003-Amex")]
        [DataRow(PXService.GlobalConstants.Partners.Storify, true, "Account001-Pi003-Amex")]
        [DataRow(PXService.GlobalConstants.Partners.Xbet, true, "Account001-Pi003-Amex")]
        [DataRow(PXService.GlobalConstants.Partners.Saturn, true, "Account001-Pi003-Amex")]
        [DataRow(PXService.GlobalConstants.Partners.AmcWeb, false, "Account001-Pi003-Amex")]
        [DataRow(PXService.GlobalConstants.Partners.DefaultTemplate, false, "Account001-Pi003-Amex")]
        [DataRow(PXService.GlobalConstants.Partners.DefaultTemplate, false, "Account001-Pi002-MC")]
        [DataRow(PXService.GlobalConstants.Partners.DefaultTemplate, false, "Account001-Pi001-Visa")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, false, "Account001-Pi003-Amex")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, false, "Account001-Pi002-MC")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, false, "Account001-Pi001-Visa")]
        [TestMethod]
        public async Task GetChallengeDescription_ReactNativeLogoSwap(string partnerName, bool reactNativePartner, string piid)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "INR";
            string country = "in";
            string language = "en-us";

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            var testHeader = new Dictionary<string, string>()
            {
                { "x-ms-test", "{\"scenarios\": \"px-service-3ds1-test-emulator\"}" }
            };

            if (string.Equals(partnerName, PXService.GlobalConstants.Partners.OfficeSMB))
            {
                string expectedPSSResponse = "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
                testHeader.Add("x-ms-flight", "PXDisablePSSCache");
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, "PXEnableIndia3DS1Challenge", testHeader);

            foreach (PIDLResource resource in pidls)
            {
                var expectedReactNativeUrl = string.Empty;
                var defaultUrl = string.Empty;
                if (piid == "Account001-Pi001-Visa")
                {
                    expectedReactNativeUrl = "https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa.svg";
                    defaultUrl = "https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa.svg";
                }
                else if (piid == "Account001-Pi002-MC")
                {
                    expectedReactNativeUrl = "https://pmservices.cp.microsoft.com/staticresourceservice/images/v4/logo_mc_left_aligned.svg";
                    defaultUrl = "https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_mc.svg";
                }
                else if (piid == "Account001-Pi003-Amex")
                {
                    expectedReactNativeUrl = "https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_amex.svg";
                    defaultUrl = "https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_amex.svg";
                }

                var logo = resource.GetDisplayHintById("challengeCardLogo") as ImageDisplayHint;
                Assert.IsNotNull(logo, "Challenge logo missing");
                Assert.IsNotNull(logo.SourceUrl, "Challenge logo url missing");

                if (reactNativePartner == true)
                {
                    Assert.AreEqual(logo.SourceUrl, expectedReactNativeUrl);
                }
                else
                {
                    Assert.AreEqual(logo.SourceUrl, defaultUrl);
                }
            }
        }

        [DataRow(PXService.GlobalConstants.Partners.SetupOfficesdx, true, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.SetupOfficesdx, true, false, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.SetupOfficesdx, false, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.SetupOfficesdx, false, false, false, "in")]
        [DataRow(PXService.GlobalConstants.Partners.SetupOffice, true, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.SetupOffice, true, false, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.SetupOffice, false, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.SetupOffice, false, false, false, "in")]
        [DataRow(PXService.GlobalConstants.Partners.XboxWeb, true, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.XboxWeb, true, false, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.XboxWeb, false, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.XboxWeb, false, false, false, "in")]
        [DataRow(PXService.GlobalConstants.Partners.Payin, true, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.Payin, true, false, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.Payin, false, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.Payin, false, false, false, "in")]
        [DataRow(PXService.GlobalConstants.Partners.ConsumerSupport, true, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.ConsumerSupport, true, false, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.ConsumerSupport, false, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.ConsumerSupport, false, false, false, "in")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeOobe, true, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeOobe, true, false, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeOobe, false, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeOobe, false, false, false, "in")]
        [DataRow(PXService.GlobalConstants.Partners.OXOOobe, true, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.OXOOobe, true, false, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.OXOOobe, false, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.OXOOobe, false, false, false, "in")]
        [DataRow(PXService.GlobalConstants.Partners.WebPay, true, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.WebPay, true, false, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.WebPay, false, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.WebPay, false, false, false, "in")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, true, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, true, false, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, true, true, "in")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, false, false, "in")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, true, true, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, true, false, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, true, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, false, false, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, true, false, true, "gb", true)]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, false, true, true, "gb", true)]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, false, false, false, "gb", true)]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, true, false, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, false, true, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, false, false, false, "gb")]
        [TestMethod]
        public async Task GetChallengeDescription_India3DSChallenge_PartnerFlight(string partnerName, bool sendPartnerFlight, bool sendTestHeader, bool shouldThreeDSOneEnabled, string country, bool isFeatureEnableValidatePIOnAttachChallenge = false)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "INR";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            if (sendPartnerFlight)
            {
                if (string.Equals("in", country, StringComparison.OrdinalIgnoreCase))
                {
                    testHeader["x-ms-flight"] = "EnableThreeDSOne,India3dsEnableForBilldesk";
                }
                else
                {
                    testHeader["x-ms-flight"] = "EnableThreeDSOne";
                }
            }

            if (sendTestHeader)
            {
                testHeader["x-ms-test"] = "{\"scenarios\":\"px-service-psd2-e2e-emulator\",\"contact\":\"pidlsdk\"}";
            }

            if (string.Equals("in", country, StringComparison.OrdinalIgnoreCase))
            {
                var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, "Account001-Pi001-Visa");
                extendedPI.PaymentInstrumentDetails.RequiredChallenge = new List<string> { "3ds" };
                PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");
            }

            if (string.Equals(partnerName, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
            {
                // The feature pxusepsstoenablevalidatepionattachchallenge is in lowercase to verify that the changes work with lowercase as well.
                string expectedPSSResponse = isFeatureEnableValidatePIOnAttachChallenge
                    ? "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"features\":{\"pxusepsstoenablevalidatepionattachchallenge\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}"
                    : "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // Act
            await GetRequest(
                url,
                testHeader,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);

                    Assert.IsNotNull(pidls);

                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    if (sendPartnerFlight)
                    {
                        Assert.IsTrue(responseHeaders.GetValues("x-ms-flight").First().IndexOf("PXEnableIndia3DS1Challenge") > 0);
                    }
                    else
                    {
                        Assert.IsTrue(responseHeaders.GetValues("x-ms-flight").First().IndexOf("PXEnableIndia3DS1Challenge") == -1);
                    }

                    if (string.Equals("gb", country, StringComparison.OrdinalIgnoreCase))
                    {
                        if (sendTestHeader)
                        {
                            Assert.AreEqual("IFramePage", pidls[0].DisplayPages[0].DisplayName);
                        }
                        else
                        {
                            var session = JsonConvert.DeserializeObject<PaymentSession>(pidls[0].ClientAction.Context.ToString());

                            if (string.Equals(partnerName, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase) && !isFeatureEnableValidatePIOnAttachChallenge)
                            {
                                Assert.IsNull(session.ChallengeType);
                            }
                            else
                            {
                                Assert.AreEqual("ValidatePIOnAttachChallenge", session.ChallengeType);
                            }
                        }
                    }
                    else if (string.Equals("in", country, StringComparison.OrdinalIgnoreCase))
                    {
                        if (shouldThreeDSOneEnabled)
                        {
                            Assert.AreEqual("authorizeCvvPage", pidls[0].DisplayPages[0].DisplayName);
                        }
                        else
                        {
                            var session = JsonConvert.DeserializeObject<PaymentSession>(pidls[0].ClientAction.Context.ToString());
                            Assert.AreEqual("NotApplicable", session.ChallengeStatus.ToString());
                        }
                    }
                });
        }

        [DataRow(PXService.GlobalConstants.Partners.Webblends, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OXODIME, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OXODIME, false, "gb")]
        [TestMethod]
        public async Task GetChallengeDescription_PSD2Challenge_PXPSD2EnableCSPProxyFrame_Fingerprint(string partnerName, bool sendCSPFlight, string country)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "GBP";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            PXSettings.PayerAuthService.ResetToDefaults();

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            if (sendCSPFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXPSD2EnableCSPProxyFrame");
            }

            testHeader["x-ms-test"] = "{\"scenarios\":\"px-service-psd2-e2e-emulator\",\"contact\":\"pidlsdk\"}";

            // Act
            await GetRequest(
                url,
                testHeader,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);

                    Assert.IsNotNull(pidls);

                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);

                    if (string.Equals("gb", country, StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.AreEqual("IFramePage", pidls[0].DisplayPages[0].DisplayName);
                        if (sendCSPFlight)
                        {
                            Assert.IsTrue(responseBody.Contains("cspStepFingerprint"));
                        }
                        else
                        {
                            Assert.IsTrue(!responseBody.Contains("cspStepFingerprint"));
                        }
                    }
                });
        }

        [DataRow(PXService.GlobalConstants.Partners.Webblends, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OXODIME, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OXODIME, false, "gb")]
        [TestMethod]
        public async Task GetChallengeDescription_PSD2Challenge_PXPSD2EnableCSPProxyFrame_Challenge(string partnerName, bool sendCSPFlight, string country)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "GBP";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            PXSettings.PayerAuthService.ArrangeResponse(
               method: HttpMethod.Post,
               urlPattern: ".*/GetThreeDSMethodURL.*",
               statusCode: HttpStatusCode.OK,
               content: "{ \"three_ds_method_url\" : \"\", \"three_ds_server_trans_id\" : \"\" }");

            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string payAuthResp = "{\"is_acs_challenge_required\":\"true\",\"enrollment_status\":\"bypassed\",\"enrollment_type\":\"three_ds\",\"three_ds_server_transaction_id\":\"f35eadb1-6517-433d-af58-a28a0150a19d\",\"acs_url\":\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\",\"acs_signed_content\":\"{\\\"MD\\\":\\\"A4560000307199\\\",\\\"PaReq\\\":\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiZjM1ZWFkYjEtNjUxNy00MzNkLWFmNTgtYTI4YTAxNTBhMTlkIn0=\\\",\\\"TermUrl\\\":\\\"https://www.merchanturl.com/Response.jsp\\\"}\",\"authenticate_update_url\":\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10009BHVVL2e8a44f83-dc56-4689-8c5d-4a7fc003c8f1/authenticate\",\"transaction_challenge_status\":\"C\",\"is_form_post_acs_url\":true,\"is_full_page_redirect\":true}";
            string sessionResp = "{\"id\":\"" + paymentSessionId + "\",\"session_type\":\"any\",\"data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXPSD2Auth-_-_-Succeeded\\\",\\\"PXPSD2Auth-C-_-Unknown\\\",\\\"PXPSD2Auth-N-TSR10-Failed\\\",\\\"PXPSD2Auth-R-_-Failed\\\",\\\"PXPSD2Comp-_-_-_-Succeeded\\\",\\\"PXPSD2Comp-_-_-01-Cancelled\\\",\\\"PXPSD2Comp-N-_-04-TimedOut\\\",\\\"PXPSD2Comp-N-TSR10-_-Failed\\\",\\\"PXPSD2Comp-R-_-_-Failed\\\",\\\"PXPSD2SettingVersionV19\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"BrowserInfo\\\":{\\\"browser_accept_header\\\":\\\"*/*\\\",\\\"browser_ip\\\":\\\"::1\\\",\\\"browser_java_enabled\\\":true,\\\"browser_language\\\":\\\"en-US\\\",\\\"browser_color_depth\\\":\\\"8\\\",\\\"browser_screen_height\\\":\\\"480\\\",\\\"browser_screen_width\\\":\\\"640\\\",\\\"browser_tz\\\":\\\"480\\\",\\\"browser_user_agent\\\":\\\"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36\\\",\\\"challenge_window_size\\\":\\\"05\\\"},\\\"MethodData\\\":{\\\"three_ds_server_trans_id\\\":\\\"123\\\",\\\"three_ds_method_url\\\":\\\"https://testurl.com\\\"},\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"73f512ef-48fb-4322-b87d-ce6a3622ef1b\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiNzNmNTEyZWYtNDhmYi00MzIyLWI4N2QtY2U2YTM2MjJlZjFiIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":null,\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-US\\\",\\\"SuccessUrl\\\":null,\\\"FailureUrl\\\":null,\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"payment_session_id\\\":\\\"Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec\\\",\\\"account_id\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"9MEvEAAAAAAEAACA\\\",\\\"partner\\\":\\\"" + partnerName + "\\\",\\\"amount\\\":100.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"IN\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"visa\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null,\\\"cvv_token \\\":null}\",\"encrypt_data\":false,\"state\":\"INCOMPLETE\",\"test_context\":{\"scenarios\":\"px-service-psd2-e2e-emulator,px-service-3ds1-show-iframe\",\"contact\":\"kowshikp\",\"context_props\":{}}}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, sessionResp);
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            if (sendCSPFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXPSD2EnableCSPProxyFrame");
            }

            testHeader["x-ms-test"] = "{\"scenarios\":\"px-service-psd2-e2e-emulator\",\"contact\":\"pidlsdk\"}";

            // Act
            await GetRequest(
                url,
                testHeader,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);

                    Assert.IsNotNull(pidls);

                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);

                    if (string.Equals("gb", country, StringComparison.OrdinalIgnoreCase))
                    {
                        if (sendCSPFlight)
                        {
                            Assert.AreEqual("PaymentChallengePage", pidls[0].DisplayPages[0].DisplayName);
                            Assert.IsTrue(responseBody.Contains("cspStepChallenge"));
                        }
                        else
                        {
                            Assert.AreEqual("PaymentChallengePage", pidls[0].DisplayPages[0].DisplayName);
                            Assert.IsTrue(!responseBody.Contains("cspStepChallenge"));
                        }
                    }
                });
        }

        [DataRow(PXService.GlobalConstants.Partners.Webblends, true, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, true, false, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, false, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OXODIME, true, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OXODIME, true, false, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OXODIME, false, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OXODIME, false, false, "gb")]
        [TestMethod]
        public async Task GetChallengeDescription_PSD2Challenge_PXPSD2EnableCSPUrlProxyFrame_Fingerprint(string partnerName, bool sendCSPFlight, bool sendCSPSanitizedInputFlight, string country)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "GBP";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            PXSettings.PayerAuthService.ResetToDefaults();

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            if (sendCSPFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXPSD2EnableCSPUrlProxyFrame");
            }

            if (sendCSPSanitizedInputFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXPSD2EnableCSPUrlProxyFrameWithSanitizedInput");
            }

            testHeader["x-ms-test"] = "{\"scenarios\":\"px-service-psd2-e2e-emulator\",\"contact\":\"pidlsdk\"}";

            // Act
            await GetRequest(
                url,
                testHeader,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);

                    Assert.IsNotNull(pidls);

                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);

                    if (string.Equals("gb", country, StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.AreEqual("IFramePage", pidls[0].DisplayPages[0].DisplayName);
                        if (sendCSPSanitizedInputFlight)
                        {
                            Assert.IsTrue(responseBody.Contains("cspStepFingerprint"));
                            Assert.IsTrue(responseBody.Contains("sourceUrl\":\"https://pmservices.cp.microsoft.com/staticresourceservice/resources/threeDS/Prod/methodDataSanitizedInput.html"));
                            Assert.IsTrue(!responseBody.Contains("displayContent"));
                        }
                        else if (sendCSPFlight)
                        {
                            Assert.IsTrue(responseBody.Contains("cspStepFingerprint"));
                            Assert.IsTrue(responseBody.Contains("sourceUrl\":\"https://pmservices.cp.microsoft.com/staticresourceservice/resources/threeDS/Prod/methodData.html"));
                            Assert.IsTrue(!responseBody.Contains("displayContent"));
                        }
                        else
                        {
                            Assert.IsTrue(!responseBody.Contains("cspStepFingerprint"));
                        }
                    }
                });
        }

        [DataRow(PXService.GlobalConstants.Partners.Webblends, true, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, true, false, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false, false, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OXODIME, true, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OXODIME, true, false, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OXODIME, false, true, "gb")]
        [DataRow(PXService.GlobalConstants.Partners.OXODIME, false, false, "gb")]
        [TestMethod]
        public async Task GetChallengeDescription_PSD2Challenge_PXPSD2EnableCSPUrlProxyFrame_Challenge(string partnerName, bool sendCSPFlight, bool sendCSPSanitizedInputFlight, string country)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "GBP";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            PXSettings.PayerAuthService.ArrangeResponse(
               method: HttpMethod.Post,
               urlPattern: ".*/GetThreeDSMethodURL.*",
               statusCode: HttpStatusCode.OK,
               content: "{ \"three_ds_method_url\" : \"\", \"three_ds_server_trans_id\" : \"\" }");

            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string payAuthResp = "{\"is_acs_challenge_required\":\"true\",\"enrollment_status\":\"bypassed\",\"enrollment_type\":\"three_ds\",\"three_ds_server_transaction_id\":\"f35eadb1-6517-433d-af58-a28a0150a19d\",\"acs_url\":\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\",\"acs_signed_content\":\"{\\\"MD\\\":\\\"A4560000307199\\\",\\\"PaReq\\\":\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiZjM1ZWFkYjEtNjUxNy00MzNkLWFmNTgtYTI4YTAxNTBhMTlkIn0=\\\",\\\"TermUrl\\\":\\\"https://www.merchanturl.com/Response.jsp\\\"}\",\"authenticate_update_url\":\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10009BHVVL2e8a44f83-dc56-4689-8c5d-4a7fc003c8f1/authenticate\",\"transaction_challenge_status\":\"C\",\"is_form_post_acs_url\":true,\"is_full_page_redirect\":true}";
            string sessionResp = "{\"id\":\"" + paymentSessionId + "\",\"session_type\":\"any\",\"data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXPSD2Auth-_-_-Succeeded\\\",\\\"PXPSD2Auth-C-_-Unknown\\\",\\\"PXPSD2Auth-N-TSR10-Failed\\\",\\\"PXPSD2Auth-R-_-Failed\\\",\\\"PXPSD2Comp-_-_-_-Succeeded\\\",\\\"PXPSD2Comp-_-_-01-Cancelled\\\",\\\"PXPSD2Comp-N-_-04-TimedOut\\\",\\\"PXPSD2Comp-N-TSR10-_-Failed\\\",\\\"PXPSD2Comp-R-_-_-Failed\\\",\\\"PXPSD2SettingVersionV19\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"BrowserInfo\\\":{\\\"browser_accept_header\\\":\\\"*/*\\\",\\\"browser_ip\\\":\\\"::1\\\",\\\"browser_java_enabled\\\":true,\\\"browser_language\\\":\\\"en-US\\\",\\\"browser_color_depth\\\":\\\"8\\\",\\\"browser_screen_height\\\":\\\"480\\\",\\\"browser_screen_width\\\":\\\"640\\\",\\\"browser_tz\\\":\\\"480\\\",\\\"browser_user_agent\\\":\\\"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36\\\",\\\"challenge_window_size\\\":\\\"05\\\"},\\\"MethodData\\\":{\\\"three_ds_server_trans_id\\\":\\\"123\\\",\\\"three_ds_method_url\\\":\\\"https://testurl.com\\\"},\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"73f512ef-48fb-4322-b87d-ce6a3622ef1b\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiNzNmNTEyZWYtNDhmYi00MzIyLWI4N2QtY2U2YTM2MjJlZjFiIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":null,\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-US\\\",\\\"SuccessUrl\\\":null,\\\"FailureUrl\\\":null,\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"payment_session_id\\\":\\\"Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec\\\",\\\"account_id\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"9MEvEAAAAAAEAACA\\\",\\\"partner\\\":\\\"" + partnerName + "\\\",\\\"amount\\\":100.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"IN\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"visa\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null,\\\"cvv_token \\\":null}\",\"encrypt_data\":false,\"state\":\"INCOMPLETE\",\"test_context\":{\"scenarios\":\"px-service-psd2-e2e-emulator,px-service-3ds1-show-iframe\",\"contact\":\"kowshikp\",\"context_props\":{}}}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, sessionResp);
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            if (sendCSPFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXPSD2EnableCSPUrlProxyFrame");
            }

            if (sendCSPSanitizedInputFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXPSD2EnableCSPUrlProxyFrameWithSanitizedInput");
            }

            testHeader["x-ms-test"] = "{\"scenarios\":\"px-service-psd2-e2e-emulator\",\"contact\":\"pidlsdk\"}";

            // Act
            await GetRequest(
                url,
                testHeader,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);

                    Assert.IsNotNull(pidls);

                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);

                    if (string.Equals("gb", country, StringComparison.OrdinalIgnoreCase))
                    {
                        if (sendCSPSanitizedInputFlight)
                        {
                            Assert.IsTrue(responseBody.Contains("PaymentChallengePage"));
                            Assert.IsTrue(responseBody.Contains("sourceUrl\":\"https://pmservices.cp.microsoft.com/staticresourceservice/resources/threeDS/Prod/sessionDataSanitizedInput.html"));
                            Assert.IsTrue(!responseBody.Contains("displayContent"));
                        }
                        else if (sendCSPFlight)
                        {
                            Assert.IsTrue(responseBody.Contains("PaymentChallengePage"));
                            Assert.IsTrue(responseBody.Contains("sourceUrl\":\"https://pmservices.cp.microsoft.com/staticresourceservice/resources/threeDS/Prod/sessionData.html"));
                            Assert.IsTrue(!responseBody.Contains("displayContent"));
                        }
                        else
                        {
                            Assert.AreEqual("PaymentChallengePage", pidls[0].DisplayPages[0].DisplayName);
                            Assert.IsTrue(!responseBody.Contains("cspStepChallenge"));
                        }
                    }
                });
        }

        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "ValidatePIOnAttachChallenge", "PSD2Challenge", true)]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "PSD2Challenge", "ValidatePIOnAttachChallenge", true)]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "ValidatePIOnAttachChallenge", "ValidatePIOnAttachChallenge", true)]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "ValidatePIOnAttachChallenge", "PSD2Challenge")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "PSD2Challenge", "ValidatePIOnAttachChallenge")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "ValidatePIOnAttachChallenge", "ValidatePIOnAttachChallenge")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "ValidatePIOnAttachChallenge", "PSD2Challenge")]
        [DataRow(PXService.GlobalConstants.Partners.OXOOobe, "ValidatePIOnAttachChallenge", "PSD2Challenge")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "PSD2Challenge", "PSD2Challenge")]
        [DataRow(PXService.GlobalConstants.Partners.OXOOobe, "PSD2Challenge", "PSD2Challenge")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "ValidatePIOnAttachChallenge", "ValidatePIOnAttachChallenge")]
        [DataRow(PXService.GlobalConstants.Partners.OXOOobe, "ValidatePIOnAttachChallenge", "ValidatePIOnAttachChallenge")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "PSD2Challenge", "ValidatePIOnAttachChallenge")]
        [DataRow(PXService.GlobalConstants.Partners.OXOOobe, "PSD2Challenge", "ValidatePIOnAttachChallenge")]
        [TestMethod]
        public async Task GetChallengeDescription_PSD2Challenge_StoredSessionChallengeType(string partnerName, string challengeType, string storedChallengeType, bool isFeatureEnableValidatePIOnAttachChallenge = false)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "EUR";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: "./GetThreeDSMethodURL.*",
                statusCode: HttpStatusCode.OK,
                content: "{ \"three_ds_method_url\" : \"\", \"three_ds_server_trans_id\" : \"\" }");

            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string payAuthResp = "{\"is_acs_challenge_required\":\"true\",\"enrollment_status\":\"bypassed\",\"enrollment_type\":\"three_ds\",\"three_ds_server_transaction_id\":\"f35eadb1-6517-433d-af58-a28a0150a19d\",\"acs_url\":\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\",\"acs_signed_content\":\"{\\\"MD\\\":\\\"A4560000307199\\\",\\\"PaReq\\\":\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiZjM1ZWFkYjEtNjUxNy00MzNkLWFmNTgtYTI4YTAxNTBhMTlkIn0=\\\",\\\"TermUrl\\\":\\\"https://www.merchanturl.com/Response.jsp\\\"}\",\"authenticate_update_url\":\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10009BHVVL2e8a44f83-dc56-4689-8c5d-4a7fc003c8f1/authenticate\",\"transaction_challenge_status\":\"C\",\"is_form_post_acs_url\":true,\"is_full_page_redirect\":true}";
            string sessionResp = "{\"id\":\"" + paymentSessionId + "\",\"session_type\":\"any\",\"data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXEnableGettingStoredSessionForChallengeDescriptionsController\\\",\\\"PXPSD2Auth-_-_-Succeeded\\\",\\\"PXPSD2Auth-C-_-Unknown\\\",\\\"PXPSD2Auth-N-TSR10-Failed\\\",\\\"PXPSD2Auth-R-_-Failed\\\",\\\"PXPSD2Comp-_-_-_-Succeeded\\\",\\\"PXPSD2Comp-_-_-01-Cancelled\\\",\\\"PXPSD2Comp-N-_-04-TimedOut\\\",\\\"PXPSD2Comp-N-TSR10-_-Failed\\\",\\\"PXPSD2Comp-R-_-_-Failed\\\",\\\"PXPSD2SettingVersionV19\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"BrowserInfo\\\":{\\\"browser_accept_header\\\":\\\"*/*\\\",\\\"browser_ip\\\":\\\"::1\\\",\\\"browser_java_enabled\\\":true,\\\"browser_language\\\":\\\"en-US\\\",\\\"browser_color_depth\\\":\\\"8\\\",\\\"browser_screen_height\\\":\\\"480\\\",\\\"browser_screen_width\\\":\\\"640\\\",\\\"browser_tz\\\":\\\"480\\\",\\\"browser_user_agent\\\":\\\"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36\\\",\\\"challenge_window_size\\\":\\\"05\\\"},\\\"MethodData\\\":{\\\"three_ds_server_trans_id\\\":\\\"123\\\",\\\"three_ds_method_url\\\":\\\"https://testurl.com\\\"},\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"73f512ef-48fb-4322-b87d-ce6a3622ef1b\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiNzNmNTEyZWYtNDhmYi00MzIyLWI4N2QtY2U2YTM2MjJlZjFiIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":null,\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-US\\\",\\\"SuccessUrl\\\":null,\\\"FailureUrl\\\":null,\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"challengeType\\\":\\\"" + storedChallengeType + "\\\",\\\"payment_session_id\\\":\\\"Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec\\\",\\\"account_id\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"9MEvEAAAAAAEAACA\\\",\\\"partner\\\":\\\"" + partnerName + "\\\",\\\"amount\\\":100.0,\\\"currency\\\":\\\"EUR\\\",\\\"country\\\":\\\"DE\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"visa\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null,\\\"cvv_token \\\":null}\",\"encrypt_data\":false,\"state\":\"INCOMPLETE\",\"test_context\":{\"scenarios\":\"px-service-billdesk-provider,px-service-psd2-e2e-emulator,px-service-3ds1-show-iframe\",\"contact\":\"kowshikp\",\"context_props\":{}}}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, sessionResp);
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSession()
                                {
                                    Id = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6",
                                    IsChallengeRequired = true,
                                    ChallengeStatus = PaymentChallengeStatus.Unknown,
                                    Signature = "1234",
                                    ChallengeType = challengeType,
                                    Amount = 100,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = "de",
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            testHeader["x-ms-test"] = "{\"scenarios\":\"px-service-psd2-e2e-emulator\",\"contact\":\"pidlsdk\"}";
            List<string> testFlight = new List<string>() { "PXEnableGettingStoredSessionForChallengeDescriptionsController" };

            if (string.Equals(partnerName, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
            {
                string expectedPSSResponse = isFeatureEnableValidatePIOnAttachChallenge
                    ? "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"features\":{\"PXUsePSSToEnableValidatePIOnAttachChallenge\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}"
                    : "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // Act
            await GetRequest(
                url,
                testHeader,
                testFlight,
                (responseCode, responseBody, responseHeaders) =>
                {
                    if (storedChallengeType.Equals("PSD2Challenge") 
                    || (string.Equals(partnerName, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase) && !isFeatureEnableValidatePIOnAttachChallenge))
                    {
                        List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);

                        // Assert
                        Assert.IsNotNull(pidls);
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                        Assert.AreEqual("PaymentChallengePage", pidls[0].DisplayPages[0].DisplayName);
                    }
                    else
                    {
                        // Assert
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                        Assert.IsTrue(responseBody.Contains("ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6"));
                        Assert.IsTrue(responseBody.Contains("ValidatePIOnAttachChallenge"));
                    }
                });
        }

        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "ValidatePIOnAttachChallenge", true)]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "ValidatePIOnAttachChallenge")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "PSD2Challenge", true)]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "PSD2Challenge")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "ValidatePIOnAttachChallenge")]
        [DataRow(PXService.GlobalConstants.Partners.OXOOobe, "ValidatePIOnAttachChallenge")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "PSD2Challenge")]
        [DataRow(PXService.GlobalConstants.Partners.OXOOobe, "PSD2Challenge")]
        [TestMethod]
        public async Task GetChallengeDescription_PSD2Challenge_ReturnedPIDLEvenWithError(string partnerName, string challengeType, bool isFeatureEnableValidatePIOnAttachChallenge = false)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "EUR";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: "./GetThreeDSMethodURL.*",
                statusCode: HttpStatusCode.OK,
                content: "{ \"three_ds_method_url\" : \"\", \"three_ds_server_trans_id\" : \"\" }");

            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string payAuthResp = "{\"is_acs_challenge_required\":\"true\",\"enrollment_status\":\"bypassed\",\"enrollment_type\":\"three_ds\",\"three_ds_server_transaction_id\":\"f35eadb1-6517-433d-af58-a28a0150a19d\",\"acs_url\":\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\",\"acs_signed_content\":\"{\\\"MD\\\":\\\"A4560000307199\\\",\\\"PaReq\\\":\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiZjM1ZWFkYjEtNjUxNy00MzNkLWFmNTgtYTI4YTAxNTBhMTlkIn0=\\\",\\\"TermUrl\\\":\\\"https://www.merchanturl.com/Response.jsp\\\"}\",\"authenticate_update_url\":\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10009BHVVL2e8a44f83-dc56-4689-8c5d-4a7fc003c8f1/authenticate\",\"transaction_challenge_status\":\"C\",\"is_form_post_acs_url\":true,\"is_full_page_redirect\":true}";

            // Returning malformed session, causing error to be thrown when getting session from session service.
            string sessionResp = "{\"id\":\"" + paymentSessionId + "\",\"session_type\":\"any\",\"data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXEnableGettingStoredSessionForChallengeDescriptionsController\\\",\\\"PXPSD2Auth-_-_-Succeeded\\\",\\\"PXPSD2Auth-C-_-Unknown\\\",\\\"PXPSD2Auth-N-TSR10-Failed\\\",\\\"PXPSD2Auth-R-_-Failed\\\",\\\"PXPSD2Comp-_-_-_-Succeeded\\\",\\\"PXPSD2Comp-_-_-01-Cancelled\\\",\\\"PXPSD2Comp-N-_-04-TimedOut\\\",\\\"PXPSD2Comp-N-TSR10-_-Failed\\\",\\\"PXPSD2Comp-R-_-_-Failed\\\",\\\"PXPSD2SettingVersionV19\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"BrowserInfo\\\":{\\\"browser_accept_header\\\":\\\"*/*\\\",\\\"browser_ip\\\":\\\"::1\\\",\\\"browser_java_enabled\\\":true,\\\"browser_language\\\":\\\"en-US\\\",\\\"browser_color_depth\\\":\\\"8\\\",\\\"browser_screen_height\\\":\\\"480\\\",\\\"browser_screen_width\\\":\\\"640\\\",\\\"browser_tz\\\":\\\"480\\\",\\\"browser_user_agent\\\":\\\"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36\\\",\\\"challenge_window_size\\\":\\\"05\\\"},\\\"MethodData\\\":{\\\"three_ds_server_trans_id\\\":\\\"123\\\",\\\"three_ds_method_url\\\":\\\"https://testurl.com\\\"},\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"73f512ef-48fb-4322-b87d-ce6a3622ef1b\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiNzNmNTEyZWYtNDhmYi00MzIyLWI4N2QtY2U2YTM2MjJlZjFiIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":null,\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-US\\\",\\\"SuccessUrl\\\":null,\\\"FailureUrl\\\":null,\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"challengeType\\\":null,\\\"payment_session_id\\\":\\\"Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec\\\",\\\"account_id\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"9MEvEAAAAAAEAACA\\\",\\\"partner\\\":\\\"" + partnerName + "\\\",\\\"amount\\\":100.0,\\\"currency\\\":\\\"EUR\\\",\\\"country\\\":\\\"DE\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"visa\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null,\\\"cvv_token \\\":null}\",\"encrypt_data\":false,\"state\":\"INCOMPLETE\",\"test_context\":{\"scenarios\":\"px-service-billdesk-provider,px-service-psd2-e2e-emulator,px-service-3ds1-show-iframe\",\"contact\":\"kowshikp\",\"context_props\":{}}}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, sessionResp);
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSession()
                                {
                                    Id = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6",
                                    IsChallengeRequired = true,
                                    ChallengeStatus = PaymentChallengeStatus.Unknown,
                                    Signature = "1234",
                                    ChallengeType = challengeType,
                                    Amount = 100,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = "de",
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            testHeader["x-ms-test"] = "{\"scenarios\":\"px-service-psd2-e2e-emulator\",\"contact\":\"pidlsdk\"}";
            List<string> testFlight = new List<string>() { "PXEnableGettingStoredSessionForChallengeDescriptionsController" };

            if (string.Equals(partnerName, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
            {
                string expectedPSSResponse = isFeatureEnableValidatePIOnAttachChallenge 
                    ? "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"features\":{\"PXUsePSSToEnableValidatePIOnAttachChallenge\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}"
                    : "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // Act
            await GetRequest(
                url,
                testHeader,
                testFlight,
                (responseCode, responseBody, responseHeaders) =>
                {
                    if (challengeType.Equals("PSD2Challenge")
                    || (string.Equals(partnerName, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase) && !isFeatureEnableValidatePIOnAttachChallenge))
                    {
                        List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);

                        // Assert
                        Assert.IsNotNull(pidls);
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                        Assert.AreEqual("PaymentChallengePage", pidls[0].DisplayPages[0].DisplayName);
                    }
                    else
                    {
                        // Assert
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, "Expected response code to be OK");
                        Assert.IsTrue(responseBody.Contains("ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6"), "Response body does not contain the expected session ID");
                        Assert.IsTrue(responseBody.Contains("ValidatePIOnAttachChallenge"), "Response body does not contain the expected challenge type");
                    }
                });
        }

        [DataRow(PXService.GlobalConstants.Partners.Webblends, "Account001-Pi001-Visa", true, true, true)]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "Account001-Pi003-Amex", true, true, true)]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "Account001-Pi002-MC", true, true, true)]
        [DataRow(PXService.GlobalConstants.Partners.WebblendsInline, "Account001-Pi001-Visa", true, true, true)]
        [DataRow(PXService.GlobalConstants.Partners.WebblendsInline, "Account001-Pi003-Amex", true, true, true)]
        [DataRow(PXService.GlobalConstants.Partners.WebblendsInline, "Account001-Pi002-MC", true, true, true)]
        [DataRow(PXService.GlobalConstants.Partners.Cart, "Account001-Pi001-Visa", true, true, true)]
        [DataRow(PXService.GlobalConstants.Partners.Cart, "Account001-Pi003-Amex", true, true, true)]
        [DataRow(PXService.GlobalConstants.Partners.Cart, "Account001-Pi002-MC", true, true, true)]
        [DataRow(PXService.GlobalConstants.Partners.AmcWeb, "Account001-Pi001-Visa", true, true, true)]
        [DataRow(PXService.GlobalConstants.Partners.AmcWeb, "Account001-Pi003-Amex", true, true, true)]
        [DataRow(PXService.GlobalConstants.Partners.AmcWeb, "Account001-Pi002-MC", true, true, true)]
        [TestMethod]
        public async Task GetChallengeCardLogoDescription_India3DSChallenge_PartnerFlight(string partnerName, string piid, bool sendPartnerFlight, bool sendTestHeader, bool shouldThreeDSOneEnabled)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "INR";
            string country = "in";
            string language = "en-us";

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            if (sendPartnerFlight)
            {
                testHeader["x-ms-flight"] = "EnableThreeDSOne";
            }

            if (sendTestHeader)
            {
                testHeader["x-ms-test"] = "{\"scenarios\":\"px-service-psd2-e2e-emulator\",\"contact\":\"pidlsdk\"}";
            }

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piid);
            extendedPI.PaymentInstrumentDetails.RequiredChallenge = new List<string> { "3ds" };
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            // Act
            await GetRequest(
                url,
                testHeader,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);
                    Assert.IsNotNull(pidls);

                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);

                    if (shouldThreeDSOneEnabled)
                    {
                        Assert.AreEqual(pidls[0].DisplayPages[0].DisplayName, "authorizeCvvPage");
                    }

                    var cardLogo = pidls[0].GetDisplayHintById("challengeCardLogo");
                    Assert.IsNotNull(cardLogo);
                    Assert.IsNotNull(cardLogo.DisplayTags["accessibilityName"]);

                    switch (piid)
                    {
                        case "Account001-Pi001-Visa":
                            Assert.AreEqual(cardLogo.DisplayTags["accessibilityName"], "Visa");
                            break;
                        case "Account001-Pi003-Amex":
                            Assert.AreEqual(cardLogo.DisplayTags["accessibilityName"], "American Express");
                            break;
                        case "Account001-Pi002-MC":
                            Assert.AreEqual(cardLogo.DisplayTags["accessibilityName"], "MasterCard");
                            break;
                    }
                });
        }

        [DataRow(PXService.GlobalConstants.Partners.Webblends, "Account001", false)]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "4089c4a0-6cb6-4bad-8ca1-a30f47b28365", true)]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "1b11f28d-2a22-4c04-aa0d-ac005cb16926", true)]
        [TestMethod]
        public async Task GetChallengeDescription_India3DSChallenge_EnableTestAccounts(string partnerName, string accountId, bool shouldThreeDSOneEnabled)
        {
            // Arrange
            string currency = "INR";
            string country = "in";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi001-Visa");
            extendedPI.PaymentInstrumentDetails.RequiredChallenge = new List<string> { "3ds" };
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            // Act
            await GetRequest(
                url,
                testHeader,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);

                    Assert.IsNotNull(pidls);

                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    if (shouldThreeDSOneEnabled)
                    {
                        Assert.IsTrue(responseHeaders.GetValues("x-ms-flight").First().IndexOf("PXEnableIndia3DS1Challenge") > 0);
                        Assert.AreEqual(pidls[0].DisplayPages[0].DisplayName, "authorizeCvvPage");
                    }
                    else
                    {
                        Assert.IsTrue(responseHeaders.GetValues("x-ms-flight").First().IndexOf("PXEnableIndia3DS1Challenge") == -1);
                        var session = JsonConvert.DeserializeObject<PaymentSession>(pidls[0].ClientAction.Context.ToString());
                        Assert.AreEqual(session.ChallengeStatus.ToString(), "NotApplicable");
                    }
                });
        }

        [DataRow(PXService.GlobalConstants.Partners.Webblends, "Account001-Pi003-Amex", "in", false)]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "Account001-Pi001-Visa", "in", true)]
        [TestMethod]
        public async Task GetChallengeDescription_India3DSChallenge_DisableChallengeForAmex(string partnerName, string piid, string country, bool shouldEnableChallenge)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "INR";
            string language = "en-us";

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            testHeader["x-ms-flight"] = "EnableThreeDSOne,India3dsEnableForBilldesk";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piid);
            extendedPI.PaymentInstrumentDetails.RequiredChallenge = new List<string> { "3ds" };
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            // Act
            await GetRequest(
                url,
                testHeader,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);

                    Assert.IsNotNull(pidls);

                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);

                    if (shouldEnableChallenge)
                    {
                        Assert.AreEqual(pidls[0].DisplayPages[0].DisplayName, "authorizeCvvPage");
                    }
                    else
                    {
                        var session = JsonConvert.DeserializeObject<PaymentSession>(pidls[0].ClientAction.Context.ToString());
                        Assert.AreEqual(session.ChallengeStatus.ToString(), "NotApplicable");
                    }
                });
        }

        [DataRow(PXService.GlobalConstants.Partners.Webblends, "IN", "INR", true)]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "IN", "SGD", false)]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "SG", "SGD", false)]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "SG", "INR", false)]
        [TestMethod]
        public async Task GetChallengeDescription_India3DSChallenge_DisableChallengeForNonIndiaMarket(string partnerName, string country, string currency, bool shouldEnableChallenge)
        {
            // Arrange
            string accountId = "Account001";
            string language = "en-us";

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = "Account001-Pi001-Visa",
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            testHeader["x-ms-flight"] = "EnableThreeDSOne,India3dsEnableForBilldesk";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, "Account001-Pi001-Visa");
            extendedPI.PaymentInstrumentDetails.RequiredChallenge = new List<string> { "3ds" };
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            // Act
            await GetRequest(
                url,
                testHeader,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);

                    Assert.IsNotNull(pidls);

                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);

                    if (shouldEnableChallenge)
                    {
                        Assert.AreEqual(pidls[0].DisplayPages[0].DisplayName, "authorizeCvvPage");
                    }
                    else
                    {
                        var session = JsonConvert.DeserializeObject<PaymentSession>(pidls[0].ClientAction.Context.ToString());
                        Assert.AreEqual(session.ChallengeStatus.ToString(), "NotApplicable");
                    }
                });
        }

        [DataRow(PXService.GlobalConstants.Partners.Azure, 0, ChallengeScenario.PaymentTransaction, false)]
        [DataRow(PXService.GlobalConstants.Partners.Azure, 100, ChallengeScenario.PaymentTransaction, true)]
        [DataRow(PXService.GlobalConstants.Partners.Azure, 0, ChallengeScenario.RecurringTransaction, false)]
        [DataRow(PXService.GlobalConstants.Partners.Azure, 100, ChallengeScenario.RecurringTransaction, false)]
        [TestMethod]
        public async Task GetChallengeDescription_NoChallenge_Zero_Price_CommercialPartners(string partnerName, int price, ChallengeScenario challengeScenario, bool shouldChallengeBeShown)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "INR";
            string country = "in";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = price,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = challengeScenario
                                })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            testHeader["x-ms-test"] = "{\"scenarios\":\"px-service-psd2-e2e-emulator\",\"contact\":\"pidlsdk\"}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, "Account001-Pi001-Visa");
            extendedPI.PaymentInstrumentDetails.RequiredChallenge = new List<string> { "3ds" };
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            // Act
            await GetRequest(
                url,
                testHeader,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);

                    Assert.IsNotNull(pidls);

                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);

                    if (!shouldChallengeBeShown)
                    {
                        var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidls[0].ClientAction.Context.ToString());
                        Assert.IsTrue(!paymentSession.IsChallengeRequired);
                    }
                    else
                    {
                        Assert.AreEqual(pidls[0].DisplayPages[0].DisplayName, "CvvChallengePage");
                    }
                });
        }

        [TestMethod]
        [DataRow("", "")]
        [DataRow("zh-gu-CN,zh-Hans-CN", "zh-gu-CN")]
        [DataRow("zh-gu-CN", "zh-gu-CN")]
        [DataRow("de-valencia", "de")]
        [DataRow("de-valencia-GB", "de")]
        [DataRow("en-US", "en-US")]
        [DataRow("en-GB,en-US", "en-GB")]
        [DataRow("zh-Hans-CN", "zh-Hans")]
        [DataRow("de-AT,de-DE,en-GB", "de-AT")]
        [DataRow("en-US,sk", "en-US")]
        [DataRow("en,de,bg", "en")]
        public void GetChallengeDescription_CheckTruncateLanguage(string browserLanguageInput, string expectedTruncatedBrowserLanguage)
        {
            string truncatedBrowserLanguage = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.BrowserInfo.TruncateLanguage(browserLanguageInput);
            Assert.AreEqual(expectedTruncatedBrowserLanguage, truncatedBrowserLanguage, false, "The browser language input is not being truncated properly to a browserLanguage under 8 characters");
        }

        [DataRow(PXService.GlobalConstants.Partners.Azure, "AadAccount001", "PuidAccount001", true)]
        [DataRow(PXService.GlobalConstants.Partners.Azure, "PuidAccount001", "PuidAccount001", false)]
        [DataRow(PXService.GlobalConstants.Partners.CommercialStores, "AadAccount001", "PuidAccount001", true)]
        [DataRow(PXService.GlobalConstants.Partners.CommercialStores, "PuidAccount001", "PuidAccount001", false)]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "AadAccount001", "PuidAccount001", true)]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "PuidAccount001", "PuidAccount001", false)]
        [TestMethod]
        public async Task GetChallengeDescription_Ownershipcheck_CommercialPartners(string partnerName, string tokenCid, string piCid, bool pass)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "INR";
            string country = "in";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        tokenCid,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentAccountId = piCid,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            testHeader["x-ms-test"] = "{\"scenarios\":\"px-service-psd2-e2e-emulator\",\"contact\":\"pidlsdk\"}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, "Account001-Pi001-Visa");
            extendedPI.PaymentInstrumentDetails.RequiredChallenge = new List<string> { "3ds" };
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, HttpMethod.Get, ".*/v4.0//paymentInstruments/Account001-Pi001-Visa/extendedView$");

            if (!pass)
            {
                string responseStr = "{\"ErrorCode\": \"AccountPINotFound\",\"Message\": \"The account and payment instrument pair can not be found.\"}";
                PXSettings.PimsService.ArrangeResponse(responseStr, HttpStatusCode.NotFound, HttpMethod.Get, ".*/paymentInstruments/Account001-Pi001-Visa$");
            }
            else
            {
                PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, HttpMethod.Get, ".*/paymentInstruments/Account001-Pi001-Visa$");
            }

            // Act
            if (pass)
            {
                await GetPidlFromPXService(url, HttpStatusCode.OK, null, testHeader);
            }
            else
            {
                await GetPidlFromPXService(url, HttpStatusCode.BadRequest, null, testHeader);
            }
        }

        /// <summary>
        /// This test is used to verify the Handle payment challenge for the legacy billdesk payment.
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="redirectionExpected"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow("azure", false)]
        [DataRow("officesmb", false)]
        [DataRow("officesmb", true)]
        [DataRow("northstarweb", false)]
        [DataRow("northstarweb", true)]
        [DataRow("bing", true)]
        [DataRow("commercialstores", true)]
        public async Task GetChallengeDescription_LegacyBillDeskPaymentChallenge(string partner, bool redirectionExpected)
        {
            // Arrange
            PXSettings.PayerAuthService.ResponseProvider.SessionId = "Z" + Guid.NewGuid().ToString();
            PXSettings.PayerAuthService.ResponseProvider.ThreeDSMethodUrl = null;
            PXSettings.PayerAuthService.ResponseProvider.ThreeDSServerTransId = Guid.NewGuid().ToString();
            PXSettings.PayerAuthService.ResponseProvider.EnrollmentStatus = "enrolled";
            PXSettings.PayerAuthService.ResponseProvider.AcsUrl = "https://paymentsredirectionservice.cp.microsoft-int.com/RedirectionService/CoreRedirection/Redirect/279dd64f-1bf2-4715-a1a3-436f62d9e27e";
            PXSettings.PayerAuthService.ResponseProvider.AcsTransId = Guid.NewGuid().ToString();
            PXSettings.PayerAuthService.ResponseProvider.TransStatus = "C";
            PXSettings.PayerAuthService.ResponseProvider.TransStatusReason = "TSR01";

            PXSettings.PimsService.ResponseProvider.RequiredChallenges = new List<string>() { "3ds2" };

            string url = string.Format(
                "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                "Account001",
                HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100,
                                    Currency = "INR",
                                    Partner = partner,
                                    Country = "IN",
                                    PaymentInstrumentAccountId = "Account001",
                                    PaymentInstrumentId = "Account001-Pi011-LegacyBillDeskPayment",
                                    Language = "en-in",
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            var testHeader = new Dictionary<string, string>();
            var redirectionPattern = redirectionExpected == true ? "Account001-PI001-InlineRedirectionDefaultTemplate" : "Account001-PI001-fullPageRedirectionDefaultTemplate";

            if (string.Equals(partner, PXService.GlobalConstants.Partners.OfficeSMB) || string.Equals(partner, PXService.GlobalConstants.Partners.NorthstarWeb))
            {
                string expectedPSSResponse = PartnerSettingsServiceMockResponseProvider.GetPSSMockResponseById(redirectionPattern).ToString();
                testHeader = new Dictionary<string, string>()
                {
                    { "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache" }
                };
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // Act
            await GetRequest(
                url,
                testHeader,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);

                    var pidls = ReadPidlResourceFromJson(responseBody);

                    if (redirectionExpected)
                    {
                        Assert.AreEqual(pidls[0].ClientAction.ActionType, ClientActionType.Redirect);
                        var redirectionUrl = JObject.Parse(pidls[0].ClientAction.Context.ToString()).SelectToken("baseUrl").ToString();
                        Assert.IsTrue(redirectionUrl.StartsWith("https://paymentsredirectionservice.cp.microsoft-int.com/RedirectionService/CoreRedirection/Redirect/"), "For the commercialstores, officesmb, and northstarweb partners, when the redirectionExpected property is set to true, there is an automatic redirect to the RDS URL.");
                    }
                    else
                    {
                        var statusCheckUrl = JObject.Parse(((pidls[0].ClientAction.Context as List<PIDLResource>)[0].DisplayPages[1].Members[0] as GroupDisplayHint).Members[0].Action.Context.ToString()).SelectToken("href").ToString();
                        Assert.IsFalse(statusCheckUrl.Contains("scenario=pollingAction"), "The URL to check the status when user clicks on button should not contain pollingaction in the scenario.");
                        Assert.IsTrue(statusCheckUrl.Contains($"partner={partner}"), "The URL is expecting the same partner name");

                        var pollingUrl = JObject.Parse((pidls[0].ClientAction.Context as List<PIDLResource>)[0].DisplayPages[0].Action.Context.ToString()).SelectToken("href").ToString();
                        Assert.IsTrue(pollingUrl.Contains("scenario=pollingAction"), "The polling URL to check the status of RDS Session should contain pollingaction in the scenario.");
                        Assert.IsTrue(pollingUrl.Contains($"partner={partner}"), "The URL is expecting the same partner name");
                    }
                });

            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PimsService.ResetToDefaults();
        }

        [TestMethod]
        [DataRow("azure")]
        [DataRow("commercialstores")]
        public async Task IndiaThreeDSURL_Failure_Test(string partner)
        {
            string transactionValidateErrorAccountId = "TransactionValidateErrorAccountId";
            string paymentSessionId = "Z10064BRGXCP2021e492-a753-40da-aac4-b21460956e8c";

            string paymentSessionResponse = "{\"Id\":\"" + paymentSessionId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"Account001\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + paymentSessionId + "\\\",\\\"account_id\\\":\\\"" + transactionValidateErrorAccountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"Account001-Pi001-Visa\\\",\\\"partner\\\":\\\"" + partner + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, paymentSessionResponse);

            PIDLData sampleData = new PIDLData();
            sampleData.Add("cvvToken", "123");

            var result = await PXClient.PostAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/AuthenticateIndiaThreeDS", transactionValidateErrorAccountId, paymentSessionId)),
                new StringContent(JsonConvert.SerializeObject(sampleData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            if (result.Content != null)
            {
                var resultContent = await result.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(resultContent);
                Assert.AreEqual("Please enter a valid DEBIT card number.", jsonObj["clientAction"]["context"]["InnerError"]["Message"].ToString());
                Assert.AreEqual(ClientActionType.Failure.ToString(), jsonObj["clientAction"]["type"].ToString());
            }
        }

        [DataRow("xboxsettings", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us", false)]
        [DataRow("storify", "Account001", "cvv", "Account001-Pi001-Visa", "123456", "us", false)]
        [DataRow("xboxsettings", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "gb", false)]
        [DataRow("storify", "Account001", "cvv", "Account001-Pi001-Visa", "123456", "gb", false)]
        [DataRow("storify", "Account001", "cvv", "Account001-Pi001-Visa", "123456", "us", true)]
        [TestMethod]
        public async Task XboxNativeChallengeDescription_AccessbilityLabelOrdering(string partner, string accountId, string type, string piid, string sessionId, string country, bool useStyleHints)
        {
            // Arrange
            string url = $"/v7.0/{accountId}/challengeDescriptions?language=en-us&country={country}&partner={partner}&type={type}&piid={piid}&sessionId={sessionId}";
            if (useStyleHints)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableXboxNativeStyleHints");
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.AreEqual(1, pidls.Count, "The count of PIDLs is not as expected");
            Assert.AreEqual(type, pidls[0].Identity[Constants.DescriptionIdentityFields.Type], "The type of the PIDL is not as expected");
            Assert.AreEqual("challenge", pidls[0].Identity[Constants.DescriptionIdentityFields.DescriptionType], "The description type of the PIDL is not as expected");

            if (type == "sms")
            {
                var page1ButtonGroup = pidls[0].DisplayPages[0].Members.Last() as ContainerDisplayHint;
                var page2ButtonGroup = pidls[0].DisplayPages[1].Members.Last() as ContainerDisplayHint;
                Assert.AreEqual(page1ButtonGroup.Members.Last().HintId, "cancelOkActionGroup", "Incorrect button group from page 1");
                Assert.AreEqual(page2ButtonGroup.Members.Last().HintId, "footerGroup", "Incorrect button group from page 2");
            }
            else if (type == "cvv")
            {
                var page1ButtonGroup = pidls[0].DisplayPages[0].Members.Last() as ContainerDisplayHint;
                var paymentFooterGroupDisplayHint = page1ButtonGroup.Members.Last() as GroupDisplayHint;
                Assert.AreEqual(paymentFooterGroupDisplayHint.Members.Last().HintId, "submitBackGroup", "Incorrect button group from page 1");

                GroupDisplayHint cvvChallengeTopGroup = pidls[0].GetDisplayHintById("cvvChallengeTopGroup") as GroupDisplayHint;
                if (useStyleHints)
                {
                    GroupDisplayHint challengeCvvGroup = cvvChallengeTopGroup.Members[2] as GroupDisplayHint;
                    Assert.IsTrue(challengeCvvGroup.HintId == "challengeCvvGroup");
                    Assert.IsTrue(challengeCvvGroup.StyleHints.SequenceEqual(new List<string> { "width-small" }));
                    Assert.IsTrue(challengeCvvGroup.Members.Count == 1);
                    Assert.IsTrue(challengeCvvGroup.Members[0].HintId == "cvv");
                }
                else
                {
                    Assert.IsTrue(cvvChallengeTopGroup.Members[2].HintId == "cvv");
                }
            }
        }

        [DataRow("xboxsettings", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us")]
        [DataRow("storify", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us")]
        [DataRow("oxodime", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us")]
        [DataRow("oxooobe", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us")]
        [DataRow("oxowebdirect", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us")]
        [DataRow("amcweb", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us")]
        [TestMethod]
        public async Task SmsValidation_ValidatePIFDEndpoint(string partner, string accountId, string type, string piid, string sessionId, string country)
        {
            // Arrange
            string url = $"/v7.0/{accountId}/challengeDescriptions?language=en-us&country={country}&partner={partner}&type={type}&piid={piid}&sessionId={sessionId}";
            PXFlightHandler.AddToEnabledFlights("PXEnableSMSChallengeValidation");

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.AreEqual(1, pidls.Count, "The count of PIDLs is not as expected");

            Assert.IsNotNull(pidls[0].GetDisplayHintById("okButton"));
            var okButton = pidls[0].GetDisplayHintById("okButton") as ButtonDisplayHint;
            var okButtonContext = okButton.Action.Context;
            Assert.IsNotNull(okButtonContext);

            Assert.IsNotNull(pidls[0].PidlResourceStrings.ServerErrorCodes);
            foreach (var serverErrors in pidls[0].PidlResourceStrings.ServerErrorCodes)
            {
                if (serverErrors.Key == "InvalidChallengeCode")
                {
                    Assert.AreEqual("InvalidChallengeCode", serverErrors.Key);
                    Assert.IsNotNull(serverErrors.Value);
                    ServerErrorCode serverErrorCodeValue = serverErrors.Value as ServerErrorCode;
                    Assert.AreEqual("otp", serverErrorCodeValue.Target);
                }
            }

            Assert.IsTrue(okButtonContext.ToString().Contains("challenge/Sms/validate"));
        }

        [DataRow("amcweb", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us")]
        [TestMethod]
        public async Task GetChallengeDescriptions_PurchaseRiskSmsChallenge(string partner, string accountId, string type, string piid, string sessionId, string country)
        {
            string url = $"/v7.0/{accountId}/challengeDescriptions?language=en-us&country={country}&partner={partner}&type={type}&piid={piid}&sessionId={sessionId}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.AreEqual(1, pidls.Count, "This count of PIDLs is not as expected");
            Assert.AreEqual(2, pidls[0].DisplayPages.Count, "The count of display pages in PIDL is not as expected");

            ButtonDisplayHint okActionButton = pidls[0].GetDisplayHintById("okActionButton") as ButtonDisplayHint;
            HyperlinkDisplayHint smsNewCodeLink = pidls[0].GetDisplayHintById("smsNewCodeLink") as HyperlinkDisplayHint;
            var okActionButtonContext = okActionButton.Action.Context;
            var smsNewCodeLinkContext = smsNewCodeLink.Action.Context;
            
            Assert.IsTrue(okActionButton.Action.ActionType.ToLower() == "restaction");
            Assert.IsTrue(okActionButtonContext.ToString().Contains("https://{pifd-endpoint}/users/{userId}/challenge/sms"));
            Assert.IsTrue(smsNewCodeLink.Action.ActionType.ToLower() == "restaction");
            Assert.IsTrue(smsNewCodeLinkContext.ToString().Contains("https://{pifd-endpoint}/users/{userId}/challenge/sms"));
        }

        [DataRow("xboxsettings", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us", "otp")]
        [DataRow("saturn", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us", "otp")]
        [DataRow("xboxsubs", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us", "otp")]
        [DataRow("storify", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us", "otp")]
        [DataRow("storify", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "gb", "otp")]
        [DataRow("oxodime", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us", "otp")]
        [DataRow("oxooobe", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us", "otp")]
        [DataRow("oxowebdirect", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "gb", "otp")]
        [DataRow("amcweb", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "gb", "otp")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_XboxNative_SMSChallenge(string partner, string accountId, string type, string piid, string sessionId, string country, string key)
        {
            // Arrange
            string url = $"/v7.0/{accountId}/challengeDescriptions?language=en-us&country={country}&partner={partner}&type={type}&piid={piid}&sessionId={sessionId}";
            PXFlightHandler.AddToEnabledFlights("PXEnableSMSChallengeValidation");

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);
            Assert.IsNotNull(pidls);

            // Assert
            // Make sure the property name in the displaypages matches the data source
            var otpDataDescription = pidls[0].DataDescription.ContainsKey(key);
            Assert.IsTrue(otpDataDescription);
            var smsChallengeCodeOtp = pidls[0].GetDisplayHintById("smsChallengeCodeOtp");
            Assert.IsNotNull(smsChallengeCodeOtp);
            Assert.AreEqual(key, smsChallengeCodeOtp.PropertyName);
        }

        [DataRow("storify", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", null, "us")]
        [DataRow("storify", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us")]
        [DataRow("xboxsettings", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", null, "us")]
        [DataRow("xboxsettings", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", "123456", "us")]
        [DataRow("storify", "Account001", "cvv", "Account001-Pi001-Visa", null, "us")]
        [DataRow("storify", "Account001", "cvv", "Account001-Pi001-Visa", "123456", "us")]
        [DataRow("xboxsettings", "Account001", "cvv", "Account001-Pi001-Visa", null, "us")]
        [DataRow("xboxsettings", "Account001", "cvv", "Account001-Pi001-Visa", "123456", "us")]
        [TestMethod]
        public async Task XboxNativeChallengeDescription_AccessibilityLabelCheck(string partner, string accountId, string type, string piid, string sessionId, string country)
        {
            string url = $"/v7.0/{accountId}/challengeDescriptions?language=en-us&country={country}&partner={partner}&type={type}&piid={piid}&operation=RenderPidlPage";
            string flight = "XboxUpdateAccessibilityNameWithPosition";
            PXFlightHandler.AddToEnabledFlights("XboxUpdateAccessibilityNameWithPosition");
            PXFlightHandler.AddToEnabledFlights("PXEnableSMSChallengeValidation");

            if (sessionId != null)
            {
                url += "&sessionId=" + sessionId;
            }

            List<PIDLResource> pidls = await GetPidlFromPXServiceWithPartnerHeader(url, "x-ms-flight", flight, flight);

            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            if (type == "sms")
            {
                GroupDisplayHint smsChallengeExplanationWrapperGroup = pidls[0].DisplayPages[0].Members[0] as GroupDisplayHint;
                GroupDisplayHint cancelOkGroup = smsChallengeExplanationWrapperGroup.Members.Last() as GroupDisplayHint;
                ButtonDisplayHint cancelButton = cancelOkGroup.Members[0] as ButtonDisplayHint;
                ButtonDisplayHint okButton = cancelOkGroup.Members[1] as ButtonDisplayHint;
                string cancelButtonAccessibilityName = "Cancel button 1 of 2";
                string okButtonAccessibilityName = "OK button 2 of 2";

                Assert.AreEqual(cancelButtonAccessibilityName, cancelButton.DisplayTags["accessibilityName"], $"accessibility name should be {cancelButtonAccessibilityName}");
                Assert.AreEqual(okButtonAccessibilityName, okButton.DisplayTags["accessibilityName"], $"accessibility name should be {okButtonAccessibilityName}");
            }
            else
            {
                GroupDisplayHint purchaseChallengeCvvPageWrapperGroup = pidls[0].DisplayPages[0].Members[0] as GroupDisplayHint;
                GroupDisplayHint paymentChallengeFooterGroup = purchaseChallengeCvvPageWrapperGroup.Members.Last() as GroupDisplayHint;
                GroupDisplayHint submitBackGroup = paymentChallengeFooterGroup.Members.Last() as GroupDisplayHint;
                ButtonDisplayHint submitButton = submitBackGroup.Members[0] as ButtonDisplayHint;
                ButtonDisplayHint backButton = submitBackGroup.Members[1] as ButtonDisplayHint;
                string submitButtonAccessibilityName = "Submit button 1 of 2";
                string backButtonAccessibilityName = "Cancel button 2 of 2";

                Assert.AreEqual(submitButtonAccessibilityName, submitButton.DisplayTags["accessibilityName"], $"accessibility name should be {submitButtonAccessibilityName}");
                Assert.AreEqual(backButtonAccessibilityName, backButton.DisplayTags["accessibilityName"], $"accessibility name should be {backButtonAccessibilityName}");
            }
        }

        /// <summary>
        /// This test is to verify the India 3DS challenge flow for PSS.
        /// The feature cvv3DSSubmitButtonDisplayContentAsNext when enabled it will display the submit button as next
        /// and when the feature submitURLToEmptyForIndia3dsChallenge is enabled it will set the submit URL to empty.
        /// </summary>
        /// <param name="partnerName">Partner name</param>
        /// <param name="flightOverrides">Check the flighting</param>
        /// <param name="partnerType">Define the partner type conusmer or commercial</param>
        /// <param name="setButtonHintIdNameForFeature">Stores the HintId value for the submit button which is required to change for the feature.</param>
        /// <param name="setButtonRequireUpdateNameForFeature">Stores the new display text name for the feature which needs to be updated for the setButtonHintIdNameForFeature hintId.</param>
        /// <returns></returns>
        [DataRow(PXService.GlobalConstants.Partners.Cart, "", "consumer")]
        [DataRow(PXService.GlobalConstants.Partners.WebblendsInline, "", "consumer")]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, "PXEnableIndia3DS1Challenge", "consumer")]
        [DataRow(PXService.GlobalConstants.Partners.Azure, "", "commercial")]
        [DataRow(PXService.GlobalConstants.Partners.Bing, "", "commercial")]
        [DataRow(PXService.GlobalConstants.Partners.CommercialStores, "", "commercial")]
        [DataRow(PXService.GlobalConstants.Partners.AmcWeb, "PXEnableIndia3DS1Challenge", "consumer")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "", "consumer")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "", "commercial")]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "", "consumer", Constants.DisplayHintIds.Cvv3DSSubmitButton, Constants.DisplayContent.NextDisplayContentText)]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, "", "commercial", Constants.DisplayHintIds.Cvv3DSSubmitButton, Constants.DisplayContent.NextDisplayContentText)]
        [TestMethod]
        public async Task GetChallengeDescription_India3DSChallenge_PSS(string partnerName, string flightOverrides, string partnerType, string setButtonHintIdNameForFeature = null, string setButtonRequireUpdateNameForFeature = null)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "INR";
            string country = "in";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";
            string featureStatusBasedOnPartnerType = null;

            if (string.Equals(partnerType, Constants.PartnerType.Commercial, StringComparison.OrdinalIgnoreCase))
            {
                featureStatusBasedOnPartnerType = "\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":[{\"enableIndia3dsForNonZeroPaymentTransaction\":true}]}";
            }
            else if (string.Equals(partnerType, Constants.PartnerType.Consumer, StringComparison.OrdinalIgnoreCase))
            {
                featureStatusBasedOnPartnerType = "\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":[{\"pxEnableIndia3DS1Challenge\":true,\"india3dsEnableForBilldesk\":true}]}";
            }

            string feature = null;
            feature = $"\"customizeDisplayContent\":{{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":[{{\"setButtonDisplayContent\":{{\"{setButtonHintIdNameForFeature}\":\"{setButtonRequireUpdateNameForFeature}\"}}}}]}}";

            string expectedPSSResponse = $"{{\"handlepaymentchallenge\":{{\"template\":\"defaultTemplate\",\"features\":{{{featureStatusBasedOnPartnerType},{feature}}}}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            var testHeader = new Dictionary<string, string>()
            {
                { "x-ms-test", "{\"scenarios\": \"px-service-3ds1-test-emulator\"}" },
                { "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache" }
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, flightOverrides, testHeader);

            foreach (PIDLResource resource in pidls)
            {
                // Assert
                var submitDisplayHint = resource.GetDisplayHintById(Constants.DisplayHintIds.Cvv3DSSubmitButton) as ButtonDisplayHint;
                if (string.Equals(partnerType, "commercial"))
                {
                    var textDisplayHint = resource.GetDisplayHintById("challengeCardNumber") as TextDisplayHint;
                    Assert.IsNotNull(textDisplayHint);
                    Assert.IsTrue(submitDisplayHint.Action.Context.ToString().Contains("/authenticateIndiaThreeDS"));
                }
                else if (string.Equals(partnerType, "consumer"))
                {
                    var expiry = resource.GetDisplayHintById("challengeCardExpiry") as TextDisplayHint;
                    Assert.IsNotNull(expiry);

                    var name = resource.GetDisplayHintById("challengeCardName") as TextDisplayHint;
                    Assert.IsNotNull(name);
                    Assert.IsTrue(submitDisplayHint.Action.Context.ToString().Contains("/browserAuthenticateThreeDSOne"));
                }

                Assert.AreEqual(setButtonHintIdNameForFeature != null ? Constants.DisplayContent.NextDisplayContentText : Constants.DisplayContent.SubmitDisplayContentText, submitDisplayHint.DisplayText());
            }
        }

        /// <summary>
        /// This test is used to validate the India 3DS challenge flow for PSS in handling the "handlePurchaseRiskChallenge" for consumer partners.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetChallengeDescription_CvvsmsChallenge_PSSForHandlePurchaseRisk()
        {
            // Arrange
            List<string> types = new List<string>() { "sms", "cvv" };
            string settingJson = "{\"handlePurchaseRiskChallenge\":{\"template\":\"defaultTemplate\",\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":[{\"pxEnableIndia3DS1Challenge\":true,\"india3dsEnableForBilldesk\":true}]}}}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(settingJson);

            foreach (string type in types)
            {
                string url = type == "sms" ? $"/v7.0/NonSimMobiAccount/challengeDescriptions?type={type}&sessionId=123456&piid=NonSimMobiAccount-Pi001-NonSimMobi&partner=officesmb&country=in&language=en-us" : $"/v7.0/Account001/challengeDescriptions?type={type}&piid=Account001-Pi001-Visa&partner=officesmb&country=in&language=en-us";

                var testHeader = new Dictionary<string, string>()
                {
                    { "x-ms-test", "{\"scenarios\": \"px-service-3ds1-test-emulator\"}" },
                    { "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache" }
                };

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, additionaHeaders: testHeader);

                foreach (PIDLResource resource in pidls)
                {
                    // Assert
                    if (string.Equals(type, "cvv"))
                    {
                        var submitDisplayHint = resource.GetDisplayHintById("cvv3DSSubmitButton") as ButtonDisplayHint;
                        var expiry = resource.GetDisplayHintById("challengeCardNumber") as TextDisplayHint;
                        var cvvVerification = resource.GetDisplayHintById("challengeCardVerificationText") as TextDisplayHint;
                        Assert.IsNotNull(expiry);
                        Assert.IsNotNull(cvvVerification);
                        Assert.IsNotNull(submitDisplayHint.Action.Context);
                    }
                    else
                    {
                        var submitDisplayHint = resource.GetDisplayHintById("okActionButton") as ButtonDisplayHint;
                        var purchaseChallengeSmsMessageText = resource.GetDisplayHintById("smsMessageText") as TextDisplayHint;
                        Assert.IsNotNull(purchaseChallengeSmsMessageText);
                        var purchaseChallengeSmsExplanationText = resource.GetDisplayHintById("smsExplanationText") as TextDisplayHint;
                        Assert.IsNotNull(purchaseChallengeSmsExplanationText);
                        Assert.IsNotNull(submitDisplayHint.Action.Context);
                    }
                }
            }
        }

        /// <summary>
        /// This CIT is used to validate the "handlePurchaseRiskChallenge" for type cvv and validate the action button display content as "OK" or "Submit" based on the feature status for PSS partners and "Ok" for non-template partners.
        /// </summary>
        /// <param name="partner">Partner name</param>
        /// <param name="setButtonHintIdNameForFeature">Stores the HintId value for the submit button which is required to change for the feature.</param>
        /// <param name="setButtonRequireUpdateNameForFeature">Stores the new display text name for the feature which needs to be updated for the setButtonHintIdNameForFeature hintId.</param>
        /// <returns></returns>
        [DataRow(Constants.VirtualPartnerNames.Macmanage, Constants.DisplayHintIds.Cvv3DSSubmitButton, Constants.DisplayContent.OkDisplayContentText)]
        [DataRow(Constants.VirtualPartnerNames.Macmanage, null, null)]
        [DataRow(Constants.VirtualPartnerNames.Macmanage, Constants.DisplayHintIds.Cvv3DSSubmitButton, null)]
        [DataRow(Constants.VirtualPartnerNames.Macmanage, null, Constants.DisplayContent.OkDisplayContentText)]
        [DataRow(Constants.PartnerNames.Cart)]
        [DataRow(Constants.PartnerNames.OXODIME)]
        [DataRow(Constants.PartnerNames.OXOWebDirect)]
        [DataRow(Constants.PartnerNames.Webblends)]
        [DataRow(Constants.PartnerNames.WebblendsInline)]
        [TestMethod]
        public async Task GetChallengeDescription_CvvChallenge_PSSForHandlePurchaseRisk(string partner, string setButtonHintIdNameForFeature = null, string setButtonRequireUpdateNameForFeature = null)
        {
            // Arrange
            foreach (string country in Constants.Countries)
            {
                if (string.Equals(partner, Constants.VirtualPartnerNames.Macmanage, StringComparison.OrdinalIgnoreCase))
                {
                    string partnerSettingResponse = "{\"handlePurchaseRiskChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"features\":{\"customizeDisplayContent\":{\"displayCustomizationDetail\":[{\"setButtonDisplayContent\":{\"" + setButtonHintIdNameForFeature + "\":\"" + setButtonRequireUpdateNameForFeature + "\"}}]}}}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                }

                string url = $"/v7.0/Account001/challengeDescriptions?type=cvv&piid=Account001-Pi001-Visa&partner={partner}&country={country}&language=en-us&operation=RenderPidlPage";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK);

                foreach (PIDLResource resource in pidls)
                {
                    // Assert
                    if (string.Equals(partner, Constants.VirtualPartnerNames.Macmanage, StringComparison.OrdinalIgnoreCase))
                    {
                        var submitDisplayHint = resource.GetDisplayHintById(Constants.DisplayHintIds.Cvv3DSSubmitButton) as ButtonDisplayHint;

                        var submitButtonActionContext = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(submitDisplayHint.Action.Context));
                        Assert.IsNotNull(submitButtonActionContext);

                        if (setButtonHintIdNameForFeature != null && setButtonRequireUpdateNameForFeature != null)
                        {
                            Assert.AreEqual(setButtonRequireUpdateNameForFeature != null ? Constants.DisplayContent.OkDisplayContentText : Constants.DisplayContent.SubmitDisplayContentText, submitDisplayHint.DisplayText());
                        }
                        else
                        {
                            Assert.IsTrue(submitDisplayHint.Action.Context.ToString().Contains("/authenticateIndiaThreeDS"));
                        }
                    }
                    else
                    {
                        var submitDisplayHint = resource.GetDisplayHintById(Constants.DisplayHintIds.OKButton) as ButtonDisplayHint;

                        Assert.AreEqual(submitDisplayHint.DisplayContent, Constants.DisplayContent.OkDisplayContentText);

                        var actionContext = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(submitDisplayHint.Action.Context));

                        Assert.IsNull(actionContext);
                    }
                }

               PXSettings.PartnerSettingsService.ResetToDefaults();
            }
        }

        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, true)]
        [DataRow(PXService.GlobalConstants.Partners.OfficeSMB, false)]
        [TestMethod]
        public async Task GetChallengeDescription_psd2IgnorePIAuthorization_PSS(string partnerName, bool psd2IgnorePIAuthorization)
        {
            // Arrange
            string accountId = "Account001";
            string piid = "Account001-Pi006-Sepa";

            string settingJson = "{\"handlepaymentchallenge\":{\"template\":\"defaultTemplate\",\"features\":{\"psd2\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"psd2IgnorePIAuthorization\":" + psd2IgnorePIAuthorization.ToString().ToLower() + "}]}}}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(settingJson);

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                 new PaymentSessionData()
                                 {
                                     Language = "en",
                                     Amount = 10.0m,
                                     Currency = "EUR",
                                     Partner = partnerName,
                                     Country = "de",
                                     HasPreOrder = false,
                                     ChallengeScenario = ChallengeScenario.PaymentTransaction,
                                     ChallengeWindowSize = ChallengeWindowSize.Four,
                                     IsMOTO = false,
                                     BillableAccountId = "blah+",
                                     ClassicProduct = "fooBar",
                                     PaymentInstrumentId = piid
                                 })));

            var testHeader = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache" }
            };

            // When psd2IgnorePIAuthorization feature is enabled, extended view API should be called instead of get PI API to check on the PI
            PXSettings.PimsService.PreProcess = (pimsRequest) =>
            {
                Assert.AreEqual(psd2IgnorePIAuthorization, pimsRequest.RequestUri.AbsolutePath.Contains("/extendedView"), "when psd2IgnorePIAuthorization is enabled, extended view API should be called instead of get PI API");
                Assert.AreEqual(psd2IgnorePIAuthorization, pimsRequest.RequestUri.Query.Contains("?partner"), "when extended view API is called, a partner query param should be present");
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, null, testHeader);
            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidls[0].ClientAction.Context.ToString());
            Assert.AreEqual(piid, paymentSession.PaymentInstrumentId);
            Assert.AreEqual(false, paymentSession.IsChallengeRequired);
            PXSettings.PimsService.ResetToDefaults();
        }

        [TestMethod]
        public async Task GetChallengeDescription_IndiaUPIChallenge()
        {
            // Arrange
            PXSettings.PayerAuthService.ResponseProvider.SessionId = "Z" + Guid.NewGuid().ToString();
            PXSettings.PayerAuthService.ResponseProvider.ThreeDSMethodUrl = null;
            PXSettings.PayerAuthService.ResponseProvider.EnrollmentStatus = "Bypassed";
            PXSettings.PayerAuthService.ResponseProvider.TransStatus = "Y";
            PXSettings.PayerAuthService.ResponseProvider.TransStatusReason = "TSR03";

            string accountId = "Account004";
            string currency = "INR";
            string country = "in";
            string piid = "Account004-Pi001-IndiaUPI";
            string language = "en-us";
            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account004", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = "webblends",
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction,
                                })));

            // Act
            await GetRequest(
                url,
                null,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);

                    var pidls = ReadPidlResourceFromJson(responseBody);
                    Assert.IsNotNull(pidls);
                    Assert.AreEqual(1, pidls.Count, "The count of PIDLs is not as expected");
                    PIDLResource pidl = pidls[0];
                    Assert.IsNotNull(pidls[0].ClientAction.Context);
                    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(pidl.ClientAction.Context.ToString());
                    Assert.IsTrue(data.Contains(new KeyValuePair<string, string>("challengeStatus", "ByPassed")));
                });
        }

        [TestMethod]
        public async Task GetChallengeDescription_IndiaUPIChallenge_Fail()
        {
            // Arrange
            PXSettings.PayerAuthService.ResponseProvider.SessionId = "Z" + Guid.NewGuid().ToString();
            PXSettings.PayerAuthService.ResponseProvider.EnrollmentStatus = "Unavailable";
            PXSettings.PayerAuthService.ResponseProvider.ThreeDSMethodUrl = null;
            PXSettings.PayerAuthService.ResponseProvider.TransStatus = "Y";
            PXSettings.PayerAuthService.ResponseProvider.TransStatusReason = "TSR03";

            string accountId = "Account004";
            string currency = "INR";
            string country = "in";
            string piid = "Account004-Pi001-IndiaUPI";
            string language = "en-us";
            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account004", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = "webblends",
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction,
                                })));

            // Act
            await GetRequest(
                url,
                null,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    // Assert
                    Assert.AreEqual(HttpStatusCode.BadRequest, responseCode);
                });
        }

        /// <Summary>
        ///  This test is to verify get challenge returns a challenge
        ///  and it gets added as a linkedpidl to the main add PI pidl
        /// </Summary>
        [DataRow("Account001", PXService.GlobalConstants.Partners.CommercialStores)]
        [TestMethod]
        public async Task AddChallenge_Success(string accountId, string partner)
        {
            var requestHeaders = new Dictionary<string, string> { { "x-ms-pidlsdk-version", "2.3.9" } };
            string url = string.Format("v7.0/{0}/paymentMethodDescriptions?country=us&partner={1}&language=en-US&family=credit_card&showChallenge=true&pxChallengeSessionId=554403e2-96ce-4c9e-aa9a-45b4c60f3f19", accountId, partner);
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, "PXChallengeSwitch", requestHeaders);
            Assert.IsNotNull(pidls[0].LinkedPidls);
            ButtonDisplayHint submitButton = (ButtonDisplayHint)pidls[0].GetDisplayHintById("saveButton");
            var actionContext = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(submitButton.Action.Context));
            Assert.IsTrue(actionContext.Href.Contains("pxChallengeSessionId"), "POST url is missing pxSessionId param");
        }

        /// <Summary>
        ///  This test is to test the challenge flow when CMS add challenge returns service error and falls back to PX HIP
        /// </Summary>
        [Ignore]
        [DataRow("Account001", PXService.GlobalConstants.Partners.CommercialStores)]
        [TestMethod]
        public async Task AddChallenge_FallBack_To_PX_HIP_On_ServiceError(string accountId, string partner)
        {
            var requestHeaders = new Dictionary<string, string> { { "x-ms-pidlsdk-version", "2.3.9" } };
            PXSettings.ChallengeManagementService.ArrangeResponse("{\"error\":{\"code\":\"InternalServerError\",\"message\":\"Somethingwentwrong,unabletocreateachallenege\",\"innerHttpError\":null,\"properties\":{}},\"properties\":{}}", HttpStatusCode.InternalServerError, HttpMethod.Post, ".*/create");
            string url = string.Format("v7.0/{0}/paymentMethodDescriptions?country=us&partner={1}&language=en-US&family=credit_card&showChallenge=true&pxChallengeSessionId=554403e2-96ce-4c9e-aa9a-45b4c60f3f19", accountId, partner);
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, "PXChallengeSwitch, PXEnableHIPCaptchaGroup", requestHeaders);
            Assert.IsNull(pidls[0].LinkedPidls);
            var captchaImage = pidls[0].GetDisplayHintById("CaptchaImage") as ImageDisplayHint;
            Assert.IsNotNull(captchaImage, "Captcha image not found hence fall back to HIP didn't work");
        }

        /// <Summary>
        ///  This test is to test the challenge flow when add challenge returns conflict error
        /// </Summary>
        [DataRow("Account001", PXService.GlobalConstants.Partners.CommercialStores)]
        [TestMethod]
        public async Task AddChallenge_Failure_UpdateSession_To_Abandoned(string accountId, string partner)
        {
            var requestHeaders = new Dictionary<string, string> { { "x-ms-pidlsdk-version", "2.3.9" } };
            PXSettings.ChallengeManagementService.ArrangeResponse("{\"error\":{\"code\":\"Conflict\",\"message\":\"Somethingwentwrong,unabletocreateachallenege\",\"innerHttpError\":null,\"properties\":{}},\"properties\":{}}", HttpStatusCode.InternalServerError, HttpMethod.Post, ".*/create");
            string url = string.Format("v7.0/{0}/paymentMethodDescriptions?country=us&partner={1}&language=en-US&family=credit_card&showChallenge=true&pxChallengeSessionId=554403e2-96ce-4c9e-aa9a-45b4c60f3f19", accountId, partner);
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, "PXChallengeSwitch", requestHeaders);
            Assert.IsNull(pidls[0].LinkedPidls, "linkedpidl shouldn't exist when there is no challenge from CMS");
        }

        /// <Summary>
        ///  This test is to test the challenge flow when add challenge returns exception
        /// </Summary>
        [DataRow("Account001", PXService.GlobalConstants.Partners.CommercialStores)]
        [TestMethod]
        public async Task AddChallenge_On_Failure(string accountId, string partner)
        {
            var requestHeaders = new Dictionary<string, string> { { "x-ms-pidlsdk-version", "2.3.9" } };
            PXSettings.ChallengeManagementService.ArrangeResponse("{\"error\":{\"code\":,\"message\":\"Somethingwentwrong,unabletocreateachallenege\",\"innerHttpError\":null,\"properties\":{}},\"properties\":{}}", HttpStatusCode.InternalServerError, HttpMethod.Post, ".*/create");
            string url = string.Format("v7.0/{0}/paymentMethodDescriptions?country=us&partner={1}&language=en-US&family=credit_card&showChallenge=true&pxChallengeSessionId=554403e2-96ce-4c9e-aa9a-45b4c60f3f19", accountId, partner);
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, "PXChallengeSwitch", requestHeaders);
            Assert.IsNull(pidls[0].LinkedPidls, "there shouldn't be a linkedpidl when CMS throws exception");
        }

        [DataRow("bing", "cvv", "Account001-Pi001-Visa", "in", false)]
        [DataRow("bing", "cvv", "Account001-Pi001-Visa", "in", true)]
        [DataRow("cart", "cvv", "Account001-Pi001-Visa", "us", false)]
        [DataRow("cart", "cvv", "Account001-Pi001-Visa", "us", true)]
        [TestMethod]
        public async Task GetChallengeDescription_CvvChallenge_EnableSecureField(string partner, string type, string piid, string country, bool enableSecureField)
        {
            if (enableSecureField)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableSecureFieldCvvChallenge");
            }

            // Arrange
            string url = $"/v7.0/Account001/challengeDescriptions?partner={partner}&type={type}&piid={piid}&country={country}&language=en-us";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            VerifyPidlData(enableSecureField, pidls);
        }

        [DataRow(PXService.GlobalConstants.Partners.Webblends, false)]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, true)]
        [DataRow(PXService.GlobalConstants.Partners.Cart, false)]
        [DataRow(PXService.GlobalConstants.Partners.Cart, true)]
        [TestMethod]
        public async Task GetChallengeDescription_India3DSChallenge_EnableSecureField(string partnerName, bool enableSecureField)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "INR";
            string country = "in";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            var testHeader = new Dictionary<string, string>()
            {
                { "x-ms-test", "{\"scenarios\": \"px-service-3ds1-test-emulator\"}" }
            };

            string flightOverrides = string.Empty;
            if (enableSecureField)
            {
                flightOverrides = "PXEnableSecureFieldIndia3DSChallenge";
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, flightOverrides, testHeader);

            // Assert
            VerifyPidlData(enableSecureField, pidls);
        }

        [DataRow(PXService.GlobalConstants.Partners.NorthStarWeb, "cvv", "Account001-Pi001-Visa", "us", true)]
        [DataRow(PXService.GlobalConstants.Partners.NorthStarWeb, "cvv", "Account001-Pi001-Visa", "us", false)]
        [TestMethod]
        public async Task GetChallengeDescription_CvvChallenge_EncryptAndTokenize(string partner, string type, string piid, string country, bool enableEncryptAndTokenize)
        {
            if (enableEncryptAndTokenize)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncryptionOtherOperation");
            }

            // Arrange
            string url = $"/v7.0/Account001/challengeDescriptions?partner={partner}&type={type}&piid={piid}&country={country}&language=en-us";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            AssertEncryptAndTokenizePIDL(enableEncryptAndTokenize, pidls);
        }

        [DataRow(PXService.GlobalConstants.Partners.NorthStarWeb, false)]
        [DataRow(PXService.GlobalConstants.Partners.NorthStarWeb, true)]
        [TestMethod]
        public async Task GetChallengeDescription_India3DSChallenge_EncryptAndTokenize(string partnerName, bool enableEncryptAndTokenize)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "INR";
            string country = "in";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            var testHeader = new Dictionary<string, string>()
            {
                { "x-ms-test", "{\"scenarios\": \"px-service-3ds1-test-emulator\"}" }
            };

            if (enableEncryptAndTokenize)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncryptionOtherOperation");
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, additionaHeaders: testHeader);

            // Assert
            AssertEncryptAndTokenizePIDL(enableEncryptAndTokenize, pidls);
        }

        [DataRow(PXService.GlobalConstants.Partners.NorthStarWeb, "cvv", "Account001-Pi001-Visa", "us", true, true, false)]
        [DataRow(PXService.GlobalConstants.Partners.DefaultTemplate, "cvv", "Account001-Pi001-Visa", "us", true, true, true)]
        [DataRow(PXService.GlobalConstants.Partners.NorthStarWeb, "cvv", "Account001-Pi001-Visa", "us", false, false, false)]
        [DataRow(PXService.GlobalConstants.Partners.NorthStarWeb, "cvv", "Account001-Pi001-Visa", "cn", true, true, false)]
        [DataRow(PXService.GlobalConstants.Partners.NorthStarWeb, "cvv", "Account001-Pi001-Visa", "cn", false, false, false)]
        [TestMethod]
        public async Task GetChallengeDescription_CvvChallenge_EncryptAndTokenizeFetchConfig(string partner, string type, string piid, string country, bool enableFetchConfig, bool enableScript, bool enableSecureField)
        {
            if (enableFetchConfig)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncryptionFetchConfigOtherOperation");
            }

            if (enableScript)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncryptionFetchConfigWithScript");
            }

            if (enableSecureField)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableSecureFieldCvvChallenge");
            }

            // Arrange
            string url = $"/v7.0/Account001/challengeDescriptions?partner={partner}&type={type}&piid={piid}&country={country}&language=en-us";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            AssertEncryptAndTokenizeFetchConfigPIDL(enableFetchConfig, enableScript, enableSecureField, pidls);
        }

        [DataRow("windowsstore", "Account001-Paysafecard", "us", true, false)]
        [DataRow("windowsstore", "Account001-Paysafecard", "us", true, true)]
        [DataRow("windowsstore", "Account001-Paysafecard", "us", false, false)]

        [DataRow("storify", "Account001-Paysafecard", "us", true, false)]
        [DataRow("storify", "Account001-Paysafecard", "us", true, true)]
        [DataRow("storify", "Account001-Paysafecard", "us", false, false)]

        [DataRow("xbox", "Account001-Paysafecard", "us", true, false)]
        [DataRow("xbox", "Account001-Paysafecard", "us", true, true)]
        [DataRow("xbox", "Account001-Paysafecard", "us", false, false)]
        [TestMethod]
        public async Task GetChallengeDescription_Challenge_EncryptAndTokenizeFetchConfig(string partner, string piid, string country, bool enableFetchConfig, bool enableScript)
        {
            if (enableFetchConfig)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncryptionFetchConfigOtherOperation");
            }

            if (enableScript)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncryptionFetchConfigWithScript");
            }

            // Arrange
            string url = $"/v7.0/Account001/challengeDescriptions?partner={partner}&piid={piid}&country={country}&language=en-us&operation=RenderPidlPage&sessionId=12345678";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls);

            foreach (var pidl in pidls)
            {
                Assert.IsNull(pidl.DataDescription);
                Assert.IsNotNull(pidl.ClientAction);
            }
        }

        [DataRow(PXService.GlobalConstants.Partners.NorthStarWeb, false, false, false)]
        [DataRow(PXService.GlobalConstants.Partners.NorthStarWeb, true, true, false)]
        [TestMethod]
        public async Task GetChallengeDescription_India3DSChallenge_EncryptAndTokenizeFetchConfig(string partnerName, bool enableFetchConfig, bool enableScript, bool enableSecureField)
        {
            // Arrange
            string accountId = "Account001";
            string currency = "INR";
            string country = "in";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = piid,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));

            var testHeader = new Dictionary<string, string>()
            {
                { "x-ms-test", "{\"scenarios\": \"px-service-3ds1-test-emulator\"}" }
            };

            if (enableFetchConfig)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncryptionFetchConfigOtherOperation");
            }

            if (enableScript)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncryptionFetchConfigWithScript");
            }

            if (enableSecureField)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableSecureFieldIndia3DSChallenge");
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, additionaHeaders: testHeader);

            // Assert
            AssertEncryptAndTokenizeFetchConfigPIDL(enableFetchConfig, enableScript, enableSecureField, pidls);
        }

        [DataRow(PXService.GlobalConstants.Partners.CommercialStores, "IN", true, true)]
        [DataRow(PXService.GlobalConstants.Partners.CommercialStores, "IN", false, false)]
        [DataRow(PXService.GlobalConstants.Partners.Azure, "IN", true, true)]
        [DataRow(PXService.GlobalConstants.Partners.Azure, "IN", false, false)]
        [DataRow(PXService.GlobalConstants.Partners.Azure, "US", true, false)]
        [TestMethod]
        public async Task GetChallengeDescription_ExpiryDateMasked_IndiaCvvChallengeFlow(string partnerName, string country, bool isfeatureFlighted, bool shouldEnableChallenge)
        {
            // Arrange
            string language = "en-us";
            string accountId = "Account011";
            string paymentInstrumentId = "Account011-Pi001-IndiaCommercialStoresExpiryDate";

            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = "INR",
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = paymentInstrumentId,
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));
            var testHeader = new Dictionary<string, string>();
            PXFlightHandler.AddToEnabledFlights("IndiaExpiryGroupDelete");
            if (isfeatureFlighted)
            {
                PXFlightHandler.AddToEnabledFlights("IndiaCvvChallengeExpiryGroupDelete");

                // to get expiry date for bing it is required
                testHeader = new Dictionary<string, string>()
                {
                    { "x-ms-test", "{\"scenarios\": \"px-service-3ds1-test-emulator\"}" }
                };
            }

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, paymentInstrumentId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            // Act
            await GetRequest(
                url,
                testHeader,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);

                    Assert.IsNotNull(pidls);
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);

                    var containerDisplayHint = pidls[0].DisplayPages[0].Members[4] as ContainerDisplayHint;
                    Assert.IsNotNull(containerDisplayHint);
                    if (shouldEnableChallenge)
                    {
                        Assert.AreEqual(containerDisplayHint.Members[0].DisplayText(), PXConstants.ExpiryDate.IndiaCvvChallengeExpiryDateMasked);
                    }
                    else
                    {
                        Assert.AreEqual(containerDisplayHint.Members[0].DisplayText(), "12/2099");
                    }
                });
        }

        private static void AssertEncryptAndTokenizeFetchConfigPIDL(bool enableFetchConfig, bool enableScript, bool enableSecureField, List<PIDLResource> pidls)
        {
            Assert.IsNotNull(pidls);

            foreach (var pidl in pidls)
            {
                if (enableFetchConfig && !enableSecureField)
                {
                    var cvvToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("cvvToken");

                    Assert.AreEqual("TokenizeMSREncrypt", cvvToken.DataProtection.ProtectionType, "Data protection type should be MSREncrypt");
                    Assert.IsNotNull(cvvToken.DataProtection.Parameters["publicKey"], "Public key should not be null");
                    Assert.IsNotNull(cvvToken.DataProtection.FetchConfig, "Fetch config should not be null");
                    Assert.AreEqual(4, cvvToken.DataProtection.FetchConfig.FetchOrder.Count, "Fetch config retry order count should be 4");

                    if (enableScript)
                    {
                        Assert.AreEqual("encryptAndTokenize.js", cvvToken.DataProtection.Parameters["encryptionScript"], "Function should not be null");
                    }
                    else
                    {
                        Assert.AreEqual("encrypt", cvvToken.DataProtection.Parameters["encryptionFunction"], "Function should not be null");
                    }
                }
                else
                {
                    Assert.IsNull(pidl.GetPropertyDescriptionByPropertyNameWithFullPath("cvvToken").DataProtection, "DataProtection should be null");

                    var cvvTokenDisplayHint = pidl.GetDisplayHintByPropertyName("cvvToken");
                    if (cvvTokenDisplayHint != null)
                    {
                        Assert.AreEqual(enableSecureField ? "secureproperty" : "property", cvvTokenDisplayHint.DisplayHintType, "DisplayHintType for cvvToken should be secure property if secure filed enabled");
                    }
                }
            }
        }

        private static void AssertEncryptAndTokenizePIDL(bool enableEncryptAndTokenize,  List<PIDLResource> pidls)
        {
            Assert.IsNotNull(pidls);

            foreach (var pidl in pidls)
            {
                if (enableEncryptAndTokenize)
                {
                    Assert.AreEqual("MSREncrypt", pidl.GetPropertyDescriptionByPropertyNameWithFullPath("cvvToken").DataProtection.ProtectionType, "Data protection type should be MSREncrypt");
                    Assert.IsNotNull(pidl.GetPropertyDescriptionByPropertyNameWithFullPath("cvvToken").DataProtection.Parameters["publicKey"], "Public key should not be null");
                }
                else
                {
                    Assert.IsNull(pidl.GetPropertyDescriptionByPropertyNameWithFullPath("cvvToken").DataProtection, "DataProtection should be null");
                }
            }
        }

        private static void VerifyPidlData(bool enableSecureField, List<PIDLResource> pidls)
        {
            Assert.IsNotNull(pidls);

            foreach (var pidl in pidls)
            {
                if (enableSecureField)
                {
                    var cvv = pidl.GetDisplayHintById("cvv") as SecurePropertyDisplayHint;
                    Assert.IsNotNull(cvv, "cvv is expected to be not null");
                    Assert.IsTrue(cvv.DisplayHintType.Contains("secureproperty"));
                }
                else
                {
                    var cvv = pidl.GetDisplayHintById("cvv") as PropertyDisplayHint;
                    Assert.IsNotNull(cvv);
                    Assert.IsFalse(cvv.DisplayHintType.Contains("secureproperty"));
                }
            }
        }
    }
}