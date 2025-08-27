// <copyright file="CSVTokenHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Model.CatalogService;
    using Microsoft.Commerce.Payments.PXService.Model.PurchaseService;
    using Microsoft.Commerce.Payments.PXService.Settings;
    using Microsoft.Commerce.Tracing;

    public class CSVTokenHelper
    {
        public static async Task<CSVTokenValidationResult> ValidateCSVToken(PXServiceSettings pxSettings, string puid, string tokenValue, string market, string language, string clientIP, EventTraceActivity traceActivityId)
        {
            CSVTokenValidationResult tokenValidationResult = new CSVTokenValidationResult();
            TokenPolicyDescription tokenPolicyDescription = null;
            try
            {
                // Get token details
                tokenPolicyDescription = await pxSettings.TokenPolicyServiceAccessor.GetTokenDescriptionAsync(puid, tokenValue, market, language, clientIP, traceActivityId);
            }
            catch (ServiceErrorResponseException ex)
            {
                SllWebLogger.TracePXServiceException($"Exception Calling Token policy description service: " + ex.ToString(), traceActivityId);

                if (string.Equals(ex.Error?.ErrorCode, HttpStatusCode.NotFound.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    tokenValidationResult.TokenStatus = CSVTokenStatus.TokenNotFound;
                    return tokenValidationResult;
                }
            }

            // Check if token is redeemable and has asset value
            if (tokenPolicyDescription != null
                    && (tokenPolicyDescription.PolicyEvaluation?.IsRedeemable ?? false)
                    && tokenPolicyDescription.Asset?.AssetValue?.Value != null
                    && !string.IsNullOrEmpty(tokenPolicyDescription.Asset?.AssetValue?.ValueMeasurement))
            {
                TokenCategory tokenCategory = tokenPolicyDescription.TokenCategory;
                if (tokenCategory == TokenCategory.Other && (tokenPolicyDescription.CatalogInfo?.MatchingAvailabilities?.Any() ?? false))
                {
                    // GET BIG IDs
                    List<string> bigIds = tokenPolicyDescription.CatalogInfo?.MatchingAvailabilities?.Select(a =>
                    {
                        if (!string.IsNullOrEmpty(a.AvailabilityId))
                        {
                            return $"{a.ProductId}/{a.SkuId}/{a.AvailabilityId}";
                        }

                        if (!string.IsNullOrEmpty(a.SkuId))
                        {
                            return $"{a.ProductId}/{a.SkuId}";
                        }

                        return a.ProductId;
                    })?.ToList();

                    if (bigIds?.Any() ?? false)
                    {
                        // Get token category by querying the catalog service
                        Catalog catalog = await pxSettings.CatalogServiceAccessor.GetProducts(bigIds, market, language, null, "Redeem", traceActivityId);

                        if (catalog?.Products?.Any(p => string.Equals(p.ProductType, "CSV", StringComparison.OrdinalIgnoreCase)) ?? false)
                        {
                            tokenCategory = TokenCategory.Csv;
                        }
                    }
                }

                if (tokenCategory == TokenCategory.Csv)
                {
                    tokenValidationResult.TokenStatus = CSVTokenStatus.ValidCSVToken;
                    tokenValidationResult.TokenValue = tokenPolicyDescription.Asset.AssetValue.Value;
                    tokenValidationResult.TokenCurrency = tokenPolicyDescription.Asset.AssetValue.ValueMeasurement;
                }
                else
                {
                    tokenValidationResult.TokenStatus = CSVTokenStatus.NonCSVToken;
                }
            }
            else if (tokenPolicyDescription?.PolicyEvaluation != null && !tokenPolicyDescription.PolicyEvaluation.IsRedeemable)
            {
                if (tokenPolicyDescription.PolicyEvaluation.PolicyResults?.Any(i => string.Equals(i.Code, "TokenNotInRedeemableState", StringComparison.OrdinalIgnoreCase)) ?? false)
                {
                    tokenValidationResult.TokenStatus = CSVTokenStatus.TokenAlreadyRedeemed;
                }
                else if (tokenPolicyDescription.PolicyEvaluation.PolicyResults?.Any(i => string.Equals(i.Code, "TokenExpired", StringComparison.OrdinalIgnoreCase)) ?? false)
                {
                    tokenValidationResult.TokenStatus = CSVTokenStatus.TokenExpired;
                }
            }
            else if (tokenPolicyDescription?.PolicyEvaluation?.IsRedeemable ?? false)
            {
                tokenValidationResult.TokenStatus = CSVTokenStatus.NonCSVToken;
            }

            return tokenValidationResult;
        }

        public static async Task<CSVTokenRedemptionResult> RedeemCSVToken(PXServiceSettings pxSettings, string puid, string tokenValue, string market, string language, string clientIP, EventTraceActivity traceActivityId)
        {
            bool isRedeemSuccessful = false;
            CSVTokenValidationResult tokenValidationResult = await ValidateCSVToken(pxSettings, puid, tokenValue, market, language, clientIP, traceActivityId);
            if ((tokenValidationResult?.TokenStatus ?? CSVTokenStatus.Unknown) == CSVTokenStatus.ValidCSVToken)
            {
                Order orderResult = null;
                try
                {
                    // Redeem CSV token
                    orderResult = await pxSettings.PurchaseServiceAccessor.RedeemCSVToken(puid, tokenValue, market, language, traceActivityId);
                }
                catch (ServiceErrorResponseException ex)
                {
                    SllWebLogger.TracePXServiceException($"Exception Calling purchase service: " + ex.ToString(), traceActivityId);

                    if (string.Equals(ex.Error?.ErrorCode, HttpStatusCode.BadRequest.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        tokenValidationResult.TokenStatus = CSVTokenStatus.Unknown;
                    }
                    else 
                    {
                        throw;
                    }
                }

                isRedeemSuccessful = string.Equals(orderResult?.OrderState, "purchased", StringComparison.OrdinalIgnoreCase);
            }

            return new CSVTokenRedemptionResult { IsSuccess = isRedeemSuccessful, TokenValidationResult = tokenValidationResult };
        }
    }
}