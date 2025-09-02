// <copyright file="BdkId.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using System;
    using Microsoft.Commerce.Payments.Common.Tracing;

    /// <summary>
    /// borrowed from private\SCS\dev\platform\SpkUtility\src\BdkId.cs
    /// Quite often in all the externally exposed BDK API the opaque billing IDs  
    ///  are passed around in base64(URL) encoded values. The raw values  
    ///  consist of 96-bit quantity (= 12 bytes) which is made of:  
    ///   - Top 64 bits are acctid  
    ///   - Next 32 bits are divided as follows  
    ///      Bit 31(top bit)  
    ///      ON => it is an accountId value  
    ///      OFF =>  a subscription value  
    ///  with Bit 31 = ON (accountIdValue)  
    ///      Here is mapping of bits (31)30,29,28  that determine what type of object it is.  
    ///      (1)00(x) --- means payment instrument.  
    ///          Bits 28 thru 16 are 0. Bits 15 thru bit 0 contain the payment instrument  
    ///      (1)01(x) --- means address id.  
    ///          Bits 28 thru 16 are 0. Bits 15 thru bit 0 contain the address id  
    ///      (1)100 --- BillingPeriod HCTI  
    ///      (1)101 --- means item instance id  
    ///          Bits 27 thru 0 are item reference id  (2^28 possible item ref ids per account)  
    ///      (1)110 --- means billing reference id  
    ///          Bits 27 thru 25 are 0 for future extention  
    ///          Bits 24 thru 0 are billing reference id (2^25 possible billing ref ids per account)  
    ///      (1)111 --- unused  
    /// with Bit 31 = OFF (subscription value)  
    ///      bits 30 thru bits 16 contain the subscription ref and  
    ///      bits 15 thru bit 0 contain the service instance ref  
    /// </summary>
    public class BdkId
    {
        private string encodedBdkId;
        
        public BdkId(long accountId, int paymentInstrumentIndex)
        {
            this.AccountId = accountId;
            this.PaymentInstrumentIndex = paymentInstrumentIndex;

            byte[] rawData = new byte[12];
            int i = 0;

            for (i = 0; i < 8; i++)
            {
                rawData[i] = (byte)(accountId & 0xFF);
                accountId >>= 8;
            }

            rawData[i + 3] = (byte)(0x4 << 5);
            rawData[i + 2] = 0;
            rawData[i + 1] = (byte)((paymentInstrumentIndex >> 8) & 0xFF);
            rawData[i + 0] = (byte)(paymentInstrumentIndex & 0xFF);

            // do base64 encoding, then do URL escape
            this.encodedBdkId = Convert.ToBase64String(rawData, 0, rawData.Length).Replace('/', '-');
        }

        public BdkId(long accountId)
        {
            this.AccountId = accountId;
            this.PaymentInstrumentIndex = 0;

            byte[] rawData = new byte[12];

            for (int i = 0; i < 8; i++)
            {
                rawData[i] = (byte)(accountId & 0xFF);
                accountId >>= 8;
            }

            // do base64 encoding, then do URL escape
            this.encodedBdkId = Convert.ToBase64String(rawData, 0, rawData.Length).Replace('/', '-');
        }

        private BdkId(string encodedString)
        {
            this.encodedBdkId = encodedString;
        }

        public long AccountId { get; private set; }

        public int PaymentInstrumentIndex { get; private set; }

        public static BdkId Parse(string encodedId, EventTraceActivity traceActivityId)
        {
            if (string.IsNullOrWhiteSpace(encodedId))
            {
                string log = "encodedId is missing.";
                throw new InvalidOperationException(log);
            }

            encodedId = encodedId.Trim();

            if (encodedId.Length != 16)
            {
                string log = string.Format("Encoded object id length must be 16: {0}", encodedId);
                throw new InvalidOperationException(log);
            }

            BdkId bdkId = new BdkId(encodedId);

            // undo URL escape  
            encodedId = encodedId.Replace('-', '/');

            // do base64 decoding of the encoded id  
            byte[] decodedValue = Convert.FromBase64String(encodedId); // lgtm[cs/base64-decoding-without-validation] Suppressing Semmle warning

            // extract first 64-bits as the account id  
            bdkId.AccountId = BitConverter.ToInt64(decodedValue, 0);

            // extract next 32-bits as per the encoding scheme  
            uint val = BitConverter.ToUInt32(decodedValue, 8);

            // bits 31,30,29 is 100 for bdk type PI and the PI is Bits 28 thru 16 are 0. Bits 15 thru bit 0 contain the payment instrument 
            if (((val >> 29) & 0x7) == 4)
            {
                bdkId.PaymentInstrumentIndex = (short)(val & 0xFFFF);
            }
            else
            {
                bdkId.PaymentInstrumentIndex = -1;
            }

            return bdkId;
        }

        public static bool TryParse(string encodedId, EventTraceActivity traceActivityId, out BdkId bdkId)
        {
            try
            {
                bdkId = BdkId.Parse(encodedId, traceActivityId);
                return true;
            }
            catch
            {
                bdkId = null;
                return false;
            }
        }

        public override string ToString()
        {
            return this.encodedBdkId;
        }
    }
}
