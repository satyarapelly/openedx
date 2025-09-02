# Contribution Guidelines 

## Target audience
Developers and PMs contributing code or docs to PX

## Overview
We welcome code and doc contributions to PX components (service + client). We want to make it as easy as 
possible for you to make changes that your business needs. To ensure that PX components stay maintainable and reliable, there are a few guidelines that we need you to follow. This doc is an established set of such guidelines.

Welcome, and thank you in advance.

## Steps

### 1. Scenario / Task / Bug / Partner Information
Before making changes, document the end-user scenario in ADO Here is a template.  Contact us with this ADO item and any 
other by emailing [PX support](mailto:ADmello@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20contribution-guidelines.md). 

A well-documented ADO item (Scenario / Task / Bug) typically includes the following:

1.  What is the end-user trying to achieve?
2.  What is current behavior of the system? - Screenshots if applicable
3.  What will the new behavior look like? - Design/mocks if applicable
4.  What is the business value of this change?
5.  What are the timelines?

This helps us plan for these upcoming changes and point out any issues.  Examples of such issues 

1.  Proposed change conflicts with changes that other teams are planning or is present in other stores.
2.  Proposed timeline does not seem feasible; PX team deploys / releases changes on a cadence which is 
    affected by various factors (e.g. holiday lock down)

If your partner information page isn't available [here](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Docs/partners/overview.md&_a=preview)

Please following the sample [PR](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX/pullrequest/6676235?path=/private/Payments/Docs/partners/overview.md&_a=files) to create your partner information page. 
### 2. Design

1.  Read [architecture docs of PX components](system/px-system.md) to understand how and where to make changes.
2.  Once the scenario / task / bug has been discussed, share how you plan to implement it with specifics. 
3.  Explain how this change will be tested and instrumented. 
4.  Address feedback and document the design.

### 3. Pull request
1. Follow [steps](development/devbox-setup.md) to setup your dev box
2. Follow steps listed in the PR checklist (to be documented) before sending it.

### 4. Deployment

If a feature regions has been assigned to you, follow the feature region guidelines (to be documented). If 
not, follow main deployment guidelines (to be documented).

### 5. Prod support

Once your changes are in prod and considered stabile, we are responsible for that code/change just like 
we are responsible for all other parts of our components. However, there is a period of time after your 
changes go to production, typically, 30 days after it starts taking 100% traffic, when you are still 
responsible for addressing any in-prod issues. We require this so that contributors have serviceability 
in mind when making changes. This allows for PX to remain maintainable despite multiple teams making 
changes.  During this time, you are responsible for fixing any bugs/eng-debt because of your change. In 
the worst case scenario of a livesite (should be very rare), this may include joining the bridge 
by following the same processes and SLAs as any livesite on your own service (5 minute Ack SLA for Sev-2 
or higher IcMs).

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:SwaroSh@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20contribution-guidelines.md).

---