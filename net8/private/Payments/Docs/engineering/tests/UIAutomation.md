# UI Automation

## Target Audience
PX Engineering

## Overview
UI Automation is a set of selenium tests that verify basic functionality across environments.

## Initial Setup
- Chrome is highly recommended to run them, but they can run on other browsers too.
- Under `C:\PSD2`, create `TestAccount.json` file with the following contents ([Password](https://portal.azure.com/#@mspmecloud.onmicrosoft.com/asset/Microsoft_Azure_KeyVault/Secret/https://px-kv-int.vault.azure.net/secrets/ppepsd2userpassword)):
```
{
"TestAccount": "mstest_psd2_1@outlook.com",
"TestPassword": "<PasswordPlaceholder>"
}
```

- Clone the [pidl sdk](https://microsoft.visualstudio.com/Universal%20Store/_git/pay.pidl.sdk) repo.
- Open the [AutomationTests](https://microsoft.visualstudio.com/Universal%20Store/_git/pay.pidl.sdk?path=/tests/AutomationTest) project.
- Make sure the project builds.

## How to run
- Pull latest from master.
- Build solution and run tests.
- All but 4 tests should pass. These tests need manual intervention.
    - On edge, open https://businessstore.microsoft.com/en-us/store?signin= and login with User1@vanubancommercial.onmicrosoft.com [password](https://portal.azure.com/#@mspmecloud.onmicrosoft.com/asset/Microsoft_Azure_KeyVault/Secret/https://px-kv-int.vault.azure.net/secrets/StoreForBusinessAccountPassword)
    - Open devtools and go to the network tab.
    - Filter calls to `https://businessstore.microsoft.com/en-us/reco`, select any of those calls and in the headers tab, copy the authorization request header token.
    - Open HandlePaymentChallengePaymentCaid.json and paste the token.
    - Open HandlePaymentChallengeRecurringCaid.json and paste the token.
    - Open ListAzurePI.json and paste the token.
    - Open CreditCardAzurePartner.json and paste the token.
    - re-run the failing tests.
  
---
For questions/clarifications, email [author/s of this doc and PX support](mailto:holugo@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20engineering/tests/cits.md).

---