// <copyright file="SllCorrelationVectorManager.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System.Diagnostics.Tracing;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Web;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.CommonSchema.Services.Logging;

    /// <summary>
    ///  The Sll correlation vector manager library.
    /// </summary>
    public class SllCorrelationVectorManager
    {
        /// <summary>
        /// Set correlation vector at request entry. More details please refer to 
        /// https://osgwiki.com/wiki/CorrelationVector.
        /// </summary>
        /// <param name="request">The request message of current operation.</param>
        /// <returns>The correlation vector that has been set.</returns>
        public static CorrelationVector SetCorrelationVectorAtRequestEntry(HttpRequestMessage request)
        {
            CorrelationVector correlationVector = null;

            // Check to see if there is a CorrelationVector present as a header using key 
            if (request.Headers.Contains(CorrelationVector.HeaderName))
            {
                // If present, get the value from the header and set the CV by calling extend. 
                // For example, if the value from the header is "tul4NUsfs9Cl7mOf.1", it will 
                // be extended to "tul4NUsfs9Cl7mOf.1.0" 
                string correlationVectorString = request.Headers.GetValues(CorrelationVector.HeaderName).First();

                // UrlDecode fixes the cases where the CV has characters like '/' and '+'
                string errorMessage = ValidateCorrelationVector(HttpUtility.UrlDecode(correlationVectorString, Encoding.UTF8));

                if (errorMessage != null)
                {
                    // If it's invalid correlation vector, trace down error but proceed anyway.
                    SllLogger.TraceMessage(string.Format("Invalid correlation vector: {0}, error: {1}.", correlationVectorString, errorMessage), EventLevel.Error);
                }
                else
                {
                    correlationVector = CorrelationVector.Extend(correlationVectorString);
                }
            }

            if (correlationVector == null)
            {
                correlationVector = new CorrelationVector();
            }

            Sll.Context.Vector = correlationVector;
            return correlationVector;
        }

        /// <summary>
        /// Validates the correlation vector. This code was stolen from Microsoft.CommonSchema.Services.Logging.CorrelationVector.ValidateCorrelationVector
        /// It mimics that functionality in its entirety.
        /// The reason for stealing, rather than using it directly is that the only way to invoke the provided validation function is to set
        /// a global static to true. This causes confusion and unintended consequences, so we will validate ourselves. 
        /// </summary>
        /// <param name="correlationVector">The vector, as a string, directly take from the header.</param>
        /// <returns>An error message. Null if there is no error.</returns>
        public static string ValidateCorrelationVector(string correlationVector)
        {
            if (string.IsNullOrWhiteSpace(correlationVector) || correlationVector.Length > 63)
            {
                return "The Correlation vector can not be null or bigger than 63 characters";
            }

            string[] array = correlationVector.Split('.');

            // As CV wiki page mentioned "The cV base increases from 16 characters in v1 to 22 characters in v2"
            if (array.Length < 2 || (array[0].Length != 16 && array[0].Length != 22))
            {
                return string.Format("Invalid correlation vector {0}", correlationVector);
            }

            for (int i = 1; i < array.Length; i++)
            {
                int num;
                if (!int.TryParse(array[i], out num))
                {
                    return string.Format("Invalid correlation vector {0}. Invalid Int32 value {1}", correlationVector, array[i]);
                }
            }

            return null;
        }
    }
}
