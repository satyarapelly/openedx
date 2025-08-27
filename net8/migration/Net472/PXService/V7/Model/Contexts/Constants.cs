// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.Contexts
{
    public static class Constants
    {
        public static class CustomerHeaderJwtTokenClaim
        {
            /// <summary>
            /// Requester claim.
            /// </summary>
            public const string Requester = "requester";

            /// <summary>
            /// Target claim.
            /// </summary>
            public const string Target = "target";

            /// <summary>
            /// AuthType claim.
            /// </summary>
            public const string AuthType = "authType";

            /// <summary>
            /// Caller name claim.
            /// </summary>
            public const string Caller = "caller";
        }

        public static class CustomerType
        {
            public const string MSA = "msa";
            public const string TNT = "tnt";
            public const string AADUSR = "aadusr";
            public const string BingAdsId = "bingadsid";
            public const string CommerceRoot = "commerceroot";
            public const string AnonymousUser = "anonymoususr";
        }
    }
}