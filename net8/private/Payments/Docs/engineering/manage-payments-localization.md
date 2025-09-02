# Manage-payments localization

## Target audience
PX Engineering team

## Overview
This document explains the localization processs for the manage-payments component, including how to check for localization status.

## Prerequisites
* [Getting started with the manage-payments project](../development/manage-payments-getting-started.md)

## Adding a new string
When adding a new string there are 2 files that need to be updated to be able to use the string in the code.

First add the string to **components\react\manage-payments\localized_resources\en-us\resource_strings.resx** 
This is the source file that the localization team uses to translate the string.
Follow the localization guidelines below to produce highly localizable strings that can be translated correctly in all languages.

Then you need to update the **locStrings.tsx file** by either running `npm run build` in the manage-payments folder or running `.\generate_locstrings.ps1` to auto-generate the file.
This file is a JS file with constants of each string, so the code can use them.
 
## How to check for localization status
### Get the LocState tool
To check for localization status you will need the LocState script.
For more info about the script and how to get it, go to: https://ceapex.visualstudio.com/CEINTL/_wiki/wikis/CEINTL.wiki/1440/Tool4Core-Check-localization-status-in-build-using-LocState.ps1  
Copy the script to some suitable location on your PC (e.g. C:\Tools\)

### Check the latest localization status
1. Go to the folder **pay.pidl.sdk\components\react\manage-payments** and pull from master.
1. Run `npm run build`
1. Go to the folder where you copied the LocState tool (e.g. C:\Tools)
1. Run `.\LocState.ps1 -FileType LCL -LocFolder {Enter full path to pay.pidl.sdk\Localize\locfolder}`
1. Open the results file.

The results file will show the percentage of localization.  
Note that there are several languages that will be reported less than 100% localized because it has been decided not to continue localizing those languages due to low ROI.  
The language codes for those languages are:  

be, bn-BD, ha, hy, ig, ku-Arab, ky, mn, nso, pa-Arab-PK, prs, quc, rw, sd, si, sw, tg, ti, tk, tn, uz, wo, xh, yo, zu 
