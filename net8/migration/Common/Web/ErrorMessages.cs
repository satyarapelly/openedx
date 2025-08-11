// <copyright file="ErrorMessages.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    /// <summary>
    /// API error messages
    /// </summary>
    public static class ErrorMessages
    {
        /// <summary>
        /// No tracking id in hearder
        /// </summary>
        public const string NoTrackingId = "Tracking id is required, you must fill x-ms-tracking-id in the request header with a GUID";

        /// <summary>
        /// Null request object
        /// </summary>
        public const string NullRequestObject = "The request object is null";

        /// <summary>
        /// Bad data format of a property or an argument
        /// </summary>
        public const string BadDataFormat = "The data of {0} is not a valid {1}";

        /// <summary>
        /// Invalid data of a property or an argument
        /// </summary>
        public const string InvalidData = "{0} cannot be {1}";

        /// <summary>
        /// A bad response is returned from the internal service
        /// </summary>
        public const string BadServiceReponse = "A bad response is returned from the internal service";
    }
}
