# PX Client

## Target Audience
PX Engineering

## Overview
This document describes problems/inefficiencies that exist today with PIDLSDK.  It also describes goals and design proposals to solve them.

## Problems / Inefficiencies

  
  - No native support for features like 
    - "Edit Address" checkbox
    - "12/22" to be parsed out into MM and YY
    - Autofill City and State based on Zipcode 
  - No way to customize styles when using Office-Fabric or React-Classic element factories
    - Office-Fabric and React-Classic elementry factories support style and theme parameters but these are not plumbed all the way through.  Hence, partner cannot customize styles.

Because of the above gaps, we currently need a separate set of components for each portal e.g. AMC, Admin Center, Azure Portal.


---
For questions/clarifications, email [author/s of this doc and PX support](mailto:JorLede@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20engineering/pxclient.md).

---