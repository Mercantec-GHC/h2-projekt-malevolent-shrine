using System;
using System.Net.Http;
using Blazor.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blazor;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Read API endpoint from Blazor appsettings (wwwroot/appsettings*.json) with a safe local fallback
        var configuredApi = builder.Configuration["ApiEndpoint"];
        var apiEndpoint = string.IsNullOrWhiteSpace(configuredApi)
            ? "http://localhost:8062/"
            : configuredApi;

        Console.WriteLine($"Blazor Environment: {builder.HostEnvironment.Environment}");
        Console.WriteLine($"Configured API Endpoint: {configuredApi}");
        Console.WriteLine($"Using API Endpoint: {apiEndpoint}");

        builder.Services.AddHttpClient<APIService>(client =>
        {
            client.BaseAddress = new Uri(apiEndpoint);
        });

        builder.Services.AddHttpClient<ActiveDirectoryService>(client =>
        {
            client.BaseAddress = new Uri(apiEndpoint);
        });

        builder.Services.AddHttpClient<AdAuthClientService>(client =>
        {
            client.BaseAddress = new Uri(apiEndpoint);
        });

        await builder.Build().RunAsync();
    }
}
