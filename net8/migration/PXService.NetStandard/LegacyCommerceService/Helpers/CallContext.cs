// <copyright file="CallContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Helpers
{
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    public class MaskXmlTextWriter : XmlTextWriter
    {
        public MaskXmlTextWriter(TextWriter tw) : base(tw) { }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            base.WriteString(string.Format("Base64 [{0}]", count));
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            base.WriteString(string.Format("BinHex [{0}]", count));
        }

        public override void WriteString(string text)
        {
            // on production, CVV encryption are 190 and CC number encryption are 222
            // 

            if (text != null && text.Length > 150)
            {
                base.WriteString(string.Format("String [{0}]", text.Length));
                return;
            }

            base.WriteString(text);
        }
    }

    public static class SerializeUtility
    {
        // used for trace only
        public static string DataContractToString(this object value)
        {
            return DataContractToString(value, value.GetType());
        }

        public static string DataContractToString(this object value, Type type)
        {
            if (value == null)
            {
                return "NULL";
            }

            var dcs = new DataContractSerializer(type);

            using (var sw = new StringWriter())
            {
                using (var xw = new MaskXmlTextWriter(sw))
                {
                    dcs.WriteObject(xw, value);
                }
                return sw.ToString();
            }
        }

        // used for trace only - no sensitive infromation to mask
        public static string ContractToStringNoMask(this object value)
        {
            return ContractToStringNoMask(value, value.GetType());
        }

        //
        public static string ContractToStringNoMask(this object value, Type type)
        {
            if (value == null)
            {
                return "NULL";
            }

            var dcs = new DataContractSerializer(type);

            using (var sw = new StringWriter())
            {
                using (var xw = new XmlTextWriter(sw))
                {
                    dcs.WriteObject(xw, value);
                }
                return sw.ToString();
            }
        }
    }

    public class CallContext : IDisposable
    {
        private string m_apiName;
        private DateTime m_startTime;
        private Exception m_exception;
        private int m_errorCode;
        private ErrorNamespace m_errorNamespace;
        private TraceLevel m_level = TraceLevel.Verbose;
        private Guid m_trackingGuid = Guid.Empty;
        private bool noMaskInTrace = false;

        private IDictionary<string, string> m_inputParameters = new Dictionary<string, string>();
        private IDictionary<string, object> m_richInputParameters = new Dictionary<string, object>();
        private IDictionary<string, string> m_outputParameters = new Dictionary<string, string>();
        private IDictionary<string, object> m_richOutputParameters = new Dictionary<string, object>();
        private IDictionary<string, string> m_additionalParameters = new Dictionary<string, string>();
        

        public ITracer Logger { get; set; }
        public Identity Requestor { get; set; }
        public Identity Delegator { get; set; }
        public string ObjectId { get; set; } 

        public CallContext(
            ITracer logger,
            Guid trackingId,
            DataAccessorType apiType
            )
            : this(logger, trackingId, apiType, null, null,null)
        { }

        public CallContext(
            ITracer logger,
            Guid trackingId,
            DataAccessorType apiType,
            bool noMaskInTrace
            )
            : this(logger, trackingId, apiType, null, null, null, noMaskInTrace)
        { }

        public CallContext(
            ITracer logger,
            Guid trackingId,
            DataAccessorType apiType,
            Identity delegatorId,
            Identity requestorId,
            string objectId
         )
            : this(logger, trackingId, apiType, delegatorId, requestorId, objectId, false)
            
        { 
        }

        public CallContext(
            ITracer logger,
            Guid trackingId,
            DataAccessorType apiType,
            Identity delegatorId,
            Identity requestorId,
            string objectId,
            bool noMaskInTrace
         )
        {
            Logger = logger;
            m_apiName = "CtpWebDataAccessor_"+apiType.ToString();
            m_trackingGuid = trackingId;
            this.Requestor = requestorId;
            this.Delegator = delegatorId;
            this.ObjectId = objectId;
            m_startTime = DateTime.Now;
            this.noMaskInTrace = noMaskInTrace;
        }

        public void Dispose()
        {
            try
            {
                DoDispose();
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                try
                {
                    this.Logger.CriticalException(ex, DataAccessErrors.DATAACCESS_E_INTERNAL_SERVER_ERROR);
                }
                catch (Exception)
                {
                    // Swallow the exception when Dispose
                }
            }
        }

        private void DoDispose()
        {
            // end the call context
            if (m_exception != null)
            {
                m_level = TraceLevel.Error;
            }
            else
            {
                m_level = TraceLevel.Verbose;

            }

            // write trace at current level
            WriteTrace(m_level);
        }

        private void WriteTrace(System.Diagnostics.TraceLevel traceLevel)
        {
            // trace context
            StringWriter s_contextField = null;
            using (StringWriter sw = new StringWriter())
            {
                XmlTextWriter xw = new XmlTextWriter(sw);

                xw.WriteStartElement("Call");
                xw.WriteAttributeString("ApiName", m_apiName);
                xw.WriteAttributeString("State", m_exception == null ? "Success" : "Fail");
                xw.WriteElementString("ST", m_startTime.ToString("MM/dd/yyyy HH:mm:ss.fff"));
                xw.WriteElementString("TimeElapsed", GetTimespanString(DateTime.Now - m_startTime));
                if (Requestor != null)
                {
                    xw.WriteStartElement("Req");
                    xw.WriteAttributeString("type", this.Requestor.IdentityType);
                    xw.WriteString(this.Requestor.IdentityValue);
                    xw.WriteEndElement();
                }
                if (this.Delegator != null)
                {
                    xw.WriteStartElement("Del");
                    xw.WriteAttributeString("type", this.Delegator.IdentityType);
                    xw.WriteString(this.Delegator.IdentityValue);
                    xw.WriteEndElement();
                }
                xw.WriteElementString("OID", this.ObjectId);
                if (m_trackingGuid != Guid.Empty)
                {
                    xw.WriteElementString("TID", m_trackingGuid.ToString());
                }

                // trace input parameters
                xw.WriteStartElement("InputParams");
                foreach (var inputParam in m_inputParameters)
                {
                    xw.WriteElementString(inputParam.Key, inputParam.Value);
                }
                foreach (var richinputParam in m_richInputParameters)
                {
                    WriteRichTrace(xw, richinputParam, this.noMaskInTrace);
                }
                xw.WriteEndElement();   // </InputParams>

                // trace output parameters
                xw.WriteStartElement("OutputParams");
                foreach (var outputParam in m_outputParameters)
                {
                    xw.WriteElementString(outputParam.Key, outputParam.Value);
                }
                foreach (var richOutputParam in m_richOutputParameters)
                {
                    WriteRichTrace(xw, richOutputParam, this.noMaskInTrace);
                }
                xw.WriteEndElement();   // </OutputParams>

                foreach (var additionalParam in m_additionalParameters)
                {
                    xw.WriteElementString(additionalParam.Key, additionalParam.Value);
                }

                xw.WriteEndElement();   // </Call>
                s_contextField = sw;
            }

            // trace exceptions
            if (m_exception != null)
            {
                int s_hrField = ConvertErrorCodeToHR(DataAccessException.MapErrorCodeByNamespace(m_errorNamespace, m_errorCode));
                string s_exceptionField = m_exception.ToString();
            }

            // TODO : Log s_contextField, s_hrField, s_exceptionField and m_apiName
        }

        public void SetException(Exception ex)
        {
            SetException(ex, ErrorNamespace.DataAccessorLayer, DataAccessErrors.DATAACCESS_E_INTERNAL_SERVER_ERROR);
        }

        /// <summary>
        /// Set exception happened in the call context. Pass null to reset call
        /// context error tracing information to empty.
        /// </summary>
        /// <param name="ex">Exception happened during processing</param>
        /// <param name="errorNamespace">error namespace</param>
        /// <param name="errorCode">Error code</param>
        public void SetException(Exception ex, ErrorNamespace errorNamespace, int errorCode)
        {
            m_exception = ex;
            m_errorCode = errorCode;
            m_errorNamespace = errorNamespace;
        }
        public void AddInputParameter(string paramName, string paramValue)
        {
            m_inputParameters[paramName] = paramValue ?? "NULL";
        }

        public void AddRichInputParameter(string paramName, object paramValue)
        {
            m_richInputParameters[paramName] = paramValue;
        }

        public void AddRichOutputParameter(string paramName, object paramValue)
        {
            m_richOutputParameters[paramName] = paramValue;
        }

        private static int ConvertErrorCodeToHR(int errorCode)
        {
            int hr;

            unchecked
            {
                hr = (int)0x80040000;
            }
            hr += errorCode;
            return hr;
        }

        private static void WriteRichTrace(XmlTextWriter xw, KeyValuePair<string, object> richOutputParam, bool noMask)
        {
            xw.WriteStartElement(richOutputParam.Key);
            if (noMask)
            {
                xw.WriteRaw(richOutputParam.Value.ContractToStringNoMask());
            }
            else
            {
                xw.WriteRaw(richOutputParam.Value.DataContractToString());
            }
            
            xw.WriteEndElement();
        }

        private static string GetTimespanString(TimeSpan time)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0:00}:{1:00}:{2:00}.{3:000}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            return sb.ToString();

        }
        #region properties
        public Exception Exception
        {
            get { return m_exception; }
        }

        #endregion
    }
}
