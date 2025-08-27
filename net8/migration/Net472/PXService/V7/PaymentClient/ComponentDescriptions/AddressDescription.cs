// <copyright file="AddressDescription.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Practices.ObjectBuilder2;

    public class AddressDescription : ComponentDescription
    {
        public const string AddressScenario = "list_pi";
        private const string PIDLDescriptionType = V7.Constants.DescriptionTypes.AddressDescription;

        public static IReadOnlyList<string> AddressDisplayDescriptions
        {
            get
            {
                return new List<string>()
                {
                        V7.Constants.DisplayHintIds.AddressLine1,
                        V7.Constants.DisplayHintIds.AddressLine2,
                        V7.Constants.DisplayHintIds.AddressLine3,
                        V7.Constants.DisplayHintIds.AddressCity,
                        V7.Constants.DisplayHintIds.AddressState,
                        V7.Constants.DisplayHintIds.AddressCounty,
                        V7.Constants.DisplayHintIds.AddressProvince,
                        V7.Constants.DisplayHintIds.AddressPostalCode,
                        V7.Constants.DisplayHintIds.AddressCountry
                };
            }
        }

        public static IReadOnlyList<string> AddressDataDescriptions
        {
            get
            {
                return new List<string>()
                {
                    V7.Constants.PropertyDescriptionIds.AddressLine1,
                    V7.Constants.PropertyDescriptionIds.AddressLine2,
                    V7.Constants.PropertyDescriptionIds.AddressLine3,
                    V7.Constants.PropertyDescriptionIds.City,
                    V7.Constants.PropertyDescriptionIds.Region,
                    V7.Constants.PropertyDescriptionIds.PostalCode,
                    V7.Constants.PropertyDescriptionIds.Country
                };
            }
        }

        public override string DescriptionType
        {
            get
            {
                return AddressDescription.PIDLDescriptionType;
            }
        }

        public override Task<List<PIDLResource>> GetDescription()
        {
            List<PIDLResource> retVal = new List<PIDLResource>();

            // Generate billing form and billing summary if tenant has save PI.
            if (this.Scenario?.Equals(AddressScenario, System.StringComparison.OrdinalIgnoreCase) ?? false)
            {
                retVal = PIDLResourceFactory.Instance.GetPaymentClientDescriptions(PIDLDescriptionType, this.Country, V7.Constants.AddressTypes.BillingSummary, this.Operation, this.Language, this.Partner, exposedFlightFeatures: this.ExposedFlightFeatures?.ToList(), scenario: this.Scenario, setting: this.PSSSetting);
                var addressForm = PIDLResourceFactory.Instance.GetPaymentClientDescriptions(PIDLDescriptionType, this.Country, V7.Constants.AddressTypes.BillingForm, this.Operation, this.Language, this.Partner, exposedFlightFeatures: this.ExposedFlightFeatures?.ToList(), scenario: this.Scenario, setting: this.PSSSetting);
                retVal.Add(addressForm.FirstOrDefault());
            }
            else
            {
                // Generate billing form if no save pi
                retVal = PIDLResourceFactory.Instance.GetPaymentClientDescriptions(PIDLDescriptionType, this.Country, V7.Constants.AddressTypes.BillingForm, this.Operation, this.Language, this.Partner, exposedFlightFeatures: this.ExposedFlightFeatures?.ToList(), scenario: this.Scenario, setting: this.PSSSetting);
            }

            List<string> skipDataDescription = new List<string>() { V7.Constants.PropertyDescriptionIds.AddressType, V7.Constants.PropertyDescriptionIds.AddressResourceId, V7.Constants.PropertyDescriptionIds.AddressCountry, V7.Constants.PropertyDescriptionIds.AddressOperation };
            List<string> skipDisplayDescription = new List<string>() { V7.Constants.DisplayHintIds.CartTax, V7.Constants.DisplayHintIds.CartSubtotal, V7.Constants.DisplayHintIds.CartTotal, V7.Constants.DisplayHintIds.ShowSummary, V7.Constants.DisplayHintIds.CancelButton };

            PropertyEvent onFocusOut = new PropertyEvent()
            {
                EventType = V7.Constants.PropertyEventType.ValidateOnChange,
                Context = new EventContext()
                {
                    Href = PidlFactory.V7.PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PidlFactory.V7.PartnerSettingsHelper.Features.EnablePaymentRequestAddressValidation, this.UsePaymentRequestApiEnabled() ? this.PaymentRequestClientActions?.Country : this.CheckoutRequestClientActions?.Country, this.PSSSetting) ?
                    string.Format(V7.Constants.UriTemplate.PifdAnonymousModernAVSForTrade, V7.Constants.AddressTypes.Billing, this.Partner, this.Language, V7.Constants.ScenarioNames.SuggestAddressesTradeAVSUsePidlModal, this.Country)
                    : string.Format(V7.Constants.SubmitUrls.CheckoutRequestsExAttachAddressMergeData, this.UsePaymentRequestApiEnabled() ? this.PaymentRequestClientActions?.PaymentRequestId : this.CheckoutRequestClientActions?.CheckoutRequestId, V7.Constants.AddressTypes.Billing, V7.Constants.ScenarioNames.MergeData),
                    Method = GlobalConstants.HTTPVerbs.POST,
                    Silent = true,
                }
            };

            // Remove Tax Related elements from dataDescription and displayDescription, and emit the address to partner so that partner can computer tax by themselves.
            if (!this.ShouldComputeTax())
            {
                RemoveTaxRelatedElements(retVal);
                onFocusOut.NextAction = ComponentDescription.CreatePartnerActionWithPidlPayload(V7.Constants.ResourceTypes.Address);
            }

            string validationRegex = "^true$";
            retVal.ForEach(pidl =>
            {
                // Update data description with broadcastTo, show summary regex and use pre existing value.
                UpdateDataDescription(pidl, skipDataDescription, validationRegex);

                // Update display description with property event, submit url and set ishidden flag
                UpdateDisplayDescription(pidl, skipDisplayDescription, onFocusOut);
            });

            return Task.FromResult(retVal);
        }

        /// <summary>
        /// Removes tax related elements from data descriptions and display hints of PIDL resources.
        /// </summary>
        /// <param name="pidlResources">List of PIDL resources to modify</param>
        private static void RemoveTaxRelatedElements(List<PIDLResource> pidlResources)
        {
            foreach (var pidl in pidlResources)
            {
                // Remove from DataDescription
                pidl.RemoveFirstDataDescriptionByPropertyName(V7.Constants.PropertyDescriptionIds.CartTax);
                pidl.RemoveFirstDataDescriptionByPropertyName(V7.Constants.PropertyDescriptionIds.CartTotal);
                pidl.RemoveFirstDataDescriptionByPropertyName(V7.Constants.PropertyDescriptionIds.CartSubtotal);

                // Remove from DisplayDescription (DisplayHints)
                pidl.RemoveDisplayHintById(V7.Constants.DisplayHintIds.CartTax);
                pidl.RemoveDisplayHintById(V7.Constants.DisplayHintIds.CartTotal);
                pidl.RemoveDisplayHintById(V7.Constants.DisplayHintIds.CartSubtotal);
            }
        }

        private static void UpdateDataDescription(PIDLResource pidl, List<string> skipDataDescription, string validationRegex)
        {
            pidl.DataDescription.Keys.Where(key => !skipDataDescription.Contains(key)).ForEach(dataDescriptionId =>
            {
                var pidlDataDescription = pidl.DataDescription[dataDescriptionId] as PropertyDescription;

                // Allows model.ts in pidl.sdk to trigger eventHub.propertyUpdated
                // which triggers onEvent callback in pidl-react components
                if (ShouldEnableEmitEventOnPropertyUpdate(dataDescriptionId))
                {
                    pidlDataDescription.EmitEventOnPropertyUpdate = true;
                }

                if (pidl.Identity[V7.Constants.PidlIdentityFields.ResourceId] == V7.Constants.AddressTypes.BillingForm)
                {
                    if (dataDescriptionId.Equals(V7.Constants.PropertyDescriptionIds.ShowSummary))
                    {
                        // Billing form show summary property should be key to render address summary
                        pidlDataDescription.IsKey = true;
                    }
                    else
                    {
                        // Address data property should set the broadcast to in order to pass the data to other component
                        pidlDataDescription.BroadcastTo = dataDescriptionId;
                    }
                }
                else if (pidl.Identity[V7.Constants.PidlIdentityFields.ResourceId] == V7.Constants.AddressTypes.BillingSummary)
                {
                    if (dataDescriptionId.Equals(V7.Constants.PropertyDescriptionIds.ShowSummary))
                    {
                        pidlDataDescription.Validation.Regex = validationRegex;
                        pidlDataDescription.Validations.ForEach(valid => { valid.Regex = validationRegex; });
                        pidlDataDescription.IsKey = true;
                    }

                    // Set user preexisting value to show the default address values.
                    pidlDataDescription.UsePreExistingValue = true;
                }
            });
        }

        private void UpdateDisplayDescription(PIDLResource pidl, List<string> skipDisplayDescription, PropertyEvent onFocusOut)
        {
            var displayHints = pidl.GetAllDisplayHints();
            displayHints.ForEach(addressDisplayHint =>
            {
                if (addressDisplayHint.HintId.Equals(V7.Constants.ButtonDisplayHintIds.SaveButton))
                {
                    // Update attach address submit url with request id.
                    UpdateSubmitURL(addressDisplayHint as ButtonDisplayHint, GlobalConstants.HTTPVerbs.POST, string.Format(V7.Constants.SubmitUrls.CheckoutRequestsExAttachAddress, UsePaymentRequestApiEnabled() ? this.PaymentRequestClientActions?.PaymentRequestId : this.CheckoutRequestClientActions?.CheckoutRequestId, V7.Constants.AddressTypes.Billing));
                    addressDisplayHint.IsHidden = true;
                }
                else
                {
                    if (!skipDisplayDescription.Contains(addressDisplayHint.HintId))
                    {
                        var addressProperty = addressDisplayHint as PropertyDisplayHint;
                        if (addressProperty != null)
                        {
                            addressProperty.Onfocusout = onFocusOut;
                        }
                    }

                    // Hide billing summary display description except the summary property.
                    if (pidl.Identity[V7.Constants.PidlIdentityFields.ResourceId] == V7.Constants.AddressTypes.BillingSummary
                        && (addressDisplayHint.DisplayHintType == HintType.Button.ToString().ToLower()
                        || addressDisplayHint.DisplayHintType == HintType.Property.ToString().ToLower()))
                    {
                        addressDisplayHint.IsHidden = true;
                    }                    
                    else if (pidl.Identity[V7.Constants.PidlIdentityFields.ResourceId] == V7.Constants.AddressTypes.BillingForm
                        && addressDisplayHint.HintId == V7.Constants.DisplayHintIds.BillingAddressTitle)
                    {
                        addressDisplayHint.IsHidden = true;
                    }
                }
            });
        }
    }
}