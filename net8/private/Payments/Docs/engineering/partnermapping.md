# Partner Mapping Documentation

## Target audience
PX Engineering and Payments PM teams

## Overview
This doc explains why we are mapping the partners and how we are doing it.

## Why Partner Map?
Since several partners may be returning the same pidl, we are mapping partners to a single variable to get that pidl and apply similarly needed functionality.  For Example, “xboxnative” is used to get the same pidl for “xboxsubs”, “xboxsettings”, “saturn”, etc.  This is done to simplify the task of adding partners that return the same pidl and pass through similar logic.

## How to map a partner
To achieve partner mapping, let's declare our functionality* in
- **C:\repos\SC.CSPayments.PX\private\Payments\Pidl\PXCommon\Constants.cs**
	- moves partner-specific logic to config
	- used in PXService and PidlFactory to redirect/route partners with similar functionality


*Partner Mapping functions are formatted “IsXboxNativePartner”, “IsWindowsNativePartner”, etc


---
For questions/clarifications, email [author/s of this doc and PX support](mailto:v-willkencel@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20engineering/partnermapping.md).

---