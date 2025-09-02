# Title
Adding new Properties to logs

## Target audience
PX Engineering team

## Overview
This will describe the process to add a new property to PX logs via a `.bond` file.


## Prerequisites
PX Service is fully setup on you machine and build is clean for Payments.sln.

## Steps
1. Find the relevant `.bond` file. For PX Service Geneva logs, the file lives at [private/Payments/shared/Library/Tracing/SllLogging/PXServiceEvents.bond](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX/commit/de851884273d6f39f054b000e33a818aef3c78ad?refName=refs%2Fheads%2Fusers%2Fmccordmatt%2Fpims-sticky-connections-fix)
1. Using the `PXServiceEvents.bond` file link above, look at the green highlighted change and make a similar addition to the file (in the example above I added my property to the class `PXServiceOutgoingOperation`).
	1. Add a comment describing your property
	1. Following the existing code pattern add a new ID (Increment by 10), required/optional, type, variableName.
		- **ex:** `200: optional string MyVarName`
1. Build the solution.
1. Your entry should show up in `c:\repos\sc.cspayments.px\private\shared\Library\Tracing\SllLogging\obj\Debug\PXServiceEvents_types.cs` (file not checked in to git) after the build.
1. You can now use this property in your logging: [example here](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX/pullrequest/7629161?_a=files&path=/private/Payments/Common/Web/SllWebLogger.cshttps://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX/commit/335723893b412e502777b2be11a7d4e54d3fd10c?refName=refs%2Fheads%2Fusers%2Fmccordmatt%2Fpims-sticky-connections-fix)  



---
For questions/clarifications, email [author/s of this doc and PX support](mailto:mccordmatt@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20operations/adding-log-properties.md).
<!--
- Replace "mccordmatt" with the document owner's alias
- Replace "development/doc-template.md" with the actual file name
-->

---