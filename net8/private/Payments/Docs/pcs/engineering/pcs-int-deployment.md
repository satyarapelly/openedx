# PCS INT DEPLOYMENT

PCS uses EV2 for deployments.

If you don't have one ready, create a new build on the master build pipeline.
  - Navigate to [SC.csPayments.PCS.master](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_build?definitionId=13537) build pipeline
  - Click "**Run Pipline**" button
  - Make sure the correct Branch/tag is selected
  - Click "**Run**" button

Once you have a build ready, create a new release on the master release pipeline
  - Navigate to [csPayments.PCS.PME.EV2](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_release?view=all&_a=releases&definitionId=15784) release pipeline
  - Click "**Create Release**" button
  - Add a Release Description if you like.
  - Click "**Create**" button  


#### `INT PCS EV2` should auto deploy.  
#### `PROD West US 2 PCS` are PROD machines that get zero traffic (failovers/backups)
  - Will require manual triggering to start first half, second half will automatically start after first half completes.
#### `PROD Central US PCS` are PROD machines that get traffic.
  - Will require manual triggering to start first half, second half will automatically start after first half completes.

---

For questions/clarifications, email [author/s of this doc and PX support](mailto:mccordmatt@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs/pcs/engineering/pcs-int-deployment.md).

---