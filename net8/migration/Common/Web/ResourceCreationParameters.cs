// <copyright file="ResourceCreationParameters.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public class ResourceCreationParameters
    {
        public Guid TrackingId { get; set; }

        public EventTraceActivity EventTraceActivity { get; set; }

        public string ValidationError { get; private set; }

        public void Validate()
        {
            if (!this.ValidateAndSetError())
            {
                throw new ValidationException(ErrorCode.InvalidRequestData, this.ValidationError);
            }
        }

        protected void SetError(string validationError)
        {
            this.ValidationError = validationError;
        }

        protected virtual bool ValidateAndSetError()
        {
            return true;
        }
    }
}
