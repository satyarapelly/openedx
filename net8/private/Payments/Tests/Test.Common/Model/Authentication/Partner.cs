// <copyright file="Partner.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Authentication
{
    public static class Partner
    {
        /// <summary>
        /// All known user names
        /// </summary>
        public enum Name
        {
            /// <summary>
            /// Cert used for api call from PIFD Service to PX
            /// </summary>
            PIFDService,

            /// <summary>
            /// Cert used for PX COTs
            /// </summary>
            PXCOT,
        }
    }
}