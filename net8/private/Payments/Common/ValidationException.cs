// <copyright file="ValidationException.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;

    [Serializable]
    public class ValidationException : Exception
    {
        private const string ValidationErrorInfoName = "ValidationErrorCode";

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with provided error code.
        /// </summary>
        /// <param name="errorCode">error code string as in <see cref="ErrorCode"/> class</param>
        /// <param name="message">exception message</param>
        public ValidationException(ErrorCode errorCode, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with provided error code.
        /// </summary>
        /// <param name="errorCode">error code string as in <see cref="ErrorCode"/> class</param>
        /// <param name="target">exception target</param>
        /// <param name="message">exception message</param>
        public ValidationException(ErrorCode errorCode, string target, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
            this.Target = target;
        }

        public ErrorCode ErrorCode { get; protected set; }

        public string Target { get; protected set; }

        public static ValidationException TraceValidationException(EventTraceActivity traceActivityId, ValidationException exception)
        {
            return exception;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);
            info.AddValue(ValidationErrorInfoName, this.ErrorCode.ToString());
        }
    }
}