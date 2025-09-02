// <copyright file="PayerAuthServiceMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Test.Common;

    public class PayerAuthServiceMockResponseProvider : IMockResponseProvider
    {
        public string SessionId { get; set; }

        public string ThreeDSMethodUrl { get; set; }

        public string ThreeDSServerTransId { get; set; }

        public string EnrollmentStatus { get; set; }

        public string AcsTransId { get; set; }

        public string AcsUrl { get; set; }

        public string AcsSignedContent { get; set; }

        public string TransStatus { get; set; }

        public string TransStatusReason { get; set; }
        
        public void ResetDefaults()
        {
            this.SessionId = Guid.NewGuid().ToString();
            this.ThreeDSMethodUrl = "https://mockThreeDSMethodUrl";
            this.ThreeDSServerTransId = Guid.NewGuid().ToString();
            this.EnrollmentStatus = "Enrolled";
            this.AcsTransId = Guid.NewGuid().ToString();
            this.AcsUrl = "https://mockAcsUrl";
            this.AcsSignedContent = "mockAcsSignedContent";
            this.TransStatus = "Y";
            this.TransStatusReason = "TSR01";
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            string responseContent = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            if (request.RequestUri.ToString().Contains("CreatePaymentSessionId"))
            {
                responseContent = string.Format(
                    "{{\"payment_session_id\" : \"{0}\"}}",
                    this.SessionId);
            }
            else if (request.RequestUri.ToString().Contains("GetThreeDSMethodURL"))
            {
                responseContent = string.Format(
                    "{{\"three_ds_method_url\" : \"{0}\", \"three_ds_server_trans_id\" : \"{1}\"}}",
                    this.ThreeDSMethodUrl,
                    this.ThreeDSServerTransId);
            }
            else if (request.RequestUri.ToString().Contains("Authenticate"))
            {
                responseContent = string.Format(
                    "{{\"three_ds_server_transaction_id\" : \"{0}\", \"acs_transaction_id\" : \"{1}\", \"enrollment_status\" : \"{2}\", \"acs_url\" : \"{3}\", \"acs_signed_content\" : \"{4}\", \"transaction_challenge_status\" : \"{5}\", \"transaction_challenge_status_reason\" : \"{6}\"}}",
                    this.ThreeDSServerTransId,
                    this.AcsTransId,
                    this.EnrollmentStatus,
                    this.AcsUrl,
                    this.AcsSignedContent,
                    this.TransStatus,
                    this.TransStatusReason);
            }
            else if (request.RequestUri.ToString().Contains("CompleteChallenge"))
            {
                responseContent = "{\"authenticate_value\":\"{\\\"authorization_parameters\\\":{\\\"PaRes\\\":\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiNDdmY2ZjMzYtNmY3NC00MTI4LThkNGMtNDQyYzM5NjRmZjRhIiwiYXV0aGVudGljYXRpb25fc3RhdHVzIjoic3VjY2VzcyJ9\\\",\\\"xid\\\":\\\"ABXcpervt=\\\",\\\"pares\\\":\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiNDdmY2ZjMzYtNmY3NC00MTI4LThkNGMtNDQyYzM5NjRmZjRhIn0=\\\",\\\"md\\\":null,\\\"cavv\\\":\\\"CGtpseerccma==\\\",\\\"cavvAlgorithm\\\":\\\"2\\\"}}\",\"eci\":\"05\",\"transaction_challenge_status\":\"Y\"}";
            }

            if (string.IsNullOrEmpty(responseContent))
            {
                return null;
            }

            return await Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    responseContent,
                    System.Text.Encoding.UTF8,
                    "application/json")
            });
        }
    }
}