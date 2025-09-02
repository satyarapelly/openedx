# Address Reduction (Address with no city and state)

## Scenario name

addressnocitystate

## Summary

In the add PI flow, we try to improve the user experience by reducing number of fields in PI form. In the addressnocitystate scenario, we don't ask user to enter city and state information.

## Enabled markets

US. No plan to support more markets yet.

## Enabled partners

officeoobe, oxooobe, webblends.
Here is a sample [PR](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX/commit/150d2ba636b6244a8d876d68aef7edf81b8da079?refName=refs/heads/master&path=/private/Payments/Pidl/PidlFactory/V7/Config/DisplayDescriptions/officeoobe/ContainerDisplayHints.csv) for future partner enables!

## Enabled PI Family and Type

Family: credit_card

Type: visa, mc, amex and discover

## Current status

The feature is in the experimental stage. We don't have any production traffic hitting the feature. If you plan to direct any prod traffic to the flow, please engage [pxsupport](mailto:PXSupport@microsoft.com&subject=enable/addressnocitystate).

---

For questions/clarifications, email [author/s of this doc and PX support](mailto:jiefan@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20scenarios/addressnocitystate).

---
