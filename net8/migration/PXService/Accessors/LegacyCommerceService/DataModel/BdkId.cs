// <copyright file="Bdkld.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Text;

    /// <summary>
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
    public class BdkId : IComparable
    {
        private long accountId = 0;             //// 64 bits
        private short payInstId = 0;            //// for account details
        private short addressId = 0;            //// for account details
        private short subRefId = 0;             //// for subscription info
        private short serviceInstanceId = 0;    //// for subscription info
        private int itemRefId = 0;              //// for subscription info (2^28 possible item ref ids)
        private int billingRefId = 0;
        private int billingPeriod = 0;
        private BdkIdType idType = BdkIdType.AccountId;
        private int globalPaymentMethodId;      ////global Payment method Id

        /// <summary>
        /// Initializes a new instance of the BdkId class.
        /// </summary>
        public BdkId()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BdkId class.
        /// </summary>
        /// <param name="encodedId">Encoded BdkId string.</param>
        public BdkId(string encodedId)
        {
            DecodeIdFromString(encodedId, this);
        }

        /// <summary>
        /// Initializes a new instance of the BdkId class.
        /// </summary>
        /// <param name="ref1">the ref1</param>
        /// <param name="ref2">the ref2</param>
        /// <param name="idRef">id ref type</param>
        public BdkId(long ref1, int ref2, BdkIdType idRef)
        {
            this.accountId = ref1;
            this.idType = idRef;
            switch (idRef)
            {
                case BdkId.BdkIdType.AccountId:
                    if (ref2 != 0)
                    {
                        throw new ArgumentException("ref2 should not be specified for accountID type");
                    }

                    break;
                case BdkId.BdkIdType.AddressId:
                    this.addressId = (short)ref2;
                    break;
                case BdkId.BdkIdType.PaymentId:
                    this.payInstId = (short)ref2;
                    break;
                case BdkId.BdkIdType.ServiceInstanceId:
                    this.serviceInstanceId = (short)ref2;
                    break;
                case BdkId.BdkIdType.SubscriptionId:
                    this.subRefId = (short)ref2;
                    break;
                case BdkId.BdkIdType.Period:
                    this.billingPeriod = ref2;
                    break;
                case BdkId.BdkIdType.BillingReferenceId:
                    this.billingRefId = ref2;
                    break;
                default:
                    throw new ArgumentException("BdkIdType is invalid");
            }
        }

        /// <summary>
        /// Bdk Id types.
        /// </summary>
        public enum BdkIdType
        {
            /// <summary>
            /// Invalid Id.
            /// </summary>
            InvalidId = 0x1,

            /// <summary>
            /// Account Id. In the case of SubscriptionRef or ServiceInstance
            /// use the corresponding IDs, not this one.
            /// </summary>
            AccountId = 0x2,

            /// <summary>
            /// Payment Id.
            /// </summary>
            PaymentId = 0x4,

            /// <summary>
            /// Address Id.
            /// </summary>
            AddressId = 0x8,

            /// <summary>
            /// Subscription Reference Id
            /// </summary>
            SubscriptionId = 0x10,

            /// <summary>
            /// Service Instance Id
            /// </summary>
            ServiceInstanceId = 0x20,

            /// <summary>
            /// Period for HCTI
            /// </summary>
            Period = 0x40,

            /// <summary>
            /// Item Instance Id
            /// </summary>
            ItemInstanceId = 0x80,

            /// <summary>
            /// Billing Reference Id
            /// </summary>
            BillingReferenceId = 0x100,

            /// <summary>
            /// Global Payment Method
            /// </summary>
            GlobalPaymentMethodId = 0x200,
        }

        /// <summary>
        /// Gets or sets AccoundId.
        /// </summary>
        public long AccountId
        {
            get { return this.accountId; }
            set { this.accountId = value; }
        }

        /// <summary>
        /// Gets or sets PayInstId.
        /// </summary>
        public short PayInstId
        {
            get { return this.payInstId; }
            set { this.payInstId = value; }
        }

        /// <summary>
        /// Gets or sets AddressId.
        /// </summary>
        public short AddressId
        {
            get { return this.addressId; }
            set { this.addressId = value; }
        }

        /// <summary>
        /// Gets or sets SubRefId.
        /// </summary>
        public short SubRefId
        {
            get { return this.subRefId; }
            set { this.subRefId = value; }
        }

        /// <summary>
        /// Gets or sets ServiceInstanceId.
        /// </summary>
        public short ServiceInstanceId
        {
            get { return this.serviceInstanceId; }
            set { this.serviceInstanceId = value; }
        }

        /// <summary>
        /// Gets or sets ItemRefId
        /// </summary>
        public int ItemRefId
        {
            get { return this.itemRefId; }
            set { this.itemRefId = value; }
        }

        /// <summary>
        /// Gets or sets billing reference id
        /// </summary>
        public int BillingReferenceId
        {
            get { return this.billingRefId; }
            set { this.billingRefId = value; }
        }

        /// <summary>
        /// Gets or sets BdkIdType.
        /// </summary>
        public BdkIdType IdType
        {
            get { return this.idType; }
            set { this.idType = value; }
        }

        /// <summary>
        /// Gets or sets Payment Method Id
        /// </summary>
        public int GlobalPaymentMethodId
        {
            get { return this.globalPaymentMethodId; }
            set { this.globalPaymentMethodId = value; }
        }

        /// <summary>
        /// Gets a value indicating whether it is High id
        /// </summary>
        public bool IsHighId
        {
            get { return (this.accountId & 0x4000000000000000L) == 0x4000000000000000L; }
        }

        /// <summary>
        /// Format a given encoded account id string to be the format like:
        /// 12345-12345-12345-12345.
        /// </summary>
        /// <param name="encodedId">encoded id</param>
        /// <returns>bdk id in string</returns>
        public static string FormatAccountId(string encodedId)
        {
            if (encodedId == null || encodedId.Length == 0)
            {
                return string.Empty;
            }

            BdkId bid = new BdkId(encodedId);
            return bid.GetFormattedAccountId();
        }

        /// <summary>
        /// check if it is account id
        /// </summary>
        /// <param name="value">the string</param>
        /// <returns>true if input is account id</returns>
        public static bool IsBdkId(string value)
        {
            try
            {
                BdkId bdkId = new BdkId(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// encode account id
        /// </summary>
        /// <param name="billableAcctID">billable account id</param>
        /// <returns>account id</returns>
        public static string EncodeAccountID(long billableAcctID)
        {
            return new BdkId(billableAcctID, 0, BdkIdType.AccountId).ToEncodedString();
        }

        /// <summary>
        /// decode account id
        /// </summary>
        /// <param name="bdkId">the account id</param>
        /// <returns>accound id</returns>
        public static long DecodeAccountID(string bdkId)
        {
            return new BdkId(bdkId).AccountId;
        }

        /// <summary>
        /// compare two bdk id
        /// </summary>
        /// <param name="a">the left</param>
        /// <param name="b">the right</param>
        /// <returns>true if smaller than</returns>
        public static bool operator <(BdkId a, BdkId b)
        {
            return (a.CompareTo(b) < 0);
        }

        /// <summary>
        /// compare two bdk id
        /// </summary>
        /// <param name="a">the left</param>
        /// <param name="b">the right</param>
        /// <returns>true if bigger than</returns>
        public static bool operator >(BdkId a, BdkId b)
        {
            return (a.CompareTo(b) > 0);
        }

        /// <summary>
        /// Dump BdkId's class members.
        /// </summary>
        /// <returns>BdkId's class members.</returns>
        public override string ToString()
        {
            return "Type=" + this.idType.ToString() + " acct: " + this.accountId
                + " sub: " + this.subRefId + " svcInst: " + this.serviceInstanceId
                + " addr: " + this.addressId + " payInst: " + this.payInstId
                + " itemRefId: " + this.itemRefId + " billingRefId: " + this.billingRefId
                + " globalPaymentMethodId: " + this.globalPaymentMethodId;
        }

        /// <summary>
        /// Get encoded BdkId.
        /// </summary>
        /// <returns>Encoded BdkId string.</returns>
        public string ToEncodedString()
        {
            return ConstructEncodedId(this);
        }

        /// <summary>
        /// Compare against another object of same type, return true if equal, otherwise, return false.
        /// </summary>
        /// <param name="obj">the other object</param>
        /// <returns>true if equals</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return (this.CompareTo((BdkId)obj) == 0);
        }

        /// <summary>
        /// Compute hash code based on the class members.
        /// </summary>
        /// <returns>Hash code of this object.</returns>
        public override int GetHashCode()
        {
            return (this.accountId != 0) ? this.accountId.GetHashCode() :
            (this.payInstId.GetHashCode() ^ this.addressId.GetHashCode() ^
            this.subRefId.GetHashCode() ^ this.serviceInstanceId.GetHashCode() ^
            this.idType.GetHashCode() ^ this.billingRefId.GetHashCode() ^
            this.globalPaymentMethodId.GetHashCode());
        }

        /// <summary>
        /// IComparable.CompareTo() function to compare
        /// against another object of same type.
        /// </summary>
        /// <param name="obj">the object</param>
        /// <returns>0 if equals, negative if less than and positive if bigger than</returns>
        public int CompareTo(object obj)
        {
            BdkId bid2 = obj as BdkId;
            if (bid2 != null)
            {
                return this.CompareTo(bid2);
            }
            else
            {
                throw new ArgumentException("Invalid object passed in for comparison");
            }
        }

        /// <summary>
        /// Compare to another BdkId and returns an integer.
        /// </summary>
        /// <param name="bid2">the other bdk id</param>
        /// <returns>0 if equals, negative if less than and positive if bigger than</returns>
        public int CompareTo(BdkId bid2)
        {
            if (bid2 == null)
            {
                return -3;
            }

            if (this.accountId < bid2.accountId)
            {
                return -1;
            }
            else if (this.accountId > bid2.accountId)
            {
                return +1;
            }
            else if (this.idType < bid2.idType)
            {
                return -2;
            }
            else if (this.idType > bid2.idType)
            {
                return +2;
            }
            else
            {
                int diff = 0;
                switch (this.idType)
                {
                    case BdkIdType.AccountId:
                    case BdkIdType.ServiceInstanceId:
                    case BdkIdType.SubscriptionId:
                        diff = (this.subRefId - bid2.subRefId) + (this.serviceInstanceId - bid2.serviceInstanceId);
                        break;

                    case BdkIdType.AddressId:
                        diff = (this.addressId - bid2.addressId);
                        break;

                    case BdkIdType.PaymentId:
                        diff = (this.payInstId - bid2.payInstId);
                        break;

                    case BdkIdType.ItemInstanceId:
                        diff = (this.itemRefId - bid2.itemRefId);
                        break;

                    case BdkIdType.BillingReferenceId:
                        diff = (this.billingRefId - bid2.billingRefId);
                        break;

                    case BdkIdType.GlobalPaymentMethodId:
                        diff = (this.globalPaymentMethodId - bid2.globalPaymentMethodId);
                        break;

                    default:
                        throw new ArgumentException("Invalid ID Type found in the BdkId object");
                }

                return diff;
            }
        }

        /// <summary>
        /// Returns a formatted account id like 12345-12345-12345-12345.
        /// </summary>
        /// <returns>account id in string</returns>
        public string GetFormattedAccountId()
        {
            if (this.idType != BdkIdType.AccountId)
            {
                return null;
            }

            string strTemp = ((ulong)this.accountId).ToString("d20");
            StringBuilder id = new StringBuilder(24); ////actually only 23 characters.
            id.AppendFormat("{0}-{1}-{2}-{3}", strTemp.Substring(0, 5), strTemp.Substring(5, 5), strTemp.Substring(10, 5), strTemp.Substring(15, 5));

            return id.ToString();
        }

        /// <summary>
        /// decode from string
        /// </summary>
        /// <param name="encodedId">encoded bdk id string</param>
        /// <param name="bid">out put</param>
        private static void DecodeIdFromString(string encodedId, BdkId bid)
        {
            if (encodedId == null)
            {
                throw new ArgumentException("Invalid object passed in for decoding");
            }

            encodedId = encodedId.Trim();

            if (encodedId.Length != 16)
            {
                throw new ArgumentException("Encoded object id length must be 16");
            }

            // undo URL escape
            string temp = encodedId;
            encodedId = temp.Replace('-', '/');

            // do base64 decoding of the encoded id
            byte[] decodedValue = Convert.FromBase64String(encodedId);

            // extract first 64-bits as the account id
            // TODO: Clean up this conversion process and make it Endian safe!
            bid.accountId = 0;
            for (int i = 7; i >= 0; i--)
            {
                bid.accountId = (bid.accountId << 8) + decodedValue[i];
            }

            // extract next 32-bits as per the encoding scheme
            uint val = 0;
            for (int i = 3; i >= 0; i--)
            {
                val = (val << 8) + decodedValue[i + 8];
            }

            // now do bit-manipulation checks to assign exact values.
            switch ((val >> 29) & 0x7)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    // high order bit is NOT SET =>
                    // the value is just a plain Account ID with sub details
                    bid.subRefId = (short)((val >> 16) & 0x7FFF);
                    bid.serviceInstanceId = (short)(val & 0xFFFF);
                    if (bid.subRefId == 0)
                    {
                        if (bid.serviceInstanceId == 0)
                        {
                            bid.idType = BdkIdType.AccountId;
                        }
                        else
                        {
                            throw new ArgumentException("Invalid ObjectId. Sub ref ID is 0 but Service Instance ID is not");
                        }
                    }
                    else if (bid.serviceInstanceId == 0)
                    {
                        bid.idType = BdkIdType.SubscriptionId;
                    }
                    else
                    {
                        bid.idType = BdkIdType.ServiceInstanceId;
                    }

                    break;

                case 4:
                    // the value is just plain account id with payInst Id
                    bid.payInstId = (short)(val & 0xFFFF);
                    bid.idType = BdkIdType.PaymentId;
                    break;

                case 5:
                    // value = accountId + address Id
                    bid.addressId = (short)(val & 0xFFFF);
                    bid.idType = BdkIdType.AddressId;
                    break;

                case 6: // looks like an item instnace id
                    //// make sure that the bit is 28 is 1
                    //// 0 is Period
                    if (1 == ((val >> 28) & 0x1))
                    {
                        bid.itemRefId = (int)(val & 0x0FFFFFFF); // get 28 bits
                        bid.idType = BdkIdType.ItemInstanceId;
                    }
                    else
                    {
                        bid.idType = BdkIdType.Period;
                    }

                    break;

                case 7:
                    if (0 == ((val >> 28) & 0x1))
                    {
                        // Bits 27 thru 25 are 0 for future extention
                        // Bits 24 thru 0 are billing reference id (2^25 possible billing ref ids per account)
                        bid.billingRefId = (int)(val & 0x1FFFFFF);
                        bid.idType = BdkIdType.BillingReferenceId;
                    }
                    else if (1 == ((val >> 28) & 0x1))
                    {
                        //// 1111 == 0xF, globalPaymentMethodId, only the low 16 bit are used for this value
                        bid.globalPaymentMethodId = (int)(val & 0xFFFF);
                        bid.idType = BdkIdType.GlobalPaymentMethodId;
                    }
                    else
                    {
                        bid.idType = BdkIdType.InvalidId;
                        throw new ArgumentException("Invalid value supplied for encoded Billing object ID: " + encodedId);
                    }

                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// construct bdk id string
        /// </summary>
        /// <param name="bid">the bdk id</param>
        /// <returns>string of bdkid</returns>
        private static string ConstructEncodedId(BdkId bid)
        {
            string encodedId;

            // convert values into a byte array
            byte[] rawData = new byte[12];
            ulong acctId = (ulong)bid.accountId;
            int i = 0;
            for (i = 0; i < 8; i++)
            {
                rawData[i] = (byte)(acctId & 0xFF);
                acctId >>= 8;
            }

            // TODO: Handle endian-ness later on
            switch (bid.idType)
            {
                case BdkIdType.AccountId:
                case BdkIdType.ServiceInstanceId:
                case BdkIdType.SubscriptionId:

                    if (bid.idType == BdkIdType.AccountId)
                    {
                        bid.serviceInstanceId = 0;
                        bid.subRefId = 0;
                    }
                    else if (bid.idType == BdkIdType.SubscriptionId)
                    {
                        bid.serviceInstanceId = 0;
                    }

                    rawData[i + 3] = (byte)((bid.subRefId >> 8) & 0xFF);
                    rawData[i + 2] = (byte)(bid.subRefId & 0xFF);
                    rawData[i + 1] = (byte)((bid.serviceInstanceId >> 8) & 0xFF);
                    rawData[i + 0] = (byte)(bid.serviceInstanceId & 0xFF);
                    break;

                case BdkIdType.PaymentId:
                    // indicator values bit 31-29 has 100
                    rawData[i + 3] = (byte)(0x4 << 5);
                    rawData[i + 2] = 0;
                    rawData[i + 1] = (byte)((bid.payInstId >> 8) & 0xFF);
                    rawData[i + 0] = (byte)(bid.payInstId & 0xFF);
                    break;

                case BdkIdType.AddressId:
                    // indicator values bit 31-29 has 101
                    rawData[i + 3] = (byte)(0x5 << 5);
                    rawData[i + 2] = 0;
                    rawData[i + 1] = (byte)((bid.addressId >> 8) & 0xFF);
                    rawData[i + 0] = (byte)(bid.addressId & 0xFF);
                    break;

                case BdkIdType.ItemInstanceId:
                    if (0 != (bid.itemRefId & 0xF0000000))
                    {
                        throw new ArgumentException("Item instance too big.");
                    }

                    // indicator values bit 31-28 has 1101
                    rawData[i + 3] = (byte)((0xD << 4) | ((bid.itemRefId >> 24) & 0x0F));
                    rawData[i + 2] = (byte)((bid.itemRefId >> 16) & 0xFF);
                    rawData[i + 1] = (byte)((bid.itemRefId >> 8) & 0xFF);
                    rawData[i + 0] = (byte)(bid.itemRefId & 0xFF);
                    break;

                case BdkIdType.Period:
                    // billing period have only 6 digits
                    if (bid.billingPeriod.ToString().Length > 6)
                    {
                        throw new ArgumentException("Billing Period cannot have more than 6 digits.");
                    }

                    rawData[i + 3] = (byte)(0xC << 4);      // billing period is less than 2^24 (=16777216) so the highest byte doesn't contain it
                    rawData[i + 2] = (byte)((bid.billingPeriod >> 16) & 0xFF);
                    rawData[i + 1] = (byte)((bid.billingPeriod >> 8) & 0xFF);
                    rawData[i + 0] = (byte)(bid.billingPeriod & 0xFF);
                    break;

                case BdkIdType.BillingReferenceId:
                    if (0 != (bid.billingRefId & 0xFE000000))
                    {
                        throw new ArgumentException("Billing reference too big.");
                    }

                    rawData[i + 3] = (byte)(0xE0 | ((bid.billingRefId >> 24) & 0x1));
                    rawData[i + 2] = (byte)((bid.billingRefId >> 16) & 0xFF);
                    rawData[i + 1] = (byte)((bid.billingRefId >> 8) & 0xFF);
                    rawData[i + 0] = (byte)(bid.billingRefId & 0xFF);
                    break;

                case BdkIdType.GlobalPaymentMethodId:
                    // indicator values bit 31-28 has 1111
                    rawData[i + 3] = (byte)(0xF << 4);
                    rawData[i + 2] = 0;          ////16bit is big enough for Global payment method id
                    rawData[i + 1] = (byte)((bid.globalPaymentMethodId >> 8) & 0xFF);
                    rawData[i + 0] = (byte)(bid.globalPaymentMethodId & 0xFF);
                    break;

                default:
                    throw new ArgumentException("Invalid BdkId object supplied for encoding.");
            } // switch

            // do base64 encoding
            encodedId = Convert.ToBase64String(rawData, 0, rawData.Length);

            // do URL escape
            string temp = encodedId;
            encodedId = temp.Replace('/', '-');

            return encodedId;
        }
    }
}
