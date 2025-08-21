// <copyright file="JsonDataMasker.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class JsonDataMasker
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502", Justification = "Dictionary is simple enough to understand and maintain.")]
        private static readonly Dictionary<string, Func<JToken, JToken>> ReplacementFuncs = new Dictionary<string, Func<JToken, JToken>>(StringComparer.OrdinalIgnoreCase)
        {
            // Ring1 outgoing token data
            { "data", DelegateMaskKeepLast4Char },

            // Part of Address object
            { "addressline1", DelegateMaskAll },
            { "addressline2", DelegateMaskAll },
            { "addressline3", DelegateMaskAll },
            { "address_line_1", DelegateMaskAll },
            { "address_line_2", DelegateMaskAll },
            { "address_line_3", DelegateMaskAll },
            { "address_line1", DelegateMaskAll },
            { "address_line2", DelegateMaskAll },
            { "address_line3", DelegateMaskAll },
            { "line1", DelegateMaskAll },
            { "line2", DelegateMaskAll },
            { "line3", DelegateMaskAll },
            { "street1", DelegateMaskAll },
            { "street2", DelegateMaskAll },
            { "street3", DelegateMaskAll },
            { "unitnumber", DelegateMaskAll },
            { "unit_number", DelegateMaskAll },

            // general
            { "email", DelegateMaskEmail },
            { "email_address", DelegateMaskEmail },
            { "encryptedPassword", DelegateMaskAll },
            { "clientWalletAccountEmailAddress", DelegateMaskEmail },
            { "defaultDisplayName", DelegateMaskKeepFirst1Char },
            
            // Part of Phone object
            { "localnumber", DelegateMaskAll },
            { "extension", DelegateMaskAll },

            // credit card
            { "accountToken", DelegateMaskKeepLast4Char },
            { "cvvToken", DelegateMaskKeepLast4Char },
            { "friendlyname", DelegateMaskAll },
            { "paymentinstrumentholdername", DelegateMaskKeepFirst1Char },
            { "accountholdername", DelegateMaskKeepFirst1Char },
            { "account_holder_name", DelegateMaskKeepFirst1Char },
            { "bankidentificationnumber", DelegateMaskKeepFirstAndLastChar },
            { "cardholdername", DelegateMaskKeepFirst1Char },
            { "expiryyear", DelegateMaskAll },
            { "expirymonth", DelegateMaskAll },
            { "expiry_year", DelegateMaskAll },
            { "expiry_month", DelegateMaskAll },
            { "expirationdate", DelegateMaskAll },
            { "name", DelegateMaskKeepFirst1Char },
            { "idn", DelegateMaskAll },
            { "contactemail", DelegateMaskEmail },
            { "contactnumber", DelegateMaskKeepFirst1Char },
            { "phoneNumber", DelegateMaskKeepFirst1Char },
            { "preferredPhoneNumber", DelegateMaskKeepFirst1Char },
            { "contactname", DelegateMaskKeepFirst1Char },
            { "shortdescription", DelegateMaskAll },
            { "longdescription", DelegateMaskAll },
            { "mediaContents", DelegateMaskAll },
            { "deviceNumber", DelegateMaskAll },
            { "firstname", DelegateMaskKeepFirst1Char },
            { "first_name", DelegateMaskKeepFirst1Char },
            { "middlename", DelegateMaskKeepFirst1Char },
            { "lastname", DelegateMaskKeepFirst1Char },
            { "last_name", DelegateMaskKeepFirst1Char },
            { "payerid", DelegateMaskAll },

            // iDEAL BA
            { "bankAccountToken", DelegateMaskKeepFirst2Char },
            { "bankIdentificationCode", DelegateMaskKeepFirst2Char },

            // Klarna
            { "nationalIdentificationNumber", DelegateMaskAll },
            { "phone", DelegateMaskKeepFirst5Char },
            { "gender", DelegateMaskAllNoLength },
            { "lastFiveCertNo", DelegateMaskAll },

            // Stripe
            { "stripePlatformKey", DelegateMaskAll },
            { "stripeUserId", DelegateMaskAll },

            // card profile data that is too long
            { "cardprofile", DelegateMaskAll },
            { "encrypteddata", DelegateMaskAll },
            { "publickey", DelegateMaskAll },
            
            // Device public Keys and blobs are too long
            { "devicePublicKeys", DelegateMaskAll },
            { "attestationKeyChain", DelegateMaskAll },
            { "attestationBlob", DelegateMaskAll },
            { "certValue", DelegateMaskAll },
            { "encodedData", DelegateMaskAll },
            { "encKeyInfo", DelegateMaskAll },
            { "apiKey", DelegateMaskKeepFirst1Char },
            { "sharedSecret", DelegateMaskKeepFirst1Char },
            { "authorizationToken", DelegateMaskKeepFirst1Char },
            { "x-pay-token", DelegateMaskKeepFirst1Char },

            // Special delegate for encRiskDataInfo object
            { "encRiskDataInfo", DelegateMaskArrayOfNameValueObjects },

            // Values sent to PX as part of dictionaries within headers.
            { "emailAddress", DelegateMaskEmail },
            { "deviceId", DelegateMaskKeepFirst5Char },
            { "xboxLiveDeviceId", DelegateMaskAll },
            { "puid", DelegateMaskAll },

            // Export pan scenario
            { "userCredential", DelegateMaskAll },
            { "keyToken", DelegateMaskAll },
            { "user_Credential", DelegateMaskAll },
            { "key_Token", DelegateMaskAll },
 
            // An Alipay account could either be an email address or a phone number
            { "alipayAccount", DelegateMaskEmailOrPhone },

            // Non-Sim Mobi
            { "msisdn", DelegateMaskKeepFirst5Char },
            { "value", DelegateMaskKeepFirst1Char },
            { "iccId", DelegateMaskKeepFirst5Char },
            { "phoneSerialNumber", DelegateMaskKeepFirst5Char },

            // Pidl displayContent can have phone numbers
            // It can also contain email addresses (e.g. Credit card add PI summary page)
            { "displayContent", DelegateMaskKeepFirst5Char },

            // Pidl displayText can contain emails and names.
            { "displayText", DelegateMaskAll },
            
            // Pidl possibleValues & possible_values can contain emails.
            { "possibleValues", DelegateMaskAll },
            { "possible_values", DelegateMaskAll },

            // Profile
            { "birth_date", DelegateMaskAll },
            { "legacy_first_name", DelegateMaskKeepFirst1Char },
            { "legacy_last_name", DelegateMaskKeepFirst1Char },

            // Wallet push notification
            { "target_device_id", DelegateMaskKeepFirst5Char },

            // Responses from LiftForward - Check and update other fields
            { "approval_token", DelegateMaskAll },
            { "Api-Key", DelegateMaskAll },
            { "pan", DelegateMaskAll },
            { "exp", DelegateMaskAll },
            { "merchant_customer_id", DelegateMaskAll },
            { "payment_method_id", DelegateMaskAll },
            { "application_id", DelegateMaskAll },
            { "tax_id_encrypted", DelegateMaskAll },
            { "tax_id_last_4_encrypted", DelegateMaskAll },
            { "date_of_birth", DelegateMaskAll },
            { "annual_income", DelegateMaskAll },
            { "monthly_housing_expense", DelegateMaskAll },
            { "mobile_phone_number", DelegateMaskAll },
            { "ip_address", DelegateMaskAll },
            { "serialNumber", DelegateMaskAll },
            { "serial_number", DelegateMaskAll },
            { "merchant_sku", DelegateMaskAll },
            { "reference_id", DelegateMaskAll },

            // Response from D365Service - Shipping Address
            { "fullAddress", DelegateMaskAll },
            { "street", DelegateMaskAll },
            { "streetNumber", DelegateMaskAll },

            // Response from D365Service - Cart
            { "isRequiredAmountPaid", DelegateMaskAll },
            { "isDiscountFullyCalculated", DelegateMaskAll },
            { "ignoreDiscountCalculation", DelegateMaskAll },
            { "amountDue", DelegateMaskAll },
            { "amountPaid", DelegateMaskAll },
            { "beginDateTime", DelegateMaskAll },
            { "businessDate", DelegateMaskAll },
            { "cancellationChargeAmount", DelegateMaskAll },
            { "estimatedShippingAmount", DelegateMaskAll },
            { "cartTypeValue", DelegateMaskAll },
            { "chargeAmount", DelegateMaskAll },
            { "customerOrderRemainingBalance", DelegateMaskAll },
            { "comment", DelegateMaskAll },
            { "invoiceComment", DelegateMaskAll },
            { "customerId", DelegateMaskAll },
            { "customerOrderModeValue", DelegateMaskAll },
            { "deliveryMode", DelegateMaskAll },
            { "deliveryModeChargeAmount", DelegateMaskAll },
            { "discountAmount", DelegateMaskAll },
            { "discountAmountWithoutTax", DelegateMaskAll },
            { "suspendedCartId", DelegateMaskAll },
            { "transactionTypeValue", DelegateMaskAll },
            { "incomeExpenseTotalAmount", DelegateMaskAll },
            { "isReturnByReceipt", DelegateMaskAll },
            { "returnTransactionHasLoyaltyPayment", DelegateMaskAll },
            { "isFavorite", DelegateMaskAll },
            { "isRecurring", DelegateMaskAll },
            { "isSuspended", DelegateMaskAll },
            { "loyaltyCardId", DelegateMaskAll },
            { "modifiedDateTime", DelegateMaskAll },
            { "orderNumber", DelegateMaskAll },
            { "availableDepositAmount", DelegateMaskAll },
            { "overriddenDepositAmount", DelegateMaskAll },
            { "overriddenDepositWithoutCarryoutAmount", DelegateMaskAll },
            { "prepaymentAmountPaid", DelegateMaskAll },
            { "prepaymentAppliedOnPickup", DelegateMaskAll },
            { "quotationExpiryDate", DelegateMaskAll },
            { "receiptEmail", DelegateMaskAll },
            { "requestedDeliveryDate", DelegateMaskAll },
            { "requiredDepositAmount", DelegateMaskAll },
            { "requiredDepositWithoutCarryoutAmount", DelegateMaskAll },
            { "salesId", DelegateMaskAll },
            { "staffId", DelegateMaskAll },
            { "subtotalAmount", DelegateMaskAll },
            { "subtotalAmountWithoutTax", DelegateMaskAll },
            { "netPrice", DelegateMaskAll },
            { "subtotalSalesAmount", DelegateMaskAll },
            { "taxAmount", DelegateMaskAll },
            { "taxOnCancellationCharge", DelegateMaskAll },
            { "taxOverrideCode", DelegateMaskAll },
            { "terminalId", DelegateMaskAll },
            { "totalAmount", DelegateMaskAll },
            { "totalSalesAmount", DelegateMaskAll },
            { "totalReturnAmount", DelegateMaskAll },
            { "totalCarryoutSalesAmount", DelegateMaskAll },
            { "totalCustomerOrderSalesAmount", DelegateMaskAll },
            { "totalManualDiscountAmount", DelegateMaskAll },
            { "totalManualDiscountPercentage", DelegateMaskAll },
            { "warehouseId", DelegateMaskAll },
            { "isCreatedOffline", DelegateMaskAll },
            { "cartStatusValue", DelegateMaskAll },
            { "receiptTransactionTypeValue", DelegateMaskAll },
            { "commissionSalesGroup", DelegateMaskAll },
            { "version", DelegateMaskAll },
            { "totalItems", DelegateMaskAll },
            { "hasTaxCalculationTriggered", DelegateMaskAll },
            { "hasChargeCalculationTriggered", DelegateMaskAll },
            { "shippingChargeAmount", DelegateMaskAll },
            { "otherChargeAmount", DelegateMaskAll },

            // AddressEnrichment key mask
            { "Ocp-Apim-Subscription-Key", DelegateMaskAll },

            // India three ds
            { "acs_signed_content", DelegateMaskAll },

            // Checkout 3PP
            { "receiptEmailAddress", DelegateMaskEmail },
            { "sellerName", DelegateMaskKeepFirst1Char },
            { "description", DelegateMaskKeepFirst1Char },

            // Redeem CSV
            { "tokenIdentifierValue", DelegateMaskAll },

            // Payment Orchestrator data
            { "ChallengeValue", DelegateMaskAll },
            { "dataValue", DelegateMaskAll },

            // Juspay vpa formats
            { "vpa", DelegateMaskEmail },
            { "upi_vpa", DelegateMaskEmail },
            { "payer_vpa", DelegateMaskEmail },
            { "merchant_vpa", DelegateMaskEmail }
        };

        private static readonly Dictionary<string, Func<JToken, JToken>> HeaderMaskFunctions = new Dictionary<string, Func<JToken, JToken>>()
        {
            { "x-ms-deviceinfo", DelegateMaskBase64EncodedValuesInDictionary },
            { "x-ms-msaprofile", DelegateMaskBase64EncodedValuesInDictionary },
            { "x-ms-xboxprofile", DelegateMaskBase64EncodedValuesInDictionary },
            { "x-ms-customer", DelegateMaskAll },
            { "x-ms-authinfo", DelegateMaskAll },
            { "Ocp-Apim-Subscription-Key", DelegateMaskAll },
            { "Api-Key", DelegateMaskAll },
            { "X-ARR-ClientCert", DelegateMaskAll }
        };

        private static readonly List<RegexMaskDefinition> RegexReplacers = new List<RegexMaskDefinition>()
        {
            new RegexMaskDefinition()
            {
                Regex = new Regex("(\")([0-9]{13,17})(\"|\\\\)", RegexOptions.Compiled),
                ReplacementFunction = m => m.Groups[1] + string.Empty.PadRight(m.Groups[2].Length, '0') + m.Groups[3]
            },
            new RegexMaskDefinition()
            {
                Regex = new Regex("(\")([A-Z]{2}[0-9]{2}[a-zA-Z0-9]{1,30})(\"|\\\\)", RegexOptions.Compiled),
                ReplacementFunction = m => m.Groups[1] + string.Empty.PadRight(m.Groups[2].Length, '0') + m.Groups[3]
            },
            new RegexMaskDefinition()
            {
                Regex = new Regex("(\")([a-zA-Z0-9]{5}-[a-zA-Z0-9]{5}-[a-zA-Z0-9]{5}-[a-zA-Z0-9]{5}-[a-zA-Z0-9]{5})(\"|\\\\)", RegexOptions.Compiled),
                ReplacementFunction = m => m.Groups[1] + "00000-00000-00000-00000-00000" + m.Groups[3]
            },
        };

        public static JToken DelegateMaskAll(JToken o)
        {
            if (o.Type != JTokenType.String)
            {
                return "MASKED";
            }

            return string.Format("MASKED({0})", o.ToString().Length);
        }

        public static JToken DelegateMaskAllNoLength(JToken o)
        {
            return "MASKED";
        }

        public static JToken DelegateMaskKeepFirst1Char(JToken o)
        {
            return MaskExceptFirstFewChars(o, 1);
        }

        public static JToken DelegateMaskKeepFirst2Char(JToken o)
        {
            return MaskExceptFirstFewChars(o, 2);
        }

        public static JToken DelegateMaskKeepFirst3Char(JToken o)
        {
            return MaskExceptFirstFewChars(o, 3);
        }

        public static JToken DelegateMaskKeepFirst5Char(JToken o)
        {
            return MaskExceptFirstFewChars(o, 5);
        }

        public static JToken DelegateMaskKeepLast4Char(JToken o)
        {
            return MaskExceptLastFewChars(o, 4);
        }

        public static JToken DelegateMaskEmailOrPhone(JToken o)
        {
            if (o.Type != JTokenType.String)
            {
                return "MASKED";
            }

            string s = o.ToString();
            return s.Contains("@") ? DelegateMaskEmail(o) : DelegateMaskKeepFirst5Char(o);
        }

        public static JToken DelegateMaskEmail(JToken o)
        {
            if (o.Type != JTokenType.String)
            {
                return "MASKED";
            }

            string[] strs = o.ToString().Split('@');
            if (strs.Length != 2)
            {
                // an email without an @ is not an email
                return string.Format("MASKED({0})", o.ToString().Length);
            }

            return (strs[0].Length > 3 ? string.Format("{0}...{1}({2})", strs[0].Substring(0, 2), strs[0].Substring(strs[0].Length - 1), strs[0].Length) : "MASKED") + "@" + strs[1];
        }

        public static JToken TruncateAndMaskName(JToken o, int length)
        {
            string strs = o?.ToString();

            if (o?.Type == JTokenType.String && strs.Length > length)
            {
                return string.Format("{0}..", strs.Substring(0, length));
            }

            return strs;
        }

        public static JToken DelegateMaskEmailWithoutLength(JToken o)
        {
            if (o.Type != JTokenType.String)
            {
                return "MASKED";
            }

            string s = o.ToString();
            string[] strs = s?.Split('@');

            if (strs?.Length == 2)
            {
                return (strs[0].Length > 3 ? string.Format("{0}...{1}", strs[0].Substring(0, 2), strs[0].Substring(strs[0].Length - 1)) : "MASKED") + "@" + strs[1];
            }
            else
            {
                // an email without an @ is not an email
                return string.Format("MASKED({0})", o.ToString().Length);
            }
        }

        public object[] Mask(params object[] plainValues)
        {
            if (plainValues == null || plainValues.Length == 0)
            {
                return null;
            }

            object[] maskedValues = new object[plainValues.Length];

            for (int i = 0; i < plainValues.Length; i++)
            {
                string s = plainValues[i] as string;
                maskedValues[i] = s != null ? RunRegexMasking(this.MaskSingle(s)) : plainValues[i];
            }

            return maskedValues;
        }

        // Headers are arrays of Key-Value pairs as shown below
        //  [{
        //    "Key" : "Accept",
        //    "Value" : "application/json"
        //  }, {
        //    "Key" : "Expect",
        //    "Value" : "100-continue"
        //  }, {
        //    "Key" : "Host",
        //    "Value" : "paymentexperience.cp.microsoft.com"
        //  }, {
        //    "Key" : "User-Agent",
        //    "Value" : "PaymentInstrumentFD"
        //  }, {
        //    "Key" : "x-ms-deviceinfo",
        //    "Value" : "ipAddress=MTIuMzQuNTYuNzg=,xboxLiveDeviceId=U29tZTE2RGlnaXROdW1iZQ=="
        //  }, {
        //    "Key" : "x-ms-msaprofile",
        //    "Value" : "PUID=UmVhbGx5Pw==,emailAddress=c3dhcm9zaEBtaWNyb3NvZnQuY29t,firstName=U3dhcm9vcA==,lastName=U2hpdmFrdW1hcg=="
        //  }]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822", Justification = "We do not want this class to be static so that we can inherit from it.")]
        public string MaskHeader(string plainValue)
        {
            plainValue = RunRegexMasking(plainValue);
            JArray jarray;
            if (TryParseJArray(plainValue, out jarray))
            {
                jarray = MaskArray(jarray, MaskKeyValuePairObject);
                return RunRegexMasking(JsonConvert.SerializeObject(jarray));
            }

            // the string does not represent a json array
            return plainValue;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822", Justification = "We do not want this class to be static so that we can inherit from it.")]
        public string MaskSingle(string plainValue)
        {
            if (string.IsNullOrWhiteSpace(plainValue))
            {
                return plainValue;
            }

            JObject jobject;
            if (TryParseJObject(plainValue, out jobject))
            {
                jobject = MaskObject(jobject);
                return RunRegexMasking(JsonConvert.SerializeObject(jobject));
            }

            JArray jarray;
            if (TryParseJArray(plainValue, out jarray))
            {
                jarray = MaskArray(jarray, MaskObject);
                return RunRegexMasking(JsonConvert.SerializeObject(jarray));
            }

            // the string does not represent a json object or a json array
            return RunRegexMasking(plainValue);
        }

        /// <summary>
        /// Between masking and keeping first few characters, masking is more important.  So, if
        /// the length of the original is smaller than charsToKeep, mask the string completely.
        /// </summary>
        /// <param name="original">Input token</param>
        /// <param name="charsToKeep">Number of characters to keep unmasked</param>
        /// <returns>Masked string</returns>
        private static JToken MaskExceptFirstFewChars(JToken original, int charsToKeep)
        {
            if (original.Type != JTokenType.String)
            {
                return "MASKED";
            }

            string s = original.ToString();
            return s.Length > charsToKeep
                ? string.Format("{0}...({1})", s.Substring(0, charsToKeep), s.Length)
                : "MASKED";
        }

        /// <summary>
        /// Between masking and keeping last few characters, masking is more important.  So, if
        /// the length of the original is smaller than charsToKeep, mask the string completely.
        /// </summary>
        /// <param name="original">Input token</param>
        /// <param name="charsToKeep">Number of characters to keep unmasked</param>
        /// <returns>Masked string</returns>
        private static JToken MaskExceptLastFewChars(JToken original, int charsToKeep)
        {
            if (original.Type != JTokenType.String)
            {
                return "MASKED";
            }

            string s = original.ToString();
            return s.Length > charsToKeep
                ? string.Format("...{0}({1})", s.Substring(s.Length - charsToKeep), s.Length)
                : "MASKED";
        }

        private static JToken DelegateMaskKeepFirstAndLastChar(JToken o)
        {
            if (o.Type != JTokenType.String)
            {
                return "MASKED";
            }

            string s = o.ToString();
            return s.Length > 0 ? string.Format("{0}...{1}({2})", s[0], s[s.Length - 1], s.Length) : "x...(0)";
        }

        // this function handles the case of [{"name":"<key>","value":"<piidata>"},{...},...] objects
        private static JToken DelegateMaskArrayOfNameValueObjects(JToken o)
        {
            if (o.Type == JTokenType.String)
            {
                // deserialize
                string s = o.ToObject<string>();

                JArray jarray;
                if (TryParseJArray(s, out jarray))
                {
                    return JsonConvert.SerializeObject(MaskArrayInNameValueForm(jarray));
                }

                JObject jobject;
                if (TryParseJObject(s, out jobject))
                {
                    return JsonConvert.SerializeObject(MaskObjectInNameValueForm(jobject));
                }

                // the string is not a serialized array or object
                return o;
            }

            if (o.Type == JTokenType.Array)
            {
                return MaskArrayInNameValueForm(o.ToObject<JArray>());
            }

            if (o.Type == JTokenType.Object)
            {
                return MaskObjectInNameValueForm(o.ToObject<JObject>());
            }

            return o;
        }

        // Example 1:
        // Original String: "PUID=UmVhbGx5Pw==,emailAddress=c3dhcm9zaEBtaWNyb3NvZnQuY29t,firstName=U3dhcm9vcA==,lastName=U2hpdmFrdW1hcg=="
        // Masked String  : "PUID=MASKED(7),emailAddress=sw...(7)@outlook.com,firstName=S...(7),lastName=S...(10)"
        //
        // Example 2:
        // Original String: "ipAddress=MTIuMzQuNTYuNzg=,xboxLiveDeviceId=U29tZTE2RGlnaXROdW1iZQ=="
        // Masked String  : "ipAddress=MTIuMzQuNTYuNzg=,xboxLiveDeviceId=MASKED(16)"

        // Input string is a comma separated collection of key-value pairs.  Key and values are themselves separated
        // by '=' and the value is Base64 encoded.  This function masks only those values, whose keys are found in the
        // ReplacementFuncs dictionary.  Also, before masking a value, it Base64 decodes it.
        private static JToken DelegateMaskBase64EncodedValuesInDictionary(JToken originalToken)
        {
            if (originalToken == null || originalToken.Type != JTokenType.String)
            {
                return originalToken;
            }

            string originalString = originalToken.ToString();
            if (originalString.IndexOf("=") == -1)
            {
                // The value is not a dictionary.  A dictionary should have atleast one '=' to separate key and value
                // and optionally ',' to separate multiple key-value pairs
                return originalToken;
            }

            try
            {
                StringBuilder newString = new StringBuilder();
                originalString = originalString.Trim('"');
                string[] keyValuePairs = originalString.Split(',');
                foreach (string keyValuePair in keyValuePairs)
                {
                    if (newString.Length > 0)
                    {
                        newString.Append(',');
                    }

                    int indexOfDelim = keyValuePair.IndexOf('=');
                    if (indexOfDelim > -1)
                    {
                        string key = keyValuePair.Substring(0, indexOfDelim).Trim();
                        string value = keyValuePair.Substring(indexOfDelim + 1);
                        if (ReplacementFuncs.ContainsKey(key))
                        {
                            var decodedData = Convert.FromBase64String(value);
                            value = System.Text.Encoding.UTF8.GetString(decodedData);
                            value = ReplacementFuncs[key](JToken.FromObject(value)).ToString();
                        }

                        newString.Append(string.Format("{0}={1}", key, value));
                    }
                    else
                    {
                        newString.Append(keyValuePair);
                    }
                }

                return newString.ToString();
            }
            catch
            {
                return "MASKED";
            }
        }

        private static JArray MaskArrayInNameValueForm(JArray jarray)
        {
            JArray newJarray = new JArray();
            foreach (JToken jtoken in jarray)
            {
                if (jtoken.Type == JTokenType.Object)
                {
                    newJarray.Add(MaskObjectInNameValueForm(jtoken.ToObject<JObject>()));
                    continue;
                }

                newJarray.Add(jtoken);
            }

            return newJarray;
        }

        private static JObject MaskObjectInNameValueForm(JObject jobject)
        {
            if (jobject == null)
            {
                return null;
            }

            JToken nameJToken;
            if (!jobject.TryGetValue("name", StringComparison.InvariantCultureIgnoreCase, out nameJToken) ||
                nameJToken.Type != JTokenType.String)
            {
                // the object has no property with "name" key or the token is not a JObject
                return jobject;
            }

            JToken valueJToken;
            if (!jobject.TryGetValue("value", StringComparison.InvariantCultureIgnoreCase, out valueJToken))
            {
                // the object has no property with "value" key
                return jobject;
            }

            // create a new JObject with all properties except "value"
            // this is done to keep the object as it was before, only the "value" property will be masked
            JObject newJobject = new JObject
            {
                jobject.Properties().Where(property => !property.Name.Equals("value", StringComparison.InvariantCultureIgnoreCase))
            };

            string key = nameJToken.ToObject<string>();
            string valueWithSourceSpelling = valueJToken.Parent.ToObject<JProperty>().Name;
            newJobject.Add(valueWithSourceSpelling, ReplacementFuncs.ContainsKey(key) ? ReplacementFuncs[key].Invoke(valueJToken) : valueJToken);

            return newJobject;
        }

        private static JObject MaskKeyValuePairObject(JObject jobject)
        {
            if (jobject == null)
            {
                return null;
            }

            string keyName = "Key";
            string valueName = "Value";
            if (jobject[keyName] == null || jobject[valueName] == null)
            {
                return jobject;
            }

            string key = jobject[keyName].ToString();
            if (!HeaderMaskFunctions.ContainsKey(key))
            {
                return jobject;
            }

            jobject[valueName] = HeaderMaskFunctions[key](jobject[valueName]);
            return jobject;
        }

        private static JObject MaskObject(JObject jobject)
        {
            // go through all the properties that are not null
            foreach (JProperty property in jobject.Properties().Where(property => property.Value.Type != JTokenType.Null))
            {
                // if there is a replacement function, invoke it
                if (ReplacementFuncs.ContainsKey(property.Name))
                {
                    property.Value = ReplacementFuncs[property.Name].Invoke(property.Value);
                    continue;
                }

                // if the value of the property is another object, go recursively into it
                if (property.Value.Type == JTokenType.Object)
                {
                    property.Value = MaskObject(property.Value.ToObject<JObject>());
                    continue;
                }

                if (property.Value.Type == JTokenType.Array)
                {
                    property.Value = MaskArray(property.Value.ToObject<JArray>(), MaskObject);
                    continue;
                }

                // if the value is a serialized string, Deserialize and go into it
                // example: {"key":"{\"abc\":\"cde\"}"}
                if (property.Value.Type == JTokenType.String)
                {
                    string v = property.Value.ToObject<string>();
                    if (string.IsNullOrWhiteSpace(v) || !(v[0].Equals('{') || v[0].Equals('[')))
                    {
                        continue;
                    }

                    JObject insideObject;
                    if (TryParseJObject(v, out insideObject))
                    {
                        insideObject = MaskObject(insideObject);
                        property.Value = JsonConvert.SerializeObject(insideObject);
                    }

                    JArray insideJarray;
                    if (TryParseJArray(v, out insideJarray))
                    {
                        insideJarray = MaskArray(insideJarray, MaskObject);
                        property.Value = JsonConvert.SerializeObject(insideJarray);
                    }
                }
            }

            return jobject;
        }

        private static JArray MaskArray(JArray jarray, Func<JObject, JObject> objectMasker)
        {
            JArray newjArray = new JArray();
            foreach (JToken jtoken in jarray)
            {
                if (jtoken.Type == JTokenType.Object)
                {
                    newjArray.Add(objectMasker(jtoken.ToObject<JObject>()));
                    continue;
                }

                if (jtoken.Type == JTokenType.Array)
                {
                    newjArray.Add(MaskArray(jtoken.ToObject<JArray>(), objectMasker));
                    continue;
                }

                newjArray.Add(jtoken);
            }

            return newjArray;
        }

        private static bool TryParseJObject(string s, out JObject jobject)
        {
            jobject = null;

            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            // this validation helps improve performance by not trying to parse if the first char is incorrect
            if (!s.Trim()[0].Equals('{'))
            {
                return false;
            }

            try
            {
                jobject = JObject.Parse(s);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        private static bool TryParseJArray(string s, out JArray jarray)
        {
            jarray = null;

            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            // this validation helps improve performance by not trying to parse if the first char is incorrect
            if (!s.Trim()[0].Equals('['))
            {
                return false;
            }

            try
            {
                jarray = JArray.Parse(s);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        private static string RunRegexMasking(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            string result = s;
            foreach (RegexMaskDefinition definition in RegexReplacers)
            {
                result = definition.Regex.Replace(result, definition.ReplacementFunction);
            }

            return result;
        }
    }
}