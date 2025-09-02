// <copyright file="ClientActionFactoryTests.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class ClientActionFactoryTests : TestBase
    {
        [TestMethod]
        [DataRow(GlobalConstants.Partners.Cart, "us", "en-us")]
        public void TestAddProfileAddressClientActionToPaymentInstrument(string partner, string country, string language)
        {
            string paymentInstrumentJson = "{\"type\":null,\"id\":\"cd565cae-6c80-4ea9-9738-87d34a15afe2\",\"accountId\":\"71df3d66-e1f7-4777-9835-8271c9a61af0\",\"paymentMethod\":{\"paymentMethodType\":\"mc\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":null,\"isNonStoredPaymentMethod\":false},\"paymentMethodGroup\":\"credit_or_debit_card\",\"groupDisplayName\":\"Credit or debit card\",\"exclusionTags\":null,\"paymentMethodFamily\":\"credit_card\",\"display\":{\"name\":\"MasterCard\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_mc_rect.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_mc_rect.png\"},{\"mimeType\":\"image/svg+ xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_mc.svg\"}]},\"AdditionalDisplayText\":null},\"status\":\"Active\",\"creationDateTime\":\"2023-08-15T23:51:20.3949595Z\",\"lastUpdatedDateTime\":\"2023-08-15T23:51:20.427\",\"details\":{\"requiredChallenge\":null,\"supportedChallenge\":null,\"hashIdentity\":null,\"pendingOn\":null,\"sessionQueryUrl\":null,\"pendingDetails\":null,\"exportable\":false,\"daysUntilExpired\":null,\"accountHolderName\":\"Test\",\"accountToken\":null,\"cvvToken\":null,\"address\":{\"id\":null,\"unit_number\":null,\"address_line1\":\"555 108th Ave NE\",\"address_line2\":null,\"address_line3\":null,\"city\":\"Bellevue\",\"district\":null,\"region\":\"wa\",\"postal_code\":\"98005\",\"country\":\"US\"},\"bankIdentificationNumber\":null,\"cardType\":\"credit\",\"isIndiaExpiryGroupDeleteFlighted\":false,\"lastFourDigits\":\"0911\",\"expiryYear\":\"2024\",\"expiryMonth\":\"1\",\"email\":null,\"redirectUrl\":null,\"billingAgreementId\":null,\"firstName\":null,\"middleName\":null,\"lastName\":null,\"payerId\":null,\"billingAgreementType\":null,\"originMarket\":null,\"userName\":null,\"phone\":null,\"msisdn\":null,\"paymentAccount\":null,\"picvRequired\":false,\"bankName\":null,\"picvDetails\":null,\"bankCode\":null,\"bankAccountType\":null,\"currency\":null,\"balance\":0.0,\"lots\":null,\"appSignUrl\":null,\"companyPONumber\":null,\"defaultDisplayName\":\"Test ••0911 1/24\",\"isFullPageRedirect\":null,\"bankAccountLastFourDigits\":null,\"issuer\":null,\"isXboxCoBrandedCard\":false,\"pointsBalanceDetails\":null,\"vpa\":null},\"clientAction\":null,\"version\":null,\"links\":null}";
            string profileJson = "{\"culture\":\"en-US\",\"email_address\":\"a@b.com\",\"locale_id\":1033,\"id\":\"c61b5d9d-ba70-5fa5-bf99-479363e9656c\",\"account_id\":\"c61b5d9d-ba70-5fa5-bf99-479363e9656c\",\"etag\":\"500767224314480972\",\"customer_id\":\"71df3d66-e1f7-4777-9835-8271c9a61af0\",\"default_address_id\":\"c8d914a4-e072-5b51-5172-1c109788d005\",\"default_shipping_address_id\":null,\"country\":null,\"snapshot_id\":\"c61b5d9d-ba70-5fa5-bf99-479363e9656c/3\",\"links\":{\"self\":{\"href\":\"71df3d66-e1f7-4777-9835-8271c9a61af0/profiles/c61b5d9d-ba70-5fa5-bf99-479363e9656c\",\"method\":\"GET\"},\"snapshot\":{\"href\":\"71df3d66-e1f7-4777-9835-8271c9a61af0/profiles/c61b5d9d-ba70-5fa5-bf99-479363e9656c/3\",\"method\":\"GET\"},\"update\":{\"href\":\"71df3d66-e1f7-4777-9835-8271c9a61af0/profiles/c61b5d9d-ba70-5fa5-bf99-479363e9656c\",\"method\":\"PUT\"},\"update2\":{\"href\":\"71df3d66-e1f7-4777-9835-8271c9a61af0/profiles/c61b5d9d-ba70-5fa5-bf99-479363e9656c\",\"method\":\"PATCH\"},\"delete\":{\"href\":\"71df3d66-e1f7-4777-9835-8271c9a61af0/profiles/c61b5d9d-ba70-5fa5-bf99-479363e9656c\",\"method\":\"DELETE\"},\"default_address\":{\"href\":\"71df3d66-e1f7-4777-9835-8271c9a61af0/addresses/c8d914a4-e072-5b51-5172-1c109788d005\",\"method\":\"GET\"}},\"object_type\":\"Profile\",\"resource_status\":\"Active\",\"type\":\"consumer\",\"first_name\":\"Test\",\"last_name\":\"Test\"}";

            Microsoft.Commerce.Payments.PimsModel.V4.PaymentInstrument paymentInstrument = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PimsModel.V4.PaymentInstrument>(paymentInstrumentJson);
            Microsoft.Commerce.Payments.PimsModel.V4.AccountProfile profile = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PimsModel.V4.AccountProfile>(profileJson);

            ClientActionFactory.AddProfileAddressClientActionToPaymentInstrument(paymentInstrument, country, "billing", language, partner, false, profile, false, null, null);

            Assert.IsNotNull(paymentInstrument.ClientAction);
            Assert.AreEqual(paymentInstrument.ClientAction.ActionType.ToString(), "Pidl");

            var profileAddressPidls = paymentInstrument.ClientAction.Context as List<Microsoft.Commerce.Payments.PidlModel.V7.PIDLResource>;
            Assert.IsNotNull(profileAddressPidls);

            var addressDetailsPage = profileAddressPidls.First().DisplayPages.First();
            Assert.AreEqual(addressDetailsPage.DisplayName, "AddressDetailsPage");

            var cancelSaveGroup = addressDetailsPage.Members.First(m => m.HintId == "cancelSaveGroup") as GroupDisplayHint;
            Assert.IsNotNull(cancelSaveGroup);

            var saveAction = cancelSaveGroup.Members.First(m => m.HintId == "saveButton").Action;
            var createAddressLink = saveAction.Context as RestLink;
            var updateProfileLink = saveAction.Context2 as RestLink;

            Assert.AreEqual(createAddressLink.Href, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses");
            Assert.AreEqual(updateProfileLink.Href, "https://{pifd-endpoint}/users/{userId}/profiles/c61b5d9d-ba70-5fa5-bf99-479363e9656c/update");
        }

        [TestMethod]
        [DataRow(GlobalConstants.Partners.Cart, "us", "en-us")]
        public void TestAddProfileV3AddressClientActionToPaymentInstrument(string partner, string country, string language)
        {
            string paymentInstrumentJson = "{\"type\":null,\"id\":\"cd565cae-6c80-4ea9-9738-87d34a15afe2\",\"accountId\":\"71df3d66-e1f7-4777-9835-8271c9a61af0\",\"paymentMethod\":{\"paymentMethodType\":\"mc\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":null,\"isNonStoredPaymentMethod\":false},\"paymentMethodGroup\":\"credit_or_debit_card\",\"groupDisplayName\":\"Credit or debit card\",\"exclusionTags\":null,\"paymentMethodFamily\":\"credit_card\",\"display\":{\"name\":\"MasterCard\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_mc_rect.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_mc_rect.png\"},{\"mimeType\":\"image/svg+ xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_mc.svg\"}]},\"AdditionalDisplayText\":null},\"status\":\"Active\",\"creationDateTime\":\"2023-08-15T23:51:20.3949595Z\",\"lastUpdatedDateTime\":\"2023-08-15T23:51:20.427\",\"details\":{\"requiredChallenge\":null,\"supportedChallenge\":null,\"hashIdentity\":null,\"pendingOn\":null,\"sessionQueryUrl\":null,\"pendingDetails\":null,\"exportable\":false,\"daysUntilExpired\":null,\"accountHolderName\":\"Test\",\"accountToken\":null,\"cvvToken\":null,\"address\":{\"id\":null,\"unit_number\":null,\"address_line1\":\"555 108th Ave NE\",\"address_line2\":null,\"address_line3\":null,\"city\":\"Bellevue\",\"district\":null,\"region\":\"wa\",\"postal_code\":\"98005\",\"country\":\"US\"},\"bankIdentificationNumber\":null,\"cardType\":\"credit\",\"isIndiaExpiryGroupDeleteFlighted\":false,\"lastFourDigits\":\"0911\",\"expiryYear\":\"2024\",\"expiryMonth\":\"1\",\"email\":null,\"redirectUrl\":null,\"billingAgreementId\":null,\"firstName\":null,\"middleName\":null,\"lastName\":null,\"payerId\":null,\"billingAgreementType\":null,\"originMarket\":null,\"userName\":null,\"phone\":null,\"msisdn\":null,\"paymentAccount\":null,\"picvRequired\":false,\"bankName\":null,\"picvDetails\":null,\"bankCode\":null,\"bankAccountType\":null,\"currency\":null,\"balance\":0.0,\"lots\":null,\"appSignUrl\":null,\"companyPONumber\":null,\"defaultDisplayName\":\"Test ••0911 1/24\",\"isFullPageRedirect\":null,\"bankAccountLastFourDigits\":null,\"issuer\":null,\"isXboxCoBrandedCard\":false,\"pointsBalanceDetails\":null,\"vpa\":null},\"clientAction\":null,\"version\":null,\"links\":null}";
            string profileJson = "{\"culture\":\"en-US\",\"email_address\":\"a@b.com\",\"locale_id\":1033,\"id\":\"c61b5d9d-ba70-5fa5-bf99-479363e9656c\",\"account_id\":\"c61b5d9d-ba70-5fa5-bf99-479363e9656c\",\"etag\":\"500767224314480972\",\"customer_id\":\"71df3d66-e1f7-4777-9835-8271c9a61af0\",\"default_address_id\":\"c8d914a4-e072-5b51-5172-1c109788d005\",\"default_shipping_address_id\":null,\"country\":null,\"snapshot_id\":\"c61b5d9d-ba70-5fa5-bf99-479363e9656c/3\",\"links\":{\"self\":{\"href\":\"71df3d66-e1f7-4777-9835-8271c9a61af0/profiles/c61b5d9d-ba70-5fa5-bf99-479363e9656c\",\"method\":\"GET\"},\"snapshot\":{\"href\":\"71df3d66-e1f7-4777-9835-8271c9a61af0/profiles/c61b5d9d-ba70-5fa5-bf99-479363e9656c/3\",\"method\":\"GET\"},\"update\":{\"href\":\"71df3d66-e1f7-4777-9835-8271c9a61af0/profiles/c61b5d9d-ba70-5fa5-bf99-479363e9656c\",\"method\":\"PUT\"},\"update2\":{\"href\":\"71df3d66-e1f7-4777-9835-8271c9a61af0/profiles/c61b5d9d-ba70-5fa5-bf99-479363e9656c\",\"method\":\"PATCH\"},\"delete\":{\"href\":\"71df3d66-e1f7-4777-9835-8271c9a61af0/profiles/c61b5d9d-ba70-5fa5-bf99-479363e9656c\",\"method\":\"DELETE\"},\"default_address\":{\"href\":\"71df3d66-e1f7-4777-9835-8271c9a61af0/addresses/c8d914a4-e072-5b51-5172-1c109788d005\",\"method\":\"GET\"}},\"object_type\":\"Profile\",\"resource_status\":\"Active\",\"type\":\"consumer\",\"first_name\":\"Test\",\"last_name\":\"Test\"}";

            Microsoft.Commerce.Payments.PimsModel.V4.PaymentInstrument paymentInstrument = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PimsModel.V4.PaymentInstrument>(paymentInstrumentJson);
            Microsoft.Commerce.Payments.PimsModel.V4.AccountProfileV3 profile = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PimsModel.V4.AccountConsumerProfileV3>(profileJson);

            ClientActionFactory.AddProfileV3AddressClientActionToPaymentInstrument(paymentInstrument, country, "billing", language, partner, false, profile, null, null);

            Assert.AreEqual(paymentInstrument.ClientAction.ActionType.ToString(), "Pidl");

            var profileAddressPidls = paymentInstrument.ClientAction.Context as List<Microsoft.Commerce.Payments.PidlModel.V7.PIDLResource>;
            Assert.IsNotNull(profileAddressPidls);
            
            var addressDetailsPage = profileAddressPidls.First().DisplayPages.First();
            Assert.AreEqual(addressDetailsPage.DisplayName, "AddressDetailsPage");

            var cancelSaveGroup = addressDetailsPage.Members
                .First(m => m.HintId == "cancelSaveGroup") as GroupDisplayHint;
            Assert.IsNotNull(cancelSaveGroup);

            var saveAction = cancelSaveGroup.Members.First(m => m.HintId == "saveButton").Action;
            var createAddressLink = saveAction.Context as RestLink;
            var updateProfileLink = saveAction.Context2 as RestLink;

            Assert.AreEqual(createAddressLink.Href, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses");
            Assert.AreEqual(updateProfileLink.Href, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/c61b5d9d-ba70-5fa5-bf99-479363e9656c");

            Assert.AreEqual(updateProfileLink.Headers["etag"], profile.Etag);
            Assert.AreEqual(updateProfileLink.Headers["If-Match"], profile.Etag);
        }
    }
}
