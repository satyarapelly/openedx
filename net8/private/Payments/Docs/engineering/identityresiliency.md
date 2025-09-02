# Identity Resiliency 
 
## Target audience
P+S Engineering
 
## Overview
Migrating out of certificates and to AAD / MSI is a FUN requirement in Nickel as described in the ADO item below
- [Scenario 32980159: FUN: C+E Services meet Identity Resiliency Targets](https://microsoft.visualstudio.com/OSGS/_workitems/edit/32980159)
 
This work  is being planned in phases as described here.
 
## Goals
In addition to completing requirements specified in the above scenario, the following are also goals of this project/initiative
- Documenting recommended patterns/practices
- Creating reusable libraries across P+S services
 
## Requirement Clarification 
 
**Requirements that we have clarified with commerce mesh already:**
- The scope of migration: migration within C+E org is required. If the caller or callee isn’t inside C+E org, we can file an exception [here](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR8PouRi41ttJnnI7WegtnEhURUw0UFpCUlE4UFE3TTUzRlNJVjFLV0U2NS4u). It will be removed from our [S360](https://vnext.s360.msftcloudes.com/blades/security?blade=KPI:068e1ca4-d017-4879-9009-c098f7589530~SLA:3~AssignedTo:All~Forums:All~_loc:Security&def=&peopleBasedNodes=wiwhite_team&tile=&global=3:4b1ada09-a3f5-4226-9122-ed8121977145).
- We found out that S360 list is inaccurate.  Action items for P+S is:
  - Remove the deprecated code path after confirming that it isn’t in use.
  - Provide a list of files that S360 should scan
  - Provide file exception if case is invalid. 
- Should payment services be registered as 1st Party or 3rd Party AAD Apps?
  - Based on the documentation [here](https://commercewiki.trafficmanager.net/best_practices/security/cert_mgmt/aadguidance.html), we should be 1st party, since we are serving Microsoft customers. 
  - 1st party is a hard requirement as part of AAD migration. 
- Is certificate auto-rotation required?
  - Yes.
- Deadline for AAD Migration?
  - There is no deadline in Ni. The requirement is to show progress.
- PCE isn't in scope

**Questions we need clarify from commerce mesh or AAD or internal:**
- Is JEDI in scope?
- Application ID per service tree or service? Will be any issue services with same id call each other? (commerce mesh doesn't have requirement and will confirm with AAD team)
- Application permission and/or delegate user permission? If delegate user permission is used, how will delegateRPS work?
- Do we need app Role? (payment)

## Execution Plan:
**Stage 1: We focus on callee and caller in P+S only**
- Collect [P+S AAD Migration Inventory](https://microsoft.sharepoint.com/:x:/t/PS-IdentityResiliency/EVghDHLylRJDgkPw6e35ZfcBd3KmVpOOrzpcN9ytGtuE1g?e=VN5NU8)
- Figure out the pattern and create callee and Caller Library

**Progress within P+S:**
- Wei Wu prototyped the callee and caller in PX INT. Proposed options to create library [details](https://microsoft.sharepoint.com/:p:/t/PS-IdentityResiliency/EZ1X8wkHLjpIvjdrwvkiNacBn0JC-uSuHWNuM10WvHoMww?e=zeBY7y).
- Murali did initial version inventory for transaction, session and payauth.
- Jie did initial PIMS Inventory, reviewed by Yuanji
More details can be found in [meeting notes](https://microsoft.sharepoint.com/:o:/t/PS-IdentityResiliency/EojPY9ta3q5IuWwuoZbF3d8BUUGRS-Csnfd3n0F2b2Q-AA?e=yQPJhN)
- Ann Supported PIMS Inventory

**Next steps in P+S:**
- Wei Wu 
   - Investigation on Callee Pattern in Progress
      - Uniform logging in AadTokenValidator
      - Resolve principal in AadTokenValidator
      - Consider Include AadTokenValidator in PIMS certificate verification library
   - Implement callee library AadTokenValidator
- Murali 
  - continue the PayMod inventory
- Jie
  - PX inventory
  - Work with commerce mesh to feature out a way to clean up S360 effectively
  - Open questions clarification
  - Resolve Yuanji's comments
  - PX AAD 1st Party Application 
- Ann
  - continue support PIMS Inventory

## Roles 
|Roles|Eng|
|-|-|
|Project Owner|Jie|
|Team Rep - PX| Wei|
|Team Rep - PIMS|Ann|
|Team Rep - PayMod|Murali| 
|Team Rep - Recon|TBD|
|PM Owner|TBD|
|Consultation|Cen|
|Fun Liaison|Dan|
|Lead|Swaroop|
 
## Responsibilities
- Project Owner - Responsible for the overall success of the project across P+S.
- Team Reps - Responsible for 
  - Providing inventory / inputs to the project owner 
  - Implementation within each service
- Lead - Also responsible for the overall success of the project across P+S.
 
## Communication / Coordination
We plan on using team's channel "Identity Resiliency" under [P+S](https://teams.microsoft.com/l/team/19%3ahADCcT4RUoKC2FrjHFx64kW2AGXPCHNPFRr91ijpAyI1%40thread.tacv2/conversations?groupId=ecb6ce5d-ccb4-40a5-a51a-4b55a40d0237&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47) for all communications and meeting. 
 
This teams channel is backed by a [sharepoint site](https://microsoft.sharepoint.com/teams/PS-IdentityResiliency/Shared%20Documents/Forms/AllItems.aspx).  So, all files uploaded in the "Files" tab of this channel will be on this [sharepoint folder](https://microsoft.sharepoint.com/teams/PS-IdentityResiliency/Shared%20Documents/Forms/AllItems.aspx)
 
## Safe Deployment / Rollout
 
Wei is currently investigating / refining a plan for this.

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:JieFan@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20engineering/identityresiliency.md).

---