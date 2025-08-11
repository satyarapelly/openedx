// <copyright file="ThrowIf.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Utility class for handling different validation checks.
    /// </summary>
    public static class ThrowIf
    {
        /// <summary>
        /// Provides argument utility functions.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Allows for a nice chain call")]
        public static class Argument
        {
            /// <summary>
            /// Checks for null args.
            /// </summary>
            /// <typeparam name="T">The type of the arg.</typeparam>
            /// <param name="argument">The arg.</param>
            /// <param name="name">The name of the arg.</param>
            /// <param name="message">Exception message if the argument is null.</param>
            public static void IsNull<T>(T argument, string name, string message = null)
            {
                if (argument == null)
                {
                    throw new ArgumentNullException(name, message);
                }
            }

            /// <summary>
            /// Checks for null args.
            /// </summary>
            /// <typeparam name="T">The type of the arg.</typeparam>
            /// <typeparam name="TException">The type of the exception.</typeparam>
            /// <param name="argument">The arg.</param>
            /// <param name="exception">The exception to throw if the argument is null.</param>
            public static void IsNull<T, TException>(T argument, TException exception)
                where TException : Exception
            {
                if (argument == null)
                {
                    throw exception;
                }
            }

            /// <summary>
            /// Checks for null or empty strings.
            /// </summary>
            /// <param name="argument">The string arg.</param>
            /// <param name="name">The name of the arg.</param>
            public static void IsNullOrWhiteSpace(string argument, string name)
            {
                if (string.IsNullOrWhiteSpace(argument))
                {
                    throw new ArgumentException($"{name} should not be null or white space.");
                }
            }

            /// <summary>
            /// Checks if the int value is out of range.
            /// </summary>
            /// <param name="value">The value to check.</param>
            /// <param name="min">The minimum allowed value (inclusive).</param>
            /// <param name="max">The maximum allowed value (inclusive).</param>
            /// <param name="name">The name of the arg.</param>
            public static void IsOutOfRange(int value, int min, int max, string name)
            {
                if (value < min || value > max)
                {
                    throw new ArgumentOutOfRangeException($"{name} is out of the expected range {min} - {max}. Actual value = {value}.");
                }
            }
        }
    }
}