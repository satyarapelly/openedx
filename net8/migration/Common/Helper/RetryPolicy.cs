// <copyright file="RetryPolicy.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    using System;

    /// <summary>
    /// Retry policy for operations
    /// </summary>
    public abstract class RetryPolicy
    {
        public virtual TimeSpan RetryWindow
        {
            get
            {
                return TimeSpan.FromSeconds(60);
            }
        }

        public virtual int RetryCount
        {
            get
            {
                return 5;
            }
        }

        public abstract TimeSpan RetryIntervals(int attempts);

        public abstract bool IsTransient(Exception exception);

        /// <summary>
        /// Indicates whether and how to retry the operation in case of exception
        /// </summary>
        /// <param name="attempts">The attempts for the operations</param>
        /// <param name="exception">The exception from the operation</param>
        /// <param name="startTime">Start time of the first retry of the operation</param>
        /// <param name="retryDelay">Time to delay for the next retry</param>
        /// <returns>True if neext retry</returns>
        public bool ShouldRetry(int attempts, Exception exception, DateTime startTime, ref TimeSpan retryDelay)
        {
            if (this.IsTransient(exception) 
                && attempts <= this.RetryCount
                && startTime.Add(this.RetryWindow) > DateTime.UtcNow)
            {
                retryDelay = this.RetryIntervals(attempts);
                return true;
            }

            retryDelay = TimeSpan.Zero;
            return false;
        }
    }
}
