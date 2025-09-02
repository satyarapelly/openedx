// <copyright file="Samples.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PXService.TestData
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using ThreeDSExternal = Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService;

    public static class Samples
    {
        public const string NewSessionId = "this-is-new-session-id";

        public const string ExistingSessionId = "this-is-existing-session-id";

        public const string ThreeDSServerTransactionId = "this-is-3ds-transaction-id";

        public const string ThreeDSMethodURL = "https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/acs/fingerprint";

        public const string PifdBaseURL = "https://pifd.base.url.com";

        public const string AccountId = "4e80e16f-fc55-407e-b954-2370291a53fb";

        public const string ValidPiid = "q6ysEAAAAAAHAACA";

        public const string ThreeDSRejectedPiid = "this-is-piid-rejected-by-threeds-server";

        public const string Bin = "0000200000001000013";

        public const string ThreeDSRejectedBin = "rejected-bin";

        public const ThreeDSExternal.ChallengeWindowSize ChallengeWindowSize = ThreeDSExternal.ChallengeWindowSize.Two;

        public const string BrowserTimeZoneOffSet = "-2";

        public static Dictionary<string, Dictionary<string, PaymentInstrument>> PaymentInstruments
        {
            get
            {
                return new Dictionary<string, Dictionary<string, PaymentInstrument>>()
                {
                    {
                        AccountId,
                        new Dictionary<string, PaymentInstrument>()
                        {
                            { ValidPiid, ThreeDSChallengeElgibilePi }
                        }
                    },
                };
            }
        }
        
        public static PaymentInstrument ThreeDSChallengeElgibilePi
        {
            get
            {
                return new PaymentInstrument
                {
                    PaymentInstrumentDetails = new PaymentInstrumentDetails
                    {
                        SurpportedChallenge = new List<string>()
                        {
                            "3ds"
                        }
                    }
                };
            }
        }

        internal static class UserAgents
        {
            internal const string WindowsEdge = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0";
            internal const string WindowsChrome = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";
            internal const string AndroidEdge = "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Mobile Safari/537.36 EdgA/125.0.0.0";
            internal const string AndroidChrome = "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Mobile Safari/537.36";
            internal const string MacSafari = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4.1 Safari/605.1.15";
            internal const string MacEdge = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML%2C like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0";
            internal const string MacChrome = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.3";
            internal const string IOSSafari = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_1_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1.1 Mobile/15E148 Safari/604.1";
            internal const string IOSEdge = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_5_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 EdgiOS/126.2592.56 Mobile/15E148 Safari/605.1.15";
            internal const string IOSChrome = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/126.0.6478.54 Mobile/15E148 Safari/604.1";
            internal const string WindowsFirefox = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:127.0) Gecko/20100101 Firefox/127.0";
        }
    }
}
