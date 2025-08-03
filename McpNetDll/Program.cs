using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace McpNetDll;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Redirect console logging to stderr
        builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

        builder.Services.AddSingleton<Extractor>();
        builder.Services
            .AddMcpServer(server => { server.ServerInfo = new() { Name = "McpNetDll", Version = "1.0.0" }; })
            .WithStdioServerTransport()
            .WithToolsFromAssembly(); // Scans the assembly for [McpServerToolType]

        var host = builder.Build();
        await host.RunAsync();
    }
}

[McpServerToolType]
public static class DllMetadataTool
{
    [McpServerTool,
     Description(
         "Analyzes a .NET assembly file (.dll) to extract its public API information with layered filtering. Supports three modes: 1) No filters - returns namespace information, 2) With namespaces - returns class information for specified namespaces, 3) With class names - returns detailed member information for specified classes. The primary use case is for an AI to get the most accurate, up-to-date API definition of a library when API calls fail or usage is unclear, helping to analyze and correct code. It is crucial that the caller should make to find and provides the absolute path to the target library's DLL file.")]
    public static string ExtractMetadata(
        Extractor extractor,
        [Description("The absolute path to the DLL file.")]
        string dllPath,
        [Description(
            "Optional: Array of namespace names to filter by. When provided, returns class information for these namespaces only.")]
        string[]? namespaces = null,
        [Description(
            "Optional: Array of full class names (format: Namespace.Class) to filter by. When provided, returns detailed member information for these classes only.")]
        string[]? classNames = null)
    {
        return extractor.ExtractWithFilters(dllPath, namespaces, classNames);
    }
}