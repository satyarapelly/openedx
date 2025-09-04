# Service Logging Library v5.1

## Support for Service Fabric and Containers in Service Fabric

Your SLL instrumented Application running in a Service Fabric Container can send Telemetry to an Xpert Agent running on your SF Host.  
* You must install our Xpert Agent Proxy.
* You must use our ServiceFabricContainer plugin.

## Xpert Agent Proxy

A Service Fabric application you can install into your Service Fabric Cluster.  This SF App will then manage/install/uninstall Xpert Agents on your Service Fabric Nodes.

Download our XpertAgentProxy nuget package and follow instructions provided.

## Microsoft.CommonSchema.Services.ServiceFabricContainer plugin

Use this plugin if your application will run in a Service Fabric or Container in Service Fabric environment.

See Code Example below.

## Microsoft.CommonSchema.Services.AspNetCore plugin

Use this plugin if your project targets ASP.NET Core.

See Code Example below.

## Code Example for Plugins

```
using Microsoft.CommonSchema.Services.ServiceFabricContainer;
using Microsoft.CommonSchema.Services.AspNetCore;

namespace Example.WebApp.Core
{
    public class Startup
    {
        IDisposable sllServiceFabric;
        IDisposable sllAspNetCore;

        public Startup(IConfiguration configuration)
        {
            // Register SLL Service Fabric / Container
            sllServiceFabric = Policies.ServiceFabricContainer.Register();

            // Register SLL Asp.Net Core
            sllAspNetCore = Policies.AspNetCore.Register();

            Configuration = configuration;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Attach SLL to AspNetCore
            app.UseSLL();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
	}
}
```

## SLL targets .NET Standard and .NET Core

You can use SLL with .NET Standard and .NET Core platforms.

## Add support for Correlation Context

    - To access, use Sll.Context.CorrelationContext
    - Mvc, Owin, and WebApi plugins will read HTTP Header and fill in fields in IncomingServiceRequest telemetry
    - HttpClient plugin will use Sll.Context.CorrelationContext to set HTTP Header and fill in fields in OutgoingServiceRequest telemetry

## Correlation Vector Spin Operator

Introduced a new Spin Operator for Correlation Vector.

## Part A appId and appVer will not be set

	- Part A appId and appVer will not be automatically set unless it's set in Correlation Context

## Helper function to access envelope extensions changed from class Property to Extension Methods

    - To access envelope extensions (i.e. env.ext.device) you can use env.Device() and env.SetDevice(device)
    - There are also env.Safe*() versions (i.e. SafeDevice(), SafeUser(), etc) that will ensure extensions are instantiated before use

## Logging extensions for logging State Transition Telemetry

    - Microsoft.CommonSchema.Services.Logging.StateTransitionLogger.SetEnable(true) to enable.
    - See Microsoft.CommonSchema.Services.Logging.SeqProcessTransitionLogger class
    - Example:
```
            new SeqProcessState()
            {
                name = "Completed",
                processName = this.processName,
                processScope = this.processScope,
                processInstance = this.processInstance,
                processValue = this.processValue,
            }
            .LogSeqProcessStateCompleted(elapsed);
```
## Updated dependent packages
    - Ms.Qos v4.0.17331.1
    - Ms.Qos.IncomingServiceRequest v4.0.17331.1
    - Ms.Qos.OutgoingServiceRequest v4.0.17331.1
    - NewtonSoft.Json v9.0.1
    - Microsoft.Diagnostics.Tracing.EventSource v1.1.28

## Fix invalid char ('<' and '>') in event name by using Data[Incoming/Outgoing]ServiceRequest instead of Data<[Incoming/Outgoing]ServiceRequest>

## Using new versioning scheme of Major.Minor for features.  Build# used for release management.  (i.e. "5.1.83", [Major].[Minor].[Build#])

