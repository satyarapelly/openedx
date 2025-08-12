// <copyright file="RewardsDescriptionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Model.RewardsService;
    using Newtonsoft.Json;

    [ApiController]
    [Route("api/[controller]")]
    public class RewardsDescriptionsController : ProxyController
    {
        /// <summary>
        /// Returns a Rewards PIDL for the given Rewards context.
        /// </summary>
        /// <group>RewardsDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/rewardsDescriptions</url>
        /// <param name="accountId">User's account id</param>
        /// <param name="type">Rewards type</param>
        /// <param name="operation">Operation name</param>
        /// <param name="country">country code</param>
        /// <param name="language">language code</param>
        /// <param name="partner">Partner name</param>
        /// <param name="rewardsContextData">the context to get rewards PIDL</param>
        /// <response code="200">List&lt;PIDLResource&gt;</response>
        /// <returns>Returns a rewards PIDL for the given rewardscontext</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        [Route("[action]")]
        public async Task<List<PIDLResource>> Get(
            string accountId,
            string type,
            string operation,
            string country = null,
            string language = null,
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string rewardsContextData = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            RewardsContextData rewardsContext = JsonConvert.DeserializeObject<RewardsContextData>(rewardsContextData);
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);

            string userId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);

            Version fullPidlSdkVersion = HttpRequestHelper.GetFullPidlSdkVersion(this.Request.ToHttpRequestMessage());

            long rewardsPoints = 0;
            decimal rewardsAmount = 0;
            string catalogSku = string.Empty;
            bool isVariableAmountSku = false;
            string rewardsCurrency = null;

            // Get user's rewards balance
            GetUserInfoResult userInfo = await this.Settings.MSRewardsServiceAccessor.GetUserInfo(userId, country, traceActivityId);

            if (userInfo?.Balance > 0)
            {
                // Get all available Microsoft Gift Card SKUs for the user
                string isCSVGiftCard = string.Empty;
                var catalogItems = userInfo.CatalogItems.Where(
                    c => string.Equals(c.Provider, "csv", System.StringComparison.OrdinalIgnoreCase)
                    && (c?.Attributes?.TryGetValue("isMicrosoftCSVGiftCard", out isCSVGiftCard) ?? false)
                    && string.Equals(isCSVGiftCard, "true", System.StringComparison.OrdinalIgnoreCase)
                    && c.Price <= userInfo.Balance).ToList();

                // Select the highest amount Microsoft Gift Card SKU that is less than or equal to the order amount
                UserFacingCatalogItem selectedCatalogItem = null;
                decimal skuAmount = 0;
                
                long variableAmountPoints = 0;
                foreach (var catalogItem in catalogItems)
                {
                    decimal amount = 0;
                    string amountVal = string.Empty;
                    decimal conversionRatio = 0;
                    string conversionRatioVal = string.Empty;
                    string isVariableAmount = string.Empty;

                    if ((catalogItem?.Configuration?.TryGetValue("isVariableAmount", out isVariableAmount) ?? false)
                        && string.Equals(isVariableAmount, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        // Ignore variable amount gift cards if the flight is enabled
                        if (this.ExposedFlightFeatures?.Contains(Flighting.Features.PXDisableMSRewardsVariableAmount, StringComparer.OrdinalIgnoreCase) ?? false)
                        {
                            continue;
                        }
                        else if ((catalogItem?.Attributes?.TryGetValue("variableItemPointsToCurrencyConversionRatio", out conversionRatioVal) ?? false)
                            && decimal.TryParse(conversionRatioVal, out conversionRatio))
                        {
                            string rangeMaxVal = string.Empty;
                            decimal rangeMax = 0;
                            string rangeMinVal = string.Empty;
                            decimal rangeMin = 0;
                            if ((catalogItem?.Configuration?.TryGetValue("variableRangeMax", out rangeMaxVal) ?? false)
                                && decimal.TryParse(rangeMaxVal, out rangeMax)
                                && (catalogItem?.Configuration?.TryGetValue("variableRangeMin", out rangeMinVal) ?? false)
                                && decimal.TryParse(rangeMinVal, out rangeMin)
                                && rangeMin <= (rewardsContext?.OrderAmount ?? 0))
                            {
                                try
                                {
                                    decimal maxPointsAmount = userInfo.Balance / conversionRatio;
                                    int variableAmount = (int)Math.Min(rewardsContext?.OrderAmount ?? 0, Math.Min(rangeMax, maxPointsAmount));
                                    if (variableAmount >= rangeMin && variableAmount > skuAmount)
                                    {
                                        selectedCatalogItem = catalogItem;
                                        variableAmountPoints = (long)Math.Round(variableAmount * conversionRatio / 5, MidpointRounding.AwayFromZero) * 5;
                                        skuAmount = variableAmount;
                                        isVariableAmountSku = true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    SllWebLogger.TracePXServiceException($"Failed to extract variable amount sku details. Error: {ex.Message}", traceActivityId);
                                }
                            }
                        }
                    }
                    else if ((catalogItem?.Configuration?.TryGetValue("amount", out amountVal) ?? false)
                        && decimal.TryParse(amountVal, out amount)
                        && amount <= rewardsContext?.OrderAmount
                        && amount > skuAmount)
                    {
                        selectedCatalogItem = catalogItem;
                        skuAmount = amount;
                        isVariableAmountSku = false;
                    }
                }

                if (selectedCatalogItem != null)
                {
                    // Show rewards checkbox
                    rewardsPoints = isVariableAmountSku ? variableAmountPoints : selectedCatalogItem.Price;
                    rewardsAmount = skuAmount;
                    catalogSku = selectedCatalogItem.Name;
                    selectedCatalogItem.Configuration?.TryGetValue("currencyCode", out rewardsCurrency);
                }
            }

            var retVal = PIDLResourceFactory.Instance.GetRewardsDescriptions(type, operation, country, language, partner);

            if (string.Equals("select", operation, StringComparison.InvariantCultureIgnoreCase))
            {
                decimal orderAmount = rewardsContext?.OrderAmount ?? 0;
                Tuple<decimal, decimal, decimal, string, string> getCSVBalanceData = await this.GetCSVBalanceData(accountId, partner, country, language, orderAmount, rewardsAmount, traceActivityId);
                ProcessSelectMSRewardsPIDL(retVal, country, language, rewardsContext?.Currency, rewardsAmount, rewardsPoints, rewardsCurrency, getCSVBalanceData.Item1, getCSVBalanceData.Item2, getCSVBalanceData.Item3, getCSVBalanceData.Item4, getCSVBalanceData.Item5, this.ExposedFlightFeatures, fullPidlSdkVersion);
            }
            else if (string.Equals("redeem", operation, StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(catalogSku))
                {
                    // empty catalogSku indicates user does not have enough balance to redeem, so we return a failure client action 
                    PIDLResource retPIDL = new PIDLResource();
                    ClientAction clientAction = new ClientAction(ClientActionType.Failure)
                    {
                        Context = new { errorCode = RewardsErrorCode.E_INSUFFICIENT_BALANCE, errorCodeName = RewardsErrorCode.E_INSUFFICIENT_BALANCE.ToString(), errorMessage = Constants.MSRewardsErrorMessages.InsufficientBalance }
                    };
                    retPIDL.ClientAction = clientAction;
                    return new List<PIDLResource> { retPIDL };
                }

                ProcessRedeemMSRewardsPIDL(retVal, country, language, partner, catalogSku, isVariableAmountSku ? rewardsAmount : (decimal?)null);
            }

            FeatureContext featureContext = new FeatureContext(
                    country,
                    GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, type, null),
                    Constants.DescriptionTypes.RewardsDescription,
                    operation,
                    scenario: null,
                    language,
                    null,
                    this.ExposedFlightFeatures,
                    setting?.Features,
                    paymentMethodfamily: null,
                    type,
                    smdMarkets: null,
                    originalPartner: partner,
                    isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request));

            PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

            return retVal;
        }

        private static void ProcessSelectMSRewardsPIDL(
            List<PIDLResource> pidlResources,
            string country,
            string language,
            string currency,
            decimal rewardsAmount,
            long rewardsPoints,
            string pointsValueCurrency,
            decimal csvBalance,
            decimal csvBalanceApplied,
            decimal csvBalanceAppliedAfterRewards,
            string csvCurrency,
            string csvPiid,
            List<string> exposedFlightFeatures = null,
            Version fullPidlSdkVersion = null)
        {
            string formattedCsvBalanceAppliedAfterRewards = CurrencyHelper.FormatCurrency(country, language, csvBalanceAppliedAfterRewards, currency);
            string formattedRewardsAmount = CurrencyHelper.FormatCurrency(country, language, rewardsAmount, currency);
            string formattedcsvBalanceApplied = CurrencyHelper.FormatCurrency(country, language, csvBalanceApplied, currency);
            string formattedCsvBalance = CurrencyHelper.FormatCurrency(country, language, csvBalance, currency);

            // to add locale to the rewards points (ex: 1000 changes to 1,000)
            var cultureInfo = PidlFactory.Helper.GetCultureInfo(language);
            string rewardsPointsText = rewardsPoints.ToString("#,#", cultureInfo);

            Dictionary<string, string> defaultValueToBeUpdated = new Dictionary<string, string>()
            {
                { Constants.PropertyDescriptionIds.PointsValueCurrency, pointsValueCurrency },
                { Constants.PropertyDescriptionIds.CsvCurrency, csvCurrency },
                { Constants.PropertyDescriptionIds.CsvPiid, csvPiid },
                { Constants.PropertyDescriptionIds.CsvBalance, csvBalance.ToString() },
                { Constants.DisplayHintIds.CsvTotal, csvBalanceApplied.ToString() },
                { Constants.DisplayHintIds.PointsValueTotal, rewardsAmount.ToString() },
                { Constants.DisplayHintIds.FormattedCsvTotal, formattedcsvBalanceApplied },
                { Constants.DisplayHintIds.FormattedPointsValueTotal, formattedRewardsAmount },
            };

            List<string> isConditionalFieldValueToBeUpdated = new List<string>
            {
                Constants.PropertyDescriptionIds.UseRedeemPoints,
                Constants.PropertyDescriptionIds.UseCsv
            };

            Dictionary<string, List<string>> conditionalFieldsToBeFormatted = new Dictionary<string, List<string>>
            {
                { Constants.DisplayHintIds.CsvTotal, new List<string> { csvBalanceAppliedAfterRewards.ToString(), csvBalanceApplied.ToString() } },
                { Constants.DisplayHintIds.FormattedCsvTotal, new List<string> { formattedCsvBalanceAppliedAfterRewards.ToString(), formattedcsvBalanceApplied.ToString() } },
                { Constants.DisplayHintIds.PointsValueTotal, new List<string> { rewardsAmount.ToString() } },
                { Constants.DisplayHintIds.FormattedPointsValueTotal, new List<string> { formattedRewardsAmount.ToString() } }
            };

            foreach (var pidlResource in pidlResources)
            {
                FormatPointsRedemptionContentLineGroup(pidlResource, rewardsPointsText, exposedFlightFeatures, fullPidlSdkVersion);

                foreach (var item in defaultValueToBeUpdated)
                {
                    var dataDescription = pidlResource.DataDescription[item.Key] as PropertyDescription;
                    if (dataDescription != null)
                    {
                        dataDescription.DefaultValue = item.Value;
                    }
                }

                for (int i = 0; i < isConditionalFieldValueToBeUpdated.Count; i++)
                {
                    var dataDescription = pidlResource.DataDescription[isConditionalFieldValueToBeUpdated[i]] as PropertyDescription;
                    if (dataDescription != null)
                    {
                        dataDescription.IsConditionalFieldValue = true;
                    }
                }

                // if user has no reward points and no csv balance, return empty page
                if (rewardsAmount == 0 && csvBalanceApplied == 0)
                {
                    PageDisplayHint page = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.SelectMSRewardsPage) as PageDisplayHint;
                    if (page != null)
                    {
                        page.Members.Clear();

                        // page should have at least an empty display hint to render
                        page.Members.Add(new TextDisplayHint() { HintId = Constants.DisplayHintIds.DummyText });
                        continue;
                    }
                }

                // remove rewards group if rewardsAmount is 0F
                RemoveGroupIfConditionMet(pidlResource, Constants.DisplayHintIds.PointsRedemptionGroup, rewardsAmount == 0);

                // remove CSV group if csvBalance that can be appllied is 0
                RemoveGroupIfConditionMet(pidlResource, Constants.DisplayHintIds.UseCsvGroup, csvBalanceApplied == 0);

                if (csvBalanceApplied > 0)
                {
                    // enable the CSV checkbox by default if csv balance is greater than 0
                    var dataDescription = pidlResource.DataDescription[Constants.PropertyDescriptionIds.UseCsv] as PropertyDescription;
                    if (dataDescription != null)
                    {
                        dataDescription.DefaultValue = true;
                    }
                }

                var useCsvContentLine1 = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.UseCsvContentLine1) as TextDisplayHint;
                if (useCsvContentLine1 != null)
                {
                    useCsvContentLine1.DisplayContent += "\n";
                }

                var textDisplayHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.UseCsvContentLine2) as TextDisplayHint;
                if (textDisplayHint != null)
                {
                    textDisplayHint.DisplayContent = string.Format(textDisplayHint.DisplayContent, formattedCsvBalance);
                }

                foreach (var item in conditionalFieldsToBeFormatted)
                {
                    var propertyDisplayHint = pidlResource.GetDisplayHintById(item.Key) as PropertyDisplayHint;
                    if (propertyDisplayHint != null)
                    {
                        string currentValue = propertyDisplayHint.ConditionalFields["value"];
                        if (item.Value.Count == 2)
                        {
                            propertyDisplayHint.ConditionalFields["value"] = string.Format(currentValue, item.Value[0], item.Value[1]);
                        }
                        else if (item.Value.Count == 1)
                        {
                            propertyDisplayHint.ConditionalFields["value"] = string.Format(currentValue, item.Value[0]);
                        }
                    }
                }
            }
        }

        private static void ProcessRedeemMSRewardsPIDL(
            List<PIDLResource> pidlResources,
            string country,
            string language,
            string partner,
            string catalogSku,
            decimal? catalogSkuVariableAmount)
        {
            foreach (var pidlResource in pidlResources)
            {
                PageDisplayHint redeemMSRewardsPage = new PageDisplayHint
                {
                    HintId = Constants.DisplayHintIds.RedeemMSRewardsPage,
                    DisplayName = Constants.DisplayHintIds.RedeemMSRewardsPage
                };

                // page should have at least an empty display hint to render
                redeemMSRewardsPage.Members.Add(new TextDisplayHint());

                var actionContext = new PXCommon.RestLink();
                actionContext.Href = $"{Constants.SubmitUrls.RedeemMSRewards}?partner={partner}&language={language}&country={country}";
                actionContext.Payload = new { catalogItem = catalogSku, catalogItemAmount = catalogSkuVariableAmount };
                actionContext.Method = "POST";
                redeemMSRewardsPage.Action = new DisplayHintAction(DisplayHintActionType.restAction.ToString(), false, actionContext, null);
                pidlResource.AddDisplayPages(new List<PageDisplayHint> { redeemMSRewardsPage });
            }
        }

        private static void FormatPointsRedemptionContentLineGroup(PIDLResource pidlResource, string rewardsPoints, List<string> exposedFlightFeatures = null, Version pidlSdkVersion = null)
        {
            if (pidlResource != null)
            {
                GroupDisplayHint groupDisplayHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PointsRedemptionContentLineGroup) as GroupDisplayHint;
                TextDisplayHint pointsRedemptionContentLine1 = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PointsRedemptionContentLine1) as TextDisplayHint;
                Version lowestCompatiblePidlVersion = new Version(Constants.PidlSdkVersionNumber.PidlSdkMajor2, Constants.PidlSdkVersionNumber.PidlSdkMinor5, Constants.PidlSdkVersionNumber.PidlSdkBuild7, Constants.PidlSdkVersionNumber.PidlSdkAlpha0);

                // TODO: Remove the check for the lowestCompatiblePidlVersion once client is updated with the 2.5.7 version
                bool updatePointsRedemptionLine1Content = pidlSdkVersion != null && lowestCompatiblePidlVersion != null && pidlSdkVersion >= lowestCompatiblePidlVersion;

                if (groupDisplayHint != null && pointsRedemptionContentLine1 != null)
                {
                    string displayContent = pointsRedemptionContentLine1.DisplayContent;

                    // Split the string by the delimiter "{0}"
                    List<string> parts = displayContent.Split(new string[] { "{0}" }, StringSplitOptions.None).ToList();
                    parts.Insert(1, string.Format("{0}", rewardsPoints));
                    groupDisplayHint.Members.Clear();
                    for (int i = 0; i < parts.Count; i++)
                    {
                        string subString = parts[i].Trim();
                        TextDisplayHint textDisplayHint = new TextDisplayHint();
                        textDisplayHint.HintId = "PointsRedemptionContentLine" + i;
                        switch (i)
                        {
                            case 0:
                                // add a space after the first string
                                if (updatePointsRedemptionLine1Content)
                                {
                                    textDisplayHint.DisplayContent = !string.IsNullOrEmpty(subString) ? subString + " " : string.Empty;
                                }
                                else
                                {
                                    textDisplayHint.DisplayContent = subString + " ";
                                }

                                break;
                            case 1:
                                textDisplayHint.StyleHints = new List<string> { "text-accent" };
                                textDisplayHint.DisplayContent = subString;

                                // If the last part is empty, add a new line at the end of the second part
                                if (parts.Count > 2 && string.IsNullOrEmpty(parts[2].Trim()) && !updatePointsRedemptionLine1Content)
                                {
                                    textDisplayHint.DisplayContent += "\n";
                                }

                                break;
                            case 2:
                                // add a space before the last string and add a new line at the end
                                textDisplayHint.DisplayContent = " " + subString;
                                if (!updatePointsRedemptionLine1Content)
                                {
                                    textDisplayHint.DisplayContent += "\n";
                                }

                                break;
                        }

                        textDisplayHint.IsHidden = string.IsNullOrEmpty(textDisplayHint.DisplayContent.Trim()) ? true : false;
                        groupDisplayHint.Members.Add(textDisplayHint);
                    }
                }
            }
        }

        private static void RemoveGroupIfConditionMet(PIDLResource pidlResource, string groupId, bool condition)
        {
            if (condition)
            {
                pidlResource.RemoveDisplayHintById(groupId);
            }
        }

        private async Task<Tuple<decimal, decimal, decimal, string, string>> GetCSVBalanceData(string accountId, string partner, string country, string language, decimal orderAmount, decimal rewardsAmount, EventTraceActivity traceActivityId)
        {
            decimal csvBalance = 0;
            decimal csvBalanceApplied = 0;
            decimal csvBalanceAppliedAfterRewards = 0;
            string csvCurrency = null;
            string csvPiid = null;

            // Get user's CSV PI
            string[] statusList = new string[] { Constants.PaymentInstrumentStatus.Active };
            List<KeyValuePair<string, string>> queryParams = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(Constants.QueryParameterName.Language, language),
                new KeyValuePair<string, string>(Constants.QueryParameterName.Country, country)
            };

            IList<PaymentInstrument> paymentInstruments = await this.Settings.PIMSAccessor.ListPaymentInstrument(accountId, 0, statusList, traceActivityId, queryParams, partner, language: language, country: country, exposedFlightFeatures: this.ExposedFlightFeatures);

            var csvPI = paymentInstruments.FirstOrDefault(pi => pi.IsCSV());

            if (csvPI?.PaymentInstrumentDetails?.Balance > 0)
            {
                // Show CSV checkbox
                csvBalance = csvPI.PaymentInstrumentDetails.Balance;
                csvBalanceApplied = Math.Min(csvBalance, orderAmount);
                csvBalanceAppliedAfterRewards = Math.Min(orderAmount - rewardsAmount, csvBalance);
                csvCurrency = csvPI?.PaymentInstrumentDetails?.Currency;
                csvPiid = csvPI?.PaymentInstrumentId;
            }

            return new Tuple<decimal, decimal, decimal, string, string>(csvBalance, csvBalanceApplied, csvBalanceAppliedAfterRewards, csvCurrency, csvPiid);
        }
    }
}