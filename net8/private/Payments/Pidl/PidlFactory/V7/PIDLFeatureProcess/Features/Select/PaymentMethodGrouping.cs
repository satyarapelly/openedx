// <copyright file="PaymentMethodGrouping.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;

    /// <summary>
    /// Class representing the PaymentMethodGroupingFeature, which is to group PM and append subpages for each PM group.
    /// </summary>
    internal class PaymentMethodGrouping : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            // The paymentMethodGrouping feature is only to group payment methods in select pm flow. 
            // SelectMS rewards do not have to enter this block of code. 
            // The paymentMethodGrouping feature is not applicable to the SelectPMDropDown template as no grouping is required for dropdown.
            if (featureContext?.ResourceType == Constants.ResourceTypes.Rewards
                || string.Equals(featureContext?.Partner, Constants.TemplateName.SelectPMDropDown, StringComparison.OrdinalIgnoreCase)
                || string.Equals(featureContext?.OriginalPartner, Constants.TemplateName.SelectPMDropDown, StringComparison.OrdinalIgnoreCase))
            {
                return new List<Action<List<PIDLResource>, FeatureContext>>();
            }

            if (PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(featureContext.Partner))
            {
                List<Action<List<PIDLResource>, FeatureContext>> retVal = new List<Action<List<PIDLResource>, FeatureContext>>()
                {
                    GroupPaymentMethod,
                    RemoveUnneededDataDescription
                };

                if (featureContext.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXSwapSelectPMPages) && CanSwapSelectPMPage(featureContext))
                {
                    retVal.Add(ShiftPMPageToFront);
                }

                if (featureContext.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXDisableRedeemCSVFlow))
                {
                    // Remove the ewallet stored value option from the select PM page if PXDisableRedeemCSVFlow is enabled
                    // ToDo: cleanup this logic once the feature is fully rolled out
                    retVal.Add(RemoveEwalletStoredValueOption);
                }

                if (featureContext.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableXboxNativeStyleHints))
                {
                    retVal.Add(PassStyleHintsForNativeSelectPM);
                }

                return retVal;
            }

            List<Action<List<PIDLResource>, FeatureContext>> actions = new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                AddClassName,
                ChangeDisplayType,
                GroupPaymentMethod,
                RemoveUnneededDataDescription,
                AddCancelButtonToHomePage
            };

            if (featureContext?.ExposedFlightFeatures != null && featureContext.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXSwapSelectPMPages) && CanSwapSelectPMPage(featureContext))
            {
                actions.Add(ShiftPMPageToFront);
            }

            return actions;
        }

        internal static void RemoveEwalletStoredValueOption(List<PIDLResource> pidlResources, FeatureContext actionParams)
        {
            foreach (PIDLResource pidlResource in pidlResources)
            {
                List<DisplayHint> selectOptions = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(pidlResource, Constants.DisplayHintIds.PaymentMethodSelect);

                foreach (DisplayHint hint in selectOptions)
                {
                    PropertyDisplayHint displayHint = hint as PropertyDisplayHint;
                    var possibleOptions = displayHint?.PossibleOptions;
                    if (possibleOptions != null)
                    {
                        possibleOptions.Remove(Constants.DisplayHintIds.EwalletStoredValueOption);
                    }

                    var possibleValues = displayHint?.PossibleValues;
                    if (possibleValues != null)
                    {
                        possibleValues.Remove(Constants.DisplayHintIds.EwalletStoredValueOption);
                    }
                }
            }
        }

        internal static void ShiftPMPageToFront(List<PIDLResource> pidlResources, FeatureContext postProcessingParams)
        {
            string pageId = GetPMFamilyByPageId(postProcessingParams?.PMGroupPageId);
            if (pidlResources == null || string.IsNullOrEmpty(pageId))
            {
                return;
            }

            for (int pos = 0; pos < pidlResources.Count; pos++)
            {
                PIDLResource resource = pidlResources.ElementAt(pos);

                if (resource?.DisplayPages != null && resource?.DisplayPages.Count > 1)
                {
                    int pagePosition = GetPagePositionForTargetedPageId(resource.DisplayPages, pageId);

                    if (pagePosition == -1)
                    {
                        continue;
                    }

                    // Remove the PM subpage from it's existing position and insert it at the front
                    PageDisplayHint paymentMethodPage = resource.DisplayPages.ElementAt(pagePosition);
                    resource.DisplayPages.RemoveAt(pagePosition);
                    resource.DisplayPages.Insert(0, paymentMethodPage);

                    // Update the back button action in subpages to navigate to the Select PM page which will be at index 1
                    List<DisplayHint> backButtonList = resource.GetAllDisplayHintsOfId(V7.Constants.DisplayHintIds.BackButton).Where(hint => hint is ButtonDisplayHint).ToList();

                    foreach (DisplayHint displayHint in backButtonList)
                    {
                        ButtonDisplayHint backButton = displayHint as ButtonDisplayHint;

                        if (backButton?.Action?.ActionType == DisplayHintActionType.moveFirst.ToString())
                        {
                            ChangeButtonActionToPageIndex(backButton, 1);
                        }
                    }

                    // Iterate through the PropertyDisplayHints in Home page and update the action to navigate to the respective payment method page
                    PageDisplayHint homePage = resource.DisplayPages.ElementAt(1);
                    List<DisplayHint> propertyDisplayHints = resource.GetAllDisplayHints(homePage).Where(hint => hint is PropertyDisplayHint).ToList();

                    foreach (DisplayHint displayHint in propertyDisplayHints)
                    {
                        PropertyDisplayHint paymentMethodGroup = displayHint as PropertyDisplayHint;
                        MapCorrectPageIdToPaymentMethodGroup(paymentMethodGroup, resource);
                    }
                }
            }
        }

        internal static void AddClassName(List<PIDLResource> inputResources, FeatureContext actionParams)
        {
            foreach (PIDLResource pidlResource in inputResources)
            {
                var displayHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodSelect) as PropertyDisplayHint;
                string className = string.Format("{0}_{1}", V7.Constants.DisplayHintIds.PaymentMethodSelect, V7.Constants.ScenarioNames.PMGrouping);
                displayHint.AddDisplayTag(className, className);
            }
        }

        internal static void ChangeDisplayType(List<PIDLResource> inputResources, FeatureContext actionParams)
        {
            foreach (PIDLResource pidlResource in inputResources)
            {
                var displayHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodSelect) as PropertyDisplayHint;
                displayHint.SelectType = Constants.PaymentMethodSelectType.ButtonList;

                // TODO: Task 54000775: The Webblends partner check below can be removed once the migration is complete and webblends starts using the `selectpmbuttonlist` partner. The `selectpmbuttonlist` already uses `displayId` as the `propertyName`, and its action is null.
                // Updating the property name and action to match with the payment method grouping.
                // Property name - it is used to navigate to second page (EWallet - has two option)
                // Action - payment method grouping does not need submit action group
                if (actionParams.Partner.Equals(Constants.PartnerNames.Webblends, StringComparison.OrdinalIgnoreCase))
                {
                    displayHint.PropertyName = "displayId";
                    displayHint.Action = null;
                }
            }
        }

        internal static void GroupPaymentMethod(List<PIDLResource> inputResources, FeatureContext postProcessingParams)
        {
            foreach (PIDLResource pidlResource in inputResources)
            {
                PopulatePaymentMethodButtonOptionsByPaymentMethodGroup(
                    pidlResource,
                    postProcessingParams.PaymentMethods,
                    postProcessingParams.Language,
                    postProcessingParams.Country,
                    PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(postProcessingParams.OriginalPartner) ? postProcessingParams.OriginalPartner : postProcessingParams.Partner,
                    postProcessingParams.Scenario,
                    postProcessingParams.ExposedFlightFeatures,
                    postProcessingParams.FeatureConfigs);
            }
        }

        internal static void RemoveUnneededDataDescription(List<PIDLResource> inputResources, FeatureContext actionParams)
        {
            foreach (PIDLResource pidlResource in inputResources)
            {
                string[] propertyIdsToBemodified = new string[]
                {
                    Constants.DataDescriptionIds.Id,
                    Constants.DataDescriptionIds.DisplayId,
                    Constants.DataDescriptionIds.PaymentInstrumentFamily,
                    Constants.DataDescriptionIds.PaymentInstrumentType,
                    Constants.DataDescriptionIds.PaymentInstrumentAction
                };

                foreach (string propertyId in propertyIdsToBemodified)
                {
                    object propertyDescriptionObject;
                    if (pidlResource.DataDescription.TryGetValue(propertyId, out propertyDescriptionObject))
                    {
                        var propertyDescription = propertyDescriptionObject as PropertyDescription;
                        if (propertyDescription != null)
                        {
                            propertyDescription.RemovePossibleValues();
                            propertyDescription.IndexedOn = null;
                            propertyDescription.DefaultValue = null;
                        }
                    }
                }
            }
        }

        internal static void PassStyleHintsForNativeSelectPM(List<PIDLResource> inputResources, FeatureContext actionParams)
        {
            if (inputResources == null)
            {
                return;
            }

            Dictionary<string, List<string>> styleHints = new Dictionary<string, List<string>>()
            {
                { Constants.DisplayHintIds.PaymentMethodSelect, new List<string>() { "layout-inline", "alignment-vertical-center", "padding-start-x-small" } },
                { Constants.GroupDisplayHintIds.PaymentMethodColumnGroup, new List<string>() { "height-fill" } },
                { Constants.GroupDisplayHintIds.PaymentOptionsGroup, null },
                { Constants.GroupDisplayHintIds.PaymentOptionTextGroup, new List<string>() { "alignment-horizontal-center" } },
                { Constants.GroupDisplayHintIds.MultiplePaymentMethodLogosRowOneGroup, new List<string>() { "height-fill", "flex-wrap", "alignment-content-space-between" } },
                { Constants.GroupDisplayHintIds.AlternativeSvgLogoWrapper, new List<string>() { "margin-bottom-medium" } },
                { Constants.DisplayHintIdPrefixes.PaymentMethodOption, new List<string>() { "width-fill", "height-fill", "margin-vertical-large" } },
                { Constants.TextDisplayHintIds.PaymentOptionText, new List<string>() { "text-bold", "text-alignment-center" } },
                { Constants.TextDisplayHintIds.PlusMore, new List<string>() { "text-bold", "font-size-small", "text-alignment-center" } },
                { Constants.DisplayHintIds.CancelGroup, new List<string>() { } },
                { Constants.ButtonDisplayHintIds.CancelButton, new List<string>() { "width-small" } },
                { Constants.ButtonDisplayHintIds.BackButton, new List<string>() { "width-small" } },
            };

            foreach (PIDLResource pidlResource in inputResources)
            {
                if (pidlResource?.DisplayPages == null)
                {
                    continue;
                }

                foreach (KeyValuePair<string, List<string>> styleHintMapper in styleHints)
                {
                    List<DisplayHint> displayHints = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(pidlResource, styleHintMapper.Key);
                    foreach (DisplayHint hint in displayHints)
                    {
                        hint.StyleHints = styleHintMapper.Value;
                    }
                }

                List<DisplayHint> selectOptions = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(pidlResource, Constants.DisplayHintIds.PaymentMethodSelect);

                foreach (DisplayHint hint in selectOptions)
                {
                    var possibleOptions = (hint as PropertyDisplayHint)?.PossibleOptions;
                    if (possibleOptions != null)
                    {
                        foreach (var option in possibleOptions)
                        {
                            if (option.Value != null)
                            {
                                // Stylehints for select option buttons
                                option.Value.StyleHints = Constants.NativeStyleHints.SelectPMOptionStyleHints;
                                AddStyleHintsToPaymentMethodOptionContent(option.Value.DisplayContent, pidlResource);
                            }
                        }
                    }
                }
            }
        }

        internal static void AddCancelButtonToHomePage(List<PIDLResource> inputResources, FeatureContext actionParams)
        {
            if (inputResources == null || inputResources.Count == 0 || actionParams?.FeatureConfigs == null)
            {
                return;
            }

            FeatureConfig featureConfig;
            actionParams.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.PaymentMethodGrouping, out featureConfig);
            if (featureConfig?.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayHintCustomizationDetail?.AddCancelButtonToHomePage == true)
                    {
                        foreach (PIDLResource resource in inputResources)
                        {
                            if (resource?.DisplayPages == null || resource.DisplayPages.Count == 0)
                            {
                                continue;
                            }

                            PageDisplayHint homePage = resource.DisplayPages.FirstOrDefault();
                            GroupDisplayHint cancelGroup = FeatureHelper.CreateGroupDisplayHint(Constants.DisplayHintIds.CancelGroup, Constants.LayoutOrientations.Inline, true);
                            string buttonContent = PidlModelHelper.GetLocalizedString(Constants.AccessibilityLabels.Cancel, actionParams.Language);
                            ButtonDisplayHint cancelButton = new ButtonDisplayHint
                            {
                                HintId = Constants.DisplayHintIds.CancelButton,
                                DisplayContent = buttonContent,
                                DisplayTags = new Dictionary<string, string>
                                {
                                    { Constants.DisplayTagKeys.AccessibilityName, buttonContent }
                                },
                                Action = new DisplayHintAction
                                {
                                    ActionType = DisplayHintActionType.gohome.ToString()
                                }
                            };

                            cancelGroup.Members.Add(cancelButton);
                            homePage?.AddDisplayHint(cancelGroup);
                        }
                    }
                }
            }
        }

        internal static void AddStyleHintsToPaymentMethodOptionContent(GroupDisplayHint paymentMethodOption, PIDLResource pidlResource)
        {
            if (paymentMethodOption == null || paymentMethodOption.Members == null)
            {
                return;
            }

            GroupDisplayHint paymentOptionLogosGroup = pidlResource.GetDisplayHintFromContainer(paymentMethodOption, Constants.GroupDisplayHintIds.MultiplePaymentMethodLogosRowOneGroup) as GroupDisplayHint;
            if (paymentOptionLogosGroup?.Members != null)
            {
                foreach (DisplayHint displayHint in paymentOptionLogosGroup.Members)
                {
                    ImageDisplayHint imageDisplayHint = displayHint as ImageDisplayHint;
                    GroupDisplayHint groupDisplayHint = displayHint as GroupDisplayHint;
                    List<string> logoStyleHints = new List<string>() { "image-small", "margin-end-none" };

                    // We either have a image display hint or a group display hint with image display hint as member
                    if (imageDisplayHint != null)
                    {
                        logoStyleHints.Add("margin-bottom-medium");
                        if (FeatureHelper.IsMediumWidthLogo(imageDisplayHint.HintId))
                        {
                            logoStyleHints.Add("image-medium");
                            paymentOptionLogosGroup.StyleHints = paymentOptionLogosGroup.StyleHints.Concat(new List<string>() { "alignment-horizontal-center" });
                        }
                    }
                    else if (groupDisplayHint != null && groupDisplayHint.Members != null && groupDisplayHint.HintId.StartsWith("alternativeSvgLogoWrapper_"))
                    {
                        imageDisplayHint = groupDisplayHint.Members.FirstOrDefault() as ImageDisplayHint;
                        if (imageDisplayHint != null && FeatureHelper.IsMediumWidthLogo(imageDisplayHint.HintId))
                        {
                            logoStyleHints.Add("image-medium");
                        }
                    }

                    if (imageDisplayHint != null)
                    {
                        imageDisplayHint.StyleHints = logoStyleHints;
                    }
                }
            }
        }

        internal static bool CanSwapSelectPMPage(FeatureContext featureContext)
        {
            if (string.IsNullOrEmpty(featureContext?.PMGroupPageId))
            {
                return false;
            }

            if (featureContext.PMGroupPageId.Contains("."))
            {
                string[] parts = featureContext.PMGroupPageId.Split('.');
                if (parts.Length > 2)
                {
                    return false;
                }
                else if (parts.Length == 2)
                {
                    // stored_value PI isn't grouped with other ewallet types. This condition is to show first page of SelectPM pidl instead of Ewallet subpage on cancelling the gift card flow.
                    string pmType = parts[1];
                    if (!string.IsNullOrEmpty(pmType) && string.Equals(pmType, Constants.PaymentMethodTypeNames.StoredValue, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static int GetPagePositionForTargetedPageId(List<PageDisplayHint> displayPages, string pageId)
        {
            if (displayPages == null)
            {
                return -1;
            }

            for (int position = 0; position < displayPages.Count; position++)
            {
                PageDisplayHint page = displayPages[position];
                if (page.HintId.Contains(pageId))
                {
                    return position;
                }
            }

            return -1;
        }

        private static void ChangeButtonActionToPageIndex(ButtonDisplayHint button, int pageIndex)
        {
            if (button == null)
            {
                return;
            }

            button.Action = new DisplayHintAction(DisplayHintActionType.moveToPageIndex.ToString());
            button.Action.Context = new PXCommon.MoveToPageIndexActionContext()
            {
                PageIndex = pageIndex
            };
        }

        private static void MapCorrectPageIdToPaymentMethodGroup(PropertyDisplayHint paymentMethodGroup, PIDLResource resource)
        {
            if (paymentMethodGroup == null)
            {
                return;
            }

            Dictionary<string, SelectOptionDescription> possibleOptions = paymentMethodGroup?.PossibleOptions;

            if (possibleOptions != null && possibleOptions.Count > 0)
            {
                foreach (KeyValuePair<string, SelectOptionDescription> possibleOption in possibleOptions)
                {
                    SelectOptionDescription paymentMethodOption = possibleOption.Value;
                    int paymentMethodPageId = GetPagePositionForTargetedPageId(resource.DisplayPages, possibleOption.Key);

                    if (paymentMethodOption != null
                        && paymentMethodPageId != -1
                        && paymentMethodOption.PidlAction?.ActionType == DisplayHintActionType.moveToPageIndex.ToString())
                    {
                        paymentMethodOption.PidlAction.Context = new PXCommon.MoveToPageIndexActionContext()
                        {
                            PageIndex = paymentMethodPageId
                        };
                    }
                }
            }
        }

        private static void PopulatePaymentMethodButtonOptionsByPaymentMethodGroup(
            PIDLResource pidlResource,
            HashSet<PaymentMethod> paymentMethods,
            string language,
            string country,
            string partnerName,
            string scenario = null,
            List<string> exposedFlightFeatures = null,
            Dictionary<string, FeatureConfig> featureConfig = null)
        {
            Dictionary<string, string> cachedTypes = new Dictionary<string, string>();
            Dictionary<string, SelectOptionDescription> possibleOptions = new Dictionary<string, SelectOptionDescription>();
            Dictionary<string, string> possibleValues = new Dictionary<string, string>();
            PaymentMethod storedValue = null;
            Dictionary<string, PaymentMethodOption> paymentMethodOptions = GetPaymentMethodOptions(paymentMethods, country, partnerName, cachedTypes, exposedFlightFeatures);

            // Example of paymentMethodOptions:
            // key: credit_card_amex_discover_mc_visa | value: PaymentMethodOption.DisplayName = Credit card or debit card, PaymentMethodOption.IsGroup = false,  PaymentMethodOption.PaymentMethods = {visa, mc, discover, amex}
            // key: ewallet_stored_value | value: PaymentMethodOption.DisplayName = Redeem a gift card, PaymentMethodOption.IsGroup = false,  PaymentMethodOption.PaymentMethods = {storedvalue}
            // key: Online_Bank_Transfer | value: PaymentMethodOption.DisplayName = Online Bank Transfer, PaymentMethodOption.IsGroup = true,  PaymentMethodOption.PaymentMethods = {paysafecard, sofort}
            foreach (PaymentMethodOption paymentMethodOption in paymentMethodOptions.Values)
            {
                string displayId = paymentMethodOption.DisplayId;
                string displayText = paymentMethodOption.DisplayName;

                if (paymentMethodOption.IsGroup && paymentMethodOption.PaymentMethods.Count > 1)
                {
                    possibleValues.Add(displayId, displayText);
                    possibleOptions.Add(displayId, GetGroupedPaymentMethodSelectOption(paymentMethodOption.PaymentMethods, displayText, displayId, pidlResource, language, country, partnerName, scenario, exposedFlightFeatures, featureConfig: featureConfig));
                }
                else
                {
                    // if a group has only one PM or could be collapsed, then PX won't show the option as a group, instead will return success event with PM details directly
                    PaymentMethod method = paymentMethodOption.PaymentMethods.First();
                    if (method.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.StoredValue))
                    {
                        // Stored value must be added at the end of the possible values list
                        storedValue = method;
                    }
                    else
                    {
                        if (PaymentSelectionHelper.IsCollapsedPaymentMethodOption(method))
                        {
                            if (!possibleValues.ContainsKey(displayId))
                            {
                                possibleValues.Add(displayId, paymentMethodOption.DisplayName);
                                possibleOptions.Add(displayId, GetNonGroupedPaymentMethodSelectOption(paymentMethodOption.PaymentMethods, displayId, displayText, PaymentSelectionHelper.GetCommaSeparatedTypes(method.PaymentMethodFamily, paymentMethods, cachedTypes), language, country, partnerName, scenario, featureConfig: featureConfig, exposedFlightFeatures: exposedFlightFeatures));
                            }
                        }
                        else
                        {
                            possibleValues.Add(displayId, paymentMethodOption.DisplayName);
                            possibleOptions.Add(displayId, GetNonGroupedPaymentMethodSelectOption(paymentMethodOption.PaymentMethods, displayId, displayText, method.PaymentMethodType, language, country, partnerName, scenario, featureConfig: featureConfig, exposedFlightFeatures: exposedFlightFeatures));
                        }
                    }
                }
            }

            // Add stored value at the end of the possible values list
            if (storedValue != null)
            {
                string displayId = PaymentSelectionHelper.GetPaymentMethodFamilyTypeDisplayId(storedValue);
                string displayText = PaymentSelectionHelper.GetPaymentMethodDisplayText(storedValue, country);

                possibleValues.Add(displayId, displayText);
                possibleOptions.Add(displayId, GetNonGroupedPaymentMethodSelectOption(new HashSet<PaymentMethod> { storedValue }, PaymentSelectionHelper.GetPaymentMethodFamilyTypeId(storedValue), displayText, storedValue.PaymentMethodType, language, country, partnerName, scenario, featureConfig: featureConfig, exposedFlightFeatures: exposedFlightFeatures));
            }

            PropertyDisplayHint paymentMethod = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodSelect) as PropertyDisplayHint;
            paymentMethod.SetPossibleOptions(possibleOptions);

            if (PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(partnerName))
            {
                PaymentSelectionHelper.AddXboxNativeSelectOptionAccessibilityTag(paymentMethod);
                paymentMethod.PropertyName = Constants.DataDescriptionIds.DisplayId;
            }
            else
            {
                PaymentSelectionHelper.AddSelectOptionAcessibilityTag(paymentMethod);

                HeadingDisplayHint headingDisplayHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodSelectHeading) as HeadingDisplayHint;

                // if original heading not found then looking for PM grouping related heading.
                if (headingDisplayHint == null)
                {
                    headingDisplayHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodPMGroupingSelectHeading) as HeadingDisplayHint;
                }
            }

            // moveToPageIndex action won't work if isOptional is false, because group option doesn't return a value in moveToPageIndex action.
            PropertyDescription propertyDescription = pidlResource.GetPropertyDescriptionByPropertyName(Constants.DataDescriptionIds.DisplayId);
            propertyDescription.IsOptional = true;
        }

        private static SelectOptionDescription GetNonGroupedPaymentMethodSelectOption(HashSet<PaymentMethod> methods, string id, string displayText, string type, string language, string country, string partnerName, string scenario, bool isSubPage = false, Dictionary<string, FeatureConfig> featureConfig = null, List<string> exposedFlightFeatures = null)
        {
            SelectOptionDescription selectOption;
            if (PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(partnerName))
            {
                int maxAllowedLogos = GetMaxAllowedLogos(partnerName);
                selectOption = BuildNativeSelectOption(methods, displayText, partnerName, maxAllowedLogos, scenario, isSubPage, featureConfig: featureConfig, exposedFlightFeatures: exposedFlightFeatures);
            }
            else
            {
                selectOption = BuildSelectOption(methods, id, displayText, language, country, partnerName, scenario, featureConfig);
            }

            ActionContext optionContext = new ActionContext()
            {
                Action = PaymentInstrumentActions.ToString(PIActionType.AddResource),
                Id = id,
                PaymentMethodFamily = methods.First().PaymentMethodFamily,
                PaymentMethodType = type
            };

            if (PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(partnerName))
            {
                string paymentMethodFamily = methods.First().PaymentMethodFamily;
                ResourceActionContext resourceActionContext = new ResourceActionContext()
                {
                    Action = PaymentInstrumentActions.ToString(PIActionType.AddResource),
                    PidlDocInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentInstrumentDescription, language, country, partnerName, type, paymentMethodFamily),
                };
                optionContext.ResourceActionContext = resourceActionContext;
            }

            selectOption.PidlAction = PaymentSelectionHelper.CreateSuccessPidlAction(optionContext, false);
            return selectOption;
        }

        private static SelectOptionDescription GetGroupedPaymentMethodSelectOption(HashSet<PaymentMethod> groupedPaymentMethods, string displayText, string groupDisplayId, PIDLResource pidlResource, string language, string country, string partnerName, string scenario, List<string> exposedFlightFeatures = null, Dictionary<string, FeatureConfig> featureConfig = null)
        {
            SelectOptionDescription selectOption;
            string paymentMethodFamily = string.Empty;

            if (PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(partnerName))
            {
                // Cup cards need "UnionPay" as part of the displaytext
                PaymentMethod firstPaymentMethod = groupedPaymentMethods.First();
                paymentMethodFamily = firstPaymentMethod.PaymentMethodFamily;

                if (PaymentSelectionHelper.IsCup(firstPaymentMethod))
                {
                    displayText = PidlModelHelper.GetLocalizedString(Constants.UnlocalizedDisplayText.XboxNativeCupCardsDisplayText);
                }

                int maxAllowedLogos = GetMaxAllowedLogos(partnerName);
                selectOption = BuildNativeSelectOption(groupedPaymentMethods, displayText, partnerName, maxAllowedLogos, scenario, featureConfig: featureConfig);
            }
            else
            {
                // Retrieve payment method family name to be used later for appending it to subpage hintId.
                // When we fallback to Select flow from Add flow, this helps to identify the subpage for a specific payment method family and display it as first page
                if (exposedFlightFeatures?.Contains(Constants.PartnerFlightValues.PXSwapSelectPMPages) == true)
                {
                    paymentMethodFamily = groupedPaymentMethods?.FirstOrDefault()?.PaymentMethodFamily ?? string.Empty;
                }

                selectOption = BuildSelectOption(groupedPaymentMethods, groupDisplayId, displayText, language, country, partnerName, scenario, featureConfig: featureConfig);
            }

            // selectOption of a PM group should naviage the user to a sub page containing all indiviual PMs belonging this the PM group
            selectOption.PidlAction = new DisplayHintAction(DisplayHintActionType.moveToPageIndex.ToString());
            selectOption.PidlAction.Context = new PXCommon.MoveToPageIndexActionContext()
            {
                PageIndex = pidlResource.DisplayPages.Count
            };

            AppendSubPageForPaymentMethodGroup(pidlResource, groupedPaymentMethods, language, country, partnerName, scenario, displayText, groupDisplayId, exposedFlightFeatures, featureConfig: featureConfig, paymentMethodFamily: paymentMethodFamily);

            return selectOption;
        }

        private static SelectOptionDescription BuildNativeSelectOption(HashSet<PaymentMethod> paymentMethods, string displayText, string partnerName, int maxAllowedLogosPerOption, string scenario = null, bool isSubPage = false, List<string> exposedFlightFeatures = null, Dictionary<string, FeatureConfig> featureConfig = null)
        {
            // null safety
            if (featureConfig == null)
            {
                featureConfig = new Dictionary<string, FeatureConfig>();
            }

            FeatureConfig configs;
            featureConfig.TryGetValue(FeatureConfiguration.FeatureNames.PaymentMethodGrouping, out configs);

            PaymentMethod firstPaymentMethod = paymentMethods.First();

            TextDisplayHint paymentOptionText = new TextDisplayHint() { HintId = Constants.TextDisplayHintIds.PaymentOptionText };
            paymentOptionText.DisplayContent = (paymentMethods.Count == 1) ? PaymentSelectionHelper.GetPaymentMethodDisplayText(firstPaymentMethod) : displayText;
            paymentOptionText.AddDisplayTag(Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Polite);

            GroupDisplayHint paymentOptionTextGroup = FeatureHelper.CreateGroupDisplayHint(Constants.GroupDisplayHintIds.PaymentOptionTextGroup);
            paymentOptionTextGroup.AddDisplayHint(paymentOptionText);

            GroupDisplayHint paymentOptionLogosGroup = FeatureHelper.CreateGroupDisplayHint(Constants.GroupDisplayHintIds.MultiplePaymentMethodLogosRowOneGroup, Constants.PartnerHintsValues.InlinePlacement);

            foreach (PaymentMethod paymentMethod in paymentMethods.Take(maxAllowedLogosPerOption))
            {
                string alternateSvgForPartner = PaymentSelectionHelper.CheckForReactNativeAlternatePaymentMethodLogoUrl(paymentMethod, partnerName, exposedFlightFeatures);
                string displayId = PaymentSelectionHelper.GetPaymentMethodFamilyTypeDisplayId(paymentMethod);

                // use png for paysafecard logo
                if (paymentMethod.PaymentMethodType.Equals(Constants.PaymentMethodTypeNames.Paysafecard))
                {
                    alternateSvgForPartner = $"{Constants.PidlUrlConstants.StaticResourceServiceImagesV4}/{Constants.StaticResourceNames.PaysafeCardPng}";
                }

                string hintId = isSubPage ? string.Format(Constants.ImageDisplayHintIds.XboxNativeSelectPMSubPageLogoTemplate, displayId) : string.Format(Constants.ImageDisplayHintIds.XboxNativeSelectPMLogoTemplate, displayId);

                // If multiple logos are possible, we need to give a special id when only a single logo is present for react-native styling purposes.
                if (paymentMethods.Count == 1)
                {
                    bool canHaveMultipleLogos = CanPaymentMethodFamilyHaveMultipleLogos(paymentMethod);

                    if (canHaveMultipleLogos)
                    {
                        hintId = string.Format(Constants.ImageDisplayHintIds.XboxNativeSelectPMSingleLogoTemplate, hintId);
                    }
                }

                ImageDisplayHint paymentOptionLogo = new ImageDisplayHint
                {
                    HintId = hintId,
                    SourceUrl = alternateSvgForPartner ?? PaymentSelectionHelper.GetPaymentMethodLogoUrl(paymentMethod),
                    AccessibilityName = PidlModelHelper.GetLocalizedString($"{paymentMethod.Display.Name} logo,")
                };

                if (alternateSvgForPartner != null)
                {
                    // alternate svg don't scale well so we need a group wrapper to allow a background colored rectangle matching the size of the standard logos
                    GroupDisplayHint alternativeSvgWrapper = FeatureHelper.CreateGroupDisplayHint($"{Constants.GroupDisplayHintIds.AlternativeSvgLogoWrapper}{hintId}");

                    if (configs != null && configs.DisplayCustomizationDetailEnabled(Constants.DisplayCustomizationDetail.RemoveDefaultStyleHints))
                    {
                        alternativeSvgWrapper.StyleHints = new List<string>() { };
                    }

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

                plusMore.AddDisplayTag(Constants.DisplayTagKeys.NoPidlddc, Constants.DisplayTags.Polite);

                GroupDisplayHint multiLogosRowTwoGroup = FeatureHelper.CreateGroupDisplayHint(Constants.GroupDisplayHintIds.MultiplePaymentMethodLogosRowOneGroup, Constants.PartnerHintsValues.InlinePlacement);
                multiLogosRowTwoGroup.AddDisplayHint(plusMore);

                paymentOptionLogosGroup.AddDisplayHint(multiLogosRowTwoGroup);
            }

            GroupDisplayHint paymentMethodOption = FeatureHelper.CreateGroupDisplayHint(string.Format("{0}_{1}", Constants.GroupDisplayHintIds.PaymentMethodOption, firstPaymentMethod.PaymentMethodType));

            List<DisplayHint> paymentMethodOptionMembers = null;

            // for SelectPMWithLogo scenario, paymentOptionTextGroup is not needed because it is redundant with displayText
            if (configs != null && configs.DisplayCustomizationDetailEnabled(Constants.DisplayCustomizationDetail.SetSelectPMWithLogo))
            {
                paymentMethodOptionMembers = new List<DisplayHint> { paymentOptionLogosGroup };
            }
            else if (configs != null && configs.DisplayCustomizationDetailEnabled(Constants.DisplayCustomizationDetail.SetGroupedSelectOptionTextBeforeLogo))
            {
                paymentMethodOptionMembers = new List<DisplayHint> { paymentOptionTextGroup, paymentOptionLogosGroup };
            }
            else
            {
                paymentMethodOptionMembers = new List<DisplayHint> { paymentOptionLogosGroup, paymentOptionTextGroup };
            }

            paymentMethodOption.AddDisplayHints(paymentMethodOptionMembers);

            SelectOptionDescription selectOption = new SelectOptionDescription { DisplayContent = paymentMethodOption };
            selectOption.AccessibilityTag = paymentOptionText.DisplayContent;
            selectOption.DisplayText = paymentOptionText.DisplayContent;

            return selectOption;
        }

        private static SelectOptionDescription BuildSelectOption(HashSet<PaymentMethod> methods, string id, string displayText, string language, string country, string partnerName, string scenario, Dictionary<string, FeatureConfig> featureConfig = null)
        {
            SelectOptionDescription selectOption = new SelectOptionDescription { DisplayText = displayText };

            selectOption.AccessibilityTag = displayText;
            selectOption.DisplayContent = FeatureHelper.CreateGroupDisplayHint(Constants.DisplayHintIdPrefixes.PaymentOptionContainer + id, Constants.PartnerHintsValues.InlinePlacement);

            selectOption.DisplayContent.AddDisplayTag(V7.Constants.DisplayTag.PiContainer, V7.Constants.DisplayTag.PiContainer);

            selectOption.StyleHints = new List<string>() { "width-fill", "content-align-start", "padding-small" };

            IEnumerable<DisplayHint> groupDisplayHints = PIDLResourceDisplayHintFactory.Instance.GetDisplayHints(partnerName, V7.Constants.GroupDisplayHintIds.PaymentOptionDisplayGroup, country, null, null);
            GroupDisplayHint groupDisplayHint = groupDisplayHints.FirstOrDefault() as GroupDisplayHint;
            if (groupDisplayHint != null)
            {
                groupDisplayHint.HintId = Constants.DisplayHintIdPrefixes.PaymentOptionTextGroup + id;
                if (groupDisplayHint.Members.Count > 0)
                {
                    TextGroupDisplayHint textGroup = groupDisplayHint.Members[0] as TextGroupDisplayHint;
                    textGroup.HintId = Constants.DisplayHintIdPrefixes.PaymentOptionTextGroup + id;
                    if (textGroup.Members.Count > 0)
                    {
                        TextDisplayHint textDisplayHint = textGroup.Members[0] as TextDisplayHint;
                        textDisplayHint.DisplayContent = displayText;
                        textDisplayHint.HintId = Constants.DisplayHintIdPrefixes.PaymentOptionText + id;
                        textDisplayHint.StyleHints = new List<string>() { "content-align-start" };
                    }
                }
            }

            GroupDisplayHint logoGroup = FeatureHelper.CreateGroupDisplayHint(Constants.DisplayHintIdPrefixes.PaymentMethodLogoContainer + id);

            foreach (PaymentMethod method in methods)
            {
                ImageDisplayHint logo = new ImageDisplayHint
                {
                    HintId = $"{V7.Constants.DisplayHintIdPrefixes.PaymentOptionLogo}{scenario}_{PaymentSelectionHelper.GetPaymentMethodFamilyTypeDisplayId(method)}",
                    SourceUrl = PaymentSelectionHelper.GetPaymentMethodLogoUrl(method),
                    AccessibilityName = method.Display.Name,
                    StyleHints = new List<string>() { "image-height-small" }
                };

                logoGroup.Members.Add(logo);
            }

            if (featureConfig == null)
            {
                featureConfig = new Dictionary<string, FeatureConfig>();
            }

            FeatureConfig configs;
            featureConfig.TryGetValue(FeatureConfiguration.FeatureNames.PaymentMethodGrouping, out configs);

            string logoGroupClassName = V7.Constants.DisplayHintIdPrefixes.PaymentMethodLogoContainer + V7.Constants.PartnerFlightValues.EnablePaymentMethodGrouping;
            groupDisplayHint.StyleHints = new List<string>() { "width-fill" };
            logoGroup.AddDisplayTag(logoGroupClassName, logoGroupClassName);
            logoGroup.StyleHints = new List<string>() { "gap-small", "direction-horizontal" };
            selectOption.DisplayContent.StyleHints = new List<string>() { "width-fill", "gap-small" };

            if (configs != null && configs.DisplayCustomizationDetailEnabled(Constants.DisplayCustomizationDetail.SetGroupedSelectOptionTextBeforeLogo))
            {
                selectOption.DisplayContent.Members.Add(groupDisplayHint);
                selectOption.DisplayContent.Members.Add(logoGroup);
            }
            else
            {
                selectOption.DisplayContent.Members.Add(logoGroup);
                selectOption.DisplayContent.Members.Add(groupDisplayHint);
            }

            return selectOption;
        }

        private static void AppendSubPageForPaymentMethodGroup(PIDLResource pidlResource, HashSet<PaymentMethod> methods, string language, string country, string partnerName, string scenario, string subPageHeading, string groupDisplayId, List<string> exposedFlightFeatures = null, Dictionary<string, FeatureConfig> featureConfig = null, string paymentMethodFamily = null)
        {
            if (paymentMethodFamily != null && paymentMethodFamily.Length > 0)
            {
                // Concatenate payment method family name to groupDisplayId which helps to identify the subpage for a specific payment method family
                // When we fallback to Select flow from Add flow, this helps to identify the subpage for a specific payment method family and display it as first page
                groupDisplayId = $"{groupDisplayId}_{paymentMethodFamily}";
            }

            PageDisplayHint paymentMethodPage = new PageDisplayHint
            {
                // TODO: the following task is created to calculate hintId using an utility function
                // Task 41043452: [PX Refactor] create an utility function to calculate hintId for element
                HintId = V7.Constants.DisplayHintIdPrefixes.PaymentMethodSubGroupPage + groupDisplayId,
                ContainerDisplayType = V7.Constants.DisplayType.Page,
                StyleHints = new List<string> { "height-fill" }
            };

            GroupDisplayHint paymentMethodColumnGroup = new GroupDisplayHint
            {
                HintId = V7.Constants.GroupDisplayHintIds.PaymentMethodColumnGroup,
                ContainerDisplayType = V7.Constants.DisplayType.Group,
            };

            subPageHeading = PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(partnerName) ? GetSubPageHeadingForXboxNative(subPageHeading) : subPageHeading;
            HeadingDisplayHint headingDisplayHint = new HeadingDisplayHint()
            {
                HintId = V7.Constants.DisplayHintIdPrefixes.PaymentMethodSubGroupPageHeading + groupDisplayId,
                DisplayContent = subPageHeading,
            };

            if (PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(partnerName))
            {
                headingDisplayHint.AddDisplayTag(Constants.DiplayHintProperties.AccessibilityName, subPageHeading);
            }

            // logos in the subpage need this flag to have a distinct id for styling purposes
            bool isSubPage = true;
            PropertyDisplayHint paymentMethodDisplayHint = GetPaymentMethodDisplayHint(methods, language, country, partnerName, scenario, isSubPage, featureConfig: featureConfig, exposedFlightFeatures: exposedFlightFeatures);

            GroupDisplayHint paymentOptionsGroup = new GroupDisplayHint
            {
                HintId = V7.Constants.GroupDisplayHintIds.PaymentOptionsGroup,
                ContainerDisplayType = V7.Constants.DisplayType.Group,
            };

            ButtonDisplayHint cancelButton = new ButtonDisplayHint()
            {
                HintId = V7.Constants.DisplayHintIds.BackButton,
                Action = new DisplayHintAction(DisplayHintActionType.moveFirst.ToString()),
                DisplayContent = PidlModelHelper.GetLocalizedString(Constants.AccessibilityLabels.Back),
                StyleHints = new List<string>() { "width-fill-large" }
            };

            cancelButton.AddDisplayTag(Constants.DiplayHintProperties.AccessibilityName, PidlModelHelper.GetLocalizedString(Constants.AccessibilityLabels.Back));

            if (featureConfig == null)
            {
                featureConfig = new Dictionary<string, FeatureConfig>();
            }

            FeatureConfig configs;
            featureConfig.TryGetValue(FeatureConfiguration.FeatureNames.PaymentMethodGrouping, out configs);

            if (configs != null && configs.DisplayCustomizationDetailEnabled(Constants.DisplayCustomizationDetail.MatchSelectPMMainPageStructureForSubPage))
            {
                paymentMethodColumnGroup.AddDisplayHint(headingDisplayHint);
                paymentOptionsGroup.AddDisplayHint(paymentMethodDisplayHint);
                paymentMethodColumnGroup.AddDisplayHint(paymentOptionsGroup);
            }
            else
            {
                paymentMethodPage.AddDisplayHint(headingDisplayHint);
                paymentMethodPage.AddDisplayHint(paymentMethodDisplayHint);
            }

            if (configs != null && configs.DisplayCustomizationDetailEnabled(Constants.DisplayCustomizationDetail.MatchSelectPMMainPageStructureForSubPage))
            {
                GroupDisplayHint cancelGroup = FeatureHelper.CreateGroupDisplayHint(V7.Constants.DisplayHintIds.CancelBackGroup, V7.Constants.PartnerHintsValues.InlinePlacement, true);
                cancelGroup.AddDisplayHint(cancelButton);
                paymentMethodColumnGroup.AddDisplayHint(cancelGroup);
                paymentMethodPage.AddDisplayHint(paymentMethodColumnGroup);
            }
            else if (partnerName.Equals(Constants.PartnerNames.Webblends, StringComparison.OrdinalIgnoreCase)
                 || partnerName.Equals(Constants.TemplateName.SelectPMButtonList, StringComparison.OrdinalIgnoreCase)
                 || exposedFlightFeatures.Contains(Constants.PartnerFlightValues.EnablePMGroupingSubpageSubmitBlock, StringComparer.OrdinalIgnoreCase))
            {
                // This Webblends partner specific condition will be removed when we migrate to PSS
                // NorthStarWeb has already migrated to PSS and it also needs the submit group, hence the SelectPMButtonList condition.
                GroupDisplayHint submitButtonGroup = FeatureHelper.CreateGroupDisplayHint(V7.Constants.DisplayHintIds.CancelBackGroup, V7.Constants.PartnerHintsValues.InlinePlacement, true);
                submitButtonGroup.AddDisplayHint(cancelButton);
                submitButtonGroup.StyleHints = new List<string>() { "padding-vertical-medium", "gap-medium", "width-fill" };
                paymentMethodPage.AddDisplayHint(submitButtonGroup);
            }
            else
            {
                paymentMethodPage.AddDisplayHint(cancelButton);
            }

            pidlResource.DisplayPages.Add(paymentMethodPage);
        }

        private static Dictionary<string, PaymentMethodOption> GetPaymentMethodOptions(HashSet<PaymentMethod> paymentMethods, string country, string partnerName, Dictionary<string, string> cachedTypes, List<string> exposedFlightFeatures)
        {
            Dictionary<string, PaymentMethodOption> paymentMethodOptions = new Dictionary<string, PaymentMethodOption>();

            foreach (PaymentMethod method in paymentMethods)
            {
                bool shouldBeGrouped = IsGrouped(method, partnerName);

                string displayId = shouldBeGrouped ? method.PaymentMethodGroup : PaymentSelectionHelper.GetPaymentMethodFamilyTypeDisplayId(method);
                string groupDisplayName = shouldBeGrouped ? PidlModelHelper.GetLocalizedString(method.GroupDisplayName) : PaymentSelectionHelper.GetPaymentMethodDisplayText(method, country, true, exposedFlightFeatures);

                // If payment method types could be collapsed (e.g. visa.mc.discover.amex) in a single add PI form, then we will not show the collapsed PMs in a group and instead directly return the collpased PMs to partner.
                // So partner could use the collapsed PMs to load Add PI form 
                if (PaymentSelectionHelper.IsCollapsedPaymentMethodOption(method))
                {
                    string types = PaymentSelectionHelper.GetCommaSeparatedTypes(method.PaymentMethodFamily, paymentMethods, cachedTypes);
                    string id = string.Format("{0}.{1}", method.PaymentMethodFamily, types);
                    displayId = PaymentSelectionHelper.GetPaymentMethodFamilyTypeDisplayId(id);
                    groupDisplayName = PaymentSelectionHelper.GetPaymentMethodDisplayText(method, country, true, exposedFlightFeatures);
                    shouldBeGrouped = false;
                }

                if (!paymentMethodOptions.ContainsKey(displayId))
                {
                    PaymentMethodOption paymentMethodOption = new PaymentMethodOption(groupDisplayName, shouldBeGrouped, new HashSet<PaymentMethod>() { method }, displayId);
                    paymentMethodOptions.Add(displayId, paymentMethodOption);
                }
                else
                {
                    paymentMethodOptions[displayId].PaymentMethods.Add(method);
                }
            }

            // if a group has only one PM, then PX won't show the option as a group, instead will return success event with PM details directly.
            // Also need to change the displayName to not use GroupDisplayName and use the display name for the single PM.
            foreach (PaymentMethodOption paymentMethodOption in paymentMethodOptions.Values)
            {
                if (paymentMethodOption.IsGroup && paymentMethodOption.PaymentMethods.Count == 1)
                {
                    paymentMethodOption.DisplayId = PaymentSelectionHelper.GetPaymentMethodFamilyTypeDisplayId(paymentMethodOption.PaymentMethods.Single());
                }
            }

            return paymentMethodOptions;
        }

        private static PropertyDisplayHint GetPaymentMethodDisplayHint(HashSet<PaymentMethod> methods, string language, string country, string partnerName, string scenario, bool isSubPage = false, Dictionary<string, FeatureConfig> featureConfig = null, List<string> exposedFlightFeatures = null)
        {
            PropertyDisplayHint paymentMethodDisplayHint = new PropertyDisplayHint()
            {
                HintId = V7.Constants.DisplayHintIds.PaymentMethodSelect,
                PropertyName = V7.Constants.DataDescriptionIds.DisplayId,
                SelectType = V7.Constants.PaymentMethodSelectType.ButtonList,
                StyleHints = new List<string>() { "gap-small" }
            };

            string className = string.Format("{0}_{1}", Constants.DisplayHintIds.PaymentMethodSelect, Constants.ScenarioNames.PMGrouping);
            paymentMethodDisplayHint.AddDisplayTag(className, className);

            Dictionary<string, SelectOptionDescription> possibleOptions = new Dictionary<string, SelectOptionDescription>();
            Dictionary<string, string> possibleValues = new Dictionary<string, string>();

            string localizedFirstKeyDisplayText = null;
            foreach (PaymentMethod method in methods)
            {
                string displayId = PaymentSelectionHelper.GetPaymentMethodFamilyTypeDisplayId(method);
                string displayText = PaymentSelectionHelper.GetPaymentMethodDisplayText(method, country);

                if (localizedFirstKeyDisplayText == null)
                {
                    localizedFirstKeyDisplayText = displayText;
                }

                possibleValues.Add(displayId, displayText);
                possibleOptions.Add(displayId, GetNonGroupedPaymentMethodSelectOption(new HashSet<PaymentMethod> { method }, PaymentSelectionHelper.GetPaymentMethodFamilyTypeId(method), displayText, method.PaymentMethodType, language, country, partnerName, scenario, isSubPage, featureConfig: featureConfig, exposedFlightFeatures: exposedFlightFeatures));
            }

            paymentMethodDisplayHint.SetPossibleOptions(possibleOptions);

            if (PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(partnerName))
            {
                PaymentSelectionHelper.AddXboxNativeSelectOptionAccessibilityTag(paymentMethodDisplayHint);
            }
            else
            {
                paymentMethodDisplayHint.AddDisplayTag("accessibilityName", PidlModelHelper.GetLocalizedString("Choose a way to pay"));
            }

            return paymentMethodDisplayHint;
        }

        private static bool CanPaymentMethodFamilyHaveMultipleLogos(PaymentMethod paymentMethod)
        {
            return string.Equals(paymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(paymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.MobileBillingNonSim, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(paymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(paymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyNames.OnlineBankTransfer, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetSubPageHeadingForXboxNative(string heading)
        {
            string headingTemplate = "Choose {0} option";
            headingTemplate = PidlModelHelper.GetLocalizedString(headingTemplate);
            return headingTemplate.Replace("{0}", heading);
        }

        private static int GetMaxAllowedLogos(string partnerName)
        {
            Dictionary<string, int> maxAllowedLogosPerSelectOptionForPartner = PaymentSelectionHelper.GetMaxAllowedLogosPerSelectOptionForPartner();
            int maxAllowedLogos = maxAllowedLogosPerSelectOptionForPartner.ContainsKey(partnerName) ? maxAllowedLogosPerSelectOptionForPartner[partnerName] : Constants.MaxAllowedPaymentMethodLogos.Six;
            return maxAllowedLogos;
        }

        private static bool IsGrouped(PaymentMethod method, string partner)
        {
            // PaymentMethodGroup property could be null for PM returned from PIMS, we will only show PMs in group if PaymentMethodGroup is not null
            bool shouldBeGrouped = method.PaymentMethodGroup != null;

            // for xbox native partners stored_value PI should not be grouped
            if (PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(partner)
                && method.EqualByFamilyAndType(Constants.PaymentMethodFamilyNames.Ewallet, Constants.PaymentMethodTypeNames.StoredValue))
            {
                shouldBeGrouped = false;
            }

            return shouldBeGrouped;
        }

        private static string GetPMFamilyByPageId(string pmGroupPageId)
        {
            string pmFamily = string.Empty;
            if (!string.IsNullOrEmpty(pmGroupPageId))
            {
                if (pmGroupPageId.Contains("."))
                {
                    string[] parts = pmGroupPageId.Split('.');
                    if (parts.Length == 2)
                    {
                        pmFamily = parts[0];
                    }
                }
                else
                {
                    pmFamily = pmGroupPageId;
                }
            }

            return pmFamily;
        }
    }
}