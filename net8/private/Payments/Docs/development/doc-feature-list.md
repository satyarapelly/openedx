# Feature List

## Accessor

| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | ChangePartnerNameForPIMS (inline) | Changes the partner name to mapped partner name in URL while sending request to PIMS | Used to convert "partner=macmanage" to "partner=commercialstores" in PIMS request url for GET and POST requests. |

## Address
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | AddBillingAddressForWindows | Used for reorganizing display content in Add Billing Address | Used for reorganizing display content in Add Billing Address for windows partner |
| 2 | AddCCTwoPageForWindows | Used for reorganizing display content in Add CC | Used for reorganizing display content in Add CC for windows partner |
| 3 | AddPayPalForWindows | Applies style hints specific to windows partner | Applies style hints specific to windows partner |
| 4 | AddVenmoForWindows | Applies style hints specific to windows partner | Applies style hints specific to windows partner |
| 5 | AddressSuggestionMessage | Remove DisplayHint AddressSuggestionMessage. Change SuggestedAddressText text to "We suggest:" | Remove DisplayHint AddressSuggestionMessage. Change SuggestedAddressText text to "We suggest:" |
| 6 | CustomizeAddressForm | Multiple options available for customizing address forms | "ungrouping first and last name elements, removing "(Optional)" text from fields, Disabling country dropdown, adding data sources, disabling fields, hiding fields, changing text on cancel button, updating address submit action, making fields required, removing fields, enabling fields" |
| 7 | dataSource | Specify data source for specific address type | can specify particular jarvis data source for address type shipping, etc. |
| 8 | convertAddressTypeTo | Convert specific address type to another | convert one type of address to another for processing / submission purposes. |
| 9 | FieldsToBeRemoved | Removes the specified fields in displayCustomizationDetail from the Data and Display descriptions of PIDL resource | remove fields not needed / not in schema for a request |
| 10 | UseTradeAVSForAddressValidation | It is a sub-feature under the addressValidation feature. | update the modern validation for PIDL action based on the data source Jarvis and its address type |
| 11 | EnableConditionalFieldsForBillingAddress | Enable / update fields for address toggle | adds the billing address toggle functionality to pidl form add cc. |
| 12 | EnableAddtionalAddressTitle | Enables additional title on address form | Enables additional title on address form |
| 13 | EnableAddtionalAddressHeader | Enables additional header on address form | Enables additional header on address form |
| 14 | EnableAddtionalAddressFooter | Enables additional footer on address form | Enables additional footer on address form |
| 15 | SkipJarvisAddressSyncToLegacy | Adds flag to query params to skip CTP account creation from jarvis | used when we want to have jarvis skip the sync to legacy |

## Credit Card
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | chinaAllowVisaMasterCard | Allows Visa and Mastercard in China. (Only for commercial and legacy consumer partners) | commercialstores enables visa and mastercard in china. |
| 2 | DisableIndiaTokenization | Disables India Tokenization by removing the IndiaTokenConsentMessage & IndiaTokenConsentMessageHyperLink from Display Description and TokenizationConsent from DataDescriptions for CreditCard flow | India tokenization feature is enabled by default in template based flow, per a compliance requirement in India. However, couple of existing partners are not ready or couldn't enable this flow yet (e.g. consumersupport partner, in the flow the user won't be present so India 3ds couldn't be completed). So this feature disables India tokenization. |
| 3 | CustomizeDisplayContent | various options to change display content in PIDL | "change save button text, change cvv challenge text, change address suggest message, add all fields required text, change select pi button text" |
| 4 | setSaveButtonDisplayContentAsNext | Used to set Save button display content as Next (only for Credit Card) | Comment says may be removed in favor of using CustomizeDisplayContent. |
| 5 | EnableTokenizationEncryption | Encrypt token of CVV and PAN on the client-side using encryption library instead of using the server-side encryption when endpoint fails | Not currently being used as native partners / console cannot support this. |
| 6 | EnableTokenizationEncryptionFetchConfig | Adds in the fetchConfig data in the PIDL under dataDescription.dataProtection.fetchConfig | PIDLSDK uses this for encryption / tokenization |
| 7 | UseTextForCVVHelpLink | Use text for the CVV help link | changes link to text used in react-native setups where browser may not be accessible. |
| 8 | ChangeExpiryStyleToTextBox | Changes style structure of expiry month and year. | Changes expiry dropdown to textbox. |
| 9 | RemoveAddressFieldsValidationForCC | Removes the regex, mandatory, min/max length etc. validations for creditcard address fields and adds the address properties to the DataDescription if not already present for all countries. | Used for partner battlenet to allow submitting custom values. |

