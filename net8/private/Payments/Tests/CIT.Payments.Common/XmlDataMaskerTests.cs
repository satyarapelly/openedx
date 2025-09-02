// <copyright company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.Payments.Common
{
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class XmlDataMaskerTests
    {
        [DataRow(null, null)]
        [DataRow("   ", "   ")]
        [DataRow(
            "<Requester xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:schemas.microsoft.com/CommercePlatform/Services/AccountService\"><IdentityProperty i:nil=\"true\" /><IdentityType>PUID</IdentityType><IdentityValue>985154368592012</IdentityValue></Requester>",
            "<Requester xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:schemas.microsoft.com/CommercePlatform/Services/AccountService\"><IdentityProperty i:nil=\"true\" /><IdentityType>PUID</IdentityType><IdentityValue>MASKED(15)</IdentityValue></Requester>")]
        [DataRow(
            "<Address xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:schemas.microsoft.com/CommercePlatform/Services/AccountService\"><AddressID>u5kTBQAAAAABAACg</AddressID><City>Vijayawada</City><CountryCode>IN</CountryCode><District /><FirstName /><FriendlyName>My Address</FriendlyName><LastName /><MapAddressResult i:nil=\"true\" /><PostalCode>520012</PostalCode><State>telangana</State><Street1>Foo Bar Street</Street1><Street2 /><Street3 /><UnitNumber /></Address>",
            "<Address xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:schemas.microsoft.com/CommercePlatform/Services/AccountService\"><AddressID>u5kTBQAAAAABAACg</AddressID><City>Vijayawada</City><CountryCode>IN</CountryCode><District /><FirstName>MASKED</FirstName><FriendlyName>MASKED(10)</FriendlyName><LastName>MASKED</LastName><MapAddressResult i:nil=\"true\" /><PostalCode>520012</PostalCode><State>telangana</State><Street1>MASKED(14)</Street1><Street2>MASKED(0)</Street2><Street3>MASKED(0)</Street3><UnitNumber>MASKED(0)</UnitNumber></Address>")]
        [DataRow(
            "<PayinInfo xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:schemas.microsoft.com/CommercePlatform/Services/AccountService\"><CompanyName>Microsoft</CompanyName><CompanyNamePronunciation>Microsoft</CompanyNamePronunciation><CorporateIdentity>1596</CorporateIdentity><CorporateLegalEntity>Microsoft Luxembourg</CorporateLegalEntity><CorporateVatId>LU11111111</CorporateVatId><CountryCode>PT</CountryCode><CreatedDate>2019-07-26T16:52:49.357</CreatedDate><Currency>EUR</Currency><CustomerType>Business</CustomerType><DefaultAddressID>HHHtxAEAAAABAACg</DefaultAddressID><Email>foobar@hotmail.com</Email><FirstName>Foo</FirstName><FirstNamePronunciation>Foo</FirstNamePronunciation><FriendlyName>WA.US</FriendlyName><HCI>YES</HCI><HCIValid>YES</HCIValid><HistoryDate>0001-01-01T00:00:00</HistoryDate><LastName>Bar</LastName><LastNamePronunciation>Bar</LastNamePronunciation><LastUpdatedDate>2019-07-26T16:53:17.827</LastUpdatedDate><Locale>en-US</Locale><PhoneSet><Phone><CountryCode>PT</CountryCode><PhoneExtension /><PhoneNumber>123123</PhoneNumber><PhonePrefix>913</PhonePrefix><PhoneType>Primary</PhoneType></Phone></PhoneSet><Status>Active</Status><TaxExemptionInfoSet i:nil=\"true\" /><Violations i:nil=\"true\" /></PayinInfo>",
            "<PayinInfo xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:schemas.microsoft.com/CommercePlatform/Services/AccountService\"><CompanyName>MASKED(9)</CompanyName><CompanyNamePronunciation>MASKED(9)</CompanyNamePronunciation><CorporateIdentity>MASKED(4)</CorporateIdentity><CorporateLegalEntity>MASKED(20)</CorporateLegalEntity><CorporateVatId>MASKED(10)</CorporateVatId><CountryCode>PT</CountryCode><CreatedDate>2019-07-26T16:52:49.357</CreatedDate><Currency>EUR</Currency><CustomerType>Business</CustomerType><DefaultAddressID>HHHtxAEAAAABAACg</DefaultAddressID><Email>fo...r(6)@hotmail.com</Email><FirstName>F...(3)</FirstName><FirstNamePronunciation>F...(3)</FirstNamePronunciation><FriendlyName>MASKED(5)</FriendlyName><HCI>MASKED(3)</HCI><HCIValid>MASKED(3)</HCIValid><HistoryDate>0001-01-01T00:00:00</HistoryDate><LastName>B...(3)</LastName><LastNamePronunciation>B...(3)</LastNamePronunciation><LastUpdatedDate>2019-07-26T16:53:17.827</LastUpdatedDate><Locale>en-US</Locale><PhoneSet><Phone><CountryCode>PT</CountryCode><PhoneExtension>MASKED(0)</PhoneExtension><PhoneNumber>1...(6)</PhoneNumber><PhonePrefix>MASKED(3)</PhonePrefix><PhoneType>Primary</PhoneType></Phone></PhoneSet><Status>Active</Status><TaxExemptionInfoSet i:nil=\"true\" /><Violations i:nil=\"true\" /></PayinInfo>")]
        [DataRow(
            "<CustomProperties><Property><Name>Birthday</Name><Namespace>Shared</Namespace><Value>1987-03-02</Value></Property><Property><Name>Nationality</Name><Namespace>Shared</Namespace><Value>SG</Value></Property></CustomProperties>",
            "<CustomProperties><Property><Name>Birthday</Name><Namespace>Shared</Namespace><Value>MASKED(10)</Value></Property><Property><Name>Nationality</Name><Namespace>Shared</Namespace><Value>MASKED(2)</Value></Property></CustomProperties>")]
        [DataTestMethod]
        public void MaskAsExpected(string input, string expectedOutput)
        {
            string actual = XmlDataMasker.Mask(input);
            Assert.AreEqual(expectedOutput, actual, "Expected and actual masked output do not match. Expected: {0}, Actual: {1}", expectedOutput, actual);
        }
    }
}
