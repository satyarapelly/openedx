// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public class CTPCommerceDataAccessor : ICTPCommereceDataAccessor
    {
        public GetSubscriptionsResponse GetSubscriptions(GetSubscriptionsRequest request, EventTraceActivity traceActivityId)
        {
            var getSubscriptionsResponse = new GetSubscriptionsResponse()
            {
                SubscriptionInfoList = new List<SubscriptionsInfo>
                    {
                        new SubscriptionsInfo()
                        {
                            SubscriptionId = "VFYAAAAAAAAAAAEA",
                            PaymentInstrumentId = "VFYAAAAAAAABAACA",
                            NextBillDate = new DateTime(2020, 10, 28),
                            ProductGuid = new Guid("55414de8-168e-4cbf-a56a-9445131bd21f"),
                            NextCycle = 1,
                            SubscriptionCycleStartDate = new DateTime(2020, 9, 28),
                            SubscriptionDescription = "Windows Azure MSDN Visual Studio Professional",
                            SubscriptionStatusInfo = new SubscriptionStatusInfo()
                            {
                                SubscriptionStatus = "Enabled"
                            }
                        }
                    },
                TotalCount = 1
            };

            return getSubscriptionsResponse;
        }
    }
}
