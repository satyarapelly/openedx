// <copyright file="IntegrationErrorCode.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    /// <summary>
    /// Integration Error Codes
    /// </summary>
    public enum IntegrationErrorCode
    {
        /// <summary>
        /// The error occurred when a parameter in the request is not in valid format
        /// </summary>
        InvalidRequestParameterFormat,

        /// <summary>
        /// The error occurred when getIssuerAPI call failed
        /// </summary>
        GetIssuerAPIError,

        /// <summary>
        /// The information for getIssuerAPI
        /// </summary>
        GetIssuerAPIInfo,

        /// <summary>
        /// The warning for getIssuerAPI
        /// </summary>
        GetIssuerAPIWarning,

        /// <summary>
        /// The debug information for getIssuerAPI
        /// </summary>
        GetIssuerAPIDebug,

        /// <summary>
        ///  The directory service information is not found
        /// </summary>
        DSInfoNotFound,

        /// <summary>
        ///  The ACS signed content is invalid
        /// </summary>
        AcsSignedContentInvalid,
        
        /// <summary>
        ///  The dsCert chain or acsCert build is invalid
        /// </summary>
        InvalidBuild,

        /// <summary>
        ///  ACS Certificate signature could not be verified
        /// </summary>
        SignatureFailure
    }
}