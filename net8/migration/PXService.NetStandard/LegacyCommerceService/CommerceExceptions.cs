// <copyright file="CommerceExceptions.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using System.Runtime.Serialization;

    public enum ErrorNamespace
    {
        None,

        DataAccessorLayer,

        DataModel,

        Commerce,

        Subs,

        DMP,

        CTPCommerce,
    }

    public interface IExceptionInfoProvider
    {
        ErrorNamespace DefaultErrorNamespace { get; }

        ErrorType DefaultErrorType { get; }

        ErrorType GetErrorType(int errorCode, string debugInfo);
    }

    public class DataAccessErrors : IExceptionInfoProvider
    {
        public ErrorNamespace DefaultErrorNamespace
        {
            get { return ErrorNamespace.DataAccessorLayer; }
        }

        public ErrorType DefaultErrorType
        {
            get { return Errors[DATAACCESS_E_INTERNAL_SERVER_ERROR]; }
        }

        public ErrorType GetErrorType(int errorCode, string debugInfo)
        {
            ErrorType errorType = Errors[DATAACCESS_E_INTERNAL_SERVER_ERROR];
            Errors.TryGetValue(errorCode, out errorType);

            ErrorType returnErrorType = errorType.Clone() as ErrorType;
            returnErrorType.ErrorDescription = debugInfo;

            return returnErrorType;
        }

        public const int DATAACCESS_E_INTERNAL_SERVER_ERROR = 80101;
        public const int DATAACCESS_E_EXTERNAL_TIMEOUT_ERROR = 80102;
        public const int DATAACCESS_E_INVALID_ARGUMENT = 80103;

        public const int DATAACCESS_E_SERVICECALL_ERROR = 80111;
        public const int DATAACCESS_E_ACCOUNT_SERVICECALL_ERROR = 80112;
        public const int DATAACCESS_E_PI_SERVICECALL_ERROR = 80113;
        public const int DATAACCESS_E_SUBS_SERVICECALL_ERROR = 80114;
        public const int DATAACCESS_E_TX_SERVICECALL_ERROR = 80115;
        public const int DATAACCESS_E_TAX_SERVICECALL_ERROR = 80116;

        public const int DATAACCESS_E_MM_SAPICERT_MISSING = 80117;
        public const int DATAACCESS_E_MM_SELFROLESOAPHEADER_MISSING = 80118;
        public const int DATAACCESS_E_MM_SITETOKEN_MISSING = 80119;
        //for validate cvv failed: ErrorCodes.CX_E_M_INVALID_PAYMENT_INSTRUMENT
        public const int DATAACCESS_E_MM_INVALID_PAYMENT_INSTRUMENT = 80120;
        public const int DATAACCESS_E_RESTAPI_SERVICECALL_ERROR = 80121;

        private static Dictionary<int, ErrorType> Errors = new Dictionary<int, ErrorType>()
        {
            {
                DATAACCESS_E_INTERNAL_SERVER_ERROR, new ErrorType
                {
                    ErrorCode = DATAACCESS_E_INTERNAL_SERVER_ERROR,
                    ErrorShortMessage = "DATAACCESS_E_INTERNAL_SERVER_ERROR",
                    ErrorLongMessage = "Server encountered unspecified error.",
                    Retriable = true,
                    IsSystemError = true,
                }
            },
            {
                DATAACCESS_E_EXTERNAL_TIMEOUT_ERROR, new ErrorType
                {
                    ErrorCode = DATAACCESS_E_EXTERNAL_TIMEOUT_ERROR,
                    ErrorShortMessage = "DATAACCESS_E_EXTERNAL_TIMEOUT_ERROR",
                    ErrorLongMessage = "Timeout when calling external component.",
                    Retriable = true,
                }
            },
            {
                DATAACCESS_E_INVALID_ARGUMENT, new ErrorType
                {
                    ErrorCode = DATAACCESS_E_INVALID_ARGUMENT,
                    ErrorShortMessage = "DATAACCESS_E_INVALID_ARGUMENT",
                    ErrorLongMessage = "Invalid argument for input.",
                    Retriable = false,
                }
            },
            {
                DATAACCESS_E_SERVICECALL_ERROR, new ErrorType
                {
                    ErrorCode = DATAACCESS_E_SERVICECALL_ERROR,
                    ErrorShortMessage = "DATAACCESS_E_SERVICECALL_ERROR",
                    ErrorLongMessage = "Response error after service call",
                    IsSystemError = true,
                }
            },
            {
                DATAACCESS_E_ACCOUNT_SERVICECALL_ERROR, new ErrorType
                {
                    ErrorCode = DATAACCESS_E_ACCOUNT_SERVICECALL_ERROR,
                    ErrorShortMessage = "DATAACCESS_E_ACCOUNT_SERVICECALL_ERROR",
                    ErrorLongMessage = "Response error after account service call",
                    IsSystemError = true,
                }
            },
            {
                DATAACCESS_E_PI_SERVICECALL_ERROR, new ErrorType
                {
                    ErrorCode = DATAACCESS_E_PI_SERVICECALL_ERROR,
                    ErrorShortMessage = "DATAACCESS_E_PI_SERVICECALL_ERROR",
                    ErrorLongMessage = "Response error after pi service call",
                    IsSystemError = true,
                }
            },
            {
                DATAACCESS_E_SUBS_SERVICECALL_ERROR, new ErrorType
                {
                    ErrorCode = DATAACCESS_E_SUBS_SERVICECALL_ERROR,
                    ErrorShortMessage = "DATAACCESS_E_SUBS_SERVICECALL_ERROR",
                    ErrorLongMessage = "Response error after subs service call",
                    IsSystemError = true,
                }
            },
            {
                DATAACCESS_E_TX_SERVICECALL_ERROR, new ErrorType
                {
                    ErrorCode = DATAACCESS_E_TX_SERVICECALL_ERROR,
                    ErrorShortMessage = "DATAACCESS_E_TX_SERVICECALL_ERROR",
                    ErrorLongMessage = "Response error after tx service call",
                    IsSystemError = true,
                }
            },
            {
                DATAACCESS_E_MM_SAPICERT_MISSING, new ErrorType{
                    ErrorCode = DATAACCESS_E_MM_SAPICERT_MISSING,
                    ErrorShortMessage = "DATAACCESS_E_MM_SAPICERT_MISSING",
                    ErrorLongMessage="No SAPI certificate config found under registry",
                    Retriable = false,
                }
            },
            {
                DATAACCESS_E_MM_SELFROLESOAPHEADER_MISSING,new ErrorType
                {
                    ErrorCode = DATAACCESS_E_MM_SELFROLESOAPHEADER_MISSING,
                    ErrorShortMessage = "DATAACCESS_E_MM_SELFROLESOAPHEADER_MISSING",
                    ErrorLongMessage = "No SAPI self role soap header found",
                    Retriable = false,
                }
            },
            {
                DATAACCESS_E_MM_SITETOKEN_MISSING,new ErrorType
                {
                    ErrorCode = DATAACCESS_E_MM_SITETOKEN_MISSING,
                    ErrorShortMessage = "DATAACCESS_E_MM_SITETOKEN_MISSING",
                    ErrorLongMessage ="No SAPI site token found",
                    Retriable = false,
                }
            },
            {
                DATAACCESS_E_MM_INVALID_PAYMENT_INSTRUMENT,new ErrorType
                {
                    ErrorCode = DATAACCESS_E_MM_INVALID_PAYMENT_INSTRUMENT,
                    ErrorShortMessage = "DATAACCESS_E_MM_INVALID_PAYMENT_INSTRUMENT",
                    ErrorLongMessage ="Invalid Payment Instrument",
                    Retriable = false,
                }
            },
            {
                DATAACCESS_E_RESTAPI_SERVICECALL_ERROR,new ErrorType
                {
                    ErrorCode = DATAACCESS_E_RESTAPI_SERVICECALL_ERROR,
                    ErrorShortMessage = "DATAACCESS_E_RESTAPI_SERVICECALL_ERROR",
                    ErrorLongMessage ="Invalid Rest Api call",
                    Retriable = false,
                }
            },
        };
    }

    [Serializable]
    public class CtpWebExceptionBaseException<T> : Exception where T : IExceptionInfoProvider, new()
    {
        static T exceptionProvider = new T();

        private const string DefaultUndefinedErrorMessage = "An unexpected error occurred.";

        protected ErrorNamespace DefaultErrorNamespace { get { return exceptionProvider.DefaultErrorNamespace; } }

        protected ErrorType DefaultErrorType { get { return exceptionProvider.DefaultErrorType; } }

        /// <summary>
        /// This ErrorCode is not mapped by ErrorNamespace
        /// </summary>
        public int ErrorCode { get; protected set; }

        public ErrorNamespace ErrorNamespace { get; protected set; }

        public ErrorType Error { get; protected set; }

        public CtpWebExceptionBaseException()
        {
            SetErrorInfo(this.DefaultErrorNamespace, this.DefaultErrorType.ErrorCode);
        }

        public CtpWebExceptionBaseException(int errorCode)
        {
            SetErrorInfo(this.DefaultErrorNamespace, errorCode);
        }

        public CtpWebExceptionBaseException(int errorCode, string message)
            : base(message)
        {
            SetErrorInfo(this.DefaultErrorNamespace, errorCode);
        }

        public CtpWebExceptionBaseException(int errorCode, string message, Exception inner)
            : base(message, inner)
        {
            SetErrorInfo(this.DefaultErrorNamespace, errorCode);
        }

        public CtpWebExceptionBaseException(Exception inner)
            : base(DefaultUndefinedErrorMessage, inner)
        {
            SetErrorInfo(this.DefaultErrorNamespace, this.DefaultErrorType.ErrorCode);
        }

        public CtpWebExceptionBaseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            SetErrorInfo(this.DefaultErrorNamespace, this.DefaultErrorType.ErrorCode);
        }

        public CtpWebExceptionBaseException(ErrorNamespace errorNamespace,
            int errorCode, string shortMessage, string longMessage, string description, bool retryable)
            : base(description)
        {
            ErrorCode = errorCode;
            ErrorNamespace = errorNamespace;
            Error = new ErrorType
            {
                ErrorCode = errorCode,
                ErrorShortMessage = shortMessage,
                ErrorLongMessage = longMessage,
                ErrorDescription = description,
                Retriable = retryable,
            };
            Error.ErrorCode = MapErrorCodeByNamespace(ErrorNamespace, Error.ErrorCode);
        }

        public static int MapErrorCodeByNamespace(ErrorNamespace errorNamespace, int errorCode)
        {
            // Each error range contains 65536 error code.
            int errorbase = 0;
            switch (errorNamespace)
            {
                case ErrorNamespace.None:
                case ErrorNamespace.DataAccessorLayer:
                case ErrorNamespace.Commerce:
                case ErrorNamespace.Subs:
                    errorbase = 0;
                    break;
                case ErrorNamespace.DataModel:
                case ErrorNamespace.DMP:
                    errorbase = 64000;
                    break;
                default:
                    break;
            }
            return errorbase + errorCode;
        }

        public override string Message
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(base.Message);
                Exception inner = this.InnerException;
                while (inner != null)
                {
                    sb.AppendFormat("Inner Exception:[{0}] {1}.\n", inner.GetType().Name, inner.Message);
                    inner = inner.InnerException;
                }

                return sb.ToString();
            }
        }

        private void SetErrorInfo(ErrorNamespace errorNamespace, int errorCode)
        {
            ErrorNamespace = errorNamespace;
            ErrorCode = errorCode;

            ErrorType errorType = new ErrorType
            {
                ErrorCode = errorCode,
                ErrorShortMessage = base.Message,
                ErrorLongMessage = base.Message,
                Retriable = true,
                IsSystemError = true,
            };

            Error = errorType.Clone() as ErrorType;
            Error.ErrorDescription = Message;
            Error.ErrorCode = MapErrorCodeByNamespace(ErrorNamespace, Error.ErrorCode);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class DataAccessException : CtpWebExceptionBaseException<DataAccessErrors>
    {
        public DataAccessorTracerResult TracerResult { get; set; }

        public string ResponseBody { get; set; }

        public DataAccessException()
            : base() { }

        public DataAccessException(int errorCode)
            : base(errorCode) { }

        public DataAccessException(int errorCode, string message)
            : base(errorCode, message) { }

        public DataAccessException(int errorCode, string message, Exception inner)
            : base(errorCode, message, inner) { }

        public DataAccessException(int errorCode, string message, Exception inner, DataAccessorTracerResult tracerResult, string responseBody = null)
            : base(errorCode, message, inner)
        {
            this.TracerResult = tracerResult;
            this.ResponseBody = responseBody;
        }

        public DataAccessException(Exception inner)
            : base(inner) { }

        protected DataAccessException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        public DataAccessException(ErrorNamespace errorNamespace,
            int errorCode, string shortMessage, string longMessage, string description, bool retryable)
            : base(errorNamespace, errorCode, shortMessage, longMessage, description, retryable) { }

        public DataAccessException(ErrorNamespace errorNamespace,
            int errorCode, string shortMessage, string longMessage, string description, bool retryable, DataAccessorTracerResult tracerResult)
            : base(errorNamespace, errorCode, shortMessage, longMessage, description, retryable)
        {
            this.TracerResult = tracerResult;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);
        }
    }
}
