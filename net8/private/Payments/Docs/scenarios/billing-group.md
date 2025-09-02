# Billing Group

## Summary
Handles everything in PX Service involving billing groups.

## Example Usage
https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/billinggroupDescriptions  
Parameters:
  - **partner:** commercialstores
  - **operation:** "SelectInstance" or "Add"
  - **country:** Two letter country code
  - **language:** locale (e.g. en-us)
  - **type:** "lightweight" or "lightweightv7"
  - **allowedPaymentMethods:** _(optional)_ array of allowed payment methods

## Code
Billing code entry point lives in [BillingGroupDescriptionsController.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/V7/Pidl/BillingGroupDescriptionsController.cs)

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:mccordmatt@microsoft.com;jiefan@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs/scenarios/billing-group.md).

---