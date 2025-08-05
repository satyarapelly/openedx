// <copyright file="InstrumentManagementServiceV3Client.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace COT.PXService
{
    using Test.Common;
    
    public class PXServiceClient : ServiceClient
    {
        public const string HeaderCorrelationVector = "MS-CV";

        public PXServiceClient(ServiceClientSettings settings)
            : base(settings)
        {
        }

    }
}
