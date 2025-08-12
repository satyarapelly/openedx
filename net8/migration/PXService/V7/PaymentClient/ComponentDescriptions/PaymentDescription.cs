// <copyright file="PaymentDescription.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Instruments;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.Extensions.DependencyInjection;

    public class PaymentDescription : ComponentDescription
    {
        private string pidlResourceIdSelectPM = "selectpm";
        private string pidlResourceIdSelectPI = "selectpi";
        private string pidlResourceIdSelectPINone = "selectpinone";
        private string pidlResourceIdDeletePI = "deletepi";
        private string pidlDescriptionType = V7.Constants.DescriptionTypes.PaymentMethodDescription;

        public IReadOnlyList<string> SelectInstanceDataDescriptions
        {
            get
            {
                List<string> possibelDataDescriptions = new List<string>()
                {
                    V7.Constants.PropertyDescriptionIds.SelectedPIID
                };

                possibelDataDescriptions.AddRange(OrderSummaryDescription.OrderSummaryDataDescriptions);
                possibelDataDescriptions.Remove(V7.Constants.PropertyDescriptionIds.CartSubtotal);
                possibelDataDescriptions.AddRange(AddressDescription.AddressDataDescriptions);

                // if compute tax is not enabled, remove tax and total property from the dataDescription
                if (!this.ShouldComputeTax())
                {
                    possibelDataDescriptions.Remove(V7.Constants.PropertyDescriptionIds.CartTax);
                    possibelDataDescriptions.Remove(V7.Constants.PropertyDescriptionIds.CartTotal);
                }

                return possibelDataDescriptions;
            }
        }

        public override string DescriptionType
        {
            get
            {
                return this.pidlDescriptionType;
            }
        }

        public static IList<PimsModel.V4.PaymentMethod> GetFilteredPaymentMethods(IList<PimsModel.V4.PaymentMethod> paymentMethods, string operation = null, string family = null, string type = null)
        {
            if (operation?.Equals(V7.Constants.Operations.Add, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                var filteredPMs = paymentMethods?.Where(pm => pm.PaymentMethodFamily.Equals(family, StringComparison.OrdinalIgnoreCase))?.ToList();

                var paymentMethodType = type?.Split(',')?.ToList();
                return filteredPMs?.Where(pm => paymentMethodType?.Contains(pm.PaymentMethodType, StringComparer.OrdinalIgnoreCase) ?? true)?.ToList();
            }

            return paymentMethods?.Where(pm => !QuickPaymentDescription.SupportedPaymentMethods.Contains(pm.PaymentMethodType, StringComparer.OrdinalIgnoreCase))?.ToList();
        }

        public static IList<PimsModel.V4.PaymentInstrument> GetFilteredPaymentInstruments(IList<PimsModel.V4.PaymentInstrument> paymentInstruments)
        {
            return paymentInstruments?.Where(pi => pi.Status == PimsModel.V4.PaymentInstrumentStatus.Active).ToList();
        }
        
        public override Task<List<PIDLResource>> GetDescription()
        {
            List<PIDLResource> retVal = null;

            // Get filtered payment methods
            var filteredPMs = PaymentDescription.GetFilteredPaymentMethods(this.PaymentMethods, this.Operation, this.Family, this.PaymentMethodType);
            ComponentDescription.ValidatePaymentMethods(filteredPMs, V7.Constants.Component.Payment, this.TraceActivityId);

            // Set PIDL description and resource id based on the operation.
            if (this.Operation?.Equals(V7.Constants.Operations.Select, System.StringComparison.OrdinalIgnoreCase) ?? false)
            {
                this.pidlDescriptionType = V7.Constants.DescriptionTypes.PaymentMethodDescription;

                // PIDL Generation
                retVal = PIDLResourceFactory.Instance.GetPaymentClientDescriptions(this.pidlDescriptionType, this.Country, this.pidlResourceIdSelectPM, this.Operation, this.Language, this.Partner, filteredPMs, null, this.ActivePaymentInstruments, exposedFlightFeatures: this.ExposedFlightFeatures?.ToList(), scenario: this.Scenario, setting: this.PSSSetting);

                // Update payment method select options
                this.GetSelectDescription(retVal, filteredPMs);
            }
            else if (this.Operation?.Equals(V7.Constants.Operations.SelectInstance, System.StringComparison.OrdinalIgnoreCase) ?? false)
            {
                this.pidlDescriptionType = V7.Constants.DescriptionTypes.PaymentInstrumentDescription;
                var resourcesId = this.ActivePaymentInstruments?.Count() > 0 ? this.pidlResourceIdSelectPI : this.pidlResourceIdSelectPINone;

                // PIDL Generation
                retVal = PIDLResourceFactory.Instance.GetPaymentClientDescriptions(this.pidlDescriptionType, this.Country, resourcesId, this.Operation, this.Language, this.Partner, filteredPMs, null, this.ActivePaymentInstruments, exposedFlightFeatures: this.ExposedFlightFeatures?.ToList(), scenario: this.Scenario, setting: this.PSSSetting);

                // Update payment instrument select options
                this.GetSelectInstanceDescription(retVal);
            }
            else if (this.Operation?.Equals(V7.Constants.Operations.Add, System.StringComparison.OrdinalIgnoreCase) ?? false)
            {
                this.pidlDescriptionType = V7.Constants.DescriptionTypes.PaymentMethodDescription;

                // PIDL Generation
                retVal = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(new HashSet<PimsModel.V4.PaymentMethod>(filteredPMs ?? new List<PimsModel.V4.PaymentMethod>()), this.Country, this.Family, this.PaymentMethodType, this.Operation, this.Language, this.Partner, exposedFlightFeatures: this.ExposedFlightFeatures?.ToList(), scenario: this.Scenario, setting: this.PSSSetting);

                // Update payment method data and display description.
                this.GetAddDescription(retVal);
            }
            else if (this.Operation?.Equals(V7.Constants.Operations.Delete, System.StringComparison.OrdinalIgnoreCase) ?? false)
            {
                this.pidlDescriptionType = V7.Constants.DescriptionTypes.PaymentInstrumentDescription;

                // PIDL Generation
                retVal = PIDLResourceFactory.Instance.GetPaymentClientDescriptions(this.pidlDescriptionType, this.Country, this.pidlResourceIdDeletePI, this.Operation, this.Language, this.Partner, filteredPMs, null, this.ActivePaymentInstruments, exposedFlightFeatures: this.ExposedFlightFeatures?.ToList(), scenario: this.Scenario, setting: this.PSSSetting);

                // Update payment method select options
                this.GetDeleteDescription(retVal);
            }

            return Task.FromResult(retVal);
        }

        private static GroupDisplayHint CreateGroupDisplayHint(string hintId, string orientation = null, bool? isSubmitGroup = null)
        {
            return new GroupDisplayHint
            {
                HintId = hintId,
                LayoutOrientation = orientation,
                IsSumbitGroup = isSubmitGroup,
            };
        }

        private static PropertyDescription GetShowSummaryProperty()
        {
            return new PropertyDescription()
            {
                PropertyType = PXCommon.Constants.DataDescriptionPropertyType.UserData,
                DataType = PXCommon.Constants.DataDescriptionDataType.TypeBool,
                PropertyDescriptionType = PXCommon.Constants.DataDescriptionDataType.TypeBool,
                IsOptional = true,
                IsUpdatable = true,
                IsKey = false,
                BroadcastTo = V7.Constants.PropertyDescriptionIds.ShowSummary
            };
        }

        /// <summary>
        /// Create show summary display hint and add to select PM. 
        /// Localization is not need since its a hidden and used to switching between address input form and address summary.
        /// </summary>
        /// <returns>New property display hint</returns>
        private static PropertyDisplayHint GetShowSummaryDisplayHint()
        {
            return new PropertyDisplayHint()
            {
                HintId = V7.Constants.DisplayHintIds.ShowSummary,
                ShowDisplayName = "false",
                DisplayHintType = HintType.Property.ToString().ToLower(),
                IsHidden = true,
                DisplayName = "Show Summary",
                PropertyName = V7.Constants.PropertyDescriptionIds.ShowSummary
            };
        }

        private static PidlInstanceDisplayHint GetPidlInstanceDisplayHint(string hintId, string pidlInstance, Dictionary<string, string> conditionalFields)
        {
            return new PidlInstanceDisplayHint()
            {
                HintId = hintId,
                PidlInstance = pidlInstance,
                TriggerSubmitOrder = ListPISelectOptionConstants.SubmitOrder,
                ConditionalFields = conditionalFields
            };
        }

        private static string BuildDeletePIDisplayText(PimsModel.V4.PaymentInstrument paymentInstrument)
        {
            if (paymentInstrument != null)
            {
                if (string.Equals(paymentInstrument.PaymentMethod.PaymentMethodFamily, V7.Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return string.Format(V7.Constants.UnlocalizedDisplayText.DeletePIDisplayTextLastFourDigits, paymentInstrument.PaymentInstrumentDetails?.LastFourDigits);
                }
                else
                {
                    return paymentInstrument.PaymentMethod?.Display?.Name ?? string.Empty;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Create List_PI select option if any saved PI.
        /// </summary>
        /// <param name="pidl">PIDL resource</param>
        /// <param name="selectAction">Selection resource action</param>
        /// <returns>List PI selection operation description</returns>
        private SelectOptionDescription CreateListPIOption(PIDLResource pidl, string selectAction)
        {
            // Get localized display text.
            var localizedDisplayText = PidlModelHelper.GetLocalizedString(ListPISelectOptionConstants.ListPIDisplayText);
            var localizedDisplayContent = PidlModelHelper.GetLocalizedString(ListPISelectOptionConstants.DisplayContent);

            // Add list pi oprtion to the ID property.
            var idProperty = pidl.DataDescription[V7.Constants.PropertyDescriptionIds.Id] as PropertyDescription;
            Dictionary<string, string> idPossibleValues = new Dictionary<string, string>() { { ListPISelectOptionConstants.ListPIKey, localizedDisplayText } };

            foreach (var pv in idProperty.PossibleValues)
            {
                idPossibleValues.Add(pv.Key, pv.Value);
            }

            idProperty.DefaultValue = ListPISelectOptionConstants.ListPIKey;
            idProperty.IsConditionalFieldValue = true;
            idProperty.SideEffects = new Dictionary<string, string>() { { V7.Constants.PropertyDescriptionIds.ShowSummary, string.Format(ListPISelectOptionConstants.ShowSummaryValue, ListPISelectOptionConstants.ListPIKey) } };
            idProperty.UpdatePossibleValues(idPossibleValues);

            // Create listpi resource action context.
            ActionContext listPIcontext = new ActionContext
            {
                Id = ListPISelectOptionConstants.ListPIKey,
                Action = selectAction,
                ResourceActionContext = this.GetResourceActionContext(selectAction)
            };

            // Create select option description.
            SelectOptionDescription listPIOption = new SelectOptionDescription
            {
                DisplayText = localizedDisplayText,
                PidlAction = new DisplayHintAction(DisplayHintActionType.success.ToString(), false, listPIcontext, null),
                IsDisabled = false,
                AccessibilityTag = localizedDisplayText
            };

            // Create payment option text and group.
            TextDisplayHint paymentOptionText = new TextDisplayHint() { HintId = V7.Constants.DisplayHintIds.PaymentOptionText };
            paymentOptionText.DisplayContent = localizedDisplayContent;

            GroupDisplayHint paymentOptionTextGroup = PaymentDescription.CreateGroupDisplayHint(V7.Constants.DisplayHintIds.PaymentOptionText);
            paymentOptionTextGroup.AddDisplayHint(paymentOptionText);

            // Add display content to list pi select option
            listPIOption.DisplayContent = PaymentDescription.CreateGroupDisplayHint(V7.Constants.DisplayHintIds.PaymentMethodOption);
            listPIOption.DisplayContent.Members.Add(paymentOptionTextGroup);

            // Set conditional fileds to select option.
            Dictionary<string, string> conditionalFields = new Dictionary<string, string> { { ListPISelectOptionConstants.IsHiddenKey, string.Format(ListPISelectOptionConstants.IsHiddenValue, localizedDisplayText) } };
            listPIOption.DisplayContent.Members.Add(GetPidlInstanceDisplayHint(V7.Constants.DisplayHintIds.PIDLInstanceListPI, ListPISelectOptionConstants.ListPIKey, conditionalFields));

            return listPIOption;
        }

        private ResourceActionContext GetResourceActionContext(string action, string component = null, string family = null, string type = null)
        {
            var actionContext = new ResourceActionContext()
            {
                Action = action,
                PidlDocInfo = new PidlDocInfo(V7.Constants.DescriptionTypes.PaymentInstrumentDescription, null, null, this.Partner)
            };

            if (component != null)
            {
                actionContext.PidlDocInfo.Parameters.Add(V7.Constants.QueryParameterName.Component, component);
            }

            if (family != null)
            {
                actionContext.PidlDocInfo.Parameters.Add(V7.Constants.QueryParameterName.Family, family);
            }

            if (type != null)
            {
                actionContext.PidlDocInfo.Parameters.Add(V7.Constants.QueryParameterName.Type, type);
            }

            return actionContext;
        }

        /// <summary>
        /// Get Payment component select operation PIDL
        /// </summary>
        /// <param name="retVal">List of PIDL resource</param>
        /// <param name="paymentMethods">Filtered payment methods</param>
        private void GetSelectDescription(List<PIDLResource> retVal, IList<PimsModel.V4.PaymentMethod> paymentMethods)
        {
            Dictionary<string, ResourceActionContext> pidlInstanceContexts = new Dictionary<string, ResourceActionContext>();
            string selectAction = PaymentInstrumentActions.ToString(PIActionType.SelectResource);
            string addAction = PaymentInstrumentActions.ToString(PIActionType.AddResource);

            retVal.ForEach(pidl =>
            {
                // Add show summary to data and display description helps address component switch between summary and input form.
                pidl.DataDescription.Add(V7.Constants.PropertyDescriptionIds.ShowSummary, GetShowSummaryProperty());
                pidl?.DisplayPages?.ForEach(page =>
                {
                    page.Members.Add(GetShowSummaryDisplayHint());
                });

                PropertyDisplayHint paymentMethodDisplayHint = pidl.GetDisplayHintById(V7.Constants.DisplayHintIds.PaymentMethod) as PropertyDisplayHint;
                Dictionary<string, SelectOptionDescription> pmPossibleOptions = new Dictionary<string, SelectOptionDescription>();

                // Add Saved PI option to the possible option list if any PI saveed already
                if (this.Scenario?.Equals(ListPISelectOptionConstants.ListPIKey) ?? false)
                {
                    // Add Saved PI option to the possible option
                    pmPossibleOptions.Add(ListPISelectOptionConstants.ListPIKey, CreateListPIOption(pidl, selectAction));

                    // Add saved PI PIDL instance context
                    pidlInstanceContexts.Add(ListPISelectOptionConstants.ListPIKey, GetResourceActionContext(selectAction, V7.Constants.Component.Payment));
                }

                // Add PIDL instance context to all the payment methods options
                foreach (var pv in paymentMethodDisplayHint.PossibleOptions)
                {
                    var context = pv.Value.PidlAction.Context as ActionContext;
                    pv.Value.AccessibilityTag = pv.Value.DisplayText;
                    context.ResourceActionContext.PidlDocInfo.Parameters[V7.Constants.QueryParameterName.Partner] = this.Partner;

                    // Create payment method logo group display Hint 
                    GroupDisplayHint optionContainer = CreateGroupDisplayHint(V7.Constants.DisplayHintIds.PaymentMethodOption);
                    GroupDisplayHint logoGroup = CreateGroupDisplayHint(V7.Constants.DisplayHintIds.LogoContainer + pv.Key);
                    var methods = paymentMethods.Where(pm => pm.PaymentMethodFamily == context.PaymentMethodFamily).ToList();

                    // Get payment method logo URL from payment methods list.
                    foreach (var method in methods)
                    {
                        ImageDisplayHint logo = new ImageDisplayHint
                        {
                            HintId = string.Format(ListPISelectOptionConstants.OptionLogoPrefix, PaymentSelectionHelper.GetPaymentMethodFamilyTypeDisplayId(method)),
                            SourceUrl = PaymentSelectionHelper.GetPaymentMethodLogoUrl(method),
                            AccessibilityName = method.Display.Name,
                        };

                        logoGroup.Members.Add(logo);
                    }
                    optionContainer.Members.Add(logoGroup);

                    GroupDisplayHint optionTextGroup = CreateGroupDisplayHint(string.Format(ListPISelectOptionConstants.OptionTextGroupPrefix, pv.Key));
                    TextDisplayHint optionText = new TextDisplayHint { HintId = string.Format(ListPISelectOptionConstants.OptionTextPrefix, pv.Key) };
                    optionText.DisplayContent = pv.Value.DisplayText;
                    optionTextGroup.Members.Add(optionText);
                    optionContainer.Members.Add(optionTextGroup);

                    // Add conditional fields to each select option
                    Dictionary<string, string> pmConditionalFields = new()
                    {
                        { ListPISelectOptionConstants.IsHiddenKey, string.Format(ListPISelectOptionConstants.IsHiddenValue, pv.Value.DisplayText) }
                    };
                    optionContainer.Members.Add(GetPidlInstanceDisplayHint(string.Format(ListPISelectOptionConstants.PIDLInstancePrefix, pv.Key), pv.Key, pmConditionalFields));

                    pv.Value.DisplayContent = optionContainer;
                    pmPossibleOptions.Add(pv.Key, pv.Value);

                    // Add payment methods PIDL instance context to PIDL resource
                    pidlInstanceContexts.Add(pv.Key, this.GetResourceActionContext(addAction, V7.Constants.Component.Payment, context?.PaymentMethodFamily, context?.PaymentMethodType));
                }

                // Set PIDL instance context to PIDL resource.
                paymentMethodDisplayHint.SetPossibleOptions(pmPossibleOptions);
                pidl.PIDLInstanceContexts = pidlInstanceContexts;

                // Hide cancel button
                var cancelBtn = pidl.GetDisplayHintById(V7.Constants.DisplayHintIds.CancelButton);
                cancelBtn.IsHidden = true;
            });
        }

        /// <summary>
        /// Get Payment component select instance(ListPI) operation PIDL
        /// </summary>
        /// <param name="retVal">List of PIDL resource</param>
        private void GetSelectInstanceDescription(List<PIDLResource> retVal)
        {
            retVal.ForEach(pidl =>
            {
                if (pidl?.DisplayPages != null && pidl.DisplayPages.Count() > 0)
                {
                    // Update ID property with default payment instrument
                    if (pidl.DataDescription.ContainsKey(V7.Constants.PropertyDescriptionIds.Id))
                    {
                        var idProperty = pidl.DataDescription[V7.Constants.PropertyDescriptionIds.Id] as PropertyDescription;
                        idProperty.DefaultValue = UsePaymentRequestApiEnabled() ? this.PaymentRequestClientActions?.PaymentMethodResults?.DefaultPaymentInstrument?.PaymentInstrumentId : this.CheckoutRequestClientActions?.PaymentMethodResults?.DefaultPaymentInstrument?.PaymentInstrumentId;
                    }

                    if (!this.ShouldComputeTax())
                    {
                        pidl.RemoveDisplayHintById(V7.Constants.DisplayHintIds.CartTax);
                        pidl.RemoveDisplayHintById(V7.Constants.DisplayHintIds.CartTotal);
                    }

                    // Add address and order summary tax and total property with broadcast
                    foreach (var broadcastData in this.SelectInstanceDataDescriptions)
                    {
                        var propertyDescription = new PropertyDescription()
                        {
                            PropertyType = PXCommon.Constants.DataDescriptionPropertyType.UserData,
                            DataType = PXCommon.Constants.DataDescriptionDataType.TypeString,
                            PropertyDescriptionType = PXCommon.Constants.DataDescriptionDataType.TypeString,
                            IsOptional = true,
                            IsUpdatable = true,
                            IsKey = false,
                            BroadcastTo = broadcastData,
                            EmitEventOnPropertyUpdate = ShouldEnableEmitEventOnPropertyUpdate(broadcastData)
                        };

                        pidl.DataDescription.Add(broadcastData, propertyDescription);

                        var propertyDisplayHint = new PropertyDisplayHint()
                        {
                            DisplayName = broadcastData,
                            HintId = broadcastData,
                            PropertyName = broadcastData,
                            ShowDisplayName = "false",
                            DisplayHintType = HintType.Property.ToString().ToLower(),
                            IsHidden = true,
                        };

                        foreach (var page in pidl.DisplayPages)
                        {
                            page.Members.Add(propertyDisplayHint);
                        }
                    }

                    // Adding hidden submit group to list PI PIDL to trigger success submit action to allow selected PIID broadcast to confirm PIDL
                    var submitGroup = new GroupDisplayHint()
                    {
                        IsSumbitGroup = true,
                        HintId = V7.Constants.DisplayHintIds.CancelSaveGroup,
                        LayoutOrientation = V7.Constants.PropertyOrientation.Inline
                    };

                    var successButtonHidden = new ButtonDisplayHint()
                    {
                        HintId = V7.Constants.DisplayHintIds.SaveButton,
                        DisplayHintType = HintType.Button.ToString().ToLower(),
                        IsHidden = true,
                        Action = new DisplayHintAction()
                        {
                            ActionType = DisplayHintActionType.success.ToString()
                        },
                        DisplayContent = PidlModelHelper.GetLocalizedString("Save")
                    };

                    submitGroup.Members.Add(successButtonHidden);

                    pidl.DisplayPages.ForEach(page =>
                    {
                        page.Members.Add(submitGroup);
                    });

                    // Update payment instrument display description with OnResourceSelected event.
                    var piDisplayHint = pidl.GetDisplayHintById(V7.Constants.DisplayHintIds.PaymentInstrument) as PropertyDisplayHint;
                    piDisplayHint?.PossibleOptions?.Remove(V7.Constants.DisplayHintIds.NewPaymentMethodLink);



                }
            });
        }

        /// <summary>
        /// Get Payment component add payment method PIDL
        /// </summary>
        /// <param name="retVal">List of PIDL resource</param>
        private void GetAddDescription(List<PIDLResource> retVal)
        {
            PIDLResourceFactory.RemoveEmptyPidlContainerHints(retVal);

            DescriptionHelper.RemoveUpdateAddresEnabled(this.Family, this.Scenario, retVal, this.ExposedFlightFeatures?.ToList());

            DescriptionHelper.AddOrUpdateServerErrorCode_CreditCardFamily(null, this.Family, this.Language, this.Partner, this.Operation, retVal, this.ExposedFlightFeatures?.ToList());

            if (this.ExposedFlightFeatures?.Contains(Flighting.Features.PXAddCCDfpIframe, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                DescriptionHelper.AddDFPIframe(retVal, this.UsePaymentRequestApiEnabled() ? this.PaymentRequestClientActions.PaymentRequestId : this.CheckoutRequestClientActions?.CheckoutRequestId, this.ExposedFlightFeatures?.ToList());
            }

            foreach (var pidl in retVal)
            {
                // Add selected PIID property data description
                var propertyDescription = new PropertyDescription()
                {
                    PropertyType = PXCommon.Constants.DataDescriptionPropertyType.UserData,
                    DataType = PXCommon.Constants.DataDescriptionDataType.TypeString,
                    PropertyDescriptionType = PXCommon.Constants.DataDescriptionDataType.TypeString,
                    IsOptional = true,
                    IsUpdatable = true,
                    IsKey = false,
                    BroadcastTo = V7.Constants.PropertyDescriptionIds.SelectedPIID
                };
                pidl.DataDescription.Add(V7.Constants.PropertyDescriptionIds.SelectedPIID, propertyDescription);

                var savePaymentDetails = new PropertyDescription()
                {
                    PropertyType = PXCommon.Constants.DataDescriptionPropertyType.UserData,
                    DataType = PXCommon.Constants.DataDescriptionDataType.TypeBool,
                    PropertyDescriptionType = PXCommon.Constants.DataDescriptionDataType.TypeBool,
                    IsOptional = true,
                    IsUpdatable = true,
                    IsKey = false
                };
                pidl.DataDescription.Add(V7.Constants.PropertyDescriptionIds.SavePaymentDetails, savePaymentDetails);

                if (pidl.DataDescription.ContainsKey(V7.Constants.PropertyDescriptionIds.Details))
                {
                    var addressDetails = pidl.GetTargetDataDescription(string.Format("{0}.{1}", V7.Constants.PropertyDescriptionIds.Details, "address"));

                    // Set UsePreExistingValue to all address property
                    if (addressDetails?.Keys != null)
                    {
                        foreach (var addressDetail in addressDetails.Keys)
                        {
                            if (AddressDescription.AddressDataDescriptions.Contains(addressDetail))
                            {
                                var addressProperty = addressDetails[addressDetail] as PropertyDescription;
                                if (addressProperty != null)
                                {
                                    addressProperty.UsePreExistingValue = true;
                                }
                            }
                        }
                    }
                }

                // Updated Expiry month and year group display text
                pidl.ShowDisplayName(V7.Constants.DisplayHintIds.ExpiryMonth, false);
                pidl.ShowDisplayName(V7.Constants.DisplayHintIds.ExpiryYear, false);

                pidl.ShowDisplayName(V7.Constants.DisplayHintIds.ExpiryGroup, true);
                pidl.SetDisplayName(V7.Constants.DisplayHintIds.ExpiryGroup, PidlModelHelper.GetLocalizedString(V7.Constants.PropertyDisplayName.ExpirationDate));

                // Hide address property and button
                var displayHints = pidl.GetAllDisplayHints();
                foreach (var displayHint in displayHints)
                {
                    if (AddressDescription.AddressDisplayDescriptions.Contains(displayHint.HintId)
                        || displayHint.HintId == V7.Constants.DisplayHintIds.SaveButton
                        || displayHint.HintId == V7.Constants.ButtonDisplayHintIds.CancelBackButton
                        || displayHint.DisplayHintType == HintType.Logo.ToString().ToLower()
                        || displayHint.DisplayHintType == HintType.Heading.ToString().ToLower()
                        || displayHint.HintId == V7.Constants.DisplayHintIds.AcceptCardMessage)
                    {
                        displayHint.IsHidden = true;
                    }

                    var propertyDisplayHint = displayHint as PropertyDisplayHint;

                    // Update account token with display example
                    if (displayHint?.PropertyName == V7.Constants.PropertyDescriptionIds.AccountToken)
                    {
                        propertyDisplayHint.DisplayExample = new List<string>() { V7.Constants.PropertyDisplayExample.AccountToken };
                    }

                    // Update account cvv token with display example
                    if (displayHint?.PropertyName == V7.Constants.PropertyDescriptionIds.CVVToken)
                    {
                        propertyDisplayHint.DisplayExample = new List<string>() { V7.Constants.PropertyDisplayExample.CVVToken };
                    }
                }

                // Update the submit URL with request id.
                ComponentDescription.UpdateSubmitURL(pidl, V7.Constants.ButtonDisplayHintIds.SaveButton, GlobalConstants.HTTPVerbs.POST, string.Format(V7.Constants.SubmitUrls.PaymentComponentPIEx, this.Country, this.Language, this.Partner));

                // Add selected PIID property display description
                var selectedPIID = new PropertyDisplayHint()
                {
                    HintId = V7.Constants.DisplayHintIds.SelectedPIID,
                    DisplayHintType = HintType.Property.ToString().ToLower(),
                    DisplayName = "Selected PIID",
                    IsHidden = true,
                    PropertyName = V7.Constants.PropertyDescriptionIds.SelectedPIID,
                };

                // Remove privacy text group, payment option save text and required text group.
                pidl.RemoveDisplayHintById(V7.Constants.DisplayHintIds.PrivacyTextGroup);
                pidl.RemoveDisplayHintById(V7.Constants.DisplayHintIds.PaymentOptionSaveText);
                pidl.RemoveDisplayHintById(V7.Constants.DisplayHintIds.StarRequiredTextGroup);

                // Add selected PIID to display description
                pidl?.DisplayPages?.ForEach(page =>
                {
                    page.Members.Add(selectedPIID);
                });
            }
        }

        /// <summary>
        /// Get Payment component delete operation PIDL
        /// </summary>
        /// <param name="retVal">List of PIDL resource</param>
        private void GetDeleteDescription(List<PIDLResource> retVal)
        {
            var paymentInstrument = UsePaymentRequestApiEnabled() ? this.PaymentRequestClientActions?.PaymentMethodResults?.PaymentInstruments?.Where(pi => pi.PaymentInstrumentId == this.PiId).FirstOrDefault() : this.CheckoutRequestClientActions?.PaymentMethodResults?.PaymentInstruments?.Where(pi => pi.PaymentInstrumentId == this.PiId).FirstOrDefault();
            retVal.ForEach(pidl =>
            {
                var idProperty = pidl.DataDescription[V7.Constants.PropertyDescriptionIds.PiId] as PropertyDescription;
                if (idProperty != null)
                {
                    idProperty.DefaultValue = this.PiId;
                }

                var deleteText = pidl.GetDisplayHintById(V7.Constants.DisplayHintIds.DeleteText) as TextDisplayHint;
                if (deleteText != null)
                {
                    deleteText.DisplayContent = deleteText?.DisplayContent?.Replace(V7.Constants.StringPlaceholders.PIPlaceholder, BuildDeletePIDisplayText(paymentInstrument));
                }

                ComponentDescription.UpdateSubmitURL(pidl, V7.Constants.ButtonDisplayHintIds.YesButton, GlobalConstants.HTTPVerbs.POST, string.Format(V7.Constants.SubmitUrls.PaymentRequestsRemoveEligiblePaymentmethods, UsePaymentRequestApiEnabled() ? this.PaymentRequestClientActions.PaymentRequestId : this.CheckoutRequestClientActions?.CheckoutRequestId));
            });
        }        

        private static class ListPISelectOptionConstants
        {
            // List PI option Display text
            public const string ListPIDisplayText = "Saved";
            public const string DisplayContent = "Saved payment method";

            // List PI select option key.
            public const string ListPIKey = "list_pi";

            // DisplayHint Id format with prefix
            public const string OptionTextPrefix = "optionText{0}";
            public const string OptionTextGroupPrefix = "optionTextGroup{0}";
            public const string PIDLInstancePrefix = "pidlInstance{0}";
            public const string OptionLogoPrefix = "optionLogo_{0}";

            // Pidl Instance DisplayHint trigger submit order.
            public const string SubmitOrder = "beforeBase";

            // Conditional fields key's and values
            public const string IsHiddenKey = "isHidden";
            public const string IsHiddenValue = "<|not|<|stringEqualsIgnoreCase|{{id}};{0}|>|>";
            public const string ShowSummaryValue = "<|ternary|<|stringEqualsIgnoreCase|{{id}};{0}|>;true;false|>";
        }
    }
}