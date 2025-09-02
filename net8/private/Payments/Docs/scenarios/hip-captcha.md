# Harding payments systems 

## HIP Captcha

## Overview

This doc contains description for the HIP captcha Challenge flow 

## Scenarios
All Scenarios are true as long as the user is identified as malicious 

## User fills out the payment form with CAPTCHA solution

### User Fails captcha verfication
```mermaid
sequenceDiagram
  Portal->>PX :GET User Response
  PX->>AnomalyDetection: Call HIP (Verify Solution)
  AnomalyDetection->>HIP Service: Verify Solution
  HIP Service-->>AnomalyDetection: Failed
  AnomalyDetection-->>HIP Service: New challenge 
  HIP Service-->>AnomalyDetection: New challenge
  AnomalyDetection-->>PX: New Challenge
  PX-->>PIDLSDK: captchaId
  PIDLSDK->>Portal: Renders autofilled ADD PI form and the captcha
  Portal-->>User: Display the captcha with Payment forms (please try again)
```

### User does not complete the captcha in time
```mermaid
sequenceDiagram
  Portal->>PX : GET User Response
  PX->>AnomalyDetection: Call HIP (Verify Solution)
  AnomalyDetection->>HIP Service: Verify Solution
  HIP Service-->>AnomalyDetection: Invalid captchaId
  AnomalyDetection-->>HIP Service: New challenge 
  HIP Service-->>AnomalyDetection: New challenge
  AnomalyDetection-->>PX: New image challenge
  PX-->>PIDLSDK: new captchaId
  PIDLSDK->>Portal: Renders autofilled ADD PI form and the captcha
  Portal-->>User: Display the captcha with Payment forms (timeout)
```

### User completes the captcha and clicks submit
```mermaid
sequenceDiagram
  Portal->>PX : GET User Response
  PX->>AnomalyDetection: Call HIP (Verify Solution)
  AnomalyDetection->>HIP Service: Verify Solution
  HIP Service-->>AnomalyDetection: Solved
  AnomalyDetection-->>PX: Solved (continue flow)
  PX-->>PX: Payment Method Added
```

 For more Documentation refer -  [here](https://microsoft.sharepoint.com/teams/PaymentExperience/_layouts/OneNote.aspx?id=%2Fteams%2FPaymentExperience%2FSiteAssets%2FPayment%20Experience&wd=target%28Projects%2FCaptcha.one%7C5502CE5B-9B53-414B-92C0-22F21A5A4048%2F%29
onenote:https://microsoft.sharepoint.com/teams/PaymentExperience/SiteAssets/Payment%20Experience/Projects/Captcha.one#section-id={5502CE5B-9B53-414B-92C0-22F21A5A4048}&end)