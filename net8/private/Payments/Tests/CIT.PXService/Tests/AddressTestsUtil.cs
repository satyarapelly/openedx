namespace CIT.PXService.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using global::Tests.Common.Model;
    using global::Tests.Common.Model.Pidl;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    class AddressTestsUtil
    {
        private static Dictionary<string, dynamic> addresses = new Dictionary<string, dynamic>()
        {
            {
                TestSuggestedAddressType.ErrorObjectNotFound, new
                {
                    error_code = "ObjectNotFound",
                    message = "The client is trying to access an object that doesn’t exist.",
                    parameters = new
                    {
                        object_type = "Address"
                    },
                    city = "Bellevue",
                    object_type = "Error"
                }
            },
            {
                TestSuggestedAddressType.NonUS, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.NonUS,
                    country = "ca",
                    region = "ON",
                    city = "Ottawa",
                    address_line1 = "100 Queen St",
                    postal_code = "K1P 1A5",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.SingleSuggestion, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.SingleSuggestion,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    address_line1 = "555 110th Ave",
                    postal_code = "98004",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.RegionSuggestionLV, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.RegionSuggestionLV,
                    country = "lv",
                    city = "Riga",
                    region = "Riga",
                    address_line1 = "Rostokas iela",
                    postal_code = "9023",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.AVSReturnsServiceUnavailable, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.SingleSuggestion,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    address_line1 = "555 110th Ave",
                    postal_code = "98004",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.MultipleSuggestions, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.MultipleSuggestions,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    address_line1 = "110th Ave",
                    postal_code = "98004",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "2811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.UserConsentTrue, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.UserConsentTrue,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    address_line1 = "110th Ave",
                    postal_code = "98052",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    is_customer_consented = true,
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.SingleSuggestionNewAddress, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    address_line1 = "555 110th Ave",
                    postal_code = "98004",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.None, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.None,
                    country = "us",
                    region = "wa",
                    city = "edmond",
                    address_line1 = "1",
                    postal_code = "98052",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.VerifiedShippable, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.VerifiedShippable,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    address_line1 = "110th Ave",
                    postal_code = "98052-8300",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.VerifiedShippable5DigitZipCode, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.VerifiedShippable,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    address_line1 = "110th Ave",
                    postal_code = "98004",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.VerifiedShippableNoSuggestions, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.VerifiedShippableNoSuggestions,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    address_line1 = "110th Ave",
                    postal_code = "98052",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.VerifiedNoSuggestions, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.VerifiedNoSuggestions,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    address_line1 = "110th Ave",
                    postal_code = "98052",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.VerifiedCN, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    id = TestSuggestedAddressType.VerifiedCN,
                    address_line1 = "fengsheng hutong",
                    city = "Beijingshi",
                    country = "CN",
                    postal_code = "100032",
                    region = "BJ",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.InteractionRequiredTR, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    id = TestSuggestedAddressType.InteractionRequiredTR,
                    address_line1 = "163 Sokak",
                    city = "Bodrum",
                    country = "TR",
                    postal_code = "48400",
                    region = "48",
                    region_name = "Muğla",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.RegionSuggestionVE, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.RegionSuggestionVE,
                    country = "ve",
                    city = "Caracas",
                    region = "VE-M",
                    address_line1 = "Avenida Venezuela",
                    postal_code = "1200",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.SetAsDefaultBillingAddress, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = true,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.SetAsDefaultBillingAddress,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    address_line1 = "110th Ave",
                    postal_code = "98004",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                }
            },
            {
                TestSuggestedAddressType.RegionSuggestionIN, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.RegionSuggestionIN,
                    country = "in",
                    city = "Delhi",
                    region = "National Capital Territory of Delhi",
                    address_line1 = "S K Bole Road",
                    postal_code = "110034",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.SuggestionAO, new
                {
                    set_as_default_shipping_address = false,
                    set_as_default_billing_address = false,
                    is_user_entered = false,
                    id = TestSuggestedAddressType.SuggestionAO,
                    country = "AO",
                    city = "Luanda",
                    address_line1 = "Rua da Missao 134",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.NoSuggestionTR, new
                {
                    id = TestSuggestedAddressType.NoSuggestionTR,
                    country = "TR",
                    city = "İstanbul",
                    region = "Muğla",
                    address_line1 = "Invalid Address",
                    postal_code = "48400",
                    first_name = "Test",
                    last_name = "Test",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.NoSuggestionTH, new
                {
                    id = TestSuggestedAddressType.NoSuggestionTR,
                    country = "TH",
                    region = "Bangkok",
                    city = "Sathorn",
                    address_line1 = "Invalid Address",
                    postal_code = "10120",
                    validation_mode = "LegacyBusiness"
                }
            }
        };

        private static Dictionary<string, dynamic> tradeAvsAddresses = new Dictionary<string, dynamic>()
        {
            {
                TestSuggestedAddressType.ErrorObjectNotFound, new
                {
                    error_code = "ObjectNotFound",
                    message = "The client is trying to access an object that doesn’t exist.",
                    parameters = new
                    {
                        object_type = "Address"
                    },
                    city = "Bellevue",
                    object_type = "Error"
                }
            },
            {
                TestSuggestedAddressType.NonUS, new
                {
                    id = TestSuggestedAddressType.NonUS,
                    country = "ca",
                    region = "ON",
                    city = "Ottawa",
                    addressLine1 = "100 Queen St",
                    postalCode = "K1P 1A5",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.SingleSuggestion, new
                {
                    id = TestSuggestedAddressType.SingleSuggestion,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    addressLine1 = "555 110th Ave",
                    postalCode = "98004",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.RegionSuggestionLV, new
                {
                    id = TestSuggestedAddressType.RegionSuggestionLV,
                    country = "lv",
                    city = "Riga",
                    region = "Riga",
                    address_line1 = "Rostokas iela",
                    postal_code = "9023",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.MultipleSuggestions, new
                {
                    id = TestSuggestedAddressType.MultipleSuggestions,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    addressLine1 = "110th Ave",
                    postalCode = "98004",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "2811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.UserConsentTrue, new
                {
                    id = TestSuggestedAddressType.UserConsentTrue,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    addressLine1 = "110th Ave",
                    postalCode = "98052",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.SingleSuggestionNewAddress, new
                {
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    addressLine1 = "555 110th Ave",
                    postalCode = "98004",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.None, new
                {
                    id = TestSuggestedAddressType.None,
                    country = "us",
                    region = "wa",
                    city = "edmond",
                    addressLine1 = "1",
                    postalCode = "98052",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.VerifiedShippable, new
                {
                    id = TestSuggestedAddressType.VerifiedShippable,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    addressLine1 = "110th Ave",
                    postalCode = "98052-8300",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.VerifiedShippable5DigitZipCode, new
                {
                    id = TestSuggestedAddressType.VerifiedShippable,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    addressLine1 = "110th Ave",
                    postalCode = "98004",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.VerifiedShippableNoSuggestions, new
                {
                    id = TestSuggestedAddressType.VerifiedShippableNoSuggestions,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    addressLine1 = "110th Ave",
                    postalCode = "98052",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.VerifiedNoSuggestions, new
                {
                    id = TestSuggestedAddressType.VerifiedNoSuggestions,
                    country = "us",
                    region = "wa",
                    city = "Bellevue",
                    addressLine1 = "110th Ave",
                    postalCode = "98052",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.VerifiedCN, new
                {
                    id = TestSuggestedAddressType.VerifiedCN,
                    address_line1 = "fengsheng hutong",
                    city = "Beijingshi",
                    country = "CN",
                    postal_code = "100032",
                    region = "BJ",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.InteractionRequiredTR, new
                {
                    id = TestSuggestedAddressType.InteractionRequiredTR,
                    address_line1 = "163 Sokak",
                    city = "Bodrum",
                    country = "TR",
                    postal_code = "48400",
                    region = "48",
                    region_name = "Muğla",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "-2811701614006979299"
                }
            },
            {
                TestSuggestedAddressType.RegionSuggestionVE, new
                {
                    id = TestSuggestedAddressType.RegionSuggestionVE,
                    country = "ve",
                    city = "Caracas",
                    region = "VE-M",
                    address_line1 = "Avenida Venezuela",
                    postal_code = "1200",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.RegionSuggestionIN, new
                {
                    id = TestSuggestedAddressType.RegionSuggestionIN,
                    country = "in",
                    city = "Delhi",
                    region = "National Capital Territory of Delhi",
                    address_line1 = "S K Bole Road",
                    postal_code = "110034",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.SuggestionAO, new
                {
                    id = TestSuggestedAddressType.SuggestionAO,
                    country = "AO",
                    city = "Lucande",
                    address_line1 = "134 Rua da Missao",
                    first_name = "Test",
                    last_name = "Test111",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.NoSuggestionTR, new
                {
                    id = TestSuggestedAddressType.NoSuggestionTR,
                    country = "TR",
                    city = "İstanbul",
                    region = "Muğla",
                    address_line1 = "Invalid Address",
                    postal_code = "48400",
                    first_name = "Test",
                    last_name = "Test",
                    phone_number = "12323211111",
                    etag = "1811701614006979264"
                }
            },
            {
                TestSuggestedAddressType.NoSuggestionTH, new
                {
                    id = TestSuggestedAddressType.NoSuggestionTH,
                    country = "TH",
                    city = "Sathorn",
                    region = "Bangkok",
                    address_line1 = "Invalid Address",
                    postal_code = "10120",
                    validation_mode = "LegacyBusiness",
                }
            }
        };

        private static Dictionary<string, dynamic> xboxNativeAddresses = new Dictionary<string, dynamic>()
        {
            {
                TestSuggestedAddressType.SingleSuggestion, new
                {
                    addressBillingV3 = new
                    {
                        id = TestSuggestedAddressType.SingleSuggestion,
                        country = "us",
                        region = "wa",
                        city = "Bellevue",
                        address_line1 = "555 110th Ave",
                        postal_code = "98004",
                        first_name = "Test",
                        last_name = "Test111",
                        phone_number = "12323211111",
                        etag = "1811701614006979264",
                        addressType = "billing_v3"
                    },
                    addressCountry = "us",
                    addressType = "px_v3_billing"
                }
            },
            {
                TestSuggestedAddressType.MultipleSuggestions, new
                {
                    addressBillingV3 = new
                    {
                        id = TestSuggestedAddressType.MultipleSuggestions,
                        country = "us",
                        region = "wa",
                        city = "Bellevue",
                        address_line1 = "110th Ave",
                        postal_code = "98004",
                        first_name = "Test",
                        last_name = "Test111",
                        phone_number = "12323211111",
                        etag = "2811701614006979264",
                        addressType = "billing_v3"
                    },
                    addressCountry = "us",
                    addressType = "px_v3_billing"
                }
            },
            {
                TestSuggestedAddressType.None, new
                {
                    addressBillingV3 = new
                    {
                        id = TestSuggestedAddressType.None,
                        country = "us",
                        region = "wa",
                        city = "edmond",
                        address_line1 = "1",
                        postal_code = "98052",
                        first_name = "Test",
                        last_name = "Test111",
                        phone_number = "12323211111",
                        etag = "-2811701614006979299",
                        addressType = "billing_v3"
                    },
                    addressCountry = "us",
                    addressType = "px_v3_billing"
                }
            },
        };

        public class TestSuggestedAddressType
        {
            public const string MultipleSuggestions = "MulitpleSuggestions";
            public const string SingleSuggestion = "SingleSuggestion";
            public const string NotValidate = "NotValidate";
            public const string NonUS = "NonUS";
            public const string ErrorObjectNotFound = "ErrorObjectNotFound";
            public const string UserConsentTrue = "UserConsentTrue";
            public const string SingleSuggestionNewAddress = "SingleSuggestionNewAddress";
            public const string None = "None";
            public const string VerifiedShippable = "VerifiedShippable";
            public const string VerifiedShippable5DigitZipCode = "VerifiedShippable5DigitZipCode";
            public const string AVSReturnsServiceUnavailable = "AVSReturnsServiceUnavailable";
            public const string VerifiedCN = "VerifiedCN";
            public const string InteractionRequiredTR = "InteractionRequiredTR";
            public const string RegionSuggestionLV = "RegionSuggestionLV";            
            public const string RegionSuggestionVE = "RegionSuggestionVE";
            public const string RegionSuggestionIN = "RegionSuggestionIN";
            public const string SetAsDefaultBillingAddress = "SetAsDefaultBillingAddress";
            public const string SuggestionAO = "SuggestionAO";
            public const string NoSuggestionTR = "NoSuggestionTR";
            public const string NoSuggestionTH = "NoSuggestionTH";
            public const string VerifiedNoSuggestions = "VerifiedNoSuggestions";
            public const string VerifiedShippableNoSuggestions = "VerifiedShippableNoSuggestions";            
        }

        public class StorifyType
        {
            public const string None = "None";
            public const string MulitpleSuggestions = "MulitpleSuggestions";
            public const string SingleSuggestion = "SingleSuggestion";
        }

        public class TestListAddressType
        {
            public const string MultipleAddresses = "MulitpleSuggestions";
            public const string SingleAddress = "SingleSuggestion";
            public const string None = "None";
        }

        public class TestValidateAddress
        {
            public const string None = "None";
            public const string MultipleSuggestions = "MulitpleSuggestions";
            public const string SingleSuggestion = "SingleSuggestion";
        }

        public static Dictionary<string, dynamic> Addresses
        {
            get
            {
                return addresses;
            }
        }

        public static Dictionary<string, dynamic> XboxNativeAddresses
        {
            get
            {
                return xboxNativeAddresses;
            }
        }

        public static Dictionary<string, dynamic> TradeAvsAddresses
        {
            get
            {
                return tradeAvsAddresses;
            }
        }

        public static void SetupListAddressPayload(SelfHostedPXServiceCore.Mocks.AccountService accountService, string accountId, string testType)
        {
            var listAddresses = new Dictionary<string, dynamic>()
            {
                { TestListAddressType.None, new
                    {
                        Items = new List<object>()
                    }
                },
                { TestListAddressType.SingleAddress, new
                    {
                        Items = new List<object>()
                        {
                            new
                            {
                                Id = "67890",
                                country = "US",
                                region = "WA",
                                city = "Redmond",
                                address_line1 = "1 Microsoft Way",
                                postal_code = "98052-8300",
                                first_name = "first",
                                last_name = "last",
                            }
                        }
                    }
                },
                { TestListAddressType.MultipleAddresses, new
                    {
                        Items = new List<object>()
                        {
                            new
                            {
                                Id = "12345",
                                country = "US",
                                region = "WA",
                                city = "Bellevue",
                                address_line1 = "555 110th Ave N",
                                postal_code = "98004-5124",
                                first_name = "test",
                                last_name = "name"
                            },
                            new
                            {
                                Id = "67890",
                                country = "US",
                                region = "WA",
                                city = "Redmond",
                                address_line1 = "1 Microsoft Way",
                                postal_code = "98052-8300"
                            }
                        }
                    }
                }
            };

            var testProfile = new
            {
                items = new List<object>()
                {
                    new
                    {
                        object_type = "Profile",
                        default_address_id = "67890",
                        default_shipping_address_id = "12345",
                        customer_id = "123",
                        resource_status = "Active",
                        type = "consumer",
                        links = new { },
                        id = "123",
                        etag = "123",
                        country = "US",
                        snapshot_id = "123"
                    }
                }
            };

            // Get Addresses
            string matchingUri = $"/{accountId}/addresses";
            accountService.ArrangeResponse(
                JsonConvert.SerializeObject(listAddresses[testType]),
                HttpStatusCode.OK,
                HttpMethod.Get,
                matchingUri);

            // Get Profile
            string profileUri = $"/{accountId}/profiles";
            accountService.ArrangeResponse(
                JsonConvert.SerializeObject(testProfile),
                HttpStatusCode.OK,
                HttpMethod.Get,
                profileUri);
        }

        public static void SetupSuggestAddressPayload(SelfHostedPXServiceCore.Mocks.AccountService accountService, SelfHostedPXServiceCore.Mocks.AddressEnrichmentService addressEnrichmentService, string accountId, string addressId)
        {
            // Arrange
            // "One Microso" is done on purpose, as we are testing the AVS will suggest the correct spelling.
            var suggestedAddresses = new Dictionary<string, dynamic>()
            {
                { TestSuggestedAddressType.NonUS, null },
                { TestSuggestedAddressType.UserConsentTrue, null },
                { TestSuggestedAddressType.ErrorObjectNotFound, null },
                { TestSuggestedAddressType.AVSReturnsServiceUnavailable, null },
                { TestSuggestedAddressType.SingleSuggestion, new
                    {
                        status = "InteractionRequired",
                        original_address = new
                        {
                             country = "US",
                             region = "WA",
                             city = "Bellevue",
                             address_line1 = "555 110th Ave NE",
                             postal_code = "98004",
                        },
                        suggested_addresses = new List<object>()
                        {
                            new
                            {
                                mailability_score = "4",
                                result_percentage = "90",
                                address_type = "H",
                                address = new
                                {
                                    country = "US",
                                    region = "WA",
                                    city = "Bellevue",
                                    address_line1 = "555 110th Ave N",
                                    postal_code = "98004-5124",
                                    latitude = "47.615521",
                                    longitude = "-122.193621"
                                }
                            }
                        }
                    }
                },
                { 
                    TestSuggestedAddressType.RegionSuggestionLV, new
                    {
                        status = "InteractionRequired",
                        original_address = new
                        {
                             country = "LV",
                             city = "Riga",
                             region = "Riga",
                             address_line1 = "Rostokas iela",
                             postal_code = "9023"
                        },
                        suggested_address = new
                        {
                            country = "LV",
                            region = "Riga",
                            city = "Riga",
                            address_line1 = "14A Rostokas iela",
                            postal_code = "1029"
                        }
                    }
                },
                { TestSuggestedAddressType.SingleSuggestionNewAddress, new
                    {
                        status = "InteractionRequired",
                        original_address = new
                        {
                             country = "US",
                             region = "WA",
                             city = "Bellevue",
                             address_line1 = "555 110th Ave NE",
                             postal_code = "98004",
                        },
                        suggested_addresses = new List<object>()
                        {
                            new
                            {
                                mailability_score = "4",
                                result_percentage = "90",
                                address_type = "H",
                                address = new
                                {
                                    country = "US",
                                    region = "WA",
                                    city = "Bellevue",
                                    address_line1 = "555 110th Ave N",
                                    postal_code = "98004-5124",
                                    latitude = "47.615521",
                                    longitude = "-122.193621"
                                }
                            }
                        }
                    }
                },
                { TestSuggestedAddressType.MultipleSuggestions, new
                    {
                        status = "StreetPartial",
                        original_address = new
                        {
                             country = "us",
                             region = "wa",
                             city = "Bellevue",
                             address_line1 = "110th Ave",
                             postal_code = "98052",
                        },
                        suggested_addresses = new List<object>()
                        {
                            new
                            {
                                mailability_score = "1",
                                result_percentage = "85",
                                address_type = "S",
                                address = new
                                {
                                    country = "US",
                                    region = "WA",
                                    city = "Bellevue",
                                    address_line1 = "100 110th Ave NE",
                                    postal_code = "98004-5124",
                                    latitude = "47.615521",
                                    longitude = "-122.193621"
                                }
                            },
                            new
                            {
                                mailability_score = "1",
                                result_percentage = "85",
                                address_type = "S",
                                address = new
                                {
                                    country = "US",
                                    region = "WA",
                                    city = "Bellevue",
                                    address_line1 = "101 110th Ave NE",
                                    postal_code = "98004-5124",
                                    latitude = "47.615521",
                                    longitude = "-122.193621"
                                }
                            },
                            new
                            {
                                mailability_score = "1",
                                result_percentage = "85",
                                address_type = "S",
                                address = new
                                {
                                    country = "US",
                                    region = "WA",
                                    city = "Bellevue",
                                    address_line1 = "102 110th Ave NE",
                                    postal_code = "98004-5124",
                                    latitude = "47.615521",
                                    longitude = "-122.193621"
                                }
                            }
                        }
                    }
                },
                {
                    TestSuggestedAddressType.None, new
                    {
                        original_address = new
                        {
                            country = "US",
                            region = "WA",
                            city = "Redmond",
                            address_line1 = "1",
                            postal_code = "98052"
                        },
                        status = "None",
                        validation_message = "Address field invalid for property: 'PostalCode', 'City', 'Province'"
                    }
                },
                {
                     TestSuggestedAddressType.VerifiedShippable, new
                    {
                        original_address = new
                        {
                            country = "us",
                            region = "wa",
                            city = "Bellevue",
                            address_line1 = "555 110th Ave NE",
                            postal_code = "98052-8300",
                        },
                        suggested_addresses = new List<object>()
                        {
                            new
                            {
                                mailability_score = "4",
                                result_percentage = "90",
                                address_type = "H",
                                address = new
                                {
                                    country = "US",
                                    region = "WA",
                                    city = "Bellevue",
                                    address_line1 = "555 110th Ave NE",
                                    postal_code = "98004-5124",
                                    latitude = "47.615521",
                                    longitude = "-122.193621"
                                }
                            }
                        },
                        status = "VerifiedShippable"
                    }
                },
                {
                     TestSuggestedAddressType.VerifiedShippable5DigitZipCode, new
                    {
                        original_address = new
                        {
                            country = "us",
                            region = "wa",
                            city = "Bellevue",
                            address_line1 = "555 110th Ave NE",
                            postal_code = "98004",
                        },
                        suggested_address = new
                        {
                            country = "US",
                            region = "WA",
                            city = "Bellevue",
                            address_line1 = "555 110th Ave NE",
                            postal_code = "98004-5124",
                            latitude = "47.615521",
                            longitude = "-122.193621",
                            province2 = "King"
                        },
                        status = "VerifiedShippable"
                    }
                },
                {
                    TestSuggestedAddressType.VerifiedShippableNoSuggestions, new
                    {
                        original_address = new
                        {
                            country = "us",
                            region = "wa",
                            city = "Bellevue",
                            address_line1 = "555 110th Ave NE",
                            postal_code = "98052",
                        },
                        status = "VerifiedShippable"
                    }
                },
                {
                    TestSuggestedAddressType.VerifiedNoSuggestions, new
                    {
                        original_address = new
                        {
                            country = "us",
                            region = "wa",
                            city = "Bellevue",
                            address_line1 = "555 110th Ave NE",
                            postal_code = "98052",
                        },
                        status = "Verified"
                    }
                },
                {
                    TestSuggestedAddressType.VerifiedCN, new
                    {
                        original_address = new
                        {
                            country = "cn",
                            region = "bj",
                            city = "beijingshi",
                            address_line1 = "fenzi hutong",
                            postal_code = "100032",
                        },
                        status = "Verified"
                    }
                },
                {
                    TestSuggestedAddressType.InteractionRequiredTR, new
                    {
                        suggested_address = new
                        {
                            country = "TR",
                            region = "48",
                            region_name = "Muğla",
                            city = "Bodrum",
                            address_line1 = "163. Sokak",
                            postal_code = "48400",
                            latitude = "37.103317",
                            longitude = "27.318062"
                        },
                        original_address = new
                        {
                            country = "TR",
                            region = "10",
                            city = "Bodrum",
                            address_line1 = "163 Sokak",
                            postal_code = "48400"
                        },
                        status = "InteractionRequired"
                    }
                },
                {
                    TestSuggestedAddressType.RegionSuggestionVE, new
                    {
                        status = "StreetPartial",
                        original_address = new
                        {
                            country = "VE",
                            city = "Caracas",
                            region = "VE-M",
                            address_line1 = "Avenida Venezuela",
                            postal_code = "1200"
                        },
                        suggested_address = new
                        {
                            country = "VE",
                            city = "Caracas",
                            region = "M",
                            address_line1 = "Avenida Venezuela",
                            postal_code = "1201",
                            latitude = "10.338998",
                            longitude = "-67.044215"
                        }
                    }
                },
                {
                    TestSuggestedAddressType.RegionSuggestionIN, new
                    {
                        status = "InteractionRequired",
                        original_address = new
                        {
                            country = "IN",
                            city = "Delhi",
                            region = "National Capital Territory of Delhi",
                            address_line1 = "S K Bole Road",
                            postal_code = "110034"
                        },
                        suggested_address = new
                        {
                            country = "IN",
                            city = "New Delhi",
                            region = "Delhi",
                            address_line1 = "S K Bole Road",
                            postal_code = "110002",
                            latitude = "10.338998",
                            longitude = "-67.044215"
                        }
                    }
                },
                {
                    TestSuggestedAddressType.SuggestionAO, new
                    {
                        status = "InteractionRequired",
                        original_address = new
                        {
                            country = "AO",
                            city = "Luanda",
                            address_line1 = "Rua da Missao 134",
                        },
                        suggested_address = new
                        {
                            country = "AO",
                            city = "Lucande",
                            address_line1 = "134 Rua da Missao",
                        }
                    }
                },
                {
                    TestSuggestedAddressType.NoSuggestionTR, new
                    {
                        original_address = new
                        {
                            country = "TR",
                            region = "Muğla",
                            city = "İstanbul",
                            address_line1 = "Invalid Address",
                            postal_code = "48400"
                        },
                        status = "None"
                    }
                },
                {
                    TestSuggestedAddressType.NoSuggestionTH, new
                    {
                        original_address = new
                        {
                            country = "TH",
                            region = "Bangkok",
                            city = "Sathorn",
                            address_line1 = "Invalid Address",
                            postal_code = "10120",
                            validation_mode = "LegacyBusiness"
                        },
                        status = "None"
                    }
                }
            };

            addressEnrichmentService.ArrangeResponse(JsonConvert.SerializeObject(suggestedAddresses[addressId]), HttpStatusCode.OK, HttpMethod.Post, "/addresses/lookup");
            SetupValidateAddressPayload(addressEnrichmentService, addressId);

            // Get Address
            string matchingUri = $"/{accountId}/addresses/{addressId}";
            accountService.ArrangeResponse(
                JsonConvert.SerializeObject(Addresses[addressId]),
                addressId != TestSuggestedAddressType.ErrorObjectNotFound ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                HttpMethod.Get,
                matchingUri);

            // Patch Address
            matchingUri = $"/{accountId}/addresses/{addressId}";
            accountService.ArrangeResponse(
                JsonConvert.SerializeObject(Addresses[addressId]),
                addressId != TestSuggestedAddressType.ErrorObjectNotFound ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                new HttpMethod("PATCH"),
                matchingUri);

            // Post Address
            matchingUri = $"/{accountId}/addresses";
            accountService.ArrangeResponse(
                JsonConvert.SerializeObject(Addresses[addressId]),
                addressId != TestSuggestedAddressType.ErrorObjectNotFound ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                HttpMethod.Post,
                matchingUri);
        }

        public static void SetupValidateAddressPayload(SelfHostedPXServiceCore.Mocks.AddressEnrichmentService addressEnrichmentService, string addressId, SelfHostedPXServiceCore.Mocks.AccountService accountService = null)
        {
            // Arrange
            var suggestedAddresses = new Dictionary<string, dynamic>()
            {
                { TestSuggestedAddressType.NonUS, null },
                { TestSuggestedAddressType.UserConsentTrue, null },
                { TestSuggestedAddressType.ErrorObjectNotFound, null },
                { TestSuggestedAddressType.AVSReturnsServiceUnavailable, null },
                {
                    TestSuggestedAddressType.None, new
                    {
                        original_address = new
                        {
                            country = "us",
                            region = "wa",
                            city = "asdf",
                            address_line1 = "asdf",
                            postal_code = "11111"
                        },
                        status = "None",
                        validation_message = "Address field invalid for property: 'PostalCode', 'City', 'Province'"
                    }
                },
                {
                    TestSuggestedAddressType.VerifiedShippable, new
                    {
                         original_address = new
                        {
                            country = "us",
                            region = "wa",
                            city = "Bellevue",
                            address_line1 = "555 110th Ave NE",
                            postal_code = "98052-8300",
                        },
                        suggested_address = new
                        {
                            country = "US",
                            region = "WA",
                            city = "Bellevue",
                            address_line1 = "555 110th Ave NE",
                            postal_code = "98004-5124",
                            latitude = "47.615521",
                            longitude = "-122.193621",
                            province2 = "King"
                        },
                        status = "VerifiedShippable"
                    }
                },
                {
                    TestSuggestedAddressType.VerifiedShippable5DigitZipCode, new
                    {
                        original_address = new
                        {
                            country = "us",
                            region = "wa",
                            city = "Bellevue",
                            address_line1 = "555 110th Ave NE",
                            postal_code = "98004",
                        },
                        suggested_address = new
                        {
                            country = "US",
                            region = "WA",
                            city = "Bellevue",
                            address_line1 = "555 110th Ave NE",
                            postal_code = "98004-5124",
                            latitude = "47.615521",
                            longitude = "-122.193621",
                            province2 = "King"
                        },
                        status = "VerifiedShippable"
                    }
                },
                {
                    TestSuggestedAddressType.VerifiedShippableNoSuggestions, new
                    {
                        original_address = new
                        {
                            country = "us",
                            region = "wa",
                            city = "Bellevue",
                            address_line1 = "555 110th Ave NE",
                            postal_code = "98052",
                        },
                        status = "VerifiedShippable"
                    }
                },
                {
                    TestSuggestedAddressType.VerifiedNoSuggestions, new
                    {
                        original_address = new
                        {
                            country = "us",
                            region = "wa",
                            city = "Bellevue",
                            address_line1 = "555 110th Ave NE",
                            postal_code = "98052",
                        },
                        status = "Verified"
                    }
                },
                {
                    TestSuggestedAddressType.SingleSuggestion, new
                    {
                        status = "InteractionRequired",
                        original_address = new
                        {
                             country = "US",
                             region = "WA",
                             city = "Bellevue",
                             address_line1 = "555 110th Ave NE",
                             postal_code = "98005",
                        },
                        suggested_address = new
                        {
                            country = "US",
                            region = "WA",
                            city = "Bellevue",
                            address_line1 = "555 110th Ave N",
                            postal_code = "98004-5124",
                            latitude = "47.615521",
                            longitude = "-122.193621",
                            province2 = "King"
                        },
                        validation_message = "Address field invalid for property: 'PostalCode'"
                    }
                },
                {
                    TestSuggestedAddressType.RegionSuggestionLV, new
                    {
                        status = "InteractionRequired",
                        original_address = new
                        {
                             country = "LV",
                             city = "Riga",
                             region = "Riga",
                             address_line1 = "Rostokas iela",
                             postal_code = "9023",
                        },
                        suggested_address = new
                        {
                            country = "LV",
                            region = "Riga",
                            city = "Riga",
                            address_line1 = "14A Rostokas iela",
                            postal_code = "1029"
                        }
                    }
                },
                {
                    TestSuggestedAddressType.SingleSuggestionNewAddress, new
                    {
                        status = "InteractionRequired",
                        original_address = new
                        {
                             country = "US",
                             region = "WA",
                             city = "Bellevue",
                             address_line1 = "555 110th Ave NE",
                             postal_code = "98005",
                        },
                        suggested_address = new
                        {
                            country = "US",
                            region = "WA",
                            city = "Bellevue",
                            address_line1 = "555 110th Ave N",
                            postal_code = "98004-5124",
                            latitude = "47.615521",
                            longitude = "-122.193621",
                            province2 = "King"
                        },
                        validation_message = "Address field invalid for property: 'PostalCode'"
                    }
                },
                {
                    TestSuggestedAddressType.MultipleSuggestions, new
                    {
                        status = "StreetPartial",
                        original_address = new
                        {
                             country = "us",
                             region = "wa",
                             city = "Bellevue",
                             address_line1 = "110th Ave NE",
                             postal_code = "98004",
                        },
                        suggested_addresses = new List<object>()
                        {
                            new
                            {
                                country = "US",
                                region = "WA",
                                city = "Bellevue",
                                address_line1 = "100 110th Ave NE",
                                postal_code = "98004-5809",
                                latitude = "47.610574",
                                longitude = "-122.193621"
                            },
                            new
                            {
                                country = "US",
                                region = "WA",
                                city = "Bellevue",
                                address_line1 = "101 110th Ave NE",
                                postal_code = "98004-5804",
                                latitude = "47.610853",
                                longitude = "-122.193596"
                            },
                            new
                            {
                                country = "US",
                                region = "WA",
                                city = "Bellevue",
                                address_line1 = "102 110th Ave NE",
                                postal_code = "98004-5804",
                                latitude = "47.611133",
                                longitude = "-122.193617"
                            }
                        }
                    }
                },
                {
                    TestSuggestedAddressType.VerifiedCN, new
                    {
                        id = TestSuggestedAddressType.VerifiedCN,
                        address_line1 = "fengsheng hutong",
                        city = "Beijingshi",
                        country = "CN",
                        postal_code = "100032",
                        region = "BJ",
                        first_name = "Test",
                        last_name = "Test111",
                        phone_number = "12323211111",
                        etag = "-2811701614006979299"
                    }
                },
                {
                    TestSuggestedAddressType.InteractionRequiredTR, new
                    {
                        id = TestSuggestedAddressType.InteractionRequiredTR,
                        address_line1 = "163 Sokak",
                        city = "Bodrum",
                        country = "TR",
                        postal_code = "48400",
                        region = "48",
                        region_name = "Muğla",
                        first_name = "Test",
                        last_name = "Test111",
                        phone_number = "12323211111",
                        etag = "-2811701614006979299"
                    }
                },
                {
                    TestSuggestedAddressType.RegionSuggestionVE, new
                    {
                        status = "StreetPartial",
                        original_address = new
                        {
                            country = "VE",
                            city = "Caracas",
                            region = "VE-M",
                            address_line1 = "Avenida Venezuela",
                            postal_code = "1200"
                        },
                        suggested_address = new
                        {
                            country = "VE",
                            city = "Caracas",
                            region = "M",
                            region_name = "Miranda",
                            address_line1 = "Avenida Venezuela",
                            postal_code = "1201",
                            latitude = "10.338998",
                            longitude = "-67.044215"
                        }
                    }
                },
                {
                    TestSuggestedAddressType.RegionSuggestionIN, new
                    {
                        status = "InteractionRequired",
                        original_address = new
                        {
                            country = "IN",
                            city = "Delhi",
                            region = "National Capital Territory of Delhi",
                            address_line1 = "S K Bole Road",
                            postal_code = "110034"
                        },
                        suggested_address = new
                        {
                            country = "IN",
                            city = "New Delhi",
                            region = "Delhi",
                            address_line1 = "S K Bole Road",
                            postal_code = "110002",
                            latitude = "10.338998",
                            longitude = "-67.044215"
                        }
                    }
                },
                {
                    TestSuggestedAddressType.SuggestionAO, new
                    {
                        status = "InteractionRequired",
                        original_address = new
                        {
                            country = "AO",
                            city = "Luanda",
                            address_line1 = "Rua da Missao 134",
                        },
                        suggested_address = new
                        {
                            country = "AO",
                            city = "Lucande",
                            address_line1 = "134 Rua da Missao",
                        }
                    }
                },
                {
                    TestSuggestedAddressType.NoSuggestionTR, new
                    {
                        original_address = new
                        {
                            country = "TR",
                            region = "Muğla",
                            city = "İstanbul",
                            address_line1 = "Invalid Address",
                            postal_code = "48400"
                        },
                        status = "None"
                    }
                },
                {
                    TestSuggestedAddressType.NoSuggestionTH, new
                    {
                        original_address = new
                        {
                            country = "TH",
                            region = "Bangkok",
                            city = "Sathorn",
                            address_line1 = "Invalid Address",
                            postal_code = "10120",
                            validation_mode = "LegacyBusiness"
                        },
                        status = "None"
                    }
                }
            };

            if (accountService != null)
            {
                accountService.ArrangeResponse(
                    JsonConvert.SerializeObject(suggestedAddresses[addressId]),
                    addressId != TestSuggestedAddressType.AVSReturnsServiceUnavailable ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable,
                    HttpMethod.Post,
                    "/addresses/validate");
            }

            addressEnrichmentService.ArrangeResponse(
                JsonConvert.SerializeObject(suggestedAddresses[addressId]),
                addressId != TestSuggestedAddressType.AVSReturnsServiceUnavailable ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable,
                HttpMethod.Post,
                "/addresses/validate");
        }

        public static DisplayHintAction VerifyUserEnteredAddressPidl(
            List<PIDLResource> resources,
            string partner,
            int pageCount = 1,
            string flightOverrides = null,
            string addressId = null,
            string scenario = null,
            bool avsSuggestEnabledAsPSSFeature = false)
        {
            var resource = resources
                    .Where(r => r.Identity[Constants.DescriptionIdentityFields.ResourceId] == "userEntered")
                    .FirstOrDefault();
            var idenity = resource.Identity;
            var resourceId = idenity[Constants.DescriptionIdentityFields.ResourceId];
            Assert.AreEqual("address", idenity["description_type"], ignoreCase: true);
            Assert.AreEqual("validateInstance", idenity["operation"], ignoreCase: true);
            Assert.AreEqual("US", idenity["country"], ignoreCase: true);
            var idDescription = JsonConvert.DeserializeObject<PropertyDescription>(JsonConvert.SerializeObject(resource.DataDescription["id"]));
            Assert.AreEqual(resources.Count, idDescription.PossibleValues.Count);
            if (addressId != null)
            {
                Assert.AreEqual($"^{addressId}$", idDescription.Validation.Regex);
            }
            else
            {
                Assert.AreEqual("^entered$", idDescription.Validation.Regex);
            }

            Assert.AreEqual(pageCount, resource.DisplayPages.Count);

            // verify submit button here.
            return VerifyUserEnteredPageUsingRadioButton(
                    resource.DisplayPages[0],
                    addressId,
                    partner,
                    flightOverrides,
                    scenario,
                    avsSuggestEnabledAsPSSFeature);
        }

        public static void VerifySuggestedAddressPidl(List<PIDLResource> resources, string partner, int pageCount = 1, string scenario = null, int buttonsOnPage = 1, bool avsSuggestEnabledAsPSSFeature = false)
        {
            // Verify the suggested pidl
            int i = 0;
            resources
                .Where(r => r.Identity[Constants.DescriptionIdentityFields.ResourceId].Contains("suggested"))
                .ToList()
                .ForEach(resource =>
                {
                    var idenity = resource.Identity;
                    var resourceId = idenity[Constants.DescriptionIdentityFields.ResourceId];

                    Assert.AreEqual("address", idenity["description_type"], ignoreCase: true);
                    Assert.AreEqual("validateInstance", idenity["operation"], ignoreCase: true);
                    Assert.AreEqual("US", idenity["country"], ignoreCase: true);
                    Assert.AreEqual($"suggested_{i}", resourceId);

                    // Verify data description
                    var idDescription = JsonConvert.DeserializeObject<PropertyDescription>(JsonConvert.SerializeObject(resource.DataDescription["id"]));
                    Assert.AreEqual(resources.Count, idDescription.PossibleValues.Count);
                    Assert.AreEqual($"^suggested_{i}$", idDescription.Validation.Regex);

                    // Verify display description
                    Assert.AreEqual(pageCount, resource.DisplayPages.Count);
                    VerifySuggestionPageUsingRadioButton(resource.DisplayPages[0], i, partner, scenario, buttonsOnPage, avsSuggestEnabledAsPSSFeature);

                    if (pageCount == 2)
                    {
                        VerifyAddressChangePage(resource.DisplayPages[1], partner, scenario, avsSuggestEnabledAsPSSFeature);
                    }

                    i++;
                });
        }

        public static DisplayHintAction VerifyUserEnteredPageUsingRadioButton(PageDisplayHint page, string addressId, string partner, string flightOverrides, string scenario, bool avsSuggestEnabledAsPSSFeature = false)
        {
            // submit option
            int buttonGroupMembersCount = avsSuggestEnabledAsPSSFeature ? 5 : 3;
            var buttonGroup = page.Members[buttonGroupMembersCount] as ContainerDisplayHint;
            Assert.AreEqual(1, buttonGroup.Members.Count); // only one button
            var button = buttonGroup.Members[0];
            Assert.AreEqual(true, button.IsHighlighted);
            Assert.AreEqual(null, button.IsBack);

            var action = button.Action;
            if (addressId != null)
            {
                // Validate existing address with patch address flighting on, patch address with If-Match
                RestLink restLink = new RestLink();
                restLink.Href = AppendParameterScenario($"{Constants.SubmitUrls.PifdAddressPostUrlTemplate}/{addressId}?partner={partner}&language=en-US", scenario);
                restLink.Payload = new
                {
                    is_customer_consented = true,
                };

                restLink.AddHeader(
                    Constants.CustomHeaders.IfMatch,
                    AddressTestsUtil.Addresses
                    .Where(pair => pair.Key != TestSuggestedAddressType.ErrorObjectNotFound && pair.Value.id == addressId)
                    .Select(pair => pair.Value.etag)
                    .FirstOrDefault());

                restLink.Method = "PATCH";
                var expectedAction = JsonConvert.SerializeObject(
                    new DisplayHintAction(Constants.ActionType.RestAction, true, restLink, null),
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                var actualAction = JsonConvert.SerializeObject(
                    action,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                Assert.AreEqual(expectedAction, actualAction);
            }
            else if (addressId == null)
            {
                // Validate new address, post address action expected with is_user_consented as true.
                Assert.AreEqual("restAction", button.Action.ActionType);
                var restLink = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(button.Action.Context));
                var expectUrl = AppendParameterScenario($"https://{{pifd-endpoint}}/users/{{userId}}/addressesEx?partner={partner}&language=en-US&avsSuggest=False", scenario);
                Assert.AreEqual(expectUrl, restLink.Href);
                Assert.AreEqual("POST", restLink.Method);
                dynamic enteredAddress = restLink.Payload;
                string postalCode = enteredAddress.postal_code;
                Assert.AreEqual(5, postalCode.Length);
                Assert.AreEqual(true, bool.Parse(enteredAddress.is_customer_consented.ToString()));
            }

            return action;
        }

        public static void VerifyModernValidationErrorStrings(string pidlContent)
        {
            string[] modernValidateErrorCodes = new string[]
            {
                "InteractionRequired",
                "None",
                "StreetPartial",
                "PremisesPartial",
                "Multiple",
                "InvalidStreet",
                "InvalidCityRegionPostalCode"
            };

            foreach (string modernValidateErrorCode in modernValidateErrorCodes)
            {
                Assert.IsTrue(pidlContent.Contains(modernValidateErrorCode), $"{modernValidateErrorCode} is missing from Error strings");
            }
        }

        public static void VerifySuggestionPageUsingRadioButton(PageDisplayHint page, int pidlIndex, string partner, string scenario = null, int buttonsOnPage = 1, bool avsSuggestEnabledAsPSSFeature = false)
        {
            // address options
            var options = page.Members[2] as ContainerDisplayHint;
            int optionMembersCount = avsSuggestEnabledAsPSSFeature ? 6 : 2;
            Assert.AreEqual(optionMembersCount, options.Members.Count);
            var suggestedOption = avsSuggestEnabledAsPSSFeature ? page.Members[4] as PropertyDisplayHint : options.Members[pidlIndex + 1] as PropertyDisplayHint;
            var zipCodeText = suggestedOption.PossibleOptions[$"suggested_{pidlIndex}"].DisplayContent.Members[4] as TextDisplayHint;
            Assert.AreEqual(10, zipCodeText.DisplayContent.Length); // zip code xxxxx-xxxx expected

            // submit option
            var buttonGroup = avsSuggestEnabledAsPSSFeature ? page.Members[5] as ContainerDisplayHint : page.Members[3] as ContainerDisplayHint;
            Assert.AreEqual(buttonsOnPage, buttonGroup.Members.Count); // only one button
            var button = buttonGroup.Members.Last();
            Assert.AreEqual(true, button.IsHighlighted);
            Assert.AreEqual(null, button.IsBack);
            Assert.AreEqual("restAction", button.Action.ActionType);
            var restLink = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(button.Action.Context));
            var expectedUrl = AppendParameterScenario($"https://{{pifd-endpoint}}/users/{{userId}}/addressesEx?partner={partner}&language=en-US&avsSuggest=False", scenario);
            Assert.AreEqual(expectedUrl, restLink.Href);

            Assert.AreEqual("POST", restLink.Method);
            dynamic suggestedAddress = restLink.Payload;
            string postalCode = suggestedAddress.postal_code;
            Assert.AreEqual(10, postalCode.Length);
        }

        public static void VerifyAddressChangePage(PageDisplayHint page, string partner, string scenario, bool avsSuggestEnabledAsPSSFeature = false)
        {
            // 1. address fields
            int addressMembersCount = avsSuggestEnabledAsPSSFeature ? 1 : 2;
            var address = page.Members[addressMembersCount] as ContainerDisplayHint;
            Assert.AreEqual(6, address.Members.Count);
            Assert.AreEqual("addressLine1", address.Members[0].HintId);
            Assert.AreEqual("addressLine2", address.Members[1].HintId);
            Assert.AreEqual("addressCity", address.Members[2].HintId);
            Assert.AreEqual("addressState", address.Members[3].HintId);
            Assert.AreEqual("addressPostalCode", address.Members[4].HintId);
            Assert.AreEqual("addressCountry", address.Members[5].HintId);

            // 2. submit option
            int buttonGroupMembersCount = avsSuggestEnabledAsPSSFeature ? 3 : 4;
            var buttonGroup = page.Members[buttonGroupMembersCount] as ContainerDisplayHint;
            Assert.AreEqual(partner == "xbox" ? 3 : 2, buttonGroup.Members.Count); // only one button

            // 2.1. back button
            var backbutton = buttonGroup.Members[0];
            Assert.AreEqual("addressBackButton", backbutton.HintId);
            Assert.AreEqual(null, backbutton.IsHighlighted);
            Assert.AreEqual(true, backbutton.IsBack);
            Assert.AreEqual(partner == "xbox" ? "gohome" : "movePrevious", backbutton.Action.ActionType);

            // 2.2. save button
            var saveButton = buttonGroup.Members[1];
            Assert.AreEqual("saveButton", saveButton.HintId);
            Assert.AreEqual(true, saveButton.IsHighlighted);
            Assert.AreEqual(null, saveButton.IsBack);
            Assert.AreEqual("submit", saveButton.Action.ActionType);
            var restLink = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(saveButton.Action.Context));
            var expectedUrl = AppendParameterScenario($"https://{{pifd-endpoint}}/users/{{userId}}/addressesEx?partner={partner}&language=en-US&avsSuggest=True", scenario);
            Assert.AreEqual(expectedUrl, restLink.Href);
            Assert.AreEqual("POST", restLink.Method);
            Assert.AreEqual(null, restLink.Payload);
        }

        public static void VerifySuggestedAddressPidlUsingButtonList(List<PIDLResource> resources)
        {
            // Verify the suggested pidl
            for (int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                var idenity = resource.Identity;
                if (!idenity["resource_id"].Contains("suggested"))
                {
                    continue;
                }

                Assert.AreEqual("address", idenity["description_type"], ignoreCase: true);
                Assert.AreEqual("validateInstance", idenity["operation"], ignoreCase: true);
                Assert.AreEqual("US", idenity["country"], ignoreCase: true);
                Assert.AreEqual($"suggested_{i}", idenity["resource_id"]);

                // Verify data description
                var idDescription = JsonConvert.DeserializeObject<PropertyDescription>(JsonConvert.SerializeObject(resource.DataDescription["id"]));
                Assert.AreEqual(resources.Count, idDescription.PossibleValues.Count);
                Assert.AreEqual($"^suggested_{i}$", idDescription.Validation.Regex);

                // Verify display description
                Assert.AreEqual(2, resource.DisplayPages.Count); // one page for now and will add one more by 10/14
                var page1 = resource.DisplayPages[0];

                // address options
                var options = page1.Members[2] as ContainerDisplayHint;
                Assert.AreEqual(2, options.Members.Count);
                var suggestedOption = options.Members[0] as PropertyDisplayHint;
                var zipCodeText = suggestedOption.PossibleOptions[$"suggested_{i}"].DisplayContent.Members[4] as TextDisplayHint;
                Assert.AreEqual(10, zipCodeText.DisplayContent.Length); // zip code xxxxx-xxxx expected

                // back button
                var buttonGroup = page1.Members[3] as ContainerDisplayHint;
                Assert.AreEqual(2, buttonGroup.Members.Count); // only one button
                var button = buttonGroup.Members[0];
                Assert.AreEqual("moveNext", button.Action.ActionType);
            }
        }

        public static void VerifyUserEnteredAddressOnlyPage(PageDisplayHint page, string addressId, string partner, string flightOverrides, string scenario)
        {
            // 1.1 Heading
            Assert.AreEqual("addressSuggestionHeading", page.Members[0].HintId);

            // 1.2 Subheading
            Assert.AreEqual("addressSuggestionMessage", page.Members[1].HintId);

            // 1.3 Address details
            var address = page.Members[2] as ContainerDisplayHint;
            Assert.AreEqual("addressEnteredOnlyGroup", address.HintId);
            Assert.AreEqual(4, address.Members.Count);
            Assert.AreEqual("addressEnteredOnlyLine1", address.Members[0].HintId);
            Assert.AreEqual("addressEnteredOnlyLine2", address.Members[1].HintId);
            Assert.AreEqual("addressEnteredOnlyCityRegion", address.Members[2].HintId);
            Assert.AreEqual("addressEnteredOnlyPostalCode", address.Members[3].HintId);

            // 1.4 Change Link
            if (partner != "xbox")
            {
                var changeButtonGroup = page.Members[3] as ContainerDisplayHint;
                Assert.AreEqual("addressChangeGroup", changeButtonGroup.HintId);
                Assert.AreEqual(1, changeButtonGroup.Members.Count);
                var changeButton = changeButtonGroup.Members[0];
                Assert.AreEqual("addressChangeButton", changeButton.HintId);
                Assert.AreEqual("moveNext", changeButton.Action.ActionType);

                // 1.5 Submit button for non xbox
                var saveButtonGroup = page.Members[4] as ContainerDisplayHint;
                Assert.AreEqual(1, saveButtonGroup.Members.Count); // only one button
                var button = saveButtonGroup.Members[0];
                VerifySubmitButton(button, addressId, partner, flightOverrides, scenario);
            }
            else
            {
                var changeButtonGroup = page.Members[3] as ContainerDisplayHint;
                Assert.AreEqual("addressBackToEditSaveGroup", changeButtonGroup.HintId);
                Assert.AreEqual(2, changeButtonGroup.Members.Count);
                var changeButton = changeButtonGroup.Members[0];
                Assert.AreEqual("addressBackToEditButton", changeButton.HintId);
                Assert.AreEqual("moveNext", changeButton.Action.ActionType);

                // 1.5 Submit button for non xbox
                var saveButtonGroup = page.Members[3] as ContainerDisplayHint;
                Assert.AreEqual(2, saveButtonGroup.Members.Count); // only one button
                var button = saveButtonGroup.Members[1];
                VerifySubmitButton(button, addressId, partner, flightOverrides, scenario);
            }
        }

        public static void VerifySubmitButton(DisplayHint button, string addressId, string partner, string flightOverrides, string scenario)
        {
            Assert.AreEqual(true, button.IsHighlighted);
            Assert.AreEqual(null, button.IsBack);
            Assert.AreEqual("restAction", button.Action.ActionType);
            var restLink = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(button.Action.Context));
            var expectedUrl = AppendParameterScenario($"https://{{pifd-endpoint}}/users/{{userId}}/addressesEx/{addressId}?partner={partner}&language=en-US", scenario);
            Assert.AreEqual(expectedUrl, restLink.Href);
            Assert.AreEqual("PATCH", restLink.Method);
        }

        public static string AppendParameterScenario(string url, string scenario)
        {
            if (!string.IsNullOrEmpty(scenario))
            {
                url += $"&scenario={scenario}";
            }

            return url;
        }

        public static void VerifyTradeAVSPidlWithSuggestedAddress(dynamic response, string resourceId, int numberOfSuggestedAddress, bool usePidlPage, string partner)
        {
            var pidls = TestBase.ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.clientAction.context));

            JObject jsonClientAction = JObject.Parse(JsonConvert.SerializeObject(response.clientAction));
            if (usePidlPage)
            {
                Assert.AreEqual("PidlPage", jsonClientAction.SelectToken("type").ToString(), ignoreCase: true);
            }
            else
            {
                Assert.AreEqual("PidlModal", jsonClientAction.SelectToken("type").ToString(), ignoreCase: true);
            }

            var idenity = pidls[0].Identity;
            Assert.AreEqual("address", idenity["description_type"], ignoreCase: true);
            Assert.AreEqual("validateInstance", idenity["operation"], ignoreCase: true);
            Assert.AreEqual("US", idenity["country"], ignoreCase: true);
            Assert.AreEqual(resourceId, idenity["resource_id"], ignoreCase: true);

            var displayPage = pidls[0].DisplayPages[0] as PageDisplayHint;
            var modalGroup = displayPage.Members[0] as GroupDisplayHint;
            bool isModalGroup = modalGroup.IsModalGroup ?? false;
            Assert.IsTrue(usePidlPage ? isModalGroup == false : isModalGroup == true);

            if (numberOfSuggestedAddress > 0)
            {
                var addressOptionsTradeAVSGroup = modalGroup.Members[2] as GroupDisplayHint;
                var addressSuggestedTradeAVS = addressOptionsTradeAVSGroup.Members[0] as PropertyDisplayHint;
                Assert.AreEqual(addressSuggestedTradeAVS.PossibleOptions.Count, numberOfSuggestedAddress + 1);
            }

            var addressUseCloseGroup = modalGroup.Members[3] as GroupDisplayHint;
            var addressSuggestionHeading = modalGroup.Members[0] as DisplayHint;
            var userThisAddressButton = addressUseCloseGroup.Members[0] as DisplayHint;
            if (string.Equals(partner, "amcweb", System.StringComparison.OrdinalIgnoreCase))
            {
                Assert.IsTrue(userThisAddressButton.DisplayTags.Values.Contains("full-width"));
                Assert.IsTrue(addressUseCloseGroup.DisplayTags.Values.Contains("full-width"));
                Assert.IsTrue(addressUseCloseGroup.DisplayTags.Values.Contains("absolute-bottom"));
                Assert.IsTrue((bool)addressSuggestionHeading.IsHidden);
            }

            var userThisAddressButtonAction = addressUseCloseGroup.Members[0].Action as DisplayHintAction;
            if (numberOfSuggestedAddress == 0)
            {
                Assert.AreEqual(userThisAddressButtonAction.ActionType.ToString(), "mergeData");
                JObject nextActionJson = JObject.Parse(JsonConvert.SerializeObject(userThisAddressButtonAction.NextAction));
                if (usePidlPage)
                {
                    Assert.AreEqual(nextActionJson["type"].ToString(), "closePidlPage");
                }
                else
                {
                    Assert.AreEqual(nextActionJson["type"].ToString(), "closeModalDialog");
                }

                JObject thirdActionJson = JObject.Parse(JsonConvert.SerializeObject(nextActionJson["nextAction"]));
                Assert.AreEqual(thirdActionJson["type"].ToString(), "continueSuspendedAction");
                Assert.IsTrue(userThisAddressButtonAction.Context.ToString().Contains("\"is_customer_consented\": true"));
            }
            else
            {
                Assert.AreEqual(userThisAddressButtonAction.ActionType.ToString(), "propertyBindingAction");
                JObject userThisAddressButtonActionContextJson = JObject.Parse(JsonConvert.SerializeObject(userThisAddressButtonAction.Context));
                var bindingPropertyName = userThisAddressButtonActionContextJson["bindingPropertyName"];
                Assert.AreEqual(bindingPropertyName.ToString(), "address_suggest_id");

                JObject actionMap = JObject.Parse(JsonConvert.SerializeObject(userThisAddressButtonActionContextJson["actionMap"]));
                var values = actionMap.ToObject<Dictionary<string, object>>();

                foreach (KeyValuePair<string, object> entry in values)
                {
                    JObject actionJson = JObject.Parse(JsonConvert.SerializeObject(entry.Value));
                    Assert.AreEqual(actionJson["type"].ToString(), "mergeData");
                    Assert.IsNotNull(actionJson["context"]["payload"]);
                    JObject nextActionJson = JObject.Parse(JsonConvert.SerializeObject(actionJson["nextAction"]));
                    if (usePidlPage)
                    {
                        Assert.AreEqual(nextActionJson["type"].ToString(), "closePidlPage");
                    }
                    else
                    {
                        Assert.AreEqual(nextActionJson["type"].ToString(), "closeModalDialog");
                    }

                    JObject thirdActionJson = JObject.Parse(JsonConvert.SerializeObject(nextActionJson["nextAction"]));
                    Assert.AreEqual(thirdActionJson["type"].ToString(), "continueSuspendedAction");

                    if (string.Equals("entered", entry.Key))
                    {
                        Assert.IsTrue(actionJson["context"]["payload"].ToString().Contains("\"is_customer_consented\": true"));
                    }
                    else
                    {
                        Assert.IsTrue(actionJson["context"]["payload"].ToString().Contains("\"is_avs_full_validation_succeeded\": true"));
                    }
                }
            }
        }

        public static void VerifyTradeAVSPidlWithSuggestedAddressPXSetIsSubmitGroupFalseForTradeAVSV1(dynamic response, string resourceId, int numberOfSuggestedAddress, bool usePidlPage, string partner, bool setIsSubmitGroupFalse)
        {
            var pidls = TestBase.ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.clientAction.context));

            JObject jsonClientAction = JObject.Parse(JsonConvert.SerializeObject(response.clientAction));

            var displayPage = pidls[0].DisplayPages[0] as PageDisplayHint;
            var modalGroup = displayPage.Members[0] as GroupDisplayHint;

            if (numberOfSuggestedAddress > 0)
            {
                var addressOptionsTradeAVSGroup = modalGroup.Members[2] as GroupDisplayHint;
                var addressChangeTradeAVSGroup = addressOptionsTradeAVSGroup.Members[1] as GroupDisplayHint;
                Assert.AreEqual(!setIsSubmitGroupFalse, addressChangeTradeAVSGroup.IsSumbitGroup);
            }
            else
            {
                var addressUseEnteredGroup = modalGroup.Members[3] as GroupDisplayHint;
                Assert.AreEqual(!setIsSubmitGroupFalse, addressUseEnteredGroup.IsSumbitGroup);
            }

            var addressUseCloseGroup = modalGroup.Members[3] as GroupDisplayHint;
            Assert.AreEqual(!setIsSubmitGroupFalse, addressUseCloseGroup.IsSumbitGroup);
        }

        public static void VerifyTradeAVSPidlWithSuggestedAddress_PidlPageV2(dynamic response, string resourceId, int numberOfSuggestedAddress, string partner)
        {
            var pidls = TestBase.ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.clientAction.context));

            JObject jsonClientAction = JObject.Parse(JsonConvert.SerializeObject(response.clientAction));
            Assert.AreEqual("PidlPage", jsonClientAction.SelectToken("type").ToString(), ignoreCase: true);

            var idenity = pidls[0].Identity;
            Assert.AreEqual("address", idenity["description_type"], ignoreCase: true);
            Assert.AreEqual("validateInstance", idenity["operation"], ignoreCase: true);
            Assert.AreEqual("US", idenity["country"], ignoreCase: true);
            Assert.AreEqual(resourceId, idenity["resource_id"], ignoreCase: true);

            var displayPage = pidls[0].DisplayPages[0] as PageDisplayHint;
            var modalGroup = displayPage.Members[0] as GroupDisplayHint;
            bool isModalGroup = modalGroup.IsModalGroup ?? false;
            Assert.IsFalse(isModalGroup);

            if (numberOfSuggestedAddress > 0)
            {
                var addressOptionsTradeAVSGroup = modalGroup.Members[5] as GroupDisplayHint;
                var addressSuggestedTradeAVS = addressOptionsTradeAVSGroup.Members[0] as PropertyDisplayHint;
                Assert.AreEqual(addressSuggestedTradeAVS.PossibleOptions.Count, numberOfSuggestedAddress + 1);

                // user entered option should be the last in the options for V2 UX
                int enteredPosition = addressSuggestedTradeAVS.PossibleOptions.Keys.ToList().IndexOf("entered");
                Assert.AreEqual(enteredPosition, addressSuggestedTradeAVS.PossibleOptions.Count - 1);

                var userEnteredOptionDisplayhint = addressSuggestedTradeAVS.PossibleOptions["entered"].DisplayContent.Members[0] as TextDisplayHint;
                Assert.AreEqual(userEnteredOptionDisplayhint.DisplayContent, "No thanks, keep the address as I've entered it");
            }

            var addressUseCloseGroup = string.Equals(partner, "commercialsignup") ? modalGroup.Members.Last() as GroupDisplayHint : displayPage.Members[1] as GroupDisplayHint;
            var addressSuggestionHeading = modalGroup.Members[0] as DisplayHint;
            var userThisAddressButton = addressUseCloseGroup.Members[0] as DisplayHint;
            if (string.Equals(partner, "amcweb", System.StringComparison.OrdinalIgnoreCase))
            {
                Assert.IsTrue(userThisAddressButton.DisplayTags.Values.Contains("full-width"));
                Assert.IsTrue(addressUseCloseGroup.DisplayTags.Values.Contains("full-width"));
                Assert.IsTrue(addressUseCloseGroup.DisplayTags.Values.Contains("absolute-bottom"));
                Assert.IsTrue((bool)addressSuggestionHeading.IsHidden);
            }

            var userThisAddressButtonAction = addressUseCloseGroup.Members[0].Action as DisplayHintAction;
            if (numberOfSuggestedAddress == 0)
            {
                Assert.AreEqual(userThisAddressButtonAction.ActionType.ToString(), "mergeData");
                JObject nextActionJson = JObject.Parse(JsonConvert.SerializeObject(userThisAddressButtonAction.NextAction));
                JObject thirdActionJson = JObject.Parse(JsonConvert.SerializeObject(nextActionJson["nextAction"]));
                Assert.AreEqual(thirdActionJson["type"].ToString(), "continueSuspendedAction");
                Assert.IsTrue(userThisAddressButtonAction.Context.ToString().Contains("\"is_customer_consented\": true"));
            }
            else
            {
                Assert.AreEqual(userThisAddressButtonAction.ActionType.ToString(), "propertyBindingAction");
                JObject userThisAddressButtonActionContextJson = JObject.Parse(JsonConvert.SerializeObject(userThisAddressButtonAction.Context));
                var bindingPropertyName = userThisAddressButtonActionContextJson["bindingPropertyName"];
                Assert.AreEqual(bindingPropertyName.ToString(), "address_suggest_id");

                JObject actionMap = JObject.Parse(JsonConvert.SerializeObject(userThisAddressButtonActionContextJson["actionMap"]));
                var values = actionMap.ToObject<Dictionary<string, object>>();

                foreach (KeyValuePair<string, object> entry in values)
                {
                    JObject actionJson = JObject.Parse(JsonConvert.SerializeObject(entry.Value));

                    Assert.AreEqual(actionJson["type"].ToString(), "mergeData");
                    Assert.IsNotNull(actionJson["context"]["payload"]);
                    JObject nextActionJson = JObject.Parse(JsonConvert.SerializeObject(actionJson["nextAction"]));
                    Assert.AreEqual(nextActionJson["type"].ToString(), "closePidlPage");

                    JObject thirdActionJson = JObject.Parse(JsonConvert.SerializeObject(nextActionJson["nextAction"]));
                    Assert.AreEqual(thirdActionJson["type"].ToString(), "continueSuspendedAction");

                    if (string.Equals("entered", entry.Key))
                    {
                        Assert.IsTrue(actionJson["context"]["payload"].ToString().Contains("\"is_customer_consented\": true"));
                    }
                    else
                    {
                        Assert.IsTrue(actionJson["context"]["payload"].ToString().Contains("\"is_avs_full_validation_succeeded\": true"));
                    }
                }
            }
        }

        public static void VerifyTradeAVSPidlWithSuggestedAddress_PidlPageV2_SetIsSubmitGroupFalse(dynamic response, string resourceId, int numberOfSuggestedAddress, string partner, bool setIsSubmitGroupFalse)
        {
            var pidls = TestBase.ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.clientAction.context));

            JObject jsonClientAction = JObject.Parse(JsonConvert.SerializeObject(response.clientAction));

            var displayPage = pidls[0].DisplayPages[0] as PageDisplayHint;
            var modalGroup = displayPage.Members[0] as GroupDisplayHint;

            var useThisAddressButtonGroup = displayPage.Members[1] as GroupDisplayHint;

            Assert.AreEqual(!setIsSubmitGroupFalse, useThisAddressButtonGroup.IsSumbitGroup);
        }

        public async static void VerifyValidationPidl(dynamic response, HttpStatusCode expectedStatusCode, string expectedErrorCode = null)
        {
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            if (expectedErrorCode != null)
            {
                string contentStr = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(contentStr);
                JToken code = json.SelectToken("ErrorCode");
                Assert.IsNotNull(code);
                Assert.AreEqual(expectedErrorCode, code.Value<string>());
            }
        }

        public static async Task EvaluateRegionISoEnabledHeader(HttpRequestMessage request)
        {
            IEnumerable<string> countriesEnabledRegionISO = new List<string>
            {
                "cn"
            };

            IEnumerable<string> regionIsoEnabled;
            string requestContent = await request.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(requestContent);
            string country = json.SelectToken("country").ToString();
            request.Headers.TryGetValues(Constants.CustomHeaders.RegionIsoEnabled, out regionIsoEnabled);

            if (countriesEnabledRegionISO.Contains(country))
            {
                Assert.IsNotNull(regionIsoEnabled);
                Assert.IsTrue(regionIsoEnabled.Contains(Constants.Values.True));
            }
            else
            {
                Assert.IsTrue(regionIsoEnabled == null || regionIsoEnabled.Count() == 0);
            }
        }
    }
}
