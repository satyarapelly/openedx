# Documentation Setup

## Target audience
Developers and PMs contributing code or documentation to PX

## Overview
This doc explains steps to setup and use DocFX compiler.  This is needed to build and deploy docs, .md and .yml files under private/Payments/Docs/, locally.  That way, you can verify your doc changes locally before a PR.  

These docs are built from main branch on a periodic basis and deployed into a [static docs website](https://aka.ms/pxdocs) and verify your doc changes locally before a PR.

## DocFX setup
This is a one-time setup.  

1. Download [docfx.zip](https://github.com/dotnet/docfx/releases/download/v2.57.1/docfx.zip) from [DocFX's GitHub](https://github.com/dotnet/docfx)
2. Extract zip file to a local folder and add folder to PATH

>[!NOTE]
>After adding to the PATH variable, you may need to restart the cmd / powershell terminal.

## Build steps
1. cd private\Payments\Docs
2. Run "docfx build --serve" to build and localhost docs website.
3. Navigate to localhost:8080 to load docs site

>[!NOTE]
>You may need to open a fresh incognito window to see latest changes.

### Common issues
1. **I am seeing build errors that have something to do with post processor, or don't have obvious steps to resolution:**
    - Try deleting the cache folders `obj` and `_site` and then build again.
1. **My images aren't showing up / I am not sure how to define the path to my images:**
    - Make sure your images are in the **images/** folder at the root level of the docfx project.

## References
- [Getting started with DocFX](https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html)


---
For questions/clarifications, email [author/s of this doc and PX support](mailto:kowshikpfte@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs/development/doc-setup.md).

---