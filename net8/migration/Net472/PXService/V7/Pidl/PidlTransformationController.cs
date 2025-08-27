// <copyright file="PidlTransformationController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System.Web.Http;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Tracing;

    public class PidlTransformationController : ProxyController
    {
        /// <summary>
        /// Pidl Transformation
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/PidlTransformation</url>
        /// <param name="transformationParameter" required="true" cref="object" in="body">transformation Parameter</param>
        /// <response code="200">A PidlTransformationResult object</response>
        /// <returns>A PidlTransformationResult object</returns>
        [HttpPost]
        public PidlTransformationResult<string> Post([FromBody] PidlTransformationParameter transformationParameter)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            PaymentsEventSource.Log.InstrumentManagementServiceTraceRequest(GlobalConstants.APINames.TransformPidlProperty, this.Request.RequestUri.AbsolutePath, traceActivityId);

            return PIDLTransformationFactory.TransformProperty(transformationParameter, this.ExposedFlightFeatures);
        }
    }
}