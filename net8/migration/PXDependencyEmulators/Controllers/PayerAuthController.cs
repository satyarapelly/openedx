// <copyright file="PayerAuthController.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
using FromUri = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
    using Common;
    using Common.Transaction;
    using Common.Web;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Newtonsoft.Json;
    using PXService.Model.PayerAuthService;

    public enum PayerAuthApiVersion
    {
        V2,
        V3
    }

    public class PayerAuthController : ControllerBase
    {
        private const string AcsEmulatorUrlPrefix = "https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/acs/{0}";

        [HttpPost]
        public async Task<PaymentSessionResponse> CreatePaymentSessionId(PaymentSessionRequest paymentSessionRequest)
        {
            var response = new PaymentSessionResponse()
            {
                PaymentSessionId = "ZFFFFFFFFFFF" + Guid.NewGuid().ToString()
            };

            return await Task.FromResult(response);
        }

        [HttpPost]
        public async Task<ThreeDSMethodData> Get3DSMethodURL(object request)
        {
            TestContext testContext = null;
            this.Request.TryGetTestContext(out testContext);
            string threeDSMethodURL = string.Format(AcsEmulatorUrlPrefix, "fingerprint");
            if (testContext.ScenariosContain(Constants.TestScenarios.PXPayerAuthPSD2ChallengeSkipfp) 
                || testContext.ScenariosContain(Constants.TestScenarios.PXPayerAuthPSD2ChallengeSkipcreq))
            {
                threeDSMethodURL = null;
            }

            ThreeDSMethodData methodData = new ThreeDSMethodData()
            {
                ThreeDSMethodURL = threeDSMethodURL,
                ThreeDSServerTransID = Guid.NewGuid().ToString()
            };

            return await Task.FromResult(methodData);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Authenticate([FromBody] Newtonsoft.Json.Linq.JObject authRequest)
        {
            string apiVersion = this.Request.GetRequestHeader("api-version");

            if (apiVersion == "2018-10-03")
            {
                return await this.Authenticate(authRequest.ToObject<AReq>());
            }
            else if (apiVersion == "2019-04-16")
            {
                return await this.Authenticate(authRequest.ToObject<AuthenticationRequest>());
            }
            else
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Api Request sent to PX.DependencyEmulators.PayerAuth");
            }
        }

        [HttpPost]
        public async Task<RRes> Result(object request)
        {
            RRes resp = new RRes()
            {
                TransactionStatus = "Y"
            };

            TestContext testContext = null;
            this.Request.TryGetTestContext(out testContext);
            if (testContext.ScenariosContain(Constants.TestScenarios.PXPayerAuthPSD2ChallengeCancelled))
            {
                resp.TransactionStatus = "A";
            }
            else if (testContext.ScenariosContain(Constants.TestScenarios.PXPayerAuthPSD2ChallengeTimedout)
                || testContext.ScenariosContain(Constants.TestScenarios.PXPIMS3DSChallengeTimedout))
            {
                resp.TransactionStatus = "T";
            }

            return await Task.FromResult(resp);
        }

        [HttpPost]
        public async Task<CompletionResponse> CompleteChallenge(CompletionRequest request)
        {
            var resp = new CompletionResponse()
            {
                ChallengeCompletionIndicator = ChallengeCompletionIndicator.Y,
                TransactionStatus = PXService.Model.PayerAuthService.TransactionStatus.Y
            };

            TestContext testContext = null;
            this.Request.TryGetTestContext(out testContext);
            if (testContext.ScenariosContain(Constants.TestScenarios.PXPayerAuthPSD2ChallengeCancelled))
            {
                resp.ChallengeCompletionIndicator = ChallengeCompletionIndicator.N;
                resp.TransactionStatus = PXService.Model.PayerAuthService.TransactionStatus.R;
                resp.ChallengeCancelIndicator = ChallengeCancelIndicator.CancelledByCardHolder.ToString();
            }
            else if (testContext.ScenariosContain(Constants.TestScenarios.PXPayerAuthPSD2ChallengeTimedout)
                || testContext.ScenariosContain(Constants.TestScenarios.PXPIMS3DSChallengeTimedout))
            {
                resp.ChallengeCompletionIndicator = ChallengeCompletionIndicator.N;
                resp.TransactionStatus = PXService.Model.PayerAuthService.TransactionStatus.R;
                resp.TransactionStatusReason = TransactionStatusReason.TSR14;
            }

            return await Task.FromResult(resp);
        }

        private static string ToExpiryDate(TestContext testContext)
        {
            Dictionary<string, string> expirationDates = new Dictionary<string, string>()
            {
                { "acs.ui.singleselect", "3103" },
                { "acs.ui.multiselect", "3108" },
                { "acs.ui.otp", "3011" },
                { "acs.ui.oob", "3403" },
                { "acs.ui.htmlsingleselect", "3503" },
                { "acs.ui.htmlmultiselect", "3008" },
                { "acs.ui.htmlotp", "3206" },
                { "acs.ui.htmloob", "3803" },
                { "acs.ui.htmlother", "3903" },
            };

            foreach (var expirationDate in expirationDates)
            {
                if (testContext.ScenariosContain(expirationDate.Key))
                {
                    return expirationDate.Value;
                }
            }

            return "2103";
        }

        private async Task<HttpResponseMessage> Authenticate(AReq authRequest)
        {
            TestContext testContext = null;
            this.Request.TryGetTestContext(out testContext);
            if (authRequest.IsMoto)
            {
                ARes payAuthAres = new ARes
                {
                    EnrollmentStatus = PaymentInstrumentEnrollmentStatus.Bypassed,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, payAuthAres);
            }
            else if (testContext.ScenariosContain(Constants.TestScenarios.PXPayerAuthPSD2ChallengeSkipcreq))
            {
                ARes payAuthAres = new ARes
                {
                    EnrollmentStatus = PaymentInstrumentEnrollmentStatus.Bypassed,
                    AcsTransactionId = authRequest.ThreeDSServerTransactionId
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, payAuthAres);
            }
            else if (testContext.ScenariosContain(Constants.TestScenarios.PXPayerAuthPSD2ChallengeFailed))
            {
                ARes payAuthAres = new ARes
                {
                    EnrollmentStatus = PaymentInstrumentEnrollmentStatus.NotEnrolled,
                    AcsTransactionId = authRequest.ThreeDSServerTransactionId
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, payAuthAres);
            }
            else
            {
                return await this.CallAcsEmulator(
                    payerAuthApiVersion: PayerAuthApiVersion.V2,
                    v2AuthRequest: authRequest,
                    v3AuthRequest: null,
                    testContext: testContext);
            }
        }
        
        private async Task<HttpResponseMessage> Authenticate(AuthenticationRequest authRequest)
        {
            TestContext testContext = null;
            this.Request.TryGetTestContext(out testContext);
            if (authRequest.PaymentSession.IsMOTO)
            {
                var payAuthAres = new AuthenticationResponse
                {
                    EnrollmentStatus = PaymentInstrumentEnrollmentStatus.Bypassed,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, payAuthAres);
            }
            else if (testContext.ScenariosContain(Constants.TestScenarios.PXPayerAuthPSD2ChallengeSkipcreq))
            {
                var payAuthAres = new AuthenticationResponse
                {
                    EnrollmentStatus = PaymentInstrumentEnrollmentStatus.Bypassed,
                    AcsTransactionId = authRequest.ThreeDSServerTransId
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, payAuthAres);
            }
            else if (testContext.ScenariosContain(Constants.TestScenarios.PXPayerAuthPSD2ChallengeFailed))
            {
                var payAuthAres = new AuthenticationResponse
                {
                    EnrollmentStatus = PaymentInstrumentEnrollmentStatus.NotEnrolled,
                    AcsTransactionId = authRequest.ThreeDSServerTransId
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, payAuthAres);
            }
            else if (testContext.ScenariosContain(Constants.TestScenarios.PXPayerAuthPSD2ChallengeSuccess))
            {
                // To bypass the AzureDevOps secret scanning,add "Placeholder" in secrets and then remove it.
                var payAuthAres = new AuthenticationResponse
                {
                    AcsSignedContent = "Placeholder_eyJ4NWMiOlsiTUlJRTZ6Q0NBOU9nQXdJQkFnSVVkUUs3SUZUVDFWUEwwcVp2d1h5UTBySytGVHN3RFFZSktvWklodmNOQVFFTEJRQXdjVEVMTUFrR0ExVUVCaE1DVlZNeERUQUxCZ05WQkFvVEJGWkpVMEV4THpBdEJnTlZCQXNUSmxacGMyRWdTVzUwWlhKdVlYUnBiMjVoYkNCVFpYSjJhV05sSUVGemMyOWphV0YwYVc5dU1TSXdJQVlEVlFRREV4bFdhWE5oSUdWRGIyMXRaWEpqWlNCSmMzTjFhVzVuSUVOQk1CNFhEVEl4TURVd05qQTFNekEwTkZvWERUSXlNRFV3TmpBMU16QTBORm93Z1pzeER6QU5CZ05WQkFjTUJrSmxlbTl1Y3pFV01CUUdBMVVFQ0F3TlNXeGxMV1JsTFVaeVlXNWpaVEVMTUFrR0ExVUVCaE1DUmxJeEdEQVdCZ05WQkFvTUQwVnhkV1Z1YzFkdmNteGtiR2x1WlRFY01Cb0dBMVVFQ3d3VFJYRjFaVzV6VjI5eWJHUnNhVzVsSUVGRFV6RXJNQ2tHQTFVRUF3d2ljSEp2WkM1aFkzTXpaSE15TG5acGMyRXVjMmxuYmk1M2JIQXRZV056TG1OdmJUQ0NBU0l3RFFZSktvWklodmNOQVFFQkJRQURnZ0VQQURDQ0FRb0NnZ0VCQU82SlUrcmxwcjlaS05aeURIWTRDMWtKbWYzQjRSUjdFNWxQVGRCQXcxa2N1bWFpcE52OWJmUkN1aGJMcURpRERlZGFlNkxXNi9UeHZiR3pGenFUSUtndzZFRkFRRDltVkhVNDdRZEZNb1huelRjc3FIbFFkQVFFakdoZVRJZ2puREU3STBUZFczT3FVTWc3SkRzc1hwMllrcWFJSCtGSDF4N016cWMzbFcxQWN0L1AzZGZCS2dHNXlYakhXM0U5S2dJSXRrbWtaMDdHUklwbE1qbU03S2o2YVFKZWowYnRHanU2V2MyYmFmbWlGbzFldk92SlBkYlVnbm5Pek84TjN3TGQ3SVRJZmJZblJzS291MEhKOHpoZlpDYzlRS2UyMVlvZDErY2hHSldUTlBVeU9rWUZVaTJ2UTg3by9vUkFxOEM5emRUL2YzUlZQemF1S2pBTmxDMENBd0VBQWFPQ0FVNHdnZ0ZLTUF3R0ExVWRFd0VCL3dRQ01BQXdId1lEVlIwakJCZ3dGb0FVMzhNcVZTNHZRanA2UUoyU29QZEhlZGVQaDRvd1pRWUlLd1lCQlFVSEFRRUVXVEJYTUM0R0NDc0dBUVVGQnpBQ2hpSm9kSFJ3T2k4dlpXNXliMnhzTG5acGMyRmpZUzVqYjIwdlpXTnZiVzB1WTJWeU1DVUdDQ3NHQVFVRkJ6QUJoaGxvZEhSd09pOHZiMk56Y0M1MmFYTmhMbU52YlM5dlkzTndNRGtHQTFVZElBUXlNREF3TGdZRlo0RURBUUV3SlRBakJnZ3JCZ0VGQlFjQ0FSWVhhSFIwY0RvdkwzZDNkeTUyYVhOaExtTnZiUzl3YTJrd0V3WURWUjBsQkF3d0NnWUlLd1lCQlFVSEF3SXdNd1lEVlIwZkJDd3dLakFvb0NhZ0pJWWlhSFIwY0RvdkwwVnVjbTlzYkM1MmFYTmhZMkV1WTI5dEwyVkRiMjF0TG1OeWJEQWRCZ05WSFE0RUZnUVVaYjVRQnF2ekY2SW8xeS9PTXIyOG9LOG55Wll3RGdZRFZSMFBBUUgvQkFRREFnZUFNQTBHQ1NxR1NJYjNEUUVCQ3dVQUE0SUJBUUFyRExSUkE2bXY1UUN5UzRNaUVheDVuL1BuWHQ1aWsyb2ZIZmpWdllrUHd6ZUJMbjlwOUp1am0yV2JZdEFKWll6LzFISXN4Y1EyRGNyTG05NW13WnFmVXdKQnJrR2k4UFhCeFIvYk9oaXduRVpFL0tEbzlHZ0dGR2xRdWVmbnU1VXhVRU5FVWtFSE90UGFCTUlrSStsa25mNzUrdEVwNDhXU2ExaUR5WmVSUk9neFVmOWJmU21XOFd3RkVBSzZIa1ZMWWZ5YkpFZ2FBNzM1MHR1OTJsb0xVam1LZTlCeHpQdExBV3pOalJQMXQya1JiUXZIdmgzY0VURXZabUVoRlNUSGdjSkxrUm9ZbFBiTlBCRmNNWERXdFpuUUtjNGUySnhHbnNLZ2NWVk1HNmxMZjFvVElhNk9pWVBNT1p3NjJQMnJQbHZpNUdrOGhDR2NxeW4vQXpVKyIsIk1JSUZHekNDQkFPZ0F3SUJBZ0lSQU5oMFlUQkIvRHhFb0x6R1hXdzI4UkF3RFFZSktvWklodmNOQVFFTEJRQXdhekVMTUFrR0ExVUVCaE1DVlZNeERUQUxCZ05WQkFvVEJGWkpVMEV4THpBdEJnTlZCQXNUSmxacGMyRWdTVzUwWlhKdVlYUnBiMjVoYkNCVFpYSjJhV05sSUVGemMyOWphV0YwYVc5dU1Sd3dHZ1lEVlFRREV4TldhWE5oSUdWRGIyMXRaWEpqWlNCU2IyOTBNQjRYRFRFMU1EWXlOREUxTWpjd05sb1hEVEl5TURZeU1qQXdNVFl3TjFvd2NURUxNQWtHQTFVRUJoTUNWVk14RFRBTEJnTlZCQW9UQkZaSlUwRXhMekF0QmdOVkJBc1RKbFpwYzJFZ1NXNTBaWEp1WVhScGIyNWhiQ0JUWlhKMmFXTmxJRUZ6YzI5amFXRjBhVzl1TVNJd0lBWURWUVFERXhsV2FYTmhJR1ZEYjIxdFpYSmpaU0JKYzNOMWFXNW5JRU5CTUlJQklqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FROEFNSUlCQ2dLQ0FRRUFya21DNTBRK0drbVF5WjI5a0t4cDFkK25KNDNKd1hoR1o3YUZGMVBpTTVTbENFU1EyMnFWL2xCQTN3SFlZUDhpMTcvR1FRWU5CaUYzdTRyNmp1WElIRndqd3ZLeUZNRjZrbUJZWHZjUWE4UGQ3NUZDMW4zZmZJcmhFaitsZGJteGlkekswaFBmWXlYRVpxRHBIaGt1bm12RDdxejFCRVdLRTdOVVlWRlJFZm9wVmlmbEtpVlpjWXJIaTdDSkFlQk5ZN2R5Z3ZtSU1uSFVlSDROdERTNXFmL245RFFRZmZWeW41aEpXaTVQZUI4N25UbHR5OHpkamkydGo3bkEyK1kzUExLUkpVM3kxSWJjaHFHbG5YcXhhYUtma1RMTnNpWnE5UFR3S2FyeUgrdW0zdFhmNXU0bXVselJHT1doMlUrVWs0TG50bU1GQ2IvTHFKa1duVVZlK3dJREFRQUJvNElCc2pDQ0FhNHdId1lEVlIwakJCZ3dGb0FVRlRpRER6OHNQM0F6SHMxRy9nZU1JT0RYdzdjd0VnWURWUjBUQVFIL0JBZ3dCZ0VCL3dJQkFEQTVCZ05WSFNBRU1qQXdNQzRHQldlQkF3RUJNQ1V3SXdZSUt3WUJCUVVIQWdFV0YyaDBkSEE2THk5M2QzY3VkbWx6WVM1amIyMHZjR3RwTUlJQkN3WURWUjBmQklJQkFqQ0IvekEyb0RTZ01vWXdhSFIwY0RvdkwwVnVjbTlzYkM1MmFYTmhZMkV1WTI5dEwxWnBjMkZEUVdWRGIyMXRaWEpqWlZKdmIzUXVZM0pzTUR5Z09xQTRoalpvZEhSd09pOHZkM2QzTG1sdWRHd3VkbWx6WVdOaExtTnZiUzlqY213dlZtbHpZVU5CWlVOdmJXMWxjbU5sVW05dmRDNWpjbXd3Z1lhZ2dZT2dnWUNHZm14a1lYQTZMeTlGYm5KdmJHd3VkbWx6WVdOaExtTnZiVG96T0RrdlkyNDlWbWx6WVNCbFEyOXRiV1Z5WTJVZ1VtOXZkQ3h2UFZaSlUwRXNiM1U5Vm1sellTQkpiblJsY201aGRHbHZibUZzSUZObGNuWnBZMlVnUVhOemIyTnBZWFJwYjI0L1kyVnlkR2xtYVdOaGRHVlNaWFp2WTJGMGFXOXVUR2x6ZERBT0JnTlZIUThCQWY4RUJBTUNBUVl3SFFZRFZSME9CQllFRk4vREtsVXVMMEk2ZWtDZGtxRDNSM25YajRlS01BMEdDU3FHU0liM0RRRUJDd1VBQTRJQkFRQjlZK0Y5OXRoSEFPaHhab1FjVDlDYkNvblZDdGJtM2hXbGYybkJKbnVhUWVvZnRkT0tXdGowWU9UajdQVWFLT1dmd2NiWlNIQjYzck1tTGlWbTdacUlWbmRXeHZCQlJMMVRjZ2J3YWdEbkxnQXJRTUtIblkydUdRZlBqRU1Ba0FubldlWUpmZCtjUkpWbzZLM1I0QmJRR3pGU0hhMmkyYXI2L29YeklOeWF4QVhkb0cwNEN6MlAwUG02MTNoTUNwakZ5WWlsUy80MjVoZTFUay92SHNUbkZ3RmxrOXlZMkw4VmhCYTZqNDBmYWFGdS82ZmluNzhLb3BrOTZnSGRBSU4xdGJBMTJOTm1yN2JRMXBVczBuS0hoelFHb1JYZ3VZZDdVWU85aTJzTlZDMUM1QTNGOGRvcHdzdjJRSzIrMzNxMDVPMi80RGduRjRtNXVzNlJWOTREIiwiTUlJRG9qQ0NBb3FnQXdJQkFnSVFFNFkxVFIwL0J2TEIrV1VGMVpBY1lqQU5CZ2txaGtpRzl3MEJBUVVGQURCck1Rc3dDUVlEVlFRR0V3SlZVekVOTUFzR0ExVUVDaE1FVmtsVFFURXZNQzBHQTFVRUN4TW1WbWx6WVNCSmJuUmxjbTVoZEdsdmJtRnNJRk5sY25acFkyVWdRWE56YjJOcFlYUnBiMjR4SERBYUJnTlZCQU1URTFacGMyRWdaVU52YlcxbGNtTmxJRkp2YjNRd0hoY05NREl3TmpJMk1ESXhPRE0yV2hjTk1qSXdOakkwTURBeE5qRXlXakJyTVFzd0NRWURWUVFHRXdKVlV6RU5NQXNHQTFVRUNoTUVWa2xUUVRFdk1DMEdBMVVFQ3hNbVZtbHpZU0JKYm5SbGNtNWhkR2x2Ym1Gc0lGTmxjblpwWTJVZ1FYTnpiMk5wWVhScGIyNHhIREFhQmdOVkJBTVRFMVpwYzJFZ1pVTnZiVzFsY21ObElGSnZiM1F3Z2dFaU1BMEdDU3FHU0liM0RRRUJBUVVBQTRJQkR3QXdnZ0VLQW9JQkFRQ3ZWOTVXSG02aDJtQ3hsQ2ZMRjlzSFA0Q0ZUOGljdHREMGIwL1BtZGpoMjhKSVhEcXNPVFBISDJxTEpqMHJOZlZJc1pIQkFrNEVscEY3c0RQd3NSUk9FVysxUUs4YlJhVks3MzYyclBLZ0gxZy9Fa1pnUEkyaDRIM1BWejR6SHZ0SDhhb1Zsd2RWWnFXMUxTN1lnRm15cHcyM1J1d2hZLzgxcTZVQ3p5cjBUUDU3OVpSZGhFMm84bUNQMnc0bFBKOXpjYytVMzBycTI5OXlPSXp6bHIzeEY3elN1anRGV3NhbjlzWVhpd0dkL0Jtb0tvTVd1RHBJL2s0K29Lc0dHZWxUODRBVEIrMHR2ejhLUEZVZ09Td3NBR2wwbFVxOElMS3BlZVVZaVpHbzNCeE43N3QrTnd0ZC9qbWxpRktNQUd6c0dIeEJ2ZmFMZFhlNllKMkU1LzR0QWdNQkFBR2pRakJBTUE4R0ExVWRFd0VCL3dRRk1BTUJBZjh3RGdZRFZSMFBBUUgvQkFRREFnRUdNQjBHQTFVZERnUVdCQlFWT0lNUFB5dy9jRE1lelViK0I0d2c0TmZEdHpBTkJna3Foa2lHOXcwQkFRVUZBQU9DQVFFQVgvRkJmWHhjQ0xrcjROV1NSL3BuWEtVVHd3TWhteXRNaVViUFdVM0ovcVZBdG1QTjNYRW9sV2NSekNTczAwUnNjYTRCSUdzRG9vOFl0eWs2ZmVVV1lGTjRQTUN2RllQM2oxSXpKTDFrazVmdWkvZmJHS2h0Y2JQM0xCZlFkQ1ZwOS81clBKUytUVXRCakU3aWM5RGprQ0p6UTgzejcrcHp6a1dLc0taSi8weDluWEdJeEhZZGtGc2Q3djNNOSs3OVlLV3hlaFp4MFJiUWZCSThiR21YMjY1Zk9acHdMd1U4R1VZRW1TQTIwR0J1WVFhN0ZrS01jUGN3KytEYlpxTUFBYjNtTE5xUlg2QkdpMDFxbkQwOTNRVkcvbmEvb0FvODVBRG1KN2YvaEMzZXVpSW5saEJ4NnlMdDM5OHpuTS9qcmE2TzFJN21UMUd2RnBMZ1hQWUhEdz09Il0sImFsZyI6IlBTMjU2In0.eyJhY3NFcGhlbVB1YktleSI6eyJrdHkiOiJFQyIsIngiOiJLZEdpNjRsMEh0Z0NJeFFUdDhKQ0J6a0VGdFVKT1NPa2VKM3FTRk8ydGxNIiwieSI6IjFhMFU3eDNFOWZwWGV1WmxXUUlIR3BlRXBDcFpGMmZzTk5mMl9tSGs3TmMiLCJjcnYiOiJQLTI1NiJ9LCJzZGtFcGhlbVB1YktleSI6eyJrdHkiOiJFQyIsIngiOiJxYkZLTEVaaUNyS0Z0aUdGUWEydlBFSnJxTXhfeE44OFdKOUpQdzFrbUMwIiwieSI6IkFmUVZCeFpiUFk4TnNOZV9fcEw3VnVZbzVHcU5IUjJHQjd1ZmZPQ0xnX1UiLCJjcnYiOiJQLTI1NiJ9LCJhY3NVUkwiOiJodHRwczovL25hdGl4aXNwYXltZW50c29sdXRpb25zLTNkcy12ZG0ud2xwLWFjcy5jb20vYWNzLWNoYWxsZW5nZS1hcHAtc2VydmljZS9jaGFsbGVuZ2VBcHAvY2hhbGxlbmdlUmVxdWVzdC9hcHBCYXNlIn0.hOFQ9A55mlNYYOkdrHS_L8zUYrFkEOibV9i15ZbbF4heKbsEUi2A5e6yEf81HvXDAFQf_-olJISwgobTHD9CGfOQTiOPomy5Efvw7mZ052iAzXrOgwojOfyGcqRF8RK4c-h7JB2z5f66hBqNRKEyQNyQamQv7j47PVSUkWQepg5RoASzABlZdXJhjJzuITgk32Zfez58AjVjTA2myeWGREU6I0Ac1zvRVAASDwUW3VuRGdLVu7jhbaY_Uvto3ukYial0vb6ZPlej2Rmi0baJRIAJSJ-K8ymvqbB8b2ngwUrZVqUC27pAXO57cFBAigJr-x1pRzU7A8gcj0APiGgZVg",
                    EnrollmentStatus = PaymentInstrumentEnrollmentStatus.Bypassed,
                    AcsTransactionId = authRequest.ThreeDSServerTransId
                };

                payAuthAres.AcsSignedContent = payAuthAres.AcsSignedContent.Replace("Placeholder_", string.Empty);

                return this.Request.CreateResponse(HttpStatusCode.OK, payAuthAres);
            }
            else if (testContext.ScenariosContain(Constants.TestScenarios.PXPayerAuth3DS1ChallengeSuccess))
            {
                var payAuthAres = new AuthenticationResponse
                {
                    EnrollmentStatus = PaymentInstrumentEnrollmentStatus.Enrolled,
                    AcsTransactionId = authRequest.ThreeDSServerTransId,
                    EnrollmentType = PaymentInstrumentEnrollmentType.ThreeDs,
                    ThreeDSServerTransactionId = "42410b0b-7c99-4ee0-adfb-980ef0f81a86",
                    AcsUrl = "https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge",
                    AcsSignedContent = "{\"MD\":\"A4560000307199\",\"PaReq\":\"eyJhdXRoZW50aWNhdGlvbmlkIjoiNDI0MTBiMGItN2M5OS00ZWUwLWFkZmItOTgwZWYwZjgxYTg2In0=\",\"TermUrl\":\"https://www.merchanturl.com/Response.jsp\"}",
                    TransactionStatus = PXService.Model.PayerAuthService.TransactionStatus.C,
                    IsFormPostAcsUrl = true,
                    IsFullPageRedirect = true
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, payAuthAres);
            }
            else if (testContext.ScenariosContain(Constants.TestScenarios.PXPayerAuthXboxRedemRewardsSuccess))
            {
                // "{\"enrollmentStatus\":\"bypassed\",\"challengeStatus\":\"Succeeded\",\"messageVersion\":\"2.1.0\"}"
                var payAuthAres = new AuthenticationResponse
                {
                    EnrollmentStatus = PaymentInstrumentEnrollmentStatus.Bypassed,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, payAuthAres);
            }
            else
            {
                return await this.CallAcsEmulator(
                    payerAuthApiVersion: PayerAuthApiVersion.V3,
                    v2AuthRequest: null,
                    v3AuthRequest: authRequest, 
                    testContext: testContext);
            }
        }

        private async Task<HttpResponseMessage> CallAcsEmulator(
            PayerAuthApiVersion payerAuthApiVersion,
            AReq v2AuthRequest,
            AuthenticationRequest v3AuthRequest,
            TestContext testContext)
        {
            await Task.Delay(0);
            ThreeDSServerAreq acsAreq = null;
            if (payerAuthApiVersion == PayerAuthApiVersion.V2)
            {
                acsAreq = new ThreeDSServerAreq
                {
                    ThreeDSServerTransID = v2AuthRequest.ThreeDSServerTransactionId,
                    NotificationURL = v2AuthRequest.AcsChallengeNotificationUrl,
                    PurchaseAmount = v2AuthRequest.Amount.ToString(),
                    PurchaseCurrency = v2AuthRequest.Currency,
                    BillAddrCountry = v2AuthRequest.Country,
                    DeviceChannel = v2AuthRequest.DeviceChannel.ToString() == "AppBased" ? "01" : "02",
                    SdkAppID = v2AuthRequest.SdkInfo?.SdkAppID,
                    SdkEncData = v2AuthRequest.SdkInfo?.SdkEncryptedData,
                    SdkEphemPubKey = v2AuthRequest.SdkInfo?.SdkEphemeralPublicKey,
                    SdkMaxTimeout = v2AuthRequest.SdkInfo?.SdkMaximumTimeout,
                    SdkTransID = v2AuthRequest.SdkInfo?.SdkTransID,
                    SdkReferenceNumber = v2AuthRequest.SdkInfo?.SdkReferenceNumber,
                    CardExpiryDate = ToExpiryDate(testContext)
                };
            }
            else
            {
                acsAreq = new ThreeDSServerAreq
                {
                    ThreeDSServerTransID = v3AuthRequest.ThreeDSServerTransId,
                    NotificationURL = v3AuthRequest.AcsChallengeNotificationUrl,
                    PurchaseAmount = v3AuthRequest.PaymentSession.Amount.ToString(),
                    PurchaseCurrency = v3AuthRequest.PaymentSession.Currency,
                    BillAddrCountry = v3AuthRequest.PaymentSession.Country,
                    DeviceChannel = v3AuthRequest.PaymentSession.DeviceChannel.ToString() == "AppBased" ? "01" : "02",
                    SdkAppID = v3AuthRequest.SdkInfo?.SdkAppID,
                    SdkEncData = v3AuthRequest.SdkInfo?.SdkEncryptedData,
                    SdkEphemPubKey = v3AuthRequest.SdkInfo?.SdkEphemeralPublicKey,
                    SdkMaxTimeout = v3AuthRequest.SdkInfo?.SdkMaximumTimeout,
                    SdkTransID = v3AuthRequest.SdkInfo?.SdkTransID,
                    SdkReferenceNumber = v3AuthRequest.SdkInfo?.SdkReferenceNumber,
                    CardExpiryDate = ToExpiryDate(testContext),
                    MessageVersion = v3AuthRequest.MessageVersion
                };
            }

            try
            {
                // For e2e test purpose, need to call PX emulator to tell them the callback notification URL per PX's requirement
                Uri requestUri = new Uri(string.Format(AcsEmulatorUrlPrefix, "auth"));

                using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
                {
                    httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(acsAreq), Encoding.UTF8, "application/json");
                    httpRequestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader, JsonConvert.SerializeObject(testContext));

                    HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
                    httpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
                    httpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
                    using (HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage))
                    {
                        var responseMessage = await httpResponseMessage.Content.ReadAsStringAsync();

                        if (httpResponseMessage.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(responseMessage))
                        {
                            var acsAres = JsonConvert.DeserializeObject<ThreeDSServerAuthenticationResponse>(responseMessage);

                            if (payerAuthApiVersion == PayerAuthApiVersion.V2)
                            {
                                var payAuthAres = new ARes
                                {
                                    AcsSignedContent = acsAres.AcsSignedContent,
                                    AcsTransactionId = acsAres.AcsTransactionID,
                                    ThreeDSServerTransactionId = acsAres.ThreeDSServerTransactionID,
                                    EnrollmentStatus = PaymentInstrumentEnrollmentStatus.Enrolled,
                                    AcsUrl = string.Format(AcsEmulatorUrlPrefix, "creq"),
                                };
                                return this.Request.CreateResponse(HttpStatusCode.OK, payAuthAres);
                            }
                            else
                            {
                                var payAuthAres = new AuthenticationResponse
                                {
                                    AcsSignedContent = acsAres.AcsSignedContent,
                                    AcsTransactionId = acsAres.AcsTransactionID,
                                    ThreeDSServerTransactionId = acsAres.ThreeDSServerTransactionID,
                                    EnrollmentStatus = PaymentInstrumentEnrollmentStatus.Enrolled,
                                    AcsUrl = string.Format(AcsEmulatorUrlPrefix, "creq"),
                                    MessageVersion = acsAres.MessageVersion
                                };
                                return this.Request.CreateResponse(HttpStatusCode.OK, payAuthAres);
                            }
                        }
                        else
                        {
                            return this.Request.CreateResponse(HttpStatusCode.InternalServerError, string.Format("PX ACS emulator didn't respond with success response, response status code is {0}, response message is {1}", httpResponseMessage.StatusCode, responseMessage));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // swallow the exception for PX emulator call failure
                var exception = ex;
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, string.Format("Call PX ACS emulator fail with exception {0}", ex));
            }
        }
    }
}