# Add kakaopay for Korean market

## Target audience
PX Engineering team

## Overview
The purpose of this doc is to provide a brief explanation about how add kakaopay PI is implemented for xboxnative partners

## Prerequisites
N/A

## To what PM Family, Type and Country does this PI belong?
 PM Family: ewallet
 PM type  : kakaopay
 Market   : Korea ("kr")

## How Add Kakaopay works
- Sequence diagram
![](../images/scenarios/AddKakaopay-xboxnative/add_kakaopay_sequence_diagram.PNG)

## Scenario Overview
1. Present "Setting up Kakaopay" entry form via /paymentMethodDescriptions
1. When "Next" button is clicked, present kakaopay's qrcode page in an iframe
1. As soon as the iframe page loads it will be polling for a success or failure from the qr code challenge.
1. The user can also click the "Cancel" button to cancel the flow at this stage.
1. Once a completed challenge is detected, the pidlsdk will give a success with the pi data.

## How to bring up this flow in test app
- start the PIDL react native windows test app
- Click on "Add Payment Instrument" on the left menu
- Choose "Korea (the republic of) as country, "storify" as partner, "Ewallet" as payment Family, "kakaopay" as payment Type and click on submit

Note: If you try to run the same flow for "storify" using web test portal, the iframe would refuse to display the qrcode page with the below error:
![](../images/scenarios/AddKakaopay-xboxnative/web_test_portal_error_with_iframe.PNG)

## Setting up Kakaopay form
![](../images/scenarios/AddKakaopay-xboxnative/settingup_kakaopay.PNG)

## QrCode Challenge Page
![](../images/scenarios/AddKakaopay-xboxnative/Qrcode_iframe_page.PNG)

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:rtalluri@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20development/AddKakaopay-xboxnative.md).

---