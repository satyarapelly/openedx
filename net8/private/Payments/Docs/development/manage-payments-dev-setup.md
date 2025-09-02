# Manage Payments Dev Box Setup

## Target audience
Developers and PMs contributing code to manage-payments project.

## Overview
This document explains how to get the manage-payments source code, build it, link it and run it on your own PC for development purposes.

## Prerequisites
- [Getting started with Manage Payments](manage-payments-getting-started.md)

## Setup
### Install tooling
1. [GIT](https://git-scm.com/downloads)
2. [NodeJS](https://nodejs.org/)
3. [Visual Studio Code](https://code.visualstudio.com/)

### Get the source code
Use GIT to clone the depot in your PC:

`git clone https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/pay.pidl.sdk`

>Note it's the same PIDL SDK depot, so you don't need to clone the depot again if you already did so during the PIDL SDK dev box setup.

### Run the setup script
1. Open a PowerShell command line.
1. Go to the folder **pay.pidl.sdk/tools/Build** in your clone location.
1. Run `./setup.ps1`

>Note that if you run `npm install` in any of the project folders, it will undo the npm links `setup.ps1` creates and you will need to rerun `setup.ps1` to remake them.

### Run in localhost
1. Go to the folder **pay.pidl.sdk\components\react\manage-payments** and run `npm run watch`, leave that terminal open.
1. In a new terminal go to the folder **pay.pidl.sdk\apps\northstar-app** and run `npm run start`, leave that terminal open.
1. If you need to make changes in **pidl-react** or **pidl-fluent-ui**, then go those folders and run `npm run watch` in new terminals as well.


## Troubleshooting
* **I get an authorization error when running setup.ps1**  
Solution: Delete the **.npmrc** file under your own user folder (`%USERPROFILE%`) and re-run `setup.ps1`.

* **I get a TypeScript error about a missing property of method when running `npm run watch` or `npm run start`**  
Solution: Pull latest from master and run `setup.ps1` again.
