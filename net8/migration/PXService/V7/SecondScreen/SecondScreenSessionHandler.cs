// <copyright file="SecondScreenSessionHandler.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Newtonsoft.Json;
    using PXInternal = Microsoft.Commerce.Payments.PXService.Model.PXInternal;

    public class SecondScreenSessionHandler
    {
        private ISessionServiceAccessor sessionServiceAccessor;

        public SecondScreenSessionHandler(
            ISessionServiceAccessor sessionServiceAccessor)
        {
            this.sessionServiceAccessor = sessionServiceAccessor;
        }

        /// <summary>
        /// Used by both Browser flow and App flow
        /// Create Payment Session Object
        /// </summary>
        /// <param name="context">QR code context values</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data session information for add cc qr code flow </returns>
        public async Task<PXInternal.QRCodeSecondScreenSession> CreateAddCCQRCodePaymentSession(
            PXInternal.QRCodeSecondScreenSession context,
            EventTraceActivity traceActivityId)
        {
            context.Id = Guid.NewGuid().ToString(); 
            context.Signature = context.GenerateSignature();
            PXInternal.QRCodeSecondScreenSession session = new PXInternal.QRCodeSecondScreenSession(context);

            if (await CallSafetyNetOperation(
                async () =>
                {
                    await this.sessionServiceAccessor.CreateSessionFromData<PXInternal.QRCodeSecondScreenSession>(
                        sessionId: context.Id,
                        sessionData: context,
                        traceActivityId: traceActivityId);
                },
                traceActivityId))
            {
                throw new ValidationException(ErrorCode.InvalidPaymentInstrumentDetails, "Create failed. Fell into safety net: " + JsonConvert.SerializeObject(GetQRCodeSafetyNetPaymentSession(context, PimsModel.V4.PaymentInstrumentStatus.Declined)));
            }

            return session;
        }

        /// <summary>
        /// Add browserInfo to stored session
        /// </summary>
        /// <param name="useCount">The browser info, collected from user request, need store in the session and used in the later call</param>
        /// <param name="context">The serialized purchase context, need store in the session and used in the later call</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <param name="status"> optional param, the pi status</param>
        /// <param name="piid"> optional param, the piid</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data BrowserFlowContext for PIDL to consume </returns>
        public async Task UpdateQrCodeSessionResourceData(
            int useCount,
            PXInternal.QRCodeSecondScreenSession context,
            EventTraceActivity traceActivityId,
            PimsModel.V4.PaymentInstrumentStatus status = PimsModel.V4.PaymentInstrumentStatus.Pending,
            string piid = null)
        {     
            context.UseCount = useCount;
            context.Status = status;
            context.PaymentInstrumentId = piid;

            if (await CallSafetyNetOperation(
                    operation: async () =>
                    {
                        await this.sessionServiceAccessor.UpdateSessionResourceData<PXInternal.QRCodeSecondScreenSession>(
                            sessionId: context.Id,
                            newSessionData: context,
                            traceActivityId: traceActivityId);
                    },
                    traceActivityId))
            {
                context.Status = PimsModel.V4.PaymentInstrumentStatus.Declined;
                throw new ValidationException(ErrorCode.InvalidPaymentInstrumentDetails, "Update failed. Fell into safety net: " + JsonConvert.SerializeObject(GetQRCodeSafetyNetPaymentSession(context, PimsModel.V4.PaymentInstrumentStatus.Declined)));
            }
            
            return;
        }

        public async Task<PXInternal.QRCodeSecondScreenSession> GetQrCodeSessionData(string sessionId, EventTraceActivity traceActivityId)
        {
            PXInternal.QRCodeSecondScreenSession paymentSession = await this.sessionServiceAccessor.GetSessionResourceData<PXInternal.QRCodeSecondScreenSession>(
                    sessionId: sessionId,
                    traceActivityId: traceActivityId);
            return paymentSession;
        }

        /// <summary>
        /// Calls a specified operation within a "safety net". That is certain types of exceptions are caught
        /// and logged.
        /// </summary>
        /// <param name="operation">The operation that could throw an exception</param>
        /// <param name="traceActivity">The event trace activity</param>
        /// <returns>
        /// True if an exception was caught. This can be used by the caller to take remedial actions.
        /// </returns>
        private static async Task<bool> CallSafetyNetOperation(
            Func<Task> operation,
            EventTraceActivity traceActivity)
        {
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException($"SafetyNet caught exception : {ex}", traceActivity);
                return true;
            }

            return false;
        }

        private static PXInternal.QRCodeSecondScreenSession GetQRCodeSafetyNetPaymentSession(
            PXInternal.QRCodeSecondScreenSession qrCodePaymentSessionData,
            PimsModel.V4.PaymentInstrumentStatus status)
        {
            var ps = qrCodePaymentSessionData;
            ps.Id = Guid.NewGuid().ToString();
            ps.Signature = ps.GenerateSignature();
            ps.Status = status;
            return ps;
        }
    }
}