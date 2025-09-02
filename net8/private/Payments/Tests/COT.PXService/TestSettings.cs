// <copyright file="TestSettings.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace COT.PXService
{
    using System;
    using System.Runtime;

    public class TestSettings
    {
        public string Partner { get; set; }

        public string AccountId { get; set; }

        public string TestFlightAccountId { get; set; }

        public string UserName { get; set; }
        
        public string Pwd { get; set; }

        public string PifdHostName { get; set; }

        public AccountInfo Psd2TestAccount { get; set; }

        public AccountInfo PSD2EuroAccount { get; set; }

        public AccountInfo PSD2EuroAccountFrictionLess { get; set; }

        public AccountInfo PSD2NonEuroAccount { get; set; }

        public string LegacyBillableAccountId { get; set; }

        public string Puid { get; set; }

        public AccountInfo GetPSD2Account(bool euroMarket)
        {
            if (euroMarket)
            { 
                return PSD2EuroAccount;
            }

            return PSD2NonEuroAccount;
        }

        public static TestSettings CreateInstance()
        {
            EnvironmentType environmentType = TestBase.Current;
            TestSettings testSettings = new TestSettings();
            testSettings.Partner = "PX.COT";

            switch (environmentType)
            {
                case EnvironmentType.OneBox:
                case EnvironmentType.IntWestUSPme:
                case EnvironmentType.IntWestUS2Pme:
                    testSettings.AccountId = "18c69db8-dd2e-48a0-8887-1ccabd0bbcb2";
                    testSettings.UserName = "px_cot_int@outlook-int.com"; // lgtm[cs/hard-coded-id]
                    testSettings.PifdHostName = "pifd.cp.microsoft-int.com";
                    testSettings.PSD2NonEuroAccount = psd2INTAccountNonEuro;
                    testSettings.PSD2EuroAccount = psd2INTAccountEuro;
                    testSettings.PSD2EuroAccountFrictionLess = psd2INTAccountFrictionLess;

                    // [SuppressMessage("Microsoft.Security", "CS002:SecretInNextLine", Justification="ID for cot")]
                    // Flighting account info px_cot_int_flighting@outlook-int.com #Bugsfor$
                    testSettings.TestFlightAccountId = "3cfa0e51-97ae-49a8-9a71-398ca2ba0683";

                    // Azure INT portal uses MSA PROD for user authentication with Payments INT environment.
                    // Legacy billable account id of px_cot_int01@outlook.com in INT environment.
                    testSettings.LegacyBillableAccountId = "DjlwEAAAAAAAAAAA";

                    // PUID of px_cot_int01@outlook.com ("altsecid": "1:live.com:0003BFFD1AC1970A")
                    testSettings.Puid = "1055518726657802";  
                    break;

                case EnvironmentType.PpeWestUSPme:
                case EnvironmentType.PpeEastUS2Pme:
                case EnvironmentType.PpeNorthCentralUSPme:
                case EnvironmentType.PpeWestCentralUSPme:
                case EnvironmentType.PpeEastUSPme:
                    testSettings.AccountId = "5f805b21-d164-41a8-ab3c-7063ecbeb5c9";
                    testSettings.UserName = "px_cot_ppe@outlook.com";
                    testSettings.PifdHostName = "paymentinstruments-int.mp.microsoft.com";
                    testSettings.PSD2NonEuroAccount = psd2PRODAccountNonEuro;
                    testSettings.PSD2EuroAccount = psd2PRODAccountEuro;
                    testSettings.PSD2EuroAccountFrictionLess = psd2PRODAccountFrictionLess;

                    // Flighting account info px_cot_ppe01@outlook.com DZjJvrbytKrkj8sa
                    testSettings.TestFlightAccountId = "fed295b8-8aac-4280-b3b7-4cea8da4286b";

                    // Legacy billable account id of px_cot_prod@outlook.com in PROD environment. Using PROD setting for PPE.
                    testSettings.LegacyBillableAccountId = "2PAlegAAAAAAAAAA";

                    // PUID of px_cot_prod@outlook.com. Using PROD setting for PPE.
                    testSettings.Puid = "1055518384663485";
                    break;

                case EnvironmentType.ProdCanaryCentralUSPme:
                case EnvironmentType.ProdWestUSPme:
                case EnvironmentType.ProdCentralUSPme:
                case EnvironmentType.ProdSouthCentralUSPme:
                case EnvironmentType.ProdNorthCentralUSPme:
                case EnvironmentType.ProdWestCentralUSPme:
                case EnvironmentType.ProdEastUS2Pme:
                case EnvironmentType.ProdEastUSPme:
                case EnvironmentType.ProdWestUS2Pme:
                    testSettings.AccountId = "4015c80e-f369-43a4-8ffe-6e9e7fdca4d6";
                    testSettings.UserName = "px_cot_prod@outlook.com";
                    testSettings.PifdHostName = "paymentinstruments.mp.microsoft.com";
                    testSettings.PSD2NonEuroAccount = psd2PRODAccountNonEuro;
                    testSettings.PSD2EuroAccount = psd2PRODAccountEuro;
                    testSettings.PSD2EuroAccountFrictionLess = psd2PRODAccountFrictionLess;

                    // Flighting account info px_cot_prod01@outlook.com,DmWr73qhTgBv6abs
                    testSettings.TestFlightAccountId = "8505da08-7ebd-4cce-b6d6-bd87e4dadd07";

                    // Legacy billable account id of px_cot_prod@outlook.com in PROD environment.
                    testSettings.LegacyBillableAccountId = "2PAlegAAAAAAAAAA";

                    // PUID of px_cot_prod@outlook.com
                    testSettings.Puid = "1055518384663485";
                    break;

                default:
                    throw new NotSupportedException("Unsupported environment");
            }

            return testSettings;
        }

        private static AccountInfo psd2OneBoxTestAccount = new AccountInfo
        {
            AccountId = "62dc8681-6753-484a-981a-128f82a43d25",
            CreditCardPiid = "cef54f3d-0ff6-4de2-9a4b-c39f3115976c",
        };

        private static AccountInfo psd2PPETestAccount = new AccountInfo
        {
            // MSA: psd2.test.00@outlook.com
            AccountId = "8e22c40d-9011-411c-a09c-c64921959f15", 
            CreditCardPiid = "1066e9e8-c594-44b2-bbe2-2a91d4595f95",
        };

        private static AccountInfo psd2INTAccountNonEuro = new AccountInfo
        {
            /*
                userId: psd2.test.00@outlook-int.com
                Pwd: Refer to the one note
                Market: US
                PID: d4bd8a76-1de1-4949-8e58-071789b8188f
                AccountId: 7e5242d0-33ea-4bd1-a691-5193af93c4c7
            */
            AccountId = "7e5242d0-33ea-4bd1-a691-5193af93c4c7",
            CreditCardPiid = "d4bd8a76-1de1-4949-8e58-071789b8188f"
        };

        private static AccountInfo psd2INTAccountEuro = new AccountInfo
        {
            /*
                userId: psd2.test.00@outlook-int.com
                Pwd: Refer to the one note
                Market: US
                PID: d4bd8a76-1de1-4949-8e58-071789b8188f
                AccountId: 55d751dd-efed-4513-9f0f-6a6f7fed321e
            */
            AccountId = "7e5242d0-33ea-4bd1-a691-5193af93c4c7",
            CreditCardPiid = "55d751dd-efed-4513-9f0f-6a6f7fed321e"
        };

        private static AccountInfo psd2INTAccountFrictionLess = new AccountInfo
        {
            /*
                userId: psd2.test.00@outlook-int.com
                Pwd: Refer to the one note 

                Challenge Type: Frictionless
                Market: GB
                PID: 3559c52e-cad2-4fe3-9656-cef9ff21f90a
                AccountId: 7e5242d0-33ea-4bd1-a691-5193af93c4c7
            */
            AccountId = "7e5242d0-33ea-4bd1-a691-5193af93c4c7",
            CreditCardPiid = "3559c52e-cad2-4fe3-9656-cef9ff21f90a"
        };

        private static AccountInfo psd2PRODAccountNonEuro = new AccountInfo
        {
            /*
                userId: psd2.test.00@outlook.com
                Pwd: Refer to the one note 

                Market: US
                PID: wmRkegAAAAABAACA
                AccountId: 8e22c40d-9011-411c-a09c-c64921959f15
            */
            AccountId = "8e22c40d-9011-411c-a09c-c64921959f15",
            CreditCardPiid = "wmRkegAAAAABAACA"
        };

        private static AccountInfo psd2PRODAccountEuro = new AccountInfo
        {
            /*
                userId: psd2.test.00@outlook.com
                Pwd: Refer to the one note 

                Market: GB
                PID: jxWigAAAAAABAACA
                AccountId: 8e22c40d-9011-411c-a09c-c64921959f15
            */
            AccountId = "8e22c40d-9011-411c-a09c-c64921959f15",
            CreditCardPiid = "jxWigAAAAAABAACA"
        };

        private static AccountInfo psd2PRODAccountFrictionLess = new AccountInfo
        {
            /*
                userId: psd2.test.00@outlook.com
                Pwd: Refer to the one note 

                Challenge Type: Frictionless
                Market: GB
                PID: jxWigAAAAAACAACA
                AccountId: 8e22c40d-9011-411c-a09c-c64921959f15
            */
            AccountId = "8e22c40d-9011-411c-a09c-c64921959f15",
            CreditCardPiid = "jxWigAAAAAACAACA"
        };
    }
}
