// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using global::Tests.Common.Model.Pidl;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using static System.Net.WebRequestMethods;
    using static CIT.PXService.Tests.AddressTestsUtil;
    using Constants = global::Tests.Common.Model.Pidl.Constants;

    [TestClass]
    public class AddressesTests : TestBase
    {
        [TestInitialize]
        public void Startup()
        {
            PXSettings.AddressEnrichmentService.Responses.Clear();
            PXSettings.AccountsService.Responses.Clear();

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [TestMethod]
        public async Task LegacyValidateAddress_ValidAddress_Success()
        {
            // Arrange
            PXSettings.AccountsService.ArrangeResponse("\"Valid\"");

            var validAddress = new
            {
                address_line1 = "1 Microsoft Way",
                country = "US",
                city = "Redmond",
                region = "WA",
                postal_code = "98052"
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/addresses/legacyValidate/", new StringContent(JsonConvert.SerializeObject(validAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [TestMethod]
        public async Task LegacyValidateAddress_InvalidAddress_BadRequestWithErrorCode()
        {
            // Arrange
            PXSettings.AccountsService.ArrangeResponse(
                statusCode: HttpStatusCode.BadRequest,
                content: "{\"Code\": \"60042\", \"Object_type\": \"AddressValidation\", \"Resource_status\": \"Active\", \"Reason\": \"MultipleCitiesFound - Details: Valid State Code and City Name passed, but the city has multiple ZIP Codes.  Returned all ZIP Codes for this city.\r\n\"}");

            // Act
            var result = await PXClient.PostAsync("/v7.0/addresses/legacyValidate/", new StringContent("someaddress", Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

            string contentStr = await result.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(contentStr);
            JToken code = json.SelectToken("ErrorCode");
            Assert.IsNotNull(code);
            Assert.AreEqual("60042", code.Value<string>());
        }

        [TestMethod]
        public async Task ModernValidateAddress_ValidAddress_Success()
        {
            // Arrange
            var success = new
            {
                original_address = new
                {
                    address_line1 = "1 Microsoft Way",
                    city = "Redmond",
                    country = "US",
                    postal_code = "98052",
                    region = "WA"
                },
                suggested_address = new
                {
                    address_line1 = "1 MICROSOFT WAY",
                    city = "REDMOND",
                    country = "US",
                    postal_code = "98052-8300",
                    region = "WA"
                },
                status = "VerifiedShippable"
            };

            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(success));

            // Act
            var validAddress = new
            {
                address_line1 = "1 Microsoft Way",
                country = "US",
                city = "Redmond",
                region = "WA",
                postal_code = "98052"
            };

            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/addresses/modernValidate/", new StringContent(JsonConvert.SerializeObject(validAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Modern address validation failed with valid address");
        }

        [TestMethod]
        public async Task ModernValidateAddress_InvalidAddress_BadRequestWithErrorCode()
        {
            // Arrange
            var error = new
            {
                original_address = new
                {
                    address_line1 = "1 Microsoft Way",
                    country = "us",
                    city = "Redmond",
                    region = "WA",
                    postal_code = "00000"
                },
                suggested_addresses = new
                {
                    address_line1 = "1 MICROSOFT WAY",
                    country = "US",
                    city = "REDMOND",
                    region = "WA",
                    postal_code = "98052-8300"
                },
                status = "InteractionRequired"
            };

            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(error));

            // Act
            var invalidAddress = new
            {
                address_line1 = "1 Microsoft Way",
                country = "us",
                city = "Redmond",
                region = "WA",
                postal_code = "00000"
            };

            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/addresses/modernValidate/", new StringContent(JsonConvert.SerializeObject(invalidAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert
            string expectedErrorCode = "InteractionRequired";

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

            string contentStr = await result.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(contentStr);
            JToken errorCode = json.SelectToken("ErrorCode");
            Assert.IsNotNull(errorCode, "ErrorCode is missing in response");
            Assert.AreEqual(expectedErrorCode, errorCode.Value<string>(), $"Modern address validation failed. Expected error code {expectedErrorCode}");
        }

        [TestMethod]
        public async Task ModernValidateAddress_InvalidPostalCode_BadRequestWithErrorCode()
        {
            // Arrange
            var error = new
            {
                original_address = new
                {
                    address_line1 = "1 Microsoft Way",
                    country = "us",
                    city = "Redmond",
                    region = "WA",
                    postal_code = "00000"
                },
                suggested_addresses = new
                {
                    address_line1 = "1 MICROSOFT WAY",
                    country = "US",
                    city = "REDMOND",
                    region = "WA",
                    postal_code = "98052-8300"
                },
                status = "InteractionRequired",
                validation_message = "Address field invalid for property: 'PostalCode'"
            };

            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(error));

            // Act
            var invalidAddress = new
            {
                address_line1 = "1 Microsoft Way",
                country = "us",
                city = "Redmond",
                region = "WA",
                postal_code = "00000"
            };

            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/addresses/modernValidate/", new StringContent(JsonConvert.SerializeObject(invalidAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert
            string expectedErrorCode = "InvalidCityRegionPostalCode";

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

            string contentStr = await result.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(contentStr);
            JToken errorCode = json.SelectToken("ErrorCode");
            Assert.IsNotNull(errorCode, "ErrorCode is missing in response");
            Assert.AreEqual(expectedErrorCode, errorCode.Value<string>(), $"Modern address validation failed. Expected error code {expectedErrorCode}");
        }

        [TestMethod]
        public async Task ModernValidateAddress_InvalidProvince_BadRequestWithErrorCode()
        {
            // Arrange
            var error = new
            {
                original_address = new
                {
                    address_line1 = "1 Microsoft Way",
                    country = "us",
                    city = "Redmond",
                    region = "AB",
                    postal_code = "98052-8300"
                },
                suggested_addresses = new
                {
                    address_line1 = "1 MICROSOFT WAY",
                    country = "US",
                    city = "REDMOND",
                    region = "WA",
                    postal_code = "98052-8300"
                },
                status = "InteractionRequired",
                validation_message = "Address field invalid for property: 'Province'"
            };

            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(error));

            // Act
            var invalidAddress = new
            {
                address_line1 = "1 Microsoft Way",
                country = "us",
                city = "Redmond",
                region = "AB",
                postal_code = "98052-8300"
            };

            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/addresses/modernValidate/", new StringContent(JsonConvert.SerializeObject(invalidAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert
            string expectedErrorCode = "InvalidCityRegionPostalCode";

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

            string contentStr = await result.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(contentStr);
            JToken errorCode = json.SelectToken("ErrorCode");
            Assert.IsNotNull(errorCode, "ErrorCode is missing in response");
            Assert.AreEqual(expectedErrorCode, errorCode.Value<string>(), $"Modern address validation failed. Expected error code {expectedErrorCode}");
        }

        [TestMethod]
        public async Task ModernValidateAddress_InvalidCity_BadRequestWithErrorCode()
        {
            // Arrange
            var error = new
            {
                original_address = new
                {
                    address_line1 = "1 Microsoft Way",
                    country = "us",
                    city = "Redmon",
                    region = "WA",
                    postal_code = "98052-8300"
                },
                suggested_addresses = new
                {
                    address_line1 = "1 MICROSOFT WAY",
                    country = "US",
                    city = "REDMOND",
                    region = "WA",
                    postal_code = "98052-8300"
                },
                status = "InteractionRequired",
                validation_message = "Address field invalid for property: 'City'"
            };

            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(error));

            // Act
            var invalidAddress = new
            {
                address_line1 = "1 Microsoft Way",
                country = "us",
                city = "Redmon",
                region = "WA",
                postal_code = "98052-8300"
            };

            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/addresses/modernValidate/", new StringContent(JsonConvert.SerializeObject(invalidAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert
            string expectedErrorCode = "InvalidCityRegionPostalCode";

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

            string contentStr = await result.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(contentStr);
            JToken errorCode = json.SelectToken("ErrorCode");
            Assert.IsNotNull(errorCode, "ErrorCode is missing in response");
            Assert.AreEqual(expectedErrorCode, errorCode.Value<string>(), $"Modern address validation failed. Expected error code {expectedErrorCode}");
        }

        [TestMethod]
        public async Task ModernValidateAddress_InvalidStreet_BadRequestWithErrorCode()
        {
            // Arrange
            var error = new
            {
                original_address = new
                {
                    address_line1 = "1 Microsoft",
                    country = "us",
                    city = "Redmond",
                    region = "WA",
                    postal_code = "98052-8300"
                },
                suggested_addresses = new
                {
                    address_line1 = "1 MICROSOFT WAY",
                    country = "US",
                    city = "REDMOND",
                    region = "WA",
                    postal_code = "98052-8300"
                },
                status = "InteractionRequired",
                validation_message = "Address field invalid for property: 'Street'"
            };

            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(error));

            // Act
            var invalidAddress = new
            {
                address_line1 = "1 Microsoft",
                country = "us",
                city = "Redmond",
                region = "WA",
                postal_code = "98052-8300"
            };

            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/addresses/modernValidate/", new StringContent(JsonConvert.SerializeObject(invalidAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert
            string expectedErrorCode = "InvalidStreet";

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

            string contentStr = await result.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(contentStr);
            JToken errorCode = json.SelectToken("ErrorCode");
            Assert.IsNotNull(errorCode, "ErrorCode is missing in response");
            Assert.AreEqual(expectedErrorCode, errorCode.Value<string>(), $"Modern address validation failed. Expected error code {expectedErrorCode}");
        }

        [DataRow("MSAAccount1", "storify")]
        [DataRow("MSAAccount1", "xboxsubs")]
        [DataRow("MSAAccount1", "xboxsettings")]
        [DataRow("MSAAccount1", "saturn")]
        [DataRow("MSAAccount1", "storify", true)]
        [DataRow("MSAAccount1", "xboxsubs", true)]
        [DataRow("MSAAccount1", "xboxsettings", true)]
        [DataRow("MSAAccount1", "saturn", true)]
        [TestMethod]
        public async Task ValidateAddress_Success_ShippingV3(string accountId, string partner, bool useAccountsForAvs = false)
        {
            // Arrange
            // "One Microso" is done on purpose, as we are testing the AVS will suggest the correct spelling.
            var response = new
            {
                original_address = new
                {
                    address_line1 = "One Microso",
                    country = "us",
                    city = "Redmond",
                    region = "wa",
                    postal_code = "98052"
                },
                suggested_address =
                new
                {
                    country = "US",
                    region = "WA",
                    city = "Redmond",
                    address_line1 = "1 Microsoft Way",
                    postal_code = "98052-8300"
                },
                status = "Verified"
            };

            PXSettings.AddressEnrichmentService.ArrangeResponse(JsonConvert.SerializeObject(response), HttpStatusCode.OK, HttpMethod.Post, "/addresses/validate");

            if (useAccountsForAvs)
            {
                PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(response), HttpStatusCode.OK, HttpMethod.Post, "/addresses/validate");
                PXFlightHandler.AddToEnabledFlights("PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment");
            }

            // Act
            // "One Microso" is done on purpose, as we are testing the AVS will suggest the correct spelling.
            var userProvidedAddress = new
            {
                country = "us",
                region = "wa",
                city = "Redmond",
                address_line1 = "One Microso",
                postal_code = "98052",
                first_name = "Test",
                last_name = "Test111",
                phone_number = "12323211111",
                is_avs_validated = string.Empty,
                validate = string.Empty,
            };

            var userProvidedData = new
            {
                addressType = "px_v3_shipping",
                addressCountry = "us",
                addressShippingV3 = userProvidedAddress,
                set_as_default_shipping_address = true
            };

            object addressItems = response.suggested_address;

            var respAddressLine1 = GetValueFromGenericObject(addressItems, "address_line1");
            var respCity = GetValueFromGenericObject(addressItems, "city");
            var respRegion = GetValueFromGenericObject(addressItems, "region");
            var respPostalCode = GetValueFromGenericObject(addressItems, "postal_code");

            var respCityPostal = string.Format(@"{0}, {1} {2}", respCity, respRegion, respPostalCode);
            var respCountry = GetValueFromGenericObject(addressItems, "country");

            var expectedSuggested = new
            {
                addressLine1 = respAddressLine1,
                cityPostal = respCityPostal,
                country = respCountry
            };

            var expectedEntered = new
            {
                addressLine1 = userProvidedAddress.address_line1,
                cityPostal = string.Format(@"{0}, {1} {2}", userProvidedAddress.city, userProvidedAddress.region, userProvidedAddress.postal_code),
                country = userProvidedAddress.country
            };

            string url = $"/v7.0/{accountId}/addressesEx?partner={partner}&language=en-US&avsSuggest=true";
            HttpResponseMessage result = await PXClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(userProvidedData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            string contentStr = await result.Content.ReadAsStringAsync();
            var patternGuid = @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}";
            contentStr = Regex.Replace(contentStr, patternGuid, "Replaced GUID");
            JObject json = JObject.Parse(contentStr);

            var context = json["clientAction"]["context"][0];
            var displayDescription = context["displayDescription"];
            var possibleOptions = displayDescription[0]["members"][2]["possibleOptions"];
            var suggestedAddress = possibleOptions["suggested_0"]["displayContent"]["members"];
            var addressLine1 = suggestedAddress[2]["displayContent"];
            var cityPostal = suggestedAddress[3]["displayContent"];
            var country = suggestedAddress[4]["displayContent"];

            var suggestedAddressAriaLabel = possibleOptions["suggested_0"]["accessibilityName"];
            var expectedAriaLabel = "We suggest Option 1 of 2 First name: Test, Last name: Test111, Address line 1: 1 Microsoft Way, City: Redmond, State: WA, Postal Code: 98052-8300, Country: US, Use this address";
            Assert.IsTrue(string.Equals(suggestedAddressAriaLabel.ToString(), expectedAriaLabel), "Suggested Address aria label from response does not match expected");

            // Checking that the suggested address lines match what we expect
            Assert.IsTrue(string.Equals(addressLine1.ToString(), expectedSuggested.addressLine1, StringComparison.OrdinalIgnoreCase), "Suggested addressLine1 from response does not match expected");
            Assert.IsTrue(string.Equals(cityPostal.ToString(), expectedSuggested.cityPostal, StringComparison.OrdinalIgnoreCase), "Suggested cityPostal from response does not match expected");
            Assert.IsTrue(string.Equals(country.ToString(), expectedSuggested.country, StringComparison.OrdinalIgnoreCase), "Suggested country from response does not match expected");

            var enteredAddress = possibleOptions["entered"]["displayContent"]["members"];
            var enteredAddressLine1 = enteredAddress[2]["displayContent"];
            var enteredCityPostal = enteredAddress[3]["displayContent"];
            var enteredCountry = enteredAddress[4]["displayContent"];

            // Checking that the returned data about stating what the user entered matches what the user entered
            Assert.IsTrue(string.Equals(enteredAddressLine1.ToString(), expectedEntered.addressLine1, StringComparison.OrdinalIgnoreCase), "Entered addressLine1 from response does not match user entered data");
            Assert.IsTrue(string.Equals(enteredCityPostal.ToString(), expectedEntered.cityPostal, StringComparison.OrdinalIgnoreCase), "Entered cityPostal from response does not match user entered data");
            Assert.IsTrue(string.Equals(enteredCountry.ToString(), expectedEntered.country, StringComparison.OrdinalIgnoreCase), "Entered country from response does not match user entered data");

            string addrObjectName = "addressShippingV3";
            var dataDescription = context["data_description"];
            var dataAddressCountry = dataDescription[addrObjectName][0]["data_description"]["country"]["default_value"];
            var dataAddressRegion = dataDescription[addrObjectName][0]["data_description"]["region"]["default_value"];
            var dataAddressCity = dataDescription[addrObjectName][0]["data_description"]["city"]["default_value"];
            var dataAddressLine1 = dataDescription[addrObjectName][0]["data_description"]["address_line1"]["default_value"];
            var dataAddressPostalCode = dataDescription[addrObjectName][0]["data_description"]["postal_code"]["default_value"];
            var dataAddressFirstName = dataDescription[addrObjectName][0]["data_description"]["first_name"]["default_value"];
            var dataAddressLastName = dataDescription[addrObjectName][0]["data_description"]["last_name"]["default_value"];
            var dataAddressPhoneNumber = dataDescription[addrObjectName][0]["data_description"]["phone_number"]["default_value"];
            var dataSetAsDefaultShippingAddress = dataDescription["set_as_default_shipping_address"]["default_value"];

            // Checking that the default values that will be plugged in to AddAddress pages 1 and 2, on a back navigation, match what the user previously entered
            Assert.IsTrue(string.Equals(dataAddressCountry.ToString(), userProvidedAddress.country, StringComparison.OrdinalIgnoreCase), "Data Description country from response does not match user entered data");
            Assert.IsTrue(string.Equals(dataAddressRegion.ToString(), userProvidedAddress.region, StringComparison.OrdinalIgnoreCase), "Data Description region from response does not match user entered data");
            Assert.IsTrue(string.Equals(dataAddressCity.ToString(), userProvidedAddress.city, StringComparison.OrdinalIgnoreCase), "Data Description city from response does not match user entered data");
            Assert.IsTrue(string.Equals(dataAddressLine1.ToString(), userProvidedAddress.address_line1, StringComparison.OrdinalIgnoreCase), "Data Description address_line1 from response does not match user entered data");
            Assert.IsTrue(string.Equals(dataAddressPostalCode.ToString(), userProvidedAddress.postal_code, StringComparison.OrdinalIgnoreCase), "Data Description postal_code from response does not match user entered data");
            Assert.IsTrue(string.Equals(dataAddressFirstName.ToString(), userProvidedAddress.first_name, StringComparison.OrdinalIgnoreCase), "Data Description first_name from response does not match user entered data");
            Assert.IsTrue(string.Equals(dataAddressLastName.ToString(), userProvidedAddress.last_name, StringComparison.OrdinalIgnoreCase), "Data Description last_name from response does not match user entered data");
            Assert.IsTrue(string.Equals(dataAddressPhoneNumber.ToString(), userProvidedAddress.phone_number, StringComparison.OrdinalIgnoreCase), "Data Description phone_number from response does not match user entered data");
            Assert.IsTrue(dataSetAsDefaultShippingAddress.ToString() == "True", "Data Description set_as_default_shipping_address from response does not match user entered data");
        }

        [DataRow("MSAAccount1", "storify")]
        [DataRow("MSAAccount1", "xboxsubs")]
        [DataRow("MSAAccount1", "xboxsettings")]
        [DataRow("MSAAccount1", "saturn")]
        [DataRow("MSAAccount1", "storify", true)]
        [DataRow("MSAAccount1", "xboxsubs", true)]
        [DataRow("MSAAccount1", "xboxsettings", true)]
        [DataRow("MSAAccount1", "saturn", true)]
        [TestMethod]
        public async Task ValidateAddress_Success_BillingV3(string accountId, string partner, bool useAccountsForAvs = false)
        {
            // Arrange
            // "One Microso" is done on purpose, as we are testing the AVS will suggest the correct spelling.
            var response = new
            {
                original_address = new
                {
                    address_line1 = "One Microso",
                    country = "us",
                    city = "Redmond",
                    region = "wa",
                    postal_code = "98052"
                },
                suggested_address =
                new
                {
                    country = "US",
                    region = "WA",
                    city = "Redmond",
                    address_line1 = "1 Microsoft Way",
                    postal_code = "98052-8300"
                },
                status = "Verified"
            };

            PXSettings.AddressEnrichmentService.ArrangeResponse(JsonConvert.SerializeObject(response), HttpStatusCode.OK, HttpMethod.Post, "/addresses/validate");

            if (useAccountsForAvs)
            {
                PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(response), HttpStatusCode.OK, HttpMethod.Post, "/addresses/validate");
                PXFlightHandler.AddToEnabledFlights("PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment");
            }

            // Act
            // "One Microso" is done on purpose, as we are testing the AVS will suggest the correct spelling.
            var userProvidedAddress = new
            {
                country = "us",
                region = "wa",
                city = "Redmond",
                address_line1 = "One Microso",
                postal_code = "98052",
                is_avs_validated = string.Empty,
                validate = string.Empty,
            };

            var userProvidedData = new
            {
                addressType = "px_v3_billing",
                addressCountry = "us",
                addressBillingV3 = userProvidedAddress,
                set_as_default_billing_address = true
            };

            object addressItems = response.suggested_address;

            var respAddressLine1 = GetValueFromGenericObject(addressItems, "address_line1");
            var respCity = GetValueFromGenericObject(addressItems, "city");
            var respRegion = GetValueFromGenericObject(addressItems, "region");
            var respPostalCode = GetValueFromGenericObject(addressItems, "postal_code");

            var respCityPostal = string.Format(@"{0}, {1} {2}", respCity, respRegion, respPostalCode);
            var respCountry = GetValueFromGenericObject(addressItems, "country");

            var expectedSuggested = new
            {
                addressLine1 = respAddressLine1,
                cityPostal = respCityPostal,
                country = respCountry
            };

            var expectedEntered = new
            {
                addressLine1 = userProvidedAddress.address_line1,
                cityPostal = string.Format(@"{0}, {1} {2}", userProvidedAddress.city, userProvidedAddress.region, userProvidedAddress.postal_code),
                country = userProvidedAddress.country
            };

            string url = $"/v7.0/{accountId}/addressesEx?partner={partner}&language=en-US&avsSuggest=true";
            HttpResponseMessage result = await PXClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(userProvidedData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            string contentStr = await result.Content.ReadAsStringAsync();
            var patternGuid = @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}";
            contentStr = Regex.Replace(contentStr, patternGuid, "Replaced GUID");
            JObject json = JObject.Parse(contentStr);

            var context = json["clientAction"]["context"][0];
            var displayDescription = context["displayDescription"];
            var possibleOptions = displayDescription[0]["members"][2]["possibleOptions"];
            var suggestedAddress = possibleOptions["suggested_0"]["displayContent"]["members"];
            var addressLine1 = suggestedAddress[2]["displayContent"];
            var cityPostal = suggestedAddress[3]["displayContent"];
            var country = suggestedAddress[4]["displayContent"];

            var suggestedAddressAriaLabel = possibleOptions["suggested_0"]["accessibilityName"];
            var expectedAriaLabel = "We suggest Option 1 of 2 Address line 1: 1 Microsoft Way, City: Redmond, State: WA, Postal Code: 98052-8300, Country: US, Use this address";
            Assert.IsTrue(string.Equals(suggestedAddressAriaLabel.ToString(), expectedAriaLabel), "Suggested Address aria label from response does not match expected");

            // Checking that the suggested address lines match what we expect
            Assert.IsTrue(string.Equals(addressLine1.ToString(), expectedSuggested.addressLine1, StringComparison.OrdinalIgnoreCase), "Suggested addressLine1 from response does not match expected");
            Assert.IsTrue(string.Equals(cityPostal.ToString(), expectedSuggested.cityPostal, StringComparison.OrdinalIgnoreCase), "Suggested cityPostal from response does not match expected");
            Assert.IsTrue(string.Equals(country.ToString(), expectedSuggested.country, StringComparison.OrdinalIgnoreCase), "Suggested country from response does not match expected");

            var enteredAddress = possibleOptions["entered"]["displayContent"]["members"];
            var enteredAddressLine1 = enteredAddress[2]["displayContent"];
            var enteredCityPostal = enteredAddress[3]["displayContent"];
            var enteredCountry = enteredAddress[4]["displayContent"];

            // Checking that the returned data about stating what the user entered matches what the user entered
            Assert.IsTrue(string.Equals(enteredAddressLine1.ToString(), expectedEntered.addressLine1, StringComparison.OrdinalIgnoreCase), "Entered addressLine1 from response does not match user entered data");
            Assert.IsTrue(string.Equals(enteredCityPostal.ToString(), expectedEntered.cityPostal, StringComparison.OrdinalIgnoreCase), "Entered cityPostal from response does not match user entered data");
            Assert.IsTrue(string.Equals(enteredCountry.ToString(), expectedEntered.country, StringComparison.OrdinalIgnoreCase), "Entered country from response does not match user entered data");

            string addrObjectName = "addressBillingV3";
            var dataDescription = context["data_description"];
            var dataAddressCountry = dataDescription[addrObjectName][0]["data_description"]["country"]["default_value"];
            var dataAddressRegion = dataDescription[addrObjectName][0]["data_description"]["region"]["default_value"];
            var dataAddressCity = dataDescription[addrObjectName][0]["data_description"]["city"]["default_value"];
            var dataAddressLine1 = dataDescription[addrObjectName][0]["data_description"]["address_line1"]["default_value"];
            var dataAddressPostalCode = dataDescription[addrObjectName][0]["data_description"]["postal_code"]["default_value"];
            var dataSetAsDefaultBillingAddress = dataDescription["set_as_default_billing_address"]["default_value"];

            // Checking that the default values that will be plugged in to AddAddress pages 1 and 2, on a back navigation, match what the user previously entered
            Assert.IsTrue(string.Equals(dataAddressCountry.ToString(), userProvidedAddress.country, StringComparison.OrdinalIgnoreCase), "Data Description country from response does not match user entered data");
            Assert.IsTrue(string.Equals(dataAddressRegion.ToString(), userProvidedAddress.region, StringComparison.OrdinalIgnoreCase), "Data Description region from response does not match user entered data");
            Assert.IsTrue(string.Equals(dataAddressCity.ToString(), userProvidedAddress.city, StringComparison.OrdinalIgnoreCase), "Data Description city from response does not match user entered data");
            Assert.IsTrue(string.Equals(dataAddressLine1.ToString(), userProvidedAddress.address_line1, StringComparison.OrdinalIgnoreCase), "Data Description address_line1 from response does not match user entered data");
            Assert.IsTrue(string.Equals(dataAddressPostalCode.ToString(), userProvidedAddress.postal_code, StringComparison.OrdinalIgnoreCase), "Data Description postal_code from response does not match user entered data");
            Assert.IsTrue(dataSetAsDefaultBillingAddress.ToString() == "True", "Data Description set_as_default_billing_address from response does not match user entered data");
        }

        [TestMethod]
        public async Task PostAddress_RateLimitPerAccount()
        {
            // Act
            var userProvidedAddress = new
            {
                country = "us",
                region = "wa",
                city = "Redmond",
                address_line1 = "One Microso",
                postal_code = "98052",
                first_name = "Test",
                last_name = "Test111",
                phone_number = "12323211111",
                is_avs_validated = string.Empty,
                validate = string.Empty,
                set_as_default_shipping_address = "false"
            };
            
            var accountPutProfile = false;

            PXSettings.AccountsService.PreProcess = async (accountReq) =>
            {
                await Task.Delay(0);
                if (accountReq.Method == HttpMethod.Put)
                {
                    accountPutProfile = true;
                }
            };

            PXFlightHandler.AddToEnabledFlights("PX9002311");

            string url = $"v7.0/abc/addressesEx?partner=cart&language=en-US&avsSuggest=false";
            HttpResponseMessage result = await PXClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(userProvidedAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual(accountPutProfile, false);
            PXSettings.AccountsService.ResetToDefaults();
        }

        /// <summary>
        /// This CIT is used to verify the account service is called with syncToLegacy=1 when the scenario is profileAddress.
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="scenario"></param>
        /// <returns></returns>
        [DataRow("amcweb", "profileAddress")]
        [DataRow("amcweb", null)]
        [DataRow("officesmb", "profileAddress")]
        [DataRow("officesmb", null)]
        [DataRow("amcweb", "paynow")]
        [DataRow("webblends", null)]
        [TestMethod]
        public async Task GetAddressDescription_ProfileAddress_ShouldCreateLegacyAccountAndSync(string partner, string scenario)
        {
            // Arrange
            var userProvidedAddress = new
            {
                country = "us",
                region = "wa",
                city = "Redmond",
                address_line1 = "One Microsoft",
                postal_code = "98052",
                first_name = "Test",
                last_name = "Test111",
                phone_number = "12323211111",
                is_avs_validated = string.Empty,
                validate = string.Empty,
                set_as_default_shipping_address = "false"
            };

            string url = $"/v7.0/Account001/addressesEx?partner={partner}&language=en-US&avsSuggest=false";

            if (scenario != null)
            {
                url += $"&scenario={scenario}";
            }

            if (string.Equals(partner, "officesmb", StringComparison.OrdinalIgnoreCase))
            {
                string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":{\"address\":{\"shipping\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null}}},\"features\":{\"useLegacyAccountAndSync\":{\"applicableMarkets\":[]}}},\"update\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":{\"address\":{\"shipping\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null}}},\"features\":{\"useLegacyAccountAndSync\":{\"applicableMarkets\":[]}}}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            PXSettings.AccountsService.PreProcess = (accountServiceRequest) =>
            {
                if ((string.Equals(partner, "amcweb", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partner, "officesmb", StringComparison.OrdinalIgnoreCase))
                   && string.Equals(scenario, "profileAddress", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.IsTrue(accountServiceRequest.RequestUri.AbsoluteUri.Contains($"/syncToLegacy=1"));
                }
                else
                {
                    Assert.IsTrue(accountServiceRequest.RequestUri.AbsoluteUri.Contains($"/syncToLegacy=3"));
                }
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(userProvidedAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert
            Assert.IsNotNull(result);
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("MSAAccount1", "storify")]
        [DataRow("MSAAccount1", "xboxsubs")]
        [DataRow("MSAAccount1", "xboxsettings")]
        [DataRow("MSAAccount1", "saturn")]
        [TestMethod]
        public async Task PostAddress_Success(string accountId, string partner)
        {
            // setup the accessor
            var response = new
            {
                id = "test-address-id",
                country = "us",
                region = "wa",
                city = "Redmond",
                address_line1 = "One Microso",
                postal_code = "98052",
                first_name = "Test",
                last_name = "Test111",
                phone_number = "12323211111",
            };

            string matchingUri = $"/{accountId}/addresses";
            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(response), HttpStatusCode.OK, HttpMethod.Post, matchingUri);

            // Act
            var userProvidedAddress = new
            {
                country = "us",
                region = "wa",
                city = "Redmond",
                address_line1 = "One Microso",
                postal_code = "98052",
                first_name = "Test",
                last_name = "Test111",
                phone_number = "12323211111",
                is_avs_validated = string.Empty,
                validate = string.Empty,
                set_as_default_shipping_address = "false"
            };

            string url = $"v7.0/{accountId}/addressesEx?partner={partner}&language=en-US&avsSuggest=false";
            HttpResponseMessage result = await PXClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(userProvidedAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var patternGuid = @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}";
            string contentStr = await result.Content.ReadAsStringAsync();
            contentStr = Regex.Replace(contentStr, patternGuid, "Replaced GUID");
            JObject json = JObject.Parse(contentStr);

            var dataDescription = json["clientAction"]["context"];

            Assert.IsTrue(dataDescription["country"].ToString().Equals(userProvidedAddress.country), "Countries do not match");
            Assert.IsTrue(dataDescription["region"].ToString().Equals(userProvidedAddress.region), "Regions do not match");
            Assert.IsTrue(dataDescription["address_line1"].ToString().Equals(userProvidedAddress.address_line1), "Address Line 1 do not match");
            Assert.IsTrue(dataDescription["postal_code"].ToString().Equals(userProvidedAddress.postal_code), "Postal Codes do not match");
            Assert.IsTrue(dataDescription["first_name"].ToString().Equals(userProvidedAddress.first_name), "First Names do not match");
            Assert.IsTrue(dataDescription["last_name"].ToString().Equals(userProvidedAddress.last_name), "Last Names do not match");
            Assert.IsTrue(dataDescription["phone_number"].ToString().Equals(userProvidedAddress.phone_number), "Phone Numbers do not match");
            Assert.IsTrue(string.Equals(dataDescription["set_as_default_shipping_address"].ToString(), userProvidedAddress.set_as_default_shipping_address, StringComparison.OrdinalIgnoreCase), "set_as_default_shipping_address do not match");
        }

        [TestMethod]
        public void PXAddressV3Info_CreationWithFields()
        {
            string objectType = "object_type_test";
            string addressLine1 = "address_line1_test";
            string addressLine2 = "address_line2_test";
            string addressLine3 = "address_line3_test";
            string city = "city_test";
            string postalCode = "postal_code_test";
            string country = "country_test";
            string region = "region_test";
            string firstName = "first_name_test";
            string lastName = "last_name_test";
            string phoneNumber = "phone_number_test";
            string defaultShipping = "True"; // This should convert to lowercase "true"
            string defaultBilling = "False"; // This should convert to lowercase "false"

            var pidlInfo = new Dictionary<string, object>()
            {
                { "object_type", objectType },
                { "address_line1", addressLine1 },
                { "address_line2", addressLine2 },
                { "address_line3", addressLine3 },
                { "city", city },
                { "postal_code", postalCode },
                { "country", country },
                { "region", region },
                { "first_name", firstName },
                { "last_name", lastName },
                { "phone_number", phoneNumber },
                { "set_as_default_shipping_address", defaultShipping },
                { "set_as_default_billing_address", defaultBilling },
            };

            var info = new PXAddressV3Info(pidlInfo).GetPropertyDictionary();

            Assert.IsTrue(info.ContainsKey("object_type"), "object_type key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("address_line1"), "address_line1 key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("address_line2"), "address_line2 key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("address_line3"), "address_line3 key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("city"), "city key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("postal_code"), "postal_code key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("country"), "country key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("region"), "region key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("first_name"), "first_name key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("last_name"), "last_name key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("phone_number"), "phone_number key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("set_as_default_shipping_address"), "set_as_default_shipping_address key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("set_as_default_billing_address"), "set_as_default_billing_address key is missing from PXAddressV3Info instance");

            Assert.IsTrue(info["object_type"].Equals(objectType), "object_type do not match");
            Assert.IsTrue(info["address_line1"].Equals(addressLine1), "address_line1 do not match");
            Assert.IsTrue(info["address_line2"].Equals(addressLine2), "address_line2 do not match");
            Assert.IsTrue(info["address_line3"].Equals(addressLine3), "address_line3 do not match");
            Assert.IsTrue(info["city"].Equals(city), "city do not match");
            Assert.IsTrue(info["postal_code"].Equals(postalCode), "postal_code do not match");
            Assert.IsTrue(info["country"].Equals(country), "country do not match");
            Assert.IsTrue(info["region"].Equals(region), "region do not match");
            Assert.IsTrue(info["first_name"].Equals(firstName), "first_name do not match");
            Assert.IsTrue(info["last_name"].Equals(lastName), "last_name do not match");
            Assert.IsTrue(info["phone_number"].Equals(phoneNumber), "phone_number do not match");
            Assert.IsTrue(info["set_as_default_shipping_address"].Equals("true"), "set_as_default_shipping_address should be 'true'");
            Assert.IsTrue(info["set_as_default_billing_address"].Equals("false"), "set_as_default_billing_address should be 'false'");
        }

        [TestMethod]
        public void PXAddressV3Info_CreationWithoutFields()
        {
            var info = new PXAddressV3Info().GetPropertyDictionary();

            Assert.IsTrue(info.ContainsKey("object_type"), "object_type key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("address_line1"), "address_line1 key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("address_line2"), "address_line2 key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("address_line3"), "address_line3 key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("city"), "city key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("postal_code"), "postal_code key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("country"), "country key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("region"), "region key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("first_name"), "first_name key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("last_name"), "last_name key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("phone_number"), "phone_number key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("set_as_default_shipping_address"), "set_as_default_shipping_address key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("set_as_default_billing_address"), "set_as_default_billing_address key is missing from PXAddressV3Info instance");

            Assert.IsTrue(info["object_type"] == null, "object_type should be null");
            Assert.IsTrue(info["address_line1"] == null, "address_line1 should be null");
            Assert.IsTrue(info["address_line2"] == null, "address_line2 should be null");
            Assert.IsTrue(info["address_line3"] == null, "address_line3 should be null");
            Assert.IsTrue(info["city"] == null, "city should be null");
            Assert.IsTrue(info["postal_code"] == null, "postal_code should be null");
            Assert.IsTrue(info["country"] == null, "country should be null");
            Assert.IsTrue(info["region"] == null, "region should be null");
            Assert.IsTrue(info["first_name"] == null, "first_name should be null");
            Assert.IsTrue(info["last_name"] == null, "last_name should be null");
            Assert.IsTrue(info["phone_number"] == null, "phone_number should be null");
            Assert.IsTrue(info["set_as_default_shipping_address"].Equals("false"), "set_as_default_shipping_address should be 'false'");
            Assert.IsTrue(info["set_as_default_billing_address"].Equals("false"), "set_as_default_billing_address should be 'false'");
        }

        [TestMethod]
        public void PXAddressV3Info_CreationWithFieldsForAddressShippingV3()
        {
            string objectType = "object_type_test";
            string addressLine1 = "address_line1_test";
            string addressLine2 = "address_line2_test";
            string addressLine3 = "address_line3_test";
            string city = "city_test";
            string postalCode = "postal_code_test";
            string country = "country_test";
            string region = "region_test";
            string firstName = "first_name_test";
            string lastName = "last_name_test";
            string phoneNumber = "phone_number_test";
            string defaultShipping = bool.TrueString;

            var addressInfo = JObject.FromObject(new
            {
                object_type = objectType,
                address_line1 = addressLine1,
                address_line2 = addressLine2,
                address_line3 = addressLine3,
                city = city,
                postal_code = postalCode,
                country = country,
                region = region,
                first_name = firstName,
                last_name = lastName,
                phone_number = phoneNumber,
            });

            var pidlInfo = new Dictionary<string, object>()
            {
                { "set_as_default_shipping_address", defaultShipping },
                { "addressShippingV3", addressInfo },
            };

            var info = new PXAddressV3Info(pidlInfo).GetPropertyDictionary();

            Assert.IsTrue(info.ContainsKey("object_type"), "object_type key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("address_line1"), "address_line1 key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("address_line2"), "address_line2 key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("address_line3"), "address_line3 key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("city"), "city key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("postal_code"), "postal_code key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("country"), "country key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("region"), "region key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("first_name"), "first_name key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("last_name"), "last_name key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("phone_number"), "phone_number key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("set_as_default_shipping_address"), "set_as_default_shipping_address key is missing from PXAddressV3Info instance");

            Assert.IsTrue(info["object_type"].Equals(objectType), "object_type do not match");
            Assert.IsTrue(info["address_line1"].Equals(addressLine1), "address_line1 do not match");
            Assert.IsTrue(info["address_line2"].Equals(addressLine2), "address_line2 do not match");
            Assert.IsTrue(info["address_line3"].Equals(addressLine3), "address_line3 do not match");
            Assert.IsTrue(info["city"].Equals(city), "city do not match");
            Assert.IsTrue(info["postal_code"].Equals(postalCode), "postal_code do not match");
            Assert.IsTrue(info["country"].Equals(country), "country do not match");
            Assert.IsTrue(info["region"].Equals(region), "region do not match");
            Assert.IsTrue(info["first_name"].Equals(firstName), "first_name do not match");
            Assert.IsTrue(info["last_name"].Equals(lastName), "last_name do not match");
            Assert.IsTrue(info["phone_number"].Equals(phoneNumber), "phone_number do not match");
            Assert.IsTrue(info["set_as_default_shipping_address"].Equals(bool.TrueString.ToLower()), "set_as_default_shipping_address should be 'true'");
        }

        [TestMethod]
        public void PXAddressV3Info_CreationWithFieldsForAddressBillingV3()
        {
            string objectType = "object_type_test";
            string addressLine1 = "address_line1_test";
            string addressLine2 = "address_line2_test";
            string addressLine3 = "address_line3_test";
            string city = "city_test";
            string postalCode = "postal_code_test";
            string country = "country_test";
            string region = "region_test";
            string defaultBilling = bool.TrueString;

            var addressInfo = JObject.FromObject(new
            {
                object_type = objectType,
                address_line1 = addressLine1,
                address_line2 = addressLine2,
                address_line3 = addressLine3,
                city = city,
                postal_code = postalCode,
                country = country,
                region = region,
            });

            var pidlInfo = new Dictionary<string, object>()
            {
                { "set_as_default_billing_address", defaultBilling },
                { "addressBillingV3", addressInfo },
            };

            var info = new PXAddressV3Info(pidlInfo).GetPropertyDictionary();

            Assert.IsTrue(info.ContainsKey("object_type"), "object_type key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("address_line1"), "address_line1 key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("address_line2"), "address_line2 key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("address_line3"), "address_line3 key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("city"), "city key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("postal_code"), "postal_code key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("country"), "country key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("region"), "region key is missing from PXAddressV3Info instance");
            Assert.IsTrue(info.ContainsKey("set_as_default_billing_address"), "set_as_default_billing_address key is missing from PXAddressV3Info instance");

            Assert.IsTrue(info["object_type"].Equals(objectType), "object_type do not match");
            Assert.IsTrue(info["address_line1"].Equals(addressLine1), "address_line1 do not match");
            Assert.IsTrue(info["address_line2"].Equals(addressLine2), "address_line2 do not match");
            Assert.IsTrue(info["address_line3"].Equals(addressLine3), "address_line3 do not match");
            Assert.IsTrue(info["city"].Equals(city), "city do not match");
            Assert.IsTrue(info["postal_code"].Equals(postalCode), "postal_code do not match");
            Assert.IsTrue(info["country"].Equals(country), "country do not match");
            Assert.IsTrue(info["region"].Equals(region), "region do not match");
            Assert.IsTrue(info["set_as_default_billing_address"].Equals(bool.TrueString.ToLower()), "set_as_default_billing_address should be 'true'");
        }

        [DataRow("MSAAccount1", true, "storify")]
        [DataRow("MSAAccount1", false, "storify")]
        [DataRow("MSAAccount1", true, "xboxsubs")]
        [DataRow("MSAAccount1", false, "xboxsubs")]
        [DataRow("MSAAccount1", true, "xboxsettings")]
        [DataRow("MSAAccount1", false, "xboxsettings")]
        [DataRow("MSAAccount1", true, "saturn")]
        [DataRow("MSAAccount1", false, "saturn")]
        [TestMethod]
        public async Task PostAddress_DefaultShippingReturnsWithCorrectValues(string accountId, bool isPatchProfile, string partner)
        {
            // setup the accessor
            var accountServiceRequestMethod = HttpMethod.Put;
            var response = new
            {
                id = "test-address-id",
                country = "us",
                region = "wa",
                city = "Redmond",
                address_line1 = "1 Microsoft Way",
                postal_code = "98052",
                first_name = "Test",
                last_name = "Test111",
                phone_number = "12323211111",
                set_as_default_shipping_address = "true"
            };

            AccountProfilesV3<AccountConsumerProfileV3> userProfiles = new AccountProfilesV3<AccountConsumerProfileV3>();
            userProfiles.UserProfiles = new List<AccountConsumerProfileV3>();

            AccountConsumerProfileV3 userProfile = new AccountConsumerProfileV3()
            {
                FirstName = "Test",
                LastName = "Test111",
                ProfileType = "consumer",
                EmailAddress = "test@test.test"
            };

            userProfiles.UserProfiles.Add(userProfile);

            string matchingPostUri = $"/{accountId}/addresses";
            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(response), HttpStatusCode.OK, HttpMethod.Post, matchingPostUri);

            string matchingGetProfileUri = $"/{accountId}/profiles";
            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(userProfiles), HttpStatusCode.OK, HttpMethod.Get, matchingGetProfileUri);

            string matchingUpdateProfileUri = $"/{accountId}/profiles";
            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(userProfile), HttpStatusCode.OK, isPatchProfile ? new HttpMethod(GlobalConstants.HTTPVerbs.PATCH) : HttpMethod.Post, matchingUpdateProfileUri);

            PXSettings.AccountsService.PreProcess = (accountServiceRequest) =>
            {
                if (accountServiceRequest.RequestUri.AbsolutePath.Contains($"/profiles") && accountServiceRequest.Method != HttpMethod.Get)
                {
                    accountServiceRequestMethod = accountServiceRequest.Method;
                }
            };

            if (isPatchProfile)
            {
                PXFlightHandler.AddToEnabledFlights("UseJarvisPatchForConsumerProfile");
            }

            // Act
            var userProvidedAddress = new
            {
                country = "us",
                region = "wa",
                city = "Redmond",
                address_line1 = "1 Microsoft Way",
                postal_code = "98052",
                first_name = "Test",
                last_name = "Test111",
                phone_number = "12323211111",
                is_avs_validated = string.Empty,
                validate = string.Empty,
                set_as_default_shipping_address = "True"
            };

            string url = $"/v7.0/{accountId}/addressesEx?partner={partner}&language=en-US&avsSuggest=false";
            HttpResponseMessage result = await PXClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(userProvidedAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            if (isPatchProfile)
            {
                Assert.AreEqual(new HttpMethod(GlobalConstants.HTTPVerbs.PATCH), accountServiceRequestMethod);
            }
            else
            {
                Assert.AreEqual(HttpMethod.Put, accountServiceRequestMethod);
            }

            var patternGuid = @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}";
            string contentStr = await result.Content.ReadAsStringAsync();
            contentStr = Regex.Replace(contentStr, patternGuid, "Replaced GUID");
            JObject json = JObject.Parse(contentStr);

            var dataDescription = json["clientAction"]["context"];

            Assert.IsTrue(string.Equals(dataDescription["set_as_default_shipping_address"].ToString(), userProvidedAddress.set_as_default_shipping_address, StringComparison.OrdinalIgnoreCase), "set_as_default_shipping_address do not match");
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("MSAAccount1", "storify")]
        [DataRow("MSAAccount1", "xboxsubs")]
        [DataRow("MSAAccount1", "xboxsettings")]
        [DataRow("MSAAccount1", "saturn")]
        [TestMethod]
        public async Task PostAddress_DefaultBillingReturnsWithCorrectValues(string accountId, string partner)
        {
            // setup the accessor
            var response = new
            {
                id = "test-address-id",
                country = "us",
                region = "wa",
                city = "Redmond",
                address_line1 = "1 Microsoft Way",
                postal_code = "98052",
                first_name = "Test",
                last_name = "Test111",
                phone_number = "12323211111",
                set_as_default_billing_address = "true"
            };

            AccountProfilesV3<AccountConsumerProfileV3> userProfiles = new AccountProfilesV3<AccountConsumerProfileV3>();
            userProfiles.UserProfiles = new List<AccountConsumerProfileV3>();

            AccountConsumerProfileV3 userProfile = new AccountConsumerProfileV3()
            {
                FirstName = "Test",
                LastName = "Test111",
                ProfileType = "consumer",
                EmailAddress = "test@test.test"
            };

            userProfiles.UserProfiles.Add(userProfile);

            string matchingPostUri = $"/{accountId}/addresses";
            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(response), HttpStatusCode.OK, HttpMethod.Post, matchingPostUri);

            string matchingGetProfileUri = $"/{accountId}/profiles";
            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(userProfiles), HttpStatusCode.OK, HttpMethod.Get, matchingGetProfileUri);

            string matchingUpdateProfileUri = $"/{accountId}/profiles";
            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(userProfile), HttpStatusCode.OK, HttpMethod.Post, matchingUpdateProfileUri);

            // Act
            var userProvidedAddress = new
            {
                country = "us",
                region = "wa",
                city = "Redmond",
                address_line1 = "1 Microsoft Way",
                postal_code = "98052",
                first_name = "Test",
                last_name = "Test111",
                phone_number = "12323211111",
                is_avs_validated = string.Empty,
                validate = string.Empty,
                set_as_default_billing_address = "True"
            };

            string url = $"/v7.0/{accountId}/addressesEx?partner={partner}&language=en-US&avsSuggest=false";
            HttpResponseMessage result = await PXClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(userProvidedAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var patternGuid = @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}";
            string contentStr = await result.Content.ReadAsStringAsync();
            contentStr = Regex.Replace(contentStr, patternGuid, "Replaced GUID");
            JObject json = JObject.Parse(contentStr);

            var dataDescription = json["clientAction"]["context"];

            Assert.IsTrue(string.Equals(dataDescription["set_as_default_billing_address"].ToString(), userProvidedAddress.set_as_default_billing_address, StringComparison.OrdinalIgnoreCase), "set_as_default_billing_address do not match");
        }

        [DataRow("webblends", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestionNewAddress, 2, null)]
        [DataRow("oxowebdirect", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestionNewAddress, 2, null)]
        [DataRow("cart", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null)]
        [DataRow("webblends_inline", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null)]
        [DataRow("officeoobe", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null)]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null)]
        [DataRow("storeoffice", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null)]
        [DataRow("consumersupport", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null)]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null)]
        [DataRow("smboobe", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null)]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null)]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null)]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null)]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null)]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "PXDisablePSSCache,PXUsePartnerSettingsService", true, false)]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "PXDisablePSSCache,PXUsePartnerSettingsService", false, true)]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "PXDisablePSSCache,PXUsePartnerSettingsService")]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null)]
        [TestMethod]
        public async Task CreateAddressAvsSuggestTrue_SingleSuggestions_PatchAddress(string partner, string addressType, int expectedPageCount, string flightingOverrides, string headers = null, bool pssWithValidateInstanceAndTemplateParnter = false, bool pssWithValidateInstanceAndNonTemplateParnter = false)
        {
            string accountId = "Account001";
            bool avsSuggestEnabledAsPSSFeature = false;
            var testheaders = new Dictionary<string, string>();

            if (string.Equals(partner, "officesmb", StringComparison.OrdinalIgnoreCase))
            {
                avsSuggestEnabledAsPSSFeature = true;
                string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShippingV3\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"billing\",\"dataSource\":\"jarvisBilling\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"hapiv1SoldToIndividual\",\"dataSource\":\"hapi\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":false},{\"addressType\":\"orgAddress\",\"dataSource\":\"jarvisOrgAddress\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":false},{\"addressType\":\"hapiserviceusageaddress\",\"dataSource\":\"hapi\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":false}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShippingV3\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"billing\",\"dataSource\":\"jarvisBilling\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"hapiv1SoldToIndividual\",\"dataSource\":\"hapi\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":false},{\"addressType\":\"orgAddress\",\"dataSource\":\"jarvisOrgAddress\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":false},{\"addressType\":\"hapiserviceusageaddress\",\"dataSource\":\"hapi\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":false}]}}},\"validateinstance\":{\"template\":\"defaulttemplate\"}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // When the defaultTemplate is used as the pss template partner, avsSuggestEnabledAsPSSFeature should be set to true. 
            // This ensures that the assertion in VerifySuggestedAddressPidl, which calls the VerifySuggestionPageUsingRadioButton method, 
            // will validate the correct number of members.
            avsSuggestEnabledAsPSSFeature = (string.Equals(partner, "oxooobe", StringComparison.OrdinalIgnoreCase) && pssWithValidateInstanceAndTemplateParnter) ? true : avsSuggestEnabledAsPSSFeature;
            
            if (string.Equals(partner, "oxooobe", StringComparison.OrdinalIgnoreCase) && headers != null)
            {
                testheaders = new Dictionary<string, string>
                {
                    { "x-ms-flight", headers }
                };

                string expectedPSSResponse = "{\"default\":{\"template\":\"oxooobe\", \"resources\":null, \"features\":null}}";
                string expectedPSSResponseWithValidateInstanceAndTemplatePartenr = "{\"default\":{\"template\":\"oxooobe\", \"resources\":null, \"features\":null},\"validateinstance\":{\"template\":\"defaulttemplate\", \"resources\":null, \"features\":null}}";
                string expectedPSSResponseWithValidateInstanceAndNonTemplatePartner = "{\"default\":{\"template\":\"oxooobe\", \"resources\":null, \"features\":null},\"validateinstance\":{\"template\":\"default\", \"resources\":null, \"features\":null}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(pssWithValidateInstanceAndTemplateParnter ? expectedPSSResponseWithValidateInstanceAndTemplatePartenr : pssWithValidateInstanceAndNonTemplateParnter ? expectedPSSResponseWithValidateInstanceAndNonTemplatePartner : expectedPSSResponse);
            }

            // Arrange
            string url = $"/v7.0/{accountId}/addressesEx?partner={partner}&language=en-US&avsSuggest=true";

            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressType);

            // Act
            dynamic response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.Addresses[addressType], additionaHeaders: testheaders, flightNames: flightingOverrides);
            Assert.IsNotNull(response);
            var pidls = ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.clientAction.context));
            Assert.IsNotNull(pidls);

            AddressTestsUtil.VerifySuggestedAddressPidl(pidls, partner, expectedPageCount, avsSuggestEnabledAsPSSFeature: avsSuggestEnabledAsPSSFeature);
            DisplayHintAction action = AddressTestsUtil.VerifyUserEnteredAddressPidl(pidls, partner, expectedPageCount, flightingOverrides, null, avsSuggestEnabledAsPSSFeature: avsSuggestEnabledAsPSSFeature);
            Assert.IsNotNull(action);
            Assert.IsNotNull(action.Context);

            dynamic link = action.Context;
            
            response = await SendRequestPXServiceWithFlightOverrides(
                link.href.ToString().Replace(global::Tests.Common.Model.Pidl.Constants.SubmitUrls.PifdBaseUrl, $"/v7.0/{accountId}"),
                new HttpMethod(link.method.ToString()),
                link.payload,
                additionaHeaders: testheaders,
                flightNames: flightingOverrides);

            Assert.AreEqual("ReturnContext", response.clientAction.type.ToString());

            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();
        }

        [DataRow("webblends", AddressTestsUtil.TestSuggestedAddressType.AVSReturnsServiceUnavailable, "")]
        [DataRow("xbox", AddressTestsUtil.TestSuggestedAddressType.AVSReturnsServiceUnavailable, "")]
        [DataRow("cart", AddressTestsUtil.TestSuggestedAddressType.AVSReturnsServiceUnavailable, "")]
        [DataRow("officeoobe", AddressTestsUtil.TestSuggestedAddressType.AVSReturnsServiceUnavailable, "")]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.AVSReturnsServiceUnavailable, "")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("webblends", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("oxowebdirect", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("cart", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("payin", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("webblends_inline", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("officeoobe", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("mseg", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("onedrive", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("smboobe", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("storeoffice", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("consumersupport", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("webblends", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("oxowebdirect", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("cart", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("payin", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("webblends_inline", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("officeoobe", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("smboobe", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("mseg", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("onedrive", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("storeoffice", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("consumersupport", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("webblends", AddressTestsUtil.TestSuggestedAddressType.AVSReturnsServiceUnavailable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("xbox", AddressTestsUtil.TestSuggestedAddressType.AVSReturnsServiceUnavailable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("cart", AddressTestsUtil.TestSuggestedAddressType.AVSReturnsServiceUnavailable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("officeoobe", AddressTestsUtil.TestSuggestedAddressType.AVSReturnsServiceUnavailable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.AVSReturnsServiceUnavailable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("webblends", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("oxowebdirect", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("cart", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("payin", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("webblends_inline", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("officeoobe", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("mseg", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("onedrive", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("smboobe", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("storeoffice", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("consumersupport", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("webblends", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("oxowebdirect", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("cart", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("payin", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("webblends_inline", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("officeoobe", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("smboobe", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("mseg", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("onedrive", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("storeoffice", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("consumersupport", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [TestMethod]
        public async Task CreateAddressAvsSuggestTrue_ReturnAddress(string partner, string addressType, string flightOverrides)
        {
            string accountId = "Account001";
            string url = $"/v7.0/{accountId}/addressesEx?partner={partner}&language=en-US&avsSuggest=true";
            
            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressType);

            // Act
            dynamic response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.Addresses[addressType], null, flightOverrides);
            Assert.AreEqual(AddressTestsUtil.Addresses[addressType].id.ToString(), response.id.ToString());

            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();
        }

        [DataRow("storeoffice,webblends_inline,officeoobe,oxooobe,oxowebdirect,amcweb,setupoffice,setupofficesdx,smboobe,cart,webblends,oxowebdirect,xboxweb", AddressTestsUtil.TestSuggestedAddressType.NonUS, "")]
        [DataRow("storeoffice,webblends_inline,officeoobe,oxooobe,oxowebdirect,amcweb,setupoffice,setupofficesdx,smboobe,cart,webblends,oxowebdirect,xboxweb", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "")]
        [DataRow("storeoffice,webblends_inline,officeoobe,oxooobe,oxowebdirect,amcweb,setupoffice,setupofficesdx,smboobe,cart,webblends,oxowebdirect,xboxweb", AddressTestsUtil.TestSuggestedAddressType.NonUS, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("storeoffice,webblends_inline,officeoobe,oxooobe,oxowebdirect,amcweb,setupoffice,setupofficesdx,smboobe,cart,webblends,oxowebdirect,xboxweb", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [TestMethod]
        public async Task CreateAddressAvsSuggestTrue_ReturnAddressObject(string partners, string addressType, string flightOverrides)
        {
            string[] partnersArr = partners.Split(',');

            foreach (string partner in partnersArr)
            {
                string accountId = "Account001";
                string url = $"/v7.0/{accountId}/addressesEx?partner={partner}&language=en-US&avsSuggest=true";

                AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressType);

                // Act
                dynamic response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.Addresses[addressType], null, flightOverrides);
                Assert.AreEqual(AddressTestsUtil.Addresses[addressType].id.ToString(), response.id.ToString());

                PXSettings.AccountsService.ResetToDefaults();
                PXSettings.AddressEnrichmentService.ResetToDefaults();
            }
        }

        [DataRow("storify", AddressTestsUtil.TestSuggestedAddressType.None, 2, "", false)]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.None, 2, "", false)]
        [DataRow("xboxsettings", AddressTestsUtil.TestSuggestedAddressType.None, 2, "", false)]
        [DataRow("saturn", AddressTestsUtil.TestSuggestedAddressType.None, 2, "", false)]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.None, 2, "", false)]
        [DataRow("windowssettings", AddressTestsUtil.TestSuggestedAddressType.None, 2, "PXUsePartnerSettingsService", true)]
        [DataRow("storify", AddressTestsUtil.TestSuggestedAddressType.None, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", false)]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.None, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", false)]
        [DataRow("xboxsettings", AddressTestsUtil.TestSuggestedAddressType.None, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", false)]
        [DataRow("saturn", AddressTestsUtil.TestSuggestedAddressType.None, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", false)]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.None, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", false)]
        [DataRow("windowssettings", AddressTestsUtil.TestSuggestedAddressType.None, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment,PXUsePartnerSettingsService", true)]
        [TestMethod]
        public async Task ValidateAddress_Billing_NoSuggestions(string partner, string addressType, int expectedPageCount, string flightingOverrides, bool requiresDefaultTemplate)
        {
            string accountId = "Account001";
            string url = $"/v7.0/{accountId}/addressesEx?partner={partner}&language=en-US&avsSuggest=true";

            // Arrange
            AddressTestsUtil.SetupValidateAddressPayload(PXSettings.AddressEnrichmentService, addressType, flightingOverrides.Contains("PXUseJarvisAccountsForAddressEnrichment") ? PXSettings.AccountsService : null);
            Dictionary<string, string> headers = new Dictionary<string, string>();

            if (requiresDefaultTemplate)
            {
                headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
            }

            // Act
            dynamic response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.XboxNativeAddresses[addressType], headers, flightingOverrides);
            var pidls = ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.clientAction.context));

            if (!requiresDefaultTemplate)
            {
                AddressTestsUtil.VerifySuggestedAddressPidl(pidls, partner, expectedPageCount);
            } 
            else 
            {
                pidls = pidls as List<PIDLResource>;
                Assert.AreEqual(expectedPageCount, pidls[0].DisplayPages.Count);

                var idenity = pidls[0].Identity;
                var resourceId = idenity[Constants.DescriptionIdentityFields.ResourceId];

                Assert.AreEqual("address", idenity["description_type"], ignoreCase: true);
                Assert.AreEqual("validateInstance", idenity["operation"], ignoreCase: true);
                Assert.AreEqual("US", idenity["country"], ignoreCase: true);
                Assert.AreEqual("userEnteredOnly", resourceId);

                PageDisplayHint page = pidls[0].DisplayPages[0] as PageDisplayHint;
                GroupDisplayHint addressOptionsGroup = page.Members[2] as GroupDisplayHint;
                Assert.AreEqual(6, addressOptionsGroup.Members.Count);
                Assert.AreEqual("addressEnteredMessage", addressOptionsGroup.Members[0].HintId);
            }

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("storify", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, "")]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, "")]
        [DataRow("xboxsettings", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, "")]
        [DataRow("saturn", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, "")]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, "")]
        [DataRow("storify", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("xboxsettings", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("saturn", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment")]
        [TestMethod]
        public async Task ValidateAddress_Billing_SingleSuggestion(string partner, string addressType, int expectedPageCount, string flightingOverrides)
        {
            string accountId = "Account001";
            string url = $"/v7.0/{accountId}/addressesEx?partner={partner}&language=en-US&avsSuggest=true";

            // Arrange
            AddressTestsUtil.SetupValidateAddressPayload(PXSettings.AddressEnrichmentService, addressType, flightingOverrides.Contains("PXUseJarvisAccountsForAddressEnrichment") ? PXSettings.AccountsService : null);

            // Act
            dynamic response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.XboxNativeAddresses[addressType], null, flightingOverrides);
            var pidls = ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.clientAction.context));
            AddressTestsUtil.VerifySuggestedAddressPidl(pidls, partner, expectedPageCount);

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("storify", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "", false, false)]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "", false, false)]
        [DataRow("xboxsettings", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "", false, false)]
        [DataRow("saturn", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "", false, false)]
        [DataRow("storify", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", false, false)]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", false, false)]
        [DataRow("xboxsettings", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", false, false)]
        [DataRow("saturn", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", false, false)]
        [DataRow("storify", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "", true, false)]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "", true, false)]
        [DataRow("xboxsettings", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "", true, false)]
        [DataRow("saturn", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "", true, false)]
        [DataRow("storify", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", true, false)]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", true, false)]
        [DataRow("xboxsettings", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", true, false)]
        [DataRow("saturn", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", true, false)]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "", true, true)]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", true, true)]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "", false, true)]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, 2, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", false, true)]
        [TestMethod]
        public async Task ValidateAddress_Billing_MulitpleSuggestions(string partner, string addressType, int expectedPageCount, string flightingOverrides, bool useStyleHints, bool applyAccentBorderOnFocus)
        {
            string accountId = "Account001";
            string url = $"/v7.0/{accountId}/addressesEx?partner={partner}&language=en-US&avsSuggest=true";
            List<string> suggestAddressBackButtonStyleHints = new List<string> { "width-small" };
            List<string> suggestedAddressButtonStyleHints = new List<string> { "height-auto", "width-medium", "margin-end-small", "padding-vertical-x-small" };
            string displayTagStyleHints = Constants.DisplayTagValues.SelectionBorderGutterAccent;

            if (useStyleHints)
            {
                flightingOverrides = string.IsNullOrEmpty(flightingOverrides) ? "PXEnableXboxNativeStyleHints" : $"{flightingOverrides},PXEnableXboxNativeStyleHints";
            }

            if (applyAccentBorderOnFocus)
            {
                flightingOverrides = string.IsNullOrEmpty(flightingOverrides) ? Constants.PartnerFlightValues.ApplyAccentBorderWithGutterOnFocus : $"{flightingOverrides},{Constants.PartnerFlightValues.ApplyAccentBorderWithGutterOnFocus}";
            }

            // Arrange
            AddressTestsUtil.SetupValidateAddressPayload(PXSettings.AddressEnrichmentService, addressType, flightingOverrides.Contains("PXUseJarvisAccountsForAddressEnrichment") ? PXSettings.AccountsService : null);

            // Act
            dynamic response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.XboxNativeAddresses[addressType], null, flightingOverrides);
            List<PIDLResource> pidls = ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.clientAction.context));
            AddressTestsUtil.VerifySuggestedAddressPidl(pidls, partner, expectedPageCount);

            DisplayHint suggestAddressBackButton = pidls[0].GetDisplayHintById("suggestAddressBackButton");
            PropertyDisplayHint suggestedAddresses = pidls[0].GetDisplayHintById("suggestedAddresses") as PropertyDisplayHint;
            Assert.IsNotNull(suggestAddressBackButton);
            Assert.IsNotNull(suggestedAddresses);

            if (useStyleHints)
            {
                DisplayHint billingAddressVerifyHeader = pidls[0].GetDisplayHintById("billingAddressVerifyHeader");
                DisplayHint addressValidationMessage = pidls[0].GetDisplayHintById("addressValidationMessage");
                DisplayHint backButtonGroup = pidls[0].GetDisplayHintById("backButtonGroup");

                Assert.IsTrue(billingAddressVerifyHeader.StyleHints.SequenceEqual(new List<string> { "margin-top-medium", "margin-start-small" }));
                Assert.IsTrue(addressValidationMessage.StyleHints.SequenceEqual(new List<string> { "margin-bottom-small", "margin-start-small" }));
                Assert.IsTrue(suggestedAddresses.StyleHints.SequenceEqual(new List<string> { "layout-inline", "alignment-vertical-center", "padding-horizontal-small" }));
                Assert.IsTrue(backButtonGroup.StyleHints.SequenceEqual(new List<string> { "margin-start-small", "margin-bottom-small" }));
                Assert.IsTrue(suggestAddressBackButton.StyleHints?.SequenceEqual(suggestAddressBackButtonStyleHints));

                foreach (var item in suggestedAddresses.PossibleOptions)
                {
                    Assert.IsTrue(item.Value.StyleHints?.SequenceEqual(suggestedAddressButtonStyleHints));
                }
            }

            foreach (PIDLResource resource in pidls)
            {
                foreach (DisplayHint displayHint in resource.GetAllDisplayHints())
                {
                    ButtonDisplayHint buttonDisplayHint = displayHint as ButtonDisplayHint;
                    PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                    if (buttonDisplayHint != null)
                    {
                        Assert.AreEqual(buttonDisplayHint.DisplayTags?.ContainsKey(Constants.DisplayTagKeys.DisplayTagStyleHints) == true && buttonDisplayHint.DisplayTags[Constants.DisplayTagKeys.DisplayTagStyleHints] == displayTagStyleHints, applyAccentBorderOnFocus);
                    }
                    else if (propertyDisplayHint != null)
                    {
                        string elementType = resource.GetElementTypeByPropertyDisplayHint(propertyDisplayHint);
                        if (elementType == Constants.ElementTypes.Dropdown || elementType == Constants.ElementTypes.Textbox)
                        {
                            Assert.AreEqual(propertyDisplayHint.DisplayTags?.ContainsKey(Constants.DisplayTagKeys.DisplayTagStyleHints) == true && propertyDisplayHint.DisplayTags[Constants.DisplayTagKeys.DisplayTagStyleHints] == displayTagStyleHints, applyAccentBorderOnFocus);
                        }
                        else if (elementType == Constants.ElementTypes.ButtonList)
                        {
                            foreach (var item in propertyDisplayHint.PossibleOptions)
                            {
                                Assert.AreEqual(item.Value.DisplayTags?.ContainsKey(Constants.DisplayTagKeys.DisplayTagStyleHints) == true && item.Value.DisplayTags[Constants.DisplayTagKeys.DisplayTagStyleHints] == displayTagStyleHints, applyAccentBorderOnFocus);
                            }
                        }
                        else
                        {
                            Assert.IsTrue(propertyDisplayHint.DisplayTags == null || !propertyDisplayHint.DisplayTags.ContainsKey(Constants.DisplayTagKeys.DisplayTagStyleHints));
                        }
                    }
                    else
                    {
                        Assert.IsTrue(displayHint.DisplayTags == null || !displayHint.DisplayTags.ContainsKey(Constants.DisplayTagKeys.DisplayTagStyleHints));
                    }
                }
            }

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("storify", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", true)]
        [DataRow("storify", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", false)]
        [DataRow("xboxsettings", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "", true)]
        [DataRow("xboxsettings", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "", false)]
        [DataRow("saturn", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", true)]
        [DataRow("saturn", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "", false)]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "PXMakeAccountsAddressEnrichmentCall,PXUseJarvisAccountsForAddressEnrichment", false)]
        [DataRow("xboxsubs", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "", true)]
        [TestMethod]
        public async Task ValidateAddress_Billing_VerifyStyleHints(string partner, string addressType, string flightingOverrides, bool useStyleHints)
        {
            string accountId = "Account001";
            string url = $"/v7.0/{accountId}/addressesEx?partner={partner}&language=en-US&avsSuggest=true";

            if (useStyleHints)
            {
                flightingOverrides = string.IsNullOrEmpty(flightingOverrides) ? "PXEnableXboxNativeStyleHints" : $"{flightingOverrides},PXEnableXboxNativeStyleHints";
            }

            // Arrange
            AddressTestsUtil.SetupValidateAddressPayload(PXSettings.AddressEnrichmentService, addressType, flightingOverrides.Contains("PXUseJarvisAccountsForAddressEnrichment") ? PXSettings.AccountsService : null);

            // Act
            dynamic response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.XboxNativeAddresses[addressType], null, flightingOverrides);
            List<PIDLResource> pidls = ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.clientAction.context));

            foreach (PIDLResource pidlResource in pidls)
            {
                foreach (DisplayHint displayHint in pidlResource.GetAllDisplayHints())
                {
                    if (useStyleHints)
                    {
                        Assert.IsTrue(displayHint.StyleHints.Count > 0);
                    }
                    else
                    {
                        Assert.IsTrue(displayHint.StyleHints == null);
                    }

                    PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                    if (propertyDisplayHint != null && propertyDisplayHint.PossibleOptions != null)
                    {
                        foreach (var option in propertyDisplayHint.PossibleOptions)
                        {
                            if (option.Value.DisplayContent != null)
                            {
                                List<string> styleHints = option.Value.StyleHints?.ToList();
                                if (useStyleHints)
                                {
                                    Assert.IsTrue(styleHints.Count > 0);
                                }
                                else
                                {
                                    Assert.IsTrue(styleHints == null);
                                }
                            }
                        }
                    }
                }
            }

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("storify", "internal")]
        [DataRow("xboxsettings", "internal")]
        [DataRow("saturn", "internal")]
        [DataRow("xboxsubs", "internal")]
        [TestMethod]
        public async Task ValidateAddress_XboxNativeAddCCBillingAddress_Suggestions_StyleHints(string partner, string addressType)
        {
            string url = $"/v7.0/Addresses/ModernValidate?type={addressType}&partner={partner}&language=en-US&scenario=suggestAddressesTradeAVS&country=US";
            PXFlightHandler.AddToEnabledFlights("PXEnableXboxNativeStyleHints");

            // Arrange
            AddressTestsUtil.SetupValidateAddressPayload(PXSettings.AddressEnrichmentService, TestSuggestedAddressType.MultipleSuggestions, null);

            // Act
            dynamic response = await SendRequestPXService(url, HttpMethod.Post, XboxNativeAddresses[TestSuggestedAddressType.MultipleSuggestions]);
            var jsonResponse = response.Content.Value;
            var pidlResourceJson = jsonResponse.ClientAction.Context;
            List<PIDLResource> pidls = ReadPidlResourceFromJson(JsonConvert.SerializeObject(jsonResponse.ClientAction.Context));

            DisplayHint billingAddressVerifyPleaseVerifyHeader = pidls[0].GetDisplayHintById("billingAddressVerifyPleaseVerifyHeader");
            DisplayHint addressValidationMessage = pidls[0].GetDisplayHintById("addressValidationMessage");
            PropertyDisplayHint suggestedAddresses = pidls[0].GetDisplayHintById("suggestedAddresses") as PropertyDisplayHint;
            DisplayHint backButtonModernValidateGroup = pidls[0].GetDisplayHintById("backButtonModernValidateGroup");
            DisplayHint suggestAddressBackClosePageButton = pidls[0].GetDisplayHintById("suggestAddressBackClosePageButton");

            Assert.IsTrue(billingAddressVerifyPleaseVerifyHeader.StyleHints.SequenceEqual(new List<string> { "dummy-stylehint" }));
            Assert.IsTrue(addressValidationMessage.StyleHints.SequenceEqual(new List<string> { "margin-bottom-small" }));
            Assert.IsTrue(suggestedAddresses.StyleHints.SequenceEqual(new List<string> { "layout-inline", "alignment-vertical-center", "padding-horizontal-x-small" }));
            Assert.IsTrue(backButtonModernValidateGroup.StyleHints.SequenceEqual(new List<string> { "dummy-stylehint" }));
            Assert.IsTrue(suggestAddressBackClosePageButton.StyleHints.SequenceEqual(new List<string> { "width-small" }));

            foreach (var item in suggestedAddresses.PossibleOptions)
            {
                Assert.IsTrue(item.Value.StyleHints.SequenceEqual(new List<string> { "height-auto", "width-medium", "margin-end-small", "padding-vertical-x-small" }));
            }

            foreach (PIDLResource pidlResource in pidls)
            {
                foreach (DisplayHint displayHint in pidlResource.GetAllDisplayHints())
                {
                    Assert.IsTrue(displayHint.StyleHints.Count > 0);
                    PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                    if (propertyDisplayHint != null && propertyDisplayHint.PossibleOptions != null)
                    {
                        foreach (var option in propertyDisplayHint.PossibleOptions)
                        {
                            if (option.Value.DisplayContent != null)
                            {
                                List<string> styleHints = option.Value.StyleHints.ToList();
                                Assert.IsTrue(styleHints.Count > 0);
                            }
                        }
                    }
                }
            }

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow(Constants.PartnerNames.Storify, TestSuggestedAddressType.MultipleSuggestions, Constants.AddressTypes.Shipping)]
        [DataRow(Constants.PartnerNames.Storify, TestSuggestedAddressType.MultipleSuggestions, Constants.AddressTypes.Billing)]
        [DataRow(Constants.PartnerNames.XboxSettings, TestSuggestedAddressType.MultipleSuggestions, Constants.AddressTypes.Shipping)]
        [DataRow(Constants.PartnerNames.XboxSettings, TestSuggestedAddressType.MultipleSuggestions, Constants.AddressTypes.Billing)]
        [TestMethod]
        public async Task ValidateAddress_XboxNativeSuggestedOptionsLabels(string partner, string addressType, string scenario)
        {
            string accountId = "Account001";
            Action<PropertyDisplayHint, PIDLResource> verifyOptions = (PropertyDisplayHint suggestedAddresses, PIDLResource resource) =>
            {
                int totalOptions = suggestedAddresses.PossibleOptions.Count, position = 1;
                string optionPositionFormat = "{0} Option {1} of {2}";
                foreach (KeyValuePair<string, SelectOptionDescription> option in suggestedAddresses.PossibleOptions)
                {
                    string optionHeader = option.Key == "entered" ? "You entered" : "We suggest";
                    string accessibilityLabel = string.Format(optionPositionFormat, optionHeader, position++, totalOptions);
                    Assert.IsTrue(option.Value.AccessibilityName.StartsWith(accessibilityLabel));

                    List<DisplayHint> textDisplayHints = resource.GetAllDisplayHints(option.Value.DisplayContent).Where(hint => hint is TextDisplayHint).ToList();
                    textDisplayHints.ForEach(hint =>
                    {
                        Assert.IsTrue(hint.DisplayTags["noPidlddc"] == "pidlddc-disable-live");
                    });
                }
            };

            // Add Address Suggestions
            SetupValidateAddressPayload(PXSettings.AddressEnrichmentService, addressType);
            
            string url = $"/v7.0/{accountId}/addressesEx?partner={partner}&language=en-US&avsSuggest=true&scenario={scenario}";
            dynamic response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.XboxNativeAddresses[addressType], null, null);
            List<PIDLResource> pidls = ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.clientAction.context));
            PropertyDisplayHint suggestedAddressList = pidls[0].DisplayPages[0].Members[2] as PropertyDisplayHint;

            verifyOptions(suggestedAddressList, pidls[0]);

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();

            // Add Payment Instrument Suggestions
            SetupValidateAddressPayload(PXSettings.AddressEnrichmentService, addressType, null);

            url = $"/v7.0/Addresses/ModernValidate?type=internal&partner={partner}&language=en-US&scenario=suggestAddressesTradeAVS&country=US";
            response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.XboxNativeAddresses[addressType], null, null);
            pidls = ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.clientAction.context));
            suggestedAddressList = pidls[0].GetDisplayHintById("suggestedAddresses") as PropertyDisplayHint;

            verifyOptions(suggestedAddressList, pidls[0]);

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, "", "us", 1, "addressAVSSuggestions", "suggestAddressesTradeAVS")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "", "us", 3, "addressAVSSuggestions", "suggestAddressesTradeAVS")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.None, "", "us", 0, "addressNoAVSSuggestions", "suggestAddressesTradeAVS")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.None, "", "us", 0, "addressNoAVSSuggestions", "suggestAddressesTradeAVS")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, "", "us", 1, "addressAVSSuggestions", "suggestAddressesTradeAVS")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "", "us", 3, "addressAVSSuggestions", "suggestAddressesTradeAVS")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.None, "", "us", 0, "addressNoAVSSuggestions", "suggestAddressesTradeAVSusePidlModal")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, "", "us", 1, "addressAVSSuggestions", "suggestAddressesTradeAVSusePidlModal")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "", "us", 3, "addressAVSSuggestions", "suggestAddressesTradeAVSusePidlModal")]
        [TestMethod]
        public async Task ModernValidateWithTradeAVS(string partner, string addressType, string flightingOverrides, string country, int numberOfExpectedSuggestedAddress, string expectedResourceId, string scenario)
        {
            string url = $"/v7.0/addresses/ModernValidate?type=hapiv1billtoorganization&partner={partner}&language=en-us&scenario={scenario}&country={country}";
            if (string.Equals(partner, "amcweb", StringComparison.OrdinalIgnoreCase))
            {
                url = $"/v7.0/addresses/ModernValidate?type=shipping&partner={partner}&language=en-us&scenario={scenario}&country={country}";
            }

            bool usePidlPage = string.Equals(scenario, "suggestAddressesTradeAVS");
            string accountId = "Account001";

            // Arrange
            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressType);

            // Act
            dynamic response;
            response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.TradeAvsAddresses[addressType], null, flightingOverrides);

            // Assert
            AddressTestsUtil.VerifyTradeAVSPidlWithSuggestedAddress(response, expectedResourceId, numberOfExpectedSuggestedAddress, usePidlPage, partner);

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.None, "PXSetIsSubmitGroupFalseForTradeAVSV1", "us", 0, "addressNoAVSSuggestions", "suggestAddressesTradeAVS", true)]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, "PXSetIsSubmitGroupFalseForTradeAVSV1", "us", 1, "addressAVSSuggestions", "suggestAddressesTradeAVS", true)]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "PXSetIsSubmitGroupFalseForTradeAVSV1", "us", 3, "addressAVSSuggestions", "suggestAddressesTradeAVS", true)]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.None, "", "us", 0, "addressNoAVSSuggestions", "suggestAddressesTradeAVS", false)]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, "", "us", 1, "addressAVSSuggestions", "suggestAddressesTradeAVS", false)]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "", "us", 3, "addressAVSSuggestions", "suggestAddressesTradeAVS", false)]
        [TestMethod]
        public async Task ModernValidateWithTradeAVS_PXSetIsSubmitGroupFalseForTradeAVSV1(string partner, string addressType, string flightingOverrides, string country, int numberOfExpectedSuggestedAddress, string expectedResourceId, string scenario, bool setIsSubmitGroupFalse)
        {
            string url = $"/v7.0/addresses/ModernValidate?type=hapiv1billtoorganization&partner={partner}&language=en-us&scenario={scenario}&country={country}";
            if (string.Equals(partner, "amcweb", StringComparison.OrdinalIgnoreCase))
            {
                url = $"/v7.0/addresses/ModernValidate?type=shipping&partner={partner}&language=en-us&scenario={scenario}&country={country}";
            }

            bool usePidlPage = string.Equals(scenario, "suggestAddressesTradeAVS");
            string accountId = "Account001";

            // Arrange
            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressType);

            // Act
            dynamic response;
            response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.TradeAvsAddresses[addressType], null, flightingOverrides);

            // Assert
            // Assert
            AddressTestsUtil.VerifyTradeAVSPidlWithSuggestedAddressPXSetIsSubmitGroupFalseForTradeAVSV1(response, expectedResourceId, numberOfExpectedSuggestedAddress, usePidlPage, partner, setIsSubmitGroupFalse);

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.RegionSuggestionLV, "lv", "Riga")]
        [DataRow("commercialstores", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, "us", "Bellevue, WA")]
        [DataRow("commercialstores", AddressTestsUtil.TestSuggestedAddressType.RegionSuggestionLV, "lv", "Riga")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.RegionSuggestionVE, "ve", "Caracas, Miranda")]
        [DataRow("commercialstores", AddressTestsUtil.TestSuggestedAddressType.RegionSuggestionVE, "ve", "Caracas, Miranda")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.RegionSuggestionIN, "in", "New Delhi, NATIONAL CAPITAL TERRITORY OF DELHI")]
        [DataRow("commercialstores", AddressTestsUtil.TestSuggestedAddressType.RegionSuggestionIN, "in", "New Delhi, NATIONAL CAPITAL TERRITORY OF DELHI")]
        [TestMethod]
        public async Task ModernValidateWithTradeAVS_ValidateRegionNameEnabled(string partner, string addressType, string country, string expectedSuggestedCityRegion)
        {
            // Arrange
            string accountId = "Account001";
            string url = $"/v7.0/addresses/ModernValidate?type=shipping&partner={partner}&language=en-us&scenario=suggestAddressesTradeAVS&country={country}";

            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressType);

            // Act
            dynamic response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.TradeAvsAddresses[addressType], null, null);
            var possibleOptions = response["clientAction"]["context"][0]["displayDescription"][0]["members"][0]["members"][2]["members"][0]["possibleOptions"];
            string enteredCityRegion = possibleOptions["entered"]["displayContent"]["members"][4]["displayContent"];
            string suggestedCityRegion = possibleOptions["suggested_0"]["displayContent"]["members"][4]["displayContent"];

            // Assert
            Assert.IsNotNull(enteredCityRegion);
            Assert.IsNotNull(suggestedCityRegion);
            Assert.AreEqual(suggestedCityRegion, expectedSuggestedCityRegion);

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("commercialstores", TestSuggestedAddressType.SuggestionAO, "AO", "Lucande", null, null)]
        [DataRow("azure", TestSuggestedAddressType.SuggestionAO, "AO", "Lucande", null, null)]
        [TestMethod]
        public async Task ModernValidateWithTradeAVS_ValidateCityRegionPostalCode(string partner, string addressType, string country, string expectedSuggestedCity, string expectedSuggestedRegion, string expectedSuggestedPostalCode)
        {
            // Arrange
            string accountId = "Account001";
            string url = $"/v7.0/addresses/ModernValidate?type=shipping&partner={partner}&language=en-us&scenario=suggestAddressesTradeAVS&country={country}";

            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressType);

            // Act
            dynamic response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.TradeAvsAddresses[addressType], null, null);
            var actionMap = response["clientAction"]["context"][0]["displayDescription"][0]["members"][0]["members"][3]["members"][0]["pidlAction"]["context"]["actionMap"];
            string suggestedCity = actionMap["suggested_0"]["context"]["payload"]["city"];
            string suggestedRegion = actionMap["suggested_0"]["context"]["payload"]["region"];
            string suggestedPostalCode = actionMap["suggested_0"]["context"]["payload"]["postal_code"];

            // Assert
            Assert.AreEqual(suggestedCity, expectedSuggestedCity);
            Assert.AreEqual(suggestedRegion, expectedSuggestedRegion);
            Assert.AreEqual(suggestedPostalCode, expectedSuggestedPostalCode);

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.VerifiedCN, "cn")]
        [DataRow("commercialstore", AddressTestsUtil.TestSuggestedAddressType.VerifiedCN, "cn")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.InteractionRequiredTR, "tr")]
        [DataRow("commercialstore", AddressTestsUtil.TestSuggestedAddressType.InteractionRequiredTR, "tr")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "us")]
        [DataRow("commercialstore", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "us")]
        [TestMethod]
        public async Task ModernValidateWithTradeAVS_ValidateRegionIsoEnabled(
            string partner, 
            string addressType,  
            string country)
        {
            string url = $"/v7.0/addresses/ModernValidate?type=shipping&partner={partner}&language=en-us&country={country}";
            string accountId = "Account001";

            // Arrange
            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(AddressTestsUtil.TradeAvsAddresses[addressType]));
            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressType);

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                await AddressTestsUtil.EvaluateRegionISoEnabledHeader(request);
            };

            // Act
            dynamic response;
            response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.TradeAvsAddresses[addressType], null, null);

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.PimsService.ResetToDefaults();
        }

        /// <summary>
        /// This CIT is used to validate the secondary_validation_mode values in the request when the flight is enabled.
        /// </summary>
        /// <param name="partner">The partner name.</param>
        /// <param name="addressType">The address type.</param>
        /// <param name="country">The country code.</param>
        /// <param name="enableSecondaryValidationModeFlightValue">Flag to enable secondary validation mode flight.</param>
        /// <param name="isSeondaryValidationModeEnabledInPayload">Flag to enable secondary validation mode in payload.</param>
        /// <param name="isModernValidateWithType">Flag to enable the modern validate with type.</param>
        /// <returns></returns>
        [DataRow("commercialsupport", AddressTestsUtil.TestSuggestedAddressType.NoSuggestionTH, "th", true, true, true)]
        [DataRow("commercialsupport", AddressTestsUtil.TestSuggestedAddressType.NoSuggestionTH, "th", false, true, true)]
        [DataRow("commercialsupport", AddressTestsUtil.TestSuggestedAddressType.NoSuggestionTH, "th", true, false, true)]
        [DataRow("commercialsupport", AddressTestsUtil.TestSuggestedAddressType.NoSuggestionTH, "th", false, false, true)]
        [DataRow("commercialsupport", AddressTestsUtil.TestSuggestedAddressType.NoSuggestionTH, "th", true, true)]
        [DataRow("commercialsupport", AddressTestsUtil.TestSuggestedAddressType.NoSuggestionTH, "th", false, true)]
        [DataRow("commercialsupport", AddressTestsUtil.TestSuggestedAddressType.NoSuggestionTH, "th", true, false)]
        [DataRow("commercialsupport", AddressTestsUtil.TestSuggestedAddressType.NoSuggestionTH, "th", false, false)]
        [TestMethod]
        public async Task ModernValidateWithTradeAVS_ValidationMode(
            string partner, 
            string addressType,  
            string country,
            bool enableSecondaryValidationModeFlightValue,
            bool isSeondaryValidationModeEnabledInPayload,
            bool isModernValidateWithType = false)
        {
            string url = isModernValidateWithType ? 
                $"/v7.0/addresses/ModernValidate?type=orgAddress&partner={partner}&language=en-us&scenario=suggestAddressesTradeAVS&country={country}"
                : "/v7.0/addresses/modernValidate/";

            // Arrange
            var address = new JObject
            {
                ["original_address"] = new JObject
                {
                    ["address_line1"] = "1 Microsoft Way",
                    ["city"] = "Sathorn",
                    ["country"] = "TH",
                    ["postal_code"] = "1012",
                    ["region"] = "Bangkok",
                    ["validation_mode"] = "LegacyBusiness"
                },
                ["suggested_address"] = new JObject
                {
                    ["address_line1"] = "1 MICROSOFT WAY",
                    ["city"] = "SATHORN",
                    ["country"] = "US",
                    ["postal_code"] = "10120",
                    ["region"] = "Bangkok"
                },
                ["status"] = "Verified"
            };

            PXSettings.AccountsService.PreProcess = (request) =>
            {
                string uri = request.RequestUri.ToString();
                if (request.Method == HttpMethod.Post)
                {
                    // Check if the request content contains validation_mode
                    if (request.Content != null)
                    {
                        var contentTask = request.Content.ReadAsStringAsync();
                        contentTask.Wait();
                        string content = contentTask.Result;

                        if (content.Contains("validation_mode"))
                        {
                            // The below assert will validate the validation mode is present as payload which we are passing as userProvidedAddress and this pre-process will execute
                            // when the accounts service is called. If any of the below asserts fail, it will throw an internal server error to the request which we are validating further.
                            // Assert that the validation mode is "LegacyBusiness" when the payload is enabled.
                            Assert.IsTrue(content.Contains("validation_mode\":\"LegacyBusiness\""));

                            if (isSeondaryValidationModeEnabledInPayload && !enableSecondaryValidationModeFlightValue)
                            {
                                // Assert that the secondary validation mode is "Business" when the payload is enabled and the flight value is not enabled.
                                Assert.IsTrue(content.Contains("secondary_validation_mode\":\"Business\""));
                            }
                            else if (enableSecondaryValidationModeFlightValue)
                            {
                                // Assert that the secondary validation mode is "LegacyBusiness" when the flight value is enabled.
                                Assert.IsTrue(content.Contains("secondary_validation_mode\":\"LegacyBusiness\""));
                            }
                            else
                            {
                                // Assert that the secondary validation mode is not "LegacyBusiness" when neither condition is met.
                                Assert.IsFalse(content.Contains("secondary_validation_mode\":\"LegacyBusiness\""));
                            }
                        }
                    }
                }
            };

            PXSettings.AccountsService.ArrangeResponse(address.ToString());

            var userProvidedAddress = isSeondaryValidationModeEnabledInPayload
                ? new
                {
                    country = "th",
                    region = "Bangkok",
                    city = "SATHORN",
                    address_line1 = "One Microso",
                    postal_code = "1012",
                    validation_mode = "LegacyBusiness",
                    secondary_validation_mode = "Business"
                }
                : (object)new
                {
                    country = "th",
                    region = "Bangkok",
                    city = "SATHORN",
                    address_line1 = "One Microso",
                    postal_code = "1012",
                    validation_mode = "LegacyBusiness"
                };

            // Act
            if (enableSecondaryValidationModeFlightValue)
            {
                PXFlightHandler.AddToEnabledFlights(Constants.PartnerFlightValues.PXEnableSecondaryValidationMode);
            }

            HttpResponseMessage result = await PXClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(userProvidedAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));
            
            Assert.IsNotNull(result);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
            string contentStr = await result.Content.ReadAsStringAsync();
            JObject responseJson = JObject.Parse(contentStr);
            Assert.IsNotNull(responseJson);

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, "", "us", 1, "addressAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "", "us", 3, "addressAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.None, "", "us", 0, "addressNoAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, "", "us", 1, "addressAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "", "us", 3, "addressAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.None, "", "us", 0, "addressNoAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [TestMethod]
        public async Task ModernValidateWithTradeAVS_PidlPageV2(string partner, string addressType, string flightingOverrides, string country, int numberOfExpectedSuggestedAddress, string expectedResourceId, string scenario)
        {
            string url = $"/v7.0/addresses/ModernValidate?type=hapiv1billtoorganization&partner={partner}&language=en-us&scenario={scenario}&country={country}";
            if (string.Equals(partner, "amcweb", StringComparison.OrdinalIgnoreCase))
            {
                url = $"/v7.0/addresses/ModernValidate?type=shipping&partner={partner}&language=en-us&scenario={scenario}&country={country}";
            }

            string accountId = "Account001";

            // Arrange
            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressType);

            // set up response for modern validate
            var success = new
            {
                original_address = new
                {
                    address_line1 = "1 Microsoft Way",
                    city = "Redmond",
                    country = "US",
                    postal_code = "98052",
                    region = "WA"
                },
                suggested_address = new
                {
                    address_line1 = "1 MICROSOFT WAY",
                    city = "REDMOND",
                    country = "US",
                    postal_code = "98052-8300",
                    region = "WA"
                },
                status = "VerifiedShippable"
            };

            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(success));

            // Act
            dynamic response;
            response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.TradeAvsAddresses[addressType], null, flightingOverrides);

            // Assert
            AddressTestsUtil.VerifyTradeAVSPidlWithSuggestedAddress_PidlPageV2(response, expectedResourceId, numberOfExpectedSuggestedAddress, partner);

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, "PXDisablePSSCache", "us", 1, "addressAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "PXDisablePSSCache", "us", 3, "addressAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.None, "PXDisablePSSCache", "us", 0, "addressNoAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.VerifiedNoSuggestions, "PXDisablePSSCache", "us", 0, "addressNoAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippableNoSuggestions, "PXDisablePSSCache", "us", 0, "addressNoAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [DataRow("commercialsignup", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, "PXDisablePSSCache", "us", 1, "addressAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [DataRow("commercialsignup", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "PXDisablePSSCache", "us", 3, "addressAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [DataRow("commercialsignup", AddressTestsUtil.TestSuggestedAddressType.None, "PXDisablePSSCache", "us", 0, "addressNoAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2")]
        [TestMethod]
        public async Task ModernValidateWithTradeAVS_PidlPageV2_UsePartnerSetting(string partner, string addressType, string flightingOverrides, string country, int numberOfExpectedSuggestedAddress, string expectedResourceId, string scenario)
        {
            string url = $"/v7.0/addresses/ModernValidate?type=hapiV1&partner={partner}&language=en-us&scenario={scenario}&country={country}";
            string expectedPSSResponse = "{\"validateinstance\":{\"template\":\"defaultTemplate\",\"features\":null}}";

            if (string.Equals(partner, "commercialsignup", StringComparison.OrdinalIgnoreCase))
            {
                expectedPSSResponse = "{\"validateinstance\":{\"template\":\"defaultTemplate\",\"features\":{\"alwaysShowAVSSubmitGroup\":{\"applicableMarkets\": [],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            var headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };

            string accountId = "Account001";

            // Arrange
            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressType);

            // set up response for modern validate
            var success = new
            {
                original_address = new
                {
                    address_line1 = "1 Microsoft Way",
                    city = "Redmond",
                    country = "US",
                    postal_code = "98052",
                    region = "WA"
                },
                suggested_address = new
                {
                    address_line1 = "1 MICROSOFT WAY",
                    city = "REDMOND",
                    country = "US",
                    postal_code = "98052-8300",
                    region = "WA"
                },
                status = "VerifiedShippable"
            };

            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(success));

            // Act
            dynamic response;
            response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.TradeAvsAddresses[addressType], null, flightingOverrides);

            // Assert
            AddressTestsUtil.VerifyTradeAVSPidlWithSuggestedAddress_PidlPageV2(response, expectedResourceId, numberOfExpectedSuggestedAddress, partner);
        }

        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, "PXDisablePSSCache", "us", 1, "addressAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2", true)]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "PXDisablePSSCache", "us", 3, "addressAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2", true)]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.None, "PXDisablePSSCache", "us", 0, "addressNoAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2", true)]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.VerifiedNoSuggestions, "PXDisablePSSCache", "us", 0, "addressNoAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2", true)]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippableNoSuggestions, "PXDisablePSSCache", "us", 0, "addressNoAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2", true)]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, "PXDisablePSSCache", "us", 1, "addressAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2", false)]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, "PXDisablePSSCache", "us", 3, "addressAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2", false)]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.None, "PXDisablePSSCache", "us", 0, "addressNoAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2", false)]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.VerifiedNoSuggestions, "PXDisablePSSCache", "us", 0, "addressNoAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2", false)]
        [DataRow("officesmb", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippableNoSuggestions, "PXDisablePSSCache", "us", 0, "addressNoAVSSuggestionsV2", "suggestAddressesTradeAVSUsePidlPageV2", false)]
        [TestMethod]
        public async Task ModernValidateWithTradeAVS_PidlPageV2_SetIsSubmitGroupFalse_UsePartnerSetting(string partner, string addressType, string flightingOverrides, string country, int numberOfExpectedSuggestedAddress, string expectedResourceId, string scenario, bool setIsSubmitGroupFalse)
        {
            string url = $"/v7.0/addresses/ModernValidate?type=hapiV1&partner={partner}&language=en-us&scenario={scenario}&country={country}";
            string expectedPSSResponse = "{\"validateinstance\":{\"template\":\"defaultTemplate\",\"features\":null}}";

            if (setIsSubmitGroupFalse)
            {
                expectedPSSResponse = "{\"validateinstance\":{\"template\":\"defaulttemplate\",\"features\":{\"setIsSubmitGroupFalse\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"fieldsToSetIsSubmitGroupFalse\":[\"addressSuggestionUseThisAddressButtonWithSuggestions\",\"addressSuggestionUseThisAddressButtonWithoutSuggestions\"]}]}}}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            var headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };

            string accountId = "Account001";

            // Arrange
            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressType);

            // set up response for modern validate
            var success = new
            {
                original_address = new
                {
                    address_line1 = "1 Microsoft Way",
                    city = "Redmond",
                    country = "US",
                    postal_code = "98052",
                    region = "WA"
                },
                suggested_address = new
                {
                    address_line1 = "1 MICROSOFT WAY",
                    city = "REDMOND",
                    country = "US",
                    postal_code = "98052-8300",
                    region = "WA"
                },
                status = "VerifiedShippable"
            };

            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(success));

            // Act
            dynamic response;
            response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.TradeAvsAddresses[addressType], null, flightingOverrides);

            // Assert
            AddressTestsUtil.VerifyTradeAVSPidlWithSuggestedAddress_PidlPageV2_SetIsSubmitGroupFalse(response, expectedResourceId, numberOfExpectedSuggestedAddress, partner, setIsSubmitGroupFalse);

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("windowsstore", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, "us", "suggestAddressesTradeAVSUsePidlPageV2")]
        [TestMethod]
        public async Task ModernValidateWithTradeAVS_UsePartnerSetting(string partner, string addressType, string country, string scenario)
        {
            string url = $"/v7.0/addresses/ModernValidate?type=internal&partner={partner}&language=en-us&scenario={scenario}&country={country}";
            string expectedPSSResponse = "{\"validateInstance\":{\"template\":\"default\",\"features\":{\"verifyAddressStyling\":{\"applicableMarkets\":[]},\"addressValidation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addCCAddressValidationPidlModification\":true}]}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXDisablePSSCache,PXUsePartnerSettingsService"
                }
            };

            string accountId = "Account001";

            // Arrange
            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressType);

            // set up response for modern validate
            var success = new
            {
                original_address = new
                {
                    address_line1 = "1 Microsoft Way",
                    city = "Redmond",
                    country = "US",
                    postal_code = "98052",
                    region = "WA"
                },
                suggested_address = new
                {
                    address_line1 = "1 MICROSOFT WAY",
                    city = "REDMOND",
                    country = "US",
                    postal_code = "98052-8300",
                    region = "WA"
                },
                status = "VerifiedShippable"
            };

            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(success));

            // Act
            dynamic response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.TradeAvsAddresses[addressType], headers, null);

            // Assert         
            var editButton = response["clientAction"]["context"][0]["displayDescription"][0]["members"][0]["members"][3]["members"][0];
            string hyperlink = editButton["displayType"];
            Assert.AreEqual("hyperlink", hyperlink);

            string pidlAction = editButton["pidlAction"]["type"];
            Assert.AreEqual("closePidlPage", pidlAction);

            Assert.IsTrue(response.ToString().Contains("addressSuggestionTradeAVSV2Page"));
            Assert.IsTrue(response.ToString().Contains("Use this address"));
            Assert.IsTrue(response.ToString().Contains("suggestBlockV2"));

            string radioButton = response["clientAction"]["context"][0]["displayDescription"][0]["members"][0]["members"][5]["members"][0]["selectType"];
            Assert.AreEqual("radio", radioButton);
        }

        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, "", "us", "suggestAddressesTradeAVS")]
        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable5DigitZipCode, "", "us", "suggestAddressesTradeAVS")]
        [TestMethod]
        public async Task ModernValidateWithTradeAVS_verifiedShippable(string partner, string addressType, string flightingOverrides, string country, string scenario)
        {
            string url = $"/v7.0/addresses/ModernValidate?type=hapiv1billtoorganization&partner={partner}&language=en-us&scenario={scenario}&country={country}";
            string accountId = "Account001";

            // Arrange
            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressType);

            // Act
            dynamic response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.TradeAvsAddresses[addressType], null, flightingOverrides);

            // Assert
            string responseString = JsonConvert.SerializeObject(response);
            JObject json = JObject.Parse(responseString);
            var type = json["clientAction"]["type"];
            Assert.IsTrue(type.ToString().Equals("MergeData"));
            var paylaod = json["clientAction"]["context"]["payload"];
            Assert.IsTrue(paylaod.ToString().Contains("\"is_avs_full_validation_succeeded\": true"));

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("azure", AddressTestsUtil.TestSuggestedAddressType.None, "", "us", 0, "AddressNoAVSSuggestions", "suggestAddressesTradeAVS")]
        [TestMethod]
        public async Task ModernValidateWithTradeAVS_legacyValidateForUS(string partner, string addressType, string flightingOverrides, string country, int numberOfExpectedSuggestedAddress, string expectedResourceId, string scenario)
        {
            string url = $"/v7.0/addresses/ModernValidate?type=hapiv1billtoorganization&partner={partner}&language=en-us&scenario={scenario}&country={country}";

            PXSettings.AccountsService.ArrangeResponse(
                statusCode: HttpStatusCode.BadRequest,
                content: "{\"Code\": \"60042\", \"Object_type\": \"AddressValidation\", \"Resource_status\": \"Active\", \"Reason\": \"MultipleCitiesFound - Details: Valid State Code and City Name passed, but the city has multiple ZIP Codes.  Returned all ZIP Codes for this city.\r\n\"}");

            // Act
            dynamic response = await SendRequestPXService(url, HttpMethod.Post, AddressTestsUtil.Addresses[addressType]);

            // Assert
            AddressTestsUtil.VerifyValidationPidl(response, HttpStatusCode.BadRequest, "60042");

            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("address_line1", "InvalidStreet")]
        [DataRow("city", "InvalidCity")]
        [DataRow("postal_code", "InvalidPostalCode")]
        [DataRow("region", "InvalidRegion")]
        [DataRow("invalid_address_fields_combination", "InvalidAddressFieldsCombination")]
        [DataRow("email", "InvalidParameter")]
        [TestMethod]
        public async Task AVSAddressPost_ValidationErrorMapping(string parameterName, string expectedErrorCode)
        {
            string url = $"/v7.0/MSAAccount1/addressesEx?partner=amcweb&language=es-MX&scenario=profileaddress&avsSuggest=false";

            PXSettings.AccountsService.ArrangeResponse(
                statusCode: HttpStatusCode.BadRequest,
                content: "{\"error_code\":\"InvalidProperty\",\"message\":\"A property has invalid data.\",\"parameters\":{\"property_name\":\"" + parameterName + "\",\"details\":\"identity property is missing\"},\"object_type\":\"Error\"}");

            // Act
            dynamic response = await SendRequestPXService(url, HttpMethod.Post, AddressTestsUtil.Addresses[AddressTestsUtil.TestSuggestedAddressType.None]);

            // Assert
            AddressTestsUtil.VerifyValidationPidl(response, HttpStatusCode.BadRequest, expectedErrorCode);

            PXSettings.AccountsService.ResetToDefaults();
        }

        [TestMethod]
        [DataRow("cart", null, true)]
        [DataRow("cart", null, false)]
        [DataRow("amcweb", "profileAddress", true)]
        [DataRow("amcweb", "profileAddress", false)]
        [DataRow("amcweb", null, true)]
        [DataRow("amcweb", null, false)]
        public async Task UpdateProfileV3PassesSyncLegacyAddressFalse(string partner, string scenario, bool flightEnabled)
        {
            if (flightEnabled)
            {
                PXFlightHandler.AddToEnabledFlights("PXJarvisProfileCallSyncLegacyAddressFalse");
            }

            string url = $"/v7.0/Account001/addressesEx?partner={partner}&language=en-us&avsSuggest=false";

            if (scenario != null)
            {
                url += $"&scenario={scenario}";
            }

            var address = Addresses[TestSuggestedAddressType.SetAsDefaultBillingAddress];

            AccountProfilesV3<AccountConsumerProfileV3> userProfiles = new AccountProfilesV3<AccountConsumerProfileV3>();
            userProfiles.UserProfiles = new List<AccountConsumerProfileV3>();

            AccountConsumerProfileV3 userProfile = new AccountConsumerProfileV3()
            {
                FirstName = "Test",
                LastName = "Test111",
                ProfileType = "consumer",
                EmailAddress = "test@test.test",
                DefaultAddressId = TestSuggestedAddressType.SetAsDefaultBillingAddress
            };

            userProfiles.UserProfiles.Add(userProfile);

            bool accountsServiceUpdateProfileV3Called = false;

            PXSettings.AccountsService.PreProcess = (avsRequest) =>
            {
                string uri = avsRequest.RequestUri.ToString();
                if (avsRequest.Method == HttpMethod.Put && uri.Contains("profile"))
                {
                    accountsServiceUpdateProfileV3Called = true;

                    bool isSyncLegacyPartnerAndScenario = partner == "ambweb" && scenario == "profileAddress";

                    if (flightEnabled && !isSyncLegacyPartnerAndScenario)
                    {
                        Assert.IsTrue(uri.Contains("syncLegacyAddress=false"), "Uri should contain syncLegacyAddress=false");
                    }
                    else
                    {
                        Assert.IsFalse(uri.Contains("syncLegacyAddress=false"), "Uri should NOT contain syncLegacyAddress=false");
                    }
                }
            };

            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(address), urlPattern: ".*address.*");
            PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(userProfiles), urlPattern: ".*profile.*");

            await SendRequestPXService(url, HttpMethod.Post, address);

            Assert.IsTrue(accountsServiceUpdateProfileV3Called, "Accounts Service should have been called.");

            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("cart", "tr", TestSuggestedAddressType.NoSuggestionTR, "İstanbul,Muğla")]
        [DataRow("azure", "tr", TestSuggestedAddressType.NoSuggestionTR, "İstanbul,Muğla")]
        [DataRow("amcweb", "tr", TestSuggestedAddressType.NoSuggestionTR, "İstanbul,Muğla")]
        [DataRow("webblends", "tr", TestSuggestedAddressType.NoSuggestionTR, "İstanbul,Muğla")]
        [DataRow("commercialstores", "tr", TestSuggestedAddressType.NoSuggestionTR, "İstanbul,Muğla")]        
        [TestMethod]
        public async Task ModernValidateWithTradeAVS_ValidateRegionNameWhenAddressHasNoSuggestions(string partner, string country, string addressType, string expectedDisplayTextOfCityAndRegion)
        {
            // Arrange
            string url = $"/v7.0/addresses/ModernValidate?type=shipping&partner={partner}&country={country}&language=en-us&scenario=suggestAddressesTradeAVS";

            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, "Account001", addressType);

            // Act
            dynamic response = await SendRequestPXServiceWithFlightOverrides(url, HttpMethod.Post, AddressTestsUtil.TradeAvsAddresses[addressType], null, null);

            // Assert
            Assert.IsNotNull(response);

            string enteredCityAndRegion = response["clientAction"]["context"][0]["displayDescription"][0]["members"][0]["members"][2]["members"][0]["members"][2]["displayContent"];
            Assert.IsNotNull(enteredCityAndRegion);
            Assert.AreEqual(enteredCityAndRegion, expectedDisplayTextOfCityAndRegion);

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        private string GetValueFromGenericObject(object obj, string key)
        {
            Type t = obj.GetType();
            PropertyInfo prop = t.GetProperty(key);

            return prop.GetValue(obj).ToString();
        }
    }
}
