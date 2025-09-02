# Single Market Directive - Commercial

Scenario [36743213](https://microsoft.visualstudio.com/OSGS/_workitems/edit/36743213): Payments -PXS SMD Commercial

## Summary
  * Single Market Directive (SMD) is a legal requirement from the European Union that is applicable to all online merchants who sell products and services to EU customers.
  * The goal of SMD is to ensure that customers within the EU are treated the same no matter which EU country they belong to with respect to their payment instruments.
  * Customers can use a payment instrument (credit or debit card) from a different EU country than the one they reside in, and merchants must allow that.
  * For example, a French customer who has a Belgian debit card must be allowed to use that in online E-commerce portals and should be treated the same as if they used a French debit card.
  * SMD requirement applies to all Microsoft Storefronts including commercial Storefronts such as Azure.
  * In Azure, the signup and manage scenarios need to support SMD (such as, add and list PI).

## SMD Behavior
### Add PI

  * Enable country dropdown in UX when partner sends 'AADSupportSMD' flight in flight header (x-ms-flight).  Country dropdown should contain only the SMD countries.
  * During Add PI, for MSA users and for Guest flow, Azure will send a flight 'DisableSMD'.  When 'DisableSMD' is sent, PX/PIDL SDK should not enable the country drop down.  'DisableSMD' overrides 'AADSupportSMD'.
  * Currently, during Add PI, if employee profile doesn't exist or if it doesn't have address, PX/PIDL SDK creates/updates employee profile with the address entered as credit card address.\
  For a SMD country, the behavior would be:
    * If user doesn't change the credit card country, create/update the employee profile with credit card address.
    * If user pick's a different country as credit card country ,  create/update the employee profile with just tenant country (without complete address) i.e. the country passed in the flight passed by partner with prefix 'originCountry_' (e.g., originCountry_es).

### List PI

  * Flight headers 'AADSupportSMD' and 'DisableSMD' from partners need to be passed to PIMS as required (Add PI, List PI) 

## Flighting strategy
  * Currently, SMD behavior is not enabled for any of the production traffic. SMD behavior can be triggered by the partner using flights in flight header (x-ms-flight)
  * Need to come up with the flighting strategy to support UAT testing and to flight SMD behavior per country.

## Additional information
  * [Single Market Directive - High level details](https://microsoft.sharepoint.com/:p:/t/SALSAPMTeam/ES_kd9mfXLFKq-vNAZ4PX18BN-QwbMgbzdZOE35uLDqvrw?e=DYCCX6)
  * [Single Market Directive - Delivery Plan](https://microsoft.sharepoint.com/:p:/t/EUSingleMarketDirectiveCommercial.-SMDCommericalPhase2/ETiOlx7EbO1NlpPOaYl5GbQBMsuWBmLZma5ZuEN-SiMzSQ?e=mdPteh)

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:VipinS@microsoft.com;KulwSingh@microsoft.com;KowshikP@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs/scenarios).

---