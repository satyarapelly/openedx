using SelfHostedPXServiceCore;
using System;
using System.Net.Http;
using System.Threading.Tasks;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        string baseUrl = args.Length > 0 ? args[0] : "http://localhost";

        Console.WriteLine($"Initializing in-memory server on {baseUrl}...");

        // Spin up the PX service entirely in-memory – no TCP sockets required.
        var selfHostedSvc = SelfHostedPxService.StartInMemory(baseUrl, false, false);

        // Verify the service is reachable by issuing a simple probe request.
        HttpResponseMessage response = await selfHostedSvc.HttpSelfHttpClient.GetAsync(new Uri(new Uri(baseUrl), "/probe"));
        Console.WriteLine($"Probe responded with {response.StatusCode}");

        Console.WriteLine("PX (in-memory) is running. Press Ctrl+C to exit.");
        var done = new TaskCompletionSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; done.TrySetResult(); };
        await done.Task;
    }
}

