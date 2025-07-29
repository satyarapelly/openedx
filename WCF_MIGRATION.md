# Migrating WCF services using .NET Upgrade Assistant

The [.NET Upgrade Assistant](https://learn.microsoft.com/en-us/dotnet/core/porting/upgrade-assistant-wcf) can help convert WCF projects to CoreWCF running on .NET 6 or later.

## Steps

1. **Install the legacy Upgrade Assistant tool**

   ```bash
   dotnet tool install upgrade-assistant -g --version 0.4.421302
   ```

   Use `--ignore-failed-sources` if installation fails due to extra NuGet feeds.

2. **Run the assistant** from your project or solution directory.

   ```bash
   upgrade-assistant upgrade MySolution.sln
   ```

3. **Follow the interactive prompts** to:
   - Backup your project
   - Convert the project file to SDK style
   - Clean up NuGet references
   - Update the target framework and packages
   - **Update WCF service to CoreWCF** (Preview)
   - Upgrade app config files
   - Update source code

During the process the `system.serviceModel` section of `App.config` is moved to `wcf.config`, and service hosting code is replaced with CoreWCF hosted on ASP.NET Core.

Example output after conversion shows usage of CoreWCF types:

```csharp
using CoreWCF;
using CoreWCF.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static async Task Main()
{
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseNetTcp(8090);
    builder.Services.AddServiceModelServices()
                    .AddServiceModelConfigurationManagerFile("wcf.config");
    var app = builder.Build();
    app.UseServiceModel(serviceBuilder =>
    {
        serviceBuilder.AddService<MyService>();
    });
    await app.StartAsync();
    await app.StopAsync();
}
```

For projects that used `System.ServiceModel`, the tool adds packages like `System.ServiceModel.Primitives` and removes the old assembly references.

Refer to the official documentation for full details on each step.
