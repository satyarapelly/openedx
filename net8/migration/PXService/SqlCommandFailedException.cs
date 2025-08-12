// <copyright file="SqlCommandFailedException.cs" company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception which occurs when a SQL command fails after exhausting retries.
    /// </summary>
    [Serializable]
    public class SqlCommandFailedException : Exception
    {
        private const string DefaultExceptionMessage = "The SQL command to {0} exhausted its retries.  See the InnerException for the failure(s).";

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCommandFailedException"/> class.
        /// </summary>
        /// <param name="serverName">The server to which the connection attempt was made.</param>
        /// <param name="commandExceptions">The list of exceptions which corresponds to each
        /// failed attempt of the command.</param>
        public SqlCommandFailedException(string serverName, IEnumerable<Exception> commandExceptions)
            : base(string.Format(CultureInfo.InvariantCulture, DefaultExceptionMessage, serverName), new AggregateException(commandExceptions))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCommandFailedException"/> class.
        /// </summary>
        /// <param name="info">The serialization state to use when constructing this
        /// instance.</param>
        /// <param name="context">The serialization context.</param>
        protected SqlCommandFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets the list of exceptions which correspond to each failed attempt of the command.
        /// </summary>
        public ReadOnlyCollection<Exception> CommandExceptions
        {
            get
            {
                return ((AggregateException)this.InnerException).InnerExceptions;
            }
        }
    }
}