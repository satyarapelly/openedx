// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.StoredValue
{
    /// <summary>
    /// Constants defines the constant values for csv
    /// </summary>
    public class Constants
    {
        public const string ProviderName = "StoredValue";
        public const string ApiVersion = "2014-05-30";
        public const string StoredValuePrefix = "CSV-";
        public const string CreateAccountUrl = @"{0}/{1}/accounts/create";
        public const string AccountInfoUrl = @"{0}/{1}/accounts/{2}";
        public const string BalanceInfoUrlExtended = @"{0}/{1}/stored-value-balance?svAccountId={2}";
        public const string ChargeUrl = @"{0}/{1}/accounts/{2}/transactions/debit";
        public const string RefundUrl = @"{0}/{1}/accounts/{2}/transactions/refund";
        public const string AuthorizeUrl = @"{0}/{1}/accounts/{2}/transactions/authorize";
        public const string SettleUrl = @"{0}/{1}/accounts/{2}/transactions/settle";
        public const string AuthReverseUrl = @"{0}/{1}/accounts/{2}/transactions/authorize/reverse";
        public const string GetChargeablePaymentInstrumentsUrl = @"{0}/{1}/accounts/{2}/chargeable-payment-instruments";
        public const string TestStoredValueHeader = "testcsv";
        public const string GetStoredValueChargeableInstrumentDetailsApiName = "GetStoredValueChargeableInstrumentDetails";
        public const string GetStoredValueBalanceApiName = "GetStoredValueBalance";
        public const string StoredValueContentKey = "StoredValueContentKey";

        public static class CallbackAck
        {
            public const string Content = "[OK]";
            public const string CallbackNotificationParseFailed = "CallbackNotification parsing failed";
            public const string MediaType = "text/plain";
        }

        public static class TransactionServiceStore
        {
            public const string Azure = "Azure";
            public const string OMS = "OMS";
        }
    }
}
