// <copyright file="DeclinedOperationException.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common
{
    using System;
    using System.Runtime.Serialization;
    using Common.Transaction;

    /// <summary>
    /// This exception class is thrown by providers when an operation fails and it is not sure of the result of the operation
    /// </summary>
    [Serializable]
    public class DeclinedOperationException : Exception
    {
        private const string DeclineCodeInfoName = "DeclineCode";

        /// <summary>
        /// Initializes a new instance of the <see cref="DeclinedOperationException"/> class.
        /// </summary>
        /// <param name="code">decline code</param>
        /// <param name="message">exception message</param>
        public DeclinedOperationException(TransactionDeclineCode code, string message)
            : base(message)
        {
            this.DeclineCode = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeclinedOperationException"/> class.
        /// </summary>
        /// <param name="code">decline code</param>
        /// <param name="message">exception message </param>
        /// <param name="innerException">inner exception</param>
        public DeclinedOperationException(TransactionDeclineCode code, string message, Exception innerException)
            : base(message, innerException)
        {
            this.DeclineCode = code;
        }

        public TransactionDeclineCode DeclineCode { get; protected set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);
            info.AddValue(DeclineCodeInfoName, this.DeclineCode.ToString());
        }
    }
}