## General
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | CustomizeDisplayTag | Display tags can cause rendering issues for certain partner pidlsdk js / element factories. Edit / Remove display tags | used mostly for native partners helps with selection borders on focus, etc. |
| 2 | CustomizeElementLocation | Can swap position of certain fields in address / credit card forms. Used to change the location / order of PIDL elements | in add cc swap cardholder name and card number fields order |
| 3 | CustomizeStructure | Update structure of PIDL for partners that have styling dependencies on structure. Used to change display content of a PIDL element | used to remove grouping of expiry month and year for battlenet pidl add cc |
| 4 | CustomizeSubmitButtonContext | Used to customize the submit button properties such as endpoint, action context | used in family funding to set a special submit context to do a PATCH to the my-family jarvis endpoint with consumerV3 profile. |
| 5 | DisableElement | Used to disable certain elements in PIDLs | not sure where used / other customize PIDL features seem to be able to do this as well. |
| 6 | EnableElement | Used to enable certain elements in PIDLs | currently can only enable field with displayHintId "addressCountry". |
| 7 | HideElement | Used to hide certain elements in PIDLs | used to hide several different fields including addressCountry, paymentOptionSaveText, paymentSummaryText. |
| 8 | RemoveElement | Used to remove certain elements in PIDLs | used to remove elements in PIDLS including select pi edit button, new payment method link, ewallet yes buttons. |
| 9 | UpdateElementType | Used to update certain element's display hint type in PIDLs | used to update privacy statement from a link to a button |

## Handle Payment Challenge
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | CvvChallengeForWindows | Applies style hints specific to windows partner | Applies style hints specific to windows partner |
| 2 | UseIFrameForPiLogOn | Changes the login link to a move last action | To replace the navigate action to a move last action for Global PIs. Also implementing a new layout for the Global PI QR code flow |
| 3 | UseOmsTransactionServiceStore (inline feature) | Use the OMS Transaction Service Store in india3DS validation parameters | deciding which transaction store to use during AuthenticateIndiaThreeDS |
| 4 |UseAzureTransactionServiceStore (inline feature) | Use the AzureTransaction Service Store in india3DS validation parameters | deciding which transaction store to use during AuthenticateIndiaThreeDS |
| 5 | PXUsePSSToEnableValidatePIOnAttachChallenge (inline feature) | This feature sets the challenge type in all instances where ValidatePIOnAttachEnabledPartners is used. It will set the challenge type to ValidatePIOnAttachChallenge or PSD2Challenge. If the challenge status fails, it will set the error codes to ValidatePIOnAttachFailed. | It can be used to set the challenge type. |

## Handle PurchaseRisk Challenge
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1  | PXEnableChallengeCvvValidation | Enables CVV validation with error messages and min/max length constraints. | Used for alphastore with the partner battlenet during checkout on the challenge page.  |
| 2  | SetActionContextEmpty  | Sets the action context to empty for the submit button. | Required for bing partner when migrating to PSS partner to ensure proper functionality. |


## Misc / Internal
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | RemoveDataSource | Removes data sources from the pidls and which resources to remove data sources can be configured via displayCustomizationDetail. Like address, profile, taxId | Feature in PX to sync with NPRS |
| 2 | SetSubmitURLToEmptyForTaxId | Sets the submitUrl to empty in TaxId resource for returning secondary pidl payload to partner on submit | It allows partner to get both primary and linked pidl payload along with submit handler. |
| 3 | DpHideCountry | Used to hide country dropdown in PIDL for TaxId form | some partners prefer to hide the country dropdown  vs disabling it. |
| 4 | UseProfilePrerequisitesV3 (Inline feature) | Use profileType + prerequisitesV3 form instead of profileType + prerequisites while adding/updating pi | Used in PaymentMethodDescription while adding PI in completeprerequisite flow. |
| 5 | noSubmitIfGSTIDEmpty | Stops from making the post call for saving GSTID if it is empty UseTradeAVSForAddressValidation. Used for TaxIdDescription | commercialstores in india will skip submitting GSTID if its empty |
| 6 | useJarvisV3ForAddress | Use jarvis v3 for address flows requiring jarvis | using v3 jarvis instead of legacy |
| 7 | useJarvisV3ForProfile | Use jarvis v3 for profile flows requiring jarvis | using v3 jarvis instead of legacy |
| 8 | psd2IgnorePIAuthorization | Used in HandlePaymentChallenge / CreatePaymentSession. Used to bypass auth checks on pi | Used by commercial partners to bypass ownership check. (With commercial partner, the user may not be the original pi owner due to being part of an organization, etc.) |
| 9 | addAllFieldsRequiredText | Used for all PI flow. Added the feature AddAllFieldsRequiredText, which displays "All fields are mandatory/required." in the form. Enable for both template (with feature) and non-template partner (with flight). | Some partners prefer to have this text displayed. |
| 10 | GroupAddressFields | Code has been refactor initial it was used with name GroupAddressFieldsForCreditCard and now it has been used with GroupAddressFields. Now the changes has been covered for the all other resource type which uses the billing, BillingInternal, Sepa and BillingGroup.  | This feature is used for grouping address fields on creditcard form for styling purposes |
| 11 | addAsteriskToAllMandatoryFields | Automatically appends an asterisk (*) to the display text of all mandatory fields in PIDL forms to visually indicate required fields. Includes accessibility support by updating screen reader tags. | This feature provides a generic solution for marking mandatory fields with asterisks. Originally designed for battlenet partner at initial but can be used to any PIDL form type as needed. |

