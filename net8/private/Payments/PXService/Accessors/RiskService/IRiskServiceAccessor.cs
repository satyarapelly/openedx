// <copyright file="IRiskServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.RiskService.V7
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PimsModel.V4;

    public interface IRiskServiceAccessor
    {
        Task<IList<PaymentInstrument>> FilterPaymentInstruments(string puid, string client, string orderId, string sessionId, IList<PaymentInstrument> paymentInstruments, List<PaymentInstrument> disabledPaymentInstruments, EventTraceActivity traceActivityId);

        Task<IList<PaymentMethod>> FilterPaymentMethods(string puid, string client, string orderId, string sessionId, IList<PaymentMethod> paymentMethods, EventTraceActivity traceActivityId);

        Task<IList<PaymentMethod>> FilterBasedOnRiskEvaluation(string client, string puid, string tid, string oid, IList<PaymentMethod> paymentMethods, string ipAddress, string locale, string deviceType, EventTraceActivity traceActivityId);

        Task<IList<PaymentMethod>> FilterBasedOnRiskEvaluation(string client, string puid, string tid, string oid, string idNameSpace, string commerceRootId, string orgId, IList<PaymentMethod> paymentMethods, string ipAddress, string locale, string deviceType, EventTraceActivity traceActivityId);
    }
}