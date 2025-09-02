# List Dynamics 365 pending orders in North Star Page

## Summary
Customer access their Payment Methods page from AMC/Manage your payments to remove a Payment Instrument (PI). Payments then calls D365 to determine if the PI the customer is attempting to remove is associated with an Active Order. If  returns  Active Order(s) for the PI customer wants to remove, then the action will be blocked.
In the North Star experience in the Payments AMC page, during remove PI, Payments calls M$ to get details of pending/active orders on a PI to display to the user. With Dynamics going live, Payments will need to call Dynamics OMS in addition to M$ to get details of pending/active orders on a PI (pre-orders, physical goods) 


## Workflow
![List Dynamics 365 pending orders Overview](../images/scenarios/listD365PendingOrders.PNG)

## Dynamics 365 Service URLs
| Environment       | Url|
| ---------------- | ----------------- | 
| Prod       | https://orders.production.store-web.dynamics.com | 
| Ppe        | https://orders.ppe.store-web.dynamics.com | 

## APIs

### Get Pending Orders Ids For A Payment Instrument
**Request Url**
*GET*  /v1.0/users/{userId}/Orders/paymentinstrumentcheck/{piid}

**Input Parameters**

| Name             | Type              | Required | description | 
| ---------------- | ----------------- | ------------------- | ------- | 
| msa:<PUID>       | string            | Yes | The MSA puid |
| piid | string | Yes  | The payment instrument id |

**Request**
None