## Payment Instrument
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | SingleMarketDirective | Enable SMD for EU markets | all EU markets enable SMD |
| 2 | SetBackButtonDisplayContentAsCancel | Change CancelBackButton text to "Cancel" | Some partners want Back button to say Cancel due to how the navigation works (iframe especially) |
| 3 | SetSaveButtonDisplayContentAsBook | Change saveButton text to "Book" | Bing travel needs to change Save button to say "book" |
| 4 | SwapLogoSource | Used to swap PI logo source url | used by native partners |
| 5 | EnableSecureField | Enables Secure Field | secure field use |
| 6 | EnableShortUrl | Enable short url for qrcodes (paypal, venmo) | primarily used for console / native |
| 7 | UpdateStaticResourceServiceEndpoint | Used to update all the logo urls in pidl display hints to point to staticresources CDN directly instead of pmservices endpoint Eg. url - [Click here](https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa.svg) is replaced with [Click here](https://staticresources.payments.microsoft-int.com/staticresourceservice/images/v4/logo_visa.svg") | Instead of replacing the references directly, we wanted to update it using feature and flight this change. We planned to enable the feature based on flight(PXUseCDNForStaticResourceService).We couldn't find an easy way to reliably measure if it is working fine and if any requests to staticresources cdn are failing, so the work was paused. |
| 8 | PXEnableModernIdealPayment |  Update the ideal payment UI to align with figma designs |  This feature is used to update the ideal payment UI to align with figma designs |
| 9 | AddPrefillAddressCheckbox | Adds prefill address checkbox on credit card forms | Adds prefill address checkbox for default template |

## Profile
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | CustomizeProfileForm | Customize various properties of profile form | used in family funding to modify PIDL and data description to match submit scheme for Jarvis my-family endpoint and convert profile back to consumer from consumerv3 when submitting to jarvis |

## Redeem
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | RedeemGiftCard | Adds stylehints to redeem PIDL | Applies style hints specific to windows partner |

## Select PM
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | OverrideCheckDisplayNameToWireTransfer | Used to override display name of check pm to "Wire Transfer" | primarily used by commercialstores we can also use for other by enabling the inline feature. |
| 2 | SkipSelectPM | This feature skips select PM if CreditCard is the only option and goes straight to CreditCard AddResource | some partners have only add credit card, so we should skip select pm and go straight to add cc |
| 3 | PaymentMethodGrouping | Group PM and append subpages for each PM group | many partners use this to group select PM and append subpages for each PM group. |
| 4 | EnableVirtualFamilyPM (Inline feature) | This feature enables the virtual family paymentMethods | commercialstores uses this to enable the virtual.invoice_basic pm |
| 5 | SetDefaultPaymentMethod | Sets / moves default pm to top | use to move the default pm to the top of the list |
| 6 | SelectPMButtonListStyleForWindows | Applies style hints specific to windows partner | Applies style hints specific to windows partner |
| 7 | UseTextOnlyForPaymentOption | Only use displayText in the select option (by removing displayContent) for JS or classic element factory which can't render displayContent properly | Only use displayText in the select option (by removing displayContent) for JS or classic element factory which can't render displayContent properly |

## SelectInstance (List Pi)
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | AddLocalCardFiltering | Updates data source for local card PIDLs with a configuration | setups up local card PIDL configuration for local data source when dataSourceConfig is null |
| 2 | AddPMButtonWithPlusIcon | Adds plus icon to the "Add a new payment method" button | ggpdeds and other partners use this for list pi to have the circled "+" icon next to add a new payment method |
| 3 | CustomizeActionContext | Action Context customization | commercialstores can enable this to replace the action context with the pi id to handle the flow themselves |
| 4 | EnableSingleInstancePidls (Inline) | This feature enables the displaysinglepi resource of selectSingleinstance operation flow | Added the displaysinglepi pidl resource similar to amcweb |
| 5 | EnableSelectSingleInstancePiDisplay (Inline) | Enables the SinglePiDisplayPidl for selectinstance/selectSingleinstance operation flow | amcweb / windowssettings can use this to get a PIDL that only displays a single pi from /paymentMethodDescriptions |
| 6 | IncludeCreditCardLogos | Includes defined credit logo urls in the data description.  | Mainly use for client side prefill scenario where we're populating list with local cards which don't have logo |
| 7 | InlineLocalCardDetails | Makes local card details template inline | Makes local card details template inline |
| 8 | ListPIForWindows | Applies style hints specific to windows partner | Applies style hints specific to windows partner |
| 9 | ShowPIExpirationInformation | Show expiration alert for expired card in list pi dropdown | partner can use this if they would like to show the alert for expired pi |
| 10 | SplitListPIInformationIntoTwoLines | Display cardHolderName in the first line and LastFourDigits and Expiry in the next line under the same group for a list payment instrument dropdown | Used to achieve two line formatting for list pi dropdown |
| 11 | UseDisabledPIsForSelectInstance (Inline) | Enables to use the disabled PIs for selectinstance/selectSingleinstance operation flow | show / use disabled PIs in list pi / select pi |

# SelectInstance Address
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | ListAddressForWindows | Applies style hints specific to windows partner | Applies style hints specific to windows partner |

## Update Payment Instrument
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | UpdateCCTwoPageForWindows | Applies style hints specific to windows partner | Applies style hints specific to windows partner |
| 2 | UpdatePidlSubmitLink | Customize the Submit Link | Can do things like update consumer profile to a PATCH operation, set taxid submits to empty to returnpayload to partner instead of submitting ourselves. |

## Update Profile Form
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | UseProfileUpdateToHapi | These inline feature is added for the profile type organization as replacement for the feature flight - PXProfileUpdateToHapi | <ul><li> Updated href to hapi-endpoint for profile. </li> <li> Replaced VatId with vat_idAdditionalData from taxId for Taiwan (TW) country with types "organization" or "legal." </li> <li> For updates with type "organization," changed operation to update_partial if the feature is enabled. </li> <li>  Applied server-side prefill (etag and ifmatch) when the feature is not enabled for employee profiles. </li> <li> Enabled client-side prefill (etag and ifmatch) when the profile type and feature are enabled. </li> <li> Set operation to update_patch for LinkedTaxIdProfile. </li></ul> |
| 2 | UseEmployeeProfileUpdateToHapi | These inline feature is added for the profile type employee as replacement for the feature flight - PXProfileUpdateToHapi | <ul><li> Updated href to hapi-endpoint for profile. </li> <li> For updates with type "employee":-  Changed operation to update_partial if the feature is enabled. </li> <li> Applied server-side prefill (etag and ifmatch) only if the feature is not enabled for employee profiles. </li> <li> Enabled client-side prefill (etag and ifmatch) when the profile type and feature are enabled." </li> </ul> |
| 3 | UseMultipleProfile | If a partner enables multiple profiles and doesn’t pass | If partner enables multiple profiles and partner doesn't pass standaloneProfile in the x-ms-flighting header to override the setting then we return multiple profiles for certain markets |

## Validate Instance
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | SetIsSubmitGroupFalse | Set submit group to false | "pidlsdk uses isSubmitGroup to determine if this is part of the submit block which can be hidden by passing in a boolean to pidlsdk.Used when partner, such as commercialstore or azure, wants to use our submit buttons on some pidl pages and their own on other pages. " |
| 2 | VerifyAddressStyling | Style hints for verify address | Applies style hints specific to windows partner |

## Add Credit Card
| **SI. NO.** | **Feature Name** | **Description** | **Use Case** |
|--------------|-------------|---------|---------|
| 1 | RemoveAcceptCardMessage | This is an inline feature added to remove the heading in AddPI flow | To remove the heading "We accept the following cards"on AddPI flow. |
| 2  | SMDDisabled | This is an inline feature to disable the country dropdown for the SMD market. | To disable the country dropdown for the SMD enabled market when using defaulttemplate as an partner |


## Notes
- Features are listed in categories it belonged to.

## How to Update
1. Add new features at the bottom of the table to the respective category.
4. Maintain consistent formatting
