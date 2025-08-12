// <copyright file="InitializationHandler.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.V7.Contexts;
    using Microsoft.Extensions.DependencyInjection;

    public class InitializationHandler
    {
        public static List<PIDLResource> GetDescription(string requestId, CheckoutRequestClientActions checkoutRequestClientActions, string partnerName, PaymentExperienceSetting setting = null, List<string> exposedFlightFeatures = null)
        {
            PIDLResource pidlResource = new PIDLResource();
            if (requestId != null)
            {
                if (RequestContext.GetRequestType(requestId) == V7.Constants.RequestContextType.Checkout)
                {
                    pidlResource = GetCheckoutRequestInitialize(checkoutRequestClientActions, partnerName, setting, exposedFlightFeatures);
                }
                else if (RequestContext.GetRequestType(requestId) == V7.Constants.RequestContextType.Payment)
                {
                    pidlResource = GetPaymentRequestInitialize(partnerName);
                }
            }

            return new List<PIDLResource>() { pidlResource };
        }

        public static List<PIDLResource> GetDescription(string requestId, PaymentRequestClientActions paymentRequestClientActions, string partnerName, PaymentExperienceSetting setting, List<string> exposedFlightFeatures)
        {
            PIDLResource pidlResource = new PIDLResource();
            pidlResource = GetPaymentRequestInitialize(partnerName, setting, paymentRequestClientActions, exposedFlightFeatures);

            return new List<PIDLResource>() { pidlResource };
        }

        private static PIDLResource GetCheckoutRequestInitialize(CheckoutRequestClientActions checkoutRequestClientActions, string partnerName, PaymentExperienceSetting setting, List<string> exposedFlightFeatures)
        {
            PIDLResource pidlResource = new PIDLResource();

            // Check payment methods has any quick payment allowed payment types.
            var quickPaymentMethods = QuickPaymentDescription.GetQuickPaymentMethods(checkoutRequestClientActions?.PaymentMethodResults?.PaymentMethods);

            // Check payment methods has any other types except quick payment types.
            var filteredPMs = PaymentDescription.GetFilteredPaymentMethods(checkoutRequestClientActions?.PaymentMethodResults?.PaymentMethods);

            // Building pidl overrides - This helps to override the existing APS's with this for mentioned resource/component
            Dictionary<string, PidlDocOverrides> pidlOverride = new Dictionary<string, PidlDocOverrides>()
            {
                { V7.Constants.DescriptionTypes.Checkout, new PidlDocOverrides() { Template = V7.Constants.UriTemplate.GetPaymentClientDescription } },
                { V7.Constants.DescriptionTypes.AddressDescription, new PidlDocOverrides() { Template = V7.Constants.UriTemplate.GetPaymentClientDescription } },
                { V7.Constants.DescriptionTypes.ProfileDescription, new PidlDocOverrides() { Template = V7.Constants.UriTemplate.GetPaymentClientDescription } },
                { V7.Constants.DescriptionTypes.ConfirmDescription, new PidlDocOverrides() { Template = V7.Constants.UriTemplate.GetPaymentClientDescription } }
            };

            // Building submission order - Confirm API used this order to submit the data.
            List<SubmissionOrder> submissionOrders = new List<SubmissionOrder>()
            {
                GetSubmissionOrder(V7.Constants.Component.Profile),
                GetSubmissionOrder(V7.Constants.Component.Address)
            };

            var pmByFamily = filteredPMs?
                 .GroupBy(pm => pm.PaymentMethodFamily)?
                 .ToDictionary(group => group.Key, group => group.ToList());

            if (pmByFamily != null)
            {
                foreach (var pm in pmByFamily)
                {
                    var types = pm.Value.Select(pmt => pmt.PaymentMethodType).ToList();
                    var typeId = string.Join("_", types);
                    submissionOrders.Add(GetSubmissionOrder($"{pm.Key}_{typeId}"));
                }
            }


            string scenario = null;
            if (PaymentDescription.GetFilteredPaymentInstruments(checkoutRequestClientActions?.PaymentMethodResults?.PaymentInstruments)?.Count > 0)
            {
                scenario = AddressDescription.AddressScenario;
                submissionOrders.Add(GetSubmissionOrder(scenario));
            }

            submissionOrders.Add(GetSubmissionOrder(V7.Constants.Component.Confirm));

            // Building components property which include all the param will be passed to paymentClient/description API
            Dictionary<string, PidlDocInfo> componentProps = new Dictionary<string, PidlDocInfo>()
            {
                { V7.Constants.Component.OrderSummaryProps, GetPidlDocInfo(V7.Constants.DescriptionTypes.Checkout, V7.Constants.Component.OrderSummary, partnerName) },
                { V7.Constants.Component.AddressProps, GetPidlDocInfo(V7.Constants.DescriptionTypes.AddressDescription, V7.Constants.Component.Address, partnerName, V7.Constants.AddressTypes.Billing, scenario) },
                { V7.Constants.Component.ProfileProps, GetPidlDocInfo(V7.Constants.DescriptionTypes.ProfileDescription, V7.Constants.Component.Profile, partnerName, type: V7.Constants.ProfileType.Checkout) },
                { V7.Constants.Component.ConfirmProps, GetPidlDocInfo(V7.Constants.DescriptionTypes.ConfirmDescription, V7.Constants.Component.Confirm, partnerName) }
            };

            // Add pidl overrides and component props if any quick payment allowed payment types
            if (quickPaymentMethods?.Count > 0)
            {
                pidlOverride.Add(V7.Constants.DescriptionTypes.ExpressCheckout, new PidlDocOverrides() { Template = V7.Constants.UriTemplate.GetPaymentClientDescription });
                componentProps.Add(V7.Constants.Component.QuickPaymentProps, GetPidlDocInfo(V7.Constants.DescriptionTypes.ExpressCheckout, V7.Constants.Component.QuickPayment, partnerName));
            }

            if (filteredPMs?.Count > 0)
            {
                pidlOverride.Add(V7.Constants.DescriptionTypes.PaymentInstrumentDescription, new PidlDocOverrides() { Template = V7.Constants.UriTemplate.GetPaymentClientDescription });
                componentProps.Add(V7.Constants.Component.PaymentProps, GetPidlDocInfo(V7.Constants.DescriptionTypes.PaymentInstrumentDescription, V7.Constants.Component.Payment, partnerName, scenario: scenario));
            }

            // if calculateTax is not true then the partner will handle showing order summary
            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableUsePOCapabilities, StringComparer.OrdinalIgnoreCase)
                && !(checkoutRequestClientActions?.Capabilities?.ComputeTax).GetValueOrDefault(false))
            {
                componentProps.Remove(V7.Constants.Component.OrderSummaryProps);
            }

            pidlResource.InitializeContext = new InitializeContext()
            {
                ComponentProps = componentProps,
                PidlDocOverrides = pidlOverride,
                SubmissionOrder = submissionOrders
            };
            return pidlResource;
        }

        private static PIDLResource GetPaymentRequestInitialize(string partnerName, PaymentExperienceSetting setting = null, PaymentRequestClientActions paymentRequestClientActions = null, List<string> exposedFlightFeatures = null)
        {
            PIDLResource pidlResource = new PIDLResource();

            // Building pidl overrides - This helps to override the existing APIS's with this for mentioned resource/component
            Dictionary<string, PidlDocOverrides> pidlOverride = new Dictionary<string, PidlDocOverrides>()
            {
                { V7.Constants.DescriptionTypes.ConfirmDescription, new PidlDocOverrides() { Template = V7.Constants.UriTemplate.GetPaymentClientDescription } }
            };

            // Building submission order - Confirm API used this order to submit the data.
            List<SubmissionOrder> submissionOrders = new List<SubmissionOrder>()
            {
                GetSubmissionOrder(V7.Constants.Component.Confirm),
            };

            // Building components property which include all the param will be passed to paymentClient/description API
            Dictionary<string, PidlDocInfo> componentProps = new Dictionary<string, PidlDocInfo>()
            {
                { V7.Constants.Component.ConfirmProps, GetPidlDocInfo(V7.Constants.DescriptionTypes.ConfirmDescription, V7.Constants.Component.Confirm, partnerName) }
            };

            // If ComponentSetting feature is enabled, dynamically add based on its components
            if (PidlFactory.V7.PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PidlFactory.V7.PartnerSettingsHelper.Features.ComponentSetting, paymentRequestClientActions?.Country, setting))
            {
                ProcessComponentSettings(
                    setting,
                    paymentRequestClientActions,
                    partnerName,
                    pidlOverride,
                    submissionOrders,
                    componentProps);
            }

            // if calculateTax is not true then the partner will handle showing order summary
            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableUsePOCapabilities, StringComparer.OrdinalIgnoreCase)
                && !(paymentRequestClientActions?.Capabilities?.ComputeTax).GetValueOrDefault(false))
            {
                componentProps.Remove(V7.Constants.Component.OrderSummaryProps);
            }

            pidlResource.InitializeContext = new InitializeContext()
            {
                ComponentProps = componentProps,
                PidlDocOverrides = pidlOverride,
                SubmissionOrder = submissionOrders
            };

            return pidlResource;
        }

        private static void ProcessComponentSettings(
            PaymentExperienceSetting setting,
            PaymentRequestClientActions paymentRequestClientActions,
            string partnerName,
            Dictionary<string, PidlDocOverrides> pidlOverride,
            List<SubmissionOrder> submissionOrders,
            Dictionary<string, PidlDocInfo> componentProps)
        {
            var featureConfig = setting?.Features != null && setting.Features.ContainsKey(PidlFactory.V7.PartnerSettingsHelper.Features.ComponentSetting)
                ? setting.Features[PidlFactory.V7.PartnerSettingsHelper.Features.ComponentSetting]
                : null;

            // Defensive: get the first displayCustomizationDetail with a non-null component list
            var componentList = featureConfig?.DisplayCustomizationDetail?.FirstOrDefault(d => d.Components != null && d.Components.Any())?.Components;

            if (componentList != null && componentList.Any())
            {
                // Clear existing collections instead of creating new ones
                pidlOverride.Clear();
                submissionOrders.Clear();
                componentProps.Clear();

                string scenario = null;
                bool hasExistingPaymentInstruments = PaymentDescription.GetFilteredPaymentInstruments(paymentRequestClientActions?.PaymentMethodResults?.PaymentInstruments)?.Count > 0;
                if (hasExistingPaymentInstruments)
                {
                    scenario = AddressDescription.AddressScenario;
                }

                // Map component name to description type and prop key
                foreach (var comp in componentList)
                {
                    switch (comp)
                    {
                        case V7.Constants.Component.OrderSummary:
                            componentProps[V7.Constants.Component.OrderSummaryProps] = GetPidlDocInfo(V7.Constants.DescriptionTypes.Checkout, V7.Constants.Component.OrderSummary, partnerName);
                            break;
                        case V7.Constants.Component.Address:
                            pidlOverride[V7.Constants.DescriptionTypes.AddressDescription] = new PidlDocOverrides() { Template = V7.Constants.UriTemplate.GetPaymentClientDescription };
                            componentProps[V7.Constants.Component.AddressProps] = GetPidlDocInfo(V7.Constants.DescriptionTypes.AddressDescription, V7.Constants.Component.Address, partnerName, V7.Constants.AddressTypes.Billing, scenario: scenario);
                            submissionOrders.Add(GetSubmissionOrder(V7.Constants.Component.Address));
                            break;
                        case V7.Constants.Component.Profile:
                            pidlOverride[V7.Constants.DescriptionTypes.ProfileDescription] = new PidlDocOverrides() { Template = V7.Constants.UriTemplate.GetPaymentClientDescription };
                            componentProps[V7.Constants.Component.ProfileProps] = GetPidlDocInfo(V7.Constants.DescriptionTypes.ProfileDescription, V7.Constants.Component.Profile, partnerName, type: V7.Constants.ProfileType.Checkout);
                            submissionOrders.Add(GetSubmissionOrder(V7.Constants.Component.Profile));
                            break;
                        case V7.Constants.Component.Payment:
                            // Check payment methods has any other types except quick payment types.
                            var filteredPMs = PaymentDescription.GetFilteredPaymentMethods(paymentRequestClientActions?.PaymentMethodResults?.PaymentMethods);
                            var pmByFamily = filteredPMs?
                                .GroupBy(pm => pm.PaymentMethodFamily)?
                                .ToDictionary(group => group.Key, group => group.ToList());

                            if (pmByFamily != null)
                            {
                                foreach (var pm in pmByFamily)
                                {
                                    var types = pm.Value.Select(pmt => pmt.PaymentMethodType).ToList();
                                    var typeId = string.Join("_", types);
                                    submissionOrders.Add(GetSubmissionOrder($"{pm.Key}_{typeId}"));
                                }
                            }

                            if (hasExistingPaymentInstruments)
                            {
                                submissionOrders.Add(GetSubmissionOrder(scenario));
                            }

                            if (filteredPMs?.Count > 0)
                            {
                                pidlOverride[V7.Constants.DescriptionTypes.PaymentInstrumentDescription] = new PidlDocOverrides()
                                {
                                    Template = V7.Constants.UriTemplate.GetPaymentClientDescription
                                };

                                componentProps[V7.Constants.Component.PaymentProps] = GetPidlDocInfo(
                                    V7.Constants.DescriptionTypes.PaymentInstrumentDescription,
                                    V7.Constants.Component.Payment,
                                    partnerName,
                                    scenario: scenario
                                );
                            }

                            break;
                        case V7.Constants.Component.QuickPayment:
                            // Check payment methods has any quick payment allowed payment types.
                            var quickPaymentMethods = QuickPaymentDescription.GetQuickPaymentMethods(paymentRequestClientActions?.PaymentMethodResults?.PaymentMethods);

                            if (quickPaymentMethods?.Count > 0)
                            {
                                pidlOverride[V7.Constants.DescriptionTypes.ExpressCheckout] = new PidlDocOverrides() { Template = V7.Constants.UriTemplate.GetPaymentClientDescription };
                                componentProps[V7.Constants.Component.QuickPaymentProps] = GetPidlDocInfo(V7.Constants.DescriptionTypes.ExpressCheckout, V7.Constants.Component.QuickPayment, partnerName);
                            }

                            break;
                        case V7.Constants.Component.Confirm:
                            pidlOverride[V7.Constants.DescriptionTypes.ConfirmDescription] = new PidlDocOverrides() { Template = V7.Constants.UriTemplate.GetPaymentClientDescription };
                            componentProps[V7.Constants.Component.ConfirmProps] = GetPidlDocInfo(V7.Constants.DescriptionTypes.ConfirmDescription, V7.Constants.Component.Confirm, partnerName);
                            submissionOrders.Add(GetSubmissionOrder(V7.Constants.Component.Confirm));
                            break;
                    }
                }
            }
        }

        private static PidlDocInfo GetPidlDocInfo(string resourceType, string component, string partner, string type = null, string scenario = null)
        {
            PidlDocInfo pidlDocInfo = new PidlDocInfo()
            {
                ResourceType = resourceType,
            };
            pidlDocInfo.SetParameters(GetParameters(component, partner, type, scenario));

            return pidlDocInfo;
        }

        private static Dictionary<string, string> GetParameters(string component, string partner, string type = null, string scenario = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { V7.Constants.QueryParameterName.Component, component },
                { V7.Constants.QueryParameterName.Partner, partner }
            };

            if (!string.IsNullOrWhiteSpace(type))
            {
                parameters.Add(V7.Constants.QueryParameterName.Type, type);
            }

            if (!string.IsNullOrWhiteSpace(scenario))
            {
                parameters.Add(V7.Constants.QueryParameterName.Scenario, scenario);
            }

            return parameters;
        }

        private static SubmissionOrder GetSubmissionOrder(string instanceName, bool validateOnly = false)
        {
            return new SubmissionOrder()
            {
                InstanceName = instanceName,
                ValidateOnly = validateOnly
            };
        }
    }
}