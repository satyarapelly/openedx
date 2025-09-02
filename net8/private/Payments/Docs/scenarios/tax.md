# Tax Id

## Summary
Handles everything in PX Service involving fetching, adding, updating Tax Ids

## Example Usage
https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/TaxIdDescriptions  
Parameters:  
  - **partner:** commercialstores 
  - **operation:** "SelectInstance", "Add", or "Update"  
  - **country:** Two letter country code  
  - **language:** locale (e.g. en-us)  
  - **type:** "commercial_tax_id" or "consumer_tax_id"  
  - **scenario:** "departmentalPurchase"  
  - **allowedPaymentMethods:** _(optional)_ array of allowed payment methods  

## Tax Id types & Countries supported
List can be found in [TaxIdsInCountries.csv](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Pidl/PidlFactory/V7/Config/TaxIdsInCountries.csv)

## Code
Tax Id code entry point lives in [TaxIdDescriptionsController.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/V7/Pidl/TaxIdDescriptionsController.cs)

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:mccordmatt@microsoft.com;jiefan@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs/scenarios/tax.md).

---