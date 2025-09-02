# Service-level Agreements

## Target audience
Anyone that depends on PX

## Overview
Service-level agreements (SLAs) describe PX team's commitments for uptime and connectivity. SLAs for 
individual PX components are listed below.

### PX Service
Reliability is measured at the PX service REST endpoint. It is calculated as shown below measured over 
a month
```Reliability = 100 * (Incoming requests - System error responses) / Incoming requests.```

Reliability SLA for all operations except the below is **99.9%**
1.  POST /paymentInstrumentsEx for Non-Sim Mobi - 95%
2.  POST /paymentInstrumentsEx/replace - 80%

### PIDLSDK
Reliability SLA for all operations except the below is **99.9%**
1.  POST /paymentInstrumentsEx for Non-Sim Mobi - 95%
2.  POST /paymentInstrumentsEx/replace - 80%

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:kowshikpfte@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20development/sla.md).

---