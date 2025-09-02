// <copyright file="FeatureFactory.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.Common.Web;

    /// <summary>
    /// Class maintaining the features to be enabled
    /// </summary>
    public class FeatureFactory : IFeatureFactory
    {
        private readonly Dictionary<string, IFeature> features = new Dictionary<string, IFeature>()
        {
            { FeatureConfiguration.FeatureNames.PaymentMethodGrouping, new PaymentMethodGrouping() },
            { FeatureConfiguration.FeatureNames.SetButtonActionToSuccessType, new SetButtonActionToSuccessType() },
            { FeatureConfiguration.FeatureNames.CustomizeDisplayContent, new CustomizeDisplayContent() },
            { FeatureConfiguration.FeatureNames.CustomizeElementLocation, new CustomizeElementLocation() },
            { FeatureConfiguration.FeatureNames.AddressValidation, new AddressValidation() },
            { FeatureConfiguration.FeatureNames.EnableElement, new EnableElement() },
            { FeatureConfiguration.FeatureNames.HideElement, new HideElement() },
            { FeatureConfiguration.FeatureNames.SingleMarketDirective, new SingleMarketDirective() },
            { FeatureConfiguration.FeatureNames.UseTextForCVVHelpLink, new UseTextForCVVHelpLink() },
            { FeatureConfiguration.FeatureNames.ShowPIExpirationInformation, new ShowPIExpirationInformation() },
            { FeatureConfiguration.FeatureNames.SplitListPIInformationIntoTwoLines, new SplitListPIInformationIntoTwoLines() },
            { FeatureConfiguration.FeatureNames.SwapLogoSource, new SwapLogoSource() },
            { FeatureConfiguration.FeatureNames.ChangeDisplayHintToText, new UpdateElementType() },
            { FeatureConfiguration.FeatureNames.RemoveElement, new RemoveElement() },
            { FeatureConfiguration.FeatureNames.EnableSecureField, new EnableSecureField() },
            { FeatureConfiguration.FeatureNames.CustomizeAddressForm, new CustomizeAddressForm() },
            { FeatureConfiguration.FeatureNames.AddRedeemGiftCardButton, new AddRedeemGiftCardButton() },
            { FeatureConfiguration.FeatureNames.CustomizeDisplayTag, new CustomizeDisplayTag() },
            { FeatureConfiguration.FeatureNames.SkipSelectPM, new SkipSelectPM() },
            { FeatureConfiguration.FeatureNames.UpdateStaticResourceServiceEndPoint, new UpdateStaticResourceServiceEndpoint() },
            { FeatureConfiguration.FeatureNames.DisableElement, new DisableElement() },
            { FeatureConfiguration.FeatureNames.DpHideCountry, new DpHideCountry() },
            { FeatureConfiguration.FeatureNames.CustomizeStructure, new CustomizeStructure() },
            { FeatureConfiguration.FeatureNames.AddPMButtonWithPlusIcon, new AddPMButtonWithPlusIcon() },
            { FeatureConfiguration.FeatureNames.SkipJarvisAddressSyncToLegacy, new SkipJarvisAddressSyncToLegacy() },
            { FeatureConfiguration.FeatureNames.CustomizeSubmitButtonContext, new CustomizeSubmitButtonContext() },
            { FeatureConfiguration.FeatureNames.CustomizeProfileForm, new CustomizeProfileForm() },
            { FeatureConfiguration.FeatureNames.UpdateCCTwoPageForWindows, new UpdateCCTwoPageForWindows() },
            { FeatureConfiguration.FeatureNames.UpdatePidlSubmitLink, new UpdatePidlSubmitLink() },
            { FeatureConfiguration.FeatureNames.AddBillingAddressForWindows, new AddBillingAddressForWindows() },
            { FeatureConfiguration.FeatureNames.AddCCTwoPageForWindows, new AddCCTwoPageForWindows() },
            { FeatureConfiguration.FeatureNames.AddPayPalForWindows, new AddPayPalForWindows() },
            { FeatureConfiguration.FeatureNames.AddVenmoForWindows, new AddVenmoForWindows() },
            { FeatureConfiguration.FeatureNames.SelectPMButtonListStyleForWindows, new SelectPMButtonListStyleForWindows() },
            { FeatureConfiguration.FeatureNames.EnableConditionalFieldsForBillingAddress, new EnableConditionalFieldsForBillingAddress() },
            { FeatureConfiguration.FeatureNames.IncludeCreditCardLogos, new IncludeCreditCardLogos() },
            { FeatureConfiguration.FeatureNames.ListPIForWindows, new ListPIForWindows() },
            { FeatureConfiguration.FeatureNames.RemoveDataSource, new RemoveDataSource() },
            { FeatureConfiguration.FeatureNames.UseIFrameForPiLogOn, new UseIFrameForPiLogOn() },
            { FeatureConfiguration.FeatureNames.VerifyAddressStyling, new VerifyAddressStyling() },
            { FeatureConfiguration.FeatureNames.UseTextOnlyForPaymentOption, new UseTextOnlyForPaymentOption() },
            { FeatureConfiguration.FeatureNames.CvvChallengeForWindows, new CvvChallengeForWindows() },
            { FeatureConfiguration.FeatureNames.InlineLocalCardDetails, new InlineLocalCardDetails() },
            { FeatureConfiguration.FeatureNames.ListAddressForWindows, new ListAddressForWindows() },
            { FeatureConfiguration.FeatureNames.RedeemGiftCard, new RedeemGiftCard() },
            { FeatureConfiguration.FeatureNames.CustomizeActionContext, new CustomizeActionContext() },
            { FeatureConfiguration.FeatureNames.EnablePlaceholder, new EnablePlaceholder() },
            { FeatureConfiguration.FeatureNames.GroupAddressFields, new GroupAddressFields() },
            { FeatureConfiguration.FeatureNames.EnableShortURL, new EnableShortUrl() },
            { FeatureConfiguration.FeatureNames.EnableTokenizationEncryption, new EnableTokenizationEncryption() },
            { FeatureConfiguration.FeatureNames.EnableTokenizationEncryptionFetchConfig, new EnableTokenizationEncryptionFetchConfig() },
            { FeatureConfiguration.FeatureNames.SetIsSubmitGroupFalse, new SetIsSubmitGroupFalse() },
            { FeatureConfiguration.FeatureNames.AddLocalCardFiltering, new AddLocalCardFiltering() },
            { FeatureConfiguration.FeatureNames.PXEnableXboxNativeStyleHints, new EnableXboxNativeStyleHints() },
            { FeatureConfiguration.FeatureNames.DisableIndiaTokenization, new DisableIndiaTokenization() },
            { FeatureConfiguration.FeatureNames.SetDefaultPaymentMethod, new SetDefaultPaymentMethod() },
            { FeatureConfiguration.FeatureNames.CustomizeTaxIdForm, new CustomizeTaxIdForm() },
            { FeatureConfiguration.FeatureNames.AddElement, new AddElement() },
            { FeatureConfiguration.FeatureNames.AddStyleHintsToDisplayHints, new AddStyleHintsToDisplayHints() },
            { FeatureConfiguration.FeatureNames.UnhideElements, new UnhideElements() },
            { FeatureConfiguration.FeatureNames.SkipSelectInstanceNoPI, new SkipSelectInstanceNoPI() },
            { FeatureConfiguration.FeatureNames.MoveSelectedPIToFirstOption, new MoveSelectedPIToFirstOption() },
            { FeatureConfiguration.FeatureNames.EnableUpdateCreditCardRegex, new EnableUpdateCreditCardRegex() },
            { FeatureConfiguration.FeatureNames.ChangeExpiryStyleToTextBox, new ChangeExpiryStyleToTextBox() },
            { FeatureConfiguration.FeatureNames.RemoveAddressFieldsValidationForCC, new RemoveAddressFieldsValidationForCC() },
            { FeatureConfiguration.FeatureNames.EnableDeletePaymentInstrument, new EnableDeletePaymentInstrument() },
            { FeatureConfiguration.FeatureNames.UpdateAddressline1Length, new UpdateAddressline1Length() },
            { FeatureConfiguration.FeatureNames.ChangeExpiryMonthYearToExpiryDateTextBox, new ChangeExpiryMonthYearToExpiryDateTextBox() },
            { FeatureConfiguration.FeatureNames.CombineExpiryMonthYearToDateTextBox, new CombineExpiryMonthYearToDateTextBox() },
            { FeatureConfiguration.FeatureNames.CustomizeSEPAForm, new CustomizeSEPAForm() },
        };

        public Dictionary<string, IFeature> GetFeatures(FeatureContext featureContext)
        {
            // Logic if FeatureConfigs are provided in featureContext
            if (featureContext.FeatureConfigs != null && featureContext.FeatureConfigs.Count > 0)
            {
                var enabledFeatures = this.features.Where(feature => FeatureConfiguration.IsEnabledUsingPartnerSettings(feature.Key, featureContext)).ToDictionary(feature => feature.Key, feature => feature.Value);

                // Override the EnableSecureField feature from Azure Flighting Experience if it is not enabled in PartnerSettings
                this.OverrideFeature(featureContext, enabledFeatures, FeatureConfiguration.FeatureNames.EnableSecureField);

                // Override the CustomizeActionContext feature from Azure Flighting Experience if it is not enabled in PartnerSettings
                this.OverrideFeature(featureContext, enabledFeatures, FeatureConfiguration.FeatureNames.CustomizeActionContext);

                // Override the UpdateDataProtections feature from Azure Flighting Experience if it is not enabled in PartnerSettings
                this.OverrideFeature(featureContext, enabledFeatures, FeatureConfiguration.FeatureNames.EnableTokenizationEncryption);

                // Override the UpdateDataProtections feature from Azure Flighting Experience if it is not enabled in PartnerSettings
                this.OverrideFeature(featureContext, enabledFeatures, FeatureConfiguration.FeatureNames.EnableTokenizationEncryptionFetchConfig);

                // Override the EnableUpdateCreditCardRegex feature from Azure Flighting Experience if it is not enabled in PartnerSettings
                this.OverrideFeature(featureContext, enabledFeatures, FeatureConfiguration.FeatureNames.EnableUpdateCreditCardRegex);
                
                this.OverrideFeature(featureContext, enabledFeatures, FeatureConfiguration.FeatureNames.UpdateAddressline1Length);

                // Override the EnableUpdateCreditCardRegex feature from Azure Flighting Experience if it is not enabled in PartnerSettings
                this.OverrideFeature(featureContext, enabledFeatures, FeatureConfiguration.FeatureNames.ChangeExpiryMonthYearToExpiryDateTextBox);

                // Override the CombineExpiryMonthYearToDateTextBox feature from Azure Flighting Experience if it is not enabled in PartnerSettings
                this.OverrideFeature(featureContext, enabledFeatures, FeatureConfiguration.FeatureNames.CombineExpiryMonthYearToDateTextBox);

                return enabledFeatures;
            }

            return this.features.Where(feature => FeatureConfiguration.IsEnabled(feature.Key, featureContext)).ToDictionary(feature => feature.Key, feature => feature.Value);
        }

        private void OverrideFeature(FeatureContext featureContext, Dictionary<string, IFeature> enabledFeatures, string featureName)
        {
            if (!enabledFeatures.ContainsKey(featureName))
            {
                if (FeatureConfiguration.IsEnabled(featureName, featureContext))
                {
                    enabledFeatures.Add(featureName, this.features[featureName]);
                }
            }
        }
    }
}