**Response**
A string JSON array with the list of active/pending orders. [Response Schema](https://orders.production.store-web.dynamics.com/swagger/index.html#/Orders/Orders_GetOrders)
```
{
  "pendingOrderIds": [
    "111_1234",
    "222_1234"
  ]
}

```

### Get An Order

**Request Url**
*GET*   /v1.0/users/{userId}/Orders?orderId={orderId} 

**Input Parameters**

| Name             | Type              | Required | description | 
| ---------------- | ----------------- | ------------------- | ------- | 
| msa:<PUID>       | string            | Yes | The MSA puid |
| orderId | string | No  | The order id |

**Request**
None

**Response**
A string JSON array with the list of orders. [Response Schema](https://orders.production.store-web.dynamics.com/swagger/index.html#/Orders/Orders_GetOrders)
```
{
    "items": [
        {
            "puid": 985154870314646,
            "cid": "",
            "receiptEmail": "mstest_pims.d365.test01@outlook.com",
            "orderId": "9W8E-fcGvZ~AFtwuqrFx02FJAzaVq-ga",
            "displayOrderId": "ZR00123808",
            "salesOrderId": "USA-0002019509",
            "orderLineItems": [
                {
                    "lineItemId": "dde3cf36-5be1-4320-a857-63fdf4200efa:US",
                    "lineItemState": "PendingFulfillment",
                    "redeemedOrderId": "",
                    "quantityItems": [
                        {
                            "quantityItemId": "1.0000000000000000",
                            "quantityItemState": "PendingFulfillment",
                            "omniChannelFulfillmentLineId": null,
                            "mcapiFulfillmentId": "",
                            "billingState": "Authorized",
                            "fulfillmentState": "None",
                            "rmaId": "",
                            "tokenIdentifier": null,
                            "canceledReason": "",
                            "canceledDate": null,
                            "returnedReason": "",
                            "returnedDate": null,
                            "warrantySalesLineId": "",
                            "isCancellable": true,
                            "estimatedDeliveryDate": "2021-06-09T12:00:00+00:00",
                            "renderEDD": true,
                            "renderEDDOverride": false
                        }
                    ],
                    "paymentDetails": {
                        "remitBy": "reseller_ms_remit",
                        "payments": [
                            {
                                "paymentInstrumentId": "w49NkgAAAAABAACA",
                                "totalAmount": 0.0,
                                "isPrimary": true
                            }
                        ]
                    },
                    "bundledCatalogs": [],
                    "catalog": {
                        "productId": "8N17J0M5ZZQS",
                        "skuId": "08ZH",
                        "sapSkuId": "VDV-00001",
                        "displayRank": 0,
                        "market": "US",
                        "language": "en-us",
                        "title": "Surface Pro 7 - Platinum, Intel Core i5, 8GB, 128GB",
                        "bundleTitle": "Surface Pro 7",
                        "productFamily": "Physical",
                        "productType": 1,
                        "listPrice": 899.99,
                        "msrp": 899.99,
                        "imageUrl": "https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE4tjV5?ver=eab4&w=150&h=150&q=60&m=6&o=f",
                        "imageBackgroundUrl": null,
                        "bundleImageUrl": "https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE4tjV5?ver=eab4&w=150&h=150&q=60&m=6&o=f",
                        "bundleImageBackgroundUrl": null,
                        "pdpUrl": "https://www.microsoft.com/en-us/p/surface-pro-7-platinum-intel-core-i5-8gb-128gb/8n17j0m5zzqs/08zh",
                        "brandId": null,
                        "subscriptionPartnerId": null,
                        "isRental": false,
                        "isPreorder": false,
                        "preOrderReleaseDate": null,
                        "publisherId": null,
                        "publisherName": null,
                        "developerName": null,
                        "paymentInstrumentExclusionFilterTags": [],
                        "paymentInstrumentInclusionFilterTags": [],
                        "redemptionUrlTemplate": null,
                        "musicDetails": null,
                        "videoDetails": null,
                        "installationTerms": null,
                        "expiringDownloadOptions": [],
                        "estimatedDeliveryOverlayMessage": null,
                        "bundleProductSkuRankMap": {
                            "8N17J0M5ZZQS/7GKZ": 1000
                        }
                    },
                    "purchaseChargeDetails": {
                        "quantityItemIds": [],
                        "itemCostDetails": {
                            "totalAmount": 990.890,
                            "totalTaxAmount": 90.900,
                            "taxDetails": [
                                {
                                    "taxType": "Generic",
                                    "taxAmount": 90.900
                                }
                            ]
                        },
                        "shippingCostDetails": {
                            "totalAmount": 0.0,
                            "totalTaxAmount": 0.0,
                            "taxDetails": []
                        },
                        "extraCostDetails": {
                            "extraCosts": [],
                            "totalAmount": 0.0,
                            "totalTaxAmount": 0.0,
                            "taxDetails": []
                        },
                        "totalAmount": 990.890,
                        "totalTaxAmount": 90.900
                    },
                    "actualChargeDetails": {
                        "quantityItemIds": [],
                        "itemCostDetails": {
                            "totalAmount": 0.0,
                            "totalTaxAmount": 0.0,
                            "taxDetails": []
                        },
                        "shippingCostDetails": {
                            "totalAmount": 0.0,
                            "totalTaxAmount": 0.0,
                            "taxDetails": []
                        },
                        "extraCostDetails": {
                            "extraCosts": [],
                            "totalAmount": 0.0,
                            "totalTaxAmount": 0.0,
                            "taxDetails": []
                        },
                        "totalAmount": 0.0,
                        "totalTaxAmount": 0.0
                    },
                    "fullRefunds": [],
                    "partialRefunds": [],
                    "quantity": 1,
                    "cancellableQuantity": 1,
                    "refundableQuantity": 0,
                    "returnableQuantity": 0,
                    "isEligibleForUpdatePayment": true,
                    "isTaxIncluded": false,
                    "taxGroup": "TaxesNotIncluded",
                    "documentId": null,
                    "enfLink": null,
                    "fulfillmentDate": null,
                    "buildToOrderDetails": null,
                    "beneficiaryId": null,
                    "gifteeRecipientId": null,
                    "recurrenceId": "",
                    "bundleId": "",
                    "bundleInstanceId": "",
                    "bundleSlotType": 0,
                    "psa": "8N17J0M5ZZQS/08ZH/8W4DDTZXC49M",
                    "shippingDetails": {
                        "shipFromId": null,
                        "shipToAddressId": "abc5061d-a99d-5df2-e422-7e352d7320a7",
                        "shippingMethodId": "0002"
                    },
                    "returnByDate": null
                }
            ],
            "billingEvents": [],
            "orderState": "PendingFulfillment",
            "paymentDetails": {
                "remitBy": "reseller_ms_remit",
                "payments": [
                    {
                        "paymentInstrumentId": "w49NkgAAAAABAACA",
                        "totalAmount": 0.0,
                        "isPrimary": true
                    }
                ]
            },
            "orderPlacedDate": "2021-06-04T18:06:51+00:00",
            "createdTime": "2021-06-04T18:06:51+00:00",
            "isInManualReview": false,
            "currencyCode": "USD",
            "market": "US",
            "language": "en-us",
            "billingInformation": {
                "soldToAddressId": "2c2a955f-c5bf-52d5-e13f-e8b350ea3353"
            },
            "isUpdatePaymentInstrumentAllowed": true,
            "omniChannelFulfillmentOrderId": null,
            "totalAmount": 990.890,
            "totalTaxAmount": 90.900,
            "totalChargedAmount": 0.0,
            "totalChargedTaxAmount": 0.0,
            "totalRefundAmount": 0.0,
            "totalRefundTaxAmount": 0.0,
            "testScenarios": "TestUser"
        }
    ],
    "hasNextPage": false,
    "apiErrors": [],
    "hasPartialResult": false
}

```

## API Error Codes

| Error Codes      | 
| ---------------- |
| NotFound       | 
| AmbiguousReference    | 
| MalformedInput       | 
| ServiceUnavailable    | 
| BadRequest       | 
| NotAllowed    | 
| Conflict      | 
| Locked    | 
| Forbidden       | 
| ServiceError    | 
| FullRefundTimeframeExceeded      | 
| IneligibleForRefund    | 
| NonRefundablePaymentInstrument       | 
| RefundDoesNotCancelSubscription    | 
| RefundsNotSupportedForTokenRedemption       | 
| RiskPolicyRejected    | 
| InvalidOrderStateForOperation      | 
| RecurrenceNotFound    | 
| TokenOrderIsIneligibleForRefund       | 
| InvalidQuantity    | 
| InsufficientRefundableQuantities       | 
| InsufficientCancellableQuantities    | 
| DuplicateLineId       | 
| InvalidLineId    | 
| InvalidOrderId     | 
| MissingLines    | 
| UpdatatePaymentNotAllowed      | 
| NotAllBundleLineIdsIncluded    | 
| StartDateGreaterThanEndDate     | 
| InsufficientRefundableLines    | 
| InsufficientReturnableQuantities      | 
| D365UnderMaintenance    | 

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:AbRijh@microsoft.com;JorLede@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs/scenarios/listD365PendingOrders.md).

---