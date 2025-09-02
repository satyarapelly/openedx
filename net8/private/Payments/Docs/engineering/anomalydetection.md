# Anomaly Detection

## Target audience
PX Engineering

## Overview
Even with testing and process controls, regressions reach PROD occasionally. To reduce the impact of such regressions, we need to detect them as soon as they occur so that we can mitigate them.  Similarly, despite following SDL, services are likely to have vulnerabilities that are unknown to the engineering team.  So, its important to also monitor PROD to detect malicious activity.  This document specifies business requirements of detection of regressions and malicious activity.

## Abbreviations on this page
  - <a id="okr-objectives-and-key-results"></a>OKR - Objectives and Key Results
  - <a id="ttm-time-to-mitigate"></a>TTM - Time To Mitigate
  - <a id="ttd-time-to-detect"></a>TTD - Time to Detect

## Business requirements

### 1. System Regressions
System regression types are listed below and should be detected automatically without the need for service owners having to specify thresholds.
1.	Increase in system errors
2.	Increase in user errors
3.	Increase in system latency
4.	Drop or increase in traffic

#### 1.1 Detect in any combination of dimensions
Dimensions are listed below and the system should detect regressions even if those regressions are confined to some specific combination of these dimensions.  This detection should also not require service owners to specify thresholds.
1. Partner (Storefront)
2. Country
3. Operation (Add PI vs. Edit PI vs. PSD2 Session)
4. PI Type
5. Flight/Experiment
6. Azure Region
7. Azure Instance

**Examples**
 1. Edit PI starts returning 5XX for a significantly larger percentage of Edit PI when PI Type is Mastercard and Country is Canada and Partner is DIME. 
 2. Add PI latency increases significantly when PI Type is Citizen’s One Financing only in the Central US region.
 3. PSD2 web fingerprinting starts returning 4XX for a significantly larger percentage of users.
 4. One of the 80 servers in PX-PROD gets into a bad state and starts returning 401 Unauthorized

#### 1.1 Time to detect
[OKR](#okr-objectives-and-key-results "Objectives and Key Results") for [TTD](#ttd-time-to-detect "Time To Detect") is less than 5 minutes and [TTM](#ttm-time-to-mitigate "Time To Mitigate") is less than 60 minutes.  So, the system should detect regressions as soon as enough requests have occurred to indicate with confidence that a regression has occurred.  For a given dimension combination, detection time should be less than the amount of time it takes to get more than 30 requests on average.
  
**Example**
1. For dimension combinations with around 30 requests per minute, detect regressions within 1 minute from start of impact
2. For dimension combinations with around 30 requests per hour, detect regressions within 1 hour from start of impact

#### 1.2 On detection, create IcM incidents
On detection of a regression, create an IcM incident to the owners of the monitored service.  The system should assign severity based on rules and hints configured by service owners.

#### 1.3 On detection, report dimensions
[OKR](#okr-objectives-and-key-results "Objectives and Key Results") for [TTM](#ttm-time-to-mitigate "Time To Mitigate") is less than 60 minutes.  When the system detects a regression, it should specify dimensions and their values.  This helps on-call engineers to take action towards mitigation quickly and reduce the impact of the incident.

**Examples**
1. "Sev 2 - Increase in system errors | Partner = cart | PIType = mc | Country = CA"

#### 1.4 On detection, report data evidence
This is data to show why something was detected as a system regression.  This helps on-call engineers to 
1. Articulate why its a regression
2. Better assess / adjust severity assigned by the system 

**Example**
1. History of values (time series of error count) in a dimension
2. Normal range based on that history, and
3. The recent value which was outside that range

#### 1.5 Allow addition/removal of dimensions
As the system evolves, we may need to add more dimensions.  As an example, we may start sending PIDLSdk version to the PX service.  At that point, we may want to add this as a dimension so that regressions in specific versions of PIDLSdk can be isolated.  We should be able to add this easily with no code change (e.g. a config change) to the system.

#### 1.6 Allow addition/removal of system regression types
As the system evolves, we may need to detect more types of system regressions in addition to the 4 types mentioned above.  The system should allow addition of new detection logic in a modular manner (e.g. each detection type is a pluggable module that use the central data infrastructure).

#### 

### 2. Detect malicious activity
Detect the following types of malicious activity 
1.	Card Testing - Attempts to add a card to our system with the sole purpose of testing its validity.
2.	D/DoS Attacks – Attempts to reduce availability.
3.	Fuzzing Attacks – Attempts to send many malformed requests in hopes to identify a vulnerability in a brute force manner.

**Examples**
1. Card Testing
   1. The same user Id or the same IP address tries to add multiple different cards in quick succession and most of these fail bank validation.
   2. Multiple add PIs are attempted with the same card holder name and most of these fail bank validation.
2. D/Dos Attack 
   1. The same user Id or the same IP address sends multiple PSD2 session calls causing us to write to the database.
3. Fuzzing Attack 
   1. The same user Id or the same IP address sends multiple malformed requests in quick succession and most of these fail in 4XX or 5XX.

### 3. Detection system fundamentals
#### 3.1 Outages of detection system should not affect reliability of monitored services
#### 3.2 Detection should not add to latency of monitored services
#### 3.3 Ensure GDPR compliance

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:kowshikpfte@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20engineering/anomalydetection.md).

---