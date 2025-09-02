# Payment Challenge

## Target audience
PX Engineering

## Overview
This doc contains description for the Payment Challenge flow that PIDL/PX supports.  

## Scenarios
### Payment Challenge Flows in India Market with 3DS
#### **Inline (redirection in same browser tab)**
1. Present CVV entry form via /challengeDescriptions
1. Present bank challenge page inline (in same browser tab)
1. Redirect to Ru (Success) or Rx (Error) url (in same browser tab) depending on challenge outcome.
1. The Success url will have some query parameters appended: piid, session id, challenge status.
  
#### **Non-Inline (Redirection in new browser tab)**
1. Present CVV entry form via /challengeDescriptions
1. Present "Verify Your Card" Page with "Open bank's website" button.
1. When "Open bank's website" button is clicked:
    1. the "Did you successfully verify your card?" page will be presented in the current browser tab.
    1. the bank's website will be open in a new tab.
1. New tab is redirected to Ru (Success) or Rx (Error) url depending on challenge outcome
1. Original browser tab with "Did you successfully verify your card?" page will be polling for a success or failure from the bank challenge.
1. The user can also click the "Yes I'm done with bank verification" button to check for a success or failure manually.
1. Once a completed challenge is detected, the pidlsdk will give a success with the session data.
  
#### **iFrame**
1. Iframe will be the default experience for partners (i.e. payin) who have not done work to open in new tab or full page redir.
1. Whether to activate iFrame over Inline or Non-Inline based on a boolean we receive from PayerAuth (`is_full_page_redirect`) is still under discussion.

## Partners
|Partner Name|Purchase Flow|
|---|---|
|amcweb|Inline|
|cart|Inline|
|webblends|Non-Inline|
|webblends_inline|Inline|