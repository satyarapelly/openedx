// <copyright file="CustomizeAddressForm.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the CustomizeAddressForm, which is used to Customize Address form.
    /// </summary>
    internal class CustomizeAddressForm : IFeature
    {
        private static Dictionary<string, Dictionary<string, string>> displayHintIdMappings = new Dictionary<string, Dictionary<string, string>>()
        {
            { Constants.DataSource.Hapi, Constants.HapiDisplayHintIdsMappings },
            { Constants.DataSource.JarvisBilling, Constants.JarvisDisplayHintIdsMappings },
            { Constants.DataSource.JarvisShipping, Constants.JarvisDisplayHintIdsMappings },
            { Constants.DataSource.JarvisShippingV3, Constants.JarvisDisplayHintIdsMappings },
            { Constants.DataSource.JarvisOrgAddress, Constants.JarvisOrgAddressDisplayHintIdsMappings }
        };

        private static Dictionary<string, Dictionary<string, object>> SubmitActionParametersByType
        {
            get
            {
                return new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase)
                {
                    { Constants.SubmitActionType.LegacyValidate, new Dictionary<string, object>()
                        {
                            { GlobalConstants.SubmitActionParams.Href, Constants.SubmitUrls.PifdAnonymousLegacyAddressValidationUrl },
                            { GlobalConstants.SubmitActionParams.Method, GlobalConstants.HttpMethods.Post },
                            { GlobalConstants.SubmitActionParams.ErrorCodeExpressions, new[] { "({contextData.innererror.code})", "({contextData.code})" } },
                            { GlobalConstants.SubmitActionParams.HeaderType, null }
                        }
                    },
                    { Constants.SubmitActionType.JarvisCM, new Dictionary<string, object>()
                        {
                            { GlobalConstants.SubmitActionParams.Href, Constants.SubmitUrls.JarvisFdAddressCreateUrlTemplate },
                            { GlobalConstants.SubmitActionParams.Method, GlobalConstants.HttpMethods.Post },
                            { GlobalConstants.SubmitActionParams.ErrorCodeExpressions, null },
                            { GlobalConstants.SubmitActionParams.HeaderType, GlobalConstants.SubmitActionHeaderTypes.Jarvis }
                        }
                    },
                    { Constants.SubmitActionType.AddressEx, new Dictionary<string, object>()
                        {
                            { GlobalConstants.SubmitActionParams.Href, Constants.SubmitUrls.PifdAddressPostUrlTemplate },
                            { GlobalConstants.SubmitActionParams.Method, GlobalConstants.HttpMethods.Post },
                            { GlobalConstants.SubmitActionParams.ErrorCodeExpressions, null },
                            { GlobalConstants.SubmitActionParams.HeaderType, GlobalConstants.SubmitActionHeaderTypes.Jarvis }
                        }
                    }
                };
            }
        }

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                CustomizeAddressComponents,
            };
        }

        internal static void CustomizeAddressComponents(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.CustomizeAddressForm, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (string.Equals(displayHintCustomizationDetail.AddressType, featureContext.OriginalTypeName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (displayHintCustomizationDetail?.UngroupAddressFirstNameLastName != null && bool.Parse(displayHintCustomizationDetail?.UngroupAddressFirstNameLastName.ToString()))
                        {
                            foreach (PIDLResource addressPidl in inputResources)
                            {
                                UngroupFirstNameLastName(addressPidl);
                            }
                        }

                        if (displayHintCustomizationDetail?.RemoveOptionalTextFromFields != null && bool.Parse(displayHintCustomizationDetail?.RemoveOptionalTextFromFields.ToString()))
                        {
                            foreach (PIDLResource addressPidl in inputResources)
                            {
                                if (displayHintIdMappings.TryGetValue(displayHintCustomizationDetail.DataSource, out Dictionary<string, string> hintIdMappings)
                                        && (hintIdMappings != null))
                                {
                                    foreach (string displayHintId in hintIdMappings.Values)
                                    {
                                        RemoveOptionalFromDisplayName(addressPidl, displayHintId, featureContext);
                                    }
                                }
                            }
                        }

                        if (displayHintCustomizationDetail?.DisableCountryDropdown != null && bool.Parse(displayHintCustomizationDetail?.DisableCountryDropdown.ToString()))
                        {
                            foreach (PIDLResource addressPidl in inputResources)
                            {
                                addressPidl.SetPropertyState(Constants.DataDescriptionPropertyNames.Country, false);
                            }
                        }

                        switch (displayHintCustomizationDetail.AddressType)
                        {
                            case Constants.AddressTypes.HapiV1SoldToIndividual:
                                if (string.Equals(featureContext.OperationType, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase) && displayHintCustomizationDetail?.UseAddressDataSourceForUpdate != null && bool.Parse(displayHintCustomizationDetail?.UseAddressDataSourceForUpdate.ToString()))
                                {
                                    foreach (PIDLResource addressPidl in inputResources)
                                    {
                                        addressPidl.AddDataSource(Constants.DataSourceNames.AddressResource, new PXCommon.DataSource(Constants.SubmitUrls.HapiV1SoldToIndividual, Constants.HTTPVerbs.GET, new Dictionary<string, string>()));
                                    }
                                }

                                break;
                            default:
                                break;
                        }

                        if (displayHintCustomizationDetail.FieldsToBeDisabled != null)
                        {
                            foreach (PIDLResource addressPidl in inputResources)
                            {
                                foreach (string hintId in displayHintCustomizationDetail?.FieldsToBeDisabled)
                                {
                                    string displayHintId;

                                    if (displayHintIdMappings.TryGetValue(displayHintCustomizationDetail.DataSource, out Dictionary<string, string> hintIdMappings)
                                        && (hintIdMappings != null && hintIdMappings.TryGetValue(hintId, out displayHintId)))
                                    {
                                        if (!string.IsNullOrEmpty(displayHintId))
                                        {
                                            FeatureHelper.DisableDisplayHint(addressPidl, displayHintId);
                                        }
                                    }
                                }
                            }
                        }

                        if (displayHintCustomizationDetail.FieldsToBeHidden != null)
                        {
                            SetFeildToBeHidden(inputResources, displayHintCustomizationDetail);
                        }

                        // Flight check is added to check if the feature is exposed for the flight PXEnableSetCancelButtonDisplayContentAsBack for non-template partner
                        if (displayHintCustomizationDetail.SetCancelButtonDisplayContentAsBack)
                        {
                            foreach (PIDLResource inputResource in inputResources)
                            {
                                FeatureHelper.EditDisplayContent(inputResource, Constants.ButtonDisplayHintIds.CancelButton, Constants.UnlocalizedDisplayText.BackButtonDisplayText);
                            }
                        }

                        // SubmitAction customization for address forms when DataSource is Jarvis
                        if (!string.IsNullOrEmpty(displayHintCustomizationDetail.SubmitActionType)
                            && SubmitActionParametersByType.TryGetValue(displayHintCustomizationDetail.SubmitActionType, out Dictionary<string, object> submitActionParameters))
                        {
                            foreach (PIDLResource resource in inputResources)
                            {
                                if (string.Equals(displayHintCustomizationDetail.SubmitActionType, Constants.SubmitActionType.AddressEx, StringComparison.OrdinalIgnoreCase))
                                {
                                    UpdateAddressExSubmitActionHref(submitActionParameters, featureContext);
                                }

                                if (resource.DisplayPages != null)
                                {
                                    SetSubmitLink(
                                    FeatureHelper.GetButtonDisplayHintInPIDLResource(resource),
                                    GetTypedValueForSubmitActionParameters<string>(submitActionParameters, GlobalConstants.SubmitActionParams.Href),
                                    GetTypedValueForSubmitActionParameters<string>(submitActionParameters, GlobalConstants.SubmitActionParams.Method),
                                    GetTypedValueForSubmitActionParameters<string[]>(submitActionParameters, GlobalConstants.SubmitActionParams.ErrorCodeExpressions),
                                    GetTypedValueForSubmitActionParameters<string>(submitActionParameters, GlobalConstants.SubmitActionParams.HeaderType));
                                }
                            }
                        }

                        if (displayHintCustomizationDetail.FieldsToMakeRequired != null)
                        {
                            SetFieldsAsRequired(displayHintCustomizationDetail, inputResources, featureContext);
                        }

                        if (displayHintCustomizationDetail.FieldsToBeRemoved != null)
                        {
                            RemoveFields(inputResources, displayHintCustomizationDetail);
                        }

                        // Enables the fields in the PIDLResource
                        if (displayHintCustomizationDetail?.FieldsToBeEnabled != null)
                        {
                            foreach (string fieldName in displayHintCustomizationDetail.FieldsToBeEnabled)
                            {
                                foreach (PIDLResource addressPidl in inputResources)
                                {
                                    if (Constants.HapiDisplayHintIdsMappings.TryGetValue(fieldName, out string hapiDisplayHintId) && !string.IsNullOrEmpty(hapiDisplayHintId))
                                    {
                                        FeatureHelper.EnableDisplayHint(addressPidl, hapiDisplayHintId);
                                    }

                                    if (Constants.JarvisDisplayHintIdsMappings.TryGetValue(fieldName, out string jarvisDisplayHintId) && !string.IsNullOrEmpty(jarvisDisplayHintId))
                                    {
                                        FeatureHelper.EnableDisplayHint(addressPidl, jarvisDisplayHintId);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void UpdateAddressExSubmitActionHref(Dictionary<string, object> submitActionParameters, FeatureContext featureContext)
        {
            if (submitActionParameters.TryGetValue(GlobalConstants.SubmitActionParams.Href, out object hrefValue)
                && hrefValue != null)
            {
                hrefValue = hrefValue + $"?partner={featureContext.Partner}&language={featureContext.Language}&avsSuggest={featureContext.AvsSuggest}";

                if (!string.IsNullOrEmpty(featureContext.Scenario))
                {
                    hrefValue = hrefValue + "&scenario=" + featureContext.Scenario;
                }

                // Update the href value
                submitActionParameters[GlobalConstants.SubmitActionParams.Href] = hrefValue;
            }
        }

        private static T GetTypedValueForSubmitActionParameters<T>(Dictionary<string, object> submitActionParameters, string key)
        {
            if (submitActionParameters.TryGetValue(key, out object parameterValue)
                && parameterValue != null && parameterValue.GetType() == typeof(T))
            {
                return (T)parameterValue;
            }

            return default(T);
        }

        /// <summary>
        /// This method is used to hide the fields in the PIDLResource
        /// </summary>
        /// <param name="pidlResource">It store the pidl</param>
        /// <param name="displayHintCustomizationDetail">It used to check the feature is enabled or not.</param>
        private static void SetFeildToBeHidden(List<PIDLResource> pidlResource, DisplayCustomizationDetail displayHintCustomizationDetail)
        {
            foreach (PIDLResource inputResoure in pidlResource)
            {
                foreach (string hintId in displayHintCustomizationDetail?.FieldsToBeHidden)
                {
                    string displayHintId;

                    if (displayHintIdMappings.TryGetValue(displayHintCustomizationDetail.DataSource, out Dictionary<string, string> hintIdMappings)
                        && (hintIdMappings != null && hintIdMappings.TryGetValue(hintId, out displayHintId)))
                    {
                        FeatureHelper.HideDisplayHint(inputResoure, displayHintId);
                    }
                }
            }
        }

        private static void RemoveFields(List<PIDLResource> pidlResource, DisplayCustomizationDetail displayHintCustomizationDetail)
        {
            foreach (PIDLResource inputResoure in pidlResource)
            {
                foreach (string hintId in displayHintCustomizationDetail?.FieldsToBeRemoved)
                {
                    string displayHintId;

                    if (displayHintIdMappings.TryGetValue(displayHintCustomizationDetail.DataSource, out Dictionary<string, string> hintIdMappings)
                        && (hintIdMappings != null && hintIdMappings.TryGetValue(hintId, out displayHintId)))
                    {
                        DisplayHint displayHint = inputResoure.GetDisplayHintById(displayHintId);

                        // Remove from the DisplayDescription
                        inputResoure.RemoveDisplayHintById(displayHintId);

                        // Remove from the DataDescription
                        PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                        if (propertyDisplayHint != null)
                        {
                            inputResoure.RemoveDataDescription(propertyDisplayHint.PropertyName);
                        }
                    }
                }
            }
        }

        private static void UngroupFirstNameLastName(PIDLResource pidlResource)
        {
            if (pidlResource?.DisplayPages != null)
            {
                foreach (PageDisplayHint displayPage in pidlResource.DisplayPages)
                {
                    int groupSequencePosition = displayPage.Members.FindIndex(displayHint => displayHint.HintId == Constants.GroupDisplayHintIds.HapiFirstNameLastNameGroup);
                    GroupDisplayHint firstNameLastNameGroup = displayPage.Members.Find(displayHint => displayHint.HintId == Constants.GroupDisplayHintIds.HapiFirstNameLastNameGroup) as GroupDisplayHint;

                    if (groupSequencePosition != -1 & firstNameLastNameGroup != null)
                    {
                        displayPage.Members.InsertRange(groupSequencePosition, firstNameLastNameGroup.Members);
                        displayPage.Members.Remove(firstNameLastNameGroup);
                    }
                }
            }
        }

        private static void RemoveOptionalFromDisplayName(PIDLResource pidlResource, string hintId, FeatureContext featureContext)
        {
            // Get the display hint by feature flight
            var displayHintbyFeatureFlight = FeatureHelper.GetDisplayHintByFeatureFlight(hintId, featureContext, Constants.FeatureFlight.RemoveOptionalInLabel).FirstOrDefault();

            var textDisplayHintWithoutOptional = displayHintbyFeatureFlight != null && string.Equals(displayHintbyFeatureFlight.HintId, hintId, StringComparison.OrdinalIgnoreCase)
                 ? displayHintbyFeatureFlight as PropertyDisplayHint : null;

            PropertyDisplayHint textDisplayHint = pidlResource.GetDisplayHintById(hintId) as PropertyDisplayHint;

            if (textDisplayHint != null && textDisplayHintWithoutOptional != null)
            {
                textDisplayHint.DisplayName = LocalizationRepository.Instance.GetLocalizedString(textDisplayHintWithoutOptional.DisplayName, featureContext.Language);
                textDisplayHint.AddOrUpdateDisplayTag(Constants.DiplayHintProperties.AccessibilityName, textDisplayHint.DisplayName);
            }
        }

        /// <summary>
        /// This method is used to make the fields mandatory for Jarivs and Hapi Address User Type.
        /// </summary>
        /// <param name="displayHintCustomizationDetail">It is used to check the feature is enaled or not.</param>
        /// <param name="pidlResource">It contains the pidl.</param>
        /// <param name="featureContext">It contains the feature context.</param>
        private static void SetFieldsAsRequired(DisplayCustomizationDetail displayHintCustomizationDetail, List<PIDLResource> pidlResource, FeatureContext featureContext)
        {
            foreach (PIDLResource addressPidl in pidlResource)
            {
                foreach (string hintId in displayHintCustomizationDetail?.FieldsToMakeRequired)
                {
                    string displayHintId;

                    if (displayHintIdMappings.TryGetValue(displayHintCustomizationDetail.DataSource, out Dictionary<string, string> hintIdMappings)
                        && (hintIdMappings != null && hintIdMappings.TryGetValue(hintId, out displayHintId)))
                    {
                        var propertyDescriptionId = addressPidl.GetDisplayHintById(displayHintId).PropertyName;
                        addressPidl.UpdateIsOptionalProperty(propertyDescriptionId, false);
                        RemoveOptionalFromDisplayName(addressPidl, displayHintId, featureContext);
                    }
                }
            }
        }

        private static void SetSubmitLink(DisplayHint button, string href, string method, string[] errorCodeExpressions, string headerType)
        {
            var newSubmitLink = new PXCommon.RestLink()
            {
                Href = href
            };

            if (method != null)
            {
                newSubmitLink.Method = method;
            }

            if (errorCodeExpressions != null)
            {
                newSubmitLink.SetErrorCodeExpressions(errorCodeExpressions);
            }

            switch (headerType)
            {
                case GlobalConstants.SubmitActionHeaderTypes.Jarvis:
                    newSubmitLink.AddHeader(Constants.CustomHeaders.ApiVersion, Constants.ApiVersions.JarvisV3);
                    newSubmitLink.AddHeader(Constants.CustomHeaders.MsCorrelationId, Guid.NewGuid().ToString());
                    newSubmitLink.AddHeader(Constants.CustomHeaders.MsTrackingId, Guid.NewGuid().ToString());
                    break;

                default:
                    break;
            }

            // If there is next action then change the submitLink for the nextAction else first one.
            if (button != null && button.Action != null)
            {
                if (button.Action.NextAction != null)
                {
                    button.Action.NextAction.Context = newSubmitLink;
                }
                else
                {
                    button.Action.Context = newSubmitLink;
                }
            }
        }
    }
}