# Wallet Service

## Target audience
PX Engineering team

## Overview
This describes the component of Wallet Service that needs to be deprecated and removed.

##
Currently the Wallet Service APIs (see idl section) has been marked as deprecated. Still in the process of removing, trying to get communication instructions from CELA.

## Components
Wallet Service is a Windows 10 Desktop (only) component containing: 

### BackgroundServiceProxy (WalletBackgroundServiceProxy.dll)
Located in Windows\System32, Windows\SysWOW64, and Windows\SysArm32

Depends on classes defined under \service\lib, \legacy, and \inc

Seems to be unused, at least not loaded as a Windows Service
    
Service Proxy to get WalletBackgroundAgentManager from Wallet Service:
- WalletBackgroundServiceProxy.cpp/h<br/>Which depends on:
	- walletitemmanagerutils
	- WalletItemManagerClient

### Extensibility (Windows.ApplicationModel.Wallet.dll)<br/>
Located in Windows\System32, Windows\SysWOW64, and Windows\SysArm32

Depends on classes defined under \service\lib (Wallet Service) and \legacy

Not a Windows Service

- dll (provides GetWalletXInstance and all classes under Windows.ApplicationModel.Wallet namespace):<br/>$~~~~$ Creates Windows.ApplicationModel.Wallet.dll
	- module.cpp
	- precomp.h

- EventArgsLib (walletactionactivatedeventargs.lib):<br/>$~~~~$ Library used when compiling Windows.ApplicationModel.Wallet.dll
	- GetWalletActionActivatedEventArgs.cpp (Provides WalletActionActivatedEventArgs)
	- WalletActionActivatedEventArgs.cpp/h (Provides information to an app that was launched as the result of a wallet action.)

