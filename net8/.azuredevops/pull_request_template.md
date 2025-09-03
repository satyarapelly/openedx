**Before submitting this PR, answer the below checklist:**

**Code**
- [ ] `Payments.sln` solution builds with no errors.
- [ ] Appropriate comments were added where the newly added code is not obvious as to its function.

**Tests**
- [ ] CITs were added as needed with appropriate asserts, covering both negative and positive cases for any .cs changes.
- [ ] Diff test were added to cover *.csv changes and localization changes. 
- [ ] Included screenshots of the UI from local development environment / corresponding React Test Portal link, if the change alters the UX.
- [ ] If none of the above are selected, provide the reason. Lead and the dev need to document the reason for not adding the tests.

**Monitoring & Validation Plan**
- [ ] Provided a query which should return the following 3 fields. 
    - feature: short name of your feature. 
    - health: If it is true, we are good. If is false, rollout dev will contact you to verify.  
    - devcontacts: 2 aliases needed.  
    - Refer to [PX PR review process](https://microsoft.sharepoint.com/:w:/t/PXDevsFTE/EYO5hxKN8khEviROnyxr4BgBr7olECeiNKHL3MiolPE6Fg?e=KHcV6K) for an example query.

**Browser capabilities**
- [ ] The change has a dependency on browser capabilities. Any change that takes a dependency on browser capabilities should be behind a flight. The change needs to be tested on different browsers (Edge, Chrome, webview1, etc.,) before rolling out to actual users.

**Flighting plan**
- [ ] The change is behind a flight.
- [ ] If the change is not behind a flight, provide the reasons why you think a flight isn’t needed. Add a flight if any of the below are applicable.
    - [ ] Does the change impact multiple partners? Or a major partner where any outage could be a Sev 2 / Sev 1?
    - [ ] Does the change impact a high traffic scenario? For example add credit card, add PayPal, PSD2, etc.
    - [ ] Does your change have a dependency on browser capabilities and/or behave differently across browsers and devices, making it impossible to test all combinations?
        - Example 1: [srcdoc](https://www.w3schools.com/TAgs/att_iframe_srcdoc.asp) support varies across browser versions. 
        - Example 2: Google Pay behaves differently on various devices and browsers. 
    - [ ] Does the change depend on downstream systems (e.g., PIMS, Jarvis, tax ID) or callers (e.g., pidlsdk, partners)? If so, will your change function normally if the dependent change isn’t present. 
    - [ ] If we need to roll back the change, will PX deployment rollback be the only option? 
    - [ ] Any potential risk of not having the change under a flight?

**Steps to follow for the leads**
- Leads approval as the 2nd approval is required for each PR.
- Leads will confirm all the PR checklist is completed and provide exceptions approval if needed.