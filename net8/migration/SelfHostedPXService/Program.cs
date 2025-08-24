// Program.cs — .NET 8, in-memory hosting (Option B)

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SelfHostedPXServiceCore;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        // Spin up the PX service and all its dependency emulators in memory with
        // routing configured so HttpContext.GetEndpoint() resolves correctly.
        using var host = SelfHostedPxService.StartInMemory(true, false);

        // Warm up like before (no real network I/O; this goes through TestServer).
        var relativeUrl = "users/me/paymentMethodDescriptions?country=tr&family=credit_card&type=mc&language=en-US&partner=storify&operation=add";
        var url = $"/v7.0/{relativeUrl}";
        var response = await GetPidlFromPXService(url, host);
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
        return await host.HttpClient.GetAsync(fullUrl);
    }

    private static string FormatJson(string json)
    {
        dynamic parsed = JsonConvert.DeserializeObject(json);
        return JsonConvert.SerializeObject(parsed, Formatting.Indented);
    }
}
