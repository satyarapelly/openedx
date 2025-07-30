// <copyright file="CTPCommerceHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages;
    using Microsoft.Commerce.Payments.PXService.Settings;

    public class CTPCommerceHelper
    {
        public static List<SubscriptionsInfo> GetSubscriptions(PXServiceSettings pxServiceSettings, List<string> billableAccountIds, string puid, EventTraceActivity traceActivityId, string language)
        {
            string identityValue;
            string identityType;
            CommerceHelper.GetIdentity(puid, null, traceActivityId, out identityValue, out identityType);

            return GetSubscriptions(pxServiceSettings, billableAccountIds, identityType, identityValue, traceActivityId, language);
        }

        private static List<SubscriptionsInfo> GetSubscriptions(PXServiceSettings pxServiceSettings, List<string> billableAccountIds, string identityType, string identityValue, EventTraceActivity traceActivityId, string language)
        {
            List<SubscriptionsInfo> subscriptions = new List<SubscriptionsInfo>();

            foreach (var billableAccountId in billableAccountIds)
            {
                // Construct an object of GetSubscriptionsRequest
                GetSubscriptionsRequest getSubscriptionsRequest = new GetSubscriptionsRequest()
                {
                    APIContext = new APIContext()
                    {
                        TrackingGuid = Guid.NewGuid(),
                    },
                    CallerInfo = new CallerInfo()
                    {
                        Requester = new Identity()
                        {
                            IdentityType = identityType,
                            IdentityValue = identityValue
                        },
                        AccountId = billableAccountId
                    },
                    Requester = new Identity()
                    {
                        IdentityType = identityType,
                        IdentityValue = identityValue
                    },
                    GetSubscriptionsOfAllPartners = true
                };

                // Make a Get Subscriptions call to CTP Commerce service
                GetSubscriptionsResponse getSubscriptionsResponse = null;
                try
                {
                    getSubscriptionsResponse = pxServiceSettings.CtpCommerceDataServiceAccessor.GetSubscriptions(getSubscriptionsRequest, traceActivityId);
                }
                catch (DataAccessException exception)
                {
                    string exceptionMessage = string.Format("Failed to retrieve subscriptions for billableAccountId: {0} Exception: {1}.", billableAccountId, exception.ToString());
                    throw ConvertDataAccessException(exception, exceptionMessage, traceActivityId, language);
                }

                if (getSubscriptionsResponse != null && getSubscriptionsResponse.SubscriptionInfoList != null)
                {
                    subscriptions.AddRange(getSubscriptionsResponse.SubscriptionInfoList);
                }
            }

            return subscriptions;
        }

        private static Exception ConvertDataAccessException(DataAccessException exception, string exceptionMessage, EventTraceActivity traceActivityId, string language)
        {
            string commerceServiceResponse = string.Empty;
            if (exception.TracerResult != null && exception.TracerResult.RawApiResponse != null)
            {
                commerceServiceResponse = string.Format("Api Response: {0}", exception.TracerResult.RawApiResponse);
                exceptionMessage += "  " + commerceServiceResponse;
            }

            if (exception.ErrorCode == DataAccessErrors.DATAACCESS_E_EXTERNAL_TIMEOUT_ERROR
                || exception.ErrorCode == DataAccessErrors.DATAACCESS_E_SERVICECALL_ERROR)
            {
                return TraceCore.TraceException(traceActivityId, new PXServiceException(exceptionMessage, GlobalConstants.PXServiceErrorCodes.CTPCommerceServiceFailed, exception));
            }
            else
            {
                return TraceCore.TraceException(traceActivityId, new InvalidOperationException(exceptionMessage, exception));
            }
        }
    }
}