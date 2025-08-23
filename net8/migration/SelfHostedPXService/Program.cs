using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SelfHostedPXServiceCore;

// optional base URL from args, e.g. https://localhost:7151
var baseUrl = args.Length > 0 ? args[0] : "https://localhost:7151";
Console.WriteLine(baseUrl is null ? "Initializing server..." : $"Initializing server on {baseUrl}...");

using var host = new SelfHostedPxService(baseUrl, useSelfHostedDependencies: true, useArrangedResponses: false);
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

Console.WriteLine("Server initialized.");

// Do one warmup call; don't crash app if it fails
try
{
    Console.WriteLine("Warming up server...");
    var url = "v7.0/bc81f231-268a-4b9f-897a-43b7397302cc/paymentMethodDescriptions?type=amex%2Cvisa%2Cmc%2Cdiscover%2Cjcb&partner=commercialstores&operation=Add&country=US&language=en-US&family=credit_card&currency=USD";

    var response = await GetPidlFromPXService(url);
    var text = await response.Content.ReadAsStringAsync();

    Console.WriteLine($"Warmup status: {(int)response.StatusCode} {response.ReasonPhrase}");
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine(FormatJsonSafe(text));
    }
}
catch (Exception ex)
{
    Console.WriteLine("Warmup failed (continuing to run):");
    Console.WriteLine(ex);
}

Console.WriteLine("Listening... Press Ctrl+C to stop.");
try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (OperationCanceledException)
{
    // Ctrl+C
}
finally
{
    Console.WriteLine("Server stopped.");
}

static async Task<HttpResponseMessage> GetPidlFromPXService(string url)
{
    var fullUrl = SelfHostedPxService.GetPXServiceUrl(url);
    fullUrl = fullUrl.Contains("completePrerequisites=true", StringComparison.OrdinalIgnoreCase)
        ? fullUrl.Replace("users/me", "EmpAccountNoAddress", StringComparison.Ordinal)
        : fullUrl.Replace("users/me", "DiffTestUser", StringComparison.Ordinal);

    fullUrl = fullUrl.Replace("users/my-org", "DiffOrgUser", StringComparison.Ordinal);

    return await SelfHostedPxService.PxHostableService.HttpSelfHttpClient.GetAsync(fullUrl);
}

static string FormatJsonSafe(string json)
{
    try
    {
        var parsed = JsonConvert.DeserializeObject(json);
        return JsonConvert.SerializeObject(parsed, Formatting.Indented);
    }
    catch
    {
        return json;
    }
}
