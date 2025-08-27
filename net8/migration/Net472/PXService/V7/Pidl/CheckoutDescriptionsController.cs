// <copyright file="CheckoutDescriptionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentThirdPartyService;
    using Microsoft.Commerce.Tracing;

    public class CheckoutDescriptionsController : ProxyController
    {
        /// <summary>
        /// Get Checkout Descriptions
        /// </summary>
        /// <group>CheckoutDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/anonymous/CheckoutDescriptions</url>
        /// <param name="checkoutId" required="true" cref="string" in="query">checkout Id</param>
        /// <param name="paymentProviderId" required="true" cref="string" in="query">payment Provider Id</param>
        /// <param name="redirectUrl" required="true" cref="string" in="query">redirect Url</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="family" required="false" cref="string" in="query">Payment method family name</param>
        /// <param name="type" required="false" cref="string" in="query">Payment method type name</param>
        /// <param name="scenario" required="false" cref="string" in="query">Scenario name</param>
        /// <returns>A list of PIDLResource</returns>
        [HttpGet]
        public async Task<object> GetCheckoutDescriptions(
            string checkoutId,
            string paymentProviderId,
            string redirectUrl,
            string partner = Constants.PartnerName.MSTeams,
            string country = null,
            string language = GlobalConstants.Defaults.Language,
            string family = null,
            string type = null,
            string scenario = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            PaymentsEventSource.Log.InstrumentManagementServiceTraceRequest(GlobalConstants.APINames.GetCheckoutDescriptions, this.Request.RequestUri.AbsolutePath, traceActivityId);
            string cV = this.Request.GetCorrelationVector().ToString();
            this.Request.AddPartnerProperty(partner?.ToLower());
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Operations.RenderPidlPage);

            try
            {
                // 1. Get the status of checkout
                var checkout = await this.Settings.PaymentThirdPartyServiceAccessor.GetCheckout(paymentProviderId, checkoutId, traceActivityId);
                if (checkout.Status == CheckoutStatus.Invalid
                    || checkout.Status == CheckoutStatus.Paid)
                {
                    return new List<PIDLResource>() { PIDLResourceFactory.GetRedirectPidl(redirectUrl, true) };
                }

                // 2. Get seller id from payment request
                var paymentRequest = await this.Settings.PaymentThirdPartyServiceAccessor.GetPaymentRequest(paymentProviderId, checkout.PaymentRequestId, traceActivityId);

                // 3. Get seller name from seller service
                var seller = await this.Settings.SellerMarketPlaceServiceAccessor.GetSeller(partner, paymentProviderId, paymentRequest.SellerId, traceActivityId);

                // 4. Construct submit url
                var pidlScenario = string.Equals(paymentRequest.Context, GlobalConstants.PaymentRequestContext.PrePaidMeeting, StringComparison.InvariantCultureIgnoreCase) ? GlobalConstants.ScenarioNames.GuestCheckoutPrepaidMeeting : null;

                string buyerCountry = country ?? seller.Country ?? GlobalConstants.Defaults.Country;

                string backButtonUrl = $"https://{{pifd-endpoint}}/CheckoutDescriptions?redirectUrl={redirectUrl}&paymentProviderId={paymentProviderId}&partner={partner}&operation=RenderPidlPage&country={country}&language={language}&checkoutId={checkoutId}&family=&type=&scenario=pidlContext";

                string payUrl = $"https://{{pifd-endpoint}}/checkoutsEx/{checkoutId}/charge?country={country}&language={language}&partner={partner}&paymentProviderId={paymentProviderId}&redirectUrl={redirectUrl}";

                var pidls = await this.GetCheckoutDescriptionFromFactory(
                    partner: partner,
                    country: buyerCountry,
                    language: language,
                    paySubmitUrl: payUrl,
                    scenario: pidlScenario,
                    traceActivityId: traceActivityId,
                    family: family,
                    type: type,
                    paymentProviderId: paymentProviderId,
                    redirectUrl: redirectUrl,
                    checkoutId: checkoutId,
                    sellerCountry: seller.Country ?? GlobalConstants.Defaults.Country,
                    backButtonUrl: backButtonUrl,
                    setting: setting);

                // 5. Update payment information and terms URL in the PIDL(s)
                foreach (var pidl in pidls)
                {
                    var checkoutItemDetails = pidl.GetDisplayHintById("paymentInformation") as ContentDisplayHint;
                    decimal amount = Iso4217AmountConverter.ConvertFromIso4217Amount(paymentRequest.Product.Price.Currency.Code, paymentRequest.Product.Price.Amount);
                    checkoutItemDetails.DisplayContent = string.Format(checkoutItemDetails.DisplayContent, paymentRequest.Product.Price.Currency.Code, amount.ToString("0.00", CultureInfo.CreateSpecificCulture(language)), seller.Name);

                    var termsDisplayHint = pidl.GetDisplayHintById("termsCheckout") as HyperlinkDisplayHint;
                    termsDisplayHint.SourceUrl = CheckoutHelper.GetCheckoutTermsURL(this.Settings.StaticResourceServiceBaseUrl, language);
                    termsDisplayHint.Action.Context = CheckoutHelper.GetCheckoutTermsURL(this.Settings.StaticResourceServiceBaseUrl, language);

                    if (string.Equals(paymentRequest.Context, GlobalConstants.PaymentRequestContext.PrePaidMeeting, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var productName = pidl.GetDisplayHintById("productName") as ContentDisplayHint;
                        productName.DisplayContent = paymentRequest?.Product?.Name;

                        var productDescription = pidl.GetDisplayHintById("productDescription") as ContentDisplayHint;
                        productDescription.DisplayContent = paymentRequest?.Product?.Description;
                    }

                    if (pidl.PidlResourceStrings == null)
                    {
                        pidl.PidlResourceStrings = new PidlResourceStrings();
                    }

                    var backButton = pidl.GetDisplayHintById("backButton") as ButtonDisplayHint;
                    if (!string.Equals(scenario, Constants.ScenarioNames.PidlClientAction, StringComparison.OrdinalIgnoreCase))
                    {   
                        if (backButton != null)
                        {
                            backButton.IsHidden = true;
                        }
                    }
                    else
                    {
                        // Constants.ScenarioNames.PidlClientAction represents Paypal PM selection scenario and Cancel button should be hidden
                        // for such scenarios on the Payment forms and back button should be visible
                        var cancelButton = pidl.GetDisplayHintById("cancelButton") as ButtonDisplayHint;
                        if (cancelButton != null)
                        {
                            cancelButton.IsHidden = true;
                        }

                        if (backButton != null)
                        {
                            backButton.IsHidden = false;
                        }
                    }

                    pidl.PidlResourceStrings.AddOrUpdateServerErrorCode(
                        Constants.ThirdPartyPaymentsErrorCodes.InvalidPaymentInstrument,
                        new ServerErrorCode()
                        {
                            Target = string.Format(
                                    "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                                    Constants.ThirdPartyPaymentsErrorTargets.CardHolderName,
                                    Constants.ThirdPartyPaymentsErrorTargets.CardNumber,
                                    Constants.ThirdPartyPaymentsErrorTargets.ExpiryMonth,
                                    Constants.ThirdPartyPaymentsErrorTargets.ExpiryYear,
                                    Constants.ThirdPartyPaymentsErrorTargets.Cvv,
                                    Constants.ThirdPartyPaymentsErrorTargets.AddressLine1,
                                    Constants.ThirdPartyPaymentsErrorTargets.AddressLine2,
                                    Constants.ThirdPartyPaymentsErrorTargets.City,
                                    Constants.ThirdPartyPaymentsErrorTargets.State,
                                    Constants.ThirdPartyPaymentsErrorTargets.ZipCode),
                            ErrorMessage = LocalizationRepository.Instance.GetLocalizedString(Constants.ThirdPartyPaymentsErrorMessages.InvalidPaymentInstrument, language),
                            RetryPolicy = new RetryPolicy()
                            {
                                RetryPolicyType = RetryPolicyType.limitedRetry,
                                Context = new RetryPolicyContext()
                                {
                                    MaxRetryCount = 3
                                }
                            }
                        });
                    pidl.PidlResourceStrings.AddOrUpdateServerErrorCode(
                        Constants.ThirdPartyPaymentsErrorCodes.CvvValueMismatch,
                        new ServerErrorCode()
                        {
                            Target = string.Format(
                                    "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                                    Constants.ThirdPartyPaymentsErrorTargets.CardHolderName,
                                    Constants.ThirdPartyPaymentsErrorTargets.CardNumber,
                                    Constants.ThirdPartyPaymentsErrorTargets.ExpiryMonth,
                                    Constants.ThirdPartyPaymentsErrorTargets.ExpiryYear,
                                    Constants.ThirdPartyPaymentsErrorTargets.Cvv,
                                    Constants.ThirdPartyPaymentsErrorTargets.AddressLine1,
                                    Constants.ThirdPartyPaymentsErrorTargets.AddressLine2,
                                    Constants.ThirdPartyPaymentsErrorTargets.City,
                                    Constants.ThirdPartyPaymentsErrorTargets.State,
                                    Constants.ThirdPartyPaymentsErrorTargets.ZipCode),
                            ErrorMessage = LocalizationRepository.Instance.GetLocalizedString(Constants.ThirdPartyPaymentsErrorMessages.CvvValueMismatch, language),
                            RetryPolicy = new RetryPolicy()
                            {
                                RetryPolicyType = RetryPolicyType.limitedRetry,
                                Context = new RetryPolicyContext()
                                {
                                    MaxRetryCount = 3
                                }
                            }
                        });
                    pidl.PidlResourceStrings.AddOrUpdateServerErrorCode(
                        Constants.ThirdPartyPaymentsErrorCodes.ExpiredPaymentInstrument,
                        new ServerErrorCode()
                        {
                            Target = string.Format("{0},{1}", Constants.ThirdPartyPaymentsErrorTargets.ExpiryMonth, Constants.ThirdPartyPaymentsErrorTargets.ExpiryYear),
                            ErrorMessage = LocalizationRepository.Instance.GetLocalizedString(Constants.ThirdPartyPaymentsErrorMessages.ExpiredPaymentInstrument, language),
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

                // During the paypal provider flow, from PM selection page if user picks one and click next(restAction attached) then the ResourceActionContext will be sent to client first and the client 
                // will initiates another call with that context info to get the actual checkout form pidl
                // Below logic is to send Resourcecontext info to the client when scenario is Constants.ScenarioNames.PidlContext
                if (string.Equals(scenario, Constants.ScenarioNames.PidlContext, StringComparison.OrdinalIgnoreCase))
                {
                    PidlDocInfo info = new PidlDocInfo();
                    Dictionary<string, string> parameters = new Dictionary<string, string>() 
                    { 
                      { "checkoutId", checkoutId }, { "language", language }, { "redirectUrl", redirectUrl },
                      { "paymentProviderId", paymentProviderId }, { "partner", partner }, { "operation", "RenderPidlPage" }, { "family", family },
                      { "type", type }, { "scenario", Constants.ScenarioNames.PidlClientAction }, { "country", country } 
                    };

                    info.SetParameters(parameters);
                    info.ResourceType = Constants.DescriptionTypes.Checkout;
                    info.AnonymousPidl = true;

                    ActionContext context = new ActionContext()
                    {
                        ResourceActionContext = new ResourceActionContext()
                        {
                            Action = PaymentInstrumentActions.ToString(PIActionType.CollectResourceInfo),
                            PidlDocInfo = info
                        }
                    };

                    ClientAction clientAction = new ClientAction(ClientActionType.Pidl, context);
                    PIDLResource pidlResource = new PIDLResource { ClientAction = clientAction };
                    return pidlResource;
                }

                return pidls;
            }
            catch (ServiceErrorResponseException ex)
            {
                List<PIDLResource> checkoutErrorPidl = PIDLResourceFactory.Instance.GetStaticCheckoutErrorDescriptions("en-us", redirectUrl, Constants.PartnerName.MSTeams);
                var errorSubText = checkoutErrorPidl[0].GetDisplayHintById("paymentErrorSubText") as ContentDisplayHint;
                var errorCV = checkoutErrorPidl[0].GetDisplayHintById("paymentErrorCV") as ContentDisplayHint;
                errorCV.DisplayContent = string.Format(errorCV.DisplayContent, cV);
                if (Constants.ThirdPartyPaymentTerminalErrorsTypeOne.Contains(ex.Error.ErrorCode))
                {
                    errorSubText.DisplayContent = string.Format(errorSubText.DisplayContent, "Please try again.");
                }

                ClientAction clientAction = new ClientAction(ClientActionType.Pidl, checkoutErrorPidl);

                return new List<PIDLResource> { new PIDLResource { ClientAction = clientAction } };
            }
        }

        private static bool IsAllCreditCard(IEnumerable<PaymentMethod> paymentMethods)
        {
            return paymentMethods.Aggregate<PaymentMethod, bool>(true, (allCC, pm) => allCC && IsCreditCardNotCup(pm));
        }

        private static bool IsCreditCardNotCup(PaymentMethod method)
        {
            return method.PaymentMethodFamily.Equals(Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                && !(method.PaymentMethodType.Equals(Constants.PaymentMethodFamily.unionpay_creditcard.ToString(), StringComparison.OrdinalIgnoreCase)
                || method.PaymentMethodType.Equals(Constants.PaymentMethodFamily.unionpay_debitcard.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsAllPayPal(IEnumerable<PaymentMethod> paymentMethods)
        {
            return paymentMethods.Aggregate<PaymentMethod, bool>(true, (allPP, pm) => allPP && IsPayPal(pm));
        }

        private static bool IsPayPal(PaymentMethod method)
        {
            return method.PaymentMethodFamily.Equals(Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                && method.PaymentMethodType.Equals(Constants.PaymentMethodType.PayPal.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool ShouldRenderCheckoutForm(IEnumerable<PimsModel.V4.PaymentMethod> filteredPaymentMethods, string family, string type)
        {
            return IsAllCreditCard(filteredPaymentMethods) || IsAllPayPal(filteredPaymentMethods) || !(string.IsNullOrEmpty(family) && string.IsNullOrEmpty(type));
        }

        private async Task<List<PIDLResource>> GetCheckoutDescriptionFromFactory(
            string partner,
            string country,
            string language,
            string paySubmitUrl,
            string scenario,
            EventTraceActivity traceActivityId,
            string family,
            string type,
            string paymentProviderId,
            string redirectUrl,
            string checkoutId,
            string sellerCountry,
            string backButtonUrl,
            PaymentExperienceSetting setting = null)
        {
            IEnumerable<PimsModel.V4.PaymentMethod> paymentMethods = null;
            paymentMethods = await this.Settings.PIMSAccessor.GetThirdPartyPaymentMethods(paymentProviderId, sellerCountry, country, traceActivityId, partner, language, null, this.ExposedFlightFeatures);

            HashSet<PimsModel.V4.PaymentMethod> paymentMethodHashSet = new HashSet<PimsModel.V4.PaymentMethod>(paymentMethods);
            List<PIDLResource> retVal;

            if (ShouldRenderCheckoutForm(paymentMethods, family, type))
            {
                // return a checkout form if there is only one type of PM available for the provider
                if (!string.IsNullOrEmpty(family) && !string.IsNullOrEmpty(type))
                {
                    paymentMethodHashSet = paymentMethodHashSet.Where(x => string.Equals(x.PaymentMethodFamily, family, StringComparison.OrdinalIgnoreCase) && type.IndexOf(x.PaymentMethodType, StringComparison.OrdinalIgnoreCase) >= 0).ToHashSet();
                }

                retVal = PIDLResourceFactory.Instance.GetCheckoutDescriptions(
                    paymentMethods: paymentMethodHashSet,
                    operation: Constants.Operations.Add,
                    partnerName: partner,
                    country: country,
                    language: language,
                    paySubmitUrl: paySubmitUrl,
                    scenario: scenario,
                    backButtonUrl: backButtonUrl,
                    setting: setting);

                foreach (PIDLResource pidl in retVal)
                {
                    if (string.Equals(Constants.PaymentMethodFamily.credit_card.ToString(), pidl.Identity["family"], StringComparison.OrdinalIgnoreCase))
                    {
                        // Remove all address related fields other than zip code
                        pidl.RemoveFirstDataDescriptionByPropertyName("address_line1");
                        pidl.RemoveFirstDataDescriptionByPropertyName("address_line2");
                        pidl.RemoveFirstDataDescriptionByPropertyName("address_line3");
                        pidl.RemoveFirstDataDescriptionByPropertyName("city");
                        pidl.RemoveFirstDataDescriptionByPropertyName("region");

                        List<string> removeDisplayHintIds = new List<string>()
                        {
                            "addressLine1",
                            "addressLine2",
                            "addressLine3",
                            "addressCity",
                            "addressState",
                            "addressProvince",
                            "addressCounty",
                        };

                        // For paypal we shouldn't be collecting Contact information such as email
                        if (string.Equals(Constants.PaymentProviderIds.PayPal.ToString(), paymentProviderId))
                        {
                            pidl.RemoveFirstDataDescriptionByPropertyName("receiptEmailAddress");

                            removeDisplayHintIds.Add("paymentConfirmationText");
                            removeDisplayHintIds.Add("contactInformationHeading");
                            removeDisplayHintIds.Add("emailAddressCheckout");
                        }

                        List<DisplayHint> removeDisplayHints = new List<DisplayHint>();
                        foreach (string removeDisplayHintId in removeDisplayHintIds)
                        {
                            var displayHint = pidl.GetDisplayHintById(removeDisplayHintId);
                            if (displayHint != null)
                            {
                                removeDisplayHints.Add(displayHint);
                            }
                        }

                        foreach (PageDisplayHint page in pidl.DisplayPages)
                        {
                            // in meeting removal address
                            foreach (ContainerDisplayHint group in page.Members)
                            {
                                foreach (var displayHint in removeDisplayHints)
                                {
                                    if (displayHint != null)
                                    {
                                        group.RemoveDisplayHint(displayHint);
                                    }
                                }
                            }
                        }

                        // Enable country drop down
                        pidl.SetPropertyState(Constants.DataDescriptionPropertyNames.Country, true);

                        // Swap display positions of Zip code and Country
                        if (pidl.DisplayPages != null && pidl.DisplayPages.Count == 1)
                        {
                            var countryIndex = pidl.DisplayPages[0].Members.FindIndex(hint => hint.HintId == "addressCountry");
                            var postalCodeIndex = pidl.DisplayPages[0].Members.FindIndex(hint => hint.HintId == "addressPostalCode");
                            if (countryIndex > 0 && postalCodeIndex > 0)
                            {
                                var tempMember = pidl.DisplayPages[0].Members[countryIndex];
                                pidl.DisplayPages[0].Members[countryIndex] = pidl.DisplayPages[0].Members[postalCodeIndex];
                                pidl.DisplayPages[0].Members[postalCodeIndex] = tempMember;
                            }
                        }
                    }
                }
            }
            else
            {
                // return a select PM form if there are more than one type of PM available for the provider
                retVal = PIDLResourceFactory.GetCheckoutPaymentSelectDescriptions(paymentMethodHashSet, country, Constants.Operations.Select, language, partner, paymentProviderId, checkoutId, redirectUrl, null, this.ExposedFlightFeatures, scenario, setting: setting);
            }

            return retVal;
        }
    }
}