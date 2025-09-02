// <copyright file="EnableShortUrl.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    internal class EnableShortUrl : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ProcessShortUrl,
            };
        }

        internal static void ProcessShortUrl(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            string shortUrl = featureContext?.ShortUrl;
            if (string.IsNullOrEmpty(shortUrl))
            {
                return;
            }

            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.EnableShortURL, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    string shortUrlInstruction = displayHintCustomizationDetail?.RemoveAnotherDeviceTextFromShortUrlInstruction == true ? Constants.UnlocalizedDisplayText.RemoveAnotherDeviceTextFromShortUrlInstruction : Constants.UnlocalizedDisplayText.ShortUrlInstructionText;

                    foreach (PIDLResource pidlResource in inputResources)
                    {
                        if (string.Equals(featureContext.Scenario, Constants.ScenarioNames.PaypalQrCode) || string.Equals(featureContext.ContextDescriptionType, Constants.ScenarioNames.PaypalQrCode))
                        {
                            AddShortURLItems(pidlResource, featureContext.Language, shortUrl, Constants.DisplayHintIds.PaypalQrCodeChallengeImage, Constants.DisplayHintIds.PaypalQrCodeImageAndURLGroup, Constants.DisplayHintIds.PaypalPIShortUrlGroup, Constants.DisplayHintIds.PaypalPIShortUrlInstruction, Constants.DisplayHintIds.PaypalPIShortUrl, shortUrlInstruction, displayHintCustomizationDetail);
                        }
                        else if (string.Equals(featureContext.Scenario, Constants.ScenarioNames.VenmoQRCode) || string.Equals(featureContext.ContextDescriptionType, Constants.ScenarioNames.VenmoQRCode))
                        {
                            AddShortURLItems(pidlResource, featureContext.Language, shortUrl, Constants.DisplayHintIds.VenmoQrCodeChallengeImage, Constants.DisplayHintIds.VenmoQrCodeImageAndURLGroup, Constants.DisplayHintIds.VenmoURLGroup, Constants.DisplayHintIds.VenmoUrlInstructionText, Constants.DisplayHintIds.VenmoShortUrl, shortUrlInstruction, displayHintCustomizationDetail);
                        }
                    }
                }
            }
        }

        internal static void AddShortURLItems(PIDLResource retVal, string language, string shortUrl, string qrCodeChallengeImageDisplayHintId, string groupUsingShortUrlHintId, string shortUrlGroupHintId, string shortUrlInstructionHintId, string shortUrlDisplayHintId, string shortUrlInstruction, DisplayCustomizationDetail displayCustomizationDetail = null)
        {
            if (retVal?.DisplayPages == null)
            {
                return;
            }

            var qrCodeChallengeImage = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ImageDisplayHint>(qrCodeChallengeImageDisplayHintId, retVal.DisplayPages);
            
            // Update the source URL of the QR code image to the short URL
            if (qrCodeChallengeImage != null)
            {
                qrCodeChallengeImage.SourceUrl = PIDLResourceFactory.GetUrlQrCodeImage(shortUrl);
            }

            var qrCodeGroup = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<GroupDisplayHint>(groupUsingShortUrlHintId, retVal.DisplayPages);
            TextDisplayHint shortUrlDisplayHeader = new TextDisplayHint
            {
                DisplayContent = LocalizationRepository.Instance.GetLocalizedString(shortUrlInstruction, language),
                HintId = shortUrlInstructionHintId
            };

            DisplayHint shortUrlText = new TextDisplayHint
            {
                DisplayContent = shortUrl,
                HintId = shortUrlDisplayHintId
            };

            DisplayHint shortUrlHyperlink = new HyperlinkDisplayHint
            {
                DisplayContent = shortUrl,
                HintId = shortUrlDisplayHintId,
                Action = new DisplayHintAction
                {
                    ActionType = DisplayHintActionType.navigate.ToString(),
                    Context = shortUrl
                }
            };

            DisplayHint shortUrlElement = displayCustomizationDetail?.DisplayShortUrlAsHyperlink == true ? shortUrlHyperlink : shortUrlText;

            GroupDisplayHint shortUrlgroup = new GroupDisplayHint
            {
                Members = { shortUrlDisplayHeader, shortUrlElement },
                HintId = shortUrlGroupHintId,
                StyleHints = new List<string> { "width-fill" }
            };

            if (qrCodeGroup != null && qrCodeGroup.Members != null)
            {
                qrCodeGroup.Members.Add(shortUrlgroup);
                qrCodeGroup.LayoutOrientation = displayCustomizationDetail?.DisplayShortUrlAsVertical == true ? Constants.PartnerHintsValues.VerticalPlacement : Constants.PartnerHintsValues.InlinePlacement;
            }
        }
    }
}
