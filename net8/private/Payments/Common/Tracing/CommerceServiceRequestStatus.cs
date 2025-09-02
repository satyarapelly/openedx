// <copyright file="CommerceServiceRequestStatus.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    public enum CommerceServiceRequestStatus
    {
        Undefined = 0,
        Unknown = 1,
        Other = 2,
        Success = 3,
        CallerError = 4,
        TransportError = 5,
        ServiceError = 6,
    }
}
