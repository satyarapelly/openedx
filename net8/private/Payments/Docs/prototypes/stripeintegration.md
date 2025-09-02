# Stripe Integration

#### Target Audience:
PX Engineering team

#### Overview
Describes Stripe APIs and how Merchants integrate with them to process payments.

#### Immediate Settle
The sequence diagram below shows interactions between a customer, the merchant and Stripe.  At design-time, the merchant creates an account with Stripe   

```mermaid
sequenceDiagram

participant Customer
participant Merchant webpage
participant Stripe webpage
participant Stripe JS lib
participant Merchant server
participant Stripe server

Customer-->>Merchant webpage: 1. Buy
Merchant webpage->>Merchant server: 2. Buy
Merchant server->>Stripe server: 3. Create CheckoutSession
Stripe server->>Merchant server: 4. CheckoutSession
Merchant server->>Merchant webpage: 5. CheckoutSession
Merchant webpage->>Stripe JS lib: 6. RedirectToCheckout(PK, CheckoutSession.Id)
Stripe JS lib->>Stripe webpage: 7. Redirect
Customer-->>Stripe webpage: 8. Enter card/PI info
Stripe webpage->>Stripe server: 9. Card/PI info
Stripe server->>Merchant webpage: 10. Redirect back to success/error pages
Stripe server->>Merchant server: 11. Payment success/error event
Merchant server->>Merchant server: 12. Fill order
Merchant server->>Merchant webpage: 13. success/error
Merchant webpage->>Merchant webpage: 14. Show success/error message

```
##### Screenshot
![Stripe checkout](../images/stripeCheckout.jpg)