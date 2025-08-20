// <copyright file="IPIMSAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model;

    public interface IPIMSAccessor
    {
        Task<object> AcquireLUKs(string accountId, string piid, ulong deviceId, object requestData, EventTraceActivity traceActivityId);

        Task<object> ConfirmLUKs(string accountId, string piid, ulong deviceId, EventTraceActivity traceActivityId);

        Task<object> GetCardProfile(string accountId, string piid, ulong deviceId, EventTraceActivity traceActivityId);

        Task<PaymentInstrument> GetPaymentInstrument(string accountId, string piid, EventTraceActivity traceActivityId, string partner = null, string country = null, string language = null, List<string> exposedFlightFeatures = null, PaymentExperienceSetting setting = null);
        
        Task<PaymentInstrument> GetExtendedPaymentInstrument(string piid, EventTraceActivity traceActivityId, string partner = null, string country = null, List<string> exposedFlightFeatures = null);

        Task<PimsSessionDetailsResource> GetSessionDetails(string accountId, string sessionQueryUrl, EventTraceActivity traceActivityId);

        Task<List<PaymentMethod>> GetPaymentMethods(string country, string family, string type, string language, EventTraceActivity traceActivityId, IList<KeyValuePair<string, string>> additionalHeaders = null, string partner = null, List<string> exposedFlightFeatures = null, string operation = null, PaymentExperienceSetting setting = null);

        Task<object> GetSeCardPersos(string accountId, string piid, ulong deviceId, EventTraceActivity traceActivityId);

        Task<PaymentInstrument[]> ListPaymentInstrument(string accountId, ulong deviceId, string[] status, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null, string partner = null, string country = null, string language = null, List<string> exposedFlightFeatures = null, string operation = null, PaymentExperienceSetting setting = null);

        Task<PaymentInstrument[]> ListUserAndTenantPaymentInstrument(ulong deviceId, string[] status, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null, string partner = null, string country = null, string language = null, List<string> exposedFlightFeatures = null, string operation = null, PaymentExperienceSetting setting = null);

        Task<PaymentInstrument> PostPaymentInstrument(string accountId, object postPiData, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null, IList<KeyValuePair<string, string>> additionalHeaders = null, string partner = null, List<string> exposedFlightFeatures = null);
        
        Task<PaymentInstrument> PostPaymentInstrument(object postPiData, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null, IList<KeyValuePair<string, string>> additionalHeaders = null, string partner = null, List<string> exposedFlightFeatures = null);

        Task RemovePaymentInstrument(string accountId, string piid, object removeReason, EventTraceActivity traceActivityId);

        Task<object> ReplenishTransactionCredentials(string accountId, string piid, ulong deviceId, object requestData, EventTraceActivity traceActivityId);

        Task<PaymentInstrument> ResumePendingOperation(string accountId, string piid, object pendingOpRequestData, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null);

        Task<PaymentInstrument> ValidatePicv(string accountId, string piid, object requestData, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null);

        Task<PaymentInstrument> UpdatePaymentInstrument(string accountId, string piid, object updatePiData, EventTraceActivity traceActivityId, string partner = null, List<string> exposedFlightFeatures = null, IEnumerable<KeyValuePair<string, string>> queryParams = null);

        Task<PaymentInstrument> UpdatePaymentInstrument(string piid, object updatePiData, EventTraceActivity traceActivityId, string partner = null, List<string> exposedFlightFeatures = null, IEnumerable<KeyValuePair<string, string>> queryParams = null);

        Task<object> ValidateCvv(string accountId, string piid, object requestData, EventTraceActivity traceActivityId);

        Task LinkSession(string accountId, string piid, LinkSession payload, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null);

        Task<ValidatePaymentInstrumentResponse> ValidatePaymentInstrument(string accountId, string piid, ValidatePaymentInstrument payload, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null);

        Task<List<PaymentMethod>> GetThirdPartyPaymentMethods(string provider, string sellerCountry, string buyerCountry, EventTraceActivity traceActivityId, string partner = null, string language = null, IList<KeyValuePair<string, string>> additionalHeaders = null, List<string> exposedFlightFeatures = null);

        Task<List<SearchTransactionAccountinfoByPI>> SearchByAccountNumber(object piid, EventTraceActivity traceActivityId);
    }
}