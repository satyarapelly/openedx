# [Draft] Team Details  (ICM Transfer)

1. **Storecore-PST-PCS to** **Storecore-PST-GTW:**

- If we are getting transaction failed related errors like below, Then we need to transfer it to StoreCore-PST-GTW team:
`{`
    `"CorrelationId": "7db8008c-e2dc-499d-8a13-77cf71b6ab1f",`
    `"ErrorCode": "InvalidRequestData",`
    `"Message": "Try that again. Something happened on our end. Waiting a bit can help.",`
    `"Source": "PXService",`
    `"InnerError": {`
    `"CorrelationId": "51f29845-cfea-4d87-876b-90a88f2663ea",`
    `"ErrorCode": "InvalidRequestData",`
    `"Message": "No processor support AN-credit_card-visa-OneStore.",`
    `"Source": "PIManagementService",`
    `"Target": "TransactionServiceClient"`

```
}
}
```

    **Or**

    `{"CorrelationId":"997bdd37-a77e-42e0-82c6-33c5156a9752","ErrorCode":"TransactionServiceClientCallFailed","Message":"Try that again. Something happened on our end. Waiting a bit can help.","Source":"PXService","InnerError":{"CorrelationId":"c513f7d7-2c9f-4bff-8249-9841be1f448d","ErrorCode":"TransactionServiceClientCallFailed","Message":"Service call failed. ServiceName: TransactionServiceClient, StatusCode: GatewayTimeout, ResponseText: {\"payment_instrument\":\"632cc329-215e-4ff6-8b6a-b3c94c7e2968\",\"amount\":0.0,\"currency\":\"USD\",\"country\":\"US\",\"additional_validation_info\":{\"cvv_result\":\"none\",\"address_validation_result\":\"unknown\",\"zipcode_validation_result\":\"unknown\",\"name_validation_result\":\"unknown\"},\"id\":\"Z10069BW9S9N5c5912c9-6ad1-4580-a1a1-228f8cda0fcd/19ebf7dd-0968-4bec-a335-18f9c971dafb\",\"status\":\"unknown\",\"type\":\"Validate\",\"version\":\"2015-02-16\",\"links\":{\"self\":{\"href\":\"/22effaad-e72f-4dd3-abac-ae87073668f1/validations/Z10069BW9S9N5c5912c9-6ad1-4580-a1a1-228f8cda0fcd/19ebf7dd-0968-4bec-a335-18f9c971dafb\",\"method\":\"GET\"},\"payments\":{\"href\":\"/22effaad-e72f-4dd3-abac-ae87073668f1/payments/Z10069BW9S9N5c5912c9-6ad1-4580-a1a1-228f8cda0fcd\",\"method\":\"GET\"}}}","Source":"PIManagementService","Target":"TransactionServiceClient"}}`

    Example:
    [Incident 337956364](https://portal.microsofticm.com/imp/v3/incidents/details/337956364/home)<span style="background-color:white"> </span>: [Incident Report][High] PXSuccessRateThresholdSev3 - 2022-09-27 13:00:00 (UTC) - an|paymentinstrumentsexcontroller-post|&lt;-- EMPTY --&gt;|credit\_card|mc|&lt;-- EMPTY --&gt; and 1 more incidents

    [Incident 342367014](https://portal.microsofticm.com/imp/v3/incidents/details/342367014/home) : [Incident Report][High] PXSuccessRateThresholdSev3 - 2022-10-17 01:00:00 (UTC) - mr|paymentinstrumentsexcontroller-post|webblends|&lt;-- EMPTY --&gt;|&lt;-- EMPTY --&gt;|&lt;-- EMPTY --&gt;

1. **Storecore-pst-pcs to** **CCE - Customer Led:**

2. In reference to [Incident 339693038](https://portal.microsofticm.com/imp/v3/incidents/details/339693038/home) : [M365-NCE] Unable to replace payment method - Replace Payment Method blade is blank.
3. I have transferred the ICM to the CCE - Customer Led team post discussing with the person **who was requesting for the logs from frontline support team.** This transfer has been done based on situation.

1. **StoreCore-pst-pcs to** **Payment Services - ICM Tenant/StoreCore-PST-PIMS:**

- Based on traces if any failiure response coming from PIMS, then we need to transfer the ICM to Payment Services - ICM Tenant/StoreCore-PST-PIMS team.

- ![](/images/livesite/1-5f5bbadf9a854d1095773f63c0d34395.jpeg)
- Example: [Incident 339187266](https://portal.microsofticm.com/imp/v3/incidents/details/339187266/home)<span style="background-color:white"> </span>: Not able to purchase domain subscription on MAC portal - urgent!