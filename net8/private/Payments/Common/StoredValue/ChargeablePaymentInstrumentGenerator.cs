// <copyright file="chargeablePaymentInstrumentGenerator.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.StoredValue
{
    using System;
    using System.Linq;
    using Instruments;

    /// <summary>
    /// ChargeablePaymentInstrumentGenerator is the class for parsing or generating the csv chargeable pi id.
    /// It's been used in two places: GetChargeableDetail API and InstrumentAccessor, the format is CSV:storedValueAccountId-lotId
    /// </summary>
    public class ChargeablePaymentInstrumentGenerator
    {
        /// <summary>
        /// GenerateChargeablePaymentId is going to generate the csv chargeable pi id.
        /// the format is CSV-storedValueAccountId-lotId
        /// </summary>
        /// <param name="storedValueAccountId">The stored value account id</param>
        /// <param name="lotId">The stored value lot id</param>
        /// <returns>The csv chargeable pi id, a sample csv chargeable pi id would be like "CSV-3150-1"</returns>
        public static string GenerateChargeablePaymentId(long storedValueAccountId, int lotId)
        {
            return string.Format("{0}{1}-{2}", Constants.StoredValuePrefix, storedValueAccountId, lotId);
        }

        /// <summary>
        /// TryParseChargeablePaymentId is used for parsing the csv chargeable pi id.
        /// Return True means the input chargeable pi id is a csv chargeable pi id and at the same time, output object will contains the StoredValuePI.
        /// Return False means the input chargeable pi id is not a csv chargeable pi id, outputobject will just be null.
        /// </summary>
        /// <param name="chargeablePaymentInstrumentName">The csv chargeable pi id</param>
        /// <param name="storedValuePi">The output for the stored value pi, if it's not a stored value chargeable pi id, return null</param>
        /// <returns>Bool value to indicate whether the input chargeable pi id is a csv chargeable pi id.</returns>
        public static bool TryParseChargeablePaymentId(string chargeablePaymentInstrumentName, out StoredValuePaymentInstrument storedValuePi)
        {
            storedValuePi = null;

            if (string.IsNullOrEmpty(chargeablePaymentInstrumentName)
                || !chargeablePaymentInstrumentName.StartsWith(Constants.StoredValuePrefix))
            {
                return false;
            }
            
            char[] delimiterChars = { '-' };
            string[] chargeablePaymentInstrumentInfo = chargeablePaymentInstrumentName.Replace(Constants.StoredValuePrefix, string.Empty).Split(delimiterChars);
            long storedValueAccountId;
            int lotId;

            // Stored value chargeable pi id must only contains two strings.
            if (chargeablePaymentInstrumentInfo.Count() != 2
                || !long.TryParse(chargeablePaymentInstrumentInfo[0], out storedValueAccountId)
                || !int.TryParse(chargeablePaymentInstrumentInfo[1], out lotId))
            {
                return false;
            }

            storedValuePi = new StoredValuePaymentInstrument
            {
                StoredValueAccountId = storedValueAccountId,
                ChargeablePaymentInstrumentId = lotId,
                Status = PaymentInstrumentStatus.Active,
            };

            return true;
        }
    }
}
