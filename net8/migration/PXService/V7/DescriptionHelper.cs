// <copyright file="DescriptionHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using static Microsoft.Commerce.Payments.PXService.V7.Constants;

    internal static class DescriptionHelper
    {
        public static void RemoveUpdateAddresEnabled(string family, string scenario, List<PIDLResource> retVal, List<string> exposedFlightFeatures)
        {
            if (string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString())
                && !exposedFlightFeatures.Contains(Flighting.Features.PXIncludeUpdateAddressCheckboxInAddCC, StringComparer.OrdinalIgnoreCase)
                && !string.Equals(scenario, Constants.ScenarioNames.WithProfileAddress, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource resource in retVal)
                {
                    resource.DataDescription.Remove(V7.Constants.PropertyDescriptionIds.UpdateAddressEnabled);
                }
            }
        }

        public static void AddDFPIframe(List<PIDLResource> retVal, string checkoutRequestId, List<string> exposedFlightFeatures)
        {
            string instance_id = Microsoft.Commerce.Payments.Common.Environments.Environment.IsProdOrPPEEnvironment ? Constants.DFPInstanceIds.PROD : Constants.DFPInstanceIds.INT;
            string displayContent = $"<!DOCTYPE html><html><head><script src=\"https://fpt.dfp.microsoft.com/mdt.js?session_id={checkoutRequestId}&instanceId={instance_id}\"></script><script>window.onload=function(){{if(window.dfp&&typeof window.dfp.doFpt===\"function\"){{window.dfp.doFpt(document);}}}}</script></head><body></body></html>";
            if (exposedFlightFeatures?.Contains(Flighting.Features.PXPaasAddCCDfpIframeForCommerceRisk) ?? false)
            {
                displayContent = $"<!DOCTYPE html><html><head><script src=\"https://df.cfp.microsoft.com/mdt.js?session_id={checkoutRequestId}\"></script><script>window.onload=function(){{if(window.dfp&&typeof window.dfp.doFpt===\"function\"){{window.dfp.doFpt(document);}}}}</script></head><body></body></html>";
            }

            foreach (PIDLResource resource in retVal)
            {
                IFrameDisplayHint dfpIFrame = new IFrameDisplayHint
                {
                    HintId = "dfpIframe",
                    DisplayContent = displayContent,
                    Width = "0px",
                    Height = "0px",
                    DisplayTags = new Dictionary<string, string> { { "accessibilityName", "dfpIframe" } },
                };

                resource.DisplayPages[0].Members.Add(dfpIFrame);
            }
        }

        public static void AddOrUpdateServerErrorCode_CreditCardFamily(string accountId, string family, string language, string partner, string operation, List<PIDLResource> retVal, List<string> exposedFlightFeatures)
        {
            if (family == Constants.PaymentMethodFamily.credit_card.ToString())
            {
                foreach (PIDLResource resource in retVal)
                {
                    if (resource.PidlResourceStrings == null)
                    {
                        resource.PidlResourceStrings = new PidlResourceStrings();
                    }

                    // Add InvalidIssuerResponse error codes to display inline message with card number field.
                    if (exposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXDisplay3dsNotEnabledErrorInline))
                    {
                        resource.PidlResourceStrings.AddOrUpdateServerErrorCode(
                        Constants.CreditCardErrorCodes.InvalidIssuerResponseWithTRPAU0009,
                        new ServerErrorCode()
                        {
                            Target = string.Format(
                                "{0}",
                                Constants.CreditCardErrorTargets.CardNumber),
                            ErrorMessage = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidIssuerResponse, language),
                        });

                        resource.PidlResourceStrings.AddOrUpdateServerErrorCode(
                        Constants.CreditCardErrorCodes.InvalidIssuerResponseWithTRPAU0008,
                        new ServerErrorCode()
                        {
                            Target = string.Format(
                                "{0}",
                                Constants.CreditCardErrorTargets.CardNumber),
                            ErrorMessage = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidIssuerResponse, language),
                        });
                    }

                    AddServerErrorCodeForAddPI(resource, language, exposedFlightFeatures);

                    // TODO: Before we enable the retry on InvalidRequestData error to all partners, we need to ensure that this specific error code is not returned for anything other than token expiry.
                    // Ideally, it would be better to get specific error code for expired tokens than the generalized InvaliRequestData
                    if (Constants.PartnersToEnableRetryOnInvalidRequestData.Contains(partner, StringComparer.OrdinalIgnoreCase))
                    {
                        resource.PidlResourceStrings.AddOrUpdateServerErrorCode(
                        Constants.CreditCardErrorCodes.InvalidRequestData,
                        new ServerErrorCode()
                        {
                            Target = string.Format(
                                "{0},{1}",
                                Constants.CreditCardErrorTargets.CardNumber,
                                Constants.CreditCardErrorTargets.Cvv),
                            ErrorMessage = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.Generic, language),
                            RetryPolicy = new RetryPolicy()
                            {
                                RetryPolicyType = RetryPolicyType.limitedRetry,
                                Context = new RetryPolicyContext()
                                {
                                    MaxRetryCount = 3
                                }
                            }
                        });
                    }

                    if (string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                        && (Constants.JarvisAccountIdHmacPartners.Contains(partner, StringComparer.OrdinalIgnoreCase)
                        || exposedFlightFeatures.Contains(Flighting.Features.PXEnableJarvisHMAC)))
                    {
                        resource.DataDescription[Constants.JarvisAccountIdHmacProperty] = new PropertyDescription()
                        {
                            PropertyType = "clientData",
                            DataType = "hidden",
                            PropertyDescriptionType = "hidden",
                            IsUpdatable = false,
                            DefaultValue = ObfuscationHelper.GetHashValue(accountId, ObfuscationHelper.JarvisAccountIdHashSalt),
                        };
                    }
                }
            }
        }

        public static void AddServerErrorCodeForAddPI(PIDLResource pidlResource, string language, List<string> exposedFlightFeatures)
        {
            pidlResource.PidlResourceStrings.AddOrUpdateServerErrorCode(
            Constants.CreditCardErrorCodes.ValidationFailed,
            new ServerErrorCode()
            {
                Target = string.Format(
                    "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                    Constants.CreditCardErrorTargets.AccountHolderName,
                    Constants.CreditCardErrorTargets.CardNumber,
                    Constants.CreditCardErrorTargets.Cvv,
                    Constants.CreditCardErrorTargets.ExpiryMonth,
                    Constants.CreditCardErrorTargets.ExpiryYear,
                    Constants.CreditCardErrorTargets.AddressLine1,
                    Constants.CreditCardErrorTargets.AddressLine2,
                    Constants.CreditCardErrorTargets.AddressLine3,
                    Constants.CreditCardErrorTargets.City,
                    Constants.CreditCardErrorTargets.State,
                    Constants.CreditCardErrorTargets.Country,
                    Constants.CreditCardErrorTargets.PostalCode),
                ErrorMessage = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.ValidationFailed, language),
                RetryPolicy = new RetryPolicy()
                {
                    RetryPolicyType = RetryPolicyType.limitedRetry,
                    Context = new RetryPolicyContext()
                    {
                        MaxRetryCount = 3
                    }
                }
            });

            if (exposedFlightFeatures != null && !exposedFlightFeatures.Contains(Flighting.Features.PXDisableInvalidPaymentInstrumentType))
            {
                pidlResource.PidlResourceStrings.AddOrUpdateServerErrorCode(
                Constants.CreditCardErrorCodes.InvalidPaymentInstrumentType,
                new ServerErrorCode()
                {
                    Target = string.Format(
                        "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                        Constants.CreditCardErrorTargets.AccountHolderName,
                        Constants.CreditCardErrorTargets.CardNumber,
                        Constants.CreditCardErrorTargets.Cvv,
                        Constants.CreditCardErrorTargets.ExpiryMonth,
                        Constants.CreditCardErrorTargets.ExpiryYear,
                        Constants.CreditCardErrorTargets.AddressLine1,
                        Constants.CreditCardErrorTargets.AddressLine2,
                        Constants.CreditCardErrorTargets.AddressLine3,
                        Constants.CreditCardErrorTargets.City,
                        Constants.CreditCardErrorTargets.State,
                        Constants.CreditCardErrorTargets.Country,
                        Constants.CreditCardErrorTargets.PostalCode),
                    ErrorMessage = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidPaymentInstrumentType, language),
                    RetryPolicy = new RetryPolicy()
                    {
                        RetryPolicyType = RetryPolicyType.limitedRetry,
                        Context = new RetryPolicyContext()
                        {
                            MaxRetryCount = 3
                        }
                    }
                });
            }
        }
    }
}