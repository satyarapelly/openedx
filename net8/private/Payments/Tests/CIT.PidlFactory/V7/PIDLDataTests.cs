// <copyright company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class PIDLDataTests
    {
        [TestMethod]
        public void TestTryGetPropertyValue()
        {
            // Arrange
            PIDLData pi = new PIDLData();
            JsonSerializerSettings serializationSettings = new JsonSerializerSettings();
            serializationSettings.NullValueHandling = NullValueHandling.Ignore;
            serializationSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            string pidlData = "{\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":\"visa\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"credit_card.visa\",\"sessionId\":\"3006b53b-665b-8b90-1df5-7ac4f9bd8363\",\"context\":\"purchase\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"greenId\":\"401d75d2-80b5-4f2c-908f-52d52b874189\"},\"details\":{\"dataType\":\"credit_card_visa_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"accountHolderName\":\"PX CIT Test\",\"accountToken\":\"Placeholder\",\"expiryMonth\":\"3\",\"expiryYear\":\"2020\",\"cvvToken\":\"Placeholder\",\"address\":{\"addressType\":\"billing\",\"addressOperation\":\"add\",\"addressCountry\":\"us\",\"address_line1\":\"1 Microsoft Way\",\"city\":\"Redmond\",\"region\":\"wa\",\"postal_code\":\"98052\",\"country\":\"us\"},\"permission\":{\"dataType\":\"permission_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"hmac\":{\"algorithm\":\"hmacsha256\",\"data\":\"VKdcPWyb+vZ/mdt+EFu64eGLbF0+08uH2N/L4ng6c=\",\"keyToken\":\"Placeholder\"},\"userCredential\":\"Bearer token\"}}}";
            pi = JsonConvert.DeserializeObject<PIDLData>(pidlData, serializationSettings);

            // Act, Assert
            Assert.IsNull(pi.TryGetPropertyValue(null), "Value of not existing property should be returned as null");
            Assert.IsNull(pi.TryGetPropertyValue("."), "Value of not existing property should be returned as null");

            Assert.AreEqual("credit_card", pi.TryGetPropertyValue("paymentMethodFamily"));
            Assert.IsNull(pi.TryGetPropertyValue("NonExistingProperty"), "Value of NonExistingProperty should be returned as null");
            
            Assert.AreEqual("401d75d2-80b5-4f2c-908f-52d52b874189", pi.TryGetPropertyValue("riskData.greenId"));            
            Assert.IsNull(pi.TryGetPropertyValue("riskData.NonExistingProperty"), "Value of NonExistingProperty should be returned as null");
            Assert.IsNull(pi.TryGetPropertyValue("NonExistingProperty.NonExistingProperty"), "Value of NonExistingProperty should be returned as null");

            Assert.AreEqual("1 Microsoft Way", pi.TryGetPropertyValue("details.address.address_line1"));
            Assert.IsNull(pi.TryGetPropertyValue("details.address.NonExistingProperty"), "Value of NonExistingProperty should be returned as null");
            Assert.IsNull(pi.TryGetPropertyValue("details.NonExistingProperty.NonExistingProperty"), "Value of NonExistingProperty should be returned as null");
            Assert.IsNull(pi.TryGetPropertyValue("NonExistingProperty.NonExistingProperty.NonExistingProperty"), "Value of NonExistingProperty should be returned as null");
        }

        [TestMethod]
        public void TestContainsProperty()
        {
            // Arrange
            PIDLData pi = new PIDLData();
            JsonSerializerSettings serializationSettings = new JsonSerializerSettings();
            serializationSettings.NullValueHandling = NullValueHandling.Ignore;
            serializationSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            string pidlData = "{\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":\"visa\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"credit_card.visa\",\"sessionId\":\"3006b53b-665b-8b90-1df5-7ac4f9bd8363\",\"context\":\"purchase\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"greenId\":\"401d75d2-80b5-4f2c-908f-52d52b874189\"},\"details\":{\"dataType\":\"credit_card_visa_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"accountHolderName\":\"PX CIT Test\",\"accountToken\":\"Placeholder\",\"expiryMonth\":\"3\",\"expiryYear\":\"2020\",\"cvvToken\":\"Placeholder\",\"address\":{\"addressType\":\"billing\",\"addressOperation\":\"add\",\"addressCountry\":\"us\",\"address_line1\":\"1 Microsoft Way\",\"city\":\"Redmond\",\"region\":\"wa\",\"postal_code\":\"98052\",\"country\":\"us\"},\"permission\":{\"dataType\":\"permission_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"hmac\":{\"algorithm\":\"hmacsha256\",\"data\":\"VKdcPWyb+vZ/mdt+EFu64eGLbF0+08uH2N/L4ng6c=\",\"keyToken\":\"Placeholder\"},\"userCredential\":\"Bearer token\"}}}";
            pi = JsonConvert.DeserializeObject<PIDLData>(pidlData, serializationSettings);

            // Act, Assert
            Assert.IsTrue(pi.ContainsProperty("details.address.city"));
            Assert.IsFalse(pi.ContainsProperty("details.address.NonExistingProperty"));
        }

        [TestMethod]
        public void TestTrySetProperty()
        {
            // Arrange
            PIDLData pi = new PIDLData();
            JsonSerializerSettings serializationSettings = new JsonSerializerSettings();
            serializationSettings.NullValueHandling = NullValueHandling.Ignore;
            serializationSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            string pidlData = "{\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":\"visa\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"credit_card.visa\",\"sessionId\":\"3006b53b-665b-8b90-1df5-7ac4f9bd8363\",\"context\":\"purchase\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"greenId\":\"401d75d2-80b5-4f2c-908f-52d52b874189\"},\"details\":{\"dataType\":\"credit_card_visa_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"accountHolderName\":\"PX CIT Test\",\"accountToken\":\"Placeholder\",\"expiryMonth\":\"3\",\"expiryYear\":\"2020\",\"cvvToken\":\"Placeholder\",\"address\":{\"addressType\":\"billing\",\"addressOperation\":\"add\",\"addressCountry\":\"us\",\"address_line1\":\"1 Microsoft Way\",\"city\":\"Redmond\",\"region\":\"wa\",\"postal_code\":\"98052\",\"country\":\"us\"},\"permission\":{\"dataType\":\"permission_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"hmac\":{\"algorithm\":\"hmacsha256\",\"data\":\"VKdcPWyb+vZ/mdt+EFu64eGLbF0+08uH2N/L4ng6c=\",\"keyToken\":\"Placeholder\"},\"userCredential\":\"Bearer token\"}}}";
            string value = "testValue";
            pi = JsonConvert.DeserializeObject<PIDLData>(pidlData, serializationSettings);

            // Act
            var updateResult = pi.TrySetProperty("details.address.city", value);
            var createResult = pi.TrySetProperty("details.address.NonExistingProperty", value);
            var rootUpdateResult = pi.TrySetProperty("paymentMethodFamily", "debit_card");

            // Assert
            Assert.IsNotNull(updateResult, "Result expected to be true or false");
            Assert.IsTrue(updateResult, "Update result is expected to be true");
            Assert.IsTrue(createResult, "Create result is expected to true");
            Assert.AreEqual(pi.TryGetPropertyValue("details.address.city"), value, "Expected value to be updated on a property");
            Assert.AreEqual(pi.TryGetPropertyValue("details.address.NonExistingProperty"), value, "Expected property to be created and value set");
            Assert.IsTrue(rootUpdateResult, "Update root level result is expected to be true");
            Assert.AreEqual(pi.TryGetPropertyValue("paymentMethodFamily"), "debit_card", "Expected value to be updated on a root property");
        }

        [TestMethod]
        public void TestRenameProperty()
        {
            // Arrange
            PIDLData address = new PIDLData();
            JsonSerializerSettings serializationSettings = new JsonSerializerSettings();
            serializationSettings.NullValueHandling = NullValueHandling.Ignore;
            serializationSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            string addressData = "{\"addressLine1\":\"One Microsoft Way\",\"addressLine2\":\"\",\"addressLine3\":null,\"city\":\"Redmond\",\"postalCode\":\"98052\"}";
            address = JsonConvert.DeserializeObject<PIDLData>(addressData, serializationSettings);

            // Act
            address.RenameProperty("addressLine1", "address_line1");
            address.RenameProperty("addressLine2", "address_line2");
            address.RenameProperty("addressLine3", "address_line3");

            // Assert
            Assert.AreEqual(address["address_line1"], "One Microsoft Way", "address_line1 is as expected");
            Assert.AreEqual(address["address_line2"], string.Empty, "address_line2 is as expected");
            Assert.AreEqual(address["address_line3"], null, "address_line3 is as expected");
            Assert.IsFalse(address.TryGetValue("addressLine1", out object val1), "addressLine1 is not renamed correctly");
            Assert.IsFalse(address.TryGetValue("addressLine2", out object val2), "addressLine2 is not renamed correctly");
            Assert.IsFalse(address.TryGetValue("addressLine3", out object val3), "addressLine3 is not renamed correctly");
        }
    }
}
