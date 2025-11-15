using McpNetDll.Core;
using McpNetDll.Helpers;
using McpNetDll.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace McpNetDll;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Error: Please provide path(s) to DLL file(s) as command line arguments.");
            Console.Error.WriteLine("Usage: McpNetDll.exe <dll-path1> [dll-path2] [...] [--json-format]");
            Console.Error.WriteLine("Options:");
            Console.Error.WriteLine("  --json-format    Use JSON output format instead of default AI-optimized format");
            Environment.Exit(1);
        }

        // Filter out flags and get only DLL paths
        var dllArgs = args.Where(arg => !arg.StartsWith("--")).ToArray();
        var dllPaths = dllArgs.Where(File.Exists)
            .Select(PathHelper.ConvertWslPath)
            .ToArray();

        if (dllPaths.Length == 0)
        {
            Console.Error.WriteLine("Error: None of the provided DLL paths exist.");
            Environment.Exit(1);
        }

        var builder = Host.CreateApplicationBuilder(args);

        // Redirect console logging to stderr
        builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

        // Check for JSON formatter flag (environment variable or --json-format flag)
        // Default is to use AI formatter unless explicitly requested otherwise
        var useJsonFormatter = Environment.GetEnvironmentVariable("MCP_JSON_FORMAT") == "true" ||
                             args.Contains("--json-format");
        var useAiFormatter = !useJsonFormatter;

        // Register the new architecture components
        builder.Services.AddCoreServices(dllPaths, useAiFormatter);

        var typeRegistry = builder.Services.BuildServiceProvider().GetRequiredService<ITypeRegistry>();
        var availableNamespaces = typeRegistry.GetAllNamespaces();
        var namespaceSummary = availableNamespaces.Any()
            ? $"Loaded {availableNamespaces.Count} namespaces: {string.Join(", ", availableNamespaces.Take(5))}{(availableNamespaces.Count > 5 ? "..." : "")}"
            : "No namespaces loaded";


        builder.Services
            .AddMcpServer(server =>
            {
                server.ServerInfo = new Implementation
                {
                    Name = $"McpNetDll ({namespaceSummary})",
                    Version = "1.0.0"
                };
            })
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        var host = builder.Build();
        await host.RunAsync();
    }
}