- idl:<br/>$~~~~$ Defines [Windows.ApplicationModel.Wallet API](https://docs.microsoft.com/en-us/uwp/api/windows.applicationmodel.wallet?view=winrt-20348)<br/>$~~~~$ Related [Windows.ApplicationModel.Wallet.System API](https://docs.microsoft.com/en-us/uwp/api/windows.applicationmodel.wallet.system?view=winrt-20348) are defined under onecoreuap/shell/published/winrt, see this [API deprecation PR](https://microsoft.visualstudio.com/OS/_git/os.2020/pullrequest/6004929)
	- WindowsPhone_Wallet.idl

- lib:<br/>$~~~~$ Provides Wallet classes and operations on them (storage and editing) for the Windows Phone Wallet app through WalletX instances<br/>$~~~~$ See [MSDN](https://docs.microsoft.com/en-us/uwp/api/windows.applicationmodel.wallet?view=winrt-20348 for the functionality of the classes<br/>$~~~~$ Sources for Windows.ApplicationModel.Wallet.dll
	- CustomPropertyRT.cpp/h
	- CustomVerbRT.cpp/h
	- FileItems.cpp/h
	- LocationRT.cpp/h
	- MakeAsyncNames.h
	- precomp.h
	- TransactionRT.cpp/h
	- utils.cpp/h
	- WalletBarcodeRT.cpp/h
	- WalletItemRT.cpp/h
	- WalletItemStoreBase.cpp/h
	- WalletItemSystemStoreServer.cpp/h
	- WalletManagerServer.cpp/h
	- WalletManagerSystemServer.cpp/h
	- WalletRT.cpp/h

- manifest:<br/>$~~~~$ Manifests for Windows.ApplicationModel.Wallet.dll
	- WalletWinRT.man (Register Wallet interfaces and proxy)
	- WalletWinRTCaps.wm.xml ("Allow 1st party app to get full access to wallet items")

- mbs:<br/>$~~~~$ Project files for Windows.ApplicationModel.Wallet.dll
	- \Microsoft-Windows-Wallet-Capabilities\product.pbxproj
	- \Microsoft-Windows-Wallet-WinRT\product.pbxproj
	- \Microsoft-Windows-Wallet-WinRT\sources.dep
	- \Microsoft-Windows-Wallet-WinRT-Wow\product.pbxproj

- published:<br/>$~~~~$ Header file to be copied for onecoreuap build
	- GetWalletActionActivatedEventArgs.h

### inc
Wallet classes header files used internally when creating components (e.g. WalletBackgroundServiceProxy.dll, Windows.ApplicationModel.Wallet.dll, walletservice.dll etc)
- DynamicLoader.h
- Wallet.h
- WalletAgentManager.h
- WalletErrors.h
- WalletFactory.h
- WalletFileUtils.h
- WalletModel.h
- WalletOrderedLock.h
- WalletSqm.h
- WalletTelemetry.h
- WalletUtils.h
- WalletUtils_priv.h

### legacy
Header files containing utilities still in use
- \appplat\inc\datasharing.h
- \appplat\inc\tokens.h
- \appplat\inc\tokentemplates.h
- \common\inc\atlcomutil.h
- \common\inc\Basic_TraceHR.h
- \common\inc\ConnectionProviderBase.h
- \common\inc\FileReadStream.h
- \common\inc\HttpTransport.h
- \common\inc\IControlChannelTriggerManager.h
- \common\inc\ifErrorReturnMacros.h
- \common\inc\LockOrderManager.h
- \common\inc\orderedlock.h
- \common\inc\sortlocaleheader.h
- \common\inc\StreamAdapters.h
- \common\inc\UtUtilsRW.h
- \common\inc\ZipContainerInit.h
- \common\inc\published\commonhandles.h
- \common\inc\published\dbgapirw.h
- \common\inc\published\ztracerw.h
- \globplat\inc\winnls_ext.h
- \globplat\inc\winnls_extendedcompat.h
- \globplat\inc\winnls_ext_tzid.h
- \media_meinfra\inc\gameids.h
- \media_meinfra\inc\XBoxLiveConstants.h
- \media_meinfra\inc\XBoxLiveErrors.h
- \media_meinfra\inc\zerror.h
- \media_meinfra\inc\ZMediaLibTypes.h
- \media_meinfra\inc\ZmfApi.h
- \media_meinfra\inc\ZmfErrors.h
- \media_meinfra\inc\ZmfTypes.h
- \media_rtl\inc\zasync.h
- \media_rtl\inc\ztrace.h
- \media_rtl\inc\ZTraceDef.h
- \media_rtl\inc\ZTraceFlowControl.h
- \media_rtl\inc\ZTraceLogging.h
- \phonebase\inc\dbgapi.h
- \phonebase\inc\wphandle.h
- \phonebase\inc\deprecated\auto_xxx.hxx
- \phonebase\inc\deprecated\hash.hxx
- \phonebase\inc\deprecated\hash_map.hxx
- \phonebase\inc\deprecated\hash_set.hxx
- \phonebase\inc\deprecated\list.hxx
- \phonebase\inc\deprecated\ptr_com.h
- \phonebase\inc\deprecated\ptr_traits.h
- \phonebase\inc\deprecated\string.hxx
- \phonebase\inc\deprecated\sync.hxx
- \phonebase\inc\deprecated\utility.hxx
- \phonebase\inc\deprecated\vector.hxx
- \shell\inc\PersonaTypes.h
- \shell\inc\ShellChromeAPI.h
- \shell\inc\ShellChromeAPIPublished.h
- \shell\inc\Shell_LASS.h
- \sqm\sqmdatapoints.h
- \sqm\wpsqm.h
- \uxplatform_avcore\inc\eventsnd.h

### service (WalletService.dll)
Located in Windows\System32

Depends on classes defined under \legacy and \inc

Runs as Wallet Service

- dll:<br/>$~~~~$ Creates WalletService.dll which contains factories for WalletX, TheWallet, WalletBackgroundAgentManager (only for AIM), and WalletDealsManager
	- main.cpp
	- precomp.h
	- ServiceTemplate.cpp/h
	- WalletFactory.cpp
	- walletResource.h
	- WalletService.h

- idl:<br/>$~~~~$ Defines WalletService APIs, usable by internal Windows components and some through [Windows.ApplicationModel.Wallet API](https://docs.microsoft.com/en-us/uwp/api/windows.applicationmodel.wallet?view=winrt-20348)
	- IWallet.idl
	- IWalletBackgroundAgentManager.idl
	- IWalletCustomProperty.idl
	- IWalletDeal.idl
	- IWalletDealsManager.idl
	- IWalletItem.idl
	- IWalletItemList.idl
	- IWalletItemListener.idl
	- IWalletItemManager.idl
	- IWalletLocationManager.idl
	- IWalletNotification.idl
	- IWalletNotificationManager.idl
	- IWalletPackageProcessor.idl
	- IWalletTransactionManager.idl
	- IWalletWebServiceManager.idl
	- IWalletX.idl
	- WalletTypes.idl

- lib:<br/>$~~~~$ Sources for WalletService.dll
	- WalletAgentManager.cpp
	- Barcode (Barcode processing, used by 3D Builder for example):
		- \common\alphanum.cpp
		- \common\AZTECHelper.cpp
		- \common\AZTECRenderer.cpp
		- \common\barcode.cpp/h
		- \common\BarcodeGen.cpp/h
		- \common\BarcodeRendererBase.cpp/h
		- \common\Code128Renderer.cpp
		- \common\Code39Renderer.cpp
		- \common\common.cpp
		- \common\commondef.h
		- \common\D2D1RenderTarget.cpp/h
		- \common\DrawingEnv.cpp/h
		- \common\ITFHelper.cpp
		- \common\ITFRenderer.cpp/h
		- \common\PDF417Helper.cpp/h
		- \common\PDF417Renderer.cpp
		- \common\pos.cpp
		- \common\POSRenderer.cpp
		- \common\QRRenderer.cpp
		- \common\sources
		- \common\wp.sources.dep
		- \qrcode\blockview.cpp/h
		- \qrcode\codewordstream.cpp/h
		- \qrcode\ColumnView.cpp/h
		- \qrcode\DataEncoder.cpp/h
		- \qrcode\ErrCorrCodeWord.cpp/h
		- \qrcode\GaloisField.cpp/h
		- \qrcode\MaskPattern.cpp/h
		- \qrcode\Mode.cpp/h
		- \qrcode\qrcode.h
		- \qrcode\ReedSolomon.cpp/h
		- \qrcode\sources
		- \qrcode\Symbol.cpp/h
		- \qrcode\version.cpp/h
		- \qrcode\wp.sources.dep
	- Client (Utilities to access WalletService):
		- WalletClientUtils.cpp (Utilities for Clients of the wallet core)
		- WalletDealUtils.cpp/h (Used to get a token pass deal files to the Wallet)
	- Core (Main Wallet classes):
		- DynamicLoader.cpp
		- Wallet.cpp
		- WalletAgentManagerServer.cpp/h
		- WalletCustomProperty.cpp/h
		- WalletDatabaseVersion1_0.h
		- WalletDeal.cpp/h
		- WalletDealsManager.cpp/h
		- WalletEncrypter.cpp/h
		- WalletESE.cpp/h
		- WalletESE_schema.h
		- WalletESE_schemaTypes.h
		- WalletESE_utils.h
		- WalletItem.cpp/h
		- WalletItemList.cpp/h
		- WalletItemManager.cpp/h
		- WalletLocationManager.cpp/h
		- WalletNotification.cpp/h
		- WalletNotificationManager.cpp/h
		- WalletServerUtils.cpp/h
		- WalletTransactionManager.cpp/h
	- Extensibility (Implementation for APIs under IWalletx.idl):
		- WalletXItem.cpp/h
		- WalletXItemList.cpp/h
		- WalletXLocationManager.cpp/h
		- WalletXServer.cpp/h
		- WalletXTransactionManager.cpp/h
	- PackageProcessor (Used to process Wallet packages for initializing Wallet Service's db):
		- cbufferedreader.cpp/h
		- Formatting.cpp/h
		- precomp.h
		- WalletPackageProcessor.cpp/h
	- WebServiceManager (Used to access Wallet data remotely):
		- precomp.h
		- WalletWebService.cpp/h
		- WalletWebServiceManager.cpp/h
		- WalletWebServiceRequest.cpp/h
		- WalletWebServiceRequestGetWalletItem.cpp/h

- manifest:<br/>$~~~~$ Manifests for WalletService.dll
	- WalletService.BackgroundProxy.man
	- WalletService.Deployment.man
	- WalletService.Instrumentation.man
	- WalletService.man
	- WalletService.Proxy.man
	- WalletService.Resources.man

- mbs:<br/>$~~~~$ Project files for WalletService.dll
	- \Microsoft-Windows-Wallet-Service\product.pbxproj
	- \Microsoft-Windows-Wallet-Service-Wow\product.pbxproj
	- \Microsoft-Windows-Wallet-Service-Wow.Resources\product.pbxproj
	- \Microsoft-Windows-Wallet-Service.BackgroundProxy\product.pbxproj
	- \Microsoft-Windows-Wallet-Service.BackgroundProxy-Wow\product.pbxproj
	- \Microsoft-Windows-Wallet-Service.Deployment\product.pbxproj
	- \Microsoft-Windows-Wallet-Service.Deployment-Wow\product.pbxproj
	- \Microsoft-Windows-Wallet-Service.Instrumentation\product.pbxproj
	- \Microsoft-Windows-Wallet-Service.Instrumentation-Wow\product.pbxproj
	- \Microsoft-Windows-Wallet-Service.Proxy\product.pbxproj
	- \Microsoft-Windows-Wallet-Service.Proxy-Wow\product.pbxproj
	- \Microsoft-Windows-Wallet-Service.Resources\product.pbxproj

### tests
Test app for WalletService

### unittests
Unit tests for WalletService classes

### utils
Servicing tool for WalletService (so you can use this to directly add and list items in the WalletService)
- \Lib\precomp.h
- \Lib\WalletFileUtils.cpp
- \Lib\WalletSqm.cpp
- \Lib\WalletUtils.cpp
- \Lib\ZTraceLogging.cpp
- \WalletSvcTool\main.cpp
- \WalletSvcTool\main.rc

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:pawisesa@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20engineering/walletdeprecation.md).

---