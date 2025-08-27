// <copyright file="ComponentDescriptionFactory.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PXService.V7.Contexts;

    public class ComponentDescriptionFactory
    {
        // This mapping will be moved to PSS.
        private static readonly Dictionary<string, List<string>> requestComponentMapping = new Dictionary<string, List<string>>
        {
            {
                V7.Constants.RequestContextType.Checkout, new List<string>
                {
                    V7.Constants.Component.QuickPayment,
                    V7.Constants.Component.OrderSummary,
                    V7.Constants.Component.Payment,
                    V7.Constants.Component.Address,
                    V7.Constants.Component.Profile,
                    V7.Constants.Component.Confirm
                }
            },
            {
                V7.Constants.RequestContextType.Payment, new List<string>
                {
                    V7.Constants.Component.Challenge,
                    V7.Constants.Component.Confirm
                }
            }
        };

        public static ComponentDescription CreateInstance(string componentName, RequestContext requestContext, Generate3DS2ChallengePIDLResource generate3DS2ChallengePIDLResource = null)
        {
            try
            {
                var requestType = RequestContext.GetRequestType(requestContext);
                if (requestComponentMapping.ContainsKey(requestType))
                {
                    var componentMapping = requestComponentMapping[requestType];
                    if (componentMapping.Contains(componentName))
                    {
                        // Todo: This needs to be refactored to have separate component creation for different request types to avoid if else inside the cases.
                        switch (componentName)
                        {
                            case V7.Constants.Component.QuickPayment: return new QuickPaymentDescription();
                            case V7.Constants.Component.OrderSummary: return new OrderSummaryDescription();
                            case V7.Constants.Component.Payment: return new PaymentDescription();
                            case V7.Constants.Component.Address: return new AddressDescription();
                            case V7.Constants.Component.Profile: return new ProfileDescription();
                            case V7.Constants.Component.Confirm:
                                if (requestType == V7.Constants.RequestContextType.Payment)
                                {
                                    return new PaymentRequestConfirmDescription(requestContext, generate3DS2ChallengePIDLResource);
                                }
                                else
                                {
                                    return new ConfirmDescription();
                                }
                        }
                    }

                    throw new ArgumentException($"Component {componentName} not found for request Id {requestContext?.RequestId}");
                }

                throw new ArgumentException($"Invalid request Id {requestContext?.RequestId}");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to create instance of component: {componentName} for request Id: {requestContext?.RequestId} ", ex);
            }

            throw new ArgumentException($"Request Id {requestContext?.RequestId} and {componentName} not supported");
        }

        public static ComponentDescription CreateInstance(string componentName, RequestContext requestContext, PaymentExperienceSetting setting, Generate3DS2ChallengePIDLResource generate3DS2ChallengePIDLResource = null)
        {
            switch (componentName)
            {
                case V7.Constants.Component.QuickPayment: return new QuickPaymentDescription();
                case V7.Constants.Component.OrderSummary: return new OrderSummaryDescription();
                case V7.Constants.Component.Payment: return new PaymentDescription();
                case V7.Constants.Component.Address: return new AddressDescription();
                case V7.Constants.Component.Profile: return new ProfileDescription();
                case V7.Constants.Component.Confirm:
                    if (PidlFactory.V7.PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PidlFactory.V7.PartnerSettingsHelper.Features.PaymentClientHandlePaymentCollection, null, setting))
                    {
                        return new ConfirmDescription();
                    }
                    else
                    {
                        return new PaymentRequestConfirmDescription(requestContext, generate3DS2ChallengePIDLResource);
                    }

                default:
                    throw new ArgumentException($"Component {componentName} not found for request Id {requestContext?.RequestId}");
            }
        }

        public static void ValidateRequiredParam(string requestParam, string paramName)
        {
            if (requestParam == null)
            {
                throw new Common.ValidationException(ErrorCode.InvalidRequestData, $"{paramName} is a required parameter");
            }
        }
    }
}