// <copyright file="PidlValidationController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System.Web.Http;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Tracing;

    public class PidlValidationController : ApiController
    {
        /// <summary>
        /// Pidl Validation
        /// </summary>
        /// <group>PidlValidation</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/PidlValidation</url>
        /// <param name="validationParameter" required="true" cref="object" in="body">transformation Parameter</param>
        /// <param name="language" required="false" cref="object" in="query">language code</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A PidlExecutionResult object</returns>
        [HttpPost]
        public PidlExecutionResult Post([FromBody] PidlValidationParameter validationParameter, string language = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            PaymentsEventSource.Log.InstrumentManagementServiceTraceRequest(GlobalConstants.APINames.ValidatePidlProperty, this.Request.RequestUri.AbsolutePath, traceActivityId);
            return PidlPropertyValidationFactory.ValidateProperty(validationParameter, language);
        }
    }
}