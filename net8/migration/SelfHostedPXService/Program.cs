// Program.cs — .NET 8 in-memory hosting using TestServer

using Newtonsoft.Json;
using SelfHostedPXServiceCore;
using System;
using System.Net.Http;
using System.Threading.Tasks;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        string fullBaseUrl = args.Length > 0 ? args[0] : "";
        string protocol = args.Length > 1 ? args[1] : "https";

        if (string.IsNullOrEmpty(fullBaseUrl))
        {
            var port = SelfHostedPxService.GetAvailablePort();
            fullBaseUrl = $"{protocol}://localhost:{port}";
        }

        Console.WriteLine($"Initializing server on {fullBaseUrl}...");

        // Spin up the PX service and all its dependency emulators in memory with
        // routing configured so HttpContext.GetEndpoint() resolves correctly.
        var selfHostedSvc = SelfHostedPxService.StartInMemory(fullBaseUrl, true, false);

        // Kick the tires on a simple request. The server writes the matched
        // endpoint to the console (see HostableService/ConfigurePipeline).
        var requestUrl = fullBaseUrl + "/probe";
        Console.WriteLine($"Calling {requestUrl} to verify endpoint resolution...");
        HttpResponseMessage response = await selfHostedSvc.HttpSelfHttpClient.GetAsync(requestUrl);
        Console.WriteLine($"Probe returned {(int)response.StatusCode}");

        // Warm up like before (no real network I/O; this goes through TestServer).
        requestUrl = fullBaseUrl + "/v7.0/account001/paymentMethodDescriptions?country=tr&family=credit_card&type=mc&language=en-US&partner=storify&operation=add";

        response = await GetPidlFromPXService(requestUrl, selfHostedSvc);
        var content = FormatJson(await response.Content.ReadAsStringAsync());

        if (response.IsSuccessStatusCode)
        {
            System.Console.WriteLine("Server successfully tested");
        }
        else
        {
            System.Console.WriteLine($"Server responded: {response.StatusCode}");
            System.Console.WriteLine($"Response content: {content}");
        }

        // Keep the process alive until Ctrl+C
        System.Console.WriteLine("PX (in-memory) is running. Press Ctrl+C to exit.");
        var done = new TaskCompletionSource();
        System.Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            done.TrySetResult();
        };
        await done.Task;
    }

    private static async Task<HttpResponseMessage> GetPidlFromPXService(string url, SelfHostedPxService host)
    {
        // Keep your existing user substitution logic
        var fullUrl = url;
        if (fullUrl.Contains("completePrerequisites=true"))
        {
            fullUrl = fullUrl.Replace("users/me", "EmpAccountNoAddress");
        }
        else
        {
            fullUrl = fullUrl.Replace("users/me", "DiffTestUser");
        }

        fullUrl = fullUrl.Replace("users/my-org", "DiffOrgUser");

        // Call the in-memory client
        return await host.HttpSelfHttpClient.GetAsync(fullUrl);
    }

    private static string FormatJson(string json)
    {
        dynamic parsed = JsonConvert.DeserializeObject(json);
        return JsonConvert.SerializeObject(parsed, Formatting.Indented);
    }

}