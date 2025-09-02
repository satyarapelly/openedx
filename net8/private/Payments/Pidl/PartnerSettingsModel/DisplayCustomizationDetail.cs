// <copyright file="DisplayCustomizationDetail.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PartnerSettingsModel
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class DisplayCustomizationDetail
    {
        [JsonProperty(PropertyName = "moveCardNumberBeforeCardHolderName")]
        public bool MoveCardNumberBeforeCardHolderName { get; set; }

        [JsonProperty(PropertyName = "moveOrganizationNameBeforeEmailAddress")]
        public bool MoveOrganizationNameBeforeEmailAddress { get; set; }

        [JsonProperty(PropertyName = "moveLastNameBeforeFirstName")]
        public bool? MoveLastNameBeforeFirstName { get; set; }

        [JsonProperty(PropertyName = "setSaveButtonDisplayContentAsNext")]
        public bool SetSaveButtonDisplayContentAsNext { get; set; }

        [JsonProperty(PropertyName = "setButtonDisplayContent")]
        public IReadOnlyDictionary<string, string> SetButtonDisplayContent { get; set; }

        [JsonProperty(PropertyName = "updateCvvChallengeTextForGCO")]
        public bool UpdateCvvChallengeTextForGCO { get; set; }

        [JsonProperty(PropertyName = "enableCountryAddorUpdateCC")]
        public bool EnableCountryAddorUpdateCC { get; set; }

        [JsonProperty(PropertyName = "removeStarRequiredTextGroup")]
        public bool? RemoveStarRequiredTextGroup { get; set; }

        // This property will also hide the middle name.
        [JsonProperty(PropertyName = "hideFirstAndLastNameForCompletePrerequisites")]
        public bool HideFirstAndLastNameForCompletePrerequisites { get; set; }

        [JsonProperty(PropertyName = "hideAddCreditDebitCardHeading")]
        public bool HideAddCreditDebitCardHeading { get; set; }

        [JsonProperty(PropertyName = "updatePaymentMethodHeadingTypeToText")]
        public bool? UpdatePaymentMethodHeadingTypeToText { get; set; }

        [JsonProperty(PropertyName = "setGroupedSelectOptionTextBeforeLogo")]
        public bool? SetGroupedSelectOptionTextBeforeLogo { get; set; }

        [JsonProperty(PropertyName = "setSelectPMWithLogo")]
        public bool? SetSelectPMWithLogo { get; set; }

        [JsonProperty(PropertyName = "useTextForCVVHelpLink")]
        public bool? UseTextForCVVHelpLink { get; set; }

        [JsonProperty(PropertyName = "removeAddCreditDebitCardHeading")]
        public bool? RemoveAddCreditDebitCardHeading { get; set; }

        [JsonProperty(PropertyName = "cvvDisplayHelpPosition")]
        public string CvvDisplayHelpPosition { get; set; }

        [JsonProperty(PropertyName = "matchSelectPMMainPageStructureForSubPage")]
        public bool? MatchSelectPMMainPageStructureForSubPage { get; set; }

        [JsonProperty(PropertyName = "useFixedSVGForMC")]
        public bool? UseFixedSVGForMC { get; set; }

        [JsonProperty(PropertyName = "enableIndia3dsForNonZeroPaymentTransaction")]
        public bool? EnableIndia3dsForNonZeroPaymentTransaction { get; set; }

        [JsonProperty(PropertyName = "pxEnableIndia3DS1Challenge")]
        public bool? PXEnableIndia3DS1Challenge { get; set; }

        [JsonProperty(PropertyName = "india3dsEnableForBilldesk")]
        public bool? India3dsEnableForBilldesk { get; set; }

        [JsonProperty(PropertyName = "usePSSForPXFeatureFlighting")]
        public bool? UsePSSForPXFeatureFlighting { get; set; }

        [JsonProperty(PropertyName = "enableSecureFieldAddCC")]
        public bool EnableSecureFieldAddCC { get; set; }

        [JsonProperty(PropertyName = "setSaveButtonDisplayContentAsBook")]
        public bool SetSaveButtonDisplayContentAsBook { get; set; }

        [JsonProperty(PropertyName = "removeCancelButton")]
        public bool RemoveCancelButton { get; set; }

        [JsonProperty(PropertyName = "removeSelectPiEditButton")]
        public bool? RemoveSelectPiEditButton { get; set; }

        [JsonProperty(PropertyName = "removeSelectPiNewPaymentMethodLink")]
        public bool? RemoveSelectPiNewPaymentMethodLink { get; set; }

        [JsonProperty(PropertyName = "setBackButtonDisplayContentAsCancel")]
        public bool SetBackButtonDisplayContentAsCancel { get; set; }

        [JsonProperty(PropertyName = "setCancelButtonDisplayContentAsBack")]
        public bool SetCancelButtonDisplayContentAsBack { get; set; }

        [JsonProperty(PropertyName = "setPrivacyStatementHyperLinkDisplayToButton")]
        public bool? SetPrivacyStatementHyperLinkDisplayToButton { get; set; }

        [JsonProperty(PropertyName = "hidePaymentSummaryText")]
        public bool HidePaymentSummaryText { get; set; }

        [JsonProperty(PropertyName = "hidepaymentOptionSaveText")]
        public bool HidepaymentOptionSaveText { get; set; }

        [JsonProperty(PropertyName = "addressType")]
        public string AddressType { get; set; }

        [JsonProperty(PropertyName = "dataSource")]
        public string DataSource { get; set; }

        [JsonProperty(PropertyName = "submitActionType")]
        public string SubmitActionType { get; set; }

        [JsonProperty(PropertyName = "fieldsToBeHidden")]
        public IEnumerable<string> FieldsToBeHidden { get; set; }

        [JsonProperty(PropertyName = "fieldsToMakeRequired")]
        public IEnumerable<string> FieldsToMakeRequired { get; set; }

        [JsonProperty(PropertyName = "removeOptionalTextFromFields")]
        public bool? RemoveOptionalTextFromFields { get; set; }

        [JsonProperty(PropertyName = "psd2IgnorePIAuthorization")]
        public bool? Psd2IgnorePIAuthorization { get; set; }

        [JsonProperty(PropertyName = "addressSuggestionMessage")]
        public bool AddressSuggestionMessage { get; set; }

        [JsonProperty(PropertyName = "updateAccessibilityNameWithPosition")]
        public bool? UpdateAccessibilityNameWithPosition { get; set; }

        [JsonProperty(PropertyName = "updateXboxElementsAccessibilityHints")]
        public bool? UpdateXboxElementsAccessibilityHints { get; set; }

        [JsonProperty(PropertyName = "displayTagsToBeRemoved")]
        public IEnumerable<KeyValuePair<string, string>> DisplayTagsToBeRemoved { get; set; }

        [JsonProperty(PropertyName = "displayTagsToBeAdded")]
        public IReadOnlyDictionary<string, Dictionary<string, string>> DisplayTagsToBeAdded { get; set; }

        [JsonProperty(PropertyName = "styleHintsToBeAdded")]
        public IReadOnlyDictionary<string, List<string>> StyleHintsToBeAdded { get; set; }

        [JsonProperty(PropertyName = "elementsToBeUnhidden")]
        public IEnumerable<string> ElementsToBeUnhidden { get; set; }

        [JsonProperty(PropertyName = "disableSelectPiRadioOption")]
        public bool? DisableSelectPiRadioOption { get; set; }

        [JsonProperty(PropertyName = "updateSelectPiButtonText")]
        public bool? UpdateSelectPiButtonText { get; set; }

        [JsonProperty(PropertyName = "removeGroupForExpiryMonthAndYear")]
        public bool? RemoveGroupForExpiryMonthAndYear { get; set; }
        
        [JsonProperty(PropertyName = "addressSuggestion")]
        public bool? AddressSuggestion { get; set; }

        [JsonProperty(PropertyName = "useAddressDataSourceForUpdate")]
        public bool? UseAddressDataSourceForUpdate { get; set; }

        [JsonProperty(PropertyName = "disableCountryDropdown")]
        public bool? DisableCountryDropdown { get; set; }

        [JsonProperty(PropertyName = "ungroupAddressFirstNameLastName")]
        public bool? UngroupAddressFirstNameLastName { get; set; }

        [JsonProperty(PropertyName = "removeDefaultStyleHints")]
        public bool? RemoveDefaultStyleHints { get; set; }

        [JsonProperty(PropertyName = "endpoint")]
        public string EndPoint { get; set; }

        [JsonProperty(PropertyName = "operation")]
        public string Operation { get; set; }

        [JsonProperty(PropertyName = "profileType")]
        public string ProfileType { get; set; }

        [JsonProperty(PropertyName = "dataFieldsToRemoveFromPayload")]
        public IEnumerable<string> DataFieldsToRemoveFromPayload { get; set; }

        [JsonProperty(PropertyName = "dataFieldsToRemoveFullPath")]
        public string DataFieldsToRemoveFullPath { get; set; }

        [JsonProperty(PropertyName = "convertProfileTypeTo")]
        public string ConvertProfileTypeTo { get; set; }

        [JsonProperty(PropertyName = "fieldsToBeDisabled")]
        public IEnumerable<string> FieldsToBeDisabled { get; set; }

        [JsonProperty(PropertyName = "fieldsToBeEnabled")]
        public IEnumerable<string> FieldsToBeEnabled { get; set; }

        [JsonProperty(PropertyName = "fieldsToBeRemoved")]
        public IEnumerable<string> FieldsToBeRemoved { get; set; }

        [JsonProperty(PropertyName = "components")]
        public IEnumerable<string> Components { get; set; }

        [JsonProperty(PropertyName = "updateConsumerProfileSubmitLinkToJarvisPatch")]
        public bool? UpdateConsumerProfileSubmitLinkToJarvisPatch { get; set; }

        [JsonProperty(PropertyName = "hidePaymentMethodHeading")]
        public bool? HidePaymentMethodHeading { get; set; }

        [JsonProperty(PropertyName = "hideChangeSettingText")]
        public bool? HideChangeSettingText { get; set; }

        [JsonProperty(PropertyName = "hideCountryDropdown")]
        public bool? HideCountryDropdown { get; set; }

        [JsonProperty(PropertyName = "removeEwalletYesButtons")]
        public bool? RemoveEwalletYesButtons { get; set; }

        [JsonProperty(PropertyName = "removeSpaceInPrivacyTextGroup")]
        public bool? RemoveSpaceInPrivacyTextGroup { get; set; }

        [JsonProperty(PropertyName = "removeDataSourceResources")]
        public IEnumerable<string> RemoveDataSourceResources { get; set; }

        [JsonProperty(PropertyName = "removeEwalletBackButtons")]
        public bool? RemoveEwalletBackButtons { get; set; }

        [JsonProperty(PropertyName = "useIFrameForPiLogOn")]
        public bool? UseIFrameForPiLogOn { get; set; }

        [JsonProperty(PropertyName = "useSuggestAddressesTradeAVSV1Scenario")]
        public bool? UseSuggestAddressesTradeAVSV1Scenario { get; set; }

        [JsonProperty(PropertyName = "addCCAddressValidationPidlModification")]
        public bool? AddCCAddressValidationPidlModification { get; set; }

        [JsonProperty(PropertyName = "verifyAddressPidlModification")]
        public bool? VerifyAddressPidlModification { get; set; }

        [JsonProperty(PropertyName = "replaceContextInstanceWithPaymentInstrumentId")]
        public bool? ReplaceContextInstanceWithPaymentInstrumentId { get; set; }

        [JsonProperty(PropertyName = "setActionContextEmpty")]
        public bool? SetActionContextEmpty { get; set; }

        [JsonProperty(PropertyName = "setSubmitURLToEmptyForTaxId")]
        public bool? SetSubmitURLToEmptyForTaxId { get; set; }

        [JsonProperty(PropertyName = "addAccessibilityNameExpressionToNegativeValue")]
        public bool? AddAccessibilityNameExpressionToNegativeValue { get; set; }

        [JsonProperty(PropertyName = "removeAddressFormHeading")]
        public bool? RemoveAddressFormHeading { get; set; }

        [JsonProperty(PropertyName = "enableIsSelectPMskippedValue")]
        public bool? EnableIsSelectPMskippedValue { get; set; }

        [JsonProperty(PropertyName = "hideAcceptCardMessage")]
        public bool? HideAcceptCardMessage { get; set; }

        [JsonProperty(PropertyName = "removeAnotherDeviceTextFromShortUrlInstruction")]
        public bool? RemoveAnotherDeviceTextFromShortUrlInstruction { get; set; }

        [JsonProperty(PropertyName = "displayShortUrlAsHyperlink")]
        public bool? DisplayShortUrlAsHyperlink { get; set; }

        [JsonProperty(PropertyName = "displayShortUrlAsVertical")]
        public bool? DisplayShortUrlAsVertical { get; set; }

        [JsonProperty(PropertyName = "hideAddressCheckBoxIfAddressIsNotPrefilledFromServer")]
        public bool? HideAddressCheckBoxIfAddressIsNotPrefilledFromServer { get; set; }

        [JsonProperty(PropertyName = "fieldsToSetIsSubmitGroupFalse")]
        public IEnumerable<string> FieldsToSetIsSubmitGroupFalse { get; set; }

        [JsonProperty(PropertyName = "displayAccentBorderWithGutterOnFocus")]
        public bool? DisplayAccentBorderWithGutterOnFocus { get; set; }

        [JsonProperty(PropertyName = "addStyleHints")]
        public bool? AddStyleHints { get; set; }

        [JsonProperty(PropertyName = "removeStyleHints")]
        public bool? RemoveStyleHints { get; set; }

        [JsonProperty(PropertyName = "removeAcceptCardMessage")]
        public bool? RemoveAcceptCardMessage { get; set; }

        [JsonProperty(PropertyName = "removeMicrosoftPrivacyTextGroup")]
        public bool? RemoveMicrosoftPrivacyTextGroup { get; set; }

        [JsonProperty(PropertyName = "hideCardLogos")]
        public bool? HideCardLogos { get; set; }

        [JsonProperty(PropertyName = "hideAddress")]
        public bool? HideAddress { get; set; }

        [JsonProperty(PropertyName = "addAllFieldsRequiredText")]
        public bool? AddAllFieldsRequiredText { get; set; }

        [JsonProperty(PropertyName = "addAsteriskToAllMandatoryFields")]
        public bool? AddAsteriskToAllMandatoryFields { get; set; }

        [JsonProperty(PropertyName = "addCancelButton")]
        public bool AddCancelButton { get; set; }

        [JsonProperty(PropertyName = "addPickAWayToPayHeading")]
        public bool? AddPickAWayToPayHeading { get; set; }

        [JsonProperty(PropertyName = "addCancelButtonToHomePage")]
        public bool? AddCancelButtonToHomePage { get; set; }

        [JsonProperty(PropertyName = "enableBackupPICheckForSkipSelectInstanceNoPI")]
        public bool? EnableBackupPICheckForSkipSelectInstanceNoPI { get; set; }

        [JsonProperty(PropertyName = "addTriggeredByForSkipSelectInstanceNoPI")]
        public bool? AddTriggeredByForSkipSelectInstanceNoPI { get; set; }

        [JsonProperty(PropertyName = "returnAddCCOnlyForSkipSelectInstanceNoPI")]
        public bool? ReturnAddCCOnlyForSkipSelectInstanceNoPI { get; set; }

        [JsonProperty(PropertyName = "updateRegexesForCards")]
        public IEnumerable<KeyValuePair<string, string>> UpdateRegexesForCards { get; set; }

        [JsonProperty(PropertyName = "updatePrefillCheckboxText")]
        public bool? UpdatePrefillCheckboxText { get; set; }

        [JsonProperty(PropertyName = "prefillCheckboxText")]
        public string PrefillCheckboxText { get; set; }

        [JsonProperty(PropertyName = "customizeNCESEPA")]
        public bool? CustomizeNCESEPA { get; set; }
    }
}