// <copyright file="RequestBatch.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService
{
    using System.Net;

    public class RequestBatch
    {
        public int BatchSize { get; set; }

        public string AccountId { get; set; }

        public string PimsResponseContent { get; set; }

        public HttpStatusCode PimsResponseCode { get; set; }

        public HttpStatusCode ExpectedPXResponseCode { get; set; }
    }
}
